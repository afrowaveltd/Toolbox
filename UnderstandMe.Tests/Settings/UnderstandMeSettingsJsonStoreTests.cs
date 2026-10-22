using Afrowave.Toolbox.UnderstandMe.Settings;
using Xunit;

namespace Afrowave.Toolbox.UnderstandMe.Tests.Settings;

public sealed class UnderstandMeSettingsJsonStoreTests
{
    [Fact]
    public void Create_load_and_revision_checked_update()
    {
        using var files = new TempFile();
        var initial = new UnderstandMeSettings();
        var first = UnderstandMeSettingsJsonStore.Save(files.Path, initial);
        Assert.Equal(first, UnderstandMeSettingsJsonStore.Load(files.Path).Revision);
        var changed = new UnderstandMeSettings
        {
            Localization = new LocalizationSettings { DefaultSourceLanguage = "cs" }
        };
        var second = UnderstandMeSettingsJsonStore.Save(files.Path, changed, first);
        Assert.NotEqual(first, second);
        Assert.Equal("cs", UnderstandMeSettingsJsonStore.Load(files.Path).Settings.Localization.DefaultSourceLanguage);
        Assert.Throws<UnderstandMeSettingsConflictException>(() => UnderstandMeSettingsJsonStore.Save(files.Path, initial, first));
    }

    [Fact]
    public void Create_only_never_overwrites_existing_file()
    {
        using var files = new TempFile();
        UnderstandMeSettingsJsonStore.Save(files.Path, new());
        var before = File.ReadAllBytes(files.Path);
        Assert.Throws<UnderstandMeSettingsConflictException>(() => UnderstandMeSettingsJsonStore.Save(files.Path, new()));
        Assert.Equal(before, File.ReadAllBytes(files.Path));
    }

    [Fact]
    public void Invalid_candidate_does_not_modify_saved_file()
    {
        using var files = new TempFile();
        var rev = UnderstandMeSettingsJsonStore.Save(files.Path, new());
        var invalid = new UnderstandMeSettings { Translation = new TranslationSettings { Queues = new() { DefaultWorkerBufferCapacity = 0 } } };
        var error = Assert.Throws<UnderstandMeSettingsValidationException>(() => UnderstandMeSettingsJsonStore.Save(files.Path, invalid, rev));
        Assert.Contains(error.Issues, x => x.Path.Contains("DefaultWorkerBufferCapacity"));
        Assert.Equal(rev, UnderstandMeSettingsJsonStore.Load(files.Path).Revision);
    }

    [Fact]
    public void Unsupported_schema_and_unknown_fields_are_rejected()
    {
        using var files = new TempFile();
        File.WriteAllText(files.Path, "{\"SchemaVersion\":2}");
        Assert.Throws<UnderstandMeSettingsValidationException>(() => UnderstandMeSettingsJsonStore.Load(files.Path));
        File.WriteAllText(files.Path, "{\"SchemaVersion\":1,\"FutureProperty\":true}");
        Assert.Throws<System.Text.Json.JsonException>(() => UnderstandMeSettingsJsonStore.Load(files.Path));
    }

    [Fact]
    public void Malformed_document_does_not_get_replaced_implicitly()
    {
        using var files = new TempFile();
        File.WriteAllText(files.Path, "{");
        Assert.Throws<System.Text.Json.JsonException>(() => UnderstandMeSettingsJsonStore.Load(files.Path));
        Assert.Equal("{", File.ReadAllText(files.Path));
    }

    private sealed class TempFile : IDisposable
    {
        private readonly string folder = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "understandme-settings-" + Guid.NewGuid().ToString("N"));
        public string Path => System.IO.Path.Combine(folder, "UnderstandMeSettings.json");
        public TempFile() => Directory.CreateDirectory(folder);
        public void Dispose()
        {
            if (Directory.Exists(folder))
                Directory.Delete(folder, true);
        }
    }
}
