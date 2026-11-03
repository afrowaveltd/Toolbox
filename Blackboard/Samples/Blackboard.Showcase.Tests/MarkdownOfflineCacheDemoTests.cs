using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Afrowave.Toolbox.Blackboard.Showcase;
using Afrowave.Toolbox.ExchangeHouse.Markdown;
using Spectre.Console.Testing;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.Showcase.Tests;

public sealed class MarkdownOfflineCacheDemoTests
{
    private static readonly Uri Sample =
        new("https://example.org/Toolbox/README.md");

    [Fact]
    public async Task Empty_offline_catalog_has_usable_menu_and_no_network_dependency()
    {
        var root = NewRoot();
        try
        {
            var cache = new MarkdownRemoteDocumentCache(Path.Combine(root, "cache"));
            var console = CreateConsole();
            console.Input.PushKey(ConsoleKey.Enter); // Browse empty saved documents
            console.Input.PushKey(ConsoleKey.Escape); // Back from main menu

            await new MarkdownOfflineCacheDemo(cache).RunAsync(
                console, root, CancellationToken.None);

            Assert.Contains("Saved Markdown documents (offline)", console.Output);
            Assert.Contains("No saved Markdown documents found", console.Output);
            Assert.False(Directory.Exists(cache.DirectoryPath));
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public async Task Offline_entry_opens_through_existing_bounded_page_viewer()
    {
        var root = NewRoot();
        try
        {
            var cache = new MarkdownRemoteDocumentCache(Path.Combine(root, "cache"));
            var body = "# Cached README\n\n- Offline works!\n";
            using var client = CreateClient(() => Ok(body));
            var saved = await cache.GetAsync(client, Sample);
            var console = CreateConsole();
            console.Input.PushKey(ConsoleKey.Enter); // browse
            console.Input.PushKey(ConsoleKey.Enter); // first saved document
            console.Input.PushKey(ConsoleKey.Enter); // open saved copy
            console.Input.PushKey(ConsoleKey.Enter); // Balanced paging profile
            console.Input.PushKey(ConsoleKey.Escape); // close pager
            console.Input.PushKey(ConsoleKey.Escape); // return to catalog
            console.Input.PushKey(ConsoleKey.Escape); // return to menu
            console.Input.PushKey(ConsoleKey.Escape); // exit

            await new MarkdownOfflineCacheDemo(cache).RunAsync(
                console, root, CancellationToken.None);

            Assert.Contains("README.md", console.Output);
            Assert.Contains("Markdown source: README.md", console.Output);
            Assert.Contains("Offline works!", console.Output);
            Assert.True(File.Exists(saved.LocalPath));
            Assert.Equal(body, await File.ReadAllTextAsync(saved.LocalPath));
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public async Task User_can_remove_one_saved_document_only_after_explicit_confirmation()
    {
        var root = NewRoot();
        try
        {
            var cache = new MarkdownRemoteDocumentCache(Path.Combine(root, "cache"));
            using var client = CreateClient(() => Ok("# Keep me unless confirmed\n"));
            var stored = await cache.GetAsync(client, Sample);
            var console = CreateConsole();
            console.Input.PushKey(ConsoleKey.Enter);     // browse
            console.Input.PushKey(ConsoleKey.Enter);     // saved document
            console.Input.PushKey(ConsoleKey.DownArrow); // export
            console.Input.PushKey(ConsoleKey.DownArrow); // remove
            console.Input.PushKey(ConsoleKey.Enter);
            console.Input.PushKey(ConsoleKey.DownArrow); // confirm remove
            console.Input.PushKey(ConsoleKey.Enter);
            console.Input.PushKey(ConsoleKey.Escape);    // back to main

            await new MarkdownOfflineCacheDemo(cache).RunAsync(
                console, root, CancellationToken.None);

            Assert.Contains("Removed 2 cache files", console.Output);
            Assert.False(File.Exists(stored.LocalPath));
            Assert.Empty((await cache.ListPageAsync()).Entries);
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public async Task Prune_menu_preserves_current_copy_and_only_removes_aged_versions()
    {
        var root = NewRoot();
        try
        {
            var cache = new MarkdownRemoteDocumentCache(Path.Combine(root, "cache"));
            var count = 0;
            using var client = CreateClient(() => Ok("version " + ++count + "\n"));
            var old = await cache.GetAsync(client, Sample);
            var current = await cache.GetAsync(client, Sample,
                MarkdownRemoteCacheMode.ForceRefresh);
            File.SetLastWriteTimeUtc(old.LocalPath, DateTime.UtcNow.AddDays(-3));

            var console = CreateConsole();
            console.Input.PushKey(ConsoleKey.DownArrow); // Prune
            console.Input.PushKey(ConsoleKey.Enter);
            console.Input.PushKey(ConsoleKey.DownArrow); // Confirm
            console.Input.PushKey(ConsoleKey.Enter);
            console.Input.PushKey(ConsoleKey.Escape); // leave main

            await new MarkdownOfflineCacheDemo(cache).RunAsync(
                console, root, CancellationToken.None);

            Assert.Contains("Pruned 1 obsolete saved versions", console.Output);
            Assert.False(File.Exists(old.LocalPath));
            Assert.True(File.Exists(current.LocalPath));
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public async Task Catalog_navigation_does_not_materialize_all_documents_in_one_menu()
    {
        var root = NewRoot();
        try
        {
            var cache = new MarkdownRemoteDocumentCache(Path.Combine(root, "cache"));
            using var client = CreateClient(() => Ok("# Offline\n"));
            for (var i = 0; i < 9; i++)
                _ = await cache.GetAsync(client,
                    new Uri($"https://example.org/readme-{i:00}.md"));
            var console = CreateConsole();
            console.Input.PushKey(ConsoleKey.Enter); // browse
            for (var i = 0; i < 8; i++)
                console.Input.PushKey(ConsoleKey.DownArrow); // next saved page
            console.Input.PushKey(ConsoleKey.Enter);
            console.Input.PushKey(ConsoleKey.Escape); // leave browser
            console.Input.PushKey(ConsoleKey.Escape); // leave menu

            await new MarkdownOfflineCacheDemo(cache).RunAsync(
                console, root, CancellationToken.None);

            Assert.Contains("Offline documents 1–8", console.Output);
            Assert.Contains("Offline documents 9–9", console.Output);
            Assert.Equal(9, (await cache.ListPageAsync(pageSize: 16)).Entries.Count);
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public async Task Signed_url_query_values_are_not_printed_in_offline_menu()
    {
        var root = NewRoot();
        try
        {
            var cache = new MarkdownRemoteDocumentCache(Path.Combine(root, "cache"));
            using var client = CreateClient(() => Ok("# Document\n"));
            _ = await cache.GetAsync(client,
                new Uri("https://example.org/private.md?token=SECRET123"));
            var console = CreateConsole();
            console.Input.PushKey(ConsoleKey.Enter); // browse
            console.Input.PushKey(ConsoleKey.Escape); // return to main
            console.Input.PushKey(ConsoleKey.Escape); // exit

            await new MarkdownOfflineCacheDemo(cache).RunAsync(
                console, root, CancellationToken.None);

            Assert.Contains("private.md", console.Output);
            Assert.DoesNotContain("SECRET123", console.Output);
        }
        finally { Directory.Delete(root, true); }
    }

    private static string NewRoot()
    {
        var path = Path.Combine(Path.GetTempPath(),
            "afrowave-offline-menu-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static TestConsole CreateConsole()
    {
        var console = new TestConsole();
        console.Profile.Capabilities.Interactive = true;
        console.Profile.Capabilities.Unicode = true;
        console.Profile.Width = 160;
        return console;
    }

    private static HttpResponseMessage Ok(string text)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(Encoding.UTF8.GetBytes(text))
        };
        response.Headers.ETag = new EntityTagHeaderValue("\"sample\"");
        return response;
    }

    private static HttpClient CreateClient(Func<HttpResponseMessage> response) =>
        new(new StubHandler(response));

    private sealed class StubHandler(Func<HttpResponseMessage> response) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = response();
            result.RequestMessage = request;
            return Task.FromResult(result);
        }
    }
}
