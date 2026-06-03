using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Issues;
using Afrowave.Toolbox.Essentials.Metadata;
using Afrowave.Toolbox.Essentials.Results;

namespace Afrowave.Toolbox.Essentials.Tests.Results;

public sealed class ResultTests
{
   [Fact]
   public void Constructor_CreatesUnknownEmptyResult()
   {
      var result = new Result();

      Assert.Equal(ResultStatus.Unknown, result.Status);
      Assert.Equal(string.Empty, result.Message);
      Assert.NotNull(result.Issues);
      Assert.Empty(result.Issues);
      Assert.NotNull(result.Metadata);
      Assert.True(result.Metadata.IsEmpty);
      Assert.False(result.IsSuccess);
      Assert.False(result.IsFailure);
      Assert.False(result.HasWarnings);
   }

   [Theory]
   [InlineData(ResultStatus.Unknown, false)]
   [InlineData(ResultStatus.Success, true)]
   [InlineData(ResultStatus.SuccessWithWarnings, true)]
   [InlineData(ResultStatus.Failed, false)]
   [InlineData(ResultStatus.Invalid, false)]
   [InlineData(ResultStatus.NotFound, false)]
   public void IsSuccess_ReturnsExpectedResult(
      ResultStatus status,
      bool expected)
   {
      var result = new Result
      {
         Status = status
      };

      Assert.Equal(expected, result.IsSuccess);
   }

   [Theory]
   [InlineData(ResultStatus.Unknown, false)]
   [InlineData(ResultStatus.Success, false)]
   [InlineData(ResultStatus.SuccessWithWarnings, false)]
   [InlineData(ResultStatus.Partial, false)]
   [InlineData(ResultStatus.Failed, true)]
   [InlineData(ResultStatus.Invalid, true)]
   [InlineData(ResultStatus.NotSupported, true)]
   [InlineData(ResultStatus.Cancelled, true)]
   [InlineData(ResultStatus.NotFound, false)]
   public void IsFailure_ReturnsExpectedResult(
      ResultStatus status,
      bool expected)
   {
      var result = new Result
      {
         Status = status
      };

      Assert.Equal(expected, result.IsFailure);
   }

   [Fact]
   public void HasWarnings_WhenStatusIsSuccessWithWarnings_ReturnsTrue()
   {
      var result = new Result
      {
         Status = ResultStatus.SuccessWithWarnings
      };

      Assert.True(result.HasWarnings);
   }

   [Fact]
   public void HasWarnings_WhenIssuesContainWarning_ReturnsTrue()
   {
      var result = new Result
      {
         Status = ResultStatus.Success,
         Issues =
         [
            IssueInfoFactory.Warning(
               "AFW_WARNING",
               "Warning message.")
         ]
      };

      Assert.True(result.HasWarnings);
   }

   [Fact]
   public void HasWarnings_WhenIssuesContainOnlyInformation_ReturnsFalse()
   {
      var result = new Result
      {
         Status = ResultStatus.Success,
         Issues =
         [
            IssueInfoFactory.Information(
               "AFW_INFO",
               "Information message.")
         ]
      };

      Assert.False(result.HasWarnings);
   }

   [Fact]
   public void HasWarnings_WhenIssuesPropertyIsNull_ReturnsFalse()
   {
      var result = new Result
      {
         Status = ResultStatus.Success,
         Issues = null!
      };

      Assert.False(result.HasWarnings);
   }

   [Fact]
   public void Ok_CreatesSuccessResult()
   {
      var result = Result.Ok();

      Assert.Equal(ResultStatus.Success, result.Status);
      Assert.Equal(string.Empty, result.Message);
      Assert.Empty(result.Issues);
      Assert.True(result.Metadata.IsEmpty);
      Assert.True(result.IsSuccess);
      Assert.False(result.IsFailure);
      Assert.False(result.HasWarnings);
   }

   [Fact]
   public void Ok_WithMessage_CreatesSuccessResultWithMessage()
   {
      var result = Result.Ok("Operation completed.");

      Assert.Equal(ResultStatus.Success, result.Status);
      Assert.Equal("Operation completed.", result.Message);
      Assert.Empty(result.Issues);
      Assert.True(result.IsSuccess);
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Ok_WithMessage_WhenMessageIsInvalid_ThrowsArgumentException(
      string? message)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
         Result.Ok(message!));
   }

   [Fact]
   public void OkWithWarnings_WhenIssuesIsNull_ThrowsArgumentNullException()
   {
      IReadOnlyList<IssueInfo>? issues = null;

      Assert.Throws<ArgumentNullException>(() =>
         Result.OkWithWarnings(issues!));
   }

   [Fact]
   public void OkWithWarnings_CreatesSuccessWithWarningsResult()
   {
      IReadOnlyList<IssueInfo> issues =
      [
         IssueInfoFactory.Warning(
            "AFW_WARNING",
            "Warning message.")
      ];

      var result = Result.OkWithWarnings(issues);

      Assert.Equal(ResultStatus.SuccessWithWarnings, result.Status);
      Assert.Equal(string.Empty, result.Message);
      Assert.Same(issues, result.Issues);
      Assert.True(result.IsSuccess);
      Assert.False(result.IsFailure);
      Assert.True(result.HasWarnings);
   }

   [Fact]
   public void Fail_WithCodeAndMessage_CreatesFailedResult()
   {
      var result = Result.Fail(
         "AFW_ERROR",
         "Something failed.");

      Assert.Equal(ResultStatus.Failed, result.Status);
      Assert.Equal("Something failed.", result.Message);
      Assert.Single(result.Issues);
      Assert.True(result.IsFailure);
      Assert.False(result.IsSuccess);

      var issue = result.Issues[0];

      Assert.Equal("AFW_ERROR", issue.Code);
      Assert.Equal("Something failed.", issue.Message);
      Assert.Equal(IssueSeverity.Error, issue.Severity);
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Fail_WithCodeAndMessage_WhenCodeIsInvalid_ThrowsArgumentException(
      string? code)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
         Result.Fail(code!, "Something failed."));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Fail_WithCodeAndMessage_WhenMessageIsInvalid_ThrowsArgumentException(
      string? message)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
         Result.Fail("AFW_ERROR", message!));
   }

   [Fact]
   public void Fail_WithIssues_WhenIssuesIsNull_ThrowsArgumentNullException()
   {
      IReadOnlyList<IssueInfo>? issues = null;

      Assert.Throws<ArgumentNullException>(() =>
         Result.Fail(issues!));
   }

   [Fact]
   public void Fail_WithIssues_CreatesFailedResultWithIssues()
   {
      IReadOnlyList<IssueInfo> issues =
      [
         IssueInfoFactory.Error(
            "AFW_ERROR",
            "Something failed.")
      ];

      var result = Result.Fail(issues);

      Assert.Equal(ResultStatus.Failed, result.Status);
      Assert.Equal(string.Empty, result.Message);
      Assert.Same(issues, result.Issues);
      Assert.True(result.IsFailure);
      Assert.False(result.IsSuccess);
   }

   [Fact]
   public void Invalid_CreatesInvalidResult()
   {
      var result = Result.Invalid(
         "AFW_INVALID",
         "Input is invalid.");

      Assert.Equal(ResultStatus.Invalid, result.Status);
      Assert.Equal("Input is invalid.", result.Message);
      Assert.Single(result.Issues);
      Assert.True(result.IsFailure);

      var issue = result.Issues[0];

      Assert.Equal("AFW_INVALID", issue.Code);
      Assert.Equal("Input is invalid.", issue.Message);
      Assert.Equal(IssueSeverity.Error, issue.Severity);
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Invalid_WhenCodeIsInvalid_ThrowsArgumentException(
      string? code)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
         Result.Invalid(code!, "Input is invalid."));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Invalid_WhenMessageIsInvalid_ThrowsArgumentException(
      string? message)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
         Result.Invalid("AFW_INVALID", message!));
   }

   [Fact]
   public void NotFound_CreatesNotFoundResult()
   {
      var result = Result.NotFound(
         "AFW_NOT_FOUND",
         "Item was not found.");

      Assert.Equal(ResultStatus.NotFound, result.Status);
      Assert.Equal("Item was not found.", result.Message);
      Assert.Single(result.Issues);
      Assert.False(result.IsFailure);

      var issue = result.Issues[0];

      Assert.Equal("AFW_NOT_FOUND", issue.Code);
      Assert.Equal("Item was not found.", issue.Message);
      Assert.Equal(IssueSeverity.Error, issue.Severity);
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void NotFound_WhenCodeIsInvalid_ThrowsArgumentException(
      string? code)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
         Result.NotFound(code!, "Item was not found."));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void NotFound_WhenMessageIsInvalid_ThrowsArgumentException(
      string? message)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
         Result.NotFound("AFW_NOT_FOUND", message!));
   }

   [Fact]
   public void Result_CanCarryMetadata()
   {
      var metadata = new MetadataBag();
      metadata.Set("source", "unit-test");

      var result = new Result
      {
         Status = ResultStatus.Success,
         Metadata = metadata
      };

      Assert.Same(metadata, result.Metadata);
      Assert.True(result.Metadata.TryGet("source", out var value));
      Assert.Equal("unit-test", value);
   }

   [Fact]
   public void WithMessage_WhenResultIsNull_ThrowsArgumentNullException()
   {
      Result? result = null;

      Assert.Throws<ArgumentNullException>(() =>
         Result.WithMessage(result!, "New message."));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void WithMessage_WhenMessageIsInvalid_ThrowsArgumentException(
      string? message)
   {
      var result = Result.Ok();

      Assert.ThrowsAny<ArgumentException>(() =>
         Result.WithMessage(result, message!));
   }

   [Fact]
   public void WithMessage_CreatesCopyWithSpecifiedMessage()
   {
      var metadata = new MetadataBag();
      metadata.Set("source", "unit-test");

      IReadOnlyList<IssueInfo> issues =
      [
         IssueInfoFactory.Warning(
            "AFW_WARNING",
            "Warning message.")
      ];

      var result = new Result
      {
         Status = ResultStatus.SuccessWithWarnings,
         Message = "Original message.",
         Issues = issues,
         Metadata = metadata
      };

      var copy = Result.WithMessage(
         result,
         "New message.");

      Assert.NotSame(result, copy);

      Assert.Equal(result.Status, copy.Status);
      Assert.Equal("New message.", copy.Message);
      Assert.Same(result.Issues, copy.Issues);
      AssertMetadataCopied(result.Metadata, copy.Metadata);
   }

   [Fact]
   public void WithMessage_DoesNotModifyOriginalResult()
   {
      var result = new Result
      {
         Status = ResultStatus.Success,
         Message = "Original message."
      };

      var copy = Result.WithMessage(
         result,
         "New message.");

      Assert.Equal("Original message.", result.Message);
      Assert.Equal("New message.", copy.Message);
   }

   [Fact]
   public void WithMessage_WhenResultIssuesAreNull_UsesEmptyIssues()
   {
      var result = new Result
      {
         Issues = null!
      };

      var copy = Result.WithMessage(
         result,
         "New message.");

      Assert.NotNull(copy.Issues);
      Assert.Empty(copy.Issues);
   }

   [Fact]
   public void WithMessage_WhenResultMetadataIsNull_UsesEmptyMetadata()
   {
      var result = new Result
      {
         Metadata = null!
      };

      var copy = Result.WithMessage(
         result,
         "New message.");

      Assert.NotNull(copy.Metadata);
      Assert.True(copy.Metadata.IsEmpty);
   }

   [Fact]
   public void WithStatus_WhenResultIsNull_ThrowsArgumentNullException()
   {
      Result? result = null;

      Assert.Throws<ArgumentNullException>(() =>
         Result.WithStatus(result!, ResultStatus.Failed));
   }

   [Theory]
   [InlineData(ResultStatus.Unknown)]
   [InlineData(ResultStatus.Success)]
   [InlineData(ResultStatus.SuccessWithWarnings)]
   [InlineData(ResultStatus.Failed)]
   [InlineData(ResultStatus.Invalid)]
   [InlineData(ResultStatus.NotFound)]
   public void WithStatus_CreatesCopyWithSpecifiedStatus(
      ResultStatus status)
   {
      var metadata = new MetadataBag();
      metadata.Set("source", "unit-test");

      IReadOnlyList<IssueInfo> issues =
      [
         IssueInfoFactory.Warning(
            "AFW_WARNING",
            "Warning message.")
      ];

      var result = new Result
      {
         Status = ResultStatus.Success,
         Message = "Original message.",
         Issues = issues,
         Metadata = metadata
      };

      var copy = Result.WithStatus(result, status);

      Assert.NotSame(result, copy);

      Assert.Equal(status, copy.Status);
      Assert.Equal(result.Message, copy.Message);
      Assert.Same(result.Issues, copy.Issues);
      AssertMetadataCopied(result.Metadata, copy.Metadata);
   }

   [Fact]
   public void WithStatus_DoesNotModifyOriginalResult()
   {
      var result = new Result
      {
         Status = ResultStatus.Success,
         Message = "Original message."
      };

      var copy = Result.WithStatus(
         result,
         ResultStatus.Failed);

      Assert.Equal(ResultStatus.Success, result.Status);
      Assert.Equal(ResultStatus.Failed, copy.Status);
   }

   [Fact]
   public void WithIssues_WhenResultIsNull_ThrowsArgumentNullException()
   {
      Result? result = null;

      IReadOnlyList<IssueInfo> issues = [];

      Assert.Throws<ArgumentNullException>(() =>
         Result.WithIssues(result!, issues));
   }

   [Fact]
   public void WithIssues_WhenIssuesIsNull_ThrowsArgumentNullException()
   {
      var result = Result.Ok();

      IReadOnlyList<IssueInfo>? issues = null;

      Assert.Throws<ArgumentNullException>(() =>
         Result.WithIssues(result, issues!));
   }

   [Fact]
   public void WithIssues_CreatesCopyWithSpecifiedIssues()
   {
      var metadata = new MetadataBag();
      metadata.Set("source", "unit-test");

      var originalIssues = IssueInfoListFactory.Information(
         "AFW_INFO",
         "Information message.");

      var newIssues = IssueInfoListFactory.Error(
         "AFW_ERROR",
         "Error message.");

      var result = new Result
      {
         Status = ResultStatus.Success,
         Message = "Original message.",
         Issues = originalIssues,
         Metadata = metadata
      };

      var copy = Result.WithIssues(result, newIssues);

      Assert.NotSame(result, copy);

      Assert.Equal(result.Status, copy.Status);
      Assert.Equal(result.Message, copy.Message);
      Assert.Same(newIssues, copy.Issues);
      AssertMetadataCopied(result.Metadata, copy.Metadata);
   }

   [Fact]
   public void WithIssues_DoesNotModifyOriginalResult()
   {
      var originalIssues = IssueInfoListFactory.Information(
         "AFW_INFO",
         "Information message.");

      var newIssues = IssueInfoListFactory.Error(
         "AFW_ERROR",
         "Error message.");

      var result = new Result
      {
         Status = ResultStatus.Success,
         Issues = originalIssues
      };

      var copy = Result.WithIssues(result, newIssues);

      Assert.Same(originalIssues, result.Issues);
      Assert.Same(newIssues, copy.Issues);
   }

   [Fact]
   public void WithMetadata_WhenResultIsNull_ThrowsArgumentNullException()
   {
      Result? result = null;
      var metadata = new MetadataBag();

      Assert.Throws<ArgumentNullException>(() =>
         Result.WithMetadata(result!, metadata));
   }

   [Fact]
   public void WithMetadata_WhenMetadataIsNull_ThrowsArgumentNullException()
   {
      var result = Result.Ok();

      MetadataBag? metadata = null;

      Assert.Throws<ArgumentNullException>(() =>
         Result.WithMetadata(result, metadata!));
   }

   [Fact]
   public void WithMetadata_CreatesCopyWithSpecifiedMetadata()
   {
      var originalMetadata = new MetadataBag();
      originalMetadata.Set("original", "yes");

      var newMetadata = new MetadataBag();
      newMetadata.Set("new", "yes");

      IReadOnlyList<IssueInfo> issues =
      [
         IssueInfoFactory.Warning(
            "AFW_WARNING",
            "Warning message.")
      ];

      var result = new Result
      {
         Status = ResultStatus.SuccessWithWarnings,
         Message = "Original message.",
         Issues = issues,
         Metadata = originalMetadata
      };

      var copy = Result.WithMetadata(result, newMetadata);

      Assert.NotSame(result, copy);

      Assert.Equal(result.Status, copy.Status);
      Assert.Equal(result.Message, copy.Message);
      Assert.Same(result.Issues, copy.Issues);
      AssertMetadataCopied(newMetadata, copy.Metadata);

      Assert.True(copy.Metadata.TryGet("new", out var value));
      Assert.Equal("yes", value);
   }

   [Fact]
   public void WithMetadata_DoesNotModifyOriginalResult()
   {
      var originalMetadata = new MetadataBag();
      originalMetadata.Set("original", "yes");

      var newMetadata = new MetadataBag();
      newMetadata.Set("new", "yes");

      var result = new Result
      {
         Status = ResultStatus.Success,
         Metadata = originalMetadata
      };

      var copy = Result.WithMetadata(result, newMetadata);

      Assert.Same(originalMetadata, result.Metadata);
      AssertMetadataCopied(newMetadata, copy.Metadata);

      Assert.True(result.Metadata.TryGet("original", out var originalValue));
      Assert.False(result.Metadata.TryGet("new", out _));
      Assert.Equal("yes", originalValue);

      Assert.True(copy.Metadata.TryGet("new", out var newValue));
      Assert.False(copy.Metadata.TryGet("original", out _));
      Assert.Equal("yes", newValue);
   }

   [Fact]
   public void AddIssue_WhenResultIsNull_ThrowsArgumentNullException()
   {
      Result? result = null;

      var issue = IssueInfoFactory.Warning(
         "AFW_WARNING",
         "Warning message.");

      Assert.Throws<ArgumentNullException>(() =>
         Result.AddIssue(result!, issue));
   }

   [Fact]
   public void AddIssue_WhenIssueIsNull_ThrowsArgumentNullException()
   {
      var result = Result.Ok();

      IssueInfo? issue = null;

      Assert.Throws<ArgumentNullException>(() =>
         Result.AddIssue(result, issue!));
   }

   [Fact]
   public void AddIssue_AppendsIssue()
   {
      var first = IssueInfoFactory.Information(
         "AFW_INFO",
         "Information message.");

      var second = IssueInfoFactory.Warning(
         "AFW_WARNING",
         "Warning message.");

      var result = new Result
      {
         Status = ResultStatus.Success,
         Message = "Original message.",
         Issues =
         [
            first
         ]
      };

      var copy = Result.AddIssue(result, second);

      Assert.NotSame(result, copy);

      Assert.Equal(result.Status, copy.Status);
      Assert.Equal(result.Message, copy.Message);
      Assert.Equal(2, copy.Issues.Count);
      Assert.Same(first, copy.Issues[0]);
      Assert.Same(second, copy.Issues[1]);
      AssertMetadataCopied(result.Metadata, copy.Metadata);
   }

   [Fact]
   public void AddIssue_DoesNotModifyOriginalResult()
   {
      var first = IssueInfoFactory.Information(
         "AFW_INFO",
         "Information message.");

      var second = IssueInfoFactory.Warning(
         "AFW_WARNING",
         "Warning message.");

      var result = new Result
      {
         Status = ResultStatus.Success,
         Issues =
         [
            first
         ]
      };

      var copy = Result.AddIssue(result, second);

      Assert.Single(result.Issues);
      Assert.Same(first, result.Issues[0]);

      Assert.Equal(2, copy.Issues.Count);
      Assert.Same(second, copy.Issues[1]);
   }

   [Fact]
   public void AddIssue_WhenOriginalIssuesAreEmpty_CreatesResultWithOneIssue()
   {
      var issue = IssueInfoFactory.Warning(
         "AFW_WARNING",
         "Warning message.");

      var result = Result.Ok();

      var copy = Result.AddIssue(result, issue);

      Assert.Single(copy.Issues);
      Assert.Same(issue, copy.Issues[0]);
   }

   [Fact]
   public void AddIssue_WhenResultIssuesAreNull_CreatesResultWithOneIssue()
   {
      var issue = IssueInfoFactory.Warning(
         "AFW_WARNING",
         "Warning message.");

      var result = new Result
      {
         Issues = null!
      };

      var copy = Result.AddIssue(result, issue);

      Assert.Single(copy.Issues);
      Assert.Same(issue, copy.Issues[0]);
   }

   [Fact]
   public void AddIssues_WhenResultIsNull_ThrowsArgumentNullException()
   {
      Result? result = null;

      IReadOnlyList<IssueInfo> issues =
      [
         IssueInfoFactory.Warning(
            "AFW_WARNING",
            "Warning message.")
      ];

      Assert.Throws<ArgumentNullException>(() =>
         Result.AddIssues(result!, issues));
   }

   [Fact]
   public void AddIssues_WhenIssuesIsNull_ThrowsArgumentNullException()
   {
      var result = Result.Ok();

      IEnumerable<IssueInfo>? issues = null;

      Assert.Throws<ArgumentNullException>(() =>
         Result.AddIssues(result, issues!));
   }

   [Fact]
   public void AddIssues_AppendsIssues()
   {
      var first = IssueInfoFactory.Information(
         "AFW_INFO",
         "Information message.");

      var second = IssueInfoFactory.Warning(
         "AFW_WARNING",
         "Warning message.");

      var third = IssueInfoFactory.Error(
         "AFW_ERROR",
         "Error message.");

      var result = new Result
      {
         Status = ResultStatus.Success,
         Message = "Original message.",
         Issues =
         [
            first
         ]
      };

      IReadOnlyList<IssueInfo> additionalIssues =
      [
         second,
         third
      ];

      var copy = Result.AddIssues(result, additionalIssues);

      Assert.NotSame(result, copy);

      Assert.Equal(result.Status, copy.Status);
      Assert.Equal(result.Message, copy.Message);
      Assert.Equal(3, copy.Issues.Count);
      Assert.Same(first, copy.Issues[0]);
      Assert.Same(second, copy.Issues[1]);
      Assert.Same(third, copy.Issues[2]);
      AssertMetadataCopied(result.Metadata, copy.Metadata);
   }

   [Fact]
   public void AddIssues_DoesNotModifyOriginalResult()
   {
      var first = IssueInfoFactory.Information(
         "AFW_INFO",
         "Information message.");

      var second = IssueInfoFactory.Warning(
         "AFW_WARNING",
         "Warning message.");

      var result = new Result
      {
         Status = ResultStatus.Success,
         Issues =
         [
            first
         ]
      };

      IReadOnlyList<IssueInfo> additionalIssues =
      [
         second
      ];

      var copy = Result.AddIssues(result, additionalIssues);

      Assert.Single(result.Issues);
      Assert.Same(first, result.Issues[0]);

      Assert.Equal(2, copy.Issues.Count);
      Assert.Same(second, copy.Issues[1]);
   }

   [Fact]
   public void AddIssues_WhenAdditionalIssuesAreEmpty_ReturnsCopyWithOriginalIssues()
   {
      var first = IssueInfoFactory.Information(
         "AFW_INFO",
         "Information message.");

      var result = new Result
      {
         Status = ResultStatus.Success,
         Issues =
         [
            first
         ]
      };

      IReadOnlyList<IssueInfo> additionalIssues = [];

      var copy = Result.AddIssues(result, additionalIssues);

      Assert.NotSame(result, copy);
      Assert.Single(copy.Issues);
      Assert.Same(first, copy.Issues[0]);
   }

   [Fact]
   public void AddIssues_ReturnsSnapshotOfAdditionalIssues()
   {
      var first = IssueInfoFactory.Information(
         "AFW_INFO",
         "Information message.");

      var second = IssueInfoFactory.Warning(
         "AFW_WARNING",
         "Warning message.");

      var result = new Result
      {
         Status = ResultStatus.Success,
         Issues =
         [
            first
         ]
      };

      var additionalIssues = new List<IssueInfo>
      {
         second
      };

      var copy = Result.AddIssues(result, additionalIssues);

      additionalIssues.Clear();

      Assert.Equal(2, copy.Issues.Count);
      Assert.Same(first, copy.Issues[0]);
      Assert.Same(second, copy.Issues[1]);
   }

   [Fact]
   public void AddMetadata_WhenResultIsNull_ThrowsArgumentNullException()
   {
      Result? result = null;

      Assert.Throws<ArgumentNullException>(() =>
         Result.AddMetadata(
            result!,
            "source",
            "unit-test"));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void AddMetadata_WhenKeyIsInvalid_ThrowsArgumentException(
      string? key)
   {
      var result = Result.Ok();

      Assert.ThrowsAny<ArgumentException>(() =>
         Result.AddMetadata(
            result,
            key!,
            "unit-test"));
   }

   [Fact]
   public void AddMetadata_WhenMetadataIsEmpty_AddsMetadataValue()
   {
      var result = Result.Ok();

      var copy = Result.AddMetadata(
         result,
         "source",
         "unit-test");

      Assert.NotSame(result, copy);

      Assert.True(result.Metadata.IsEmpty);

      Assert.Equal(1, copy.Metadata.Count);
      Assert.True(copy.Metadata.TryGet("source", out var value));
      Assert.Equal("unit-test", value);
   }

   [Fact]
   public void AddMetadata_CopiesExistingMetadataAndAddsValue()
   {
      var metadata = new MetadataBag();
      metadata.Set("first", "one");

      var result = new Result
      {
         Status = ResultStatus.Success,
         Message = "Original message.",
         Metadata = metadata
      };

      var copy = Result.AddMetadata(
         result,
         "second",
         "two");

      Assert.NotSame(result, copy);
      Assert.NotSame(result.Metadata, copy.Metadata);

      Assert.Equal(1, result.Metadata.Count);
      Assert.Equal(2, copy.Metadata.Count);

      Assert.True(copy.Metadata.TryGet("first", out var first));
      Assert.True(copy.Metadata.TryGet("second", out var second));

      Assert.Equal("one", first);
      Assert.Equal("two", second);
   }

   [Fact]
   public void AddMetadata_WhenKeyAlreadyExists_OverridesValueInCopyOnly()
   {
      var metadata = new MetadataBag();
      metadata.Set("source", "original");

      var result = new Result
      {
         Status = ResultStatus.Success,
         Metadata = metadata
      };

      var copy = Result.AddMetadata(
         result,
         "source",
         "changed");

      Assert.NotSame(result, copy);
      Assert.NotSame(result.Metadata, copy.Metadata);

      Assert.True(result.Metadata.TryGet("source", out var originalValue));
      Assert.True(copy.Metadata.TryGet("source", out var copiedValue));

      Assert.Equal("original", originalValue);
      Assert.Equal("changed", copiedValue);
   }

   [Fact]
   public void AddMetadata_PreservesOtherResultValues()
   {
      IReadOnlyList<IssueInfo> issues =
      [
         IssueInfoFactory.Warning(
            "AFW_WARNING",
            "Warning message.")
      ];

      var result = new Result
      {
         Status = ResultStatus.SuccessWithWarnings,
         Message = "Original message.",
         Issues = issues
      };

      var copy = Result.AddMetadata(
         result,
         "source",
         "unit-test");

      Assert.Equal(result.Status, copy.Status);
      Assert.Equal(result.Message, copy.Message);
      Assert.Same(result.Issues, copy.Issues);

      Assert.True(copy.Metadata.TryGet("source", out var value));
      Assert.Equal("unit-test", value);
   }

   [Fact]
   public void AddMetadata_DoesNotModifyOriginalResult()
   {
      var metadata = new MetadataBag();
      metadata.Set("original", "yes");

      var result = new Result
      {
         Status = ResultStatus.Success,
         Metadata = metadata
      };

      var copy = Result.AddMetadata(
         result,
         "new",
         "yes");

      Assert.True(result.Metadata.TryGet("original", out var originalValue));
      Assert.False(result.Metadata.TryGet("new", out _));
      Assert.Equal("yes", originalValue);

      Assert.True(copy.Metadata.TryGet("original", out var copiedOriginalValue));
      Assert.True(copy.Metadata.TryGet("new", out var copiedNewValue));

      Assert.Equal("yes", copiedOriginalValue);
      Assert.Equal("yes", copiedNewValue);
   }

   [Fact]
   public void AddMetadata_WhenResultMetadataIsNull_CreatesMetadataBagWithValue()
   {
      var result = new Result
      {
         Metadata = null!
      };

      var copy = Result.AddMetadata(
         result,
         "source",
         "unit-test");

      Assert.NotNull(copy.Metadata);
      Assert.True(copy.Metadata.TryGet("source", out var value));
      Assert.Equal("unit-test", value);
   }

   [Fact]
   public void FromIssues_WhenIssuesIsNull_ThrowsArgumentNullException()
   {
      IReadOnlyList<IssueInfo>? issues = null;

      Assert.Throws<ArgumentNullException>(() =>
         Result.FromIssues(issues!));
   }

   [Fact]
   public void FromIssues_WhenIssuesAreEmpty_CreatesSuccessResult()
   {
      IReadOnlyList<IssueInfo> issues = [];

      var result = Result.FromIssues(issues);

      Assert.Equal(ResultStatus.Success, result.Status);
      Assert.Equal(string.Empty, result.Message);
      Assert.Same(issues, result.Issues);
      Assert.True(result.Metadata.IsEmpty);
      Assert.True(result.IsSuccess);
      Assert.False(result.IsFailure);
      Assert.False(result.HasWarnings);
   }

   [Fact]
   public void FromIssues_WhenIssuesContainOnlyInformation_CreatesSuccessResult()
   {
      IReadOnlyList<IssueInfo> issues =
      [
         IssueInfoFactory.Information(
            "AFW_INFO",
            "Information message.")
      ];

      var result = Result.FromIssues(issues);

      Assert.Equal(ResultStatus.Success, result.Status);
      Assert.Same(issues, result.Issues);
      Assert.True(result.IsSuccess);
      Assert.False(result.IsFailure);
      Assert.False(result.HasWarnings);
   }

   [Fact]
   public void FromIssues_WhenIssuesContainWarning_CreatesSuccessWithWarningsResult()
   {
      IReadOnlyList<IssueInfo> issues =
      [
         IssueInfoFactory.Information(
            "AFW_INFO",
            "Information message."),
         IssueInfoFactory.Warning(
            "AFW_WARNING",
            "Warning message.")
      ];

      var result = Result.FromIssues(issues);

      Assert.Equal(ResultStatus.SuccessWithWarnings, result.Status);
      Assert.Same(issues, result.Issues);
      Assert.True(result.IsSuccess);
      Assert.False(result.IsFailure);
      Assert.True(result.HasWarnings);
   }

   [Fact]
   public void FromIssues_WhenIssuesContainError_CreatesFailedResult()
   {
      IReadOnlyList<IssueInfo> issues =
      [
         IssueInfoFactory.Warning(
            "AFW_WARNING",
            "Warning message."),
         IssueInfoFactory.Error(
            "AFW_ERROR",
            "Error message.")
      ];

      var result = Result.FromIssues(issues);

      Assert.Equal(ResultStatus.Failed, result.Status);
      Assert.Same(issues, result.Issues);
      Assert.False(result.IsSuccess);
      Assert.True(result.IsFailure);
      Assert.True(result.HasWarnings);
   }

   [Fact]
   public void FromIssues_WhenIssuesContainCritical_CreatesFailedResult()
   {
      IReadOnlyList<IssueInfo> issues =
      [
         IssueInfoFactory.Critical(
            "AFW_CRITICAL",
            "Critical message.")
      ];

      var result = Result.FromIssues(issues);

      Assert.Equal(ResultStatus.Failed, result.Status);
      Assert.Same(issues, result.Issues);
      Assert.True(result.IsFailure);
   }

   [Fact]
   public void FromIssues_WhenIssuesContainFatal_CreatesFailedResult()
   {
      IReadOnlyList<IssueInfo> issues =
      [
         IssueInfoFactory.Fatal(
            "AFW_FATAL",
            "Fatal message.")
      ];

      var result = Result.FromIssues(issues);

      Assert.Equal(ResultStatus.Failed, result.Status);
      Assert.Same(issues, result.Issues);
      Assert.True(result.IsFailure);
   }

   [Fact]
   public void FromIssues_WithMessage_WhenIssuesIsNull_ThrowsArgumentNullException()
   {
      IReadOnlyList<IssueInfo>? issues = null;

      Assert.Throws<ArgumentNullException>(() =>
         Result.FromIssues(
            issues!,
            "Validation completed."));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void FromIssues_WithMessage_WhenMessageIsInvalid_ThrowsArgumentException(
      string? message)
   {
      IReadOnlyList<IssueInfo> issues = [];

      Assert.ThrowsAny<ArgumentException>(() =>
         Result.FromIssues(
            issues,
            message!));
   }

   [Fact]
   public void FromIssues_WithMessage_CreatesResultWithMessage()
   {
      IReadOnlyList<IssueInfo> issues =
      [
         IssueInfoFactory.Warning(
            "AFW_WARNING",
            "Warning message.")
      ];

      var result = Result.FromIssues(
         issues,
         "Validation completed with warnings.");

      Assert.Equal(ResultStatus.SuccessWithWarnings, result.Status);
      Assert.Equal("Validation completed with warnings.", result.Message);
      Assert.Same(issues, result.Issues);
      Assert.True(result.IsSuccess);
      Assert.True(result.HasWarnings);
   }

   [Fact]
   public void FromIssues_WithMessage_WhenIssuesContainError_CreatesFailedResultWithMessage()
   {
      IReadOnlyList<IssueInfo> issues =
      [
         IssueInfoFactory.Error(
            "AFW_ERROR",
            "Error message.")
      ];

      var result = Result.FromIssues(
         issues,
         "Validation failed.");

      Assert.Equal(ResultStatus.Failed, result.Status);
      Assert.Equal("Validation failed.", result.Message);
      Assert.Same(issues, result.Issues);
      Assert.True(result.IsFailure);
   }

   [Fact]
   public void FromIssue_WhenIssueIsNull_ThrowsArgumentNullException()
   {
      IssueInfo? issue = null;

      Assert.Throws<ArgumentNullException>(() =>
         Result.FromIssue(issue!));
   }

   [Fact]
   public void FromIssue_WhenIssueIsInformation_CreatesSuccessResult()
   {
      var issue = IssueInfoFactory.Information(
         "AFW_INFO",
         "Information message.");

      var result = Result.FromIssue(issue);

      Assert.Equal(ResultStatus.Success, result.Status);
      Assert.Equal(string.Empty, result.Message);
      Assert.Single(result.Issues);
      Assert.Same(issue, result.Issues[0]);
      Assert.True(result.IsSuccess);
      Assert.False(result.IsFailure);
      Assert.False(result.HasWarnings);
   }

   [Fact]
   public void FromIssue_WhenIssueIsWarning_CreatesSuccessWithWarningsResult()
   {
      var issue = IssueInfoFactory.Warning(
         "AFW_WARNING",
         "Warning message.");

      var result = Result.FromIssue(issue);

      Assert.Equal(ResultStatus.SuccessWithWarnings, result.Status);
      Assert.Equal(string.Empty, result.Message);
      Assert.Single(result.Issues);
      Assert.Same(issue, result.Issues[0]);
      Assert.True(result.IsSuccess);
      Assert.False(result.IsFailure);
      Assert.True(result.HasWarnings);
   }

   [Fact]
   public void FromIssue_WhenIssueIsError_CreatesFailedResult()
   {
      var issue = IssueInfoFactory.Error(
         "AFW_ERROR",
         "Error message.");

      var result = Result.FromIssue(issue);

      Assert.Equal(ResultStatus.Failed, result.Status);
      Assert.Equal(string.Empty, result.Message);
      Assert.Single(result.Issues);
      Assert.Same(issue, result.Issues[0]);
      Assert.False(result.IsSuccess);
      Assert.True(result.IsFailure);
      Assert.True(result.HasWarnings);
   }

   [Fact]
   public void FromIssue_WhenIssueIsCritical_CreatesFailedResult()
   {
      var issue = IssueInfoFactory.Critical(
         "AFW_CRITICAL",
         "Critical message.");

      var result = Result.FromIssue(issue);

      Assert.Equal(ResultStatus.Failed, result.Status);
      Assert.Single(result.Issues);
      Assert.Same(issue, result.Issues[0]);
      Assert.True(result.IsFailure);
   }

   [Fact]
   public void FromIssue_WhenIssueIsFatal_CreatesFailedResult()
   {
      var issue = IssueInfoFactory.Fatal(
         "AFW_FATAL",
         "Fatal message.");

      var result = Result.FromIssue(issue);

      Assert.Equal(ResultStatus.Failed, result.Status);
      Assert.Single(result.Issues);
      Assert.Same(issue, result.Issues[0]);
      Assert.True(result.IsFailure);
   }

   [Fact]
   public void FromIssue_WithMessage_WhenIssueIsNull_ThrowsArgumentNullException()
   {
      IssueInfo? issue = null;

      Assert.Throws<ArgumentNullException>(() =>
         Result.FromIssue(
            issue!,
            "Validation failed."));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void FromIssue_WithMessage_WhenMessageIsInvalid_ThrowsArgumentException(
      string? message)
   {
      var issue = IssueInfoFactory.Warning(
         "AFW_WARNING",
         "Warning message.");

      Assert.ThrowsAny<ArgumentException>(() =>
         Result.FromIssue(
            issue,
            message!));
   }

   [Fact]
   public void FromIssue_WithMessage_CreatesResultWithMessage()
   {
      var issue = IssueInfoFactory.Warning(
         "AFW_WARNING",
         "Warning message.");

      var result = Result.FromIssue(
         issue,
         "Validation completed with warnings.");

      Assert.Equal(ResultStatus.SuccessWithWarnings, result.Status);
      Assert.Equal("Validation completed with warnings.", result.Message);
      Assert.Single(result.Issues);
      Assert.Same(issue, result.Issues[0]);
      Assert.True(result.IsSuccess);
      Assert.True(result.HasWarnings);
   }

   [Fact]
   public void FromIssue_WithMessage_WhenIssueIsError_CreatesFailedResultWithMessage()
   {
      var issue = IssueInfoFactory.Error(
         "AFW_ERROR",
         "Error message.");

      var result = Result.FromIssue(
         issue,
         "Validation failed.");

      Assert.Equal(ResultStatus.Failed, result.Status);
      Assert.Equal("Validation failed.", result.Message);
      Assert.Single(result.Issues);
      Assert.Same(issue, result.Issues[0]);
      Assert.True(result.IsFailure);
   }

   [Fact]
   public void Information_CreatesSuccessResultWithInformationIssue()
   {
      var result = Result.Information(
         "AFW_INFO",
         "Information message.");

      Assert.Equal(ResultStatus.Success, result.Status);
      Assert.Equal(string.Empty, result.Message);
      Assert.Single(result.Issues);
      Assert.True(result.IsSuccess);
      Assert.False(result.IsFailure);
      Assert.False(result.HasWarnings);

      var issue = result.Issues[0];

      Assert.Equal("AFW_INFO", issue.Code);
      Assert.Equal("Information message.", issue.Message);
      Assert.Equal(IssueSeverity.Information, issue.Severity);
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Information_WhenCodeIsInvalid_ThrowsArgumentException(
      string? code)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
         Result.Information(
            code!,
            "Information message."));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Information_WhenMessageIsInvalid_ThrowsArgumentException(
      string? message)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
         Result.Information(
            "AFW_INFO",
            message!));
   }

   [Fact]
   public void Warning_CreatesSuccessWithWarningsResultWithWarningIssue()
   {
      var result = Result.Warning(
         "AFW_WARNING",
         "Warning message.");

      Assert.Equal(ResultStatus.SuccessWithWarnings, result.Status);
      Assert.Equal(string.Empty, result.Message);
      Assert.Single(result.Issues);
      Assert.True(result.IsSuccess);
      Assert.False(result.IsFailure);
      Assert.True(result.HasWarnings);

      var issue = result.Issues[0];

      Assert.Equal("AFW_WARNING", issue.Code);
      Assert.Equal("Warning message.", issue.Message);
      Assert.Equal(IssueSeverity.Warning, issue.Severity);
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Warning_WhenCodeIsInvalid_ThrowsArgumentException(
      string? code)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
         Result.Warning(
            code!,
            "Warning message."));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Warning_WhenMessageIsInvalid_ThrowsArgumentException(
      string? message)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
         Result.Warning(
            "AFW_WARNING",
            message!));
   }

   [Fact]
   public void Partial_CreatesPartialResultWithWarningIssue()
   {
      var result = Result.Partial(
         "AFW_PARTIAL",
         "Operation completed partially.");

      Assert.Equal(ResultStatus.Partial, result.Status);
      Assert.Equal("Operation completed partially.", result.Message);
      Assert.Single(result.Issues);
      Assert.False(result.IsSuccess);
      Assert.False(result.IsFailure);
      Assert.True(result.HasWarnings);

      var issue = result.Issues[0];

      Assert.Equal("AFW_PARTIAL", issue.Code);
      Assert.Equal("Operation completed partially.", issue.Message);
      Assert.Equal(IssueSeverity.Warning, issue.Severity);
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Partial_WhenCodeIsInvalid_ThrowsArgumentException(
      string? code)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
         Result.Partial(
            code!,
            "Operation completed partially."));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Partial_WhenMessageIsInvalid_ThrowsArgumentException(
      string? message)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
         Result.Partial(
            "AFW_PARTIAL",
            message!));
   }

   [Fact]
   public void NotSupported_CreatesNotSupportedResultWithErrorIssue()
   {
      var result = Result.NotSupported(
         "AFW_NOT_SUPPORTED",
         "Operation is not supported.");

      Assert.Equal(ResultStatus.NotSupported, result.Status);
      Assert.Equal("Operation is not supported.", result.Message);
      Assert.Single(result.Issues);
      Assert.False(result.IsSuccess);
      Assert.True(result.IsFailure);
      Assert.True(result.HasWarnings);

      var issue = result.Issues[0];

      Assert.Equal("AFW_NOT_SUPPORTED", issue.Code);
      Assert.Equal("Operation is not supported.", issue.Message);
      Assert.Equal(IssueSeverity.Error, issue.Severity);
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void NotSupported_WhenCodeIsInvalid_ThrowsArgumentException(
      string? code)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
         Result.NotSupported(
            code!,
            "Operation is not supported."));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void NotSupported_WhenMessageIsInvalid_ThrowsArgumentException(
      string? message)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
         Result.NotSupported(
            "AFW_NOT_SUPPORTED",
            message!));
   }

   [Fact]
   public void Cancelled_CreatesCancelledResultWithWarningIssue()
   {
      var result = Result.Cancelled(
         "AFW_CANCELLED",
         "Operation was cancelled.");

      Assert.Equal(ResultStatus.Cancelled, result.Status);
      Assert.Equal("Operation was cancelled.", result.Message);
      Assert.Single(result.Issues);
      Assert.False(result.IsSuccess);
      Assert.True(result.IsFailure);
      Assert.True(result.HasWarnings);

      var issue = result.Issues[0];

      Assert.Equal("AFW_CANCELLED", issue.Code);
      Assert.Equal("Operation was cancelled.", issue.Message);
      Assert.Equal(IssueSeverity.Warning, issue.Severity);
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Cancelled_WhenCodeIsInvalid_ThrowsArgumentException(
      string? code)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
         Result.Cancelled(
            code!,
            "Operation was cancelled."));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Cancelled_WhenMessageIsInvalid_ThrowsArgumentException(
      string? message)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
         Result.Cancelled(
            "AFW_CANCELLED",
            message!));
   }

   private static void AssertMetadataCopied(
      MetadataBag expected,
      MetadataBag actual)
   {
      Assert.NotSame(expected, actual);
      Assert.Equal(expected.Count, actual.Count);

      foreach(var item in expected.Items)
      {
         Assert.True(actual.TryGet(item.Key, out var actualValue));
         Assert.Equal(item.Value, actualValue);
      }
   }
}