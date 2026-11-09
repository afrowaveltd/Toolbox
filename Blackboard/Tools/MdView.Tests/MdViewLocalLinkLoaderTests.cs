using System.Text;
using Afrowave.Toolbox.Blackboard.Tools.MdView;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView.Tests;

/// <summary>Local Markdown link opening and validation regression coverage.</summary>
public sealed class MdViewLocalLinkLoaderTests
{
    [Fact]
    public async Task OpensNestedMarkdownRelativeToCurrentDocument()
    {
        using var fixture = new TempDocs();
        var readme = fixture.Write("README.md", "# Home");
        fixture.Write("Docs/Install/en.md", "# Instalace 🌍\r\nDetails");
        var origin = MdViewDocumentOrigin.FromLocal(readme, fixture.Root);

        await using var linked = await MdViewLocalLinkLoader.OpenAsync(
            origin, "Docs/Install/en.md#setup");
        Assert.Equal("setup", linked.Target.Fragment);
        using var reader = linked.Snapshot.OpenReader();
        Assert.Equal("# Instalace 🌍\r\nDetails", await reader.ReadToEndAsync());
        Assert.Equal(MdViewDocumentOriginKind.Local, linked.Target.Origin.Kind);
    }

    [Fact]
    public async Task NestedDocumentCanNavigateBackWithinRoot()
    {
        using var fixture = new TempDocs();
        fixture.Write("README.md", "# home");
        var child = fixture.Write("Docs/Install/en.md", "# install");
        var origin = MdViewDocumentOrigin.FromLocal(child, fixture.Root);

        await using var linked = await MdViewLocalLinkLoader.OpenAsync(
            origin, "../../README.md");
        Assert.Equal(Path.Combine(fixture.Root, "README.md"),
            linked.Target.Origin.Location);
        Assert.Equal(6, linked.Snapshot.ByteLength);
    }

    [Fact]
    public async Task RejectsNonMarkdownAndOutsideRootBeforeOpening()
    {
        using var fixture = new TempDocs();
        var readme = fixture.Write("README.md", "# Home");
        fixture.Write("Docs/data.txt", "plain");
        var origin = MdViewDocumentOrigin.FromLocal(readme, fixture.Root);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            MdViewLocalLinkLoader.OpenAsync(origin, "Docs/data.txt"));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            MdViewLocalLinkLoader.OpenAsync(origin, "../private.md"));
    }

    [Fact]
    public async Task RejectsInvalidUtf8AndSnapshotDiskBudget()
    {
        using var fixture = new TempDocs();
        var readme = fixture.Write("README.md", "# home");
        var invalid = Path.Combine(fixture.Root, "bad.md");
        File.WriteAllBytes(invalid, new byte[] { 0xC3, 0x28 });
        fixture.Write("large.md", new string('a', 100));
        var origin = MdViewDocumentOrigin.FromLocal(readme, fixture.Root);

        await Assert.ThrowsAsync<InvalidDataException>(() =>
            MdViewLocalLinkLoader.OpenAsync(origin, "bad.md"));
        await Assert.ThrowsAsync<InvalidDataException>(() =>
            MdViewLocalLinkLoader.OpenAsync(origin, "large.md",
                maximumSnapshotBytes: 8));
    }

    [Fact]
    public async Task RejectsSymbolicLinkComponentsWhenAvailable()
    {
        using var fixture = new TempDocs();
        var source = fixture.Write("README.md", "# Home");
        var real = Path.Combine(fixture.Root, "real");
        Directory.CreateDirectory(real);
        File.WriteAllText(Path.Combine(real, "en.md"), "# real");
        var link = Path.Combine(fixture.Root, "link");
        try
        {
            Directory.CreateSymbolicLink(link, real);
        }
        catch (Exception exception) when (
            exception is UnauthorizedAccessException or IOException or
                PlatformNotSupportedException or NotSupportedException)
        {
            // Windows can require Developer Mode or elevated privileges.
            return;
        }

        var origin = MdViewDocumentOrigin.FromLocal(source, fixture.Root);
        await Assert.ThrowsAsync<IOException>(() =>
            MdViewLocalLinkLoader.OpenAsync(origin, "link/en.md"));
    }

    private sealed class TempDocs : IDisposable
    {
        public string Root { get; } = Path.Combine(
            Path.GetTempPath(), "mdview-link-loader-" + Guid.NewGuid().ToString("N"));

        public TempDocs() => Directory.CreateDirectory(Root);

        public string Write(string relative, string text)
        {
            var path = Path.Combine(Root, relative.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, text, new UTF8Encoding(false));
            return path;
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
                Directory.Delete(Root, recursive: true);
        }
    }
}
