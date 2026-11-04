using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.Cache;

/// <summary>One public repository offered for interactive README selection.</summary>
public sealed record GitHubRepositoryInfo(
    string Name, string? Description = null, bool Archived = false, bool Fork = false);

/// <summary>Whether a catalog came from GitHub, an unexpired cache or an offline fallback.</summary>
public enum GitHubRepositoryCatalogState { Downloaded, FreshCache, Stale, Error }

/// <summary>Complete bounded repository listing result; a stale result is never called fresh.</summary>
public sealed record GitHubRepositoryCatalogResult(
    IReadOnlyList<GitHubRepositoryInfo> Repositories, GitHubRepositoryCatalogState State,
    bool Truncated = false, string? Warning = null);

/// <summary>
/// Enumerates public GitHub user/organization repositories, with a persistent verified
/// JSON snapshot. Network refresh never replaces a complete snapshot with a partial one.
/// </summary>
public sealed class GitHubRepositoryCatalogSource
{
    private const int PageSize = 100;
    private const int MaximumPages = 10; // At most 1000 repositories and 11 GETs on a refresh.
    private const int MaximumResponseBytes = 4 * 1024 * 1024;
    private const int SnapshotVersion = 1;
    private static readonly TimeSpan Freshness = TimeSpan.FromMinutes(30);
    private static readonly Regex OwnerPattern = new(@"\A[A-Za-z0-9](?:[A-Za-z0-9-]{0,38})\z",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex RepoPattern = new(@"\A[A-Za-z0-9_.-]{1,100}\z",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly HttpClient _client;
    private readonly VerifiedDocumentCache _cache;
    private readonly TimeProvider _clock;

    /// <summary>Creates a public-only GitHub catalog source using a caller-owned HTTP client.</summary>
    public GitHubRepositoryCatalogSource(HttpClient client, VerifiedDocumentCache cache, TimeProvider? clock = null)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _clock = clock ?? TimeProvider.System;
    }

    /// <summary>
    /// Loads an owner's repository names, checking GitHub at most once per 30 minutes
    /// per cache root. Offline failures may return a previously validated stale list.
    /// </summary>
    public async Task<GitHubRepositoryCatalogResult> ListAsync(
        string owner, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(owner) || !OwnerPattern.IsMatch(owner))
            throw new ArgumentException("Specify a valid GitHub user or organization name.", nameof(owner));

        var key = "github-repositories:v1:" + owner.ToLowerInvariant();
        var cached = await _cache.GetAsync(key, cancellationToken).ConfigureAwait(false);
        var snapshot = await ReadSnapshotAsync(cached, cancellationToken).ConfigureAwait(false);

        if (cached is not null && snapshot is not null)
        {
            var age = _clock.GetUtcNow() - cached.StoredAtUtc;
            if (age >= TimeSpan.Zero && age < Freshness)
                return Result(snapshot, GitHubRepositoryCatalogState.FreshCache);
        }

        try
        {
            var accountUri = new Uri("https://api.github.com/users/" + Uri.EscapeDataString(owner));
            using var accountResponse = await GetAsync(accountUri, cancellationToken).ConfigureAwait(false);
            using var accountJson = await ReadJsonAsync(accountResponse, cancellationToken).ConfigureAwait(false);
            if (accountJson.RootElement.ValueKind != JsonValueKind.Object ||
                !accountJson.RootElement.TryGetProperty("type", out var accountType) ||
                accountType.ValueKind != JsonValueKind.String)
                throw new InvalidDataException("GitHub account metadata has an unexpected shape.");

            var organization = accountType.GetString() == "Organization";
            var prefix = organization ? "orgs/" : "users/";
            var entries = new List<GitHubRepositoryInfo>();
            var hasMore = false;

            for (var page = 1; page <= MaximumPages; page++)
            {
                var uri = new Uri("https://api.github.com/" + prefix +
                    Uri.EscapeDataString(owner) + "/repos?per_page=" + PageSize +
                    "&page=" + page + (organization ? "&type=public" : "&type=owner"));
                using var response = await GetAsync(uri, cancellationToken).ConfigureAwait(false);
                using var json = await ReadJsonAsync(response, cancellationToken).ConfigureAwait(false);
                if (json.RootElement.ValueKind != JsonValueKind.Array)
                    throw new InvalidDataException("GitHub repository catalog was not a JSON array.");

                foreach (var repo in json.RootElement.EnumerateArray())
                {
                    if (repo.ValueKind != JsonValueKind.Object ||
                        !repo.TryGetProperty("name", out var nameProperty) ||
                        nameProperty.ValueKind != JsonValueKind.String)
                        continue;

                    var name = nameProperty.GetString() ?? "";
                    if (!RepoPattern.IsMatch(name) || name is "." or ".." ||
                        (repo.TryGetProperty("private", out var privateProperty) &&
                         privateProperty.ValueKind == JsonValueKind.True))
                        continue;

                    var description = repo.TryGetProperty("description", out var d) &&
                        d.ValueKind == JsonValueKind.String ? CleanDescription(d.GetString()) : null;
                    var archived = repo.TryGetProperty("archived", out var a) &&
                        a.ValueKind == JsonValueKind.True;
                    var fork = repo.TryGetProperty("fork", out var f) &&
                        f.ValueKind == JsonValueKind.True;
                    entries.Add(new GitHubRepositoryInfo(name, description, archived, fork));
                }

                hasMore = response.Headers.TryGetValues("Link", out var links) &&
                    links.Any(link => link.Contains("rel=\"next\"", StringComparison.OrdinalIgnoreCase));
                if (!hasMore) break;
            }

            var complete = new CatalogSnapshot(SnapshotVersion, hasMore,
                entries.DistinctBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ToArray());

            // Store one snapshot only after every requested page and all parsing succeeded.
            var bytes = JsonSerializer.SerializeToUtf8Bytes(complete);
            await using var stream = new MemoryStream(bytes, writable: false);
            await _cache.PutAsync(key, stream, cancellationToken).ConfigureAwait(false);
            return Result(complete, GitHubRepositoryCatalogState.Downloaded);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception ex) when (ex is HttpRequestException or IOException or InvalidDataException or
                                  JsonException or OperationCanceledException or TimeoutException)
        {
            var message = ex is HttpRequestException { StatusCode: not null } http
                ? "GitHub catalog request returned HTTP " + (int)http.StatusCode!.Value + "."
                : "GitHub catalog could not be refreshed.";
            return snapshot is null
                ? new GitHubRepositoryCatalogResult([], GitHubRepositoryCatalogState.Error, Warning: message)
                : Result(snapshot, GitHubRepositoryCatalogState.Stale, message + " Using last verified list.");
        }
    }

    private static GitHubRepositoryCatalogResult Result(
        CatalogSnapshot snapshot, GitHubRepositoryCatalogState state, string? warning = null)
    {
        if (snapshot.Truncated)
            warning = (warning is null ? "" : warning + " ") +
                "Only the first 1,000 repositories were loaded; the account has more.";
        return new GitHubRepositoryCatalogResult(snapshot.Repositories, state,
            snapshot.Truncated, warning);
    }

    private async Task<HttpResponseMessage> GetAsync(Uri uri, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        request.Headers.UserAgent.ParseAdd("Afrowave-Toolbox-MarkdownReader/0.1");
        request.Headers.Add("X-GitHub-Api-Version", "2022-11-28");
        var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead,
            cancellationToken).ConfigureAwait(false);
        if (response.RequestMessage?.RequestUri is { } finalUri && finalUri != uri)
        {
            response.Dispose();
            throw new InvalidDataException("GitHub catalog redirected; automatic redirects are forbidden.");
        }
        if (!response.IsSuccessStatusCode)
        {
            var status = response.StatusCode;
            response.Dispose();
            throw new HttpRequestException("GitHub catalog HTTP " + (int)status + ".", null, status);
        }
        return response;
    }

    private static async Task<JsonDocument> ReadJsonAsync(
        HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.Content.Headers.ContentLength > MaximumResponseBytes)
            throw new InvalidDataException("GitHub catalog page exceeds the safe size limit.");

        await using var input = await response.Content.ReadAsStreamAsync(cancellationToken)
            .ConfigureAwait(false);
        using var buffer = new MemoryStream();
        var block = new byte[8192];
        int read;
        while ((read = await input.ReadAsync(block, cancellationToken).ConfigureAwait(false)) > 0)
        {
            if (buffer.Length + read > MaximumResponseBytes)
                throw new InvalidDataException("GitHub catalog page exceeds the safe size limit.");
            await buffer.WriteAsync(block.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
        }
        return JsonDocument.Parse(buffer.ToArray(), new JsonDocumentOptions { MaxDepth = 32 });
    }

    private static async Task<CatalogSnapshot?> ReadSnapshotAsync(
        CachedDocument? cached, CancellationToken token)
    {
        if (cached is null) return null;
        try
        {
            var data = await File.ReadAllTextAsync(cached.ContentPath, token).ConfigureAwait(false);
            var snapshot = JsonSerializer.Deserialize<CatalogSnapshot>(data);
            if (snapshot is null || snapshot.Version != SnapshotVersion ||
                snapshot.Repositories is null || snapshot.Repositories.Length > MaximumPages * PageSize ||
                snapshot.Repositories.Any(r => r is null || !RepoPattern.IsMatch(r.Name) ||
                                               r.Name is "." or ".."))
                return null;
            return snapshot;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            return null;
        }
    }

    private static string? CleanDescription(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var clean = new string(value.Where(c => !char.IsControl(c)).Take(160).ToArray()).Trim();
        return clean.Length == 0 ? null : clean;
    }

    private sealed record CatalogSnapshot(int Version, bool Truncated, GitHubRepositoryInfo[] Repositories);
}
