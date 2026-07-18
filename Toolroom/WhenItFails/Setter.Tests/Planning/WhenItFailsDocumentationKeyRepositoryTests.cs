using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Planning;

public sealed class WhenItFailsDocumentationKeyRepositoryTests
{
    [Fact]
    public async Task RepositoryErrorCatalog_HasUniqueNonEmptyDocumentationKeys()
    {
        string repositoryRoot = FindRepositoryRoot();
        string catalogPath = Path.Combine(
            repositoryRoot,
            "Jsons",
            "WhenItFails",
            "errors.en.json");

        var loadResponse = await new JsonErrorCatalogLoader().LoadFromFileAsync(catalogPath);

        Assert.True(
            loadResponse.IsSuccess,
            $"Repository error catalog could not be loaded: {loadResponse.Message}");

        ErrorCatalogDocument catalog = Assert.IsType<ErrorCatalogDocument>(loadResponse.Data);
        DocumentationKeyCheckReport report =
            new WhenItFailsDocumentationKeyChecker().Check(catalog);

        Assert.True(
            report.IsValid,
            CreateFailureMessage(report));
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Toolbox.sln")) &&
                Directory.Exists(Path.Combine(directory.FullName, "Jsons", "WhenItFails")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not locate the Toolbox repository root from the test output directory.");
    }

    private static string CreateFailureMessage(DocumentationKeyCheckReport report)
    {
        IEnumerable<string> missing = report.MissingKeys.Select(
            issue => $"missing: {issue.ErrorId} ({issue.ErrorCode})");
        IEnumerable<string> duplicates = report.DuplicateKeys.Select(
            duplicate =>
                $"duplicate: {duplicate.DocumentationKey} => " +
                string.Join(", ", duplicate.Errors.Select(issue => issue.ErrorId)));

        return string.Join(
            Environment.NewLine,
            new[] { "Repository documentation-key validation failed." }
                .Concat(missing)
                .Concat(duplicates));
    }
}
