using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Issues;
using Afrowave.Toolbox.Essentials.Metadata;

namespace Afrowave.Toolbox.Essentials.Tests.Issues;

/// <summary>
/// Tests for the <see cref="IssueInfo"/> class.
/// </summary>
public class IssueInfoTests
{
    [Fact]
    public void DefaultConstruction_HasExpectedDefaults()
    {
        // Act
        var issue = new IssueInfo();

        // Assert
        Assert.Equal(string.Empty, issue.Code);
        Assert.Null(issue.Number);
        Assert.Equal(string.Empty, issue.Message);
        Assert.Null(issue.Details);
        Assert.Equal(IssueSeverity.Error, issue.Severity);
        Assert.NotNull(issue.Metadata);
        Assert.True(issue.Metadata.IsEmpty);
    }

    [Fact]
    public void InitWithCode_SetsCode()
    {
        // Act
        var issue = new IssueInfo { Code = "E001" };

        // Assert
        Assert.Equal("E001", issue.Code);
    }

    [Fact]
    public void InitWithNumber_SetsNumber()
    {
        // Act
        var issue = new IssueInfo { Number = 42 };

        // Assert
        Assert.Equal(42, issue.Number);
    }

    [Fact]
    public void InitWithMessage_SetsMessage()
    {
        // Act
        var issue = new IssueInfo { Message = "Something went wrong" };

        // Assert
        Assert.Equal("Something went wrong", issue.Message);
    }

    [Fact]
    public void InitWithDetails_SetsDetails()
    {
        // Act
        var issue = new IssueInfo { Details = "Additional context about the error" };

        // Assert
        Assert.Equal("Additional context about the error", issue.Details);
    }

    [Theory]
    [InlineData(IssueSeverity.None)]
    [InlineData(IssueSeverity.Trace)]
    [InlineData(IssueSeverity.Debug)]
    [InlineData(IssueSeverity.Information)]
    [InlineData(IssueSeverity.Warning)]
    [InlineData(IssueSeverity.Error)]
    [InlineData(IssueSeverity.Critical)]
    [InlineData(IssueSeverity.Fatal)]
    public void InitWithSeverity_SetsSeverity(IssueSeverity severity)
    {
        // Act
        var issue = new IssueInfo { Severity = severity };

        // Assert
        Assert.Equal(severity, issue.Severity);
    }

    [Fact]
    public void InitWithMetadata_SetsMetadata()
    {
        // Arrange
        var metadata = new MetadataBag();
        metadata["source"] = "validator";

        // Act
        var issue = new IssueInfo { Metadata = metadata };

        // Assert
        Assert.Same(metadata, issue.Metadata);
        Assert.Equal("validator", issue.Metadata["source"]);
    }

    [Fact]
    public void CompleteIssue_AllPropertiesSet()
    {
        // Arrange
        var metadata = new MetadataBag();
        metadata["field"] = "username";
        metadata["constraint"] = "length";

        // Act
        var issue = new IssueInfo
        {
            Code = "VAL001",
            Number = 100,
            Message = "Validation failed",
            Details = "Username must be between 3 and 20 characters",
            Severity = IssueSeverity.Warning,
            Metadata = metadata
        };

        // Assert
        Assert.Equal("VAL001", issue.Code);
        Assert.Equal(100, issue.Number);
        Assert.Equal("Validation failed", issue.Message);
        Assert.Equal("Username must be between 3 and 20 characters", issue.Details);
        Assert.Equal(IssueSeverity.Warning, issue.Severity);
        Assert.Equal("username", issue.Metadata["field"]);
        Assert.Equal("length", issue.Metadata["constraint"]);
    }

    [Fact]
    public void ImplementsIHasCode()
    {
        // Arrange
        var issue = new IssueInfo { Code = "TEST001" };

        // Act
        var hasCode = issue as Afrowave.Toolbox.Essentials.Interfaces.IHasCode;

        // Assert
        Assert.NotNull(hasCode);
        Assert.Equal("TEST001", hasCode.Code);
    }

    [Fact]
    public void ImplementsIHasNumber()
    {
        // Arrange
        var issue = new IssueInfo { Number = 999 };

        // Act
        var hasNumber = issue as Afrowave.Toolbox.Essentials.Interfaces.IHasNumber;

        // Assert
        Assert.NotNull(hasNumber);
        Assert.Equal(999, hasNumber.Number);
    }

    [Fact]
    public void ImplementsIHasDetails()
    {
        // Arrange
        var issue = new IssueInfo { Details = "Detailed information" };

        // Act
        var hasDetails = issue as Afrowave.Toolbox.Essentials.Interfaces.IHasDetails;

        // Assert
        Assert.NotNull(hasDetails);
        Assert.Equal("Detailed information", hasDetails.Details);
    }

    [Fact]
    public void ImplementsIHasSeverity()
    {
        // Arrange
        var issue = new IssueInfo { Severity = IssueSeverity.Critical };

        // Act
        var hasSeverity = issue as Afrowave.Toolbox.Essentials.Interfaces.IHasSeverity;

        // Assert
        Assert.NotNull(hasSeverity);
        Assert.Equal(IssueSeverity.Critical, hasSeverity.Severity);
    }

    [Fact]
    public void ImplementsIHasMessage()
    {
        // Arrange
        var issue = new IssueInfo { Message = "Test message" };

        // Act
        var hasMessage = issue as Afrowave.Toolbox.Essentials.Interfaces.IHasMessage;

        // Assert
        Assert.NotNull(hasMessage);
        Assert.Equal("Test message", hasMessage.Message);
    }

    [Fact]
    public void ImplementsIHasMetadata()
    {
        // Arrange
        var metadata = new MetadataBag();
        metadata["key"] = "value";
        var issue = new IssueInfo { Metadata = metadata };

        // Act
        var hasMetadata = issue as Afrowave.Toolbox.Essentials.Interfaces.IHasMetadata;

        // Assert
        Assert.NotNull(hasMetadata);
        Assert.Same(metadata, hasMetadata.Metadata);
    }
}
