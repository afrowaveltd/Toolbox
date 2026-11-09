using Afrowave.Toolbox.Blackboard.Tools.MdView;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView.Tests;

/// <summary>Regression tests for pure Markdown link resolution without I/O.</summary>
public sealed class MdViewLinkResolverTests
{
    [Fact]
    public void LocalLinksResolveAgainstDocumentDirectoryNotProcessDirectory()
    {
        var root = Path.Combine(Path.GetTempPath(), "mdview-docs-root");
        var source = MdViewDocumentOrigin.FromLocal(
            Path.Combine(root, "README.md"), root);
        var child = MdViewLinkResolver.Resolve(source, "Docs/Install/en.md#setup");

        Assert.Equal(Path.GetFullPath(Path.Combine(root, "Docs", "Install", "en.md")),
            child.Origin.Location);
        Assert.Equal("setup", child.Fragment);
        var parent = MdViewLinkResolver.Resolve(child.Origin, "../../README.md");
        Assert.Equal(source.Location, parent.Origin.Location);
        Assert.Null(parent.Fragment);
    }

    [Fact]
    public void LocalRootEscapesAndAbsoluteTargetsAreRejected()
    {
        var root = Path.Combine(Path.GetTempPath(), "mdview-root");
        var source = MdViewDocumentOrigin.FromLocal(
            Path.Combine(root, "README.md"), root);
        Assert.Throws<ArgumentException>(() =>
            MdViewLinkResolver.Resolve(source, "../elsewhere.md"));
        Assert.Throws<ArgumentException>(() =>
            MdViewLinkResolver.Resolve(source, "/etc/passwd"));
        Assert.Throws<ArgumentException>(() =>
            MdViewLinkResolver.Resolve(source, "Docs/%2e%2e/other.md"));
        Assert.Throws<ArgumentException>(() =>
            MdViewLinkResolver.Resolve(source, "file:///tmp/foo.md"));
        Assert.Throws<ArgumentException>(() =>
            MdViewDocumentOrigin.FromLocal(
                Path.Combine(root, "..", "outside.md"), root));
    }

    [Fact]
    public void HttpsLinksFollowSameOriginAndPreserveBasePath()
    {
        var source = MdViewDocumentOrigin.FromHttps(
            "https://docs.example.org/manual/README.md");
        var link = MdViewLinkResolver.Resolve(source, "Docs/en.md#installation");
        Assert.Equal("https://docs.example.org/manual/Docs/en.md",
            link.Origin.Location);
        Assert.Equal("installation", link.Fragment);
        var upper = MdViewLinkResolver.Resolve(link.Origin, "../README.md");
        Assert.Equal(source.Location, upper.Origin.Location);
        var root = MdViewLinkResolver.Resolve(source, "/guide.md");
        Assert.Equal("https://docs.example.org/guide.md", root.Origin.Location);
    }

    [Theory]
    [InlineData("//evil.example.org/README.md")]
    [InlineData("https://evil.example.org/file.md")]
    [InlineData("javascript:alert(1)")]
    [InlineData("data:text/html;base64,QQ==")]
    [InlineData("file:///etc/passwd")]
    [InlineData("Docs/%2fprivate.md")]
    [InlineData("Docs/en.md?token=x")]
    public void UnsafeOrUnsupportedLinksAreRejected(string href)
    {
        var source = MdViewDocumentOrigin.FromHttps(
            "https://docs.example.org/manual/README.md");
        Assert.Throws<ArgumentException>(() =>
            MdViewLinkResolver.Resolve(source, href));
    }

    [Fact]
    public void GitHubLinksRetainOwnerRepositoryAndResolvedRef()
    {
        var source = MdViewDocumentOrigin.FromGitHub(
            "afrowaveltd", "Toolbox", "release/v2", "README.md");
        var target = MdViewLinkResolver.Resolve(source,
            "Blackboard/Tools/MdView/Docs/CLI/en.md#usage");
        Assert.Equal(MdViewDocumentOriginKind.GitHub, target.Origin.Kind);
        Assert.Equal("Blackboard/Tools/MdView/Docs/CLI/en.md", target.Origin.Location);
        Assert.Equal("release/v2", target.Origin.Ref);
        Assert.Equal("Toolbox", target.Origin.Repository);
        Assert.Equal("usage", target.Fragment);
        var previous = MdViewLinkResolver.Resolve(target.Origin,
            "../../../../../README.md");
        Assert.Equal("README.md", previous.Origin.Location);
        Assert.Throws<ArgumentException>(() =>
            MdViewLinkResolver.Resolve(source, "../other-project/file.md"));
    }

    [Fact]
    public void AnchorOnlyLinksDoNotModifyDocumentOrigin()
    {
        var source = MdViewDocumentOrigin.FromGitHub(
            "afrowaveltd", "Toolbox", "main", "README.md");
        var target = MdViewLinkResolver.Resolve(source, "#installation");
        Assert.Same(source, target.Origin);
        Assert.Equal("installation", target.Fragment);
    }
}
