using System.Net;
using System.Net.Http;
using System.Text;
using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.Cache;
using Afrowave.Toolbox.Blackboard.Tools.MdView;
using Spectre.Console.Testing;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView.Tests;

public sealed class MdViewGitHubRepositoryPickerTests
{
    [Fact]
    public async Task SelectsExistingPublicRepositoryAndPreservesCase()
    {
        using var temp = new Fixture();
        using var http = Client(request => request.RequestUri!.AbsolutePath == "/users/AfrowaveLtd"
            ? Ok("""{"type":"Organization"}""")
            : Ok("""[{"name":"Ajis.Dotnet"},{"name":"Toolbox"}]"""));
        var console = Interactive();
        console.Input.PushKey(ConsoleKey.DownArrow); // Toolbox (alphabetical catalog)
        console.Input.PushKey(ConsoleKey.Enter);

        var picked = await new MdViewGitHubRepositoryPicker(console,
            new GitHubRepositoryCatalogSource(http, new VerifiedDocumentCache(temp.Root)))
            .PickAsync("AfrowaveLtd");

        Assert.NotNull(picked);
        Assert.Equal(MdViewSourceKind.GitHub, picked.Kind);
        Assert.Equal("AfrowaveLtd", picked.Value);
        Assert.Equal("Toolbox", picked.Repository);
        Assert.Contains("Ajis.Dotnet", console.Output);
        Assert.Contains("Toolbox", console.Output);
    }

    [Fact]
    public async Task EscapeCancelsWithoutFetchingReadme()
    {
        using var temp = new Fixture();
        var calls = 0;
        using var http = Client(request =>
        {
            calls++;
            Assert.DoesNotContain("/readme", request.RequestUri!.AbsolutePath);
            return request.RequestUri.AbsolutePath == "/users/owner"
                ? Ok("""{"type":"User"}""")
                : Ok("""[{"name":"One"}]""");
        });
        var console = Interactive();
        console.Input.PushKey(ConsoleKey.Escape);

        var picked = await new MdViewGitHubRepositoryPicker(console,
            new GitHubRepositoryCatalogSource(http, new VerifiedDocumentCache(temp.Root)))
            .PickAsync("owner");

        Assert.Null(picked);
        Assert.Equal(2, calls); // user account + repository catalog; no README
    }

    [Fact]
    public async Task FilterFindsRepositoryByDescription()
    {
        using var temp = new Fixture();
        using var http = Client(request => request.RequestUri!.AbsolutePath == "/users/owner"
            ? Ok("""{"type":"User"}""")
            : Ok("""[{"name":"First","description":"Other thing"},{"name":"Second","description":"Markdown reader"}]"""));
        var console = Interactive();
        console.Input.PushKey(ConsoleKey.DownArrow);
        console.Input.PushKey(ConsoleKey.DownArrow); // Filter after two repositories
        console.Input.PushKey(ConsoleKey.Enter);
        console.Input.PushTextWithEnter("reader");
        console.Input.PushKey(ConsoleKey.Enter); // only matching Second

        var picked = await new MdViewGitHubRepositoryPicker(console,
            new GitHubRepositoryCatalogSource(http, new VerifiedDocumentCache(temp.Root)))
            .PickAsync("owner");

        Assert.Equal("Second", picked?.Repository);
    }

    [Fact]
    public async Task RepositoryDescriptionCannotInjectSpectreMarkup()
    {
        using var temp = new Fixture();
        using var http = Client(request => request.RequestUri!.AbsolutePath == "/users/owner"
            ? Ok("""{"type":"User"}""")
            : Ok("""[{"name":"One","description":"[red]not markup[/]"}]"""));
        var console = Interactive();
        console.Input.PushKey(ConsoleKey.Enter);

        var picked = await new MdViewGitHubRepositoryPicker(console,
            new GitHubRepositoryCatalogSource(http, new VerifiedDocumentCache(temp.Root)))
            .PickAsync("owner");

        Assert.Equal("One", picked?.Repository);
        Assert.Contains("[red]not markup[/]", console.Output);
    }

    [Fact]
    public async Task NoninteractiveOwnerSelectionFailsBeforeHttp()
    {
        using var temp = new Fixture();
        using var http = Client(_ => throw new InvalidOperationException("No request expected"));
        var console = new TestConsole();
        console.Profile.Capabilities.Interactive = false;

        var picker = new MdViewGitHubRepositoryPicker(console,
            new GitHubRepositoryCatalogSource(http, new VerifiedDocumentCache(temp.Root)));
        await Assert.ThrowsAsync<InvalidOperationException>(() => picker.PickAsync("owner"));
    }

    [Fact]
    public async Task CatalogForbiddenOffersManualRepositoryNameInsteadOfCrashing()
    {
        using var temp = new Fixture();
        var requests = 0;
        using var http = Client(_ =>
        {
            requests++;
            return new HttpResponseMessage(HttpStatusCode.Forbidden);
        });
        var console = Interactive();
        console.Input.PushTextWithEnter("Toolbox");

        var picked = await new MdViewGitHubRepositoryPicker(console,
            new GitHubRepositoryCatalogSource(http, new VerifiedDocumentCache(temp.Root)))
            .PickAsync("afrowaveltd");

        Assert.Equal(MdViewSourceKind.GitHub, picked?.Kind);
        Assert.Equal("afrowaveltd", picked?.Value);
        Assert.Equal("Toolbox", picked?.Repository);
        Assert.Contains("HTTP 403", console.Output);
        Assert.Contains("skip the catalog", console.Output);
        Assert.Equal(1, requests); // No retry or README fetch in the picker.
    }

    [Fact]
    public async Task CatalogNotFoundOffersBackWithoutAnyReadmeFetch()
    {
        using var temp = new Fixture();
        var requests = 0;
        using var http = Client(_ =>
        {
            requests++;
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });
        var console = Interactive();
        console.Input.PushKey(ConsoleKey.Enter); // Empty manual name = back.
        var picker = new MdViewGitHubRepositoryPicker(console,
            new GitHubRepositoryCatalogSource(http, new VerifiedDocumentCache(temp.Root)));

        var picked = await picker.PickAsync("missing");

        Assert.Null(picked);
        Assert.Contains("HTTP 404", console.Output);
        Assert.Equal(1, requests);
    }

    [Fact]
    public async Task ManualRepositoryNameRejectsPathTraversalBeforeSelection()
    {
        using var temp = new Fixture();
        using var http = Client(_ => new HttpResponseMessage(HttpStatusCode.Forbidden));
        var console = Interactive();
        console.Input.PushTextWithEnter("../outside");
        console.Input.PushTextWithEnter("Valid.Repo");

        var picked = await new MdViewGitHubRepositoryPicker(console,
            new GitHubRepositoryCatalogSource(http, new VerifiedDocumentCache(temp.Root)))
            .PickAsync("owner");

        Assert.Equal("Valid.Repo", picked?.Repository);
        Assert.Contains("Invalid repository name", console.Output);
    }

    [Fact]
    public async Task EmptyVerifiedCatalogAllowsDirectRepositoryName()
    {
        using var temp = new Fixture();
        using var http = Client(request => request.RequestUri!.AbsolutePath == "/users/owner"
            ? Ok("""{"type":"User"}""")
            : Ok("[]"));
        var console = Interactive();
        console.Input.PushTextWithEnter("KnownRepo");

        var picked = await new MdViewGitHubRepositoryPicker(console,
            new GitHubRepositoryCatalogSource(http, new VerifiedDocumentCache(temp.Root)))
            .PickAsync("owner");

        Assert.Equal("KnownRepo", picked?.Repository);
        Assert.Contains("No public repositories were found", console.Output);
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

    private static HttpResponseMessage Ok(string json) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(json, Encoding.UTF8, "application/json")
    };

    private sealed class Handler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken) => Task.FromResult(responder(request));
    }

    private sealed class Fixture : IDisposable
    {
        public string Root { get; } =
            Path.Combine(Path.GetTempPath(), "mdview-repo-picker-" + Guid.NewGuid().ToString("N"));

        public void Dispose()
        {
            if (Directory.Exists(Root)) Directory.Delete(Root, recursive: true);
        }
    }
}
