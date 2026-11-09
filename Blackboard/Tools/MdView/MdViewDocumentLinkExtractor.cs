using Afrowave.Toolbox.ExchangeHouse.Markdown;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>A navigable Markdown link with an exact original source offset.</summary>
/// <param name="Label">Visible Markdown label.</param>
/// <param name="Destination">Unmodified destination from the Markdown parser.</param>
/// <param name="SourceOffset">Zero-based UTF-16 source offset of the opening link marker.</param>
/// <param name="Target">Resolved document and optional fragment.</param>
public sealed record MdViewDocumentLink(
    string Label, string Destination, int SourceOffset, MdViewResolvedLink Target);

/// <summary>
/// Bounded opt-in link extraction using the existing source-aware Markdown AST.
/// Does not treat fenced code, inline code, images or escaped syntax as links.
/// </summary>
/// <remarks>
/// This first slice parses a complete, small Markdown string. It is not
/// intended for huge documents; streaming indexed discovery is future work.
/// Unsupported/external links are skipped rather than opened automatically.
/// </remarks>
public static class MdViewDocumentLinkExtractor
{
    /// <summary>Maximum UTF-16 characters accepted for in-memory link extraction.</summary>
    public const int MaximumDocumentCharacters = 1024 * 1024;

    /// <summary>Maximum links retained from one document.</summary>
    public const int MaximumLinks = 4096;

    /// <summary>Extracts resolvable document links without file or network access.</summary>
    /// <param name="source">Complete trusted decoded document text.</param>
    /// <param name="origin">Typed origin used to resolve each link target.</param>
    /// <returns>Links in document order, preserving their source positions.</returns>
    public static IReadOnlyList<MdViewDocumentLink> Extract(
        string source, MdViewDocumentOrigin origin)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(origin);
        if (source.Length > MaximumDocumentCharacters)
            throw new ArgumentOutOfRangeException(nameof(source),
                "Link extraction requires a document no larger than 1 Mi UTF-16 characters.");

        var found = new List<MdViewDocumentLink>();
        foreach (var block in MarkdownParser.Parse(source).Blocks)
        {
            Visit(block.Inlines);
            if (found.Count == MaximumLinks)
                break;
        }
        return found;

        void Visit(IReadOnlyList<MarkdownInlineNode> nodes)
        {
            foreach (var node in nodes)
            {
                if (found.Count >= MaximumLinks)
                    return;
                if (node.Kind == MarkdownInlineKind.Link &&
                    !string.IsNullOrEmpty(node.Destination))
                {
                    try
                    {
                        var target = MdViewLinkResolver.Resolve(origin, node.Destination);
                        found.Add(new MdViewDocumentLink(
                            node.Text, node.Destination, node.Start, target));
                    }
                    catch (ArgumentException)
                    {
                        // External URLs and forbidden relative targets are not
                        // navigable by this intentionally same-source picker.
                    }
                }
                else if (node.Children.Count > 0)
                {
                    Visit(node.Children);
                }
            }
        }
    }
}
