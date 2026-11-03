using System.Text;
using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.Cache;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.Cache.Tests;

public sealed class VerifiedDocumentCacheTests
{
    [Fact]
    public async Task PersistsAcrossInstances()
    {
        using var fixture = new CacheFixture();
        const string text = "# README\nHello 🌍\n";
        var stored = await fixture.Cache.PutAsync("github:afrowaveltd/toolbox", Bytes(text));
        var reopened = new VerifiedDocumentCache(fixture.Root);
        var found = await reopened.GetAsync("github:afrowaveltd/toolbox");
        Assert.NotNull(found);
        Assert.Equal(stored.Sha256, found.Sha256);
        Assert.Equal(text, await File.ReadAllTextAsync(found.ContentPath));
        Assert.Null(await reopened.GetAsync("github:other/repo"));
    }

    [Fact]
    public async Task RejectedUnsafeReplacementKeepsOldEntry()
    {
        using var fixture = new CacheFixture();
        var before = await fixture.Cache.PutAsync("source", Bytes("good document"));
        await Assert.ThrowsAsync<InvalidDataException>(() =>
            fixture.Cache.PutAsync("source", Bytes("bad\u001b[31m")));
        var after = await fixture.Cache.GetAsync("source");
        Assert.Equal(before.Sha256, after?.Sha256);
    }

    [Fact]
    public async Task RejectedInvalidUtf8KeepsOldEntry()
    {
        using var fixture = new CacheFixture();
        var before = await fixture.Cache.PutAsync("source", Bytes("valid"));
        await Assert.ThrowsAsync<DecoderFallbackException>(() =>
            fixture.Cache.PutAsync("source", new MemoryStream(new byte[] { 0x61, 0xc3, 0x28 })));
        Assert.Equal(before.Sha256, (await fixture.Cache.GetAsync("source"))?.Sha256);
    }

    [Fact]
    public async Task RejectsTamperedBlob()
    {
        using var fixture = new CacheFixture();
        var entry = await fixture.Cache.PutAsync("source", Bytes("hello"));
        await File.WriteAllTextAsync(entry.ContentPath, "other");
        Assert.Null(await fixture.Cache.GetAsync("source"));
    }

    [Fact]
    public async Task EnforcesLimitWithoutPublishingPartialData()
    {
        using var fixture = new CacheFixture(4);
        await Assert.ThrowsAsync<InvalidDataException>(() =>
            fixture.Cache.PutAsync("source", Bytes("hello")));
        Assert.Null(await fixture.Cache.GetAsync("source"));
    }

    [Fact]
    public async Task InterruptedWriteRetainsPreviousDocument()
    {
        using var fixture = new CacheFixture();
        var before = await fixture.Cache.PutAsync("source", Bytes("original"));
        using var tokenSource = new CancellationTokenSource();
        tokenSource.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await fixture.Cache.PutAsync("source", Bytes("replacement"), tokenSource.Token));
        Assert.Equal(before.Sha256, (await fixture.Cache.GetAsync("source"))?.Sha256);
    }

    private static MemoryStream Bytes(string text) => new(Encoding.UTF8.GetBytes(text));

    private sealed class CacheFixture : IDisposable
    {
        public string Root { get; } = Path.Combine(Path.GetTempPath(), "afrowave-cache-tests-" + Guid.NewGuid().ToString("N"));
        public VerifiedDocumentCache Cache { get; }
        public CacheFixture(long maximumBytes = 1024 * 1024) =>
            Cache = new VerifiedDocumentCache(Root, maximumBytes);
        public void Dispose()
        {
            if (Directory.Exists(Root)) Directory.Delete(Root, recursive: true);
        }
    }
}
