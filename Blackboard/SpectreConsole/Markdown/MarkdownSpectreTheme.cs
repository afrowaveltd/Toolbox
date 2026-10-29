using Spectre.Console;

namespace Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown;

/// <summary>Configurable presentation styles, independent of the ExchangeHouse Markdown syntax model.</summary>
public sealed class MarkdownSpectreTheme
{
    /// <summary>Base paragraph styling.</summary>
    public Style Body { get; init; } = Style.Plain;
    /// <summary>Heading level 1.</summary>
    public Style Heading1 { get; init; } = new(foreground: Color.Blue, decoration: Decoration.Bold);
    /// <summary>Heading level 2.</summary>
    public Style Heading2 { get; init; } = new(foreground: Color.Green, decoration: Decoration.Bold);
    /// <summary>Heading level 3.</summary>
    public Style Heading3 { get; init; } = new(foreground: Color.Yellow, decoration: Decoration.Bold);
    /// <summary>Heading level 4.</summary>
    public Style Heading4 { get; init; } = new(decoration: Decoration.Bold);
    /// <summary>Heading level 5.</summary>
    public Style Heading5 { get; init; } = new(decoration: Decoration.Bold);
    /// <summary>Heading level 6.</summary>
    public Style Heading6 { get; init; } = new(decoration: Decoration.Bold);
    /// <summary>Strong inline text.</summary>
    public Style Strong { get; init; } = new(decoration: Decoration.Bold);
    /// <summary>Emphasized inline text.</summary>
    public Style Emphasis { get; init; } = new(decoration: Decoration.Italic);
    /// <summary>Fenced and inline code.</summary>
    public Style Code { get; init; } = new(foreground: Color.Yellow);
    /// <summary>Link label.</summary>
    public Style Link { get; init; } = new(foreground: Color.Blue, decoration: Decoration.Underline);
    /// <summary>Style for an image alternative description in text-only terminals.</summary>
    public Style Image { get; init; } = new(foreground: Color.Green, decoration: Decoration.Italic);
    /// <summary>Style for the optional literal image destination.</summary>
    public Style ImageDestination { get; init; } = Style.Plain;
    /// <summary>Prefix used for accessible image descriptions.</summary>
    public string ImagePrefix { get; init; } = "[Image: ";
    /// <summary>Suffix used for accessible image descriptions.</summary>
    public string ImageSuffix { get; init; } = "]";
    /// <summary>Show image resource destinations as plain text, without opening them.</summary>
    public bool ShowImageDestinations { get; init; }

    /// <summary>Optional link destination.</summary>
    public Style LinkDestination { get; init; } = Style.Plain;
    /// <summary>Bullet or ordered-list marker presentation.</summary>
    public Style ListMarker { get; init; } = new(foreground: Color.Green);
    /// <summary>Replacement marker for list items.</summary>
    public string ListBullet { get; init; } = "•";
    /// <summary>Prefix for fenced code lines.</summary>
    public string CodeIndent { get; init; } = "    ";
    /// <summary>Show code fence delimiters as well as their contents.</summary>
    public bool ShowCodeFenceDelimiters { get; init; }
    /// <summary>Show original link destinations as ordinary text after labels.</summary>
    public bool ShowLinkDestinations { get; init; }

    /// <summary>Get configured style for an ATX heading level 1-6.</summary>
    public Style GetHeadingStyle(int level) => level switch
    {
        1 => Heading1, 2 => Heading2, 3 => Heading3,
        4 => Heading4, 5 => Heading5, 6 => Heading6,
        _ => throw new ArgumentOutOfRangeException(nameof(level))
    };
}
