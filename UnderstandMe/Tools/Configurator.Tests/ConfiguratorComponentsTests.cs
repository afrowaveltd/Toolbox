using System.Text.Json;
using Afrowave.Toolbox.UnderstandMe.Configurator;
using Afrowave.Toolbox.UnderstandMe.Settings;
using Xunit;

namespace Afrowave.Toolbox.UnderstandMe.Configurator.Tests;

public sealed class ConfiguratorComponentsTests
{
    [Fact]
    public void Adding_both_types_uses_descriptor_defaults_without_writing_to_disk()
    {
        using var folder = new TempFolder();
        var session = ConfiguratorSession.CreateNew(folder.Path);
        Assert.Contains(ConfiguratorSession.GetAvailableTypes(SettingsComponentKind.TranslationBackend),
            x => x.Type == "LibreTranslate");
        Assert.Contains(ConfiguratorSession.GetAvailableTypes(SettingsComponentKind.LocalizationStorage),
            x => x.Type == "FileJson");

        session.AddComponent(SettingsComponentKind.TranslationBackend, "Libre", "LibreTranslate");
        session.AddComponent(SettingsComponentKind.LocalizationStorage, "local", "FileJson");

        Assert.False(File.Exists(folder.Path));
        Assert.True(session.IsDirty);
        Assert.Empty(session.Validate());
        Assert.Single(session.ListComponents(SettingsComponentKind.TranslationBackend));
        Assert.Single(session.ListComponents(SettingsComponentKind.LocalizationStorage));
        Assert.Equal("http://localhost:5000",
            session.GetComponent(SettingsComponentKind.TranslationBackend, "Libre").Options["Endpoint"].GetString());
        Assert.Equal("Locales",
            session.GetComponent(SettingsComponentKind.LocalizationStorage, "local").Options["Directory"].GetString());

        session.Save();
        Assert.False(session.IsDirty);
        var loaded = UnderstandMeSettingsJsonStore.Load(folder.Path).Settings;
        Assert.Equal("Libre", Assert.Single(loaded.Translation.Providers).Id);
        Assert.Equal("local", Assert.Single(loaded.Localization.Storages).Id);
    }

    [Fact]
    public void Changing_option_marks_existing_document_dirty_and_persists_when_confirmed()
    {
        using var folder = new TempFolder();
        UnderstandMeSettingsJsonStore.Save(folder.Path, new UnderstandMeSettings());
        var session = ConfiguratorSession.Open(folder.Path);
        Assert.False(session.IsDirty);
        var before = File.ReadAllBytes(folder.Path);

        session.AddComponent(SettingsComponentKind.TranslationBackend, "live", "LibreTranslate");
        Assert.True(session.IsDirty);
        session.SetOption(SettingsComponentKind.TranslationBackend, "live", "Endpoint",
            JsonSerializer.SerializeToElement("https://localhost:5000"));
        session.SetProviderEnabled("live", false);

        Assert.True(session.IsDirty);
        Assert.Equal(before, File.ReadAllBytes(folder.Path));
        Assert.Empty(session.Validate());
        session.Save();
        Assert.False(session.IsDirty);
        var reloaded = ConfiguratorSession.Open(folder.Path);
        Assert.False(reloaded.IsDirty);
        Assert.False(Assert.Single(reloaded.Draft.Translation.Providers).Enabled);
        Assert.Equal("https://localhost:5000",
            reloaded.GetComponent(SettingsComponentKind.TranslationBackend, "live").Options["Endpoint"].GetString());
    }

    [Fact]
    public void Invalid_field_values_and_duplicate_ids_do_not_mutate_the_draft()
    {
        using var folder = new TempFolder();
        var session = ConfiguratorSession.CreateNew(folder.Path);
        var kind = SettingsComponentKind.TranslationBackend;
        session.AddComponent(kind, "abc", "LibreTranslate");
        var before = JsonSerializer.Serialize(session.Draft);

        Assert.Throws<ArgumentException>(() =>
            session.AddComponent(kind, "ABC", "LibreTranslate"));
        Assert.Throws<ArgumentException>(() =>
            session.AddComponent(kind, "new", "Nonexistent"));
        Assert.Throws<ArgumentException>(() =>
            session.SetOption(kind, "abc", "Endpoint", JsonSerializer.SerializeToElement("ftp://invalid")));
        Assert.Throws<ArgumentException>(() =>
            session.SetOption(kind, "abc", "MaxConcurrentRequests", JsonSerializer.SerializeToElement(0)));
        Assert.Throws<ArgumentException>(() =>
            session.SetOption(kind, "abc", "NotRegistered", JsonSerializer.SerializeToElement("unexpected")));

        Assert.Equal(before, JsonSerializer.Serialize(session.Draft));
    }

    [Fact]
    public void Conditional_secret_reference_is_enforced_without_storing_literal_key()
    {
        using var folder = new TempFolder();
        var session = ConfiguratorSession.CreateNew(folder.Path);
        var kind = SettingsComponentKind.TranslationBackend;
        session.AddComponent(kind, "cloud", "LibreTranslate");

        session.SetOption(kind, "cloud", "AuthenticationMode", JsonSerializer.SerializeToElement("ApiKey"));
        Assert.Contains(session.Validate(), issue => issue.Path.EndsWith(".ApiKeyReference"));
        Assert.Throws<ArgumentException>(() =>
            session.SetOption(kind, "cloud", "ApiKeyReference", JsonSerializer.SerializeToElement("raw-api-key")));
        session.SetOption(kind, "cloud", "ApiKeyReference", JsonSerializer.SerializeToElement("env:LIBRE_KEY"));
        Assert.Empty(session.Validate());
        session.Save();
        Assert.Equal("env:LIBRE_KEY",
            Assert.Single(UnderstandMeSettingsJsonStore.Load(folder.Path).Settings.Translation.Providers)
                .Options["ApiKeyReference"].GetString());

        session.RemoveOption(kind, "cloud", "ApiKeyReference");
        Assert.Contains(session.Validate(), issue => issue.Path.EndsWith(".ApiKeyReference"));
    }

    [Fact]
    public void Storage_properties_and_delete_are_discardable_and_validation_uses_core()
    {
        using var folder = new TempFolder();
        UnderstandMeSettingsJsonStore.Save(folder.Path, new());
        var session = ConfiguratorSession.Open(folder.Path);
        var before = File.ReadAllBytes(folder.Path);
        var kind = SettingsComponentKind.LocalizationStorage;
        session.AddComponent(kind, "docs", "FileJson");
        session.SetStorageProperties("docs", "cs", false);
        session.SetOption(kind, "docs", "Directory", JsonSerializer.SerializeToElement("MyLocales"));
        Assert.Empty(session.Validate());
        Assert.False(Assert.Single(session.Draft.Localization.Storages).SetValueFromKey);
        Assert.Equal("cs", Assert.Single(session.Draft.Localization.Storages).SourceLanguage);
        session.RemoveComponent(kind, "docs");
        Assert.False(session.IsDirty);
        Assert.Equal(before, File.ReadAllBytes(folder.Path));
        Assert.Throws<ArgumentException>(() => session.RemoveComponent(kind, "unknown"));
    }

    [Fact]
    public void Session_rejects_stale_external_revision_after_component_change()
    {
        using var folder = new TempFolder();
        UnderstandMeSettingsJsonStore.Save(folder.Path, new());
        var session = ConfiguratorSession.Open(folder.Path);
        session.AddComponent(SettingsComponentKind.LocalizationStorage, "docs", "FileJson");
        UnderstandMeSettingsJsonStore.Save(folder.Path, new UnderstandMeSettings
        {
            Localization = new LocalizationSettings { DefaultSourceLanguage = "fr" }
        }, session.Revision);
        Assert.Throws<UnderstandMeSettingsConflictException>(() => session.Save());
    }

    private sealed class TempFolder : IDisposable
    {
        private readonly string _folder = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(), "um-components-" + Guid.NewGuid().ToString("N"));
        public TempFolder() => Directory.CreateDirectory(_folder);
        public string Path => System.IO.Path.Combine(_folder, "UnderstandMeSettings.json");
        public void Dispose() => Directory.Delete(_folder, recursive: true);
    }
}
