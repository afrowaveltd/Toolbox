using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.Cache;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.Cache.Tests;

public sealed class HttpMarkdownDocumentSourceTests
{
    private static readonly Uri Address = new("https://example.org/README.md");

    [Fact]
    public async Task Conditional304ReusesPersistedDocumentAcrossInstances()
    {
        using var temp = new TempCache();
        var calls = 0;
        using var client = Client(request =>
        {
            calls++;
            if (calls == 1)
            {
                Assert.Empty(request.Headers.IfNoneMatch);
                return Ok("# First", "\"revision-1\"");
            }
            Assert.Equal("\"revision-1\"", Assert.Single(request.Headers.IfNoneMatch).ToString());
            return new HttpResponseMessage(HttpStatusCode.NotModified);
        });
        var first = new HttpMarkdownDocumentSource(client, new VerifiedDocumentCache(temp.Root));
        var downloaded = await first.OpenAsync(Address);
        Assert.Equal(HttpDocumentState.Downloaded, downloaded.State);
        var second = new HttpMarkdownDocumentSource(client, new VerifiedDocumentCache(temp.Root));
        var unchanged = await second.OpenAsync(Address);
        Assert.Equal(HttpDocumentState.NotModified, unchanged.State);
        Assert.Equal(downloaded.Document!.Sha256, unchanged.Document!.Sha256);
        Assert.Equal("# First", await File.ReadAllTextAsync(unchanged.Document.ContentPath));
        Assert.Equal(2, calls);
        Assert.NotNull(unchanged.Document.CheckedAtUtc);
    }

    [Fact]
    public async Task Modified200AtomicallyReplacesOldContent()
    {
        using var temp = new TempCache();
        var count = 0;
        using var client = Client(_ => ++count == 1 ? Ok("# Old", "\"v1\"") : Ok("# New", "\"v2\""));
        var source = new HttpMarkdownDocumentSource(client, new VerifiedDocumentCache(temp.Root));
        var before = await source.OpenAsync(Address);
        var after = await source.OpenAsync(Address);
        Assert.Equal(HttpDocumentState.Downloaded, after.State);
        Assert.NotEqual(before.Document!.Sha256, after.Document!.Sha256);
        Assert.Equal("\"v2\"", after.Document.ETag);
        Assert.Equal("# New", await File.ReadAllTextAsync(after.Document.ContentPath));
    }

    [Fact]
    public async Task NetworkFailureOffersExplicitlyStaleCopy()
    {
        using var temp = new TempCache();
        using (var initialClient = Client(_ => Ok("# Offline", "\"v1\"")))
            await new HttpMarkdownDocumentSource(initialClient, new VerifiedDocumentCache(temp.Root)).OpenAsync(Address);

        using var failedClient = Client(_ => throw new HttpRequestException("offline"));
        var result = await new HttpMarkdownDocumentSource(failedClient,
            new VerifiedDocumentCache(temp.Root)).OpenAsync(Address);
        Assert.Equal(HttpDocumentState.Stale, result.State);
        Assert.NotNull(result.Warning);
        Assert.Equal("# Offline", await File.ReadAllTextAsync(result.Document!.ContentPath));
    }

    [Fact]
    public async Task InvalidNewDocumentDoesNotOverwriteValidCache()
    {
        using var temp = new TempCache();
        var count = 0;
        using var client = Client(_ =>
        {
            if (++count == 1) return Ok("# Safe", "\"v1\"");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(new byte[] { 0x23, 0x20, 0x1b })
            };
        });
        var source = new HttpMarkdownDocumentSource(client, new VerifiedDocumentCache(temp.Root));
        var old = await source.OpenAsync(Address);
        var fallback = await source.OpenAsync(Address);
        Assert.Equal(HttpDocumentState.Stale, fallback.State);
        Assert.Equal(old.Document!.Sha256, fallback.Document!.Sha256);
    }

    [Fact]
    public async Task HttpErrorWithoutCacheReturnsError()
    {
        using var temp = new TempCache();
        using var client = Client(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        var result = await new HttpMarkdownDocumentSource(client,
            new VerifiedDocumentCache(temp.Root)).OpenAsync(Address);
        Assert.Equal(HttpDocumentState.Error, result.State);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Null(result.Document);
    }

    [Fact]
    public async Task RejectsInsecureOrQueryBearingUrls()
    {
        using var temp = new TempCache();
        using var client = Client(_ => throw new Exception("Should not be invoked"));
        var source = new HttpMarkdownDocumentSource(client, new VerifiedDocumentCache(temp.Root));
        await Assert.ThrowsAsync<ArgumentException>(() => source.OpenAsync(new Uri("http://example.org/readme.md")));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            source.OpenAsync(new Uri("https://example.org/readme.md?token=secret")));
    }

    private static HttpClient Client(Func<HttpRequestMessage, HttpResponseMessage> callback) =>
        new(new Handler(callback), disposeHandler: true);

    private static HttpResponseMessage Ok(string text, string etag)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(text, Encoding.UTF8, "text/markdown")
        };
        response.Headers.ETag = EntityTagHeaderValue.Parse(etag);
        return response;
    }

    private sealed class Handler(Func<HttpRequestMessage, HttpResponseMessage> callback) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(callback(request));
    }

    private sealed class TempCache : IDisposable
    {
        public string Root { get; } = Path.Combine(Path.GetTempPath(), "aw-http-cache-" + Guid.NewGuid().ToString("N"));
        public void Dispose()
        {
            if (Directory.Exists(Root)) Directory.Delete(Root, true);
        }
    }
}
