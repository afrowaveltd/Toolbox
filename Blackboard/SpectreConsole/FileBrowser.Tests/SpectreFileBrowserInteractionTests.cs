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


    [Fact]
    public async Task Search_displays_both_paths_and_opens_the_explicitly_selected_second_match()
    {
        var root = Path.Combine(Path.GetTempPath(), "AfrowaveFB-Search-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "A"));
        Directory.CreateDirectory(Path.Combine(root, "B"));
        File.WriteAllText(Path.Combine(root, "A", "target.json"), "{}");
        File.WriteAllText(Path.Combine(root, "B", "target.json"), "{}");
        try
        {
            var console = ConsoleWithInput();

            // Browse: A, B, Parent, Search. Then choose default Mask mode.
            for (var i = 0; i < 3; i++)
                console.Input.PushKey(ConsoleKey.DownArrow);
            console.Input.PushKey(ConsoleKey.Enter);
            console.Input.PushKey(ConsoleKey.Enter);
            console.Input.PushTextWithEnter("target.json");

            // The results menu contains A/target.json then B/target.json.
            console.Input.PushKey(ConsoleKey.DownArrow);
            console.Input.PushKey(ConsoleKey.Enter);

            // The browser is now in B and highlights the matching file.
            console.Input.PushKey(ConsoleKey.Enter);

            var browser = new SpectreFileBrowser(new FileBrowserLocalSource(), console);
            var result = await browser.PickAsync(new FileBrowserLocation("local", root),
                options: new SpectreFileBrowserOptions { IconMode = FileBrowserIconMode.Off });

            Assert.True(result.IsSelected);
            Assert.Equal(Path.Combine(root, "B", "target.json"), result.Location!.Value);
            Assert.Contains("A/target.json", console.Output);
            Assert.Contains("B/target.json", console.Output);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Search_without_results_then_escape_leaves_navigation_untouched()
    {
        var root = Path.Combine(Path.GetTempPath(), "AfrowaveFB-Miss-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var console = ConsoleWithInput();
            // Browse in empty folder: Parent, Search, Tree, Filter, Back.
            console.Input.PushKey(ConsoleKey.DownArrow);
            console.Input.PushKey(ConsoleKey.Enter);
            console.Input.PushKey(ConsoleKey.Enter);
            console.Input.PushTextWithEnter("missing.json");
            console.Input.PushKey(ConsoleKey.Escape);

            var browser = new SpectreFileBrowser(new FileBrowserLocalSource(), console);
            var result = await browser.PickAsync(new FileBrowserLocation("local", root),
                options: new SpectreFileBrowserOptions { IconMode = FileBrowserIconMode.Off });

            Assert.False(result.IsSelected);
            Assert.Contains("Nothing found; original directory unchanged.", console.Output);
            Assert.Contains(root, console.Output);
        }
        finally
        {
            Directory.Delete(root);
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
