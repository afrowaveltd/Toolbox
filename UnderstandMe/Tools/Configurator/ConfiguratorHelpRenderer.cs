using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown;
using Spectre.Console;

namespace Afrowave.Toolbox.UnderstandMe.Configurator;

/// <summary>
/// Application-specific styling adapter for the reusable Blackboard Markdown renderer.
/// Markdown syntax and safe Spectre.Console output belong to ExchangeHouse / Blackboard,
/// not to this Configurator application.
/// </summary>
public static class ConfiguratorHelpRenderer
{
    // No parsing or rendering implementation is duplicated here. This remains a
    // narrow compatibility facade for callers using the original Help API.
    private static readonly MarkdownSpectreTheme HelpTheme = new()
    {
        ShowLinkDestinations = true,
        CodeIndent = "    "
    };

    /// <summary>
    /// Render offline help using the independent Spectre.Console Markdown component.
    /// No link is opened and no settings or files are modified.
    /// </summary>
    public static void Render(IAnsiConsole console, string markdown) =>
        MarkdownSpectreRenderer.Render(console, markdown, HelpTheme);
}
