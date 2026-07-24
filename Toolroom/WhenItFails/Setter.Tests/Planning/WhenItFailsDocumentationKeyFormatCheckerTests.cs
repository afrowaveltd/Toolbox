using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Planning;

public sealed class WhenItFailsDocumentationKeyFormatCheckerTests
{
    [Theory]
    [InlineData("when-it-fails/errors/network/network-unavailable")]
    [InlineData("errors/general/error-404")]
    [InlineData("a/b")]
    public void Check_WithCanonicalKey_ReturnsValidReport(string documentationKey)
    {
        DocumentationKeyFormatCheckReport report =
            new WhenItFailsDocumentationKeyFormatChecker().Check(
                CreateCatalog(CreateError(1001, documentationKey)));

        Assert.True(report.IsValid);
        Assert.Empty(report.InvalidKeys);
    }

    [Theory]
    [InlineData("single-segment")]
    [InlineData("When-It-Fails/errors/network")]
    [InlineData("when_it_fails/errors/network")]
    [InlineData("when-it-fails//network")]
    [InlineData("when-it-fails/errors/network/")]
    [InlineData("/when-it-fails/errors/network")]
    [InlineData("when-it-fails/errors/network unavailable")]
    [InlineData("when-it-fails/errors/-network")]
    [InlineData("when-it-fails/errors/network-")]
    [InlineData("when-it-fails/errors/network--unavailable")]
    [InlineData(" when-it-fails/errors/network")]
    [InlineData("when-it-fails/errors/network ")]
    [InlineData("síť/chyba")]
    public void Check_WithNonCanonicalKey_ReportsInvalidKey(string documentationKey)
    {
        DocumentationKeyFormatCheckReport report =
            new WhenItFailsDocumentationKeyFormatChecker().Check(
                CreateCatalog(CreateError(1001, documentationKey)));

        Assert.False(report.IsValid);
        InvalidDocumentationKeyFormat issue = Assert.Single(report.InvalidKeys);
        Assert.Equal(documentationKey, issue.DocumentationKey);
        Assert.Equal(1001, issue.ErrorCode);
    }

    [Fact]
    public void Check_WithMissingKeys_IgnoresThemBecausePresenceCheckerOwnsThatRule()
    {
        DocumentationKeyFormatCheckReport report =
            new WhenItFailsDocumentationKeyFormatChecker().Check(
                CreateCatalog(
                    CreateError(1001, null),
                    CreateError(1002, string.Empty),
                    CreateError(1003, " ")));

        Assert.True(report.IsValid);
        Assert.Empty(report.InvalidKeys);
    }

    [Fact]
    public void Check_WithMultipleInvalidKeys_OrdersIssuesByCodeThenId()
    {
        ErrorCatalogDocument catalog = CreateCatalog(
            CreateError(1003, "UPPER/third", "AFW-C-0003"),
            CreateError(1001, "UPPER/first", "AFW-B-0001"),
            CreateError(1001, "UPPER/second", "AFW-A-0001"));

        DocumentationKeyFormatCheckReport report =
            new WhenItFailsDocumentationKeyFormatChecker().Check(catalog);

        Assert.Equal(
            ["AFW-A-0001", "AFW-B-0001", "AFW-C-0003"],
            report.InvalidKeys.Select(issue => issue.ErrorId));
    }

    [Fact]
    public async Task RepositoryErrorCatalog_UsesCanonicalDocumentationKeys()
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
        DocumentationKeyFormatCheckReport report =
            new WhenItFailsDocumentationKeyFormatChecker().Check(catalog);

        Assert.True(
            report.IsValid,
            string.Join(
                Environment.NewLine,
                report.InvalidKeys.Select(issue =>
                    $"{issue.ErrorId} ({issue.ErrorCode}): {issue.DocumentationKey}")));
    }

    [Fact]
    public void Check_WithNullCatalog_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new WhenItFailsDocumentationKeyFormatChecker().Check(null!));
    }

    private static ErrorCatalogDocument CreateCatalog(params ErrorDefinition[] errors)
    {
        return new ErrorCatalogDocument
        {
            Errors = errors.ToList()
        };
    }

    private static ErrorDefinition CreateError(
        int code,
        string? documentationKey,
        string? id = null)
    {
        return new ErrorDefinition
        {
            Code = code,
            Id = id ?? $"AFW-TEST-{code}",
            Name = $"ERROR{code}",
            DocumentationKey = documentationKey
        };
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
