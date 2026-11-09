using System.Net;
using System.Net.Http;
using System.Text;
using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.Cache;
using Afrowave.Toolbox.Blackboard.Tools.MdView;
using Spectre.Console.Testing;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView.Tests;

/// <summary>HTTPS navigation, verified offline cache and terminal selection tests.</summary>
public sealed class MdViewHttpsNavigationTests
{
    private const string HomeUrl = "https://docs.example.org/README.md";

    [Fact]
    public void HttpsBrowserRequiresExplicitCommand()
    {
        var command = MdViewCommand.Parse("BROWSE-URL", HomeUrl);
        Assert.Equal(MdViewSourceKind.HttpsLinkBrowser, command.Kind);
        Assert.Equal(HomeUrl, command.Value);
        Assert.Equal(MdViewSourceKind.Url,
            MdViewCommand.Parse("url", HomeUrl).Kind);
    }

    [Fact]
    public async Task RelativeLinksAndBackForwardRevalidateSource()
    {
        using var temp = new Fixture();
        var requested = new List<string>();
        using var http = Client(request =>
        {
            requested.Add(request.RequestUri!.AbsoluteUri);
            return Ok(request.RequestUri.AbsolutePath == "/README.md"
                ? "# Home\n[Install](Docs/en.md)"
                : "# Documentation");
        });
        var cache = new VerifiedDocumentCache(temp.Root);
        var source = new HttpMarkdownDocumentSource(http, cache);
        var session = await MdViewHttpsNavigationSession.OpenAsync(
            source, MdViewDocumentOrigin.FromHttps(HomeUrl));

        await session.FollowAsync("Docs/en.md#documentation");
        Assert.Equal("https://docs.example.org/Docs/en.md",
            session.Current.Target.Origin.Location);
        Assert.Equal("documentation", session.Current.Target.Fragment);
        Assert.True(await session.BackAsync());
        Assert.Equal(HomeUrl, session.Current.Target.Origin.Location);
        Assert.True(await session.ForwardAsync());
        Assert.Equal("documentation", session.Current.Target.Fragment);
        Assert.Equal(new[] {
            HomeUrl, "https://docs.example.org/Docs/en.md",
            HomeUrl, "https://docs.example.org/Docs/en.md"
        }, requested);
    }

    [Fact]
    public async Task SameDocumentAnchorDoesNotTriggerHttpRequest()
    {
        using var temp = new Fixture();
        var calls = 0;
        using var http = Client(_ =>
        {
            calls++;
            return Ok("# Introduction\n## Installation");
        });
        var session = await MdViewHttpsNavigationSession.OpenAsync(
            new HttpMarkdownDocumentSource(http, new VerifiedDocumentCache(temp.Root)),
            MdViewDocumentOrigin.FromHttps(HomeUrl));

        await session.FollowAsync("#installation");
        Assert.Equal("installation", session.Current.Target.Fragment);
        Assert.True(await session.BackAsync());
        Assert.True(await session.ForwardAsync());
        Assert.Equal(1, calls);
        Assert.Equal(2, session.History.Count);
    }

    [Fact]
    public async Task UnavailableTargetDoesNotDiscardCurrentPage()
    {
        using var temp = new Fixture();
        using var http = Client(request =>
            request.RequestUri!.AbsolutePath == "/README.md"
                ? Ok("# Home")
                : new HttpResponseMessage(HttpStatusCode.NotFound));
        var session = await MdViewHttpsNavigationSession.OpenAsync(
            new HttpMarkdownDocumentSource(http, new VerifiedDocumentCache(temp.Root)),
            MdViewDocumentOrigin.FromHttps(HomeUrl));

        await Assert.ThrowsAsync<IOException>(() =>
            session.FollowAsync("Docs/missing.md"));
        Assert.Equal(HomeUrl, session.Current.Target.Origin.Location);
        Assert.Equal(1, session.History.Count);
        Assert.False(session.History.CanGoBack);
    }

    [Fact]
    public async Task BackNavigationExplicitlyUsesStaleValidatedCache()
    {
        using var temp = new Fixture();
        var homeRequests = 0;
        using var http = Client(request =>
        {
            if (request.RequestUri!.AbsolutePath != "/README.md")
                return Ok("# Docs");
            if (++homeRequests == 1)
                return Ok("# Offline Home");
            throw new HttpRequestException("Offline");
        });
        var session = await MdViewHttpsNavigationSession.OpenAsync(
            new HttpMarkdownDocumentSource(http, new VerifiedDocumentCache(temp.Root)),
            MdViewDocumentOrigin.FromHttps(HomeUrl));

        await session.FollowAsync("Docs/en.md");
        Assert.True(await session.BackAsync());
        Assert.Equal(HttpDocumentState.Stale, session.Current.Result.State);
        Assert.NotNull(session.Current.Result.Warning);
        Assert.Equal("# Offline Home", await File.ReadAllTextAsync(
            session.Current.Result.Document!.ContentPath));
    }

    [Fact]
    public async Task BrowserRendersLinkedDocumentAndBackViaVirtualTerminal()
    {
        using var temp = new Fixture();
        using var http = Client(request => Ok(
            request.RequestUri!.AbsolutePath == "/README.md"
                ? "# Home\n[Read docs](Docs/en.md)"
                : "# Linked Documentation"));
        var console = Interactive();
        console.Input.PushKey(ConsoleKey.Enter); // Follow
        console.Input.PushKey(ConsoleKey.Enter); // Choose Read docs
        console.Input.PushKey(ConsoleKey.Enter); // Back (Docs: only Back and Exit)
        console.Input.PushKey(ConsoleKey.DownArrow); // Forward
        console.Input.PushKey(ConsoleKey.DownArrow); // Exit
        console.Input.PushKey(ConsoleKey.Enter);

        await new MdViewHttpsLinkBrowser(console,
            new HttpMarkdownDocumentSource(http, new VerifiedDocumentCache(temp.Root)))
            .RunAsync(HomeUrl);

        Assert.Contains("Linked Documentation", console.Output);
        Assert.Contains("Source: Downloaded", console.Output);
        Assert.Contains("Forward", console.Output);
    }

    [Fact]
    public async Task NoninteractiveBrowserFailsBeforeNetworkRequest()
    {
        using var temp = new Fixture();
        using var http = Client(_ =>
            throw new InvalidOperationException("Network must not be contacted."));
        var console = new TestConsole();
        console.Profile.Capabilities.Interactive = false;

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new MdViewHttpsLinkBrowser(console,
                new HttpMarkdownDocumentSource(http, new VerifiedDocumentCache(temp.Root)))
                .RunAsync(HomeUrl));
    }

    private static TestConsole Interactive()
    {
        var console = new TestConsole();
        console.Profile.Capabilities.Interactive = true;
        console.Profile.Capabilities.Unicode = true;
        console.Profile.Width = 120;
        return console;
    }

    private static HttpClient Client(Func<HttpRequestMessage, HttpResponseMessage> responder) =>
        new(new Handler(responder), disposeHandler: true);

    private static HttpResponseMessage Ok(string content) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(content, Encoding.UTF8, "text/markdown")
        };

    private sealed class Handler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(responder(request));
    }

    private sealed class Fixture : IDisposable
    {
        public string Root { get; } = Path.Combine(
            Path.GetTempPath(), "mdview-https-nav-" + Guid.NewGuid().ToString("N"));

        public void Dispose()
        {
            if (Directory.Exists(Root))
                Directory.Delete(Root, recursive: true);
        }
    }
}
