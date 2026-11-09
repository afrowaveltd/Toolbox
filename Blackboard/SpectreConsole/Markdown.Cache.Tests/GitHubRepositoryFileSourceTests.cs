using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.Cache;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.Cache.Tests;

/// <summary>GitHub repository identity, exact-ref file and verified cache regression tests.</summary>
public sealed class GitHubRepositoryFileSourceTests
{
    [Fact]
    public async Task DiscoversActualBranchAndNestedReadmeBeforeRetrievingItsBytes()
    {
        using var temp = new Fixture();
        var requested = new List<string>();
        using var http = Client(request =>
        {
            requested.Add(request.RequestUri!.AbsoluteUri);
            Assert.Contains(request.Headers.Accept,
                item => item.MediaType == (request.RequestUri.AbsolutePath.Contains("/contents/")
                    ? "application/vnd.github.raw+json" : "application/vnd.github+json"));
            Assert.NotEmpty(request.Headers.UserAgent);
            return Reply(request);
        });
        var source = new GitHubRepositoryFileSource(
            http, new VerifiedDocumentCache(temp.Root));

        var metadata = await source.ResolveReadmeAsync("AfrowaveLtd", "Toolbox");
        Assert.Equal("release/v2", metadata.File.Reference);
        Assert.Equal("Docs/README.md", metadata.File.Path);
        Assert.Equal(HttpDocumentState.Downloaded, metadata.State);

        var document = await source.OpenAsync(metadata.File);
        Assert.Equal(HttpDocumentState.Downloaded, document.State);
        Assert.Equal("# GitHub Docs\n[Setup](Setup/en.md)",
            await File.ReadAllTextAsync(document.Document!.ContentPath));
        Assert.Equal(3, requested.Count);
        Assert.Equal("https://api.github.com/repos/AfrowaveLtd/Toolbox", requested[0]);
        Assert.Contains("/readme?ref=release%2Fv2", requested[1]);
        Assert.Contains("/contents/Docs/README.md?ref=release%2Fv2", requested[2]);
    }

    [Fact]
    public async Task ConditionalRequestsReuseExactlyMatchedMetadataAndFile()
    {
        using var temp = new Fixture();
        var calls = 0;
        using var http = Client(request =>
        {
            calls++;
            if (calls > 3)
            {
                Assert.Equal("\"v1\"", Assert.Single(request.Headers.IfNoneMatch).ToString());
                return new HttpResponseMessage(HttpStatusCode.NotModified);
            }
            return Reply(request);
        });
        var reader = new GitHubRepositoryFileSource(
            http, new VerifiedDocumentCache(temp.Root));
        var first = await reader.ResolveReadmeAsync("AfrowaveLtd", "Toolbox");
        await reader.OpenAsync(first.File);
        var second = await reader.ResolveReadmeAsync("AFROWAVELTD", "toolbox");
        var content = await reader.OpenAsync(second.File);

        Assert.Equal(6, calls);
        Assert.Equal(HttpDocumentState.NotModified, second.State);
        Assert.Equal(HttpDocumentState.NotModified, content.State);
        Assert.NotNull(content.Document);
    }

    [Fact]
    public async Task OfflineFallbackRetainsMetadataAndFileFromPriorVerifiedCopy()
    {
        using var temp = new Fixture();
        using (var http = Client(Reply))
        {
            var provider = new GitHubRepositoryFileSource(
                http, new VerifiedDocumentCache(temp.Root));
            var readme = await provider.ResolveReadmeAsync("AfrowaveLtd", "Toolbox");
            await provider.OpenAsync(readme.File);
        }

        using var offline = Client(_ => throw new HttpRequestException("offline"));
        var reader = new GitHubRepositoryFileSource(
            offline, new VerifiedDocumentCache(temp.Root));
        var metadata = await reader.ResolveReadmeAsync("afrowaveltd", "toolbox");
        var file = await reader.OpenAsync(metadata.File);
        Assert.Equal(HttpDocumentState.Stale, metadata.State);
        Assert.NotNull(metadata.Warning);
        Assert.Equal("release/v2", metadata.File.Reference);
        Assert.Equal("Docs/README.md", metadata.File.Path);
        Assert.Equal(HttpDocumentState.Stale, file.State);
        Assert.Equal("# GitHub Docs\n[Setup](Setup/en.md)",
            await File.ReadAllTextAsync(file.Document!.ContentPath));
    }

    [Fact]
    public async Task UnsafeReadmeMetadataIsNeverUsedForAFileRequest()
    {
        using var temp = new Fixture();
        var calls = 0;
        using var http = Client(request =>
        {
            calls++;
            if (request.RequestUri!.AbsolutePath.EndsWith("/readme", StringComparison.Ordinal))
                return Json("""{"path":"../../secret.md"}""");
            if (request.RequestUri.AbsolutePath.Contains("/contents/", StringComparison.Ordinal))
                throw new InvalidOperationException("Unsafe path must not be fetched.");
            return Json("""{"default_branch":"main"}""");
        });
        var source = new GitHubRepositoryFileSource(
            http, new VerifiedDocumentCache(temp.Root));
        await Assert.ThrowsAsync<IOException>(() =>
            source.ResolveReadmeAsync("owner", "repo"));
        Assert.Equal(2, calls);
    }

    [Fact]
    public async Task IncorrectFileEncodingFallsBackWithoutReplacingPreviousBlob()
    {
        using var temp = new Fixture();
        var file = new GitHubRepositoryFile("owner", "repo", "main", "Docs/en.md");
        var attempts = 0;
        using var http = Client(_ =>
        {
            attempts++;
            if (attempts == 1)
                return Raw("# Verified");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(new byte[] { 0x23, 0x1B, 0x5B })
            };
        });
        var source = new GitHubRepositoryFileSource(
            http, new VerifiedDocumentCache(temp.Root));
        var initial = await source.OpenAsync(file);
        var fallback = await source.OpenAsync(file);
        Assert.Equal(HttpDocumentState.Downloaded, initial.State);
        Assert.Equal(HttpDocumentState.Stale, fallback.State);
        Assert.Equal(initial.Document!.Sha256, fallback.Document!.Sha256);
    }

    [Fact]
    public async Task InvalidSourcePathsAndRefAreRejectedBeforeRequest()
    {
        using var temp = new Fixture();
        using var http = Client(_ => throw new InvalidOperationException("No request expected."));
        var source = new GitHubRepositoryFileSource(
            http, new VerifiedDocumentCache(temp.Root));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            source.OpenAsync(new GitHubRepositoryFile("owner", "repo", "main", "../escape.md")));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            source.OpenAsync(new GitHubRepositoryFile("owner", "repo", "main", "Docs/%2e%2e.md")));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            source.OpenAsync(new GitHubRepositoryFile("owner", "repo", "main", "Docs/data.txt")));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            source.OpenAsync(new GitHubRepositoryFile("owner", "repo", "bad?ref", "Docs/en.md")));
    }

    [Fact]
    public async Task DifferentRefsHaveIndependentVerifiedCacheEntries()
    {
        using var temp = new Fixture();
        var calls = 0;
        using var http = Client(request =>
        {
            calls++;
            return Raw("# " + request.RequestUri!.Query);
        });
        var source = new GitHubRepositoryFileSource(
            http, new VerifiedDocumentCache(temp.Root));
        var main = await source.OpenAsync(
            new GitHubRepositoryFile("owner", "repo", "main", "Docs/en.md"));
        var feature = await source.OpenAsync(
            new GitHubRepositoryFile("owner", "repo", "feature/v2", "Docs/en.md"));
        Assert.Equal(2, calls);
        Assert.NotEqual(main.Document!.Sha256, feature.Document!.Sha256);
        Assert.Equal(HttpDocumentState.Downloaded, feature.State);
    }

    private static HttpResponseMessage Reply(HttpRequestMessage request)
    {
        var uri = request.RequestUri!;
        if (uri.AbsolutePath.EndsWith("/readme", StringComparison.Ordinal))
            return Json("""{"path":"Docs/README.md"}""");
        if (uri.AbsolutePath.Contains("/contents/", StringComparison.Ordinal))
            return Raw("# GitHub Docs\n[Setup](Setup/en.md)");
        return Json("""{"default_branch":"release/v2"}""");
    }

    private static HttpResponseMessage Json(string body) =>
        CreateResponse(body, "application/json");

    private static HttpResponseMessage Raw(string body) =>
        CreateResponse(body, "text/markdown");

    private static HttpResponseMessage CreateResponse(string body, string contentType)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(body, Encoding.UTF8, contentType)
        };
        response.Headers.ETag = EntityTagHeaderValue.Parse("\"v1\"");
        return response;
    }

    private static HttpClient Client(Func<HttpRequestMessage, HttpResponseMessage> response) =>
        new(new Handler(response), disposeHandler: true);

    private sealed class Handler(Func<HttpRequestMessage, HttpResponseMessage> response)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(response(request));
    }

    private sealed class Fixture : IDisposable
    {
        public string Root { get; } = Path.Combine(
            Path.GetTempPath(), "mdview-github-file-" + Guid.NewGuid().ToString("N"));

        public void Dispose()
        {
            if (Directory.Exists(Root))
                Directory.Delete(Root, recursive: true);
        }
    }
}
