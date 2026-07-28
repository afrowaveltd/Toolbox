using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Planning;

public sealed class WhenItFailsDocumentationKeyCheckerTests
{
    [Fact]
    public void Check_WithEmptyCatalog_ReturnsValidEmptyReport()
    {
        ErrorCatalogDocument catalog = CreateCatalog();

        DocumentationKeyCheckReport report =
            new WhenItFailsDocumentationKeyChecker().Check(catalog);

        Assert.True(report.IsValid);
        Assert.Equal(0, report.TotalErrors);
        Assert.Empty(report.MissingKeys);
        Assert.Empty(report.DuplicateKeys);
    }

    [Fact]
    public void Check_WithNullErrorCollection_ReturnsValidEmptyReport()
    {
        ErrorCatalogDocument catalog = new()
        {
            Errors = null!
        };

        DocumentationKeyCheckReport report =
            new WhenItFailsDocumentationKeyChecker().Check(catalog);

        Assert.True(report.IsValid);
        Assert.Equal(0, report.TotalErrors);
        Assert.Empty(report.MissingKeys);
        Assert.Empty(report.DuplicateKeys);
    }

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
    public void Check_WithMixedKeyStates_ReturnsCompleteReport()
    {
        ErrorCatalogDocument catalog = CreateCatalog(
            CreateError(1004, "AFW-NET-0004", "Unique", "network.unique"),
            CreateError(1002, "AFW-NET-0002", "Duplicate second", "NETWORK.SHARED"),
            CreateError(1003, "AFW-NET-0003", "Missing", null),
            CreateError(1001, "AFW-NET-0001", "Duplicate first", "network.shared"));

        DocumentationKeyCheckReport report =
            new WhenItFailsDocumentationKeyChecker().Check(catalog);

        Assert.False(report.IsValid);
        Assert.Equal(4, report.TotalErrors);
        Assert.Equal("AFW-NET-0003", Assert.Single(report.MissingKeys).ErrorId);

        DuplicateDocumentationKey duplicate = Assert.Single(report.DuplicateKeys);
        Assert.Equal(
            ["AFW-NET-0001", "AFW-NET-0002"],
            duplicate.Errors.Select(issue => issue.ErrorId));
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
    public void Check_WithEqualMissingKeyCodes_OrdersIssuesByIdIgnoringCase()
    {
        ErrorCatalogDocument catalog = CreateCatalog(
            CreateError(1001, "AFW-NET-B", "Second", null),
            CreateError(1001, "afw-net-a", "First", null));

        DocumentationKeyCheckReport report =
            new WhenItFailsDocumentationKeyChecker().Check(catalog);

        Assert.Equal(
            ["afw-net-a", "AFW-NET-B"],
            report.MissingKeys.Select(issue => issue.ErrorId));
    }

    [Fact]
    public void Check_WithMissingKey_PreservesIssueValues()
    {
        const string documentationKey = "   ";
        ErrorCatalogDocument catalog = CreateCatalog(
            CreateError(1001, "AFW-NET-0001", "Unavailable", documentationKey));

        DocumentationKeyCheckReport report =
            new WhenItFailsDocumentationKeyChecker().Check(catalog);

        DocumentationKeyIssue issue = Assert.Single(report.MissingKeys);
        Assert.Equal("AFW-NET-0001", issue.ErrorId);
        Assert.Equal(1001, issue.ErrorCode);
        Assert.Equal("Unavailable", issue.ErrorName);
        Assert.Equal(documentationKey, issue.DocumentationKey);
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
    public void Check_WithThreeMatchingKeys_ReportsOneCompleteDuplicateGroup()
    {
        ErrorCatalogDocument catalog = CreateCatalog(
            CreateError(1003, "AFW-NET-0003", "Third", "NETWORK.SHARED"),
            CreateError(1001, "AFW-NET-0001", "First", "network.shared"),
            CreateError(1002, "AFW-NET-0002", "Second", " Network.Shared "));

        DocumentationKeyCheckReport report =
            new WhenItFailsDocumentationKeyChecker().Check(catalog);

        DuplicateDocumentationKey duplicate = Assert.Single(report.DuplicateKeys);
        Assert.Equal(
            ["AFW-NET-0001", "AFW-NET-0002", "AFW-NET-0003"],
            duplicate.Errors.Select(issue => issue.ErrorId));
    }

    [Fact]
    public void Check_WithSurroundingWhitespace_TrimsDuplicateGroupKey()
    {
        ErrorCatalogDocument catalog = CreateCatalog(
            CreateError(1002, "AFW-NET-0002", "Second", " Network.Unavailable "),
            CreateError(1001, "AFW-NET-0001", "First", "network.unavailable"));

        DocumentationKeyCheckReport report =
            new WhenItFailsDocumentationKeyChecker().Check(catalog);

        DuplicateDocumentationKey duplicate = Assert.Single(report.DuplicateKeys);
        Assert.Equal("Network.Unavailable", duplicate.DocumentationKey);
    }

    [Fact]
    public void Check_WithEqualDuplicateKeyCodes_OrdersIssuesByIdIgnoringCase()
    {
        ErrorCatalogDocument catalog = CreateCatalog(
            CreateError(1001, "AFW-NET-B", "Second", "network.unavailable"),
            CreateError(1001, "afw-net-a", "First", "NETWORK.UNAVAILABLE"));

        DocumentationKeyCheckReport report =
            new WhenItFailsDocumentationKeyChecker().Check(catalog);

        DuplicateDocumentationKey duplicate = Assert.Single(report.DuplicateKeys);
        Assert.Equal(
            ["afw-net-a", "AFW-NET-B"],
            duplicate.Errors.Select(issue => issue.ErrorId));
    }

    [Fact]
    public void Check_WithDuplicateKeys_PreservesIssueValues()
    {
        const string firstDocumentationKey = "network.unavailable";
        const string secondDocumentationKey = " Network.Unavailable ";
        ErrorCatalogDocument catalog = CreateCatalog(
            CreateError(1002, "AFW-NET-0002", "Second", secondDocumentationKey),
            CreateError(1001, "AFW-NET-0001", "First", firstDocumentationKey));

        DocumentationKeyCheckReport report =
            new WhenItFailsDocumentationKeyChecker().Check(catalog);

        DuplicateDocumentationKey duplicate = Assert.Single(report.DuplicateKeys);
        Assert.Collection(
            duplicate.Errors,
            issue =>
            {
                Assert.Equal("AFW-NET-0001", issue.ErrorId);
                Assert.Equal(1001, issue.ErrorCode);
                Assert.Equal("First", issue.ErrorName);
                Assert.Equal(firstDocumentationKey, issue.DocumentationKey);
            },
            issue =>
            {
                Assert.Equal("AFW-NET-0002", issue.ErrorId);
                Assert.Equal(1002, issue.ErrorCode);
                Assert.Equal("Second", issue.ErrorName);
                Assert.Equal(secondDocumentationKey, issue.DocumentationKey);
            });
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
    public void Check_WithMultipleDuplicateGroups_KeepsGroupMembersIsolated()
    {
        ErrorCatalogDocument catalog = CreateCatalog(
            CreateError(1004, "AFW-A-0004", "Alpha second", "ALPHA.KEY"),
            CreateError(1001, "AFW-Z-0001", "Zeta first", "zeta.key"),
            CreateError(1002, "AFW-A-0002", "Alpha first", "alpha.key"),
            CreateError(1003, "AFW-Z-0003", "Zeta second", "ZETA.KEY"));

        DocumentationKeyCheckReport report =
            new WhenItFailsDocumentationKeyChecker().Check(catalog);

        Assert.Collection(
            report.DuplicateKeys,
            group => Assert.Equal(
                ["AFW-A-0002", "AFW-A-0004"],
                group.Errors.Select(issue => issue.ErrorId)),
            group => Assert.Equal(
                ["AFW-Z-0001", "AFW-Z-0003"],
                group.Errors.Select(issue => issue.ErrorId)));
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
