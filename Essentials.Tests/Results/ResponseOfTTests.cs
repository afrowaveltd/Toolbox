using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Issues;
using Afrowave.Toolbox.Essentials.Metadata;
using Afrowave.Toolbox.Essentials.Results;

namespace Afrowave.Toolbox.Essentials.Tests.Results;

public sealed class ResponseOfTTests
{
   [Fact]
   public void Constructor_CreatesUnknownEmptyResponse()
   {
      var response = new Response<string>();

      Assert.Equal(ResultStatus.Unknown, response.Status);
      Assert.Equal(string.Empty, response.Message);
      Assert.Null(response.Data);
      Assert.NotNull(response.Issues);
      Assert.Empty(response.Issues);
      Assert.NotNull(response.Metadata);
      Assert.True(response.Metadata.IsEmpty);
      Assert.False(response.HasData);
      Assert.False(response.IsSuccess);
      Assert.False(response.IsFailure);
      Assert.False(response.HasWarnings);
   }

   [Fact]
   public void HasData_WhenDataIsNull_ReturnsFalse()
   {
      var response = new Response<string>
      {
         Data = null
      };

      Assert.False(response.HasData);
   }

   [Fact]
   public void HasData_WhenDataIsNotNull_ReturnsTrue()
   {
      var response = new Response<string>
      {
         Data = "payload"
      };

      Assert.True(response.HasData);
   }

   [Fact]
   public void HasData_WithValueType_ReturnsTrueWhenValueIsDefault()
   {
      var response = new Response<int>
      {
         Data = 0
      };

      Assert.True(response.HasData);
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
      var response = new Response<string>
      {
         Status = status
      };

      Assert.Equal(expected, response.IsSuccess);
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
      var response = new Response<string>
      {
         Status = status
      };

      Assert.Equal(expected, response.IsFailure);
   }

   [Fact]
   public void HasWarnings_WhenStatusIsSuccessWithWarnings_ReturnsTrue()
   {
      var response = new Response<string>
      {
         Status = ResultStatus.SuccessWithWarnings
      };

      Assert.True(response.HasWarnings);
   }

   [Fact]
   public void HasWarnings_WhenIssuesContainWarning_ReturnsTrue()
   {
      var response = new Response<string>
      {
         Status = ResultStatus.Success,
         Issues =
         [
            IssueInfoFactory.Warning(
               "AFW_WARNING",
               "Warning message.")
         ]
      };

      Assert.True(response.HasWarnings);
   }

   [Fact]
   public void HasWarnings_WhenIssuesContainOnlyInformation_ReturnsFalse()
   {
      var response = new Response<string>
      {
         Status = ResultStatus.Success,
         Issues =
         [
            IssueInfoFactory.Information(
               "AFW_INFO",
               "Information message.")
         ]
      };

      Assert.False(response.HasWarnings);
   }

   [Fact]
   public void HasWarnings_WhenIssuesPropertyIsNull_ReturnsFalse()
   {
      var response = new Response<string>
      {
         Status = ResultStatus.Success,
         Issues = null!
      };

      Assert.False(response.HasWarnings);
   }

   [Fact]
   public void Ok_WithData_CreatesSuccessResponseWithData()
   {
      var response = Response<string>.Ok("payload");

      Assert.Equal(ResultStatus.Success, response.Status);
      Assert.Equal(string.Empty, response.Message);
      Assert.Equal("payload", response.Data);
      Assert.True(response.HasData);
      Assert.Empty(response.Issues);
      Assert.True(response.Metadata.IsEmpty);
      Assert.True(response.IsSuccess);
      Assert.False(response.IsFailure);
      Assert.False(response.HasWarnings);
   }

   [Fact]
   public void Ok_WithNullData_CreatesSuccessResponseWithoutData()
   {
      var response = Response<string>.Ok(null);

      Assert.Equal(ResultStatus.Success, response.Status);
      Assert.Null(response.Data);
      Assert.False(response.HasData);
      Assert.True(response.IsSuccess);
   }

   [Fact]
   public void Ok_WithValueTypeData_CreatesSuccessResponseWithData()
   {
      var response = Response<int>.Ok(42);

      Assert.Equal(ResultStatus.Success, response.Status);
      Assert.Equal(42, response.Data);
      Assert.True(response.HasData);
      Assert.True(response.IsSuccess);
   }

   [Fact]
   public void Ok_WithDataAndMessage_CreatesSuccessResponseWithDataAndMessage()
   {
      var response = Response<string>.Ok(
         "payload",
         "Operation completed.");

      Assert.Equal(ResultStatus.Success, response.Status);
      Assert.Equal("Operation completed.", response.Message);
      Assert.Equal("payload", response.Data);
      Assert.True(response.HasData);
      Assert.True(response.IsSuccess);
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Ok_WithDataAndMessage_WhenMessageIsInvalid_ThrowsArgumentException(
      string? message)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
         Response<string>.Ok("payload", message!));
   }

   [Fact]
   public void OkWithWarnings_WhenIssuesIsNull_ThrowsArgumentNullException()
   {
      IReadOnlyList<IssueInfo>? issues = null;

      Assert.Throws<ArgumentNullException>(() =>
         Response<string>.OkWithWarnings("payload", issues!));
   }

   [Fact]
   public void OkWithWarnings_CreatesSuccessWithWarningsResponse()
   {
      IReadOnlyList<IssueInfo> issues =
      [
         IssueInfoFactory.Warning(
            "AFW_WARNING",
            "Warning message.")
      ];

      var response = Response<string>.OkWithWarnings(
         "payload",
         issues);

      Assert.Equal(ResultStatus.SuccessWithWarnings, response.Status);
      Assert.Equal(string.Empty, response.Message);
      Assert.Equal("payload", response.Data);
      Assert.True(response.HasData);
      Assert.Same(issues, response.Issues);
      Assert.True(response.IsSuccess);
      Assert.False(response.IsFailure);
      Assert.True(response.HasWarnings);
   }

   [Fact]
   public void OkWithWarnings_WithNullData_CreatesSuccessWithWarningsResponseWithoutData()
   {
      IReadOnlyList<IssueInfo> issues =
      [
         IssueInfoFactory.Warning(
            "AFW_WARNING",
            "Warning message.")
      ];

      var response = Response<string>.OkWithWarnings(
         null,
         issues);

      Assert.Equal(ResultStatus.SuccessWithWarnings, response.Status);
      Assert.Null(response.Data);
      Assert.False(response.HasData);
      Assert.Same(issues, response.Issues);
      Assert.True(response.HasWarnings);
   }

   [Fact]
   public void Fail_WithCodeAndMessage_CreatesFailedResponse()
   {
      var response = Response<string>.Fail(
         "AFW_ERROR",
         "Something failed.");

      Assert.Equal(ResultStatus.Failed, response.Status);
      Assert.Equal("Something failed.", response.Message);
      Assert.Null(response.Data);
      Assert.False(response.HasData);
      Assert.Single(response.Issues);
      Assert.True(response.IsFailure);
      Assert.False(response.IsSuccess);

      var issue = response.Issues[0];

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
         Response<string>.Fail(code!, "Something failed."));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Fail_WithCodeAndMessage_WhenMessageIsInvalid_ThrowsArgumentException(
      string? message)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
         Response<string>.Fail("AFW_ERROR", message!));
   }

   [Fact]
   public void Fail_WithIssues_WhenIssuesIsNull_ThrowsArgumentNullException()
   {
      IReadOnlyList<IssueInfo>? issues = null;

      Assert.Throws<ArgumentNullException>(() =>
         Response<string>.Fail(issues!));
   }

   [Fact]
   public void Fail_WithIssues_CreatesFailedResponseWithIssues()
   {
      IReadOnlyList<IssueInfo> issues =
      [
         IssueInfoFactory.Error(
            "AFW_ERROR",
            "Something failed.")
      ];

      var response = Response<string>.Fail(issues);

      Assert.Equal(ResultStatus.Failed, response.Status);
      Assert.Equal(string.Empty, response.Message);
      Assert.Null(response.Data);
      Assert.False(response.HasData);
      Assert.Same(issues, response.Issues);
      Assert.True(response.IsFailure);
      Assert.False(response.IsSuccess);
   }

   [Fact]
   public void Invalid_CreatesInvalidResponse()
   {
      var response = Response<string>.Invalid(
         "AFW_INVALID",
         "Input is invalid.");

      Assert.Equal(ResultStatus.Invalid, response.Status);
      Assert.Equal("Input is invalid.", response.Message);
      Assert.Null(response.Data);
      Assert.False(response.HasData);
      Assert.Single(response.Issues);
      Assert.True(response.IsFailure);

      var issue = response.Issues[0];

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
         Response<string>.Invalid(code!, "Input is invalid."));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Invalid_WhenMessageIsInvalid_ThrowsArgumentException(
      string? message)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
         Response<string>.Invalid("AFW_INVALID", message!));
   }

   [Fact]
   public void NotFound_CreatesNotFoundResponse()
   {
      var response = Response<string>.NotFound(
         "AFW_NOT_FOUND",
         "Item was not found.");

      Assert.Equal(ResultStatus.NotFound, response.Status);
      Assert.Equal("Item was not found.", response.Message);
      Assert.Null(response.Data);
      Assert.False(response.HasData);
      Assert.Single(response.Issues);
      Assert.False(response.IsFailure);

      var issue = response.Issues[0];

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
         Response<string>.NotFound(code!, "Item was not found."));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void NotFound_WhenMessageIsInvalid_ThrowsArgumentException(
      string? message)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
         Response<string>.NotFound("AFW_NOT_FOUND", message!));
   }

   [Fact]
   public void Response_CanCarryMetadata()
   {
      var metadata = new MetadataBag();
      metadata.Set("source", "unit-test");

      var response = new Response<string>
      {
         Status = ResultStatus.Success,
         Data = "payload",
         Metadata = metadata
      };

      Assert.Same(metadata, response.Metadata);
      Assert.True(response.Metadata.TryGet("source", out var value));
      Assert.Equal("unit-test", value);
   }

   [Fact]
   public void WithMessage_WhenResponseIsNull_ThrowsArgumentNullException()
   {
      Response<string>? response = null;

      Assert.Throws<ArgumentNullException>(() =>
         Response<string>.WithMessage(response!, "New message."));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void WithMessage_WhenMessageIsInvalid_ThrowsArgumentException(
      string? message)
   {
      var response = Response<string>.Ok("payload");

      Assert.ThrowsAny<ArgumentException>(() =>
         Response<string>.WithMessage(response, message!));
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

      var response = new Response<string>
      {
         Status = ResultStatus.SuccessWithWarnings,
         Message = "Original message.",
         Data = "payload",
         Issues = issues,
         Metadata = metadata
      };

      var copy = Response<string>.WithMessage(
         response,
         "New message.");

      Assert.NotSame(response, copy);

      Assert.Equal(response.Status, copy.Status);
      Assert.Equal("New message.", copy.Message);
      Assert.Equal(response.Data, copy.Data);
      Assert.Same(response.Issues, copy.Issues);
      AssertMetadataCopied(response.Metadata, copy.Metadata);
   }

   [Fact]
   public void WithMessage_DoesNotModifyOriginalResponse()
   {
      var response = new Response<string>
      {
         Status = ResultStatus.Success,
         Message = "Original message.",
         Data = "payload"
      };

      var copy = Response<string>.WithMessage(
         response,
         "New message.");

      Assert.Equal("Original message.", response.Message);
      Assert.Equal("New message.", copy.Message);
   }

   [Fact]
   public void WithMessage_WhenResponseIssuesAreNull_UsesEmptyIssues()
   {
      var response = new Response<string>
      {
         Issues = null!
      };

      var copy = Response<string>.WithMessage(
         response,
         "New message.");

      Assert.NotNull(copy.Issues);
      Assert.Empty(copy.Issues);
   }

   [Fact]
   public void WithMessage_WhenResponseMetadataIsNull_UsesEmptyMetadata()
   {
      var response = new Response<string>
      {
         Metadata = null!
      };

      var copy = Response<string>.WithMessage(
         response,
         "New message.");

      Assert.NotNull(copy.Metadata);
      Assert.True(copy.Metadata.IsEmpty);
   }

   [Fact]
   public void WithStatus_WhenResponseIsNull_ThrowsArgumentNullException()
   {
      Response<string>? response = null;

      Assert.Throws<ArgumentNullException>(() =>
         Response<string>.WithStatus(response!, ResultStatus.Failed));
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

      var response = new Response<string>
      {
         Status = ResultStatus.Success,
         Message = "Original message.",
         Data = "payload",
         Issues = issues,
         Metadata = metadata
      };

      var copy = Response<string>.WithStatus(response, status);

      Assert.NotSame(response, copy);

      Assert.Equal(status, copy.Status);
      Assert.Equal(response.Message, copy.Message);
      Assert.Equal(response.Data, copy.Data);
      Assert.Same(response.Issues, copy.Issues);
      AssertMetadataCopied(response.Metadata, copy.Metadata);
   }

   [Fact]
   public void WithStatus_DoesNotModifyOriginalResponse()
   {
      var response = new Response<string>
      {
         Status = ResultStatus.Success,
         Message = "Original message.",
         Data = "payload"
      };

      var copy = Response<string>.WithStatus(
         response,
         ResultStatus.Failed);

      Assert.Equal(ResultStatus.Success, response.Status);
      Assert.Equal(ResultStatus.Failed, copy.Status);
   }

   [Fact]
   public void WithData_WhenResponseIsNull_ThrowsArgumentNullException()
   {
      Response<string>? response = null;

      Assert.Throws<ArgumentNullException>(() =>
         Response<string>.WithData(response!, "new payload"));
   }

   [Fact]
   public void WithData_CreatesCopyWithSpecifiedData()
   {
      var metadata = new MetadataBag();
      metadata.Set("source", "unit-test");

      IReadOnlyList<IssueInfo> issues =
      [
         IssueInfoFactory.Warning(
            "AFW_WARNING",
            "Warning message.")
      ];

      var response = new Response<string>
      {
         Status = ResultStatus.SuccessWithWarnings,
         Message = "Original message.",
         Data = "original payload",
         Issues = issues,
         Metadata = metadata
      };

      var copy = Response<string>.WithData(
         response,
         "new payload");

      Assert.NotSame(response, copy);

      Assert.Equal(response.Status, copy.Status);
      Assert.Equal(response.Message, copy.Message);
      Assert.Equal("new payload", copy.Data);
      Assert.True(copy.HasData);
      Assert.Same(response.Issues, copy.Issues);
      AssertMetadataCopied(response.Metadata, copy.Metadata);
   }

   [Fact]
   public void WithData_AllowsNullData()
   {
      var response = new Response<string>
      {
         Status = ResultStatus.Success,
         Message = "Original message.",
         Data = "payload"
      };

      var copy = Response<string>.WithData(response, null);

      Assert.Null(copy.Data);
      Assert.False(copy.HasData);
   }

   [Fact]
   public void WithData_WithValueTypeDefaultValue_HasDataReturnsTrue()
   {
      var response = new Response<int>
      {
         Status = ResultStatus.Success,
         Data = 42
      };

      var copy = Response<int>.WithData(response, 0);

      Assert.Equal(0, copy.Data);
      Assert.True(copy.HasData);
   }

   [Fact]
   public void WithData_DoesNotModifyOriginalResponse()
   {
      var response = new Response<string>
      {
         Status = ResultStatus.Success,
         Data = "original payload"
      };

      var copy = Response<string>.WithData(
         response,
         "new payload");

      Assert.Equal("original payload", response.Data);
      Assert.Equal("new payload", copy.Data);
   }

   [Fact]
   public void WithIssues_WhenResponseIsNull_ThrowsArgumentNullException()
   {
      Response<string>? response = null;

      IReadOnlyList<IssueInfo> issues = [];

      Assert.Throws<ArgumentNullException>(() =>
         Response<string>.WithIssues(response!, issues));
   }

   [Fact]
   public void WithIssues_WhenIssuesIsNull_ThrowsArgumentNullException()
   {
      var response = Response<string>.Ok("payload");

      IReadOnlyList<IssueInfo>? issues = null;

      Assert.Throws<ArgumentNullException>(() =>
         Response<string>.WithIssues(response, issues!));
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

      var response = new Response<string>
      {
         Status = ResultStatus.Success,
         Message = "Original message.",
         Data = "payload",
         Issues = originalIssues,
         Metadata = metadata
      };

      var copy = Response<string>.WithIssues(response, newIssues);

      Assert.NotSame(response, copy);

      Assert.Equal(response.Status, copy.Status);
      Assert.Equal(response.Message, copy.Message);
      Assert.Equal(response.Data, copy.Data);
      Assert.Same(newIssues, copy.Issues);
      AssertMetadataCopied(response.Metadata, copy.Metadata);
   }

   [Fact]
   public void WithIssues_DoesNotModifyOriginalResponse()
   {
      var originalIssues = IssueInfoListFactory.Information(
         "AFW_INFO",
         "Information message.");

      var newIssues = IssueInfoListFactory.Error(
         "AFW_ERROR",
         "Error message.");

      var response = new Response<string>
      {
         Status = ResultStatus.Success,
         Data = "payload",
         Issues = originalIssues
      };

      var copy = Response<string>.WithIssues(response, newIssues);

      Assert.Same(originalIssues, response.Issues);
      Assert.Same(newIssues, copy.Issues);
   }

   [Fact]
   public void WithMetadata_WhenResponseIsNull_ThrowsArgumentNullException()
   {
      Response<string>? response = null;
      var metadata = new MetadataBag();

      Assert.Throws<ArgumentNullException>(() =>
         Response<string>.WithMetadata(response!, metadata));
   }

   [Fact]
   public void WithMetadata_WhenMetadataIsNull_ThrowsArgumentNullException()
   {
      var response = Response<string>.Ok("payload");

      MetadataBag? metadata = null;

      Assert.Throws<ArgumentNullException>(() =>
         Response<string>.WithMetadata(response, metadata!));
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

      var response = new Response<string>
      {
         Status = ResultStatus.SuccessWithWarnings,
         Message = "Original message.",
         Data = "payload",
         Issues = issues,
         Metadata = originalMetadata
      };

      var copy = Response<string>.WithMetadata(response, newMetadata);

      Assert.NotSame(response, copy);

      Assert.Equal(response.Status, copy.Status);
      Assert.Equal(response.Message, copy.Message);
      Assert.Equal(response.Data, copy.Data);
      Assert.Same(response.Issues, copy.Issues);
      AssertMetadataCopied(newMetadata, copy.Metadata);

      Assert.True(copy.Metadata.TryGet("new", out var value));
      Assert.Equal("yes", value);
   }

   [Fact]
   public void WithMetadata_DoesNotModifyOriginalResponse()
   {
      var originalMetadata = new MetadataBag();
      originalMetadata.Set("original", "yes");

      var newMetadata = new MetadataBag();
      newMetadata.Set("new", "yes");

      var response = new Response<string>
      {
         Status = ResultStatus.Success,
         Data = "payload",
         Metadata = originalMetadata
      };

      var copy = Response<string>.WithMetadata(response, newMetadata);

      Assert.Same(originalMetadata, response.Metadata);
      AssertMetadataCopied(newMetadata, copy.Metadata);

      Assert.True(response.Metadata.TryGet("original", out var originalValue));
      Assert.False(response.Metadata.TryGet("new", out _));
      Assert.Equal("yes", originalValue);

      Assert.True(copy.Metadata.TryGet("new", out var newValue));
      Assert.False(copy.Metadata.TryGet("original", out _));
      Assert.Equal("yes", newValue);
   }

   [Fact]
   public void AddIssue_WhenResponseIsNull_ThrowsArgumentNullException()
   {
      Response<string>? response = null;

      var issue = IssueInfoFactory.Warning(
         "AFW_WARNING",
         "Warning message.");

      Assert.Throws<ArgumentNullException>(() =>
         Response<string>.AddIssue(response!, issue));
   }

   [Fact]
   public void AddIssue_WhenIssueIsNull_ThrowsArgumentNullException()
   {
      var response = Response<string>.Ok("payload");

      IssueInfo? issue = null;

      Assert.Throws<ArgumentNullException>(() =>
         Response<string>.AddIssue(response, issue!));
   }

   [Fact]
   public void AddIssue_AppendsIssueAndPreservesData()
   {
      var first = IssueInfoFactory.Information(
         "AFW_INFO",
         "Information message.");

      var second = IssueInfoFactory.Warning(
         "AFW_WARNING",
         "Warning message.");

      var response = new Response<string>
      {
         Status = ResultStatus.Success,
         Message = "Original message.",
         Data = "payload",
         Issues =
         [
            first
         ]
      };

      var copy = Response<string>.AddIssue(response, second);

      Assert.NotSame(response, copy);

      Assert.Equal(response.Status, copy.Status);
      Assert.Equal(response.Message, copy.Message);
      Assert.Equal("payload", copy.Data);
      Assert.True(copy.HasData);
      Assert.Equal(2, copy.Issues.Count);
      Assert.Same(first, copy.Issues[0]);
      Assert.Same(second, copy.Issues[1]);
      AssertMetadataCopied(response.Metadata, copy.Metadata);
   }

   [Fact]
   public void AddIssue_DoesNotModifyOriginalResponse()
   {
      var first = IssueInfoFactory.Information(
         "AFW_INFO",
         "Information message.");

      var second = IssueInfoFactory.Warning(
         "AFW_WARNING",
         "Warning message.");

      var response = new Response<string>
      {
         Status = ResultStatus.Success,
         Data = "payload",
         Issues =
         [
            first
         ]
      };

      var copy = Response<string>.AddIssue(response, second);

      Assert.Single(response.Issues);
      Assert.Same(first, response.Issues[0]);
      Assert.Equal("payload", response.Data);

      Assert.Equal(2, copy.Issues.Count);
      Assert.Same(second, copy.Issues[1]);
      Assert.Equal("payload", copy.Data);
   }

   [Fact]
   public void AddIssue_WhenOriginalIssuesAreEmpty_CreatesResponseWithOneIssue()
   {
      var issue = IssueInfoFactory.Warning(
         "AFW_WARNING",
         "Warning message.");

      var response = Response<string>.Ok("payload");

      var copy = Response<string>.AddIssue(response, issue);

      Assert.Single(copy.Issues);
      Assert.Same(issue, copy.Issues[0]);
      Assert.Equal("payload", copy.Data);
      Assert.True(copy.HasData);
   }

   [Fact]
   public void AddIssue_WhenResponseIssuesAreNull_CreatesResponseWithOneIssue()
   {
      var issue = IssueInfoFactory.Warning(
         "AFW_WARNING",
         "Warning message.");

      var response = new Response<string>
      {
         Issues = null!
      };

      var copy = Response<string>.AddIssue(response, issue);

      Assert.Single(copy.Issues);
      Assert.Same(issue, copy.Issues[0]);
   }

   [Fact]
   public void AddIssue_WithNullData_PreservesNoData()
   {
      var issue = IssueInfoFactory.Warning(
         "AFW_WARNING",
         "Warning message.");

      var response = Response<string>.Ok(null);

      var copy = Response<string>.AddIssue(response, issue);

      Assert.Null(copy.Data);
      Assert.False(copy.HasData);
      Assert.Single(copy.Issues);
      Assert.Same(issue, copy.Issues[0]);
   }

   [Fact]
   public void AddIssue_WithValueTypeDefaultData_PreservesData()
   {
      var issue = IssueInfoFactory.Warning(
         "AFW_WARNING",
         "Warning message.");

      var response = Response<int>.Ok(0);

      var copy = Response<int>.AddIssue(response, issue);

      Assert.Equal(0, copy.Data);
      Assert.True(copy.HasData);
      Assert.Single(copy.Issues);
      Assert.Same(issue, copy.Issues[0]);
   }

   [Fact]
   public void AddIssues_WhenResponseIsNull_ThrowsArgumentNullException()
   {
      Response<string>? response = null;

      IReadOnlyList<IssueInfo> issues =
      [
         IssueInfoFactory.Warning(
            "AFW_WARNING",
            "Warning message.")
      ];

      Assert.Throws<ArgumentNullException>(() =>
         Response<string>.AddIssues(response!, issues));
   }

   [Fact]
   public void AddIssues_WhenIssuesIsNull_ThrowsArgumentNullException()
   {
      var response = Response<string>.Ok("payload");

      IEnumerable<IssueInfo>? issues = null;

      Assert.Throws<ArgumentNullException>(() =>
         Response<string>.AddIssues(response, issues!));
   }

   [Fact]
   public void AddIssues_AppendsIssuesAndPreservesData()
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

      var response = new Response<string>
      {
         Status = ResultStatus.Success,
         Message = "Original message.",
         Data = "payload",
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

      var copy = Response<string>.AddIssues(response, additionalIssues);

      Assert.NotSame(response, copy);

      Assert.Equal(response.Status, copy.Status);
      Assert.Equal(response.Message, copy.Message);
      Assert.Equal("payload", copy.Data);
      Assert.True(copy.HasData);
      Assert.Equal(3, copy.Issues.Count);
      Assert.Same(first, copy.Issues[0]);
      Assert.Same(second, copy.Issues[1]);
      Assert.Same(third, copy.Issues[2]);
      AssertMetadataCopied(response.Metadata, copy.Metadata);
   }

   [Fact]
   public void AddIssues_DoesNotModifyOriginalResponse()
   {
      var first = IssueInfoFactory.Information(
         "AFW_INFO",
         "Information message.");

      var second = IssueInfoFactory.Warning(
         "AFW_WARNING",
         "Warning message.");

      var response = new Response<string>
      {
         Status = ResultStatus.Success,
         Data = "payload",
         Issues =
         [
            first
         ]
      };

      IReadOnlyList<IssueInfo> additionalIssues =
      [
         second
      ];

      var copy = Response<string>.AddIssues(response, additionalIssues);

      Assert.Single(response.Issues);
      Assert.Same(first, response.Issues[0]);
      Assert.Equal("payload", response.Data);

      Assert.Equal(2, copy.Issues.Count);
      Assert.Same(second, copy.Issues[1]);
      Assert.Equal("payload", copy.Data);
   }

   [Fact]
   public void AddIssues_WhenAdditionalIssuesAreEmpty_ReturnsCopyWithOriginalIssues()
   {
      var first = IssueInfoFactory.Information(
         "AFW_INFO",
         "Information message.");

      var response = new Response<string>
      {
         Status = ResultStatus.Success,
         Data = "payload",
         Issues =
         [
            first
         ]
      };

      IReadOnlyList<IssueInfo> additionalIssues = [];

      var copy = Response<string>.AddIssues(response, additionalIssues);

      Assert.NotSame(response, copy);
      Assert.Single(copy.Issues);
      Assert.Same(first, copy.Issues[0]);
      Assert.Equal("payload", copy.Data);
      Assert.True(copy.HasData);
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

      var response = new Response<string>
      {
         Status = ResultStatus.Success,
         Data = "payload",
         Issues =
         [
            first
         ]
      };

      var additionalIssues = new List<IssueInfo>
      {
         second
      };

      var copy = Response<string>.AddIssues(response, additionalIssues);

      additionalIssues.Clear();

      Assert.Equal(2, copy.Issues.Count);
      Assert.Same(first, copy.Issues[0]);
      Assert.Same(second, copy.Issues[1]);
      Assert.Equal("payload", copy.Data);
   }

   [Fact]
   public void AddIssues_WithNullData_PreservesNoData()
   {
      var response = Response<string>.Ok(null);

      IReadOnlyList<IssueInfo> additionalIssues =
      [
         IssueInfoFactory.Warning(
            "AFW_WARNING",
            "Warning message.")
      ];

      var copy = Response<string>.AddIssues(response, additionalIssues);

      Assert.Null(copy.Data);
      Assert.False(copy.HasData);
      Assert.Single(copy.Issues);
   }

   [Fact]
   public void AddIssues_WithValueTypeDefaultData_PreservesData()
   {
      var response = Response<int>.Ok(0);

      IReadOnlyList<IssueInfo> additionalIssues =
      [
         IssueInfoFactory.Warning(
            "AFW_WARNING",
            "Warning message.")
      ];

      var copy = Response<int>.AddIssues(response, additionalIssues);

      Assert.Equal(0, copy.Data);
      Assert.True(copy.HasData);
      Assert.Single(copy.Issues);
   }

   [Fact]
   public void AddMetadata_WhenResponseIsNull_ThrowsArgumentNullException()
   {
      Response<string>? response = null;

      Assert.Throws<ArgumentNullException>(() =>
         Response<string>.AddMetadata(
            response!,
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
      var response = Response<string>.Ok("payload");

      Assert.ThrowsAny<ArgumentException>(() =>
         Response<string>.AddMetadata(
            response,
            key!,
            "unit-test"));
   }

   [Fact]
   public void AddMetadata_WhenMetadataIsEmpty_AddsMetadataValue()
   {
      var response = Response<string>.Ok("payload");

      var copy = Response<string>.AddMetadata(
         response,
         "source",
         "unit-test");

      Assert.NotSame(response, copy);

      Assert.True(response.Metadata.IsEmpty);

      Assert.Equal(1, copy.Metadata.Count);
      Assert.True(copy.Metadata.TryGet("source", out var value));
      Assert.Equal("unit-test", value);
   }

   [Fact]
   public void AddMetadata_CopiesExistingMetadataAndAddsValue()
   {
      var metadata = new MetadataBag();
      metadata.Set("first", "one");

      var response = new Response<string>
      {
         Status = ResultStatus.Success,
         Message = "Original message.",
         Data = "payload",
         Metadata = metadata
      };

      var copy = Response<string>.AddMetadata(
         response,
         "second",
         "two");

      Assert.NotSame(response, copy);
      Assert.NotSame(response.Metadata, copy.Metadata);

      Assert.Equal(1, response.Metadata.Count);
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

      var response = new Response<string>
      {
         Status = ResultStatus.Success,
         Data = "payload",
         Metadata = metadata
      };

      var copy = Response<string>.AddMetadata(
         response,
         "source",
         "changed");

      Assert.NotSame(response, copy);
      Assert.NotSame(response.Metadata, copy.Metadata);

      Assert.True(response.Metadata.TryGet("source", out var originalValue));
      Assert.True(copy.Metadata.TryGet("source", out var copiedValue));

      Assert.Equal("original", originalValue);
      Assert.Equal("changed", copiedValue);
   }

   [Fact]
   public void AddMetadata_PreservesOtherResponseValues()
   {
      IReadOnlyList<IssueInfo> issues =
      [
         IssueInfoFactory.Warning(
            "AFW_WARNING",
            "Warning message.")
      ];

      var response = new Response<string>
      {
         Status = ResultStatus.SuccessWithWarnings,
         Message = "Original message.",
         Data = "payload",
         Issues = issues
      };

      var copy = Response<string>.AddMetadata(
         response,
         "source",
         "unit-test");

      Assert.Equal(response.Status, copy.Status);
      Assert.Equal(response.Message, copy.Message);
      Assert.Equal(response.Data, copy.Data);
      Assert.Same(response.Issues, copy.Issues);
      Assert.True(copy.HasData);

      Assert.True(copy.Metadata.TryGet("source", out var value));
      Assert.Equal("unit-test", value);
   }

   [Fact]
   public void AddMetadata_DoesNotModifyOriginalResponse()
   {
      var metadata = new MetadataBag();
      metadata.Set("original", "yes");

      var response = new Response<string>
      {
         Status = ResultStatus.Success,
         Data = "payload",
         Metadata = metadata
      };

      var copy = Response<string>.AddMetadata(
         response,
         "new",
         "yes");

      Assert.True(response.Metadata.TryGet("original", out var originalValue));
      Assert.False(response.Metadata.TryGet("new", out _));
      Assert.Equal("yes", originalValue);

      Assert.True(copy.Metadata.TryGet("original", out var copiedOriginalValue));
      Assert.True(copy.Metadata.TryGet("new", out var copiedNewValue));

      Assert.Equal("yes", copiedOriginalValue);
      Assert.Equal("yes", copiedNewValue);
   }

   [Fact]
   public void AddMetadata_WhenResponseMetadataIsNull_CreatesMetadataBagWithValue()
   {
      var response = new Response<string>
      {
         Metadata = null!
      };

      var copy = Response<string>.AddMetadata(
         response,
         "source",
         "unit-test");

      Assert.NotNull(copy.Metadata);
      Assert.True(copy.Metadata.TryGet("source", out var value));
      Assert.Equal("unit-test", value);
   }

   [Fact]
   public void AddMetadata_PreservesNullData()
   {
      var response = Response<string>.Ok(null);

      var copy = Response<string>.AddMetadata(
         response,
         "source",
         "unit-test");

      Assert.Null(copy.Data);
      Assert.False(copy.HasData);

      Assert.True(copy.Metadata.TryGet("source", out var value));
      Assert.Equal("unit-test", value);
   }

   [Fact]
   public void AddMetadata_PreservesValueTypeDefaultData()
   {
      var response = Response<int>.Ok(0);

      var copy = Response<int>.AddMetadata(
         response,
         "source",
         "unit-test");

      Assert.Equal(0, copy.Data);
      Assert.True(copy.HasData);

      Assert.True(copy.Metadata.TryGet("source", out var value));
      Assert.Equal("unit-test", value);
   }

   [Fact]
   public void FromIssues_WhenIssuesIsNull_ThrowsArgumentNullException()
   {
      IReadOnlyList<IssueInfo>? issues = null;

      Assert.Throws<ArgumentNullException>(() =>
         Response<string>.FromIssues(
            "payload",
            issues!));
   }

   [Fact]
   public void FromIssues_WhenIssuesAreEmpty_CreatesSuccessResponseWithData()
   {
      IReadOnlyList<IssueInfo> issues = [];

      var response = Response<string>.FromIssues(
         "payload",
         issues);

      Assert.Equal(ResultStatus.Success, response.Status);
      Assert.Equal(string.Empty, response.Message);
      Assert.Equal("payload", response.Data);
      Assert.True(response.HasData);
      Assert.Same(issues, response.Issues);
      Assert.True(response.Metadata.IsEmpty);
      Assert.True(response.IsSuccess);
      Assert.False(response.IsFailure);
      Assert.False(response.HasWarnings);
   }

   [Fact]
   public void FromIssues_WithNullData_WhenIssuesAreEmpty_CreatesSuccessResponseWithoutData()
   {
      IReadOnlyList<IssueInfo> issues = [];

      var response = Response<string>.FromIssues(
         null,
         issues);

      Assert.Equal(ResultStatus.Success, response.Status);
      Assert.Null(response.Data);
      Assert.False(response.HasData);
      Assert.Same(issues, response.Issues);
      Assert.True(response.IsSuccess);
   }

   [Fact]
   public void FromIssues_WithValueTypeDefaultData_CreatesSuccessResponseWithData()
   {
      IReadOnlyList<IssueInfo> issues = [];

      var response = Response<int>.FromIssues(
         0,
         issues);

      Assert.Equal(ResultStatus.Success, response.Status);
      Assert.Equal(0, response.Data);
      Assert.True(response.HasData);
      Assert.Same(issues, response.Issues);
   }

   [Fact]
   public void FromIssues_WhenIssuesContainOnlyInformation_CreatesSuccessResponse()
   {
      IReadOnlyList<IssueInfo> issues =
      [
         IssueInfoFactory.Information(
            "AFW_INFO",
            "Information message.")
      ];

      var response = Response<string>.FromIssues(
         "payload",
         issues);

      Assert.Equal(ResultStatus.Success, response.Status);
      Assert.Equal("payload", response.Data);
      Assert.True(response.HasData);
      Assert.Same(issues, response.Issues);
      Assert.True(response.IsSuccess);
      Assert.False(response.IsFailure);
      Assert.False(response.HasWarnings);
   }

   [Fact]
   public void FromIssues_WhenIssuesContainWarning_CreatesSuccessWithWarningsResponse()
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

      var response = Response<string>.FromIssues(
         "payload",
         issues);

      Assert.Equal(ResultStatus.SuccessWithWarnings, response.Status);
      Assert.Equal("payload", response.Data);
      Assert.True(response.HasData);
      Assert.Same(issues, response.Issues);
      Assert.True(response.IsSuccess);
      Assert.False(response.IsFailure);
      Assert.True(response.HasWarnings);
   }

   [Fact]
   public void FromIssues_WhenIssuesContainError_CreatesFailedResponse()
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

      var response = Response<string>.FromIssues(
         "payload",
         issues);

      Assert.Equal(ResultStatus.Failed, response.Status);
      Assert.Equal("payload", response.Data);
      Assert.True(response.HasData);
      Assert.Same(issues, response.Issues);
      Assert.False(response.IsSuccess);
      Assert.True(response.IsFailure);
      Assert.True(response.HasWarnings);
   }

   [Fact]
   public void FromIssues_WhenIssuesContainCritical_CreatesFailedResponse()
   {
      IReadOnlyList<IssueInfo> issues =
      [
         IssueInfoFactory.Critical(
            "AFW_CRITICAL",
            "Critical message.")
      ];

      var response = Response<string>.FromIssues(
         "payload",
         issues);

      Assert.Equal(ResultStatus.Failed, response.Status);
      Assert.Equal("payload", response.Data);
      Assert.True(response.HasData);
      Assert.Same(issues, response.Issues);
      Assert.True(response.IsFailure);
   }

   [Fact]
   public void FromIssues_WhenIssuesContainFatal_CreatesFailedResponse()
   {
      IReadOnlyList<IssueInfo> issues =
      [
         IssueInfoFactory.Fatal(
            "AFW_FATAL",
            "Fatal message.")
      ];

      var response = Response<string>.FromIssues(
         "payload",
         issues);

      Assert.Equal(ResultStatus.Failed, response.Status);
      Assert.Equal("payload", response.Data);
      Assert.True(response.HasData);
      Assert.Same(issues, response.Issues);
      Assert.True(response.IsFailure);
   }

   [Fact]
   public void FromIssues_WithMessage_WhenIssuesIsNull_ThrowsArgumentNullException()
   {
      IReadOnlyList<IssueInfo>? issues = null;

      Assert.Throws<ArgumentNullException>(() =>
         Response<string>.FromIssues(
            "payload",
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
         Response<string>.FromIssues(
            "payload",
            issues,
            message!));
   }

   [Fact]
   public void FromIssues_WithMessage_CreatesResponseWithMessage()
   {
      IReadOnlyList<IssueInfo> issues =
      [
         IssueInfoFactory.Warning(
            "AFW_WARNING",
            "Warning message.")
      ];

      var response = Response<string>.FromIssues(
         "payload",
         issues,
         "Validation completed with warnings.");

      Assert.Equal(ResultStatus.SuccessWithWarnings, response.Status);
      Assert.Equal("Validation completed with warnings.", response.Message);
      Assert.Equal("payload", response.Data);
      Assert.True(response.HasData);
      Assert.Same(issues, response.Issues);
      Assert.True(response.IsSuccess);
      Assert.True(response.HasWarnings);
   }

   [Fact]
   public void FromIssues_WithMessage_WhenIssuesContainError_CreatesFailedResponseWithMessage()
   {
      IReadOnlyList<IssueInfo> issues =
      [
         IssueInfoFactory.Error(
            "AFW_ERROR",
            "Error message.")
      ];

      var response = Response<string>.FromIssues(
         "payload",
         issues,
         "Validation failed.");

      Assert.Equal(ResultStatus.Failed, response.Status);
      Assert.Equal("Validation failed.", response.Message);
      Assert.Equal("payload", response.Data);
      Assert.True(response.HasData);
      Assert.Same(issues, response.Issues);
      Assert.True(response.IsFailure);
   }

   [Fact]
   public void FromIssues_WithMessage_AndNullData_PreservesNoData()
   {
      IReadOnlyList<IssueInfo> issues =
      [
         IssueInfoFactory.Warning(
            "AFW_WARNING",
            "Warning message.")
      ];

      var response = Response<string>.FromIssues(
         null,
         issues,
         "Validation completed with warnings.");

      Assert.Equal(ResultStatus.SuccessWithWarnings, response.Status);
      Assert.Equal("Validation completed with warnings.", response.Message);
      Assert.Null(response.Data);
      Assert.False(response.HasData);
      Assert.Same(issues, response.Issues);
      Assert.True(response.HasWarnings);
   }

   [Fact]
   public void FromIssues_WithMessage_AndValueTypeDefaultData_PreservesData()
   {
      IReadOnlyList<IssueInfo> issues =
      [
         IssueInfoFactory.Information(
            "AFW_INFO",
            "Information message.")
      ];

      var response = Response<int>.FromIssues(
         0,
         issues,
         "Validation completed.");

      Assert.Equal(ResultStatus.Success, response.Status);
      Assert.Equal("Validation completed.", response.Message);
      Assert.Equal(0, response.Data);
      Assert.True(response.HasData);
      Assert.Same(issues, response.Issues);
   }

   [Fact]
   public void FromIssue_WhenIssueIsNull_ThrowsArgumentNullException()
   {
      IssueInfo? issue = null;

      Assert.Throws<ArgumentNullException>(() =>
         Response<string>.FromIssue(
            "payload",
            issue!));
   }

   [Fact]
   public void FromIssue_WhenIssueIsInformation_CreatesSuccessResponseWithData()
   {
      var issue = IssueInfoFactory.Information(
         "AFW_INFO",
         "Information message.");

      var response = Response<string>.FromIssue(
         "payload",
         issue);

      Assert.Equal(ResultStatus.Success, response.Status);
      Assert.Equal(string.Empty, response.Message);
      Assert.Equal("payload", response.Data);
      Assert.True(response.HasData);
      Assert.Single(response.Issues);
      Assert.Same(issue, response.Issues[0]);
      Assert.True(response.IsSuccess);
      Assert.False(response.IsFailure);
      Assert.False(response.HasWarnings);
   }

   [Fact]
   public void FromIssue_WhenIssueIsWarning_CreatesSuccessWithWarningsResponseWithData()
   {
      var issue = IssueInfoFactory.Warning(
         "AFW_WARNING",
         "Warning message.");

      var response = Response<string>.FromIssue(
         "payload",
         issue);

      Assert.Equal(ResultStatus.SuccessWithWarnings, response.Status);
      Assert.Equal(string.Empty, response.Message);
      Assert.Equal("payload", response.Data);
      Assert.True(response.HasData);
      Assert.Single(response.Issues);
      Assert.Same(issue, response.Issues[0]);
      Assert.True(response.IsSuccess);
      Assert.False(response.IsFailure);
      Assert.True(response.HasWarnings);
   }

   [Fact]
   public void FromIssue_WhenIssueIsError_CreatesFailedResponseWithData()
   {
      var issue = IssueInfoFactory.Error(
         "AFW_ERROR",
         "Error message.");

      var response = Response<string>.FromIssue(
         "payload",
         issue);

      Assert.Equal(ResultStatus.Failed, response.Status);
      Assert.Equal(string.Empty, response.Message);
      Assert.Equal("payload", response.Data);
      Assert.True(response.HasData);
      Assert.Single(response.Issues);
      Assert.Same(issue, response.Issues[0]);
      Assert.False(response.IsSuccess);
      Assert.True(response.IsFailure);
      Assert.True(response.HasWarnings);
   }

   [Fact]
   public void FromIssue_WhenIssueIsCritical_CreatesFailedResponseWithData()
   {
      var issue = IssueInfoFactory.Critical(
         "AFW_CRITICAL",
         "Critical message.");

      var response = Response<string>.FromIssue(
         "payload",
         issue);

      Assert.Equal(ResultStatus.Failed, response.Status);
      Assert.Equal("payload", response.Data);
      Assert.True(response.HasData);
      Assert.Single(response.Issues);
      Assert.Same(issue, response.Issues[0]);
      Assert.True(response.IsFailure);
   }

   [Fact]
   public void FromIssue_WhenIssueIsFatal_CreatesFailedResponseWithData()
   {
      var issue = IssueInfoFactory.Fatal(
         "AFW_FATAL",
         "Fatal message.");

      var response = Response<string>.FromIssue(
         "payload",
         issue);

      Assert.Equal(ResultStatus.Failed, response.Status);
      Assert.Equal("payload", response.Data);
      Assert.True(response.HasData);
      Assert.Single(response.Issues);
      Assert.Same(issue, response.Issues[0]);
      Assert.True(response.IsFailure);
   }

   [Fact]
   public void FromIssue_WithNullData_PreservesNoData()
   {
      var issue = IssueInfoFactory.Warning(
         "AFW_WARNING",
         "Warning message.");

      var response = Response<string>.FromIssue(
         null,
         issue);

      Assert.Equal(ResultStatus.SuccessWithWarnings, response.Status);
      Assert.Null(response.Data);
      Assert.False(response.HasData);
      Assert.Single(response.Issues);
      Assert.Same(issue, response.Issues[0]);
      Assert.True(response.HasWarnings);
   }

   [Fact]
   public void FromIssue_WithValueTypeDefaultData_PreservesData()
   {
      var issue = IssueInfoFactory.Information(
         "AFW_INFO",
         "Information message.");

      var response = Response<int>.FromIssue(
         0,
         issue);

      Assert.Equal(ResultStatus.Success, response.Status);
      Assert.Equal(0, response.Data);
      Assert.True(response.HasData);
      Assert.Single(response.Issues);
      Assert.Same(issue, response.Issues[0]);
   }

   [Fact]
   public void FromIssue_WithMessage_WhenIssueIsNull_ThrowsArgumentNullException()
   {
      IssueInfo? issue = null;

      Assert.Throws<ArgumentNullException>(() =>
         Response<string>.FromIssue(
            "payload",
            issue!,
            "Request failed."));
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
         Response<string>.FromIssue(
            "payload",
            issue,
            message!));
   }

   [Fact]
   public void FromIssue_WithMessage_CreatesResponseWithMessage()
   {
      var issue = IssueInfoFactory.Warning(
         "AFW_WARNING",
         "Warning message.");

      var response = Response<string>.FromIssue(
         "payload",
         issue,
         "Request completed with warnings.");

      Assert.Equal(ResultStatus.SuccessWithWarnings, response.Status);
      Assert.Equal("Request completed with warnings.", response.Message);
      Assert.Equal("payload", response.Data);
      Assert.True(response.HasData);
      Assert.Single(response.Issues);
      Assert.Same(issue, response.Issues[0]);
      Assert.True(response.IsSuccess);
      Assert.True(response.HasWarnings);
   }

   [Fact]
   public void FromIssue_WithMessage_WhenIssueIsError_CreatesFailedResponseWithMessage()
   {
      var issue = IssueInfoFactory.Error(
         "AFW_ERROR",
         "Error message.");

      var response = Response<string>.FromIssue(
         "payload",
         issue,
         "Request failed.");

      Assert.Equal(ResultStatus.Failed, response.Status);
      Assert.Equal("Request failed.", response.Message);
      Assert.Equal("payload", response.Data);
      Assert.True(response.HasData);
      Assert.Single(response.Issues);
      Assert.Same(issue, response.Issues[0]);
      Assert.True(response.IsFailure);
   }

   [Fact]
   public void FromIssue_WithMessageAndNullData_PreservesNoData()
   {
      var issue = IssueInfoFactory.Warning(
         "AFW_WARNING",
         "Warning message.");

      var response = Response<string>.FromIssue(
         null,
         issue,
         "Request completed with warnings.");

      Assert.Equal(ResultStatus.SuccessWithWarnings, response.Status);
      Assert.Equal("Request completed with warnings.", response.Message);
      Assert.Null(response.Data);
      Assert.False(response.HasData);
      Assert.Single(response.Issues);
      Assert.Same(issue, response.Issues[0]);
      Assert.True(response.HasWarnings);
   }

   [Fact]
   public void FromIssue_WithMessageAndValueTypeDefaultData_PreservesData()
   {
      var issue = IssueInfoFactory.Information(
         "AFW_INFO",
         "Information message.");

      var response = Response<int>.FromIssue(
         0,
         issue,
         "Request completed.");

      Assert.Equal(ResultStatus.Success, response.Status);
      Assert.Equal("Request completed.", response.Message);
      Assert.Equal(0, response.Data);
      Assert.True(response.HasData);
      Assert.Single(response.Issues);
      Assert.Same(issue, response.Issues[0]);
   }

   [Fact]
   public void Information_CreatesSuccessResponseWithDataAndInformationIssue()
   {
      var response = Response<string>.Information(
         "payload",
         "AFW_INFO",
         "Information message.");

      Assert.Equal(ResultStatus.Success, response.Status);
      Assert.Equal(string.Empty, response.Message);
      Assert.Equal("payload", response.Data);
      Assert.True(response.HasData);
      Assert.Single(response.Issues);
      Assert.True(response.IsSuccess);
      Assert.False(response.IsFailure);
      Assert.False(response.HasWarnings);

      var issue = response.Issues[0];

      Assert.Equal("AFW_INFO", issue.Code);
      Assert.Equal("Information message.", issue.Message);
      Assert.Equal(IssueSeverity.Information, issue.Severity);
   }

   [Fact]
   public void Information_WithNullData_CreatesSuccessResponseWithoutData()
   {
      var response = Response<string>.Information(
         null,
         "AFW_INFO",
         "Information message.");

      Assert.Equal(ResultStatus.Success, response.Status);
      Assert.Null(response.Data);
      Assert.False(response.HasData);
      Assert.Single(response.Issues);

      var issue = response.Issues[0];

      Assert.Equal("AFW_INFO", issue.Code);
      Assert.Equal("Information message.", issue.Message);
      Assert.Equal(IssueSeverity.Information, issue.Severity);
   }

   [Fact]
   public void Information_WithValueTypeDefaultData_PreservesData()
   {
      var response = Response<int>.Information(
         0,
         "AFW_INFO",
         "Information message.");

      Assert.Equal(ResultStatus.Success, response.Status);
      Assert.Equal(0, response.Data);
      Assert.True(response.HasData);
      Assert.Single(response.Issues);

      var issue = response.Issues[0];

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
         Response<string>.Information(
            "payload",
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
         Response<string>.Information(
            "payload",
            "AFW_INFO",
            message!));
   }

   [Fact]
   public void Warning_CreatesSuccessWithWarningsResponseWithDataAndWarningIssue()
   {
      var response = Response<string>.Warning(
         "payload",
         "AFW_WARNING",
         "Warning message.");

      Assert.Equal(ResultStatus.SuccessWithWarnings, response.Status);
      Assert.Equal(string.Empty, response.Message);
      Assert.Equal("payload", response.Data);
      Assert.True(response.HasData);
      Assert.Single(response.Issues);
      Assert.True(response.IsSuccess);
      Assert.False(response.IsFailure);
      Assert.True(response.HasWarnings);

      var issue = response.Issues[0];

      Assert.Equal("AFW_WARNING", issue.Code);
      Assert.Equal("Warning message.", issue.Message);
      Assert.Equal(IssueSeverity.Warning, issue.Severity);
   }

   [Fact]
   public void Warning_WithNullData_CreatesSuccessWithWarningsResponseWithoutData()
   {
      var response = Response<string>.Warning(
         null,
         "AFW_WARNING",
         "Warning message.");

      Assert.Equal(ResultStatus.SuccessWithWarnings, response.Status);
      Assert.Null(response.Data);
      Assert.False(response.HasData);
      Assert.Single(response.Issues);
      Assert.True(response.IsSuccess);
      Assert.True(response.HasWarnings);

      var issue = response.Issues[0];

      Assert.Equal("AFW_WARNING", issue.Code);
      Assert.Equal("Warning message.", issue.Message);
      Assert.Equal(IssueSeverity.Warning, issue.Severity);
   }

   [Fact]
   public void Warning_WithValueTypeDefaultData_PreservesData()
   {
      var response = Response<int>.Warning(
         0,
         "AFW_WARNING",
         "Warning message.");

      Assert.Equal(ResultStatus.SuccessWithWarnings, response.Status);
      Assert.Equal(0, response.Data);
      Assert.True(response.HasData);
      Assert.Single(response.Issues);
      Assert.True(response.HasWarnings);

      var issue = response.Issues[0];

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
         Response<string>.Warning(
            "payload",
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
         Response<string>.Warning(
            "payload",
            "AFW_WARNING",
            message!));
   }

   [Fact]
   public void Partial_CreatesPartialResponseWithDataAndWarningIssue()
   {
      var response = Response<string>.Partial(
         "payload",
         "AFW_PARTIAL",
         "Operation completed partially.");

      Assert.Equal(ResultStatus.Partial, response.Status);
      Assert.Equal("Operation completed partially.", response.Message);
      Assert.Equal("payload", response.Data);
      Assert.True(response.HasData);
      Assert.Single(response.Issues);
      Assert.False(response.IsSuccess);
      Assert.False(response.IsFailure);
      Assert.True(response.HasWarnings);

      var issue = response.Issues[0];

      Assert.Equal("AFW_PARTIAL", issue.Code);
      Assert.Equal("Operation completed partially.", issue.Message);
      Assert.Equal(IssueSeverity.Warning, issue.Severity);
   }

   [Fact]
   public void Partial_WithNullData_CreatesPartialResponseWithoutData()
   {
      var response = Response<string>.Partial(
         null,
         "AFW_PARTIAL",
         "Operation completed partially.");

      Assert.Equal(ResultStatus.Partial, response.Status);
      Assert.Null(response.Data);
      Assert.False(response.HasData);
      Assert.Single(response.Issues);
      Assert.True(response.HasWarnings);
   }

   [Fact]
   public void Partial_WithValueTypeDefaultData_PreservesData()
   {
      var response = Response<int>.Partial(
         0,
         "AFW_PARTIAL",
         "Operation completed partially.");

      Assert.Equal(ResultStatus.Partial, response.Status);
      Assert.Equal(0, response.Data);
      Assert.True(response.HasData);
      Assert.Single(response.Issues);
      Assert.True(response.HasWarnings);
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Partial_WhenCodeIsInvalid_ThrowsArgumentException(
      string? code)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
         Response<string>.Partial(
            "payload",
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
         Response<string>.Partial(
            "payload",
            "AFW_PARTIAL",
            message!));
   }

   [Fact]
   public void NotSupported_CreatesNotSupportedResponseWithDataAndErrorIssue()
   {
      var response = Response<string>.NotSupported(
         "payload",
         "AFW_NOT_SUPPORTED",
         "Operation is not supported.");

      Assert.Equal(ResultStatus.NotSupported, response.Status);
      Assert.Equal("Operation is not supported.", response.Message);
      Assert.Equal("payload", response.Data);
      Assert.True(response.HasData);
      Assert.Single(response.Issues);
      Assert.False(response.IsSuccess);
      Assert.True(response.IsFailure);
      Assert.True(response.HasWarnings);

      var issue = response.Issues[0];

      Assert.Equal("AFW_NOT_SUPPORTED", issue.Code);
      Assert.Equal("Operation is not supported.", issue.Message);
      Assert.Equal(IssueSeverity.Error, issue.Severity);
   }

   [Fact]
   public void NotSupported_WithNullData_CreatesNotSupportedResponseWithoutData()
   {
      var response = Response<string>.NotSupported(
         null,
         "AFW_NOT_SUPPORTED",
         "Operation is not supported.");

      Assert.Equal(ResultStatus.NotSupported, response.Status);
      Assert.Null(response.Data);
      Assert.False(response.HasData);
      Assert.Single(response.Issues);
      Assert.True(response.IsFailure);
      Assert.True(response.HasWarnings);
   }

   [Fact]
   public void NotSupported_WithValueTypeDefaultData_PreservesData()
   {
      var response = Response<int>.NotSupported(
         0,
         "AFW_NOT_SUPPORTED",
         "Operation is not supported.");

      Assert.Equal(ResultStatus.NotSupported, response.Status);
      Assert.Equal(0, response.Data);
      Assert.True(response.HasData);
      Assert.Single(response.Issues);
      Assert.True(response.IsFailure);
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void NotSupported_WhenCodeIsInvalid_ThrowsArgumentException(
      string? code)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
         Response<string>.NotSupported(
            "payload",
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
         Response<string>.NotSupported(
            "payload",
            "AFW_NOT_SUPPORTED",
            message!));
   }

   [Fact]
   public void Cancelled_CreatesCancelledResponseWithDataAndWarningIssue()
   {
      var response = Response<string>.Cancelled(
         "payload",
         "AFW_CANCELLED",
         "Operation was cancelled.");

      Assert.Equal(ResultStatus.Cancelled, response.Status);
      Assert.Equal("Operation was cancelled.", response.Message);
      Assert.Equal("payload", response.Data);
      Assert.True(response.HasData);
      Assert.Single(response.Issues);
      Assert.False(response.IsSuccess);
      Assert.True(response.IsFailure);
      Assert.True(response.HasWarnings);

      var issue = response.Issues[0];

      Assert.Equal("AFW_CANCELLED", issue.Code);
      Assert.Equal("Operation was cancelled.", issue.Message);
      Assert.Equal(IssueSeverity.Warning, issue.Severity);
   }

   [Fact]
   public void Cancelled_WithNullData_CreatesCancelledResponseWithoutData()
   {
      var response = Response<string>.Cancelled(
         null,
         "AFW_CANCELLED",
         "Operation was cancelled.");

      Assert.Equal(ResultStatus.Cancelled, response.Status);
      Assert.Null(response.Data);
      Assert.False(response.HasData);
      Assert.Single(response.Issues);
      Assert.True(response.IsFailure);
      Assert.True(response.HasWarnings);
   }

   [Fact]
   public void Cancelled_WithValueTypeDefaultData_PreservesData()
   {
      var response = Response<int>.Cancelled(
         0,
         "AFW_CANCELLED",
         "Operation was cancelled.");

      Assert.Equal(ResultStatus.Cancelled, response.Status);
      Assert.Equal(0, response.Data);
      Assert.True(response.HasData);
      Assert.Single(response.Issues);
      Assert.True(response.IsFailure);
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Cancelled_WhenCodeIsInvalid_ThrowsArgumentException(
      string? code)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
         Response<string>.Cancelled(
            "payload",
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
         Response<string>.Cancelled(
            "payload",
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