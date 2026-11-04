using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.Cache;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.Cache.Tests;

public sealed class GitHubReadmeSourceTests
{
    [Fact]
    public async Task UsesOfficialPreferredReadmeEndpointAndRawMediaType()
    {
        using var fixture = new Fixture();
        var calls = 0;
        using var client = NewClient(request =>
        {
            calls++;
            Assert.Equal("https://api.github.com/repos/afrowaveltd/Toolbox/readme",
                request.RequestUri!.AbsoluteUri);
            Assert.Contains(request.Headers.Accept, x => x.MediaType == "application/vnd.github.raw+json");
            Assert.NotEmpty(request.Headers.UserAgent);
            Assert.Equal("2022-11-28", Assert.Single(request.Headers.GetValues("X-GitHub-Api-Version")));
            return Ok("# Toolbox", "\"etag1\"");
        });
        var reader = new GitHubReadmeSource(client, new VerifiedDocumentCache(fixture.Root));
        var result = await reader.OpenAsync("afrowaveltd", "Toolbox");
        Assert.Equal(HttpDocumentState.Downloaded, result.State);
        Assert.Equal("# Toolbox", await File.ReadAllTextAsync(result.Document!.ContentPath));
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task DifferentCapitalizationsSharePersistedCacheAndSendConditionalGet()
    {
        using var fixture = new Fixture();
        var calls = 0;
        using var client = NewClient(request =>
        {
            calls++;
            if (calls == 1) return Ok("## Cached README", "\"version-1\"");
            Assert.Equal("\"version-1\"", Assert.Single(request.Headers.IfNoneMatch).ToString());
            return new HttpResponseMessage(HttpStatusCode.NotModified);
        });
        var first = new GitHubReadmeSource(client, new VerifiedDocumentCache(fixture.Root));
        var before = await first.OpenAsync("AfrowaveLtd", "ToolBox");
        var second = new GitHubReadmeSource(client, new VerifiedDocumentCache(fixture.Root));
        var after = await second.OpenAsync("AFROWAVELTD", "toolbox");
        Assert.Equal(HttpDocumentState.NotModified, after.State);
        Assert.Equal(before.Document!.Sha256, after.Document!.Sha256);
        Assert.Equal(2, calls);
    }

    [Fact]
    public async Task OfflineGitHubReturnsVerifiedStaleCopy()
    {
        using var fixture = new Fixture();
        using (var success = NewClient(_ => Ok("# Saved", "\"v1\"")))
            await new GitHubReadmeSource(success, new VerifiedDocumentCache(fixture.Root))
                .OpenAsync("afrowaveltd", "toolbox");

        using var failed = NewClient(_ => throw new HttpRequestException("Offline"));
        var stale = await new GitHubReadmeSource(failed, new VerifiedDocumentCache(fixture.Root))
            .OpenAsync("AFROWAVELTD", "TOOLBOX");
        Assert.Equal(HttpDocumentState.Stale, stale.State);
        Assert.NotNull(stale.Warning);
        Assert.Equal("# Saved", await File.ReadAllTextAsync(stale.Document!.ContentPath));
    }

    [Fact]
    public async Task MissingReadmeWithoutCacheIsAnError()
    {
        using var fixture = new Fixture();
        using var client = NewClient(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        var result = await new GitHubReadmeSource(client, new VerifiedDocumentCache(fixture.Root))
            .OpenAsync("afrowaveltd", "missing");
        Assert.Equal(HttpDocumentState.Error, result.State);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Null(result.Document);
    }

    [Fact]
    public async Task UnsafeReplacementKeepsOldGitHubReadme()
    {
        using var fixture = new Fixture();
        var attempts = 0;
        using var client = NewClient(_ =>
        {
            if (++attempts == 1) return Ok("# Good", "\"one\"");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(new byte[] { 0x23, 0x1b, 0x5b })
            };
        });
        var reader = new GitHubReadmeSource(client, new VerifiedDocumentCache(fixture.Root));
        var initial = await reader.OpenAsync("afrowaveltd", "toolbox");
        var fallback = await reader.OpenAsync("afrowaveltd", "toolbox");
        Assert.Equal(HttpDocumentState.Stale, fallback.State);
        Assert.Equal(initial.Document!.Sha256, fallback.Document!.Sha256);
    }

    [Theory]
    [InlineData("../owner", "repo")]
    [InlineData("owner", "../repo")]
    [InlineData("owner/name", "repo")]
    [InlineData("owner", "")]
    [InlineData("", "repo")]
    [InlineData("owner", "repo?token=abc")]
    [InlineData("owner\n", "repo")]
    [InlineData("owner", "repo\n")]
    public async Task InvalidOwnerOrRepositoryNeverSendsRequest(string owner, string repository)
    {
        using var fixture = new Fixture();
        using var client = NewClient(_ => throw new InvalidOperationException("No HTTP request expected."));
        var reader = new GitHubReadmeSource(client, new VerifiedDocumentCache(fixture.Root));
        await Assert.ThrowsAsync<ArgumentException>(() => reader.OpenAsync(owner, repository));
    }

    [Fact]
    public async Task Unexpected304WithEmptyCacheIsNotSuccess()
    {
        using var fixture = new Fixture();
        using var client = NewClient(_ => new HttpResponseMessage(HttpStatusCode.NotModified));
        var result = await new GitHubReadmeSource(client, new VerifiedDocumentCache(fixture.Root))
            .OpenAsync("owner", "repo");
        Assert.Equal(HttpDocumentState.Error, result.State);
        Assert.Null(result.Document);
    }

    private static HttpResponseMessage Ok(string value, string etag)
    {
        var result = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(value, Encoding.UTF8, "text/markdown")
        };
        result.Headers.ETag = EntityTagHeaderValue.Parse(etag);
        return result;
    }

    private static HttpClient NewClient(Func<HttpRequestMessage, HttpResponseMessage> respond) =>
        new(new Handler(respond), disposeHandler: true);

    private sealed class Handler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken) => Task.FromResult(respond(request));
    }

    private sealed class Fixture : IDisposable
    {
        public string Root { get; } =
            Path.Combine(Path.GetTempPath(), "aw-github-readme-" + Guid.NewGuid().ToString("N"));

        public void Dispose()
        {
            if (Directory.Exists(Root)) Directory.Delete(Root, recursive: true);
        }
    }
}
