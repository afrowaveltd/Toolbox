using System.Buffers;
using System.Net;
using System.Net.Http.Headers;

namespace Afrowave.Toolbox.Blackboard.Showcase;

/// <summary>
/// Temporary, read-only local copy of a remote Markdown document.
/// A successful download owns exactly one uniquely named file under OS Temp.
/// Dispose it after the reader/pager is closed to delete that file.
/// </summary>
public sealed class RemoteMarkdownTempDocument : IDisposable, IAsyncDisposable
{
    private int _disposed;

    private RemoteMarkdownTempDocument(string path, Uri source, long byteCount)
    {
        TempPath = path;
        Source = source;
        ByteCount = byteCount;
    }

    /// <summary>Complete and unmodified raw response bytes on local temporary disk.</summary>
    public string TempPath { get; }

    /// <summary>Original requested URI.</summary>
    public Uri Source { get; }

    /// <summary>Number of downloaded bytes.</summary>
    public long ByteCount { get; }

    /// <summary>
    /// Download a trusted HTTPS Markdown URL with a bounded memory buffer.
    /// The supplied HttpClient controls timeouts, TLS, proxy and redirect policy.
    /// The response is fully spooled to OS Temp before the returned document is
    /// offered for random-access paging.
    /// </summary>
    /// <remarks>
    /// Do not expose this method as an SSRF-safe server endpoint accepting
    /// arbitrary untrusted URLs. Network/DNS policies belong to the caller.
    /// A hard byte ceiling applies even when Content-Length is unknown.
    /// </remarks>
    public static async Task<RemoteMarkdownTempDocument> DownloadAsync(
        HttpClient httpClient,
        Uri source,
        long maxBytes = 16L * 1024 * 1024 * 1024,
        CancellationToken cancellationToken = default,
        string? tempDirectory = null)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(source);
        if (!source.IsAbsoluteUri ||
            source.Scheme != Uri.UriSchemeHttps ||
            string.IsNullOrWhiteSpace(source.Host) ||
            !string.IsNullOrEmpty(source.UserInfo))
            throw new ArgumentException("A trusted absolute HTTPS URL without userinfo is required.", nameof(source));
        if (maxBytes < 1)
            throw new ArgumentOutOfRangeException(nameof(maxBytes));

        // CreateNew avoids replacing a file even in the unlikely event of a name collision.
        var directory = tempDirectory ?? Path.GetTempPath();
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "afrowave-markdown-" +
            Guid.NewGuid().ToString("N") + ".tmp");
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, source);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/markdown"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));

            using var response = await httpClient.SendAsync(
                request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            // Reject redirects that unexpectedly downgrade to insecure HTTP.
            if (response.RequestMessage?.RequestUri is { } destination &&
                destination.Scheme != Uri.UriSchemeHttps)
                throw new HttpRequestException("A redirect downgraded the download from HTTPS.");

            var declared = response.Content.Headers.ContentLength;
            if (declared is > 0 && declared > maxBytes)
                throw new InvalidDataException("Remote document exceeds the configured disk quota.");

            await using var remote = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var local = new FileStream(
                path, FileMode.CreateNew, FileAccess.Write,
                FileShare.Read | FileShare.Delete,
                bufferSize: 64 * 1024,
                options: FileOptions.Asynchronous | FileOptions.SequentialScan);

            // OS Temp may be shared; prevent other Unix accounts from
            // reading a downloaded private document. Fail closed if the
            // filesystem cannot enforce owner-only permissions.
            if (!OperatingSystem.IsWindows())
                File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite);

            var buffer = ArrayPool<byte>.Shared.Rent(64 * 1024);
            long total = 0;
            try
            {
                int count;
                while ((count = await remote.ReadAsync(
                           buffer.AsMemory(0, 64 * 1024), cancellationToken)) != 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (count > maxBytes - total)
                        throw new InvalidDataException(
                            "Remote document exceeded the configured disk quota during streaming.");
                    await local.WriteAsync(buffer.AsMemory(0, count), cancellationToken);
                    total += count;
                }
                await local.FlushAsync(cancellationToken);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            return new RemoteMarkdownTempDocument(path, source, total);
        }
        catch
        {
            // Never leave a partial file on a failed/cancelled download.
            try { File.Delete(path); }
            catch (IOException) { /* OS may hold an in-progress handle; original exception wins. */ }
            catch (UnauthorizedAccessException) { /* Original exception wins. */ }
            throw;
        }
    }

    /// <summary>Open an independent, seekable read-only stream for paging or scanning.</summary>
    public FileStream OpenRead()
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);
        return new FileStream(TempPath, FileMode.Open, FileAccess.Read,
            FileShare.Read | FileShare.Delete,
            bufferSize: 64 * 1024,
            options: FileOptions.Asynchronous | FileOptions.RandomAccess);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 0)
            File.Delete(TempPath);
    }

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }
}
