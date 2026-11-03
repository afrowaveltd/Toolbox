using Afrowave.Toolbox.Blackboard.Tools.MdView;
using Spectre.Console.Testing;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView.Tests;

public sealed class MdViewInteractiveSourcePickerTests
{
    [Fact]
    public async Task EscapeFromSourceMenuExitsWithoutOpeningAnything()
    {
        var console = Interactive();
        console.Input.PushKey(ConsoleKey.Escape);

        var result = await new MdViewInteractiveSourcePicker(console)
            .PickAsync(Path.GetTempPath());

        Assert.Null(result);
        Assert.Contains("Afrowave mdview", console.Output);
        Assert.Contains("Local Markdown file", console.Output);
    }

    [Fact]
    public async Task LocalFileBrowserSelectsExistingMarkdownWithoutChangingItsBytes()
    {
        using var directory = new TemporaryDirectory();
        var file = Path.Combine(directory.Root, "README.MD");
        await File.WriteAllTextAsync(file, "# Local document\n");
        var original = await File.ReadAllBytesAsync(file);

        var console = Interactive();
        console.Input.PushKey(ConsoleKey.Enter); // Local source
        console.Input.PushKey(ConsoleKey.Enter); // The only filtered Markdown file

        var result = await new MdViewInteractiveSourcePicker(console)
            .PickAsync(directory.Root);

        Assert.NotNull(result);
        Assert.Equal(MdViewSourceKind.Local, result.Kind);
        Assert.Equal(file, result.Value);
        Assert.Equal(original, await File.ReadAllBytesAsync(file));
    }

    [Fact]
    public async Task EscapingLocalBrowserReturnsToSourceMenu()
    {
        using var directory = new TemporaryDirectory();
        var console = Interactive();
        console.Input.PushKey(ConsoleKey.Enter);  // Local source
        console.Input.PushKey(ConsoleKey.Escape); // FileBrowser: back
        console.Input.PushKey(ConsoleKey.Escape); // Source menu: exit

        var result = await new MdViewInteractiveSourcePicker(console)
            .PickAsync(directory.Root);

        Assert.Null(result);
        Assert.Equal(2, console.Output.Split("Afrowave mdview", StringSplitOptions.None).Length - 1);
    }

    [Fact]
    public async Task GitHubSourceCollectsOwnerAndRepository()
    {
        var console = Interactive();
        console.Input.PushKey(ConsoleKey.DownArrow);
        console.Input.PushKey(ConsoleKey.DownArrow);
        console.Input.PushKey(ConsoleKey.Enter);
        console.Input.PushTextWithEnter("AfrowaveLtd/Toolbox");

        var result = await new MdViewInteractiveSourcePicker(console)
            .PickAsync(Path.GetTempPath());

        Assert.NotNull(result);
        Assert.Equal(MdViewSourceKind.GitHub, result.Kind);
        Assert.Equal("AfrowaveLtd", result.Value);
        Assert.Equal("Toolbox", result.Repository);
    }

    [Fact]
    public async Task HttpsSourceCollectsValidUrl()
    {
        var console = Interactive();
        console.Input.PushKey(ConsoleKey.DownArrow);
        console.Input.PushKey(ConsoleKey.Enter);
        console.Input.PushTextWithEnter("https://example.org/help.md");

        var result = await new MdViewInteractiveSourcePicker(console)
            .PickAsync(Path.GetTempPath());

        Assert.NotNull(result);
        Assert.Equal(MdViewSourceKind.Url, result.Kind);
        Assert.Equal("https://example.org/help.md", result.Value);
    }

    [Fact]
    public async Task UrlWithQueryIsRejectedAndReturnsToMenu()
    {
        var console = Interactive();
        console.Input.PushKey(ConsoleKey.DownArrow);
        console.Input.PushKey(ConsoleKey.Enter);
        console.Input.PushTextWithEnter("https://example.org/help.md?token=secret");
        console.Input.PushKey(ConsoleKey.Escape);

        var result = await new MdViewInteractiveSourcePicker(console)
            .PickAsync(Path.GetTempPath());

        Assert.Null(result);
        Assert.Contains("without credentials, query or fragment", console.Output);
    }

    [Fact]
    public async Task NonInteractiveConsoleCannotPromptForSource()
    {
        var console = new TestConsole();
        console.Profile.Capabilities.Interactive = false;
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new MdViewInteractiveSourcePicker(console).PickAsync(Path.GetTempPath()));
    }

    [Theory]
    [InlineData("README.md", true)]
    [InlineData("README.MARKDOWN", true)]
    [InlineData("README.txt", false)]
    [InlineData("README.md.exe", false)]
    public void MarkdownExtensionIsValidatedAgainAfterBrowser(string name, bool expected) =>
        Assert.Equal(expected, MdViewInteractiveSourcePicker.IsMarkdownPath(name));

    private static TestConsole Interactive()
    {
        var console = new TestConsole();
        console.Profile.Capabilities.Interactive = true;
        console.Profile.Capabilities.Unicode = true;
        console.Profile.Width = 120;
        return console;
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public string Root { get; } =
            Path.Combine(Path.GetTempPath(), "mdview-selector-" + Guid.NewGuid().ToString("N"));

        public TemporaryDirectory() => Directory.CreateDirectory(Root);

        public void Dispose()
        {
            if (Directory.Exists(Root)) Directory.Delete(Root, recursive: true);
        }
    }
}
