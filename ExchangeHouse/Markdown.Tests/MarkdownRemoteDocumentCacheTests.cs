using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Afrowave.Toolbox.ExchangeHouse.Markdown;
using Xunit;

namespace Afrowave.Toolbox.ExchangeHouse.Markdown.Tests;

public sealed class MarkdownRemoteDocumentCacheTests
{
    private static readonly Uri DocumentUrl =
        new("https://raw.githubusercontent.com/afrowaveltd/Toolbox/master/README.md");

    [Fact]
    public async Task First_open_saves_binary_exact_copy_and_offline_open_survives_new_instance()
    {
        var root = NewDirectory();
        try
        {
            var expected = Encoding.UTF8.GetBytes("# Přehled 🐈\n\n- value\r\n");
            var calls = 0;
            using var http = Client(request =>
            {
                calls++;
                Assert.Null(request.Headers.IfModifiedSince);
                Assert.Empty(request.Headers.IfNoneMatch);
                return Ok(expected, "\"v1\"");
            });
            var first = await new MarkdownRemoteDocumentCache(root).GetAsync(
                http, DocumentUrl);
            Assert.False(first.FromCache);
            Assert.False(first.IsStale);
            Assert.Equal("\"v1\"", first.ETag);
            Assert.Equal(expected.LongLength, first.ByteCount);
            Assert.Equal(expected, await File.ReadAllBytesAsync(first.LocalPath));
            Assert.Equal(1, calls);

            var reopened = await new MarkdownRemoteDocumentCache(root).GetAsync(
                http, DocumentUrl, MarkdownRemoteCacheMode.OfflineOnly);
            Assert.True(reopened.FromCache);
            Assert.True(reopened.IsStale);
            Assert.Equal(first.LocalPath, reopened.LocalPath);
            Assert.Equal(1, calls);

            await using var indexed = await MarkdownDiskPageIndex.BuildAsync(
                reopened.LocalPath, tempDirectory: root);
            Assert.True(indexed.PageCount > 0);
            Assert.Equal(expected.LongLength, new FileInfo(reopened.LocalPath).Length);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public async Task Online_revalidation_sends_etag_and_last_modified_and_reuses_304()
    {
        var root = NewDirectory();
        try
        {
            var modified = new DateTimeOffset(2026, 10, 1, 10, 30, 0,
                TimeSpan.Zero);
            var hits = 0;
            using var http = Client(request =>
            {
                hits++;
                if (hits == 1)
                {
                    Assert.Empty(request.Headers.IfNoneMatch);
                    var message = Ok(Encoding.UTF8.GetBytes("cached\n"), "\"a\"");
                    message.Content.Headers.LastModified = modified;
                    return message;
                }
                Assert.Contains(request.Headers.IfNoneMatch,
                    tag => tag.ToString() == "\"a\"");
                Assert.Equal(modified, request.Headers.IfModifiedSince);
                return new HttpResponseMessage(HttpStatusCode.NotModified);
            });
            var cache = new MarkdownRemoteDocumentCache(root);
            var original = await cache.GetAsync(http, DocumentUrl);
            var second = await cache.GetAsync(http, DocumentUrl);
            Assert.Equal(2, hits);
            Assert.True(second.FromCache);
            Assert.False(second.IsStale);
            Assert.Equal(original.LocalPath, second.LocalPath);
            Assert.Equal("cached\n", await File.ReadAllTextAsync(second.LocalPath));
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public async Task Refresh_replaces_manifest_atomically_and_keeps_prior_version_readable()
    {
        var root = NewDirectory();
        try
        {
            var count = 0;
            using var http = Client(request =>
            {
                count++;
                Assert.Empty(request.Headers.IfNoneMatch);
                return Ok(Encoding.UTF8.GetBytes("version " + count + "\n"),
                    "\"v" + count + "\"");
            });
            var cache = new MarkdownRemoteDocumentCache(root);
            var first = await cache.GetAsync(http, DocumentUrl);
            var second = await cache.GetAsync(http, DocumentUrl,
                MarkdownRemoteCacheMode.ForceRefresh);
            var latest = await cache.GetAsync(http, DocumentUrl,
                MarkdownRemoteCacheMode.OfflineOnly);

            Assert.Equal(2, count);
            Assert.NotEqual(first.LocalPath, second.LocalPath);
            Assert.Equal(second.LocalPath, latest.LocalPath);
            Assert.Equal("version 1\n", await File.ReadAllTextAsync(first.LocalPath));
            Assert.Equal("version 2\n", await File.ReadAllTextAsync(second.LocalPath));
            Assert.Equal(3, await cache.RemoveAsync(DocumentUrl));
            Assert.False(File.Exists(first.LocalPath));
            Assert.False(File.Exists(second.LocalPath));
            await Assert.ThrowsAsync<FileNotFoundException>(() => cache.GetAsync(
                http, DocumentUrl, MarkdownRemoteCacheMode.OfflineOnly));
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public async Task Offline_failures_and_server_errors_use_prior_copy_but_404_does_not()
    {
        var root = NewDirectory();
        try
        {
            using var success = Client(_ => Ok("ready\n"u8.ToArray(), "\"ready\""));
            var cache = new MarkdownRemoteDocumentCache(root);
            var original = await cache.GetAsync(success, DocumentUrl);

            using var offline = Client(_ => throw new HttpRequestException(
                "Network unavailable."));
            var stale = await cache.GetAsync(offline, DocumentUrl);
            Assert.True(stale.FromCache);
            Assert.True(stale.IsStale);
            Assert.Equal(original.LocalPath, stale.LocalPath);

            using var serverFailure = Client(_ =>
                new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
            Assert.True((await cache.GetAsync(serverFailure, DocumentUrl)).IsStale);

            using var forbidden = Client(_ =>
                new HttpResponseMessage(HttpStatusCode.Forbidden));
            await Assert.ThrowsAsync<HttpRequestException>(() =>
                cache.GetAsync(forbidden, DocumentUrl));
            Assert.Equal(original.LocalPath,
                (await cache.GetAsync(forbidden, DocumentUrl,
                    MarkdownRemoteCacheMode.OfflineOnly)).LocalPath);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public async Task Missing_offline_document_never_attempts_a_network_call()
    {
        var root = NewDirectory();
        try
        {
            var called = false;
            using var http = Client(_ =>
            {
                called = true;
                return Ok("unexpected"u8.ToArray(), "\"x\"");
            });
            var cache = new MarkdownRemoteDocumentCache(root);
            await Assert.ThrowsAsync<FileNotFoundException>(() =>
                cache.GetAsync(http, DocumentUrl, MarkdownRemoteCacheMode.OfflineOnly));
            Assert.False(called);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public async Task Too_large_refresh_does_not_damage_the_last_good_cached_copy()
    {
        var root = NewDirectory();
        try
        {
            using var tiny = Client(_ => Ok("good\n"u8.ToArray(), "\"good\""));
            var cache = new MarkdownRemoteDocumentCache(root);
            var original = await cache.GetAsync(tiny, DocumentUrl);

            using var big = Client(_ => Ok(new byte[2048], "\"big\""));
            await Assert.ThrowsAsync<InvalidDataException>(() =>
                cache.GetAsync(big, DocumentUrl, MarkdownRemoteCacheMode.ForceRefresh,
                    maxBytes: 1024));

            var saved = await cache.GetAsync(tiny, DocumentUrl,
                MarkdownRemoteCacheMode.OfflineOnly);
            Assert.Equal(original.LocalPath, saved.LocalPath);
            Assert.Equal("good\n", await File.ReadAllTextAsync(saved.LocalPath));
            Assert.DoesNotContain(Directory.GetFiles(root),
                p => p.EndsWith(".part", StringComparison.Ordinal));
            Assert.Equal(2, Directory.GetFiles(root).Length);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public async Task Saved_copy_is_independent_and_does_not_overwrite_without_permission()
    {
        var root = NewDirectory();
        try
        {
            using var http = Client(_ => Ok("keep\n"u8.ToArray(), "\"v\""));
            var cache = new MarkdownRemoteDocumentCache(Path.Combine(root, "cache"));
            var document = await cache.GetAsync(http, DocumentUrl);
            var saved = Path.Combine(root, "permanent.md");
            await MarkdownRemoteDocumentCache.SaveCopyAsync(document, saved);
            Assert.Equal("keep\n", await File.ReadAllTextAsync(saved));
            await Assert.ThrowsAsync<IOException>(() =>
                MarkdownRemoteDocumentCache.SaveCopyAsync(document, saved));

            await cache.RemoveAsync(DocumentUrl);
            Assert.True(File.Exists(saved));
            Assert.Equal("keep\n", await File.ReadAllTextAsync(saved));
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public async Task Refuses_insecure_urls_and_corrupt_manifest_does_not_escape_cache_root()
    {
        var root = NewDirectory();
        try
        {
            using var http = Client(_ => Ok("safe\n"u8.ToArray(), "\"safe\""));
            var cache = new MarkdownRemoteDocumentCache(root);
            await Assert.ThrowsAsync<ArgumentException>(() =>
                cache.GetAsync(http, new Uri("http://example.org/not-https")));
            await Assert.ThrowsAsync<ArgumentException>(() =>
                cache.GetAsync(http, new Uri("https://user:password@example.org/a.md")));

            _ = await cache.GetAsync(http, DocumentUrl);
            var manifest = Assert.Single(Directory.GetFiles(root, "*.json"));
            await File.WriteAllTextAsync(manifest,
                "{\"Version\":1,\"FileName\":\"../../outside.md\",\"ByteCount\":5}");
            await Assert.ThrowsAsync<FileNotFoundException>(() =>
                cache.GetAsync(http, DocumentUrl, MarkdownRemoteCacheMode.OfflineOnly));
            Assert.True(File.Exists(manifest));
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    private static HttpResponseMessage Ok(byte[] bytes, string tag)
    {
        var message = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(bytes)
        };
        message.Headers.ETag = new EntityTagHeaderValue(tag);
        return message;
    }

    private static HttpClient Client(Func<HttpRequestMessage, HttpResponseMessage> handler) =>
        new(new StubHandler(handler));

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var response = handler(request);
            response.RequestMessage = request;
            return Task.FromResult(response);
        }
    }

    private static string NewDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(),
            "afrowave-markdown-persistent-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
