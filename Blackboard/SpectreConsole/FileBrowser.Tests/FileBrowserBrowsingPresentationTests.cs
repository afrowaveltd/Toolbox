using Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser.Tests;

public sealed class FileBrowserBrowsingPresentationTests
{
    private static readonly FileBrowserFileIcons Icons = new();
    private static readonly FileBrowserTerminalCapabilities EmojiTerminal = new(true, true, true);

    [Theory]
    [InlineData("/home", FileBrowserKnownFolderKind.UserProfiles)]
    [InlineData("/home/", FileBrowserKnownFolderKind.UserProfiles)]
    [InlineData("/home/alice", FileBrowserKnownFolderKind.UserHome)]
    [InlineData("/etc", FileBrowserKnownFolderKind.SystemConfiguration)]
    [InlineData("/var/log", FileBrowserKnownFolderKind.Logs)]
    [InlineData("/tmp", FileBrowserKnownFolderKind.Temporary)]
    [InlineData("/work/home", FileBrowserKnownFolderKind.None)]
    [InlineData("/home2", FileBrowserKnownFolderKind.None)]
    [InlineData("/HOME", FileBrowserKnownFolderKind.None)]
    [InlineData("/home/bob", FileBrowserKnownFolderKind.None)]
    [InlineData("/usr/etc", FileBrowserKnownFolderKind.None)]
    [InlineData("/home/../etc", FileBrowserKnownFolderKind.None)]
    [InlineData("home", FileBrowserKnownFolderKind.None)]
    [InlineData("/home//alice", FileBrowserKnownFolderKind.None)]
    public void Linux_known_directories_require_exact_absolute_paths(
        string path, FileBrowserKnownFolderKind expected)
    {
        var folders = new FileBrowserKnownFolders(FileBrowserPlatform.Linux, "/home/alice");
        Assert.Equal(expected, folders.Classify(path, isDirectory: true));
    }

    [Theory]
    [InlineData(@"C:\Users", FileBrowserKnownFolderKind.UserProfiles)]
    [InlineData(@"c:\users", FileBrowserKnownFolderKind.UserProfiles)]
    [InlineData(@"C:/Users/Jane", FileBrowserKnownFolderKind.UserHome)]
    [InlineData(@"C:\Users\Jane\", FileBrowserKnownFolderKind.UserHome)]
    [InlineData(@"C:\Users\Bob", FileBrowserKnownFolderKind.None)]
    [InlineData(@"C:\Somewhere\Users", FileBrowserKnownFolderKind.None)]
    [InlineData(@"C:\UsersArchive", FileBrowserKnownFolderKind.None)]
    [InlineData(@"D:\Users", FileBrowserKnownFolderKind.None)]
    [InlineData(@"C:\Users\Jane\..\Bob", FileBrowserKnownFolderKind.None)]
    [InlineData(@"Users", FileBrowserKnownFolderKind.None)]
    public void Windows_known_directories_use_actual_profile_drive(
        string path, FileBrowserKnownFolderKind expected)
    {
        var folders = new FileBrowserKnownFolders(FileBrowserPlatform.Windows, @"C:\Users\Jane");
        Assert.Equal(expected, folders.Classify(path, true));
    }

    [Fact]
    public void Windows_profiles_do_not_require_the_c_drive()
    {
        var folders = new FileBrowserKnownFolders(FileBrowserPlatform.Windows, @"D:\Users\Čeněk");
        Assert.Equal(FileBrowserKnownFolderKind.UserProfiles, folders.Classify(@"d:\USERS", true));
        Assert.Equal(FileBrowserKnownFolderKind.UserHome, folders.Classify(@"d:\users\Čeněk", true));
        Assert.Equal(FileBrowserKnownFolderKind.None, folders.Classify(@"C:\Users", true));
    }

    [Fact]
    public void Mac_directory_names_are_recognized_only_at_their_true_locations()
    {
        var folders = new FileBrowserKnownFolders(FileBrowserPlatform.MacOS, "/Users/marta");
        Assert.Equal(FileBrowserKnownFolderKind.UserProfiles, folders.Classify("/Users", true));
        Assert.Equal(FileBrowserKnownFolderKind.UserHome, folders.Classify("/Users/marta", true));
        Assert.Equal(FileBrowserKnownFolderKind.Applications, folders.Classify("/Applications", true));
        Assert.Equal(FileBrowserKnownFolderKind.None, folders.Classify("/workspace/Applications", true));
        Assert.Equal(FileBrowserKnownFolderKind.None, folders.Classify("/users", true));
    }

    [Fact]
    public void Unknown_platform_only_recognizes_explicit_home_path()
    {
        var folders = new FileBrowserKnownFolders(FileBrowserPlatform.Other, "/users/alice");
        Assert.Equal(FileBrowserKnownFolderKind.UserHome, folders.Classify("/users/alice", true));
        Assert.Equal(FileBrowserKnownFolderKind.None, folders.Classify("/home", true));
        Assert.Equal(FileBrowserKnownFolderKind.None, folders.Classify("/etc", true));
    }

    [Fact]
    public void A_file_or_unknown_path_is_not_mistaken_for_special_directory()
    {
        var folders = new FileBrowserKnownFolders(FileBrowserPlatform.Linux, "/home/alice");
        Assert.Equal(FileBrowserKnownFolderKind.None, folders.Classify("/home", false));
        Assert.Equal(FileBrowserKnownFolderKind.None, folders.Classify(null, true));
        Assert.Equal(FileBrowserKnownFolderKind.None, folders.Classify("", true));
        Assert.Equal(FileBrowserKnownFolderKind.None, folders.Classify(" ", true));
    }

    [Theory]
    [InlineData(FileBrowserKnownFolderKind.UserProfiles)]
    [InlineData(FileBrowserKnownFolderKind.UserHome)]
    [InlineData(FileBrowserKnownFolderKind.SystemConfiguration)]
    [InlineData(FileBrowserKnownFolderKind.Logs)]
    [InlineData(FileBrowserKnownFolderKind.Temporary)]
    [InlineData(FileBrowserKnownFolderKind.Applications)]
    public void Known_directories_have_distinct_optional_emoji_and_accessible_labels(
        FileBrowserKnownFolderKind kind)
    {
        Assert.False(string.IsNullOrWhiteSpace(FileBrowserKnownFolders.GetLabel(kind)));
        var icon = FileBrowserKnownFolders.GetIcon(kind, FileBrowserIconMode.On, default);
        Assert.NotEqual("📁", icon);
        Assert.False(string.IsNullOrWhiteSpace(icon));
        Assert.Equal("", FileBrowserKnownFolders.GetIcon(kind, FileBrowserIconMode.Off, EmojiTerminal));
        Assert.Equal("", FileBrowserKnownFolders.GetIcon(kind, FileBrowserIconMode.Auto, default));
    }

    [Fact]
    public void Known_icons_fall_back_to_small_unicode_and_generic_directory_when_unknown()
    {
        var conservative = new FileBrowserTerminalCapabilities(true, true);
        Assert.Equal("⌂", FileBrowserKnownFolders.GetIcon(
            FileBrowserKnownFolderKind.UserHome, FileBrowserIconMode.Auto, conservative));
        Assert.Equal("🏠", FileBrowserKnownFolders.GetIcon(
            FileBrowserKnownFolderKind.UserHome, FileBrowserIconMode.Auto, EmojiTerminal));
        Assert.Equal("📁", FileBrowserKnownFolders.GetIcon(
            FileBrowserKnownFolderKind.None, FileBrowserIconMode.On, EmojiTerminal));
        Assert.Equal("Folder", FileBrowserKnownFolders.GetLabel(FileBrowserKnownFolderKind.None));
    }

    [Fact]
    public void Invalid_known_folder_or_platform_is_rejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new FileBrowserKnownFolders((FileBrowserPlatform)777));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => FileBrowserKnownFolders.GetLabel((FileBrowserKnownFolderKind)777));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => FileBrowserKnownFolders.GetIcon((FileBrowserKnownFolderKind)777, FileBrowserIconMode.On, default));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => FileBrowserKnownFolders.GetIcon(FileBrowserKnownFolderKind.UserHome, (FileBrowserIconMode)777, default));
    }

    [Theory]
    [InlineData("config.json", false, true)]
    [InlineData("config.JSON", false, true)]
    [InlineData("README.md", false, false)]
    [InlineData("media.mp3", false, false)]
    [InlineData("config.json", true, true)]
    [InlineData("notes", true, true)]
    public void Json_category_filter_preserves_directory_navigation(
        string name, bool directory, bool expected)
    {
        var filter = new FileBrowserFileFilter(includedKinds: [FileBrowserFileKind.Json]);
        Assert.Equal(expected, filter.Allows(name, directory));
    }

    [Fact]
    public void Extension_filter_is_case_insensitive_and_supports_compound_suffixes()
    {
        var filter = new FileBrowserFileFilter(includedExtensions: [".json", ".tar.gz"]);
        Assert.True(filter.Allows("appsettings.JSON", false));
        Assert.True(filter.Allows("backup.TAR.GZ", false));
        Assert.False(filter.Allows("README.md", false));
        Assert.False(filter.Allows("archive.tar.xz", false));
        Assert.False(filter.Allows(".json", false));
        Assert.True(filter.Allows("some-folder", true));
    }

    [Fact]
    public void Multiple_categories_are_or_combined_but_category_and_extension_are_and_combined()
    {
        var fileKinds = new FileBrowserFileFilter(includedKinds:
            [FileBrowserFileKind.Json, FileBrowserFileKind.Configuration]);
        Assert.True(fileKinds.Allows("doc.json", false));
        Assert.True(fileKinds.Allows(".gitignore", false));
        Assert.False(fileKinds.Allows("song.mp3", false));

        var combined = new FileBrowserFileFilter(
            includedKinds: [FileBrowserFileKind.Json, FileBrowserFileKind.Text],
            includedExtensions: [".json"]);
        Assert.True(combined.Allows("doc.json", false));
        Assert.False(combined.Allows("README.md", false));
        Assert.False(combined.Allows("music.mp3", false));
    }

    [Fact]
    public void Empty_explicit_filter_matches_no_files_but_leaves_folders()
    {
        var filter = new FileBrowserFileFilter(includedKinds: []);
        Assert.False(filter.Allows("a.json", false));
        Assert.True(filter.Allows("folder.json", true));

        var emptyExtensions = new FileBrowserFileFilter(includedExtensions: []);
        Assert.False(emptyExtensions.Allows("a.json", false));
        Assert.True(emptyExtensions.Allows("folder", true));
    }

    [Fact]
    public void Caller_can_hide_folders_but_default_is_to_keep_them()
    {
        var filter = new FileBrowserFileFilter(
            includedExtensions: [".json"], includeDirectories: false);
        Assert.False(filter.Allows("a-folder", true));
        Assert.True(filter.Allows("a.json", false));
        Assert.False(filter.Allows("", false));
    }

    [Fact]
    public void Custom_classification_controls_type_filter_without_leaking_to_default()
    {
        var customized = new FileBrowserFileIcons(
            new Dictionary<string, FileBrowserFileKind> { [".foo"] = FileBrowserFileKind.Json });
        var filter = new FileBrowserFileFilter(
            includedKinds: [FileBrowserFileKind.Json], classifier: customized);
        Assert.True(filter.Allows("x.foo", false));
        Assert.False(new FileBrowserFileFilter(includedKinds: [FileBrowserFileKind.Json])
            .Allows("x.foo", false));
    }

    [Theory]
    [InlineData("json")]
    [InlineData(".")]
    [InlineData(".two/parts")]
    [InlineData(".two\\parts")]
    public void Invalid_extensions_are_rejected(string extension)
    {
        Assert.Throws<ArgumentException>(() =>
            new FileBrowserFileFilter(includedExtensions: [extension]));
    }

    [Fact]
    public void Invalid_kind_cannot_enter_a_filter()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new FileBrowserFileFilter(includedKinds: [(FileBrowserFileKind)999]));
    }

    [Fact]
    public void File_rows_are_exactly_one_ascii_space_to_the_right()
    {
        var file = FileBrowserEntryPresentation.Format(
            "appsettings.json", false, Icons, FileBrowserIconMode.Off, default);
        var directory = FileBrowserEntryPresentation.Format(
            "config", true, Icons, FileBrowserIconMode.Off, default);
        Assert.Equal(" appsettings.json", file);
        Assert.Equal("config/", directory);
        Assert.StartsWith(" ", file);
        Assert.False(directory.StartsWith(' '));
    }

    [Fact]
    public void Specialized_folder_icon_does_not_replace_filename_or_file_indentation()
    {
        var folder = FileBrowserEntryPresentation.Format("alice", true, Icons,
            FileBrowserIconMode.On, EmojiTerminal, FileBrowserKnownFolderKind.UserHome);
        var file = FileBrowserEntryPresentation.Format("track.flac", false, Icons,
            FileBrowserIconMode.On, EmojiTerminal);
        Assert.Equal("🏠 alice/", folder);
        Assert.Equal(" 🎵 track.flac", file);
    }

    [Fact]
    public void Folder_names_with_slash_and_file_names_with_markup_are_plain_text()
    {
        Assert.Equal("folder/", FileBrowserEntryPresentation.Format("folder/", true,
            Icons, FileBrowserIconMode.Off, default));
        Assert.Equal(" [red]README.md", FileBrowserEntryPresentation.Format("[red]README.md",
            false, Icons, FileBrowserIconMode.Off, default));
    }

    [Fact]
    public void Control_characters_are_escaped_in_display_only()
    {
        const string name = "bad\u001b[31m\nname.json";
        var originalName = name;
        var output = FileBrowserEntryPresentation.Format(
            name, false, Icons, FileBrowserIconMode.Off, default);
        Assert.Equal(@" bad\u001B[31m\u000Aname.json", output);
        Assert.Equal(name, originalName);
    }

    [Fact]
    public void Unicode_file_names_remain_intact_in_display_and_classifier()
    {
        var output = FileBrowserEntryPresentation.Format(
            "Písnička🎵.mp3", false, Icons, FileBrowserIconMode.Off, default);
        Assert.Equal(" Písnička🎵.mp3", output);
        Assert.Equal(FileBrowserFileKind.Audio, Icons.Classify("Písnička🎵.mp3", false));
    }

    [Fact]
    public void Invalid_presentations_fail_early()
    {
        Assert.Throws<ArgumentNullException>(() =>
            FileBrowserEntryPresentation.Format(null!, false, Icons, FileBrowserIconMode.Off, default));
        Assert.Throws<ArgumentNullException>(() =>
            FileBrowserEntryPresentation.Format("file", false, null!, FileBrowserIconMode.Off, default));
        Assert.Throws<ArgumentException>(() =>
            FileBrowserEntryPresentation.Format("", false, Icons, FileBrowserIconMode.Off, default));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            FileBrowserEntryPresentation.Format("file", false, Icons, FileBrowserIconMode.Off, default,
                FileBrowserKnownFolderKind.UserHome));
    }
}
