using Afrowave.Toolbox.UnderstandMe.Localization;
using Xunit;

namespace Afrowave.Toolbox.UnderstandMe.Tests.Localization;

public sealed class LocalizationTranslationScannerTests
{
    [Fact]
    public void Detects_only_missing_and_invalid_exact_target_entries_without_modifying_store()
    {
        var store = new SnapshotStore();
        store.Put("en", "welcome", "Hello {name}");
        store.Put("en", "bye", "Goodbye");
        store.Put("en", "total", "Total {count}");
        store.Put("en", "bad-source", "Invalid {");
        store.Put("cs", "welcome", "Ahoj {name}");
        store.Put("cs", "total", "Celkem {amount}");
        var result = LocalizationTranslationScanner.Scan(store, "en", "cs");
        Assert.Equal(2, result.Count);
        Assert.Equal(new LocalizationRepairItem("bye", "Goodbye", LocalizationRepairReason.MissingTarget), result[0]);
        Assert.Equal(new LocalizationRepairItem("total", "Total {count}", LocalizationRepairReason.InvalidTargetPlaceholders), result[1]);
        Assert.Equal("Celkem {amount}", store.Load("cs")["total"]);
        Assert.Equal(4, store.Load("en").Count);
    }

    [Fact]
    public void Parent_fallback_does_not_hide_exact_target_gap()
    {
        var store = new SnapshotStore();
        store.Put("en", "greeting", "Hello {name}");
        store.Put("pt", "greeting", "Olá {name}");
        var item = Assert.Single(LocalizationTranslationScanner.Scan(store, "en", "pt-BR"));
        Assert.Equal(LocalizationRepairReason.MissingTarget, item.Reason);
    }

    [Fact]
    public void Same_source_and_target_need_no_repair()
    {
        var store = new SnapshotStore();
        store.Put("en", "x", "Hello");
        Assert.Empty(LocalizationTranslationScanner.Scan(store, " en ", "EN"));
    }

    [Fact]
    public void Empty_source_does_not_invent_dictionaryless_keys()
    {
        Assert.Empty(LocalizationTranslationScanner.Scan(new SnapshotStore(), "en", "cs"));
    }

    [Fact]
    public void Invalid_input_is_rejected()
    {
        Assert.Throws<ArgumentNullException>(() => LocalizationTranslationScanner.Scan(null!, "en", "cs"));
        Assert.Throws<ArgumentException>(() => LocalizationTranslationScanner.Scan(new SnapshotStore(), "", "cs"));
        Assert.Throws<ArgumentException>(() => LocalizationTranslationScanner.Scan(new SnapshotStore(), "en", " "));
    }

    private sealed class SnapshotStore : ILocalizationDictionaryStore
    {
        private readonly Dictionary<string, Dictionary<string, string>> _data = new(StringComparer.OrdinalIgnoreCase);
        public void Put(string culture, string key, string value)
        {
            if (!_data.TryGetValue(culture, out var entries))
                _data[culture] = entries = new(StringComparer.Ordinal);
            entries[key] = value;
        }
        public IReadOnlyDictionary<string, string> Load(string culture) =>
            _data.TryGetValue(culture, out var entries)
                ? new Dictionary<string, string>(entries, StringComparer.Ordinal)
                : new Dictionary<string, string>(StringComparer.Ordinal);
        public bool TryAdd(string culture, string key, string value)
        {
            if (!_data.ContainsKey(culture))
                _data[culture] = new(StringComparer.Ordinal);
            return _data[culture].TryAdd(key, value);
        }
    }
}
