using Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser.Tests;

public sealed class FileBrowserTerminalDetectionTests
{
    [Theory]
    [InlineData("xterm-256color", "C.UTF-8")]
    [InlineData("screen-256color", "cs_CZ.UTF-8")]
    [InlineData("tmux-256color", "en_US.utf8")]
    [InlineData(null, "C.UTF-8")]
    public void Interactive_unix_utf8_locale_recovers_false_negative_from_spectre(
        string? term, string locale)
    {
        Assert.True(FileBrowserTerminalDetection.SupportsUnicode(
            false, true, false, true, term, locale));
    }

    [Theory]
    [InlineData(false, false, true, "xterm", "C.UTF-8")]
    [InlineData(true, true, true, "xterm", "C.UTF-8")]
    [InlineData(true, false, true, "dumb", "C.UTF-8")]
    [InlineData(true, false, false, "xterm", "C.UTF-8")]
    [InlineData(true, false, true, "xterm", "C")]
    [InlineData(true, false, true, "xterm", null)]
    public void Unsafe_or_unknown_contexts_do_not_force_unicode(
        bool interactive, bool redirected, bool unix, string term, string? locale)
    {
        Assert.False(FileBrowserTerminalDetection.SupportsUnicode(
            false, interactive, redirected, unix, term, locale));
    }

    [Fact]
    public void Supported_spectre_profile_is_respected_for_interactive_terminal()
    {
        Assert.True(FileBrowserTerminalDetection.SupportsUnicode(
            true, true, false, false, "xterm", null));
    }

    [Fact]
    public void Fallback_does_not_enable_emoji()
    {
        var unicode = FileBrowserTerminalDetection.SupportsUnicode(
            false, true, false, true, "xterm", "C.UTF-8");
        var caps = new FileBrowserTerminalCapabilities(unicode, true);
        Assert.Equal("▸", FileBrowserFileIcons.GetIcon(
            FileBrowserFileKind.Directory, FileBrowserIconMode.Auto, caps));
        Assert.NotEqual("📁", FileBrowserFileIcons.GetIcon(
            FileBrowserFileKind.Directory, FileBrowserIconMode.Auto, caps));
    }
}
