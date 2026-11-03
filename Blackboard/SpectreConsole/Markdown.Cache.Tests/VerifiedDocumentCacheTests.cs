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


    [Fact]
    public async Task IndependentInstancesDoNotPublishMixedContent()
    {
        using var fixture = new CacheFixture();
        var first = new VerifiedDocumentCache(fixture.Root);
        var second = new VerifiedDocumentCache(fixture.Root);
        var one = new string('A', 300_000);
        var two = new string('B', 300_000);
        await Task.WhenAll(
            first.PutAsync("same", Bytes(one)),
            second.PutAsync("same", Bytes(two)));
        var found = await fixture.Cache.GetAsync("same");
        Assert.NotNull(found);
        var content = await File.ReadAllTextAsync(found.ContentPath);
        Assert.True(content == one || content == two);
        Assert.Equal(300_000, found.Length);
    }

    [Fact]
    public async Task CorruptedExistingDigestBlobIsRepaired()
    {
        using var fixture = new CacheFixture();
        var original = await fixture.Cache.PutAsync("source", Bytes("healthy"));
        await File.WriteAllTextAsync(original.ContentPath, "damaged");
        Assert.Null(await fixture.Cache.GetAsync("source"));
        await fixture.Cache.PutAsync("source", Bytes("healthy"));
        Assert.Equal("healthy", await File.ReadAllTextAsync(
            (await fixture.Cache.GetAsync("source"))!.ContentPath));
    }

    [Fact]
    public async Task RejectsSymlinkedCacheSubdirectory()
    {
        using var fixture = new CacheFixture();
        if (!OperatingSystem.IsLinux()) return;
        var external = Path.Combine(Path.GetTempPath(), "afrowave-external-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(external);
        Directory.CreateDirectory(fixture.Root);
        try
        {
            Directory.CreateSymbolicLink(Path.Combine(fixture.Root, "blobs"), external);
            await Assert.ThrowsAsync<IOException>(() => fixture.Cache.PutAsync("source", Bytes("text")));
        }
        finally
        {
            // Do not recursively delete through a symlink in the test fixture.
            Directory.Delete(Path.Combine(fixture.Root, "blobs"));
            Directory.Delete(external);
        }
    }

    [Fact]
    public void DefaultCacheLocationIsUserSpecificAndAbsolute()
    {
        var path = CacheDirectory.GetDefaultPath();
        Assert.True(Path.IsPathFullyQualified(path));
        Assert.Contains("afrowave", path, StringComparison.OrdinalIgnoreCase);
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
