using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown;
using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.TextMate;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>Creates a Markdown theme with optional bundled TextMate code-fence grammars.</summary>
public static class MdViewHighlighting
{
    /// <summary>
    /// Enable all available TextMate grammars without coupling the parser or base
    /// renderer to native syntax assets. Unavailable native assets safely degrade
    /// to plain code instead of making the document unreadable.
    /// </summary>
    public static MarkdownSpectreTheme CreateTheme()
    {
        try
        {
            return new MarkdownSpectreTheme
            {
                SyntaxHighlighter = new MarkdownTextMateHighlighter()
            };
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return new MarkdownSpectreTheme();
        }
    }
}
