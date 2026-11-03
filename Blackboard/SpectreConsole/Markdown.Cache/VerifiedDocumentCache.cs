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

    public async Task<CachedDocument> PutAsync(string sourceKey, Stream content, CancellationToken cancellationToken = default)
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

            var entry = new CachedDocument(sourceKey, digest, length, DateTimeOffset.UtcNow, blob);
            var index = IndexPath(entries, sourceKey);
            EnsureNotReparsePoint(index);
            var tempIndex = index + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                await File.WriteAllTextAsync(tempIndex, JsonSerializer.Serialize(
                    new CacheManifest(sourceKey, digest, length, entry.StoredAtUtc), JsonOptions), StrictUtf8, cancellationToken).ConfigureAwait(false);
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
            return new CachedDocument(sourceKey, manifest.Hash, manifest.Length, manifest.StoredAtUtc, blob);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException
                                         or InvalidDataException or DecoderFallbackException or ArgumentException)
        {
            return null;
        }
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
        if ((File.Exists(path) || Directory.Exists(path)) &&
            (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
            throw new IOException("Cache root or entry must not be a symbolic link or reparse point.");
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

    private sealed record CacheManifest(string SourceKey, string Hash, long Length, DateTimeOffset StoredAtUtc);
}

/// <param name="ContentPath">Internal validated blob path, not a public download URL.</param>
public sealed record CachedDocument(string SourceKey, string Sha256, long Length, DateTimeOffset StoredAtUtc, string ContentPath);
