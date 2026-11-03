using System.Net;
using System.Text;
using Afrowave.Toolbox.Blackboard.Showcase;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.Showcase.Tests;

public sealed class RemoteMarkdownTempDocumentTests
{
    [Fact]
    public async Task Remote_document_is_spooled_unmodified_and_can_be_paged_after_connection_closes()
    {
        var expected = Encoding.UTF8.GetBytes("# Nadpis\n\nPříliš žluťoučký kůň\n\n" +
            new string('x', 200_000));
        var directory = NewTestDirectory();

        try
        {
            using var client = CreateClient(() => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(expected)
            });

            var uri = new Uri("https://example.org/docs/guide.md");
            var document = await RemoteMarkdownTempDocument.DownloadAsync(
                client, uri, tempDirectory: directory);
            var path = document.TempPath;

            Assert.Equal(uri, document.Source);
            Assert.Equal(expected.LongLength, document.ByteCount);
            Assert.True(File.Exists(path));
            Assert.Equal(expected, await File.ReadAllBytesAsync(path));

            // The HTTP response has already been consumed and disposed.
            await using (var pager = document.OpenRead())
            {
                Assert.True(pager.CanSeek);
                pager.Seek(expected.Length - 10, SeekOrigin.Begin);
                var tail = new byte[10];
                Assert.Equal(10, await pager.ReadAsync(tail));
                Assert.All(tail, b => Assert.Equal((byte)'x', b));
            }

            await document.DisposeAsync();
            Assert.False(File.Exists(path));
            Assert.Throws<ObjectDisposedException>(() => document.OpenRead());
            document.Dispose(); // idempotent
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task Response_larger_than_configured_limit_is_rejected_before_copying()
    {
        var directory = NewTestDirectory();
        try
        {
            using var client = CreateClient(() => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(new byte[8192])
            });

            await Assert.ThrowsAsync<InvalidDataException>(() =>
                RemoteMarkdownTempDocument.DownloadAsync(
                    client, new Uri("https://example.org/large.md"),
                    maxBytes: 1024, tempDirectory: directory));

            Assert.Empty(Directory.GetFiles(directory));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task Unknown_content_length_still_enforces_disk_quota_and_removes_partial_file()
    {
        var directory = NewTestDirectory();
        try
        {
            using var client = CreateClient(() =>
            {
                var content = new StreamContent(new MemoryStream(new byte[16_384]));
                // StreamContent with a readable stream may supply Length
                // on serialization; the byte ceiling applies regardless.
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
            });

            await Assert.ThrowsAsync<InvalidDataException>(() =>
                RemoteMarkdownTempDocument.DownloadAsync(
                    client, new Uri("https://example.org/unknown.md"),
                    maxBytes: 1024, tempDirectory: directory));

            Assert.Empty(Directory.GetFiles(directory));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task Error_status_does_not_leave_temp_file()
    {
        var directory = NewTestDirectory();
        try
        {
            using var client = CreateClient(() =>
                new HttpResponseMessage(HttpStatusCode.NotFound));

            await Assert.ThrowsAsync<HttpRequestException>(() =>
                RemoteMarkdownTempDocument.DownloadAsync(
                    client, new Uri("https://example.org/missing.md"),
                    tempDirectory: directory));

            Assert.Empty(Directory.GetFiles(directory));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task Rejects_insecure_urls_and_credentials()
    {
        using var client = CreateClient(() => new HttpResponseMessage(HttpStatusCode.OK));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            RemoteMarkdownTempDocument.DownloadAsync(
                client, new Uri("http://example.org/a.md")));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            RemoteMarkdownTempDocument.DownloadAsync(
                client, new Uri("https://user:pass@example.org/a.md")));
    }

    [Fact]
    public async Task Cancellation_before_request_does_not_create_a_document()
    {
        var directory = NewTestDirectory();
        try
        {
            using var client = CreateClient(() =>
                new HttpResponseMessage(HttpStatusCode.OK));
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                RemoteMarkdownTempDocument.DownloadAsync(
                    client, new Uri("https://example.org/a.md"),
                    cancellationToken: cts.Token, tempDirectory: directory));
            Assert.Empty(Directory.GetFiles(directory));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static string NewTestDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(),
            "afrowave-markdown-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return directory;
    }

    private static HttpClient CreateClient(Func<HttpResponseMessage> response) =>
        new(new StubHandler(response));

    private sealed class StubHandler(Func<HttpResponseMessage> response) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var message = response();
            message.RequestMessage = request;
            return Task.FromResult(message);
        }
    }
}
