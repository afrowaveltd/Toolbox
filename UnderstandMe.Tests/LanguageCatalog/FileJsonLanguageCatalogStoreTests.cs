using System.Text.Json;
using Afrowave.Toolbox.UnderstandMe.LanguageCatalog;
using Xunit;

namespace Afrowave.Toolbox.UnderstandMe.Tests.LanguageCatalog;

public sealed class FileJsonLanguageCatalogStoreTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "LanguageCatalog-" + Guid.NewGuid().ToString("N"));
    private string FilePath => Path.Combine(_folder, "languages.json");
    private FileJsonLanguageCatalogStore Store => new(FilePath);

    [Fact]
    public void Absent_catalog_is_empty_and_editable()
    {
        ILanguageCatalogReader reader = Store;
        Assert.Empty(reader.GetAll());
        Assert.Null(reader.Find("lg"));
        Assert.False(File.Exists(FilePath));
        Assert.True(((ILanguageCatalogEditor)Store).Upsert(new LanguageCatalogEntry
        { Code = "lg", Name = "Ganda", NativeName = "Luganda" }));
        Assert.Equal("Luganda", reader.Find("lg")?.NativeName);
    }

    [Fact]
    public void Reads_existing_schema_with_endonyms_and_rtl()
    {
        Write("""[{"code":"en","name":"English","native":"English"},{"code":"ar","name":"Arabic","native":"العربية","rtl":true}]""");
        var items = Store.GetAll();
        Assert.Equal(2, items.Count);
        Assert.True(Store.Find("AR")!.RightToLeft);
        Assert.Equal("العربية", Store.Find("ar")!.NativeName);
        Assert.Null(Store.Find("en")!.RightToLeft);
    }

    [Fact]
    public void Upsert_preserves_custom_fields_and_sorts_alphabetically()
    {
        Write("""[{"code":"sw","name":"Swahili","native":"Kiswahili","custom":{"origin":"manual"}},{"code":"en","name":"English","native":"English"}]""");
        Assert.True(Store.Upsert(new LanguageCatalogEntry { Code = "SW", Name = "Swahili", NativeName = "Kiswahili (updated)" }));
        Assert.True(Store.Upsert(new LanguageCatalogEntry { Code = "cs", Name = "Czech", NativeName = "Česky" }));
        using var doc = JsonDocument.Parse(File.ReadAllText(FilePath));
        var all = doc.RootElement.EnumerateArray().ToArray();
        Assert.Equal(new[] { "cs", "en", "sw" }, all.Select(x => x.GetProperty("code").GetString()));
        Assert.Equal("manual", all[2].GetProperty("custom").GetProperty("origin").GetString());
        Assert.Equal("Kiswahili (updated)", all[2].GetProperty("native").GetString());
    }

    [Fact]
    public void Removing_a_language_is_explicit_and_does_not_modify_other_entries()
    {
        Write("""[{"code":"en","name":"English","native":"English"},{"code":"fr","name":"French","native":"Français"}]""");
        Assert.True(Store.Remove("FR"));
        Assert.False(Store.Remove("fr"));
        Assert.Null(Store.Find("fr"));
        Assert.Equal("English", Store.Find("en")!.NativeName);
    }

    [Fact]
    public void Repeated_identical_upsert_has_no_write()
    {
        var e = new LanguageCatalogEntry { Code = "ha-Latn", Name = "Hausa (Latin)", NativeName = "Hausa", RightToLeft = false };
        Assert.True(Store.Upsert(e));
        string original = File.ReadAllText(FilePath);
        Assert.False(Store.Upsert(e));
        Assert.Equal(original, File.ReadAllText(FilePath));
    }

    [Fact]
    public void Duplicate_codes_case_insensitively_are_rejected_without_mutation()
    {
        const string json = """[{"code":"en","name":"English","native":"English"},{"code":"EN","name":"English","native":"English"}]""";
        Write(json);
        Assert.Throws<JsonException>(() => Store.GetAll());
        Assert.Throws<JsonException>(() => Store.Upsert(new LanguageCatalogEntry { Code = "fr", Name = "French", NativeName = "Français" }));
        Assert.Equal(json, File.ReadAllText(FilePath));
    }

    [Fact]
    public void Malformed_catalog_is_never_overwritten()
    {
        Write("""[{"code":"en","name":"English","native":4}]""");
        string before = File.ReadAllText(FilePath);
        Assert.Throws<JsonException>(() => Store.Upsert(new LanguageCatalogEntry { Code = "sw", Name = "Swahili", NativeName = "Kiswahili" }));
        Assert.Equal(before, File.ReadAllText(FilePath));
    }

    [Fact]
    public void Rejects_unsafe_identifiers_and_invalid_rtl()
    {
        Assert.Throws<ArgumentException>(() => Store.Remove("../cs"));
        Assert.Throws<ArgumentException>(() => Store.Upsert(new LanguageCatalogEntry
        { Code = "../cs", Name = "Invalid", NativeName = "Invalid" }));
        Write("""[{"code":"ar","name":"Arabic","native":"العربية","rtl":"yes"}]""");
        Assert.Throws<JsonException>(() => Store.GetAll());
    }

    [Fact]
    public void A_single_store_instance_serializes_concurrent_writes()
    {
        var store = Store;
        Parallel.For(0, 30, i => store.Upsert(new LanguageCatalogEntry
        { Code = "x-" + i.ToString("D2"), Name = "Language " + i, NativeName = "Language " + i }));
        Assert.Equal(30, store.GetAll().Count);
    }

    private void Write(string content)
    {
        Directory.CreateDirectory(_folder);
        File.WriteAllText(FilePath, content);
    }

    public void Dispose()
    {
        if (Directory.Exists(_folder))
            Directory.Delete(_folder, recursive: true);
    }
}
