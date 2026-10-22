using Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser.Tests;

public sealed class FileBrowserTreeSearchTests
{
    private static FileBrowserLocation At(string path) => new("mock", path);
    private static FileBrowserSourceEntry File(string name, string path) => new(name, At(path), false);
    private static FileBrowserSourceEntry Folder(string name, string path, bool traverse = true)
        => new(name, At(path), true, traverse);

    [Fact]
    public async Task Lazy_tree_only_loads_requested_directory_and_keeps_untraversable_folders_visible()
    {
        var source = new FakeSource(new Dictionary<string, FileBrowserSourceEntry[]>
        {
            ["/"] = [Folder("zeta", "/zeta"), File("test.json", "/test.json"),
                Folder("Alpha", "/Alpha"), Folder("linked", "/linked", false)],
            ["/Alpha"] = [Folder("nested", "/Alpha/nested")]
        });
        var tree = new FileBrowserDirectoryTree(source);
        var children = await tree.GetChildrenAsync(At("/"));

        Assert.Equal(["Alpha", "linked", "zeta"], children.Select(x => x.Name).ToArray());
        Assert.False(children[1].CanExpand);
        Assert.Equal(["/"], source.Listed);

        Assert.Equal("nested", Assert.Single(await tree.GetChildrenAsync(At("/Alpha"))).Name);
        Assert.Equal(["/", "/Alpha"], source.Listed);
    }

    [Fact]
    public async Task Search_shows_all_matches_as_full_relative_paths_and_a_pruned_tree()
    {
        var source = new FakeSource(new Dictionary<string, FileBrowserSourceEntry[]>
        {
            ["/start"] = [Folder("FOLDER", "/start/FOLDER"), Folder("ANOTHERFOLDER", "/start/ANOTHERFOLDER")],
            ["/start/FOLDER"] = [Folder("SUBFOLDER", "/start/FOLDER/SUBFOLDER")],
            ["/start/FOLDER/SUBFOLDER"] = [File("cohledame.txt", "/start/FOLDER/SUBFOLDER/cohledame.txt")],
            ["/start/ANOTHERFOLDER"] = [File("cohledame.txt", "/start/ANOTHERFOLDER/cohledame.txt"),
                File("other.md", "/start/ANOTHERFOLDER/other.md")]
        });

        var session = new FileBrowserNavigationSession(source, At("/start"));
        var results = await session.SearchAsync("cohledame.txt");

        Assert.Equal(FileBrowserSearchStatus.Completed, results.Status);
        Assert.Equal(2, results.Matches.Count);
        Assert.Equal(At("/start"), session.CurrentDirectory);
        Assert.Null(session.HighlightedEntry);

        var rows = FileBrowserSearchResultsPresentation.CreateRows(results);
        Assert.Equal(["ANOTHERFOLDER/cohledame.txt", "FOLDER/SUBFOLDER/cohledame.txt"],
            rows.Select(x => x.RelativePath).ToArray());
        Assert.NotEqual(rows[0].Location, rows[1].Location);

        var branches = FileBrowserSearchResultsPresentation.CreateTree(results);
        Assert.Equal(["ANOTHERFOLDER", "FOLDER"], branches.Select(x => x.Name).ToArray());
        Assert.Null(branches[0].MatchIndex);
        Assert.Equal("cohledame.txt", Assert.Single(branches[0].Children).Name);
        var subFolder = Assert.Single(branches[1].Children);
        Assert.Equal("SUBFOLDER", subFolder.Name);
        Assert.Equal("cohledame.txt", Assert.Single(subFolder.Children).Name);

        // User explicitly chooses the second displayed result: map its original
        // match index instead of assuming presentation order equals search order.
        Assert.True(session.SelectSearchResult(results, rows[1].MatchIndex));
        Assert.Equal(At("/start/FOLDER/SUBFOLDER"), session.CurrentDirectory);
        Assert.Equal("cohledame.txt", session.HighlightedEntry!.Name);
    }

    [Fact]
    public async Task No_match_leaves_current_folder_and_prior_selection_unchanged()
    {
        var source = new FakeSource(new Dictionary<string, FileBrowserSourceEntry[]>
        {
            ["/"] = [File("found.json", "/found.json")]
        });
        var session = new FileBrowserNavigationSession(source, At("/"));
        var initial = await session.SearchAsync("found.json");
        Assert.True(session.SelectSearchResult(initial, 0));
        var previousHighlight = session.HighlightedEntry;

        var results = await session.SearchAsync("missing.json");
        Assert.Empty(results.Matches);
        Assert.Empty(FileBrowserSearchResultsPresentation.CreateRows(results));
        Assert.Empty(FileBrowserSearchResultsPresentation.CreateTree(results));
        Assert.Equal(FileBrowserSearchStatus.Completed, results.Status);
        Assert.False(session.SelectSearchResult(results, 0));
        Assert.Equal(At("/"), session.CurrentDirectory);
        Assert.Same(previousHighlight, session.HighlightedEntry);
    }

    [Fact]
    public async Task Wildcards_work_and_source_case_sensitivity_is_respected()
    {
        var entries = new Dictionary<string, FileBrowserSourceEntry[]>
        {
            ["/"] = [File("config1.json", "/config1.json"),
                File("config2.JSON", "/config2.JSON"), File("configs.json", "/configs.json")]
        };
        var sensitive = new FileBrowserRecursiveSearch(new FakeSource(entries, true));
        var insensitive = new FileBrowserRecursiveSearch(new FakeSource(entries, false));

        Assert.Single((await sensitive.FindAsync(At("/"), "config?.json")).Matches);
        Assert.Equal(3, (await insensitive.FindAsync(At("/"), "config*.json")).Matches.Count);
        Assert.Single((await insensitive.FindAsync(At("/"), "config2.json")).Matches);
    }

    [Fact]
    public async Task Search_returns_multiple_matches_with_same_filename_and_different_identity()
    {
        var source = new FakeSource(new Dictionary<string, FileBrowserSourceEntry[]>
        {
            ["/"] = [Folder("a", "/a"), Folder("b", "/b")],
            ["/a"] = [File("settings.json", "/a/settings.json")],
            ["/b"] = [File("settings.json", "/b/settings.json")]
        });
        var result = await new FileBrowserRecursiveSearch(source).FindAsync(At("/"), "*.json");

        Assert.Equal(2, result.Matches.Count);
        var rows = FileBrowserSearchResultsPresentation.CreateRows(result);
        Assert.Equal(["a/settings.json", "b/settings.json"], rows.Select(x => x.RelativePath).ToArray());
        Assert.NotEqual(rows[0].Location, rows[1].Location);
    }

    [Fact]
    public async Task Recursion_does_not_enter_symlink_like_directory_or_cyclic_location()
    {
        var source = new FakeSource(new Dictionary<string, FileBrowserSourceEntry[]>
        {
            ["/"] = [Folder("do-not-follow", "/link", false), Folder("safe", "/safe")],
            ["/link"] = [File("secret.json", "/link/secret.json")],
            ["/safe"] = [Folder("back", "/"), File("good.json", "/safe/good.json")]
        });
        var found = await new FileBrowserRecursiveSearch(source).FindAsync(At("/"), "*.json");
        Assert.Equal("good.json", Assert.Single(found.Matches).Entry.Name);
        Assert.Equal(2, found.VisitedDirectories);
        Assert.DoesNotContain("/link", source.Listed);
    }

    [Fact]
    public async Task Filters_limit_results_not_directory_traversal()
    {
        var source = new FakeSource(new Dictionary<string, FileBrowserSourceEntry[]>
        {
            ["/"] = [Folder("hidden.notjson", "/hidden"), File("song.mp3", "/song.mp3")],
            ["/hidden"] = [File("config.json", "/hidden/config.json"), File("notes.txt", "/hidden/notes.txt")]
        });
        var result = await new FileBrowserRecursiveSearch(source).FindAsync(
            At("/"), "*", new FileBrowserSearchOptions
            {
                Filter = new FileBrowserFileFilter(includedKinds: [FileBrowserFileKind.Json])
            });
        Assert.Equal("hidden.notjson/config.json",
            Assert.Single(FileBrowserSearchResultsPresentation.CreateRows(result)).RelativePath);
    }

    [Fact]
    public async Task Search_status_indicates_partial_unreadable_directory()
    {
        var source = new FakeSource(new Dictionary<string, FileBrowserSourceEntry[]>
        {
            ["/"] = [Folder("bad", "/bad"), Folder("good", "/good")],
            ["/good"] = [File("config.json", "/good/config.json")]
        }, failures: ["/bad"]);
        var found = await new FileBrowserRecursiveSearch(source).FindAsync(At("/"), "*.json");
        Assert.Equal("config.json", Assert.Single(found.Matches).Entry.Name);
        Assert.Equal(FileBrowserSearchStatus.Partial, found.Status);
        Assert.Equal(1, found.SkippedDirectories);
    }

    [Fact]
    public async Task Result_cap_avoids_false_claim_of_complete_search()
    {
        var source = new FakeSource(new Dictionary<string, FileBrowserSourceEntry[]>
        {
            ["/"] = [File("1.json", "/1.json"), File("2.json", "/2.json"), File("3.json", "/3.json")]
        });
        var result = await new FileBrowserRecursiveSearch(source).FindAsync(
            At("/"), "*.json", new FileBrowserSearchOptions { MaxResults = 2 });
        Assert.Equal(2, result.Matches.Count);
        Assert.Equal(2, FileBrowserSearchResultsPresentation.CreateRows(result).Count);
        Assert.Equal(FileBrowserSearchStatus.LimitReached, result.Status);
    }

    [Fact]
    public async Task Directory_depth_limit_marks_result_incomplete()
    {
        var source = new FakeSource(new Dictionary<string, FileBrowserSourceEntry[]>
        {
            ["/"] = [Folder("deeper", "/deeper")],
            ["/deeper"] = [File("target.json", "/deeper/target.json")]
        });
        var result = await new FileBrowserRecursiveSearch(source).FindAsync(
            At("/"), "*.json", new FileBrowserSearchOptions { MaxDepth = 0 });
        Assert.Empty(result.Matches);
        Assert.Equal(FileBrowserSearchStatus.LimitReached, result.Status);
        Assert.Equal(1, result.VisitedDirectories);
    }

    [Fact]
    public async Task Directory_budget_is_observed()
    {
        var source = new FakeSource(new Dictionary<string, FileBrowserSourceEntry[]>
        {
            ["/"] = [Folder("a", "/a")],
            ["/a"] = [File("file.json", "/a/file.json")]
        });
        var result = await new FileBrowserRecursiveSearch(source).FindAsync(
            At("/"), "*.json", new FileBrowserSearchOptions { MaxDirectories = 1 });
        Assert.Empty(result.Matches);
        Assert.Equal(FileBrowserSearchStatus.LimitReached, result.Status);
    }

    [Fact]
    public async Task Entry_budget_stops_large_directory_with_explicit_status()
    {
        var source = new FakeSource(new Dictionary<string, FileBrowserSourceEntry[]>
        {
            ["/"] = [File("first.txt", "/first.txt"), File("match.json", "/match.json"),
                File("another.json", "/another.json")]
        });
        var result = await new FileBrowserRecursiveSearch(source).FindAsync(
            At("/"), "*.json", new FileBrowserSearchOptions { MaxEntriesPerDirectory = 2 });
        Assert.Single(result.Matches);
        Assert.Equal(FileBrowserSearchStatus.LimitReached, result.Status);
    }

    [Fact]
    public async Task Cancelled_search_does_not_move_browser()
    {
        var source = new FakeSource(new Dictionary<string, FileBrowserSourceEntry[]> { ["/"] = [] });
        var session = new FileBrowserNavigationSession(source, At("/"));
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => session.SearchAsync("*", cancellationToken: cts.Token));
        Assert.Equal(At("/"), session.CurrentDirectory);
        Assert.Null(session.HighlightedEntry);
    }

    [Fact]
    public async Task Search_for_directory_is_explicitly_opt_in()
    {
        var source = new FakeSource(new Dictionary<string, FileBrowserSourceEntry[]>
        {
            ["/"] = [Folder("reports", "/reports")],
            ["/reports"] = []
        });
        var engine = new FileBrowserRecursiveSearch(source);
        Assert.Empty((await engine.FindAsync(At("/"), "reports")).Matches);

        var found = await engine.FindAsync(At("/"), "reports",
            new FileBrowserSearchOptions { IncludeDirectories = true });
        Assert.True(Assert.Single(found.Matches).Entry.IsDirectory);
        Assert.True(Assert.Single(FileBrowserSearchResultsPresentation.CreateTree(found)).IsDirectory);
    }

    [Fact]
    public async Task Search_paths_escape_control_and_provider_separator_characters()
    {
        var source = new FakeSource(new Dictionary<string, FileBrowserSourceEntry[]>
        {
            ["/"] = [Folder("strange/sub", "/opaque1")],
            ["/opaque1"] = [File("bad\u001B.json", "/opaque2")]
        });
        var result = await new FileBrowserRecursiveSearch(source).FindAsync(At("/"), "*");
        var row = Assert.Single(FileBrowserSearchResultsPresentation.CreateRows(result));
        Assert.Equal(@"strange\u002Fsub/bad\u001B.json", row.RelativePath);
        Assert.Equal("/opaque2", row.Location.Value);
    }

    [Fact]
    public async Task Selection_rejects_stale_and_out_of_range_results()
    {
        var source = new FakeSource(new Dictionary<string, FileBrowserSourceEntry[]>
        {
            ["/"] = [Folder("a", "/a"), File("root.json", "/root.json")],
            ["/a"] = [File("nested.json", "/a/nested.json")]
        });
        var session = new FileBrowserNavigationSession(source, At("/"));
        var results = await session.SearchAsync("*.json");
        Assert.False(session.SelectSearchResult(results, -1));
        Assert.False(session.SelectSearchResult(results, 99));
        Assert.True(session.SelectSearchResult(results, 1));
        Assert.Equal(At("/a"), session.CurrentDirectory);
        Assert.False(session.SelectSearchResult(results, 0));
        Assert.Equal(At("/a"), session.CurrentDirectory);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("../secrets.json")]
    [InlineData(@"sub\*.json")]
    [InlineData("bad\nfile")]
    public async Task Bad_name_patterns_fail_validation(string pattern)
    {
        var engine = new FileBrowserRecursiveSearch(
            new FakeSource(new Dictionary<string, FileBrowserSourceEntry[]> { ["/"] = [] }));
        await Assert.ThrowsAsync<ArgumentException>(() => engine.FindAsync(At("/"), pattern));
    }

    [Fact]
    public async Task Foreign_locations_are_not_interpreted_as_local()
    {
        var src = new FakeSource(new Dictionary<string, FileBrowserSourceEntry[]> { ["/"] = [] });
        var foreign = new FileBrowserLocation("ftp", "/");
        await Assert.ThrowsAsync<ArgumentException>(() =>
            new FileBrowserRecursiveSearch(src).FindAsync(foreign, "*"));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            new FileBrowserDirectoryTree(src).GetChildrenAsync(foreign));
        Assert.Throws<ArgumentException>(() => new FileBrowserNavigationSession(src, foreign));
    }

    [Fact]
    public async Task Local_source_enumerates_files_and_parent_without_changing_process_directory()
    {
        var tmp = Path.Combine(Path.GetTempPath(), "AfrowaveFB-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tmp);
        try
        {
            Directory.CreateDirectory(Path.Combine(tmp, "docs"));
            File.WriteAllText(Path.Combine(tmp, "one.json"), "{}");
            var cwd = Environment.CurrentDirectory;
            var src = new FileBrowserLocalSource();
            var entries = await src.ListAsync(new FileBrowserLocation("local", tmp));
            Assert.Contains(entries, e => e.Name == "docs" && e.IsDirectory);
            Assert.Contains(entries, e => e.Name == "one.json" && !e.IsDirectory);
            Assert.Equal(cwd, Environment.CurrentDirectory);
            var parent = await src.GetParentAsync(new FileBrowserLocation("local", tmp));
            Assert.Equal(Path.GetDirectoryName(tmp), parent?.Value);
        }
        finally
        {
            Directory.Delete(tmp, recursive: true);
        }
    }

    [Fact]
    public async Task Local_source_refuses_foreign_or_relative_locations()
    {
        var src = new FileBrowserLocalSource();
        await Assert.ThrowsAsync<ArgumentException>(() =>
            src.ListAsync(new FileBrowserLocation("ftp", "/root")));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            src.ListAsync(new FileBrowserLocation("local", "relative")));
    }

    private sealed class FakeSource : IFileBrowserSource
    {
        private readonly IReadOnlyDictionary<string, FileBrowserSourceEntry[]> _entries;
        private readonly HashSet<string> _failures;

        public FakeSource(
            IReadOnlyDictionary<string, FileBrowserSourceEntry[]> entries,
            bool isCaseSensitive = true, IEnumerable<string>? failures = null)
        {
            _entries = entries;
            IsCaseSensitive = isCaseSensitive;
            _failures = new HashSet<string>(failures ?? []);
        }

        public string Id => "mock";
        public bool IsCaseSensitive { get; }
        public List<string> Listed { get; } = [];

        public Task<IReadOnlyList<FileBrowserSourceEntry>> ListAsync(
            FileBrowserLocation directory, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Listed.Add(directory.Value);
            if (_failures.Contains(directory.Value))
                throw new UnauthorizedAccessException("Test-only directory access denied");

            if (!_entries.TryGetValue(directory.Value, out var entries))
                return Task.FromResult<IReadOnlyList<FileBrowserSourceEntry>>([]);

            return Task.FromResult<IReadOnlyList<FileBrowserSourceEntry>>(entries);
        }

        public Task<FileBrowserLocation?> GetParentAsync(
            FileBrowserLocation directory, CancellationToken cancellationToken = default)
            => Task.FromResult<FileBrowserLocation?>(null);
    }
}
