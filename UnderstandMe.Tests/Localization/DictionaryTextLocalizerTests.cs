using System.Text.Json;
using Afrowave.Toolbox.UnderstandMe.Localization;
using Xunit;

namespace Afrowave.Toolbox.UnderstandMe.Tests.Localization;

public sealed class DictionaryTextLocalizerTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "UnderstandMe-" + Guid.NewGuid().ToString("N"));
    private FileJsonDictionaryStore Store => new(_root);

    private DictionaryTextLocalizer Create(bool registerMissing = true) =>
        new(Store, new LocalizationOptions { SourceCulture = "en", RegisterMissingSourceKeys = registerMissing });

    [Fact]
    public void Dictionaryless_start_registers_missing_phrase()
    {
        Assert.Equal("Hello there", Create().Get("Hello there", "sw"));
        using var json = JsonDocument.Parse(File.ReadAllText(Path.Combine(_root, "en.json")));
        Assert.Equal("Hello there", json.RootElement.GetProperty("Hello there").GetString());
    }

    [Fact]
    public void Target_translation_has_precedence()
    {
        Write("en", """{"Hello":"Hello"}""");
        Write("cs", """{"Hello":"Ahoj"}""");
        Assert.Equal("Ahoj", Create().Get("Hello", "cs"));
    }

    [Fact]
    public void Region_falls_back_to_parent_then_source()
    {
        Write("en", """{"Hello":"Hello","OnlySource":"Source text"}""");
        Write("cs", """{"Hello":"Ahoj"}""");
        var t = Create();
        Assert.Equal("Ahoj", t.Get("Hello", "cs-CZ"));
        Assert.Equal("Source text", t.Get("OnlySource", "cs-CZ"));
    }

    [Fact]
    public void Source_dictionary_is_not_overwritten()
    {
        Write("en", """{"Hello":"Preferred English"}""");
        var t = Create();
        Assert.Equal("Preferred English", t.Get("Hello", "sw"));
        Assert.Equal("New phrase", t.Get("New phrase", "sw"));
        Assert.Equal("New phrase", t.Get("New phrase", "sw"));
        using var json = JsonDocument.Parse(File.ReadAllText(Path.Combine(_root, "en.json")));
        Assert.Equal(2, json.RootElement.EnumerateObject().Count());
        Assert.Equal("Preferred English", json.RootElement.GetProperty("Hello").GetString());
    }

    [Fact]
    public void Can_disable_registration()
    {
        Assert.Equal("Missing", Create(false).Get("Missing", "sw"));
        Assert.False(File.Exists(Path.Combine(_root, "en.json")));
    }

    [Fact]
    public void Named_placeholders_do_not_modify_catalog()
    {
        Write("cs", """{"Hello {name} from {city}":"Ahoj {name} z {city}!"}""");
        var t = Create();
        Assert.Equal("Ahoj Bolaji z Lagos!",
            t.Format("Hello {name} from {city}", new Dictionary<string, string>
            { ["name"] = "Bolaji", ["city"] = "Lagos" }, "cs"));
        Assert.Contains("{name}", File.ReadAllText(Path.Combine(_root, "cs.json")));
    }

    [Fact]
    public void Missing_placeholders_and_recursive_values_are_safe()
    {
        Assert.Equal("Hello {unknown} {name}",
            Create(false).Format("Hello {unknown} {name}",
                new Dictionary<string, string> { ["name"] = "{name}" }, "en"));
    }

    [Fact]
    public void Double_braces_remain_literal()
    {
        Assert.Equal("{{name}} Ada",
            Create(false).Format("{{name}} {name}", new Dictionary<string, string> { ["name"] = "Ada" }, "en"));
    }

    [Fact]
    public void Explicit_reload_updates_existing_instance()
    {
        Write("cs", """{"Hello":"Ahoj"}""");
        var t = Create();
        Assert.Equal("Ahoj", t.Get("Hello", "cs"));
        Write("cs", """{"Hello":"Nazdar"}""");
        Assert.Equal("Ahoj", t.Get("Hello", "cs"));
        Assert.True(t.Reload("cs"));
        Assert.Equal("Nazdar", t.Get("Hello", "cs"));
    }

    [Fact]
    public void Invalid_reload_preserves_valid_snapshot()
    {
        Write("cs", """{"Hello":"Ahoj"}""");
        var t = Create();
        Assert.Equal("Ahoj", t.Get("Hello", "cs"));
        Write("cs", """{"Hello":12}""");
        Assert.False(t.Reload("cs"));
        Assert.Equal("Ahoj", t.Get("Hello", "cs"));
    }

    [Fact]
    public void Broken_source_file_is_not_destroyed()
    {
        Write("en", """{"Old":null}""");
        Assert.Equal("Unknown", Create().Get("Unknown", "sw"));
        Assert.Equal("""{"Old":null}""", File.ReadAllText(Path.Combine(_root, "en.json")));
    }

    [Fact]
    public void Invalid_culture_paths_are_rejected()
    {
        Assert.Throws<ArgumentException>(() => Store.Load("../other"));
        Assert.Throws<ArgumentException>(() => Store.TryAdd("sw/UG", "key", "value"));
    }

    [Fact]
    public void Duplicate_keys_are_rejected()
    {
        Write("cs", """{"A":"a","A":"b"}""");
        Assert.Throws<JsonException>(() => Store.Load("cs"));
    }

    [Fact]
    public void Concurrent_registration_does_not_lose_keys()
    {
        var t = Create();
        Parallel.For(0, 25, i => t.Get("Key " + i, "sw"));
        using var json = JsonDocument.Parse(File.ReadAllText(Path.Combine(_root, "en.json")));
        Assert.Equal(25, json.RootElement.EnumerateObject().Count());
    }


    [Fact]
    public void Memory_store_works_without_json_files_or_translation_engines()
    {
        var store = new MemoryStore();
        store.Set("cs", "Welcome", "Vítej");
        var t = new DictionaryTextLocalizer(store);
        Assert.Equal("Vítej", t.Get("Welcome", "cs"));
        Assert.Equal("New phrase", t.Get("New phrase", "sw"));
        Assert.Equal("New phrase", store.Load("en")["New phrase"]);
        store.Set("cs", "Welcome", "Ahoj");
        Assert.Equal("Vítej", t.Get("Welcome", "cs"));
        Assert.True(t.Reload("cs"));
        Assert.Equal("Ahoj", t.Get("Welcome", "cs"));
    }


    [Theory]
    [InlineData("I have {count} kids", "Mám děti")]
    [InlineData("I have {count} kids", "Mám {count} a {count} dětí")]
    [InlineData("I have {count} kids", "Mám {number} dětí")]
    [InlineData("I have {count} kids", "Mám {count dětí")]
    [InlineData("I have {count} kids", "Mám {count}} dětí")]
    public void Invalid_target_placeholder_is_rejected_and_source_values_survive(
        string source, string invalidTranslation)
    {
        Write("cs", JsonSerializer.Serialize(new Dictionary<string, string>
        {
            [source] = invalidTranslation
        }));

        var localizer = Create(registerMissing: false);
        Assert.Equal(source, localizer.Get(source, "cs"));
        Assert.Equal("I have 3 kids", localizer.Format(source,
            new Dictionary<string, string> { ["count"] = "3" }, "cs"));

        // Validation must not delete the bad candidate or mutate the source store.
        Assert.Equal(invalidTranslation, Store.Load("cs")[source]);
        Assert.False(File.Exists(Path.Combine(_root, "en.json")));
    }

    [Fact]
    public void Invalid_region_translation_falls_back_to_valid_parent_translation()
    {
        Write("en", """{"Hello {name}":"Hello {name}"}""");
        Write("cs-CZ", """{"Hello {name}":"Ahoj"}""");
        Write("cs", """{"Hello {name}":"Nazdar {name}"}""");

        Assert.Equal("Nazdar Ada", Create().Format("Hello {name}",
            new Dictionary<string, string> { ["name"] = "Ada" }, "cs-CZ"));
    }

    [Fact]
    public void Symbolic_key_uses_source_dictionary_template_for_validation_and_fallback()
    {
        Write("en", """{"Kids":"I have {count} kids"}""");
        Write("cs", """{"Kids":"Mám děti"}""");

        var localizer = Create();
        Assert.Equal("I have {count} kids", localizer.Get("Kids", "cs"));
        Assert.Equal("I have 3 kids", localizer.Format("Kids",
            new Dictionary<string, string> { ["count"] = "3" }, "cs"));
    }

    [Fact]
    public void Target_with_reordered_placeholders_is_valid()
    {
        Write("cs", """{"{name} has {count} items":"{count} položek má {name}"}""");

        Assert.Equal("3 položek má Ada", Create().Format("{name} has {count} items",
            new Dictionary<string, string> { ["name"] = "Ada", ["count"] = "3" }, "cs"));
    }

    [Fact]
    public void Strict_runtime_validation_can_be_explicitly_disabled_for_legacy_data()
    {
        Write("cs", """{"Hello {name}":"Ahoj {name} z {city}"}""");
        var localizer = new DictionaryTextLocalizer(Store, new LocalizationOptions
        {
            SourceCulture = "en",
            RegisterMissingSourceKeys = false,
            ValidateLocalizedPlaceholders = false
        });

        Assert.Equal("Ahoj Ada z Lagos", localizer.Format("Hello {name}",
            new Dictionary<string, string> { ["name"] = "Ada", ["city"] = "Lagos" }, "cs"));
    }

    [Fact]
    public void Translation_without_placeholders_may_still_be_valid()
    {
        Write("cs", """{"Hello there":"Dobrý den"}""");
        Assert.Equal("Dobrý den", Create().Get("Hello there", "cs"));
    }

    private sealed class MemoryStore : ILocalizationDictionaryStore
    {
        private readonly Dictionary<string, Dictionary<string, string>> _data =
            new(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyDictionary<string, string> Load(string culture) =>
            _data.TryGetValue(culture, out var values)
                ? new Dictionary<string, string>(values, StringComparer.Ordinal)
                : new Dictionary<string, string>(StringComparer.Ordinal);

        public bool TryAdd(string culture, string key, string value)
        {
            if (!_data.TryGetValue(culture, out var values))
                _data[culture] = values = new Dictionary<string, string>(StringComparer.Ordinal);
            return values.TryAdd(key, value);
        }

        public void Set(string culture, string key, string value)
        {
            if (!_data.TryGetValue(culture, out var values))
                _data[culture] = values = new Dictionary<string, string>(StringComparer.Ordinal);
            values[key] = value;
        }
    }

    private void Write(string culture, string json)
    {
        Directory.CreateDirectory(_root);
        File.WriteAllText(Path.Combine(_root, culture + ".json"), json);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }
}
