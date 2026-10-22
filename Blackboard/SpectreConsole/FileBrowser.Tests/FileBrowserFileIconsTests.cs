using Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser.Tests;

public sealed class FileBrowserFileIconsTests
{
    private readonly FileBrowserFileIcons _icons = new();

    [Theory]
    [InlineData("notes.txt", FileBrowserFileKind.Text)]
    [InlineData("README", FileBrowserFileKind.Text)]
    [InlineData("README.md", FileBrowserFileKind.Text)]
    [InlineData("config.JSON", FileBrowserFileKind.Json)]
    [InlineData("appsettings.json", FileBrowserFileKind.Json)]
    [InlineData("settings.yaml", FileBrowserFileKind.Configuration)]
    [InlineData(".gitignore", FileBrowserFileKind.Configuration)]
    [InlineData("Dockerfile", FileBrowserFileKind.Code)]
    [InlineData("Program.cs", FileBrowserFileKind.Code)]
    [InlineData("manual.pdf", FileBrowserFileKind.Pdf)]
    [InlineData("budget.xlsx", FileBrowserFileKind.Spreadsheet)]
    [InlineData("slides.pptx", FileBrowserFileKind.Presentation)]
    [InlineData("photo.jpg", FileBrowserFileKind.Image)]
    [InlineData("song.FLAC", FileBrowserFileKind.Audio)]
    [InlineData("movie.mkv", FileBrowserFileKind.Video)]
    [InlineData("backup.tar.gz", FileBrowserFileKind.Archive)]
    [InlineData("archive.7z", FileBrowserFileKind.Archive)]
    [InlineData("installer.exe", FileBrowserFileKind.Executable)]
    [InlineData("history.sqlite", FileBrowserFileKind.Database)]
    [InlineData("typeface.woff2", FileBrowserFileKind.Font)]
    [InlineData("mystery.xyz", FileBrowserFileKind.Unknown)]
    [InlineData(".json", FileBrowserFileKind.Unknown)]
    public void Recognizes_types_by_entry_name_without_reading_contents(
        string fileName, FileBrowserFileKind expected)
    {
        Assert.Equal(expected, _icons.Classify(fileName, false));
    }

    [Fact]
    public void Directory_flag_overrides_extension_even_for_json_named_folder()
    {
        Assert.Equal(FileBrowserFileKind.Directory, _icons.Classify("documents.mp3", true));
        Assert.Equal(FileBrowserFileKind.Directory, _icons.Classify(null, true));
        Assert.Equal(FileBrowserFileKind.Unknown, _icons.Classify(null, false));
        Assert.Equal(FileBrowserFileKind.Unknown, _icons.Classify(" ", false));
    }

    [Fact]
    public void Custom_extension_and_exact_name_overrides_are_local_to_instance()
    {
        var customized = new FileBrowserFileIcons(
            new Dictionary<string, FileBrowserFileKind>
            {
                [".abc"] = FileBrowserFileKind.Audio,
                [".tar.gz"] = FileBrowserFileKind.Database,
                [".TXT"] = FileBrowserFileKind.Code
            },
            new Dictionary<string, FileBrowserFileKind>
            {
                ["my.abc"] = FileBrowserFileKind.Image
            });

        Assert.Equal(FileBrowserFileKind.Audio, customized.Classify("track.ABC", false));
        Assert.Equal(FileBrowserFileKind.Database, customized.Classify("backup.tar.gz", false));
        Assert.Equal(FileBrowserFileKind.Image, customized.Classify("my.abc", false));
        Assert.Equal(FileBrowserFileKind.Code, customized.Classify("readme.txt", false));
        Assert.Equal(FileBrowserFileKind.Text, _icons.Classify("readme.txt", false));
        Assert.Equal(FileBrowserFileKind.Unknown, _icons.Classify("track.abc", false));
    }

    [Theory]
    [InlineData("abc")]
    [InlineData(".")]
    [InlineData(".one/two")]
    [InlineData(".one\\two")]
    public void Rejects_invalid_extension_override_names(string extension)
    {
        Assert.Throws<ArgumentException>(() =>
            new FileBrowserFileIcons(new Dictionary<string, FileBrowserFileKind>
            {
                [extension] = FileBrowserFileKind.Audio
            }));
    }

    [Fact]
    public void Rejects_unsupported_override_file_types()
    {
        Assert.Throws<ArgumentException>(() => new FileBrowserFileIcons(
            new Dictionary<string, FileBrowserFileKind> { [".abc"] = (FileBrowserFileKind)999 }));
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(false, false)]
    public void Auto_mode_omits_icons_when_terminal_is_not_suitable(bool unicode, bool interactive)
    {
        var terminal = new FileBrowserTerminalCapabilities(unicode, interactive);
        Assert.Equal("", FileBrowserFileIcons.GetIcon(FileBrowserFileKind.Audio, FileBrowserIconMode.Auto, terminal));
    }

    [Fact]
    public void Auto_uses_narrow_symbol_without_emoji_confirmation()
    {
        var terminal = new FileBrowserTerminalCapabilities(SupportsUnicode: true, IsInteractive: true);
        Assert.Equal("♪", FileBrowserFileIcons.GetIcon(FileBrowserFileKind.Audio, FileBrowserIconMode.Auto, terminal));
        Assert.Equal("▸", FileBrowserFileIcons.GetIcon(FileBrowserFileKind.Directory, FileBrowserIconMode.Auto, terminal));
        Assert.NotEqual("🎵", FileBrowserFileIcons.GetIcon(FileBrowserFileKind.Audio, FileBrowserIconMode.Auto, terminal));
    }

    [Fact]
    public void Rich_emoji_requires_confirmed_support_or_explicit_override()
    {
        var terminal = new FileBrowserTerminalCapabilities(true, true, SupportsEmoji: true);
        Assert.Equal("🎵", FileBrowserFileIcons.GetIcon(FileBrowserFileKind.Audio, FileBrowserIconMode.Auto, terminal));
        Assert.Equal("📁", FileBrowserFileIcons.GetIcon(FileBrowserFileKind.Directory, FileBrowserIconMode.Auto, terminal));
        Assert.Equal("🎵", FileBrowserFileIcons.GetIcon(FileBrowserFileKind.Audio, FileBrowserIconMode.On, default));
        Assert.Equal("", FileBrowserFileIcons.GetIcon(FileBrowserFileKind.Audio, FileBrowserIconMode.Off, terminal));
    }

    [Fact]
    public void Every_kind_has_a_readable_label_and_optional_rich_icon()
    {
        foreach (var kind in Enum.GetValues<FileBrowserFileKind>())
        {
            Assert.False(string.IsNullOrWhiteSpace(FileBrowserFileIcons.GetLabel(kind)));
            Assert.False(string.IsNullOrWhiteSpace(FileBrowserFileIcons.GetIcon(
                kind, FileBrowserIconMode.On, default)));
        }
    }

    [Fact]
    public void Original_unicode_names_and_current_directory_are_not_modified()
    {
        const string name = "Česká-hudba-😀.mp3";
        var currentDirectory = Environment.CurrentDirectory;

        Assert.Equal(FileBrowserFileKind.Audio, _icons.Classify(name, false));
        Assert.Equal(name, "Česká-hudba-😀.mp3");
        Assert.Equal(currentDirectory, Environment.CurrentDirectory);
    }

    [Fact]
    public void Rejects_undefined_kinds_and_modes()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            FileBrowserFileIcons.GetLabel((FileBrowserFileKind)999));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            FileBrowserFileIcons.GetIcon((FileBrowserFileKind)999, FileBrowserIconMode.On, default));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            FileBrowserFileIcons.GetIcon(FileBrowserFileKind.Audio, (FileBrowserIconMode)999, default));
    }
}
