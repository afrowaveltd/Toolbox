using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Planning;

public sealed class WhenItFailsDocumentationKeyCheckerTests
{
    [Fact]
    public void Check_WithUniqueNonEmptyKeys_ReturnsValidReport()
    {
        ErrorCatalogDocument catalog = CreateCatalog(
            CreateError(1002, "AFW-NET-0002", "Timeout", "network.timeout"),
            CreateError(1001, "AFW-NET-0001", "Unavailable", "network.unavailable"));

        DocumentationKeyCheckReport report =
            new WhenItFailsDocumentationKeyChecker().Check(catalog);

        Assert.True(report.IsValid);
        Assert.Equal(2, report.TotalErrors);
        Assert.Empty(report.MissingKeys);
        Assert.Empty(report.DuplicateKeys);
    }

    [Fact]
    public void Check_WithNullEmptyAndWhitespaceKeys_ReportsMissingKeysInCodeOrder()
    {
        ErrorCatalogDocument catalog = CreateCatalog(
            CreateError(1003, "AFW-NET-0003", "Third", " "),
            CreateError(1001, "AFW-NET-0001", "First", null),
            CreateError(1002, "AFW-NET-0002", "Second", string.Empty));

        DocumentationKeyCheckReport report =
            new WhenItFailsDocumentationKeyChecker().Check(catalog);

        Assert.False(report.IsValid);
        Assert.Equal(
            [1001, 1002, 1003],
            report.MissingKeys.Select(issue => issue.ErrorCode));
        Assert.Empty(report.DuplicateKeys);
    }

    [Fact]
    public void Check_WithCaseInsensitiveDuplicateKeys_ReportsOneDuplicateGroup()
    {
        ErrorCatalogDocument catalog = CreateCatalog(
            CreateError(1002, "AFW-NET-0002", "Second", " Network.Unavailable "),
            CreateError(1001, "AFW-NET-0001", "First", "network.unavailable"),
            CreateError(1003, "AFW-NET-0003", "Third", "network.timeout"));

        DocumentationKeyCheckReport report =
            new WhenItFailsDocumentationKeyChecker().Check(catalog);

        Assert.False(report.IsValid);
        DuplicateDocumentationKey duplicate = Assert.Single(report.DuplicateKeys);
        Assert.Equal("network.unavailable", duplicate.DocumentationKey, ignoreCase: true);
        Assert.Equal(
            [1001, 1002],
            duplicate.Errors.Select(issue => issue.ErrorCode));
    }

    [Fact]
    public void Check_WithMultipleDuplicateGroups_OrdersGroupsByKey()
    {
        ErrorCatalogDocument catalog = CreateCatalog(
            CreateError(1001, "AFW-A-0001", "A1", "zeta.key"),
            CreateError(1002, "AFW-A-0002", "A2", "alpha.key"),
            CreateError(1003, "AFW-A-0003", "A3", "ZETA.KEY"),
            CreateError(1004, "AFW-A-0004", "A4", "ALPHA.KEY"));

        DocumentationKeyCheckReport report =
            new WhenItFailsDocumentationKeyChecker().Check(catalog);

        Assert.Equal(
            ["alpha.key", "zeta.key"],
            report.DuplicateKeys.Select(group => group.DocumentationKey),
            StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void Check_WithNullCatalog_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new WhenItFailsDocumentationKeyChecker().Check(null!));
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
        string id,
        string name,
        string? documentationKey)
    {
        return new ErrorDefinition
        {
            Code = code,
            Id = id,
            Name = name,
            DocumentationKey = documentationKey
        };
    }
}