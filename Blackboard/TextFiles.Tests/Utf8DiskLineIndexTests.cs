using System.Text;
using Afrowave.Toolbox.Blackboard.TextFiles;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.TextFiles.Tests;

public sealed class Utf8DiskLineIndexTests
{
    [Fact]
    public async Task EmptyFileHasNoPhantomLine()
    {
        using var fixture = new Fixture();
        var file = await fixture.WriteAsync("");
        await using var index = await Utf8DiskLineIndex.OpenAsync(file);
        Assert.Equal(0, index.ByteLength);
        Assert.Equal(0, index.LineCount);
        Assert.Equal(8, index.IndexBytes);
        var page = await index.ReadPageAsync(0);
        Assert.Empty(page.Lines);
        Assert.False(page.HasMore);
    }

    [Fact]
    public async Task CrLfCrAndLfAreSeparatePhysicalLineTerminators()
    {
        using var fixture = new Fixture();
        var file = await fixture.WriteAsync("Alpha\r\nBeta\nGamma\rDelta");
        await using var index = await Utf8DiskLineIndex.OpenAsync(file);
        Assert.Equal(4, index.LineCount);
        Assert.Equal(40, index.IndexBytes);
        var page = await index.ReadPageAsync(0);
        Assert.Equal(new[] { "Alpha", "Beta", "Gamma", "Delta" }, page.Lines);
        Assert.False(page.HasMore);
    }

    [Fact]
    public async Task FinalNewlineDoesNotCreateAnExtraEmptyLine()
    {
        using var fixture = new Fixture();
        var file = await fixture.WriteAsync("First\n\n");
        await using var index = await Utf8DiskLineIndex.OpenAsync(file);
        Assert.Equal(2, index.LineCount);
        Assert.Equal(new[] { "First", "" }, (await index.ReadPageAsync(0)).Lines);
    }

    [Fact]
    public async Task Utf8BomIsRemovedOnlyAtDocumentStartAndUnicodeIsPreserved()
    {
        using var fixture = new Fixture();
        var bytes = new UTF8Encoding(true).GetPreamble()
            .Concat(Encoding.UTF8.GetBytes("# Nadpis 🌍\nPříliš žluťoučký kůň"))
            .ToArray();
        var file = await fixture.WriteBytesAsync(bytes);
        await using var index = await Utf8DiskLineIndex.OpenAsync(file);
        Assert.Equal(2, index.LineCount);
        var page = await index.ReadPageAsync(0);
        Assert.Equal(new[] { "# Nadpis 🌍", "Příliš žluťoučký kůň" }, page.Lines);
    }

    [Fact]
    public async Task PagesAreRandomAccessWithoutReadingTheEntireDocument()
    {
        using var fixture = new Fixture();
        var file = await fixture.WriteAsync(string.Join("\n",
            Enumerable.Range(0, 257).Select(i => "Line " + i)));
        await using var index = await Utf8DiskLineIndex.OpenAsync(file);
        Assert.Equal(257, index.LineCount);
        Assert.Equal(8L * 258, index.IndexBytes);
        var first = await index.ReadPageAsync(0, 128);
        Assert.Equal(128, first.Lines.Count);
        Assert.Equal(128, first.NextLine);
        Assert.True(first.HasMore);
        var last = await index.ReadPageAsync(256, 128);
        Assert.Equal("Line 256", Assert.Single(last.Lines));
        Assert.False(last.HasMore);
        var back = await index.ReadPageAsync(120, 3);
        Assert.Equal(new[] { "Line 120", "Line 121", "Line 122" }, back.Lines);
    }

    [Fact]
    public async Task CrLfAcrossIndexBufferBoundaryIsOneNewline()
    {
        using var fixture = new Fixture();
        var line = new string('a', 65535);
        var file = await fixture.WriteAsync(line + "\r\nSecond");
        await using var index = await Utf8DiskLineIndex.OpenAsync(file, 128 * 1024);
        Assert.Equal(2, index.LineCount);
        var page = await index.ReadPageAsync(0);
        Assert.Equal(line, page.Lines[0]);
        Assert.Equal("Second", page.Lines[1]);
    }

    [Fact]
    public async Task TemporaryIndexBudgetFailsExplicitlyWithoutTruncatingDocument()
    {
        using var fixture = new Fixture();
        var file = await fixture.WriteAsync("One\nTwo\nThree");
        await Assert.ThrowsAsync<InvalidDataException>(() =>
            Utf8DiskLineIndex.OpenAsync(file, maximumIndexBytes: 24));
    }

    [Fact]
    public async Task UnicodeCodepointSplitAcrossIndexBuffersIsAccepted()
    {
        using var fixture = new Fixture();
        var first = new string('a', 65535) + "🌍";
        var file = await fixture.WriteAsync(first + "\nNext");
        await using var index = await Utf8DiskLineIndex.OpenAsync(file, 128 * 1024);
        var page = await index.ReadPageAsync(0, 2);
        Assert.Equal(first, page.Lines[0]);
        Assert.Equal("Next", page.Lines[1]);
    }

    [Fact]
    public async Task InvalidUtf8AfterTheFirstBufferRejectsTheEntireIndex()
    {
        using var fixture = new Fixture();
        var bytes = Enumerable.Repeat((byte)'a', 70_000).Concat(new byte[] { 0xC3, 0x28 }).ToArray();
        var file = await fixture.WriteBytesAsync(bytes);
        await Assert.ThrowsAsync<InvalidDataException>(() => Utf8DiskLineIndex.OpenAsync(file));
    }

    [Fact]
    public async Task UnsafeControlBeyondTheFirstPageRejectsBeforeDisplay()
    {
        using var fixture = new Fixture();
        var file = await fixture.WriteAsync(new string('x', 70_000) + "\u001b[31m");
        await Assert.ThrowsAsync<InvalidDataException>(() => Utf8DiskLineIndex.OpenAsync(file));
    }

    [Fact]
    public async Task Utf16IsNotSilentlyTreatedAsUtf8()
    {
        using var fixture = new Fixture();
        var bytes = Encoding.Unicode.GetPreamble()
            .Concat(Encoding.Unicode.GetBytes("# Test")).ToArray();
        var file = await fixture.WriteBytesAsync(bytes);
        await Assert.ThrowsAsync<InvalidDataException>(() => Utf8DiskLineIndex.OpenAsync(file));
    }

    [Fact]
    public async Task ExcessivelyLongLineReturnsExplicitErrorRatherThanTruncation()
    {
        using var fixture = new Fixture();
        var file = await fixture.WriteAsync(new string('a', 4097));
        await using var index = await Utf8DiskLineIndex.OpenAsync(file, 4096);
        Assert.Equal(1, index.LineCount);
        await Assert.ThrowsAsync<InvalidDataException>(() => index.ReadPageAsync(0));
    }

    [Fact]
    public async Task ByteBudgetCreatesShorterPageAndPreservesNextLine()
    {
        using var fixture = new Fixture();
        const int longLineLength = 60000;
        var file = await fixture.WriteAsync(string.Join("\n",
            Enumerable.Repeat(new string('z', longLineLength), 100)));
        await using var index = await Utf8DiskLineIndex.OpenAsync(file, 65536);
        var first = await index.ReadPageAsync(0, 100);
        Assert.True(first.HasMore);
        Assert.InRange(first.Lines.Count, 1, 99);
        Assert.Equal(first.Lines.Count, first.NextLine);
        var second = await index.ReadPageAsync(first.NextLine, 100);
        Assert.False(second.HasMore);
        Assert.Equal(100, first.Lines.Count + second.Lines.Count);
    }

    [Fact]
    public async Task CancelledIndexCreationDoesNotReturnUnvalidatedContent()
    {
        using var fixture = new Fixture();
        var file = await fixture.WriteAsync("Safe content");
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            Utf8DiskLineIndex.OpenAsync(file, cancellationToken: cancellation.Token));
    }

    [Fact]
    public async Task InvalidPageArgumentsAreRejected()
    {
        using var fixture = new Fixture();
        var file = await fixture.WriteAsync("First\nSecond");
        await using var index = await Utf8DiskLineIndex.OpenAsync(file);
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => index.ReadPageAsync(-1));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => index.ReadPageAsync(0, 0));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => index.ReadPageAsync(0, 129));
        Assert.Empty((await index.ReadPageAsync(2)).Lines);
    }

    private sealed class Fixture : IDisposable
    {
        public string Root { get; } =
            Path.Combine(Path.GetTempPath(), "afrowave-index-test-" + Guid.NewGuid().ToString("N"));

        public Fixture() => Directory.CreateDirectory(Root);

        public Task<string> WriteAsync(string content) =>
            WriteBytesAsync(Encoding.UTF8.GetBytes(content));

        public async Task<string> WriteBytesAsync(byte[] content)
        {
            var path = Path.Combine(Root, "README.md");
            await File.WriteAllBytesAsync(path, content);
            return path;
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
                Directory.Delete(Root, recursive: true);
        }
    }
}
