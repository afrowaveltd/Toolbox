using System.Net;
using System.Net.Http.Headers;

namespace Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.Cache;

/// <summary>Small HTTP(S) document adapter; never renders Markdown or executes received text.</summary>
public sealed class HttpMarkdownDocumentSource
{
    private readonly HttpClient _client;
    private readonly VerifiedDocumentCache _cache;
    private readonly bool _allowHttp;

    public HttpMarkdownDocumentSource(HttpClient client, VerifiedDocumentCache cache, bool allowInsecureHttp = false)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _allowHttp = allowInsecureHttp;
    }

    public async Task<HttpDocumentResult> OpenAsync(Uri uri, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(uri);
        if (!uri.IsAbsoluteUri ||
            (uri.Scheme != Uri.UriSchemeHttps && !(_allowHttp && uri.Scheme == Uri.UriSchemeHttp)) ||
            !string.IsNullOrEmpty(uri.UserInfo) || !string.IsNullOrEmpty(uri.Query) ||
            !string.IsNullOrEmpty(uri.Fragment))
            throw new ArgumentException("Use an absolute HTTP(S) URL without credentials, query or fragment.", nameof(uri));

        var key = "http:" + uri.AbsoluteUri;
        var cached = await _cache.GetAsync(key, cancellationToken).ConfigureAwait(false);
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            if (cached?.ETag is { } etag && EntityTagHeaderValue.TryParse(etag, out var header))
                request.Headers.IfNoneMatch.Add(header);
            if (cached?.LastModified is { } lastModified)
                request.Headers.IfModifiedSince = lastModified;

            using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead,
                cancellationToken).ConfigureAwait(false);
            // Do not transparently accept a changed source identity after a redirect.
            if (response.RequestMessage?.RequestUri is { } finalUri && finalUri != uri)
                return Failure(cached, response.StatusCode, "Redirected responses require an explicit source decision.");

            if (response.StatusCode == HttpStatusCode.NotModified)
            {
                if (cached is null)
                    return new HttpDocumentResult(null, HttpDocumentState.Error, response.StatusCode,
                        "Server returned 304 but no validated local document exists.");
                var verified = await _cache.MarkNotModifiedAsync(key, cached.Sha256, cancellationToken)
                    .ConfigureAwait(false);
                return verified is null
                    ? new HttpDocumentResult(null, HttpDocumentState.Error, response.StatusCode,
                        "Cached entry changed during revalidation; retry.")
                    : new HttpDocumentResult(verified, HttpDocumentState.NotModified, response.StatusCode);
            }

            if (response.StatusCode != HttpStatusCode.OK)
                return Failure(cached, response.StatusCode, "Remote source returned HTTP " + (int)response.StatusCode + ".");

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            var stored = await _cache.PutAsync(key, stream, cancellationToken,
                response.Headers.ETag?.ToString(), response.Content.Headers.LastModified).ConfigureAwait(false);
            return new HttpDocumentResult(stored, HttpDocumentState.Downloaded, response.StatusCode);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException or IOException or TimeoutException or
                                  InvalidDataException or System.Text.DecoderFallbackException or
                                  OperationCanceledException)
        {
            return Failure(cached, null, "Request or document validation failed; cached content may be out of date.");
        }
    }

    private static HttpDocumentResult Failure(CachedDocument? existing, HttpStatusCode? status, string message) =>
        new(existing, existing is null ? HttpDocumentState.Error : HttpDocumentState.Stale,
            status, message);
}

public enum HttpDocumentState { Downloaded, NotModified, Stale, Error }

/// <summary>Stale indicates a validated old copy, never a successful HTTP freshness check.</summary>
public sealed record HttpDocumentResult(CachedDocument? Document, HttpDocumentState State,
    HttpStatusCode? StatusCode = null, string? Warning = null);
