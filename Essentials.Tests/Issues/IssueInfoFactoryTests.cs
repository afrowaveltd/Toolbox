using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Issues;
using Afrowave.Toolbox.Essentials.Metadata;

namespace Afrowave.Toolbox.Essentials.Tests.Issues;

public sealed class IssueInfoFactoryTests
{
   [Fact]
   public void WithDetails_WhenIssueIsNull_ThrowsArgumentNullException()
   {
      IssueInfo? issue = null;

      Assert.Throws<ArgumentNullException>(() =>
          IssueInfoFactory.WithDetails(issue!, "Detailed information."));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   [InlineData("Detailed information.")]
   [InlineData(" details ")]
   public void WithDetails_CreatesCopyWithSpecifiedDetails(
       string? details)
   {
      var metadata = new MetadataBag();
      metadata.Set("source", "unit-test");

      var issue = new IssueInfo
      {
         Code = "AFW001",
         Number = 42,
         Message = "Test issue.",
         Details = "Original details.",
         Severity = IssueSeverity.Warning,
         Metadata = metadata
      };

      var copy = IssueInfoFactory.WithDetails(issue, details);

      Assert.NotSame(issue, copy);

      Assert.Equal(issue.Code, copy.Code);
      Assert.Equal(issue.Number, copy.Number);
      Assert.Equal(issue.Message, copy.Message);
      Assert.Equal(details, copy.Details);
      Assert.Equal(issue.Severity, copy.Severity);
      Assert.Same(issue.Metadata, copy.Metadata);
   }

   [Fact]
   public void WithDetails_DoesNotModifyOriginalIssue()
   {
      var issue = new IssueInfo
      {
         Code = "AFW001",
         Number = 42,
         Message = "Test issue.",
         Details = "Original details.",
         Severity = IssueSeverity.Warning
      };

      var copy = IssueInfoFactory.WithDetails(
          issue,
          "New details.");

      Assert.Equal("Original details.", issue.Details);
      Assert.Equal("New details.", copy.Details);
   }

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
   public void Create_WithNumber_WhenCodeIsInvalid_ThrowsArgumentException(
    string? code)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
          IssueInfoFactory.Create(
              code!,
              1001,
              "Message.",
              IssueSeverity.Warning));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Create_WithNumber_WhenMessageIsInvalid_ThrowsArgumentException(
       string? message)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
          IssueInfoFactory.Create(
              "AFW001",
              1001,
              message!,
              IssueSeverity.Warning));
   }

   [Theory]
   [InlineData(null)]
   [InlineData(0)]
   [InlineData(1)]
   [InlineData(42)]
   [InlineData(-1)]
   public void Create_WithNumber_CreatesIssueWithNumber(
       int? number)
   {
      var issue = IssueInfoFactory.Create(
          "AFW001",
          number,
          "Test issue.",
          IssueSeverity.Warning);

      Assert.Equal("AFW001", issue.Code);
      Assert.Equal(number, issue.Number);
      Assert.Equal("Test issue.", issue.Message);
      Assert.Null(issue.Details);
      Assert.Equal(IssueSeverity.Warning, issue.Severity);
      Assert.NotNull(issue.Metadata);
      Assert.True(issue.Metadata.IsEmpty);
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
   public void Create_WithNumber_UsesSpecifiedSeverity(
       IssueSeverity severity)
   {
      var issue = IssueInfoFactory.Create(
          "AFW001",
          1001,
          "Test issue.",
          severity);

      Assert.Equal(severity, issue.Severity);
   }

   [Fact]
   public void WithNumber_WhenIssueIsNull_ThrowsArgumentNullException()
   {
      IssueInfo? issue = null;

      Assert.Throws<ArgumentNullException>(() =>
          IssueInfoFactory.WithNumber(issue!, 1001));
   }

   [Theory]
   [InlineData(null)]
   [InlineData(0)]
   [InlineData(1)]
   [InlineData(42)]
   [InlineData(-1)]
   public void WithNumber_CreatesCopyWithSpecifiedNumber(
       int? number)
   {
      var metadata = new MetadataBag();
      metadata.Set("source", "unit-test");

      var issue = new IssueInfo
      {
         Code = "AFW001",
         Number = 7,
         Message = "Test issue.",
         Details = "Detailed information.",
         Severity = IssueSeverity.Warning,
         Metadata = metadata
      };

      var copy = IssueInfoFactory.WithNumber(issue, number);

      Assert.NotSame(issue, copy);

      Assert.Equal(issue.Code, copy.Code);
      Assert.Equal(number, copy.Number);
      Assert.Equal(issue.Message, copy.Message);
      Assert.Equal(issue.Details, copy.Details);
      Assert.Equal(issue.Severity, copy.Severity);
      Assert.Same(issue.Metadata, copy.Metadata);
   }

   [Fact]
   public void WithNumber_DoesNotModifyOriginalIssue()
   {
      var issue = new IssueInfo
      {
         Code = "AFW001",
         Number = 7,
         Message = "Test issue.",
         Details = "Detailed information.",
         Severity = IssueSeverity.Warning
      };

      var copy = IssueInfoFactory.WithNumber(issue, 1001);

      Assert.Equal(7, issue.Number);
      Assert.Equal(1001, copy.Number);
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

   [Fact]
   public void WithMessage_WhenIssueIsNull_ThrowsArgumentNullException()
   {
      IssueInfo? issue = null;

      Assert.Throws<ArgumentNullException>(() =>
          IssueInfoFactory.WithMessage(issue!, "New message."));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void WithMessage_WhenMessageIsInvalid_ThrowsArgumentException(
       string? message)
   {
      var issue = IssueInfoFactory.Warning(
          "AFW_WARNING",
          "Warning message.");

      Assert.ThrowsAny<ArgumentException>(() =>
          IssueInfoFactory.WithMessage(issue, message!));
   }

   [Fact]
   public void WithMessage_CreatesCopyWithSpecifiedMessage()
   {
      var metadata = new MetadataBag();
      metadata.Set("source", "unit-test");

      var issue = new IssueInfo
      {
         Code = "AFW001",
         Number = 42,
         Message = "Original message.",
         Details = "Detailed information.",
         Severity = IssueSeverity.Warning,
         Metadata = metadata
      };

      var copy = IssueInfoFactory.WithMessage(
          issue,
          "New message.");

      Assert.NotSame(issue, copy);

      Assert.Equal(issue.Code, copy.Code);
      Assert.Equal(issue.Number, copy.Number);
      Assert.Equal("New message.", copy.Message);
      Assert.Equal(issue.Details, copy.Details);
      Assert.Equal(issue.Severity, copy.Severity);
      Assert.Same(issue.Metadata, copy.Metadata);
   }

   [Fact]
   public void WithMessage_DoesNotModifyOriginalIssue()
   {
      var issue = new IssueInfo
      {
         Code = "AFW001",
         Number = 42,
         Message = "Original message.",
         Details = "Detailed information.",
         Severity = IssueSeverity.Warning
      };

      var copy = IssueInfoFactory.WithMessage(
          issue,
          "New message.");

      Assert.Equal("Original message.", issue.Message);
      Assert.Equal("New message.", copy.Message);
   }

   [Fact]
   public void WithSeverity_WhenIssueIsNull_ThrowsArgumentNullException()
   {
      IssueInfo? issue = null;

      Assert.Throws<ArgumentNullException>(() =>
          IssueInfoFactory.WithSeverity(issue!, IssueSeverity.Error));
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
   public void WithSeverity_CreatesCopyWithSpecifiedSeverity(
       IssueSeverity severity)
   {
      var metadata = new MetadataBag();
      metadata.Set("source", "unit-test");

      var issue = new IssueInfo
      {
         Code = "AFW001",
         Number = 42,
         Message = "Test issue.",
         Details = "Detailed information.",
         Severity = IssueSeverity.Warning,
         Metadata = metadata
      };

      var copy = IssueInfoFactory.WithSeverity(issue, severity);

      Assert.NotSame(issue, copy);

      Assert.Equal(issue.Code, copy.Code);
      Assert.Equal(issue.Number, copy.Number);
      Assert.Equal(issue.Message, copy.Message);
      Assert.Equal(issue.Details, copy.Details);
      Assert.Equal(severity, copy.Severity);
      Assert.Same(issue.Metadata, copy.Metadata);
   }

   [Fact]
   public void WithSeverity_DoesNotModifyOriginalIssue()
   {
      var issue = new IssueInfo
      {
         Code = "AFW001",
         Number = 42,
         Message = "Test issue.",
         Details = "Detailed information.",
         Severity = IssueSeverity.Warning
      };

      var copy = IssueInfoFactory.WithSeverity(
          issue,
          IssueSeverity.Error);

      Assert.Equal(IssueSeverity.Warning, issue.Severity);
      Assert.Equal(IssueSeverity.Error, copy.Severity);
   }

   [Fact]
   public void WithCode_WhenIssueIsNull_ThrowsArgumentNullException()
   {
      IssueInfo? issue = null;

      Assert.Throws<ArgumentNullException>(() =>
          IssueInfoFactory.WithCode(issue!, "AFW_NEW_CODE"));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void WithCode_WhenCodeIsInvalid_ThrowsArgumentException(
       string? code)
   {
      var issue = IssueInfoFactory.Warning(
          "AFW_WARNING",
          "Warning message.");

      Assert.ThrowsAny<ArgumentException>(() =>
          IssueInfoFactory.WithCode(issue, code!));
   }

   [Fact]
   public void WithCode_CreatesCopyWithSpecifiedCode()
   {
      var metadata = new MetadataBag();
      metadata.Set("source", "unit-test");

      var issue = new IssueInfo
      {
         Code = "AFW_OLD_CODE",
         Number = 42,
         Message = "Test issue.",
         Details = "Detailed information.",
         Severity = IssueSeverity.Warning,
         Metadata = metadata
      };

      var copy = IssueInfoFactory.WithCode(
          issue,
          "AFW_NEW_CODE");

      Assert.NotSame(issue, copy);

      Assert.Equal("AFW_NEW_CODE", copy.Code);
      Assert.Equal(issue.Number, copy.Number);
      Assert.Equal(issue.Message, copy.Message);
      Assert.Equal(issue.Details, copy.Details);
      Assert.Equal(issue.Severity, copy.Severity);
      Assert.Same(issue.Metadata, copy.Metadata);
   }

   [Fact]
   public void WithCode_DoesNotModifyOriginalIssue()
   {
      var issue = new IssueInfo
      {
         Code = "AFW_OLD_CODE",
         Number = 42,
         Message = "Test issue.",
         Details = "Detailed information.",
         Severity = IssueSeverity.Warning
      };

      var copy = IssueInfoFactory.WithCode(
          issue,
          "AFW_NEW_CODE");

      Assert.Equal("AFW_OLD_CODE", issue.Code);
      Assert.Equal("AFW_NEW_CODE", copy.Code);
   }
}