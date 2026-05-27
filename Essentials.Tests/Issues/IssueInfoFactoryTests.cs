using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Issues;
using Afrowave.Toolbox.Essentials.Metadata;

namespace Afrowave.Toolbox.Essentials.Tests.Issues;

public sealed class IssueInfoFactoryTests
{
   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Create_WhenCodeIsInvalid_ThrowsArgumentException(string? code)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
          IssueInfoFactory.Create(code!, "Message.", IssueSeverity.Warning));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Create_WhenMessageIsInvalid_ThrowsArgumentException(string? message)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
          IssueInfoFactory.Create("AFW001", message!, IssueSeverity.Warning));
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
   public void Create_WithSeverity_CreatesIssue(IssueSeverity severity)
   {
      var issue = IssueInfoFactory.Create(
          "AFW001",
          "Test issue.",
          severity);

      Assert.Equal("AFW001", issue.Code);
      Assert.Equal("Test issue.", issue.Message);
      Assert.Equal(severity, issue.Severity);
      Assert.Null(issue.Number);
      Assert.Null(issue.Details);
      Assert.NotNull(issue.Metadata);
      Assert.True(issue.Metadata.IsEmpty);
   }

   [Fact]
   public void Create_WithDetails_CreatesIssueWithDetails()
   {
      var issue = IssueInfoFactory.Create(
          "AFW001",
          "Test issue.",
          "Detailed information.",
          IssueSeverity.Warning);

      Assert.Equal("AFW001", issue.Code);
      Assert.Equal("Test issue.", issue.Message);
      Assert.Equal("Detailed information.", issue.Details);
      Assert.Equal(IssueSeverity.Warning, issue.Severity);
      Assert.Null(issue.Number);
      Assert.NotNull(issue.Metadata);
      Assert.True(issue.Metadata.IsEmpty);
   }

   [Fact]
   public void Create_WithDetails_AllowsNullDetails()
   {
      var issue = IssueInfoFactory.Create(
          "AFW001",
          "Test issue.",
          null,
          IssueSeverity.Warning);

      Assert.Equal("AFW001", issue.Code);
      Assert.Equal("Test issue.", issue.Message);
      Assert.Null(issue.Details);
      Assert.Equal(IssueSeverity.Warning, issue.Severity);
   }

   [Fact]
   public void Information_CreatesInformationIssue()
   {
      var issue = IssueInfoFactory.Information(
          "AFW_INFO",
          "Information message.");

      Assert.Equal("AFW_INFO", issue.Code);
      Assert.Equal("Information message.", issue.Message);
      Assert.Equal(IssueSeverity.Information, issue.Severity);
   }

   [Fact]
   public void Warning_CreatesWarningIssue()
   {
      var issue = IssueInfoFactory.Warning(
          "AFW_WARNING",
          "Warning message.");

      Assert.Equal("AFW_WARNING", issue.Code);
      Assert.Equal("Warning message.", issue.Message);
      Assert.Equal(IssueSeverity.Warning, issue.Severity);
   }

   [Fact]
   public void Error_CreatesErrorIssue()
   {
      var issue = IssueInfoFactory.Error(
          "AFW_ERROR",
          "Error message.");

      Assert.Equal("AFW_ERROR", issue.Code);
      Assert.Equal("Error message.", issue.Message);
      Assert.Equal(IssueSeverity.Error, issue.Severity);
   }

   [Fact]
   public void Critical_CreatesCriticalIssue()
   {
      var issue = IssueInfoFactory.Critical(
          "AFW_CRITICAL",
          "Critical message.");

      Assert.Equal("AFW_CRITICAL", issue.Code);
      Assert.Equal("Critical message.", issue.Message);
      Assert.Equal(IssueSeverity.Critical, issue.Severity);
   }

   [Fact]
   public void Fatal_CreatesFatalIssue()
   {
      var issue = IssueInfoFactory.Fatal(
          "AFW_FATAL",
          "Fatal message.");

      Assert.Equal("AFW_FATAL", issue.Code);
      Assert.Equal("Fatal message.", issue.Message);
      Assert.Equal(IssueSeverity.Fatal, issue.Severity);
   }

   [Fact]
   public void WithMetadata_WhenIssueIsNull_ThrowsArgumentNullException()
   {
      IssueInfo? issue = null;
      var metadata = new MetadataBag();

      Assert.Throws<ArgumentNullException>(() =>
          IssueInfoFactory.WithMetadata(issue!, metadata));
   }

   [Fact]
   public void WithMetadata_WhenMetadataIsNull_ThrowsArgumentNullException()
   {
      var issue = IssueInfoFactory.Warning(
          "AFW_WARNING",
          "Warning message.");

      MetadataBag? metadata = null;

      Assert.Throws<ArgumentNullException>(() =>
          IssueInfoFactory.WithMetadata(issue, metadata!));
   }

   [Fact]
   public void WithMetadata_CreatesCopyWithSpecifiedMetadata()
   {
      var issue = new IssueInfo
      {
         Code = "AFW001",
         Number = 42,
         Message = "Test issue.",
         Details = "Detailed information.",
         Severity = IssueSeverity.Warning
      };

      var metadata = new MetadataBag();
      metadata.Set("provider", "ollama-local");
      metadata.Set("profile", "markdown-refine");

      var copy = IssueInfoFactory.WithMetadata(issue, metadata);

      Assert.NotSame(issue, copy);

      Assert.Equal(issue.Code, copy.Code);
      Assert.Equal(issue.Number, copy.Number);
      Assert.Equal(issue.Message, copy.Message);
      Assert.Equal(issue.Details, copy.Details);
      Assert.Equal(issue.Severity, copy.Severity);

      Assert.Same(metadata, copy.Metadata);
      Assert.Equal(2, copy.Metadata.Count);
      Assert.True(copy.Metadata.TryGet("provider", out var provider));
      Assert.True(copy.Metadata.TryGet("profile", out var profile));
      Assert.Equal("ollama-local", provider);
      Assert.Equal("markdown-refine", profile);
   }

   [Fact]
   public void WithMetadata_DoesNotModifyOriginalIssue()
   {
      var originalMetadata = new MetadataBag();
      originalMetadata.Set("original", "yes");

      var issue = new IssueInfo
      {
         Code = "AFW001",
         Message = "Test issue.",
         Severity = IssueSeverity.Warning,
         Metadata = originalMetadata
      };

      var newMetadata = new MetadataBag();
      newMetadata.Set("new", "yes");

      var copy = IssueInfoFactory.WithMetadata(issue, newMetadata);

      Assert.Same(originalMetadata, issue.Metadata);
      Assert.Same(newMetadata, copy.Metadata);

      Assert.True(issue.Metadata.TryGet("original", out var originalValue));
      Assert.False(issue.Metadata.TryGet("new", out _));
      Assert.Equal("yes", originalValue);

      Assert.True(copy.Metadata.TryGet("new", out var newValue));
      Assert.False(copy.Metadata.TryGet("original", out _));
      Assert.Equal("yes", newValue);
   }
}