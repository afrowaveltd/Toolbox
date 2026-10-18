using System.Text.Json;
using Afrowave.Toolbox.UnderstandMe.Localization;
using Xunit;

namespace Afrowave.Toolbox.UnderstandMe.Tests.Localization;

public sealed class JsonTextLocalizerTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "UnderstandMe-" + Guid.NewGuid().ToString("N"));
    private FileJsonDictionaryStore Store => new(_root);

    private JsonTextLocalizer Create(bool registerMissing = true) =>
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
        Write("cs", """{"Hello {name}":"Ahoj {name} z {city}!"}""");
        var t = Create();
        Assert.Equal("Ahoj Bolaji z Lagos!",
            t.Format("Hello {name}", new Dictionary<string, string>
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
