using Afrowave.Toolbox.Blackboard.Tools.MdView;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView.Tests;

/// <summary>Bounded history and local snapshot navigation regression coverage.</summary>
public sealed class MdViewNavigationHistoryTests
{
    [Fact]
    public void BackForwardAndBranchingPreserveDocumentAndFragment()
    {
        var source = MdViewDocumentOrigin.FromHttps("https://docs.example.org/README.md");
        var history = new MdViewNavigationHistory(capacity: 3);
        var first = new MdViewResolvedLink(source, null);
        var second = new MdViewResolvedLink(source, "intro");
        var third = MdViewLinkResolver.Resolve(source, "Docs/en.md");
        history.Visit(first);
        history.Visit(second);
        history.Visit(third);

        Assert.True(history.GoBack());
        Assert.Equal(second, history.Current);
        Assert.True(history.CanGoForward);
        Assert.True(history.GoBack());
        Assert.Equal(first, history.Current);
        Assert.False(history.GoBack());
        Assert.True(history.GoForward());
        history.Visit(MdViewLinkResolver.Resolve(source, "Other.md"));
        Assert.False(history.CanGoForward);
        Assert.Equal(3, history.Count);
    }

    [Fact]
    public void CapacityEvictsOldestAndRepeatedVisitsAreNoOps()
    {
        var source = MdViewDocumentOrigin.FromGitHub(
            "afrowaveltd", "Toolbox", "main", "README.md");
        var history = new MdViewNavigationHistory(capacity: 2);
        history.Visit(new MdViewResolvedLink(source, null));
        history.Visit(new MdViewResolvedLink(source, null));
        Assert.Single(new[] { history.Current });
        Assert.Equal(1, history.Count);
        history.Visit(new MdViewResolvedLink(source, "one"));
        history.Visit(new MdViewResolvedLink(source, "two"));
        Assert.Equal(2, history.Count);
        Assert.True(history.GoBack());
        Assert.Equal("one", history.Current!.Fragment);
        Assert.False(history.CanGoBack);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new MdViewNavigationHistory(capacity: 0));
    }

    [Fact]
    public async Task LocalSessionFollowsBacksAndBranchesWithVerifiedSnapshots()
    {
        using var docs = new Docs();
        var home = docs.Write("README.md", "# Home");
        docs.Write("Docs/en.md", "# Docs");
        docs.Write("Other.md", "# Other");
        var origin = MdViewDocumentOrigin.FromLocal(home, docs.Root);
        await using var navigation = await MdViewLocalNavigationSession.OpenAsync(origin);

        await navigation.FollowAsync("Docs/en.md#start");
        Assert.Equal("start", navigation.Current.Target.Fragment);
        Assert.Equal("# Docs", await ReadCurrentAsync(navigation));
        Assert.True(await navigation.BackAsync());
        Assert.Equal("# Home", await ReadCurrentAsync(navigation));
        Assert.True(await navigation.ForwardAsync());
        Assert.Equal("# Docs", await ReadCurrentAsync(navigation));
        await navigation.FollowAsync("../Other.md");
        Assert.Equal("# Other", await ReadCurrentAsync(navigation));
        Assert.False(navigation.History.CanGoForward);
    }

    [Fact]
    public async Task FailedLocalOpenLeavesHistoryAndCurrentDocumentUnchanged()
    {
        using var docs = new Docs();
        var home = docs.Write("README.md", "# Home");
        var origin = MdViewDocumentOrigin.FromLocal(home, docs.Root);
        await using var navigation = await MdViewLocalNavigationSession.OpenAsync(origin);

        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            navigation.FollowAsync("missing.md"));
        Assert.Equal(1, navigation.History.Count);
        Assert.False(navigation.History.CanGoBack);
        Assert.Equal("# Home", await ReadCurrentAsync(navigation));
    }

    [Fact]
    public async Task FailedBackDoesNotAdvanceHistory()
    {
        using var docs = new Docs();
        var home = docs.Write("README.md", "# Home");
        docs.Write("Docs/en.md", "# Docs");
        await using var navigation = await MdViewLocalNavigationSession.OpenAsync(
            MdViewDocumentOrigin.FromLocal(home, docs.Root));
        await navigation.FollowAsync("Docs/en.md");
        File.Delete(home);

        await Assert.ThrowsAsync<FileNotFoundException>(() => navigation.BackAsync());
        Assert.Equal("en.md", Path.GetFileName(navigation.History.Current!.Origin.Location));
        Assert.Equal("# Docs", await ReadCurrentAsync(navigation));
    }

    private static async Task<string> ReadCurrentAsync(MdViewLocalNavigationSession session)
    {
        using var reader = session.Current.Snapshot.OpenReader();
        return await reader.ReadToEndAsync();
    }

    private sealed class Docs : IDisposable
    {
        public string Root { get; } = Path.Combine(
            Path.GetTempPath(), "mdview-navigation-test-" + Guid.NewGuid().ToString("N"));
        public Docs() => Directory.CreateDirectory(Root);
        public string Write(string relative, string contents)
        {
            var path = Path.Combine(Root, relative.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, contents);
            return path;
        }
        public void Dispose() => Directory.Delete(Root, recursive: true);
    }
}
