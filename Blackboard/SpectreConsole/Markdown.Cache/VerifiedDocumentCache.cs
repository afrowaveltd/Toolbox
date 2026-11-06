using System.Buffers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.Cache;

/// <summary>
/// Disk-backed, read-only retrieval of fully verified UTF-8 documents.
/// Writes use immutable content blobs and an atomically published metadata index.
/// The caller owns source identities, authorization policy and freshness decisions.
/// </summary>
public sealed class VerifiedDocumentCache
{
    private readonly string _root;
    private readonly long _maximumDocumentBytes;
    private readonly SemaphoreSlim _writes = new(1, 1);
    private static readonly TimeSpan LockRetryInterval = TimeSpan.FromMilliseconds(50);
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>Initializes a content-addressed cache with an explicit document size limit.</summary>
    /// <param name="rootDirectory">Absolute or relative root used for private content blobs and metadata.</param>
    /// <param name="maximumDocumentBytes">Maximum bytes accepted for a single validated document (default: 256 MiB).</param>
    /// <exception cref="ArgumentException">The cache location is missing or invalid.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The size limit is not positive.</exception>
    public VerifiedDocumentCache(string rootDirectory, long maximumDocumentBytes = 256L * 1024 * 1024)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory))
            throw new ArgumentException("A cache directory is required.", nameof(rootDirectory));
        if (maximumDocumentBytes <= 0)
            throw new ArgumentOutOfRangeException(nameof(maximumDocumentBytes));
        _root = Path.GetFullPath(rootDirectory);
        EnsureNotReparsePoint(_root);
        _maximumDocumentBytes = maximumDocumentBytes;
    }

    /// <summary>Validates UTF-8 and terminal safety, hashes the content and atomically publishes a cache entry.</summary>
    /// <param name="sourceKey">Stable caller-defined identity of the source document.</param>
    /// <param name="content">Readable stream of source bytes; ownership remains with the caller.</param>
    /// <param name="cancellationToken">Cancellation token for reading, writing and validation.</param>
    /// <param name="eTag">Optional HTTP ETag for future conditional requests.</param>
    /// <param name="lastModified">Optional HTTP Last-Modified timestamp for revalidation.</param>
    /// <returns>Metadata and local path for the fully validated cached document.</returns>
    /// <exception cref="InvalidDataException">Content exceeds its size limit or is unsafe/invalid UTF-8.</exception>
    public async Task<CachedDocument> PutAsync(string sourceKey, Stream content, CancellationToken cancellationToken = default,
        string? eTag = null, DateTimeOffset? lastModified = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceKey);
        ArgumentNullException.ThrowIfNull(content);
        if (!content.CanRead) throw new ArgumentException("Source stream must be readable.", nameof(content));

        await _writes.WaitAsync(cancellationToken).ConfigureAwait(false);
        string? temporary = null;
        try
        {
            var blobs = Path.Combine(_root, "blobs");
            var entries = Path.Combine(_root, "entries");
            Directory.CreateDirectory(_root);
            EnsureNotReparsePoint(_root);
            await using var processLock = await AcquireProcessLockAsync(cancellationToken).ConfigureAwait(false);
            Directory.CreateDirectory(blobs);
            Directory.CreateDirectory(entries);
            EnsureNotReparsePoint(blobs);
            EnsureNotReparsePoint(entries);
            temporary = Path.Combine(_root, ".incoming-" + Guid.NewGuid().ToString("N"));
            long length = 0;
            using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            await using (var target = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write,
                FileShare.None, 81920, FileOptions.Asynchronous))
            {
                var buffer = ArrayPool<byte>.Shared.Rent(81920);
                try
                {
                    int count;
                    while ((count = await content.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken).ConfigureAwait(false)) != 0)
                    {
                        length = checked(length + count);
                        if (length > _maximumDocumentBytes)
                            throw new InvalidDataException("Document exceeds the configured cache size limit.");
                        hash.AppendData(buffer, 0, count);
                        await target.WriteAsync(buffer.AsMemory(0, count), cancellationToken).ConfigureAwait(false);
                    }
                    await target.FlushAsync(cancellationToken).ConfigureAwait(false);
                }
                finally { ArrayPool<byte>.Shared.Return(buffer); }
            }

            await ValidateUtf8Async(temporary, cancellationToken).ConfigureAwait(false);
            var digest = Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
            var blob = Path.Combine(blobs, digest + ".md");
            // Never follow a pre-existing blob symlink or trust a corrupt digest-named file.
            if (File.Exists(blob) || Directory.Exists(blob))
            {
                EnsureNotReparsePoint(blob);
                if (!await MatchesDigestAsync(blob, digest, length, cancellationToken).ConfigureAwait(false))
                    File.Move(temporary, blob, overwrite: true);
                else
                    File.Delete(temporary);
            }
            else File.Move(temporary, blob);
            temporary = null;

            var now = DateTimeOffset.UtcNow;
            var entry = new CachedDocument(sourceKey, digest, length, now, blob, eTag, lastModified, now);
            var index = IndexPath(entries, sourceKey);
            EnsureNotReparsePoint(index);
            var tempIndex = index + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                await File.WriteAllTextAsync(tempIndex, JsonSerializer.Serialize(
                    new CacheManifest(sourceKey, digest, length, entry.StoredAtUtc, eTag, lastModified, now), JsonOptions), StrictUtf8, cancellationToken).ConfigureAwait(false);
                File.Move(tempIndex, index, overwrite: true);
            }
            finally { if (File.Exists(tempIndex)) File.Delete(tempIndex); }
            return entry;
        }
        finally
        {
            if (temporary is not null && File.Exists(temporary)) File.Delete(temporary);
            _writes.Release();
        }
    }

    /// <summary>Returns null on absent, corrupt or invalid entries; never returns unverified bytes.</summary>
    public async Task<CachedDocument?> GetAsync(string sourceKey, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceKey);
        var entries = Path.Combine(_root, "entries");
        var blobs = Path.Combine(_root, "blobs");
        var index = IndexPath(entries, sourceKey);
        try
        {
            EnsureNotReparsePoint(_root);
            EnsureNotReparsePoint(entries);
            EnsureNotReparsePoint(blobs);
            EnsureNotReparsePoint(index);
            if (!File.Exists(index)) return null;
            var manifest = JsonSerializer.Deserialize<CacheManifest>(
                await File.ReadAllTextAsync(index, cancellationToken).ConfigureAwait(false), JsonOptions);
            if (manifest is null || manifest.SourceKey != sourceKey ||
                manifest.Length < 0 || manifest.Length > _maximumDocumentBytes ||
                string.IsNullOrEmpty(manifest.Hash) || manifest.Hash.Length != 64 || !manifest.Hash.All(Uri.IsHexDigit) ||
                manifest.Hash != manifest.Hash.ToLowerInvariant())
                return null;
            var blob = Path.Combine(_root, "blobs", manifest.Hash + ".md");
            EnsureNotReparsePoint(blob);
            if (!await MatchesDigestAsync(blob, manifest.Hash, manifest.Length, cancellationToken).ConfigureAwait(false)) return null;
            await ValidateUtf8Async(blob, cancellationToken).ConfigureAwait(false);
            return new CachedDocument(sourceKey, manifest.Hash, manifest.Length, manifest.StoredAtUtc, blob,
                manifest.ETag, manifest.LastModified, manifest.CheckedAtUtc);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException
                                         or InvalidDataException or DecoderFallbackException or ArgumentException)
        {
            return null;
        }
    }

    /// <summary>
    /// Records a successful HTTP 304 only if the same validated content is still indexed.
    /// Does not modify content bytes or overwrite a newer concurrent writer.
    /// </summary>
    public async Task<CachedDocument?> MarkNotModifiedAsync(string sourceKey, string expectedSha256,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedSha256);
        await _writes.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            Directory.CreateDirectory(_root);
            EnsureNotReparsePoint(_root);
            await using var processLock = await AcquireProcessLockAsync(cancellationToken).ConfigureAwait(false);
            var current = await GetAsync(sourceKey, cancellationToken).ConfigureAwait(false);
            if (current is null || current.Sha256 != expectedSha256) return null;
            var entries = Path.Combine(_root, "entries");
            EnsureNotReparsePoint(entries);
            var index = IndexPath(entries, sourceKey);
            EnsureNotReparsePoint(index);
            var updated = current with { CheckedAtUtc = DateTimeOffset.UtcNow };
            var tempIndex = index + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                await File.WriteAllTextAsync(tempIndex, JsonSerializer.Serialize(
                    new CacheManifest(updated.SourceKey, updated.Sha256, updated.Length, updated.StoredAtUtc,
                        updated.ETag, updated.LastModified, updated.CheckedAtUtc), JsonOptions), StrictUtf8,
                    cancellationToken).ConfigureAwait(false);
                File.Move(tempIndex, index, overwrite: true);
            }
            finally { if (File.Exists(tempIndex)) File.Delete(tempIndex); }
            return updated;
        }
        finally { _writes.Release(); }
    }

    private async Task<FileStream> AcquireProcessLockAsync(CancellationToken cancellationToken)
    {
        var lockPath = Path.Combine(_root, ".write.lock");
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            EnsureNotReparsePoint(lockPath);
            try
            {
                // FileShare.None provides exclusive writer admission across cache instances/processes.
                return new FileStream(lockPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                // Another writer may own the lock; retry without blocking cancellation.
                await Task.Delay(LockRetryInterval, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private static void EnsureNotReparsePoint(string path)
    {
        try
        {
            if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
                throw new IOException("Cache root or entry must not be a symbolic link or reparse point.");
        }
        catch (FileNotFoundException) { }
        catch (DirectoryNotFoundException) { }
    }

    private static async Task<bool> MatchesDigestAsync(string blob, string expectedHash, long expectedLength, CancellationToken token)
    {
        if (!File.Exists(blob) || new FileInfo(blob).Length != expectedLength) return false;
        using var sha = SHA256.Create();
        await using var input = new FileStream(blob, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, FileOptions.Asynchronous);
        var hash = Convert.ToHexString(await sha.ComputeHashAsync(input, token).ConfigureAwait(false)).ToLowerInvariant();
        return string.Equals(hash, expectedHash, StringComparison.Ordinal);
    }

    private static string IndexPath(string entries, string key)
    {
        var digest = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        return Path.Combine(entries, Convert.ToHexString(digest).ToLowerInvariant() + ".json");
    }

    private static async Task ValidateUtf8Async(string path, CancellationToken token)
    {
        // Strict UTF-8 decoding rejects malformed byte sequences even at chunk boundaries.
        await using var input = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, FileOptions.Asynchronous);
        using var reader = new StreamReader(input, StrictUtf8, detectEncodingFromByteOrderMarks: false, bufferSize: 8192, leaveOpen: false);
        var chars = new char[8192];
        int count;
        while ((count = await reader.ReadAsync(chars.AsMemory(), token).ConfigureAwait(false)) != 0)
        {
            for (var i = 0; i < count; i++)
            {
                var c = chars[i];
                if ((c < 0x20 && c != '\n' && c != '\r' && c != '\t') || c == 0x7f ||
                    (c >= 0x80 && c <= 0x9f))
                    throw new InvalidDataException("Document contains unsafe terminal control characters.");
            }
        }
    }

    private sealed record CacheManifest(string SourceKey, string Hash, long Length, DateTimeOffset StoredAtUtc,
        string? ETag = null, DateTimeOffset? LastModified = null, DateTimeOffset? CheckedAtUtc = null);
}

/// <summary>Metadata for one fully validated immutable local document blob.</summary>
/// <param name="SourceKey">Caller-defined source identity, distinct from the content hash.</param>
/// <param name="Sha256">Lowercase SHA-256 digest of the validated source bytes.</param>
/// <param name="Length">Document length in bytes.</param>
/// <param name="StoredAtUtc">Time the current content was stored in the cache.</param>
/// <param name="ContentPath">Internal validated blob path, not a public download URL.</param>
/// <param name="ETag">Optional HTTP entity tag of the source document.</param>
/// <param name="LastModified">Optional HTTP modification time supplied by the origin.</param>
/// <param name="CheckedAtUtc">Most recent successful origin freshness check, if any.</param>
public sealed record CachedDocument(string SourceKey, string Sha256, long Length, DateTimeOffset StoredAtUtc,
    string ContentPath, string? ETag = null, DateTimeOffset? LastModified = null, DateTimeOffset? CheckedAtUtc = null);
