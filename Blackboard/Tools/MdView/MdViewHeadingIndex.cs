using System.Globalization;
using System.Text;
using Afrowave.Toolbox.ExchangeHouse.Markdown;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>One recognized Markdown heading and its unique document anchor.</summary>
/// <param name="Anchor">Normalized heading slug, including duplicate suffix.</param>
/// <param name="Title">Recognized heading text.</param>
/// <param name="SourceOffset">UTF-16 position of the heading in the document.</param>
public sealed record MdViewHeadingAnchor(string Anchor, string Title, int SourceOffset);

/// <summary>
/// In-memory heading index for the opt-in small-document browser.
/// Uses existing Markdown block classification so fenced-code headings
/// are never indexed.
/// </summary>
public static class MdViewHeadingIndex
{
    /// <summary>Indexes recognized ATX headings in a bounded Markdown document.</summary>
    /// <param name="markdown">Complete decoded Markdown text.</param>
    /// <returns>Ordered headings with unique, GitHub-style lowercase slugs.</returns>
    public static IReadOnlyList<MdViewHeadingAnchor> Create(string markdown)
    {
        ArgumentNullException.ThrowIfNull(markdown);
        if (markdown.Length > MdViewDocumentLinkExtractor.MaximumDocumentCharacters)
            throw new ArgumentOutOfRangeException(nameof(markdown));

        var headings = new List<MdViewHeadingAnchor>();
        var used = new HashSet<string>(StringComparer.Ordinal);
        foreach (var block in MarkdownParser.Parse(markdown).Blocks)
        {
            if (block.Kind != MarkdownBlockKind.Heading)
                continue;
            var title = block.Text.Trim().TrimEnd('#').TrimEnd();
            var slug = Slugify(title);
            var unique = slug;
            var number = 0;
            while (!used.Add(unique))
                unique = slug + "-" + (++number).ToString(CultureInfo.InvariantCulture);
            headings.Add(new MdViewHeadingAnchor(unique, title, block.Start));
        }
        return headings;
    }

    /// <summary>Finds an exact normalized anchor; returns null if absent.</summary>
    /// <param name="headings">Heading index for the current document.</param>
    /// <param name="fragment">Fragment without the leading #.</param>
    /// <returns>Recognized destination, if any.</returns>
    public static MdViewHeadingAnchor? Find(
        IReadOnlyList<MdViewHeadingAnchor> headings, string fragment)
    {
        ArgumentNullException.ThrowIfNull(headings);
        ArgumentNullException.ThrowIfNull(fragment);
        return headings.FirstOrDefault(x =>
            string.Equals(x.Anchor, fragment, StringComparison.Ordinal));
    }

    private static string Slugify(string title)
    {
        var builder = new StringBuilder(title.Length);
        foreach (var c in title.ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(c) || c is '_' or '-')
                builder.Append(c);
            else if (char.IsWhiteSpace(c))
                builder.Append('-');
        }
        return builder.ToString();
    }
}
