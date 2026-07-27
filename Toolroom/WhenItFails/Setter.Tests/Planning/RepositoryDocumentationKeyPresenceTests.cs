using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Planning;

public sealed class RepositoryDocumentationKeyPresenceTests
{
    [Fact]
    public async Task RepositoryErrorCatalog_HasCompleteUniqueDocumentationKeys()
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
        Assert.NotNull(catalog.Errors);
        Assert.NotEmpty(catalog.Errors);

        DocumentationKeyCheckReport report =
            new WhenItFailsDocumentationKeyChecker().Check(catalog);

        Assert.Equal(catalog.Errors.Count, report.TotalErrors);
        Assert.True(
            report.IsValid,
            string.Join(
                Environment.NewLine,
                report.MissingKeys.Select(issue =>
                    $"Missing: {issue.ErrorId} ({issue.ErrorCode})")
                    .Concat(report.DuplicateKeys.Select(group =>
                        $"Duplicate: {group.DocumentationKey}"))));
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
}
