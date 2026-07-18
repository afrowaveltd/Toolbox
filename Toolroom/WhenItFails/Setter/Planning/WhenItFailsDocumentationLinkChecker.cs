using System.Text.RegularExpressions;
using Afrowave.Toolbox.Essentials.Results;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;

/// <summary>
/// Checks local Markdown links in the canonical Setter documentation without modifying files.
/// </summary>
internal sealed class WhenItFailsDocumentationLinkChecker
{
    private static readonly Regex MarkdownLinkRegex = new(
        @"!?\[[^\]]*\]\((?<target><[^>]+>|[^)\r\n]+)\)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex OptionalTitleRegex = new(
        @"^(?<path>.+?)\s+(?:""[^""]*""|'[^']*'|\([^\)]*\))$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex FencedCodeBlockRegex = new(
        @"(?ms)^[ \t]*(?<fence>`{3,}|~{3,})[^\r\n]*\r?\n.*?^[ \t]*\k<fence>[ \t]*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex InlineCodeRegex = new(
        @"(?<!`)`+[^\r\n]*?`+(?!`)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>
    /// Checks README.md and Markdown files beneath the Setter Docs directory.
    /// </summary>
    public async Task<Response<DocumentationLinkCheckReport>> CheckAsync(
        string inputPath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(inputPath))
        {
            return Response<DocumentationLinkCheckReport>.Invalid(
                code: "DocumentationPathIsEmpty",
                message: "Repository root or Setter directory path cannot be empty.");
        }

        string setterPath = ResolveSetterPath(inputPath);
        if (!IsSetterDocumentationDirectory(setterPath))
        {
            return Response<DocumentationLinkCheckReport>.NotFound(
                code: "SetterDocumentationDirectoryNotFound",
                message: $"Setter directory was not found: {setterPath}");
        }

        string[] markdownFiles = EnumerateCanonicalMarkdownFiles(setterPath)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        List<BrokenDocumentationLink> brokenLinks = [];
        int checkedLinks = 0;

        foreach (string markdownPath in markdownFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string content = await File.ReadAllTextAsync(markdownPath, cancellationToken);
            string checkableContent = RemoveCode(content);

            foreach (Match match in MarkdownLinkRegex.Matches(checkableContent))
            {
                string rawTarget = ExtractTarget(match.Groups["target"].Value);
                if (ShouldIgnore(rawTarget))
                {
                    continue;
                }

                checkedLinks++;
                string localTarget = NormalizeLocalTarget(rawTarget);
                string resolvedPath = Path.GetFullPath(
                    Path.Combine(Path.GetDirectoryName(markdownPath)!, localTarget));

                if (File.Exists(resolvedPath) || Directory.Exists(resolvedPath))
                {
                    continue;
                }

                brokenLinks.Add(new BrokenDocumentationLink(
                    SourceFile: Path.GetRelativePath(setterPath, markdownPath),
                    Target: rawTarget,
                    ResolvedPath: resolvedPath));
            }
        }

        DocumentationLinkCheckReport report = new(
            SetterPath: setterPath,
            MarkdownFilesChecked: markdownFiles.Length,
            LocalLinksChecked: checkedLinks,
            BrokenLinks: brokenLinks);

        if (brokenLinks.Count == 0)
        {
            return Response<DocumentationLinkCheckReport>.Ok(
                report,
                $"Checked {checkedLinks} local Markdown link(s); no broken links were found.");
        }

        Response<DocumentationLinkCheckReport> invalidResponse =
            Response<DocumentationLinkCheckReport>.Invalid(
                code: "BrokenDocumentationLinksFound",
                message: $"Found {brokenLinks.Count} broken local Markdown link(s).");

        return Response<DocumentationLinkCheckReport>.WithData(invalidResponse, report);
    }

    private static string ResolveSetterPath(string inputPath)
    {
        string fullPath = Path.GetFullPath(inputPath.Trim());
        string nestedSetterPath = Path.Combine(
            fullPath,
            "Toolroom",
            "WhenItFails",
            "Setter");

        if (IsSetterDocumentationDirectory(nestedSetterPath))
        {
            return nestedSetterPath;
        }

        return fullPath;
    }

    private static bool IsSetterDocumentationDirectory(string path)
    {
        return Directory.Exists(path)
            && File.Exists(Path.Combine(path, "README.md"))
            && Directory.Exists(Path.Combine(path, "Docs"));
    }

    private static IEnumerable<string> EnumerateCanonicalMarkdownFiles(string setterPath)
    {
        string readmePath = Path.Combine(setterPath, "README.md");
        if (File.Exists(readmePath))
        {
            yield return readmePath;
        }

        string docsPath = Path.Combine(setterPath, "Docs");
        foreach (string markdownPath in Directory.EnumerateFiles(
                     docsPath,
                     "*.md",
                     SearchOption.AllDirectories))
        {
            yield return markdownPath;
        }
    }

    private static string RemoveCode(string content)
    {
        string withoutFencedBlocks = FencedCodeBlockRegex.Replace(content, string.Empty);
        return InlineCodeRegex.Replace(withoutFencedBlocks, string.Empty);
    }

    private static string ExtractTarget(string targetExpression)
    {
        string trimmed = targetExpression.Trim();
        if (trimmed.StartsWith('<'))
        {
            int closingBracket = trimmed.IndexOf('>');
            return closingBracket > 0
                ? trimmed[1..closingBracket]
                : trimmed.Trim('<', '>');
        }

        Match titleMatch = OptionalTitleRegex.Match(trimmed);
        return titleMatch.Success
            ? titleMatch.Groups["path"].Value.Trim()
            : trimmed;
    }

    private static bool ShouldIgnore(string target)
    {
        if (string.IsNullOrWhiteSpace(target) || target.StartsWith('#'))
        {
            return true;
        }

        return Uri.TryCreate(target, UriKind.Absolute, out _);
    }

    private static string NormalizeLocalTarget(string target)
    {
        int fragmentIndex = target.IndexOf('#');
        if (fragmentIndex >= 0)
        {
            target = target[..fragmentIndex];
        }

        int queryIndex = target.IndexOf('?');
        if (queryIndex >= 0)
        {
            target = target[..queryIndex];
        }

        return Uri.UnescapeDataString(target.Replace('/', Path.DirectorySeparatorChar));
    }
}

/// <summary>
/// Result of checking local Markdown links.
/// </summary>
internal sealed record DocumentationLinkCheckReport(
    string SetterPath,
    int MarkdownFilesChecked,
    int LocalLinksChecked,
    IReadOnlyList<BrokenDocumentationLink> BrokenLinks);

/// <summary>
/// Describes one unresolved local Markdown link.
/// </summary>
internal sealed record BrokenDocumentationLink(
    string SourceFile,
    string Target,
    string ResolvedPath);