using Afrowave.Toolbox.UnderstandMe.Configurator;
using Afrowave.Toolbox.UnderstandMe.Settings;
using Spectre.Console.Testing;
using Xunit;

namespace Afrowave.Toolbox.UnderstandMe.Configurator.Tests;

public sealed class ConfiguratorHelpTests
{
    [Fact]
    public void Every_registered_topic_has_bundled_english_markdown()
    {
        Assert.Equal(5, ConfiguratorHelpCatalog.Topics.Count);
        foreach (var topic in ConfiguratorHelpCatalog.Topics)
        {
            var content = ConfiguratorHelpCatalog.ReadEnglish(topic.Id);
            Assert.StartsWith("# ", content);
            Assert.True(content.Length > 100);
            Assert.True(ConfiguratorStrings.GetBundledMessages("en").ContainsKey(topic.TitleKey));
            Assert.True(ConfiguratorStrings.GetBundledMessages("cs").ContainsKey(topic.TitleKey));
        }
        Assert.Throws<ArgumentException>(() => ConfiguratorHelpCatalog.ReadEnglish("../secret"));
    }

    [Fact]
    public async Task Main_menu_help_opens_topic_offline_then_returns_to_main_menu()
    {
        var console = InteractiveConsole();
        for (var i = 0; i < 4; i++)
            console.Input.PushKey(ConsoleKey.DownArrow); // Fifth menu entry: Help.
        console.Input.PushKey(ConsoleKey.Enter);
        console.Input.PushKey(ConsoleKey.Enter); // First topic: Getting started.
        console.Input.PushKey(ConsoleKey.Enter); // Return to help topics.
        console.Input.PushKey(ConsoleKey.Escape); // Back to main menu.
        console.Input.PushKey(ConsoleKey.Escape); // Exit application.

        await new ConfiguratorApp(console).RunAsync();
        Assert.Contains("Help topics", console.Output);
        Assert.Contains("Getting started", console.Output);
        Assert.Contains("The Configurator edits UnderstandMe settings", console.Output);
    }

    [Fact]
    public async Task Context_help_returns_without_mutating_existing_settings_file()
    {
        var directory = Path.Combine(Path.GetTempPath(), "um-help-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "UnderstandMeSettings.json");
            UnderstandMeSettingsJsonStore.Save(path, new UnderstandMeSettings());
            var original = File.ReadAllBytes(path);
            var console = InteractiveConsole();
            for (var i = 0; i < 3; i++)
                console.Input.PushKey(ConsoleKey.DownArrow); // Editor: Translation providers.
            console.Input.PushKey(ConsoleKey.Enter);
            console.Input.PushKey(ConsoleKey.DownArrow); // Add registration -> context Help.
            console.Input.PushKey(ConsoleKey.Enter);
            console.Input.PushKey(ConsoleKey.Enter); // Return from topic.
            console.Input.PushKey(ConsoleKey.Escape); // Return from registration list.
            console.Input.PushKey(ConsoleKey.Escape); // Return from editor.
            console.Input.PushKey(ConsoleKey.Escape); // Exit main menu.

            await new ConfiguratorApp(console, preferencesPath: Path.Combine(directory, "ui.json"))
                .RunAsync(path);
            Assert.Contains("Register a translation provider", console.Output);
            Assert.Contains("GET", console.Output);
            Assert.Equal(original, File.ReadAllBytes(path));
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void Help_renderer_uses_shared_markdown_inline_and_link_presentation()
    {
        var console = new TestConsole();
        console.Profile.Capabilities.Unicode = true;
        console.Profile.Width = 120;

        ConfiguratorHelpRenderer.Render(console,
            "# Help\nUse **bold** and *italic*, then `code`.\n- [Guide](../en.md)\n");

        Assert.Contains("Help", console.Output);
        Assert.Contains("Use bold and italic, then code.", console.Output);
        Assert.Contains("Guide (../en.md)", console.Output);
        Assert.DoesNotContain("**bold**", console.Output);
        Assert.DoesNotContain("[Guide](../en.md)", console.Output);
    }

    [Fact]
    public void Help_renderer_never_evaluates_spectre_markup_and_keeps_code_literal()
    {
        var console = new TestConsole();
        console.Profile.Width = 120;

        ConfiguratorHelpRenderer.Render(console,
            "# [red]Literal[/]\n```md\n**not bold** [link](target)\n```\n");

        Assert.Contains("[red]Literal[/]", console.Output);
        Assert.Contains("**not bold** [link](target)", console.Output);
    }

    [Fact]
    public void Help_renderer_rejects_null_inputs_without_side_effects()
    {
        Assert.Throws<ArgumentNullException>(() => ConfiguratorHelpRenderer.Render(null!, "# Help"));
        Assert.Throws<ArgumentNullException>(() => ConfiguratorHelpRenderer.Render(new TestConsole(), null!));
    }

    private static TestConsole InteractiveConsole()
    {
        var console = new TestConsole();
        console.Profile.Capabilities.Interactive = true;
        return console;
    }
}
