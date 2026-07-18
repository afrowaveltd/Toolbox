using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Planning;

public sealed class WhenItFailsDocumentationLinkCheckerTests
{
    [Fact]
    public async Task CheckAsync_WithValidLocalLinks_ReturnsSuccess()
    {
        string setterPath = CreateSetterDirectory();

        try
        {
            string docsPath = Path.Combine(setterPath, "Docs");
            string targetPath = Path.Combine(docsPath, "Getting Started.md");
            await File.WriteAllTextAsync(targetPath, "# Getting started");
            await File.WriteAllTextAsync(
                Path.Combine(setterPath, "README.md"),
                "[Encoded](Docs/Getting%20Started.md#section)\n" +
                "[Raw spaces](Docs/Getting Started.md)\n" +
                "[With title](Docs/Getting%20Started.md \"Getting started\")\n" +
                "[Web](https://example.com/docs)\n" +
                "[Anchor](#overview)");

            Response<DocumentationLinkCheckReport> response =
                await new WhenItFailsDocumentationLinkChecker().CheckAsync(setterPath);

            Assert.True(response.IsSuccess);
            Assert.NotNull(response.Data);
            Assert.Equal(2, response.Data.MarkdownFilesChecked);
            Assert.Equal(3, response.Data.LocalLinksChecked);
            Assert.Empty(response.Data.BrokenLinks);
        }
        finally
        {
            Directory.Delete(setterPath, recursive: true);
        }
    }

    [Fact]
    public async Task CheckAsync_WithBrokenLocalLink_ReturnsReportWithFailure()
    {
        string setterPath = CreateSetterDirectory();

        try
        {
            await File.WriteAllTextAsync(
                Path.Combine(setterPath, "README.md"),
                "[Missing](Docs/Missing.md)");

            Response<DocumentationLinkCheckReport> response =
                await new WhenItFailsDocumentationLinkChecker().CheckAsync(setterPath);

            Assert.False(response.IsSuccess);
            Assert.NotNull(response.Data);
            BrokenDocumentationLink brokenLink = Assert.Single(response.Data.BrokenLinks);
            Assert.Equal("README.md", brokenLink.SourceFile);
            Assert.Equal("Docs/Missing.md", brokenLink.Target);
            Assert.Contains(
                response.Issues,
                issue => issue.Code == "BrokenDocumentationLinksFound");
        }
        finally
        {
            Directory.Delete(setterPath, recursive: true);
        }
    }

    [Fact]
    public async Task CheckAsync_WithRepositoryRoot_ResolvesOnlySetterDirectory()
    {
        string repositoryPath = Path.Combine(
            Path.GetTempPath(),
            "Afrowave.Toolbox.Tests",
            Guid.NewGuid().ToString("N"));
        string setterPath = Path.Combine(
            repositoryPath,
            "Toolroom",
            "WhenItFails",
            "Setter");
        CreateSetterDirectory(setterPath);
        await File.WriteAllTextAsync(Path.Combine(setterPath, "README.md"), "# Setter");

        Directory.CreateDirectory(Path.Combine(repositoryPath, "Docs"));
        await File.WriteAllTextAsync(
            Path.Combine(repositoryPath, "README.md"),
            "[This must not be scanned](Docs/Missing.md)");

        try
        {
            Response<DocumentationLinkCheckReport> response =
                await new WhenItFailsDocumentationLinkChecker().CheckAsync(repositoryPath);

            Assert.True(response.IsSuccess);
            Assert.NotNull(response.Data);
            Assert.Equal(Path.GetFullPath(setterPath), response.Data.SetterPath);
            Assert.Empty(response.Data.BrokenLinks);
        }
        finally
        {
            Directory.Delete(repositoryPath, recursive: true);
        }
    }

    [Fact]
    public async Task CheckAsync_IgnoresReadmeSourceDirectory()
    {
        string setterPath = CreateSetterDirectory();

        try
        {
            await File.WriteAllTextAsync(Path.Combine(setterPath, "README.md"), "# Setter");
            string sourceDirectory = Path.Combine(setterPath, "Readme");
            Directory.CreateDirectory(sourceDirectory);
            await File.WriteAllTextAsync(
                Path.Combine(sourceDirectory, "en.md"),
                "[Generated-relative link](Docs/Missing.md)");

            Response<DocumentationLinkCheckReport> response =
                await new WhenItFailsDocumentationLinkChecker().CheckAsync(setterPath);

            Assert.True(response.IsSuccess);
            Assert.NotNull(response.Data);
            Assert.Equal(1, response.Data.MarkdownFilesChecked);
            Assert.Empty(response.Data.BrokenLinks);
        }
        finally
        {
            Directory.Delete(setterPath, recursive: true);
        }
    }

    [Fact]
    public async Task CheckAsync_WithMissingSetterDirectory_ReturnsNotFound()
    {
        string missingPath = Path.Combine(
            Path.GetTempPath(),
            "Afrowave.Toolbox.Tests",
            Guid.NewGuid().ToString("N"));

        Response<DocumentationLinkCheckReport> response =
            await new WhenItFailsDocumentationLinkChecker().CheckAsync(missingPath);

        Assert.False(response.IsSuccess);
        Assert.Contains(
            response.Issues,
            issue => issue.Code == "SetterDocumentationDirectoryNotFound");
    }

    [Fact]
    public async Task CheckAsync_WithEmptyPath_ReturnsInvalid()
    {
        Response<DocumentationLinkCheckReport> response =
            await new WhenItFailsDocumentationLinkChecker().CheckAsync(" ");

        Assert.False(response.IsSuccess);
        Assert.Contains(
            response.Issues,
            issue => issue.Code == "DocumentationPathIsEmpty");
    }

    private static string CreateSetterDirectory()
    {
        string setterPath = Path.Combine(
            Path.GetTempPath(),
            "Afrowave.Toolbox.Tests",
            Guid.NewGuid().ToString("N"),
            "Setter");
        CreateSetterDirectory(setterPath);
        return setterPath;
    }

    private static void CreateSetterDirectory(string setterPath)
    {
        Directory.CreateDirectory(Path.Combine(setterPath, "Commands"));
        Directory.CreateDirectory(Path.Combine(setterPath, "Docs"));
        File.WriteAllText(Path.Combine(setterPath, "Program.cs"), "// test Setter marker");
    }
}