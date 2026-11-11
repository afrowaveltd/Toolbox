using System.Net;
using System.Net.Http;
using System.Text;
using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.Cache;
using Afrowave.Toolbox.Blackboard.Tools.MdView;
using Spectre.Console.Testing;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView.Tests;

/// <summary>Exact-ref GitHub Markdown navigation and terminal integration regression tests.</summary>
public sealed class MdViewGitHubNavigationTests
{
    [Fact]
    public void GitHubBrowseCommandDoesNotChangeTheExistingGitMode()
    {
        var two = MdViewCommand.Parse("browse-git", "owner/repo");
        var three = MdViewCommand.Parse("BROWSE-GIT", "owner", "repo");
        Assert.Equal(MdViewSourceKind.GitHubLinkBrowser, two.Kind);
        Assert.Equal(MdViewSourceKind.GitHubLinkBrowser, three.Kind);
        Assert.Equal("owner", two.Value);
        Assert.Equal("repo", three.Repository);
        Assert.Equal(MdViewSourceKind.GitHub,
            MdViewCommand.Parse("git", "owner", "repo").Kind);
    }

    [Fact]
    public async Task NestedLinksAndBackForwardStayOnDiscoveredRef()
    {
        using var temp = new Fixture();
        var requests = new List<string>();
        using var http = Client(request =>
        {
            requests.Add(request.RequestUri!.AbsoluteUri);
            return Reply(request);
        });
        var session = await MdViewGitHubNavigationSession.OpenAsync(
            Source(http, temp.Root), "owner", "repo");

        Assert.Equal("feature/v2", session.Readme.File.Reference);
        Assert.Equal("README.md", session.Current.Target.Origin.Location);
        await session.FollowAsync("Docs/en.md#installation");
        Assert.Equal("Docs/en.md", session.Current.Target.Origin.Location);
        Assert.Equal("installation", session.Current.Target.Fragment);
        Assert.Equal("feature/v2", session.Current.Target.Origin.Ref);
        Assert.True(await session.BackAsync());
        Assert.Equal("README.md", session.Current.Target.Origin.Location);
        Assert.True(await session.ForwardAsync());
        Assert.Equal("installation", session.Current.Target.Fragment);
        Assert.Equal(6, requests.Count);
        Assert.All(requests.Skip(2), x => Assert.Contains("ref=feature%2Fv2", x));
    }

    [Fact]
    public async Task FragmentOnlyHistoryNeverRefetchesTheFile()
    {
        using var temp = new Fixture();
        var requests = 0;
        using var http = Client(request =>
        {
            requests++;
            return Reply(request);
        });
        var session = await MdViewGitHubNavigationSession.OpenAsync(
            Source(http, temp.Root), "owner", "repo");
        await session.FollowAsync("#start");
        Assert.True(await session.BackAsync());
        Assert.True(await session.ForwardAsync());
        Assert.Equal(3, requests);
        Assert.Equal(2, session.History.Count);
        Assert.Equal("start", session.Current.Target.Fragment);
    }

    [Fact]
    public async Task MissingRemoteTargetLeavesCurrentPageAndHistoryUnchanged()
    {
        using var temp = new Fixture();
        using var http = Client(request =>
            request.RequestUri!.AbsolutePath.EndsWith("/contents/Docs/missing.md",
                StringComparison.Ordinal)
                ? new HttpResponseMessage(HttpStatusCode.NotFound)
                : Reply(request));
        var session = await MdViewGitHubNavigationSession.OpenAsync(
            Source(http, temp.Root), "owner", "repo");

        await Assert.ThrowsAsync<IOException>(() =>
            session.FollowAsync("Docs/missing.md"));
        Assert.Equal("README.md", session.Current.Target.Origin.Location);
        Assert.Equal(1, session.History.Count);
    }

    [Fact]
    public async Task OversizedLinkedFileDoesNotAdvanceHistory()
    {
        using var temp = new Fixture();
        using var http = Client(request =>
            request.RequestUri!.AbsolutePath.EndsWith("/contents/Docs/large.md",
                StringComparison.Ordinal)
                ? Raw(new string('a', 300))
                : Reply(request));
        var session = await MdViewGitHubNavigationSession.OpenAsync(
            Source(http, temp.Root), "owner", "repo",
            maximumDocumentBytes: 128);

        await Assert.ThrowsAsync<InvalidDataException>(() =>
            session.FollowAsync("Docs/large.md"));
        Assert.Equal("README.md", session.Current.Target.Origin.Location);
        Assert.Equal(1, session.History.Count);
    }

    [Fact]
    public async Task BrowserFollowsMarkdownAndReturnsViaVirtualTerminal()
    {
        using var temp = new Fixture();
        using var http = Client(Reply);
        var terminal = Interactive();
        terminal.Input.PushKey(ConsoleKey.Enter); // Follow from README
        terminal.Input.PushKey(ConsoleKey.Enter); // Docs/en.md
        terminal.Input.PushKey(ConsoleKey.Enter); // Back from Docs
        terminal.Input.PushKey(ConsoleKey.DownArrow); // Forward
        terminal.Input.PushKey(ConsoleKey.DownArrow); // Exit
        terminal.Input.PushKey(ConsoleKey.Enter);

        await new MdViewGitHubLinkBrowser(terminal, Source(http, temp.Root))
            .RunAsync("owner", "repo");
        Assert.Contains("Documentation", terminal.Output);
        Assert.Contains("Navigable Markdown links: 1", terminal.Output);
        Assert.Contains("Available links (first 3):", terminal.Output);
        Assert.Contains("Documentation → Docs/en.md", terminal.Output);
        Assert.Contains("Follow a link", terminal.Output);
        Assert.Contains("GitHub: owner/repo @ feature/v2", terminal.Output);
        Assert.Contains("Source: Downloaded", terminal.Output);
        Assert.Contains("Forward", terminal.Output);
    }

    [Fact]
    public async Task LongGitHubReadmeDisplaysPagesInsteadOfDumpingTheWholeSource()
    {
        using var temp = new Fixture();
        var markdown = "# Intro\n" +
            string.Join("\n", Enumerable.Range(1, 70).Select(n => "sample line " + n));
        using var http = Client(request =>
            request.RequestUri!.AbsolutePath.EndsWith("/contents/README.md",
                StringComparison.Ordinal)
                ? Raw(markdown)
                : Reply(request));
        var terminal = Interactive();
        terminal.Input.PushKey(ConsoleKey.Enter); // Next page
        terminal.Input.PushKey(ConsoleKey.Escape);

        await new MdViewGitHubLinkBrowser(terminal, Source(http, temp.Root))
            .RunAsync("owner", "repo");

        Assert.Contains("Page: source lines 1–24", terminal.Output);
        Assert.Contains("Page: source lines 25–48", terminal.Output);
        Assert.Contains("sample line 47", terminal.Output);
        Assert.DoesNotContain("sample line 70", terminal.Output);
    }

    [Fact]
    public async Task PreviousPageReturnsToTheOriginalSourceRange()
    {
        using var temp = new Fixture();
        var markdown = "# Intro\n" +
            string.Join("\n", Enumerable.Range(1, 70).Select(n => "line " + n));
        var requests = 0;
        using var http = Client(request =>
        {
            requests++;
            return request.RequestUri!.AbsolutePath.EndsWith("/contents/README.md",
                StringComparison.Ordinal)
                ? Raw(markdown)
                : Reply(request);
        });
        var terminal = Interactive();
        terminal.Input.PushKey(ConsoleKey.Enter); // Next page
        terminal.Input.PushKey(ConsoleKey.DownArrow); // Previous page
        terminal.Input.PushKey(ConsoleKey.Enter);
        terminal.Input.PushKey(ConsoleKey.Escape);

        await new MdViewGitHubLinkBrowser(terminal, Source(http, temp.Root))
            .RunAsync("owner", "repo");

        Assert.Equal(2, terminal.Output.Split(
            "Page: source lines 1–24", StringSplitOptions.None).Length - 1);
        Assert.Contains("Page: source lines 25–48", terminal.Output);
        Assert.Equal(3, requests); // Branch + README metadata + one source fetch.
    }

    [Fact]
    public async Task LongGitHubReadmeAllowsJumpingDirectlyToAHeading()
    {
        using var temp = new Fixture();
        var markdown = "# Intro\n" +
            string.Join("\n", Enumerable.Range(2, 48).Select(n => "line " + n)) +
            "\n## Target heading\n" +
            string.Join("\n", Enumerable.Range(51, 20).Select(n => "line " + n));
        using var http = Client(request =>
            request.RequestUri!.AbsolutePath.EndsWith("/contents/README.md",
                StringComparison.Ordinal)
                ? Raw(markdown)
                : Reply(request));
        var terminal = Interactive();
        terminal.Input.PushKey(ConsoleKey.DownArrow); // Go to heading
        terminal.Input.PushKey(ConsoleKey.Enter);
        terminal.Input.PushKey(ConsoleKey.DownArrow); // Target heading
        terminal.Input.PushKey(ConsoleKey.Enter);
        terminal.Input.PushKey(ConsoleKey.Escape);

        await new MdViewGitHubLinkBrowser(terminal, Source(http, temp.Root))
            .RunAsync("owner", "repo");

        Assert.Contains("Page: source lines 50–", terminal.Output);
        Assert.Contains("Target heading", terminal.Output);
    }

    [Fact]
    public async Task LongGitHubReadmeAllowsJumpingToSourceLine()
    {
        using var temp = new Fixture();
        var markdown = "# Intro\n" +
            string.Join("\n", Enumerable.Range(2, 70).Select(n => "line " + n));
        using var http = Client(request =>
            request.RequestUri!.AbsolutePath.EndsWith("/contents/README.md",
                StringComparison.Ordinal)
                ? Raw(markdown)
                : Reply(request));
        var terminal = Interactive();
        terminal.Input.PushKey(ConsoleKey.DownArrow); // Go to heading
        terminal.Input.PushKey(ConsoleKey.DownArrow); // Go to source line
        terminal.Input.PushKey(ConsoleKey.Enter);
        terminal.Input.PushTextWithEnter("42");
        terminal.Input.PushKey(ConsoleKey.Escape);

        await new MdViewGitHubLinkBrowser(terminal, Source(http, temp.Root))
            .RunAsync("owner", "repo");

        Assert.Contains("Page: source lines 42–65", terminal.Output);
        Assert.Contains("line 42", terminal.Output);
    }

    [Fact]
    public async Task LinksRemainUsableWhenReadmeIsPaged()
    {
        using var temp = new Fixture();
        var markdown = "# Home\n[Documentation](Docs/en.md)\n" +
            string.Join("\n", Enumerable.Range(1, 60).Select(n => "line " + n));
        using var http = Client(request =>
            request.RequestUri!.AbsolutePath.EndsWith("/contents/README.md",
                StringComparison.Ordinal)
                ? Raw(markdown)
                : Reply(request));
        var terminal = Interactive();
        terminal.Input.PushKey(ConsoleKey.Enter); // Follow link
        terminal.Input.PushKey(ConsoleKey.Enter); // Documentation
        terminal.Input.PushKey(ConsoleKey.Enter); // Back from Docs
        terminal.Input.PushKey(ConsoleKey.Escape); // Exit on original page

        await new MdViewGitHubLinkBrowser(terminal, Source(http, temp.Root))
            .RunAsync("owner", "repo");

        Assert.Contains("Page: source lines 1–24", terminal.Output);
        Assert.Contains("GitHub: owner/repo @ feature/v2 / Docs/en.md",
            terminal.Output);
        Assert.Contains("GitHub: owner/repo @ feature/v2 / README.md",
            terminal.Output);
    }

    [Fact]
    public async Task NoninteractiveBrowserDoesNotPerformNetworkRequests()
    {
        using var temp = new Fixture();
        using var http = Client(_ => throw new InvalidOperationException("No HTTP request expected."));
        var terminal = new TestConsole();
        terminal.Profile.Capabilities.Interactive = false;
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new MdViewGitHubLinkBrowser(terminal, Source(http, temp.Root))
                .RunAsync("owner", "repo"));
    }

    private static GitHubRepositoryFileSource Source(HttpClient http, string root) =>
        new(http, new VerifiedDocumentCache(root));

    private static HttpResponseMessage Reply(HttpRequestMessage request)
    {
        var path = request.RequestUri!.AbsolutePath;
        if (path.EndsWith("/readme", StringComparison.Ordinal))
            return Json("""{"path":"README.md"}""");
        if (path.EndsWith("/contents/README.md", StringComparison.Ordinal))
            return Raw("# Home\n[Documentation](Docs/en.md)");
        if (path.EndsWith("/contents/Docs/en.md", StringComparison.Ordinal))
            return Raw("# Documentation\n## Installation");
        return Json("""{"default_branch":"feature/v2"}""");
    }

    private static HttpResponseMessage Raw(string content) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(content, Encoding.UTF8, "text/markdown")
        };

    private static HttpResponseMessage Json(string content) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        };

    private static HttpClient Client(Func<HttpRequestMessage, HttpResponseMessage> responder) =>
        new(new Handler(responder), disposeHandler: true);

    private static TestConsole Interactive()
    {
        var terminal = new TestConsole();
        terminal.Profile.Capabilities.Interactive = true;
        terminal.Profile.Capabilities.Unicode = true;
        terminal.Profile.Width = 120;
        return terminal;
    }

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
            Path.GetTempPath(), "mdview-github-navigation-" + Guid.NewGuid().ToString("N"));

        public void Dispose()
        {
            if (Directory.Exists(Root))
                Directory.Delete(Root, recursive: true);
        }
    }
}
