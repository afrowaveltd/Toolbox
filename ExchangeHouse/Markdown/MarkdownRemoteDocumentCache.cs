using System.Buffers;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Afrowave.Toolbox.ExchangeHouse.Markdown;

/// <summary>How to obtain a remote Markdown document backed by a reusable local copy.</summary>
public enum MarkdownRemoteCacheMode
{
    /// <summary>Conditionally revalidate a cached document; use it if connectivity fails.</summary>
    OnlineFirst,

    /// <summary>Never access the network; open an existing cached copy only.</summary>
    OfflineOnly,

    /// <summary>Request the complete remote document without conditional headers.</summary>
    ForceRefresh
}

/// <summary>
/// The immutable path and status of a completed cached document. The caller
/// may build temporary Markdown indexes over LocalPath and dispose those
/// indexes without removing the cached source.
/// </summary>
public sealed record MarkdownCachedRemoteDocument(
    Uri Source, string LocalPath, long ByteCount, bool FromCache,
    bool IsStale, string? ETag, DateTimeOffset? LastModified);

/// <summary>
/// Reusable disk-backed cache for trusted HTTPS Markdown documents.
/// Stores versioned source files plus one atomically replaced manifest per URL.
/// Does not store document bodies, page indexes or a document catalog in RAM.
/// </summary>
/// <remarks>
/// Intended for local interactive applications. It is not an SSRF-safe server
/// URL proxy, a shared authenticated-document cache, nor a globally
/// synchronized multiprocess store. Private remote sources require isolated
/// user/identity-specific cache roots and appropriate network policy.
/// </remarks>
public sealed class MarkdownRemoteDocumentCache
{
    private const long DefaultMaxDocumentBytes = 256L * 1024 * 1024;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = false
    };
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly string _directory;

    private sealed record CacheManifest(
        int Version, string FileName, long ByteCount,
        string? ETag, DateTimeOffset? LastModified);

    /// <summary>
    /// A caller-supplied dedicated directory or a per-user application cache
    /// directory (XDG_CACHE_HOME on Unix when configured). Not OS Temp.
    /// </summary>
    public MarkdownRemoteDocumentCache(string? directory = null)
    {
        _directory = Path.GetFullPath(directory ?? DefaultDirectory());
    }

    /// <summary>The directory containing persisted source versions and manifests.</summary>
    public string DirectoryPath => _directory;

    /// <summary>
    /// Open a cached source or download/revalidate it with a bounded buffer.
    /// The maxBytes limit applies to both newly downloaded and stored files.
    /// </summary>
    public async Task<MarkdownCachedRemoteDocument> GetAsync(
        HttpClient client, Uri source,
        MarkdownRemoteCacheMode mode = MarkdownRemoteCacheMode.OnlineFirst,
        long maxBytes = DefaultMaxDocumentBytes,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        ValidateSource(source);
        if (!Enum.IsDefined(mode))
            throw new ArgumentOutOfRangeException(nameof(mode));
        if (maxBytes <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxBytes));

        await _gate.WaitAsync(cancellationToken);
        try
        {
            var key = CacheKey(source);
            var existing = await ReadManifestAsync(key, cancellationToken);
            if (existing is not null && existing.ByteCount > maxBytes)
                throw new InvalidDataException(
                    "The cached Markdown exceeds the configured document limit.");

            if (mode == MarkdownRemoteCacheMode.OfflineOnly)
            {
                if (existing is null)
                    throw new FileNotFoundException(
                        "No valid cached copy exists for the requested HTTPS document.");
                return ToResult(source, existing, fromCache: true, stale: true);
            }

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, source);
                request.Headers.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("text/markdown"));
                request.Headers.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("text/plain"));
                if (mode == MarkdownRemoteCacheMode.OnlineFirst && existing is not null)
                {
                    if (existing.ETag is { } tag &&
                        EntityTagHeaderValue.TryParse(tag, out var validator))
                        request.Headers.IfNoneMatch.Add(validator);
                    if (existing.LastModified is { } modified)
                        request.Headers.IfModifiedSince = modified;
                }

                using var response = await client.SendAsync(
                    request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                if (response.RequestMessage?.RequestUri is { } final &&
                    final.Scheme != Uri.UriSchemeHttps)
                    throw new HttpRequestException(
                        "A redirect downgraded the remote document from HTTPS.");

                if (response.StatusCode == HttpStatusCode.NotModified)
                {
                    if (existing is null)
                        throw new HttpRequestException(
                            "Server returned 304 without a valid cached document.");
                    return ToResult(source, existing, fromCache: true, stale: false);
                }
                if ((int)response.StatusCode >= 500 && existing is not null)
                    return ToResult(source, existing, fromCache: true, stale: true);
                response.EnsureSuccessStatusCode();

                var length = response.Content.Headers.ContentLength;
                if (length.HasValue && length.Value > maxBytes)
                    throw new InvalidDataException(
                        "Remote Markdown exceeds the configured document limit.");

                var versionName = key + "." + Guid.NewGuid().ToString("N") + ".md";
                var tempPath = Path.Combine(_directory, key + "." +
                    Guid.NewGuid().ToString("N") + ".part");
                var versionPath = Path.Combine(_directory, versionName);
                string? tempManifestPath = null;
                try
                {
                    EnsureDirectory();
                    long total = 0;
                    await using (var network = await response.Content.ReadAsStreamAsync(
                        cancellationToken))
                    await using (var local = new FileStream(
                        tempPath, FileMode.CreateNew, FileAccess.Write,
                        FileShare.None, 64 * 1024,
                        FileOptions.Asynchronous | FileOptions.SequentialScan))
                    {
                        MakePrivate(tempPath);
                        var buffer = ArrayPool<byte>.Shared.Rent(64 * 1024);
                        try
                        {
                            int count;
                            while ((count = await network.ReadAsync(
                                buffer.AsMemory(0, 64 * 1024), cancellationToken)) != 0)
                            {
                                if (count > maxBytes - total)
                                    throw new InvalidDataException(
                                        "Remote Markdown exceeded its byte limit during transfer.");
                                await local.WriteAsync(
                                    buffer.AsMemory(0, count), cancellationToken);
                                total += count;
                            }
                            await local.FlushAsync(cancellationToken);
                        }
                        finally
                        {
                            ArrayPool<byte>.Shared.Return(buffer);
                        }
                    }

                    // Immutable content versions keep an older open document
                    // usable while the manifest switches to a newer version.
                    File.Move(tempPath, versionPath);
                    var manifest = new CacheManifest(1, versionName, total,
                        response.Headers.ETag?.ToString(),
                        response.Content.Headers.LastModified);
                    tempManifestPath = Path.Combine(_directory, key + "." +
                        Guid.NewGuid().ToString("N") + ".json.part");
                    await File.WriteAllTextAsync(tempManifestPath,
                        JsonSerializer.Serialize(manifest, JsonOptions),
                        cancellationToken);
                    MakePrivate(tempManifestPath);
                    File.Move(tempManifestPath,
                        Path.Combine(_directory, key + ".json"), overwrite: true);
                    return ToResult(source, manifest, fromCache: false, stale: false);
                }
                finally
                {
                    // Failed updates never replace a valid old manifest.
                    TryDelete(tempPath);
                    if (tempManifestPath is not null)
                        TryDelete(tempManifestPath);
                }
            }
            catch (HttpRequestException ex) when (existing is not null &&
                !cancellationToken.IsCancellationRequested &&
                (ex.StatusCode is null || (int)ex.StatusCode.Value >= 500))
            {
                return ToResult(source, existing, fromCache: true, stale: true);
            }
            catch (OperationCanceledException) when (existing is not null &&
                !cancellationToken.IsCancellationRequested)
            {
                // Timeout or transport cancellation, not a caller cancellation.
                return ToResult(source, existing, fromCache: true, stale: true);
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>
    /// Explicitly remove all cached versions and the manifest of one URL.
    /// Never removes temporary structural indexes or unrelated documents.
    /// </summary>
    public async Task<int> RemoveAsync(
        Uri source, CancellationToken cancellationToken = default)
    {
        ValidateSource(source);
        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (!Directory.Exists(_directory))
                return 0;
            var key = CacheKey(source);
            var removed = 0;
            foreach (var path in Directory.EnumerateFiles(_directory,
                key + ".*", SearchOption.TopDirectoryOnly))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var name = Path.GetFileName(path);
                if (name == key + ".json" ||
                    (name.StartsWith(key + ".", StringComparison.Ordinal) &&
                     name.EndsWith(".md", StringComparison.Ordinal)))
                {
                    File.Delete(path);
                    removed++;
                }
            }
            return removed;
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>
    /// Save a verified cache result outside the cache directory. A temporary
    /// sibling is copied and then moved into place, so errors leave no partial
    /// target. Existing target files are not replaced by default.
    /// </summary>
    public static async Task SaveCopyAsync(
        MarkdownCachedRemoteDocument document, string destination,
        bool overwrite = false, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentException.ThrowIfNullOrWhiteSpace(destination);
        var target = Path.GetFullPath(destination);
        var parent = Path.GetDirectoryName(target)!;
        if (!Directory.Exists(parent))
            throw new DirectoryNotFoundException(parent);
        var temp = Path.Combine(parent, "." + Path.GetFileName(target) + "." +
            Guid.NewGuid().ToString("N") + ".part");
        try
        {
            await using (var input = new FileStream(document.LocalPath,
                FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete,
                64 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan))
            await using (var output = new FileStream(temp,
                FileMode.CreateNew, FileAccess.Write, FileShare.None,
                64 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                MakePrivate(temp);
                await input.CopyToAsync(output, cancellationToken);
                await output.FlushAsync(cancellationToken);
            }
            File.Move(temp, target, overwrite);
        }
        finally
        {
            TryDelete(temp);
        }
    }

    private MarkdownCachedRemoteDocument ToResult(
        Uri source, CacheManifest entry, bool fromCache, bool stale) =>
        new(source, Path.Combine(_directory, entry.FileName),
            entry.ByteCount, fromCache, stale, entry.ETag, entry.LastModified);

    private async Task<CacheManifest?> ReadManifestAsync(
        string key, CancellationToken cancellationToken)
    {
        var manifestPath = Path.Combine(_directory, key + ".json");
        if (!File.Exists(manifestPath))
            return null;
        try
        {
            var json = await File.ReadAllTextAsync(manifestPath, cancellationToken);
            var manifest = JsonSerializer.Deserialize<CacheManifest>(json, JsonOptions);
            if (manifest is null || manifest.Version != 1 ||
                manifest.ByteCount < 0 || manifest.FileName is null ||
                !manifest.FileName.StartsWith(key + ".", StringComparison.Ordinal) ||
                !manifest.FileName.EndsWith(".md", StringComparison.Ordinal) ||
                !Guid.TryParseExact(
                    manifest.FileName.Substring(key.Length + 1, 32), "N",
                    out _) ||
                manifest.FileName.Length != key.Length + 1 + 32 + 3)
                return null;
            var path = Path.Combine(_directory, manifest.FileName);
            var file = new FileInfo(path);
            return file.Exists && file.Length == manifest.ByteCount
                ? manifest : null;
        }
        catch (JsonException) { return null; }
        catch (IOException) { return null; }
        catch (ArgumentException) { return null; }
    }

    private void EnsureDirectory()
    {
        Directory.CreateDirectory(_directory);
        if (!OperatingSystem.IsWindows())
            File.SetUnixFileMode(_directory,
                UnixFileMode.UserRead | UnixFileMode.UserWrite |
                UnixFileMode.UserExecute);
    }

    private static void MakePrivate(string path)
    {
        if (!OperatingSystem.IsWindows())
            File.SetUnixFileMode(path,
                UnixFileMode.UserRead | UnixFileMode.UserWrite);
    }

    private static string CacheKey(Uri source)
    {
        // Fragments never affect HTTP content, and the URL is not written in
        // clear text as a filename. Different paths or query strings differ.
        var canonical = source.GetComponents(
            UriComponents.SchemeAndServer | UriComponents.PathAndQuery,
            UriFormat.UriEscaped);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)))
            .ToLowerInvariant();
    }

    private static void ValidateSource(Uri source)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (!source.IsAbsoluteUri || source.Scheme != Uri.UriSchemeHttps ||
            string.IsNullOrWhiteSpace(source.Host) ||
            !string.IsNullOrEmpty(source.UserInfo))
            throw new ArgumentException(
                "A trusted absolute HTTPS URL without userinfo is required.",
                nameof(source));
    }

    private static string DefaultDirectory()
    {
        if (!OperatingSystem.IsWindows())
        {
            var xdg = Environment.GetEnvironmentVariable("XDG_CACHE_HOME");
            if (!string.IsNullOrWhiteSpace(xdg) &&
                Path.IsPathFullyQualified(xdg))
                return Path.Combine(xdg, "afrowave", "markdown-documents");
            var home = Environment.GetFolderPath(
                Environment.SpecialFolder.UserProfile);
            if (!string.IsNullOrWhiteSpace(home))
                return Path.Combine(home, ".cache", "afrowave", "markdown-documents");
        }
        var local = Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(local))
            throw new InvalidOperationException(
                "No per-user cache directory is available; supply a directory.");
        return Path.Combine(local, "Afrowave", "MarkdownDocuments");
    }

    private static void TryDelete(string path)
    {
        try { File.Delete(path); }
        catch (IOException) { /* Preserve the original exception. */ }
        catch (UnauthorizedAccessException) { /* Preserve the original exception. */ }
    }
}
