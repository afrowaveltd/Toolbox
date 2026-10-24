using Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser;
using Spectre.Console.Testing;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser.Tests;

public sealed class SpectreFileBrowserInteractionTests
{
    [Fact]
    public async Task Enter_selects_a_real_file_without_touching_current_directory()
    {
        var path = CreateFixture();
        try
        {
            var console = ConsoleWithInput();
            console.Input.PushKey(ConsoleKey.Enter);
            var previousDirectory = Environment.CurrentDirectory;
            var result = await new SpectreFileBrowser(new FileBrowserLocalSource(), console)
                .PickAsync(new FileBrowserLocation("local", path),
                    options: new SpectreFileBrowserOptions { IconMode = FileBrowserIconMode.Off });

            Assert.True(result.IsSelected);
            Assert.False(result.IsDirectory);
            Assert.Equal(Path.Combine(path, "a.ajis"), result.Location!.Value);
            Assert.Equal(previousDirectory, Environment.CurrentDirectory);
        }
        finally
        {
            Directory.Delete(path, recursive: true);
        }
    }

    [Fact]
    public async Task Escape_cancels_without_choosing_any_file()
    {
        var path = CreateFixture();
        try
        {
            var console = ConsoleWithInput();
            console.Input.PushKey(ConsoleKey.Escape);
            var result = await new SpectreFileBrowser(new FileBrowserLocalSource(), console)
                .PickAsync(new FileBrowserLocation("local", path));
            Assert.False(result.IsSelected);
            Assert.Null(result.Location);
        }
        finally
        {
            Directory.Delete(path, recursive: true);
        }
    }

    [Fact]
    public async Task Initial_universal_mask_filter_shows_ajis_and_json_not_other_types()
    {
        var path = CreateFixture();
        try
        {
            var console = ConsoleWithInput();
            console.Input.PushKey(ConsoleKey.Enter);
            var result = await new SpectreFileBrowser(new FileBrowserLocalSource(), console)
                .PickAsync(new FileBrowserLocation("local", path),
                    options: new SpectreFileBrowserOptions
                    {
                        IconMode = FileBrowserIconMode.Off,
                        FileFilter = new FileBrowserFileFilter(
                            nameMatcher: new FileBrowserNameMatcher("*.ajis;*.json"))
                    });
            Assert.True(result.IsSelected);
            Assert.EndsWith("a.ajis", result.Location!.Value);
            Assert.Contains("b.json", console.Output);
            Assert.DoesNotContain("c.txt", console.Output);
        }
        finally
        {
            Directory.Delete(path, recursive: true);
        }
    }

    private static TestConsole ConsoleWithInput()
    {
        var console = new TestConsole();
        console.Profile.Capabilities.Interactive = true;
        console.Profile.Capabilities.Unicode = true;
        return console;
    }

    private static string CreateFixture()
    {
        var path = Path.Combine(Path.GetTempPath(), "AfrowaveFB-TUI-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        File.WriteAllText(Path.Combine(path, "a.ajis"), "data");
        File.WriteAllText(Path.Combine(path, "b.json"), "{}");
        File.WriteAllText(Path.Combine(path, "c.txt"), "notes");
        return path;
    }
}
