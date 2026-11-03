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

    [Fact]
    public async Task Cache_quota_rejects_second_document_without_losing_offline_copy()
    {
        var root = NewDirectory();
        try
        {
            var second = new Uri("https://example.org/docs/second.md");
            var cache = new MarkdownRemoteDocumentCache(root,
                maxTotalCacheBytes: 20 * 1024);
            using var firstClient = Client(_ => Ok("kept offline\n"u8.ToArray(), "\"one\""));
            var original = await cache.GetAsync(firstClient, DocumentUrl);
            var before = await cache.GetUsageAsync();
            Assert.Equal(2, before.FileCount);
            Assert.True(before.UsedBytes < before.MaxTotalBytes);
            Assert.Equal(20 * 1024, before.MaxTotalBytes);
            Assert.Equal(before.MaxTotalBytes - before.UsedBytes,
                before.RemainingBytes);

            using var nextClient = Client(_ => Ok(new byte[8 * 1024], "\"large\""));
            await Assert.ThrowsAsync<InvalidDataException>(() =>
                cache.GetAsync(nextClient, second));

            var after = await cache.GetUsageAsync();
            Assert.Equal(before, after);
            Assert.DoesNotContain(Directory.GetFiles(root),
                p => p.EndsWith(".part", StringComparison.Ordinal));
            var offline = await cache.GetAsync(firstClient, DocumentUrl,
                MarkdownRemoteCacheMode.OfflineOnly);
            Assert.Equal(original.LocalPath, offline.LocalPath);
            Assert.Equal("kept offline\n", await File.ReadAllTextAsync(offline.LocalPath));
            await Assert.ThrowsAsync<FileNotFoundException>(() => cache.GetAsync(
                firstClient, second, MarkdownRemoteCacheMode.OfflineOnly));
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public async Task Unknown_content_length_stream_stops_at_total_quota_and_keeps_saved_document()
    {
        var root = NewDirectory();
        try
        {
            var cache = new MarkdownRemoteDocumentCache(root,
                maxTotalCacheBytes: 24 * 1024);
            using var first = Client(_ => Ok("offline copy\n"u8.ToArray(), "\"safe\""));
            var saved = await cache.GetAsync(first, DocumentUrl);
            var otherUrl = new Uri("https://example.org/unknown-length.md");
            using var unknown = Client(_ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new UnspecifiedLengthContent(new byte[32 * 1024])
            });

            await Assert.ThrowsAsync<InvalidDataException>(() =>
                cache.GetAsync(unknown, otherUrl));
            Assert.Equal("offline copy\n", await File.ReadAllTextAsync(saved.LocalPath));
            Assert.Equal(saved.LocalPath, (await cache.GetAsync(first,
                DocumentUrl, MarkdownRemoteCacheMode.OfflineOnly)).LocalPath);
            Assert.Equal(2, (await cache.GetUsageAsync()).FileCount);
            Assert.DoesNotContain(Directory.GetFiles(root),
                p => p.EndsWith(".part", StringComparison.Ordinal));
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public async Task Explicit_pruning_removes_old_versions_only_and_preserves_other_urls()
    {
        var root = NewDirectory();
        try
        {
            var url2 = new Uri("https://example.org/guide-two.md");
            var counter = 0;
            using var http = Client(_ => Ok(
                Encoding.UTF8.GetBytes("generation " + ++counter + "\n"),
                "\"v" + counter + "\""));
            var cache = new MarkdownRemoteDocumentCache(root);
            var older = await cache.GetAsync(http, DocumentUrl);
            var newer = await cache.GetAsync(http, DocumentUrl,
                MarkdownRemoteCacheMode.ForceRefresh);
            var newest = await cache.GetAsync(http, DocumentUrl,
                MarkdownRemoteCacheMode.ForceRefresh);
            var other = await cache.GetAsync(http, url2);
            var before = await cache.GetUsageAsync();
            Assert.Equal(6, before.FileCount); // two manifests, four .md copies
            Assert.True(File.Exists(older.LocalPath));
            Assert.True(File.Exists(newer.LocalPath));

            // Default 24h protection avoids pruning a just-opened document.
            Assert.Equal(0, await cache.PruneObsoleteVersionsAsync());
            Assert.Equal(2, await cache.PruneObsoleteVersionsAsync(TimeSpan.Zero));
            var after = await cache.GetUsageAsync();
            Assert.Equal(before.FileCount - 2, after.FileCount);
            Assert.True(after.UsedBytes < before.UsedBytes);
            Assert.False(File.Exists(older.LocalPath));
            Assert.False(File.Exists(newer.LocalPath));
            Assert.True(File.Exists(newest.LocalPath));
            Assert.True(File.Exists(other.LocalPath));

            Assert.Equal(newest.LocalPath, (await cache.GetAsync(http,
                DocumentUrl, MarkdownRemoteCacheMode.OfflineOnly)).LocalPath);
            Assert.Equal(other.LocalPath, (await cache.GetAsync(http,
                url2, MarkdownRemoteCacheMode.OfflineOnly)).LocalPath);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public async Task Prune_never_guesses_when_manifest_is_missing_or_corrupt()
    {
        var root = NewDirectory();
        try
        {
            using var http = Client(_ => Ok("reusable\n"u8.ToArray(), "\"v\""));
            var cache = new MarkdownRemoteDocumentCache(root);
            var a = await cache.GetAsync(http, DocumentUrl);
            var b = await cache.GetAsync(http, DocumentUrl,
                MarkdownRemoteCacheMode.ForceRefresh);
            var manifest = Assert.Single(Directory.GetFiles(root, "*.json"));
            await File.WriteAllTextAsync(manifest, "{invalid json");
            Assert.Equal(0, await cache.PruneObsoleteVersionsAsync(TimeSpan.Zero));
            Assert.True(File.Exists(a.LocalPath));
            Assert.True(File.Exists(b.LocalPath));
            Assert.Equal(3, (await cache.GetUsageAsync()).FileCount);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public async Task Quota_counts_only_owned_cache_files_without_a_body_catalog()
    {
        var root = NewDirectory();
        try
        {
            var cache = new MarkdownRemoteDocumentCache(root);
            var empty = await cache.GetUsageAsync();
            Assert.Equal(0, empty.FileCount);
            Assert.False(Directory.Exists(Path.Combine(root, "missing")));
            await File.WriteAllBytesAsync(Path.Combine(root, "unrelated.txt"), new byte[500]);
            using var client = Client(_ => Ok("hi\n"u8.ToArray(), "\"v\""));
            var copy = await cache.GetAsync(client, DocumentUrl);
            var usage = await cache.GetUsageAsync();
            Assert.Equal(2, usage.FileCount);
            Assert.True(usage.UsedBytes > copy.ByteCount);
            Assert.True(usage.UsedBytes < 500);
            Assert.Equal(0, await cache.PruneObsoleteVersionsAsync(TimeSpan.Zero));

            await cache.RemoveAsync(DocumentUrl);
            Assert.Equal(0, (await cache.GetUsageAsync()).UsedBytes);
            Assert.True(File.Exists(Path.Combine(root, "unrelated.txt")));
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public async Task Small_or_invalid_total_quota_and_negative_retention_age_are_rejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new MarkdownRemoteDocumentCache(maxTotalCacheBytes: 100));
        var root = NewDirectory();
        try
        {
            var cache = new MarkdownRemoteDocumentCache(root);
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                cache.PruneObsoleteVersionsAsync(TimeSpan.FromSeconds(-1)));
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public async Task Oversized_manifest_is_not_loaded_as_valid_cache_metadata()
    {
        var root = NewDirectory();
        try
        {
            var cache = new MarkdownRemoteDocumentCache(root);
            using var http = Client(_ => Ok("persisted\n"u8.ToArray(), "\"v\""));
            var saved = await cache.GetAsync(http, DocumentUrl);
            var manifest = Assert.Single(Directory.GetFiles(root, "*.json"));
            await File.WriteAllTextAsync(manifest, new string('X', 128 * 1024));
            await Assert.ThrowsAsync<FileNotFoundException>(() =>
                cache.GetAsync(http, DocumentUrl, MarkdownRemoteCacheMode.OfflineOnly));
            Assert.True(File.Exists(saved.LocalPath));
            Assert.Equal(0, await cache.PruneObsoleteVersionsAsync(TimeSpan.Zero));
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public async Task Oversized_http_etag_cannot_publish_an_unusable_manifest()
    {
        var root = NewDirectory();
        try
        {
            var cache = new MarkdownRemoteDocumentCache(root);
            using var original = Client(_ => Ok("safe\n"u8.ToArray(), "\"v\""));
            var existing = await cache.GetAsync(original, DocumentUrl);
            var before = await cache.GetUsageAsync();

            var veryLongTag = "\"" + new string('x', 20 * 1024) + "\"";
            using var bad = Client(_ => Ok("new\n"u8.ToArray(), veryLongTag));
            await Assert.ThrowsAsync<InvalidDataException>(() =>
                cache.GetAsync(bad, DocumentUrl, MarkdownRemoteCacheMode.ForceRefresh));

            Assert.Equal(before, await cache.GetUsageAsync());
            Assert.Equal(existing.LocalPath, (await cache.GetAsync(original,
                DocumentUrl, MarkdownRemoteCacheMode.OfflineOnly)).LocalPath);
            Assert.Equal("safe\n", await File.ReadAllTextAsync(existing.LocalPath));
            Assert.DoesNotContain(Directory.GetFiles(root),
                file => file.EndsWith(".part", StringComparison.Ordinal));
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public async Task Document_catalog_lists_only_valid_cached_files_with_bounded_page_size()
    {
        var root = NewDirectory();
        try
        {
            var cache = new MarkdownRemoteDocumentCache(root);
            using var client = Client(_ => Ok("doc\n"u8.ToArray(), "\"v\""));
            var firstUrl = DocumentUrl;
            var secondUrl = new Uri("https://example.org/second.md");
            var first = await cache.GetAsync(client, firstUrl);
            var second = await cache.GetAsync(client, secondUrl);
            await File.WriteAllTextAsync(Path.Combine(root, "unrelated.json"), "{}");

            var all = await cache.ListPageAsync(pageSize: 4);
            Assert.Equal(2, all.Entries.Count);
            Assert.False(all.HasMore);
            Assert.Contains(all.Entries, x => x.Source == firstUrl &&
                x.LocalPath == first.LocalPath && x.ByteCount == first.ByteCount);
            Assert.Contains(all.Entries, x => x.Source == secondUrl &&
                x.LocalPath == second.LocalPath);
            Assert.All(all.Entries, x => Assert.Equal(64, x.CacheKey.Length));

            var p1 = await cache.ListPageAsync(pageSize: 1);
            var p2 = await cache.ListPageAsync(skip: 1, pageSize: 1);
            var end = await cache.ListPageAsync(skip: 2, pageSize: 1);
            Assert.Single(p1.Entries);
            Assert.Single(p2.Entries);
            Assert.True(p1.HasMore);
            Assert.False(p2.HasMore);
            Assert.Empty(end.Entries);
            Assert.NotEqual(p1.Entries[0].CacheKey, p2.Entries[0].CacheKey);
            Assert.Equal(2, (await cache.ListPageAsync(pageSize: 64)).Entries.Count);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public async Task Legacy_manifest_remains_openable_even_without_stored_source_url()
    {
        var root = NewDirectory();
        try
        {
            var cache = new MarkdownRemoteDocumentCache(root);
            using var client = Client(_ => Ok("legacy\n"u8.ToArray(), "\"tag\""));
            var stored = await cache.GetAsync(client, DocumentUrl);
            var manifestPath = Assert.Single(Directory.GetFiles(root, "*.json"));
            var parsed = System.Text.Json.Nodes.JsonNode.Parse(
                await File.ReadAllTextAsync(manifestPath))!.AsObject();
            parsed.Remove("SourceUrl");
            await File.WriteAllTextAsync(manifestPath, parsed.ToJsonString());

            var page = await new MarkdownRemoteDocumentCache(root).ListPageAsync();
            var legacy = Assert.Single(page.Entries);
            Assert.Null(legacy.Source);
            Assert.Equal(stored.LocalPath, legacy.LocalPath);
            Assert.Equal("legacy\n", await File.ReadAllTextAsync(legacy.LocalPath));
            var export = Path.Combine(root, "Exported-Legacy.md");
            await MarkdownRemoteDocumentCache.SaveCopyAsync(legacy, export);
            Assert.Equal("legacy\n", await File.ReadAllTextAsync(export));
            Assert.Equal(stored.LocalPath,
                (await cache.GetAsync(client, DocumentUrl,
                    MarkdownRemoteCacheMode.OfflineOnly)).LocalPath);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public async Task Falsified_url_in_cache_metadata_is_never_shown_as_a_trusted_source()
    {
        var root = NewDirectory();
        try
        {
            var cache = new MarkdownRemoteDocumentCache(root);
            using var client = Client(_ => Ok("safe\n"u8.ToArray(), "\"v\""));
            _ = await cache.GetAsync(client, DocumentUrl);
            var manifestPath = Assert.Single(Directory.GetFiles(root, "*.json"));
            var parsed = System.Text.Json.Nodes.JsonNode.Parse(
                await File.ReadAllTextAsync(manifestPath))!.AsObject();
            parsed["SourceUrl"] = "https://evil.example/docs.md";
            await File.WriteAllTextAsync(manifestPath, parsed.ToJsonString());

            var page = await cache.ListPageAsync();
            Assert.Null(Assert.Single(page.Entries).Source);
            Assert.Single(page.Entries);
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                cache.ListPageAsync(pageSize: 65));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                cache.ListPageAsync(skip: -1));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                cache.ListPageAsync(pageSize: 0));
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    private sealed class UnspecifiedLengthContent(byte[] payload) : HttpContent
    {
        protected override Task SerializeToStreamAsync(
            Stream stream, TransportContext? context) =>
            stream.WriteAsync(payload, 0, payload.Length);

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }

        protected override Task<Stream> CreateContentReadStreamAsync() =>
            Task.FromResult<Stream>(new MemoryStream(payload, writable: false));
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
