using Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser.Tests;

public sealed class FileBrowserNameMatcherTests
{
    [Theory]
    [InlineData("*.ajis;*.json", "dictionary.ajis", true)]
    [InlineData("*.ajis;*.json", "settings.JSON", true)]
    [InlineData("*.ajis;*.json", "photo.png", false)]
    [InlineData("*", "anything", true)]
    [InlineData("README", "readme", true)]
    [InlineData("config?.xml", "config1.xml", true)]
    [InlineData("*.tar.gz;*.zip", "backup.tar.gz", true)]
    public void Multi_masks_support_mixed_extensions_and_exact_names(
        string pattern, string file, bool expected)
    {
        Assert.Equal(expected, new FileBrowserNameMatcher(pattern).Matches(file));
    }

    [Fact]
    public void Explicit_case_sensitivity_is_respected()
    {
        Assert.False(new FileBrowserNameMatcher("*.json", ignoreCase: false)
            .Matches("SETTINGS.JSON"));
        Assert.True(new FileBrowserNameMatcher("*.json", ignoreCase: true)
            .Matches("SETTINGS.JSON"));
    }

    [Fact]
    public void Regex_can_express_alternative_extensions_without_glob_translation()
    {
        var matcher = new FileBrowserNameMatcher(
            @"\.(ajis|json)$", FileBrowserPatternMode.RegularExpression);
        Assert.True(matcher.Matches("settings.AJIS"));
        Assert.True(matcher.Matches("settings.json"));
        Assert.False(matcher.Matches("notes.txt"));
    }

    [Fact]
    public void Regex_respects_case_settings()
    {
        var matcher = new FileBrowserNameMatcher(
            @"\.json$", FileBrowserPatternMode.RegularExpression, ignoreCase: false);
        Assert.True(matcher.Matches("example.json"));
        Assert.False(matcher.Matches("example.JSON"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("*.json;")]
    [InlineData(";*.json")]
    [InlineData("*.json;;*.ajis")]
    [InlineData("folder/*.json")]
    [InlineData(@"folder\*.json")]
    public void Invalid_wildcard_patterns_are_rejected(string pattern)
    {
        Assert.Throws<ArgumentException>(() => new FileBrowserNameMatcher(pattern));
    }

    [Fact]
    public void Regex_syntax_and_timeout_are_checked()
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            new FileBrowserNameMatcher("(", FileBrowserPatternMode.RegularExpression));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new FileBrowserNameMatcher(".*", FileBrowserPatternMode.RegularExpression,
                regexTimeout: TimeSpan.Zero));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new FileBrowserNameMatcher(".*", FileBrowserPatternMode.RegularExpression,
                regexTimeout: TimeSpan.FromSeconds(10)));
    }

    [Fact]
    public void Regex_does_not_allow_catastrophic_unbounded_evaluation()
    {
        var matcher = new FileBrowserNameMatcher(
            @"^(a+)+$", FileBrowserPatternMode.RegularExpression,
            regexTimeout: TimeSpan.FromMilliseconds(1));
        Assert.Throws<System.Text.RegularExpressions.RegexMatchTimeoutException>(
            () => matcher.Matches(new string('a', 3000) + "!"));
    }

    [Fact]
    public void Listing_filter_uses_same_custom_multi_mask_and_keeps_navigation_folders()
    {
        var filter = new FileBrowserFileFilter(
            nameMatcher: new FileBrowserNameMatcher("*.ajis;*.json"));
        Assert.True(filter.Allows("dictionary.ajis", false));
        Assert.True(filter.Allows("config.json", false));
        Assert.False(filter.Allows("readme.md", false));
        Assert.True(filter.Allows("other.folder", true));
    }

    [Fact]
    public void Listing_filter_can_apply_regex_and_semantic_category_together()
    {
        var filter = new FileBrowserFileFilter(
            includedKinds: [FileBrowserFileKind.Json],
            nameMatcher: new FileBrowserNameMatcher(
                @"^config.*\.json$", FileBrowserPatternMode.RegularExpression));
        Assert.True(filter.Allows("config.ONE.json", false));
        Assert.False(filter.Allows("unrelated.json", false));
        Assert.False(filter.Allows("config.one.txt", false));
    }

    [Fact]
    public async Task Recursive_search_returns_both_ajis_and_json_via_one_mask_list()
    {
        var source = new MockSource();
        var result = await new FileBrowserRecursiveSearch(source).FindAsync(
            new FileBrowserLocation("mock", "/"), "*.ajis;*.json");

        Assert.Equal(2, result.Matches.Count);
        Assert.Equal(["strings.ajis", "settings.json"],
            result.Matches.Select(m => m.Entry.Name).ToArray());
    }

    [Fact]
    public async Task Recursive_search_supports_explicit_regex_mode()
    {
        var source = new MockSource();
        var options = new FileBrowserSearchOptions
        {
            PatternMode = FileBrowserPatternMode.RegularExpression
        };
        var result = await new FileBrowserRecursiveSearch(source).FindAsync(
            new FileBrowserLocation("mock", "/"), @"\.(json|ajis)$", options);
        Assert.Equal(2, result.Matches.Count);
    }

    private sealed class MockSource : IFileBrowserSource
    {
        public string Id => "mock";
        public bool IsCaseSensitive => false;

        public Task<IReadOnlyList<FileBrowserSourceEntry>> ListAsync(
            FileBrowserLocation directory, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<FileBrowserSourceEntry> entries =
            [
                new("strings.ajis", new FileBrowserLocation(Id, "/strings.ajis"), false),
                new("settings.json", new FileBrowserLocation(Id, "/settings.json"), false),
                new("song.mp3", new FileBrowserLocation(Id, "/song.mp3"), false)
            ];
            return Task.FromResult(entries);
        }

        public Task<FileBrowserLocation?> GetParentAsync(
            FileBrowserLocation directory, CancellationToken cancellationToken = default)
            => Task.FromResult<FileBrowserLocation?>(null);
    }
}
