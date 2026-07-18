using System.Text.RegularExpressions;
using Afrowave.Toolbox.Essentials.Results;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;

/// <summary>
/// Checks local Markdown links in the Setter documentation without modifying files.
/// </summary>
internal sealed class WhenItFailsDocumentationLinkChecker
{
    private static readonly Regex MarkdownLinkRegex = new(
        @"!?\[[^\]]*\]\((?<target><[^>]+>|[^\s\)]+)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>
    /// Checks all Markdown files beneath the Setter directory.
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
        if (!Directory.Exists(setterPath))
        {
            return Response<DocumentationLinkCheckReport>.NotFound(
                code: "SetterDocumentationDirectoryNotFound",
                message: $"Setter directory was not found: {setterPath}");
        }

        string[] markdownFiles = Directory
            .EnumerateFiles(setterPath, "*.md", SearchOption.AllDirectories)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        List<BrokenDocumentationLink> brokenLinks = [];
        int checkedLinks = 0;

        foreach (string markdownPath in markdownFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string content = await File.ReadAllTextAsync(markdownPath, cancellationToken);

            foreach (Match match in MarkdownLinkRegex.Matches(content))
            {
                string rawTarget = match.Groups["target"].Value.Trim();
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
        if (File.Exists(Path.Combine(fullPath, "README.md"))
            && Directory.Exists(Path.Combine(fullPath, "Docs")))
        {
            return fullPath;
        }

        return Path.Combine(fullPath, "Toolroom", "WhenItFails", "Setter");
    }

    private static bool ShouldIgnore(string target)
    {
        string unwrapped = target.Trim('<', '>');
        if (string.IsNullOrWhiteSpace(unwrapped) || unwrapped.StartsWith('#'))
        {
            return true;
        }

        return Uri.TryCreate(unwrapped, UriKind.Absolute, out _);
    }

    private static string NormalizeLocalTarget(string target)
    {
        string unwrapped = target.Trim('<', '>');
        int fragmentIndex = unwrapped.IndexOf('#');
        if (fragmentIndex >= 0)
        {
            unwrapped = unwrapped[..fragmentIndex];
        }

        int queryIndex = unwrapped.IndexOf('?');
        if (queryIndex >= 0)
        {
            unwrapped = unwrapped[..queryIndex];
        }

        return Uri.UnescapeDataString(unwrapped.Replace('/', Path.DirectorySeparatorChar));
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
