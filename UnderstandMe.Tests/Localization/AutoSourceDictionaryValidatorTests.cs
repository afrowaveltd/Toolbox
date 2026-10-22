using Afrowave.Toolbox.UnderstandMe.Localization;
using Xunit;

namespace Afrowave.Toolbox.UnderstandMe.Tests.Localization;

public sealed class AutoSourceDictionaryValidatorTests
{
    [Fact]
    public void Removes_only_inconsistent_source_entries_and_keeps_targets()
    {
        using var fixture = new Fixture();
        fixture.Store.TryAdd("en", "Hello", "Helo");
        fixture.Store.TryAdd("en", "Goodbye", "Goodbye");
        fixture.Store.TryAdd("cs", "Hello", "Ahoj");
        var result = AutoSourceDictionaryValidator.ValidateAndPrune(fixture.Store, "en");
        Assert.Equal(new AutoSourceDictionaryValidationResult(2, 1, 1, 0), result);
        Assert.False(fixture.Store.Load("en").ContainsKey("Hello"));
        Assert.Equal("Goodbye", fixture.Store.Load("en")["Goodbye"]);
        Assert.Equal("Ahoj", fixture.Store.Load("cs")["Hello"]);
        Assert.True(fixture.Store.TryAdd("en", "Hello", "Hello"));
    }

    [Fact]
    public void Json_escaped_and_literal_unicode_and_angle_brackets_are_equivalent()
    {
        using var fixture = new Fixture();
        Directory.CreateDirectory(fixture.DirectoryPath);
        File.WriteAllText(Path.Combine(fixture.DirectoryPath, "cs.json"),
            "{\"\\u010Desk\\u00FD\":\"český\",\"<tag>\":\"\\u003Ctag\\u003E\"}");
        var result = AutoSourceDictionaryValidator.ValidateAndPrune(fixture.Store, "cs");
        Assert.Equal(new AutoSourceDictionaryValidationResult(2, 0, 0, 0), result);
    }

    [Fact]
    public void Read_only_store_reports_inconsistency_without_mutating()
    {
        var store = new ReadOnlyStore();
        var result = AutoSourceDictionaryValidator.ValidateAndPrune(store, "en");
        Assert.Equal(new AutoSourceDictionaryValidationResult(1, 1, 0, 1), result);
        Assert.Equal("wrong", store.Load("en")["key"]);
    }

    [Fact]
    public void Repeated_scan_is_idempotent_and_empty_source_is_valid()
    {
        using var fixture = new Fixture();
        fixture.Store.TryAdd("en", "a", "b");
        Assert.Equal(1, AutoSourceDictionaryValidator.ValidateAndPrune(fixture.Store, "en").Removed);
        Assert.Equal(new AutoSourceDictionaryValidationResult(0, 0, 0, 0),
            AutoSourceDictionaryValidator.ValidateAndPrune(fixture.Store, "en"));
    }

    [Fact]
    public void Invalid_arguments_are_rejected()
    {
        Assert.Throws<ArgumentNullException>(() => AutoSourceDictionaryValidator.ValidateAndPrune(null!, "en"));
        using var fixture = new Fixture();
        Assert.Throws<ArgumentException>(() => AutoSourceDictionaryValidator.ValidateAndPrune(fixture.Store, " "));
    }

    private sealed class ReadOnlyStore : ILocalizationDictionaryStore
    {
        public IReadOnlyDictionary<string, string> Load(string culture) => new Dictionary<string, string> { ["key"] = "wrong" };
        public bool TryAdd(string culture, string key, string value) => false;
    }

    private sealed class Fixture : IDisposable
    {
        public string DirectoryPath { get; } = Path.Combine(Path.GetTempPath(), "understandme-source-" + Guid.NewGuid().ToString("N"));
        public FileJsonDictionaryStore Store { get; }
        public Fixture() => Store = new FileJsonDictionaryStore(DirectoryPath);
        public void Dispose()
        {
            if (Directory.Exists(DirectoryPath))
                Directory.Delete(DirectoryPath, true);
        }
    }
}
