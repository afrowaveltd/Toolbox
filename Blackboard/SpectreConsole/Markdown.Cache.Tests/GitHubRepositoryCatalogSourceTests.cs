using System.Net;
using System.Net.Http;
using System.Text;
using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.Cache;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.Cache.Tests;

public sealed class GitHubRepositoryCatalogSourceTests
{
    [Fact]
    public async Task OrganizationUsesPublicReposEndpointAndStoresVerifiedSnapshot()
    {
        using var fixture = new Fixture();
        var requests = new List<string>();
        using var client = Client(request =>
        {
            requests.Add(request.RequestUri!.AbsoluteUri);
            Assert.NotEmpty(request.Headers.UserAgent);
            Assert.Contains(request.Headers.Accept, a => a.MediaType == "application/vnd.github+json");
            Assert.Equal("2022-11-28", Assert.Single(request.Headers.GetValues("X-GitHub-Api-Version")));
            return request.RequestUri.AbsolutePath == "/users/afrowaveltd"
                ? Ok("""{"type":"Organization"}""")
                : Ok("""[{"name":"Toolbox","description":"  Useful libraries  ","archived":false,"fork":false},{"name":"Ajis.Dotnet","description":"AJIS","archived":true,"fork":false}]""");
        });

        var source = new GitHubRepositoryCatalogSource(client, new VerifiedDocumentCache(fixture.Root));
        var result = await source.ListAsync("afrowaveltd");
        Assert.Equal(GitHubRepositoryCatalogState.Downloaded, result.State);
        Assert.Equal(new[] { "Ajis.Dotnet", "Toolbox" }, result.Repositories.Select(r => r.Name).ToArray());
        Assert.Equal("Useful libraries", result.Repositories[1].Description);
        Assert.True(result.Repositories[0].Archived);
        Assert.Equal(new[]
        {
            "https://api.github.com/users/afrowaveltd",
            "https://api.github.com/orgs/afrowaveltd/repos?per_page=100&page=1&type=public"
        }, requests);

        using var offline = Client(_ => throw new InvalidOperationException("Fresh cache must not request network"));
        var second = await new GitHubRepositoryCatalogSource(offline, new VerifiedDocumentCache(fixture.Root))
            .ListAsync("AFROWAVELTD");
        Assert.Equal(GitHubRepositoryCatalogState.FreshCache, second.State);
        Assert.Equal(2, second.Repositories.Count);
    }

    [Fact]
    public async Task UserAccountUsesOwnedPublicReposEndpoint()
    {
        using var fixture = new Fixture();
        var requests = new List<string>();
        using var client = Client(request =>
        {
            requests.Add(request.RequestUri!.AbsoluteUri);
            return request.RequestUri.AbsolutePath == "/users/octocat"
                ? Ok("""{"type":"User"}""")
                : Ok("""[{"name":"Hello-World","description":null,"private":false}]""");
        });

        var result = await new GitHubRepositoryCatalogSource(client, new VerifiedDocumentCache(fixture.Root))
            .ListAsync("octocat");
        Assert.Equal(GitHubRepositoryCatalogState.Downloaded, result.State);
        Assert.Equal("Hello-World", Assert.Single(result.Repositories).Name);
        Assert.EndsWith("/users/octocat/repos?per_page=100&page=1&type=owner", requests[1]);
    }

    [Fact]
    public async Task MultiplePagesAreCombinedAndPrivateOrUnsafeEntriesAreIgnored()
    {
        using var fixture = new Fixture();
        var requests = new List<string>();
        using var client = Client(request =>
        {
            var uri = request.RequestUri!;
            requests.Add(uri.AbsoluteUri);
            if (uri.AbsolutePath == "/users/octocat")
                return Ok("""{"type":"User"}""");

            // Match the exact page parameter, not a substring: "per_page=100"
            // also contains "page=1", which would incorrectly repeat page 1.
            if (uri.Query.Split('&').Any(parameter =>
                parameter.Equals("page=1", StringComparison.Ordinal)))
                return Ok("""[{"name":"Bravo","private":false},{"name":"bad/name"},{"name":"Secret","private":true},{"name":"Bravo"}]""", next: true);
            return Ok("""[{"name":"Alpha","description":"A\nB","private":false}]""");
        });

        var result = await new GitHubRepositoryCatalogSource(client, new VerifiedDocumentCache(fixture.Root))
            .ListAsync("octocat");
        Assert.Equal(new[]
        {
            "https://api.github.com/users/octocat",
            "https://api.github.com/users/octocat/repos?per_page=100&page=1&type=owner",
            "https://api.github.com/users/octocat/repos?per_page=100&page=2&type=owner"
        }, requests);
        Assert.Equal(new[] { "Alpha", "Bravo" }, result.Repositories.Select(r => r.Name).ToArray());
        Assert.Equal("AB", result.Repositories[0].Description);
        Assert.False(result.Truncated);
    }

    [Fact]
    public async Task OfflineUsesStaleSnapshotWhenCacheIsOlderThanFreshnessWindow()
    {
        using var fixture = new Fixture();
        using (var first = Client(request => request.RequestUri!.AbsolutePath.EndsWith("/repos", StringComparison.Ordinal)
            ? Ok("""[{"name":"Cached","private":false}]""")
            : Ok("""{"type":"Organization"}""")))
        {
            var initial = await new GitHubRepositoryCatalogSource(first, new VerifiedDocumentCache(fixture.Root))
                .ListAsync("afrowaveltd");
            Assert.Single(initial.Repositories);
        }

        using var offline = Client(_ => throw new HttpRequestException("Offline"));
        var later = new TestClock(DateTimeOffset.UtcNow.AddMinutes(35));
        var cached = await new GitHubRepositoryCatalogSource(
            offline, new VerifiedDocumentCache(fixture.Root), later).ListAsync("AFROWAVELTD");
        Assert.Equal(GitHubRepositoryCatalogState.Stale, cached.State);
        Assert.Contains("last verified list", cached.Warning);
        Assert.Equal("Cached", Assert.Single(cached.Repositories).Name);
    }

    [Fact]
    public async Task MissingOwnerWithoutSnapshotReportsError()
    {
        using var fixture = new Fixture();
        using var client = Client(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        var result = await new GitHubRepositoryCatalogSource(client, new VerifiedDocumentCache(fixture.Root))
            .ListAsync("missing");
        Assert.Equal(GitHubRepositoryCatalogState.Error, result.State);
        Assert.Empty(result.Repositories);
        Assert.Contains("HTTP 404", result.Warning);
    }

    [Fact]
    public async Task CorruptApiJsonIsNotSaved()
    {
        using var fixture = new Fixture();
        using var client = Client(request => request.RequestUri!.AbsolutePath.EndsWith("/repos", StringComparison.Ordinal)
            ? Ok("not JSON") : Ok("""{"type":"Organization"}"""));
        var source = new GitHubRepositoryCatalogSource(client, new VerifiedDocumentCache(fixture.Root));
        var result = await source.ListAsync("afrowaveltd");
        Assert.Equal(GitHubRepositoryCatalogState.Error, result.State);
        Assert.Empty(result.Repositories);
        using var missingNetwork = Client(_ => throw new HttpRequestException());
        var repeat = await new GitHubRepositoryCatalogSource(missingNetwork,
            new VerifiedDocumentCache(fixture.Root)).ListAsync("afrowaveltd");
        Assert.Equal(GitHubRepositoryCatalogState.Error, repeat.State);
    }

    [Theory]
    [InlineData("")]
    [InlineData("../user")]
    [InlineData("owner/name")]
    [InlineData("a?secret=1")]
    [InlineData("invalid\n")]
    public async Task InvalidOwnersAreRejectedBeforeNetwork(string owner)
    {
        using var fixture = new Fixture();
        using var client = Client(_ => throw new InvalidOperationException("No HTTP allowed"));
        var source = new GitHubRepositoryCatalogSource(client, new VerifiedDocumentCache(fixture.Root));
        await Assert.ThrowsAsync<ArgumentException>(() => source.ListAsync(owner));
    }

    [Fact]
    public async Task PaginationIsBoundedAndMarkedTruncated()
    {
        using var fixture = new Fixture();
        var requests = 0;
        using var client = Client(request =>
        {
            requests++;
            return request.RequestUri!.AbsolutePath.EndsWith("/repos", StringComparison.Ordinal)
                ? Ok("""[{"name":"Repo"}]""", next: true)
                : Ok("""{"type":"Organization"}""");
        });
        var result = await new GitHubRepositoryCatalogSource(client,
            new VerifiedDocumentCache(fixture.Root)).ListAsync("afrowaveltd");
        Assert.Equal(GitHubRepositoryCatalogState.Downloaded, result.State);
        Assert.True(result.Truncated);
        Assert.Contains("first 1,000", result.Warning);
        Assert.Equal(11, requests); // 1 account lookup + 10 bounded pages
    }

    private static HttpResponseMessage Ok(string json, bool next = false)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        if (next)
            response.Headers.TryAddWithoutValidation("Link",
                "<https://api.github.com/users/a/repos?page=2>; rel=\"next\"");
        return response;
    }

    private static HttpClient Client(Func<HttpRequestMessage, HttpResponseMessage> response) =>
        new(new Handler(response), disposeHandler: true);

    private sealed class Handler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken) => Task.FromResult(respond(request));
    }

    private sealed class TestClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class Fixture : IDisposable
    {
        public string Root { get; } =
            Path.Combine(Path.GetTempPath(), "aw-github-catalog-" + Guid.NewGuid().ToString("N"));

        public void Dispose()
        {
            if (Directory.Exists(Root)) Directory.Delete(Root, recursive: true);
        }
    }
}
