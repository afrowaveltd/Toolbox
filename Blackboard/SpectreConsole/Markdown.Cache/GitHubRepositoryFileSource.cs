using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.Cache;

/// <summary>A precisely identified Markdown file in a public GitHub repository.</summary>
/// <param name="Owner">Account or organization login.</param>
/// <param name="Repository">Repository name.</param>
/// <param name="Reference">Resolved branch, tag or commit, never an assumed default.</param>
/// <param name="Path">Canonical repository-relative Markdown file path.</param>
public sealed record GitHubRepositoryFile(
    string Owner, string Repository, string Reference, string Path);

/// <summary>Discovered default-branch README identity and metadata freshness.</summary>
/// <param name="File">Resolved README file in the reported default branch.</param>
/// <param name="State">Freshness of the required repository and README metadata.</param>
/// <param name="Warning">Explicit stale/offline warning when applicable.</param>
public sealed record GitHubReadmeLocation(
    GitHubRepositoryFile File, HttpDocumentState State, string? Warning = null);

/// <summary>
/// Public GitHub repository Markdown file provider, using official REST
/// metadata and Contents endpoints with the existing verified disk cache.
/// </summary>
/// <remarks>
/// This provider never guesses a default branch or README filename.
/// Metadata is schema-validated before caching. The HTTP client must have
/// automatic redirects disabled. A stale metadata result is visibly marked
/// and the file is still retrieved under its explicit resolved ref.
/// </remarks>
public sealed class GitHubRepositoryFileSource
{
    // GitHub README JSON may embed the entire README as Base64. Keep the
    // remote response bounded, but persist only the validated identity fields.
    private const int MaximumMetadataResponseBytes = 2 * 1024 * 1024;
    private const int MaximumCachedMetadataBytes = 8 * 1024;
    private static readonly Regex OwnerPattern = new(
        @"\A[A-Za-z0-9](?:[A-Za-z0-9-]{0,38})\z",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex RepositoryPattern = new(
        @"\A[A-Za-z0-9_.-]{1,100}\z",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly HttpClient _client;
    private readonly VerifiedDocumentCache _cache;

    /// <summary>Initializes an adapter with a caller-owned HTTP client and verified cache.</summary>
    /// <param name="client">HTTP client configured without automatic redirects.</param>
    /// <param name="cache">Disk cache for validated metadata and documents.</param>
    public GitHubRepositoryFileSource(HttpClient client, VerifiedDocumentCache cache)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    /// <summary>
    /// Discovers the real default branch and preferred README path through
    /// two official metadata requests, retaining verified offline fallback.
    /// </summary>
    /// <param name="owner">GitHub owner.</param>
    /// <param name="repository">Repository name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Canonical README identity and metadata freshness.</returns>
    public async Task<GitHubReadmeLocation> ResolveReadmeAsync(
        string owner, string repository, CancellationToken cancellationToken = default)
    {
        ValidateRepository(owner, repository);
        var prefix = RepositoryApiPrefix(owner, repository);
        var key = owner.ToLowerInvariant() + "/" + repository.ToLowerInvariant();

        var branch = await GetMetadataAsync(
            "github-repo-branch:v1:" + key, prefix,
            "default_branch", root =>
            {
                var value = ReadRequiredString(root, "default_branch");
                ValidateReference(value);
                return value;
            },
            cancellationToken).ConfigureAwait(false);
        var resolvedRef = branch.Value;
        if (resolvedRef is null)
            throw new IOException(branch.Warning ?? "GitHub default branch is unavailable.");
        ValidateReference(resolvedRef);

        var readmeUri = new Uri(prefix + "/readme?ref=" +
            Uri.EscapeDataString(resolvedRef));
        var readme = await GetMetadataAsync(
            "github-repo-readme-path:v1:" + key + ":" +
            Uri.EscapeDataString(resolvedRef), readmeUri,
            "path", root =>
            {
                var value = ReadRequiredString(root, "path");
                ValidateFile(new GitHubRepositoryFile(
                    owner, repository, resolvedRef, value));
                return value;
            },
            cancellationToken).ConfigureAwait(false);
        var resolvedPath = readme.Value;
        if (resolvedPath is null)
            throw new IOException(readme.Warning ?? "GitHub README metadata is unavailable.");

        var identity = new GitHubRepositoryFile(
            owner, repository, resolvedRef, resolvedPath);
        ValidateFile(identity);

        var state = branch.State == HttpDocumentState.Stale ||
                    readme.State == HttpDocumentState.Stale
            ? HttpDocumentState.Stale
            : branch.State == HttpDocumentState.NotModified &&
              readme.State == HttpDocumentState.NotModified
                ? HttpDocumentState.NotModified
                : HttpDocumentState.Downloaded;
        var warnings = new[] { branch.Warning, readme.Warning }
            .Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
        return new GitHubReadmeLocation(identity, state,
            warnings.Length == 0 ? null : string.Join(" ", warnings));
    }

    /// <summary>
    /// Retrieves a specific Markdown file at its explicitly resolved ref.
    /// Offline fallback requires a previously fully verified copy of that
    /// exact owner/repository/ref/path identity.
    /// </summary>
    /// <param name="file">Exact repository file identity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Verified content and explicit freshness state.</returns>
    public async Task<HttpDocumentResult> OpenAsync(
        GitHubRepositoryFile file, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);
        ValidateFile(file);
        var path = string.Join("/", file.Path.Split('/').Select(Uri.EscapeDataString));
        var uri = new Uri(RepositoryApiPrefix(file.Owner, file.Repository) +
            "/contents/" + path + "?ref=" + Uri.EscapeDataString(file.Reference));
        // Escape the entire ref and path in separate key components. A ref
        // containing '/' must not collide with a different path/ref pair.
        var key = "github-repo-file:v1:" + file.Owner.ToLowerInvariant() + "/" +
            file.Repository.ToLowerInvariant() + ":ref:" +
            Uri.EscapeDataString(file.Reference) + ":path:" +
            Uri.EscapeDataString(file.Path);
        var previous = await _cache.GetAsync(key, cancellationToken).ConfigureAwait(false);

        try
        {
            using var request = NewRequest(uri, "application/vnd.github.raw+json", previous);
            using var response = await _client.SendAsync(
                request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);
            if (response.RequestMessage?.RequestUri is { } redirected && redirected != uri)
                return Failure(previous, response.StatusCode,
                    "GitHub redirected the file request; explicit approval is required.");
            if (response.StatusCode == HttpStatusCode.NotModified)
            {
                if (previous is null)
                    return new HttpDocumentResult(null, HttpDocumentState.Error,
                        response.StatusCode, "GitHub returned 304 without a verified file.");
                var unchanged = await _cache.MarkNotModifiedAsync(
                    key, previous.Sha256, cancellationToken).ConfigureAwait(false);
                return unchanged is null
                    ? new HttpDocumentResult(null, HttpDocumentState.Error,
                        response.StatusCode, "Cached file changed during revalidation.")
                    : new HttpDocumentResult(unchanged, HttpDocumentState.NotModified,
                        response.StatusCode);
            }
            if (response.StatusCode != HttpStatusCode.OK)
                return Failure(previous, response.StatusCode,
                    "GitHub file request returned HTTP " + (int)response.StatusCode + ".");

            await using var stream = await response.Content.ReadAsStreamAsync(
                cancellationToken).ConfigureAwait(false);
            var saved = await _cache.PutAsync(key, stream, cancellationToken,
                response.Headers.ETag?.ToString(), response.Content.Headers.LastModified)
                .ConfigureAwait(false);
            return new HttpDocumentResult(saved, HttpDocumentState.Downloaded,
                response.StatusCode);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException or IOException or
                                   TimeoutException or InvalidDataException or
                                   DecoderFallbackException or OperationCanceledException)
        {
            return Failure(previous, null,
                "GitHub file retrieval failed; verified cached content may be stale.");
        }
    }

    private async Task<MetadataValue> GetMetadataAsync(
        string key, Uri uri, string selectedProperty, Func<JsonElement, string> select,
        CancellationToken token)
    {
        var previous = await _cache.GetAsync(key, token).ConfigureAwait(false);
        try
        {
            using var request = NewRequest(uri, "application/vnd.github+json", previous);
            using var response = await _client.SendAsync(
                request, HttpCompletionOption.ResponseHeadersRead, token).ConfigureAwait(false);
            if (response.RequestMessage?.RequestUri is { } redirected && redirected != uri)
                return await MetadataFallbackAsync(previous,
                    "GitHub metadata redirected; refusing changed source.", select, token)
                    .ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.NotModified)
            {
                if (previous is null)
                    return new MetadataValue(null, HttpDocumentState.Error,
                        "GitHub returned metadata HTTP 304 without a verified cache.");
                var updated = await _cache.MarkNotModifiedAsync(
                    key, previous.Sha256, token).ConfigureAwait(false);
                if (updated is null)
                    return new MetadataValue(null, HttpDocumentState.Error,
                        "GitHub metadata changed during validation.");
                var value = await ReadMetadataValueAsync(updated, select, token)
                    .ConfigureAwait(false);
                return new MetadataValue(value, HttpDocumentState.NotModified, null);
            }

            if (response.StatusCode != HttpStatusCode.OK)
                return await MetadataFallbackAsync(previous,
                    "GitHub metadata returned HTTP " + (int)response.StatusCode + ".", select, token)
                    .ConfigureAwait(false);

            await using var stream = await response.Content.ReadAsStreamAsync(token)
                .ConfigureAwait(false);
            using var buffer = new MemoryStream();
            var chunk = new byte[8192];
            int read;
            while ((read = await stream.ReadAsync(chunk.AsMemory(), token)
                .ConfigureAwait(false)) != 0)
            {
                if (buffer.Length + read > MaximumMetadataResponseBytes)
                    throw new InvalidDataException("GitHub metadata exceeds the byte budget.");
                buffer.Write(chunk, 0, read);
            }
            var data = buffer.ToArray();
            string validatedValue;
            using (var json = JsonDocument.Parse(data))
                validatedValue = select(json.RootElement);
            // The original JSON may contain Base64 document bytes. They have
            // no value for identity lookups: store a tiny validated projection,
            // retaining the upstream ETag for correct conditional revalidation.
            var compact = JsonSerializer.SerializeToUtf8Bytes(
                new Dictionary<string, string> { [selectedProperty] = validatedValue });
            if (compact.Length > MaximumCachedMetadataBytes)
                throw new InvalidDataException("GitHub identity metadata exceeds the cache budget.");
            using var content = new MemoryStream(compact, writable: false);
            var saved = await _cache.PutAsync(
                key, content, token, response.Headers.ETag?.ToString(),
                response.Content.Headers.LastModified).ConfigureAwait(false);
            return new MetadataValue(
                await ReadMetadataValueAsync(saved, select, token).ConfigureAwait(false),
                HttpDocumentState.Downloaded, null);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex) when (ex is HttpRequestException or IOException or
                                   TimeoutException or InvalidDataException or
                                   JsonException or DecoderFallbackException or
                                   ArgumentException or OperationCanceledException)
        {
            return await MetadataFallbackAsync(
                previous, "GitHub metadata unavailable or invalid; using verified cache if present.",
                select, token).ConfigureAwait(false);
        }
    }

    private static async Task<string> ReadMetadataValueAsync(
        CachedDocument entry, Func<JsonElement, string> select, CancellationToken token)
    {
        if (entry.Length > MaximumCachedMetadataBytes)
            throw new InvalidDataException("Cached GitHub metadata exceeds the budget.");
        using var json = JsonDocument.Parse(
            await File.ReadAllBytesAsync(entry.ContentPath, token).ConfigureAwait(false));
        return select(json.RootElement);
    }

    private static async Task<MetadataValue> MetadataFallbackAsync(
        CachedDocument? previous, string warning,
        Func<JsonElement, string> select, CancellationToken token)
    {
        if (previous is null)
            return new MetadataValue(null, HttpDocumentState.Error, warning);
        try
        {
            var value = await ReadMetadataValueAsync(previous, select, token)
                .ConfigureAwait(false);
            return new MetadataValue(value, HttpDocumentState.Stale, warning);
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException or
                                   JsonException or UnauthorizedAccessException or
                                   ArgumentException)
        {
            return new MetadataValue(null, HttpDocumentState.Error,
                warning + " Cached metadata is invalid.");
        }
    }

    private static string ReadRequiredString(JsonElement root, string name)
    {
        if (root.ValueKind != JsonValueKind.Object ||
            !root.TryGetProperty(name, out var value) ||
            value.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(value.GetString()))
            throw new InvalidDataException("GitHub metadata lacks a valid " + name + ".");
        return value.GetString()!;
    }

    private static HttpRequestMessage NewRequest(
        Uri uri, string accept, CachedDocument? cached)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(accept));
        request.Headers.UserAgent.ParseAdd("Afrowave-Toolbox-MarkdownReader/0.1");
        request.Headers.Add("X-GitHub-Api-Version", "2022-11-28");
        if (cached?.ETag is { } etag && EntityTagHeaderValue.TryParse(etag, out var parsed))
            request.Headers.IfNoneMatch.Add(parsed);
        if (cached?.LastModified is { } modified)
            request.Headers.IfModifiedSince = modified;
        return request;
    }

    private static Uri RepositoryApiPrefix(string owner, string repository) =>
        new("https://api.github.com/repos/" + Uri.EscapeDataString(owner) +
            "/" + Uri.EscapeDataString(repository));

    private static void ValidateRepository(string owner, string repository)
    {
        if (string.IsNullOrWhiteSpace(owner) || !OwnerPattern.IsMatch(owner) ||
            string.IsNullOrWhiteSpace(repository) || !RepositoryPattern.IsMatch(repository) ||
            repository is "." or "..")
            throw new ArgumentException("Invalid GitHub owner or repository name.");
    }

    private static void ValidateReference(string reference)
    {
        if (string.IsNullOrWhiteSpace(reference) || reference.Length > 1024 ||
            reference.StartsWith('/') || reference.EndsWith('/') ||
            reference.Any(c => char.IsControl(c) || char.IsWhiteSpace(c) ||
                               c is '\\' or '?' or '#' or '%'))
            throw new ArgumentException("Invalid GitHub repository ref.");
    }

    private static void ValidateFile(GitHubRepositoryFile file)
    {
        ValidateRepository(file.Owner, file.Repository);
        ValidateReference(file.Reference);
        var path = file.Path;
        if (string.IsNullOrWhiteSpace(path) || path.Length > 4096 ||
            path.StartsWith('/') ||
            path.Any(c => char.IsControl(c) || c is '\\' or '%' or '?' or '#' or ':') ||
            path.Split('/').Any(p => p.Length > 255 || p is "" or "." or ".."))
            throw new ArgumentException("Invalid repository file path.");
        var extension = Path.GetExtension(path);
        if (!extension.Equals(".md", StringComparison.OrdinalIgnoreCase) &&
            !extension.Equals(".markdown", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Only Markdown repository files can be browsed.");
    }

    private static HttpDocumentResult Failure(
        CachedDocument? cached, HttpStatusCode? code, string warning) =>
        new(cached, cached is null ? HttpDocumentState.Error : HttpDocumentState.Stale,
            code, warning);

    private sealed record MetadataValue(
        string? Value, HttpDocumentState State, string? Warning);
}
