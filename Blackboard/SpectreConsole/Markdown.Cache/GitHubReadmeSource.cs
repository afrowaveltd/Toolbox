using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;

namespace Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.Cache;

/// <summary>
/// Reads the preferred public repository README through GitHub's official REST endpoint.
/// This is an independent source adapter; no Markdown parsing, terminal rendering or
/// GitHub branch/filename guessing occurs here.
/// </summary>
public sealed class GitHubReadmeSource
{
    private static readonly Regex OwnerPattern = new(@"\A[A-Za-z0-9](?:[A-Za-z0-9-]{0,38})\z",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex RepositoryPattern = new(@"\A[A-Za-z0-9_.-]{1,100}\z",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private readonly HttpClient _client;
    private readonly VerifiedDocumentCache _cache;

    public GitHubReadmeSource(HttpClient client, VerifiedDocumentCache cache)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public async Task<HttpDocumentResult> OpenAsync(string owner, string repository,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(owner) || !OwnerPattern.IsMatch(owner) ||
            string.IsNullOrWhiteSpace(repository) || !RepositoryPattern.IsMatch(repository) ||
            repository is "." or "..")
            throw new ArgumentException("Specify a valid GitHub repository owner and name.");

        // Owner/repository identifiers are case-insensitive according to GitHub's REST API.
        // Branch names, filenames and ref overrides are intentionally NOT supported here.
        var cacheKey = "github-readme:" + owner.ToLowerInvariant() + "/" + repository.ToLowerInvariant();
        var uri = new Uri("https://api.github.com/repos/" + Uri.EscapeDataString(owner) +
                          "/" + Uri.EscapeDataString(repository) + "/readme");
        var existing = await _cache.GetAsync(cacheKey, cancellationToken).ConfigureAwait(false);
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.raw+json"));
            request.Headers.UserAgent.ParseAdd("Afrowave-Toolbox-MarkdownReader/0.1");
            request.Headers.Add("X-GitHub-Api-Version", "2022-11-28");
            if (existing?.ETag is { } etag && EntityTagHeaderValue.TryParse(etag, out var parsed))
                request.Headers.IfNoneMatch.Add(parsed);
            if (existing?.LastModified is { } modified)
                request.Headers.IfModifiedSince = modified;

            using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead,
                cancellationToken).ConfigureAwait(false);
            if (response.RequestMessage?.RequestUri is { } finalUri && finalUri != uri)
                return Failure(existing, response.StatusCode, "A redirect requires an explicit source decision.");

            if (response.StatusCode == HttpStatusCode.NotModified)
            {
                if (existing is null)
                    return new HttpDocumentResult(null, HttpDocumentState.Error, response.StatusCode,
                        "GitHub returned 304 without a verified cached README.");
                var unchanged = await _cache.MarkNotModifiedAsync(cacheKey, existing.Sha256, cancellationToken)
                    .ConfigureAwait(false);
                return unchanged is null
                    ? new HttpDocumentResult(null, HttpDocumentState.Error, response.StatusCode,
                        "Cached README changed while being checked; retry.")
                    : new HttpDocumentResult(unchanged, HttpDocumentState.NotModified, response.StatusCode);
            }

            if (response.StatusCode != HttpStatusCode.OK)
                return Failure(existing, response.StatusCode,
                    "GitHub README request returned HTTP " + (int)response.StatusCode + ".");

            // The raw media type returns the README bytes, not the base64 JSON metadata.
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            var document = await _cache.PutAsync(cacheKey, stream, cancellationToken,
                response.Headers.ETag?.ToString(), response.Content.Headers.LastModified).ConfigureAwait(false);
            return new HttpDocumentResult(document, HttpDocumentState.Downloaded, response.StatusCode);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception ex) when (ex is HttpRequestException or IOException or TimeoutException or
                                  InvalidDataException or DecoderFallbackException or OperationCanceledException)
        {
            return Failure(existing, null,
                "GitHub connection or README validation failed; cached content may be outdated.");
        }
    }

    private static HttpDocumentResult Failure(CachedDocument? existing, HttpStatusCode? status, string message) =>
        new(existing, existing is null ? HttpDocumentState.Error : HttpDocumentState.Stale, status, message);
}
