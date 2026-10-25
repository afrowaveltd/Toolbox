using Afrowave.Toolbox.UnderstandMe.Configurator;
using Afrowave.Toolbox.UnderstandMe.Settings;
using Spectre.Console.Testing;
using Xunit;

namespace Afrowave.Toolbox.UnderstandMe.Configurator.Tests;

public sealed class ConfiguratorSessionTests
{
    [Fact]
    public void New_draft_is_not_written_until_explicit_save()
    {
        using var temp = new TempFolder();
        var session = ConfiguratorSession.CreateNew(temp.File);
        session.SetSourceLanguage("cs");
        session.SetWorkerBufferCapacity(8);
        Assert.False(File.Exists(temp.File));
        Assert.Empty(session.Validate());
        session.Save();
        Assert.True(File.Exists(temp.File));
        Assert.False(session.IsDirty);
        var written = UnderstandMeSettingsJsonStore.Load(temp.File);
        Assert.Equal("cs", written.Settings.Localization.DefaultSourceLanguage);
        Assert.Equal(8, written.Settings.Translation.Queues.DefaultWorkerBufferCapacity);
    }

    [Fact]
    public void Editing_scalars_preserves_other_configured_components()
    {
        using var temp = new TempFolder();
        var settings = new UnderstandMeSettings
        {
            Localization = new LocalizationSettings { Storages = [new LocalizationStorageSettings { Id = "main" }] },
            Translation = new TranslationSettings { Providers = [new TranslationBackendSettings { Id = "local", Type = "LibreTranslate" }] }
        };
        UnderstandMeSettingsJsonStore.Save(temp.File, settings);
        var original = File.ReadAllBytes(temp.File);
        var session = ConfiguratorSession.Open(temp.File);
        session.SetSourceLanguage("fr");
        session.SetWorkerBufferCapacity(3);
        Assert.True(session.IsDirty);
        Assert.Equal("main", Assert.Single(session.Draft.Localization.Storages).Id);
        Assert.Equal("local", Assert.Single(session.Draft.Translation.Providers).Id);
        Assert.Equal(original, File.ReadAllBytes(temp.File));
    }

    [Fact]
    public void Bad_draft_never_overwrites_and_external_changes_cause_conflict()
    {
        using var temp = new TempFolder();
        UnderstandMeSettingsJsonStore.Save(temp.File, new());
        var session = ConfiguratorSession.Open(temp.File);
        session.SetWorkerBufferCapacity(0);
        Assert.Contains(session.Validate(), issue => issue.Path.Contains("DefaultWorkerBufferCapacity"));
        Assert.Throws<UnderstandMeSettingsValidationException>(() => session.Save());

        var expected = UnderstandMeSettingsJsonStore.Save(temp.File, new UnderstandMeSettings
        {
            Localization = new LocalizationSettings { DefaultSourceLanguage = "de" }
        }, session.Revision);

        session.SetWorkerBufferCapacity(5);
        session.SetSourceLanguage("cs");
        Assert.Throws<UnderstandMeSettingsConflictException>(() => session.Save());
        Assert.Equal(expected, UnderstandMeSettingsJsonStore.Load(temp.File).Revision);
        Assert.True(session.IsDirty);
    }

    [Fact]
    public void Invalid_file_is_not_rewritten_or_claimed_as_new()
    {
        using var temp = new TempFolder();
        File.WriteAllText(temp.File, "{ invalid");
        Assert.Throws<IOException>(() => ConfiguratorSession.CreateNew(temp.File));
        Assert.Throws<System.Text.Json.JsonException>(() => ConfiguratorSession.Open(temp.File));
        Assert.Equal("{ invalid", File.ReadAllText(temp.File));
    }

    [Fact]
    public async Task Escape_exits_empty_interactive_editor()
    {
        var console = TestConsole();
        console.Input.PushKey(ConsoleKey.Escape);
        await new ConfiguratorApp(console).RunAsync();
        Assert.StartsWith("\u001b[2J", console.Output);
        Assert.Equal(1, console.Output.Split("\u001b[2J").Length - 1);
        Assert.Contains("Choose an action", console.Output);
    }

    [Fact]
    public async Task Escape_after_load_leaves_bytes_unchanged()
    {
        using var temp = new TempFolder();
        UnderstandMeSettingsJsonStore.Save(temp.File, new());
        var before = File.ReadAllBytes(temp.File);
        var console = TestConsole();
        console.Input.PushKey(ConsoleKey.Escape);
        await new ConfiguratorApp(console).RunAsync(temp.File);
        Assert.Equal(before, File.ReadAllBytes(temp.File));
    }

    [Fact]
    public async Task FileBrowser_selects_json_and_loads_existing_settings()
    {
        using var temp = new TempFolder();
        var path = Path.Combine(Path.GetDirectoryName(temp.File)!, "settings.json");
        File.WriteAllText(path, "{\"SchemaVersion\":1}");
        var before = File.ReadAllBytes(path);
        var console = TestConsole();
        console.Input.PushKey(ConsoleKey.Enter); // Choose Browse JSON from the initial menu.
        console.Input.PushKey(ConsoleKey.Enter); // Select the only *.json file.
        console.Input.PushKey(ConsoleKey.Escape); // Exit the loaded settings menu.

        await new ConfiguratorApp(console, Path.GetDirectoryName(path)).RunAsync();

        Assert.Contains("Opened: " + path, console.Output);
        Assert.Equal(before, File.ReadAllBytes(path));
    }

    [Fact]
    public async Task Cannot_start_interactive_editor_with_redirected_console()
    {
        var console = new Spectre.Console.Testing.TestConsole();
        console.Profile.Capabilities.Interactive = false;
        await Assert.ThrowsAsync<InvalidOperationException>(() => new ConfiguratorApp(console).RunAsync());
    }

    private static Spectre.Console.Testing.TestConsole TestConsole()
    {
        var result = new Spectre.Console.Testing.TestConsole();
        result.Profile.Capabilities.Interactive = true;
        result.Profile.Capabilities.Unicode = true;
        return result;
    }

    private sealed class TempFolder : IDisposable
    {
        private readonly string _path = Path.Combine(Path.GetTempPath(), "um-configurator-" + Guid.NewGuid().ToString("N"));
        public TempFolder() => Directory.CreateDirectory(_path);
        public string File => Path.Combine(_path, "UnderstandMeSettings.json");
        public void Dispose() => Directory.Delete(_path, recursive: true);
    }
}
