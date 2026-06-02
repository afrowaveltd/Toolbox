using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Extensions;
using Afrowave.Toolbox.Essentials.Interfaces;
using Afrowave.Toolbox.Essentials.Issues;
using Afrowave.Toolbox.Essentials.Metadata;

namespace Afrowave.Toolbox.Essentials.Tests.Extensions;

public sealed class ResultExtensionsTests
{
   [Fact]
   public void HasStatus_WhenResultIsNull_ThrowsArgumentNullException()
   {
      IResult? result = null;

      Assert.Throws<ArgumentNullException>(() =>
          result!.HasStatus(ResultStatus.Success));
   }

   [Theory]
   [InlineData(ResultStatus.Unknown, ResultStatus.Unknown, true)]
   [InlineData(ResultStatus.Success, ResultStatus.Success, true)]
   [InlineData(ResultStatus.SuccessWithWarnings, ResultStatus.SuccessWithWarnings, true)]
   [InlineData(ResultStatus.Failed, ResultStatus.Failed, true)]
   [InlineData(ResultStatus.Invalid, ResultStatus.Invalid, true)]
   [InlineData(ResultStatus.NotFound, ResultStatus.NotFound, true)]
   [InlineData(ResultStatus.Success, ResultStatus.Failed, false)]
   [InlineData(ResultStatus.Failed, ResultStatus.Success, false)]
   public void HasStatus_ReturnsExpectedResult(
       ResultStatus currentStatus,
       ResultStatus expectedStatus,
       bool expected)
   {
      var result = new TestResult
      {
         Status = currentStatus
      };

      var actual = result.HasStatus(expectedStatus);

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void IsUnknown_WhenResultIsNull_ThrowsArgumentNullException()
   {
      IResult? result = null;

      Assert.Throws<ArgumentNullException>(() =>
          result!.IsUnknown());
   }

   [Theory]
   [InlineData(ResultStatus.Unknown, true)]
   [InlineData(ResultStatus.Success, false)]
   [InlineData(ResultStatus.SuccessWithWarnings, false)]
   [InlineData(ResultStatus.Failed, false)]
   [InlineData(ResultStatus.Invalid, false)]
   [InlineData(ResultStatus.NotFound, false)]
   public void IsUnknown_ReturnsExpectedResult(
       ResultStatus status,
       bool expected)
   {
      var result = new TestResult
      {
         Status = status
      };

      var actual = result.IsUnknown();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void IsInvalid_WhenResultIsNull_ThrowsArgumentNullException()
   {
      IResult? result = null;

      Assert.Throws<ArgumentNullException>(() =>
          result!.IsInvalid());
   }

   [Theory]
   [InlineData(ResultStatus.Unknown, false)]
   [InlineData(ResultStatus.Success, false)]
   [InlineData(ResultStatus.SuccessWithWarnings, false)]
   [InlineData(ResultStatus.Failed, false)]
   [InlineData(ResultStatus.Invalid, true)]
   [InlineData(ResultStatus.NotFound, false)]
   public void IsInvalid_ReturnsExpectedResult(
       ResultStatus status,
       bool expected)
   {
      var result = new TestResult
      {
         Status = status
      };

      var actual = result.IsInvalid();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void IsNotFound_WhenResultIsNull_ThrowsArgumentNullException()
   {
      IResult? result = null;

      Assert.Throws<ArgumentNullException>(() =>
          result!.IsNotFound());
   }

   [Theory]
   [InlineData(ResultStatus.Unknown, false)]
   [InlineData(ResultStatus.Success, false)]
   [InlineData(ResultStatus.SuccessWithWarnings, false)]
   [InlineData(ResultStatus.Failed, false)]
   [InlineData(ResultStatus.Invalid, false)]
   [InlineData(ResultStatus.NotFound, true)]
   public void IsNotFound_ReturnsExpectedResult(
       ResultStatus status,
       bool expected)
   {
      var result = new TestResult
      {
         Status = status
      };

      var actual = result.IsNotFound();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasIssues_WhenResultIsNull_ThrowsArgumentNullException()
   {
      IResult? result = null;

      Assert.Throws<ArgumentNullException>(() =>
          result!.HasIssues());
   }

   [Fact]
   public void HasIssues_WhenIssuesAreEmpty_ReturnsFalse()
   {
      var result = new TestResult
      {
         Issues = IssueInfoListFactory.Empty()
      };

      var actual = result.HasIssues();

      Assert.False(actual);
   }

   [Fact]
   public void HasIssues_WhenIssuesContainItem_ReturnsTrue()
   {
      var result = new TestResult
      {
         Issues = IssueInfoListFactory.Warning(
              "AFW_WARNING",
              "Warning message.")
      };

      var actual = result.HasIssues();

      Assert.True(actual);
   }

   [Fact]
   public void HasErrors_WhenResultIsNull_ThrowsArgumentNullException()
   {
      IResult? result = null;

      Assert.Throws<ArgumentNullException>(() =>
          result!.HasErrors());
   }

   [Fact]
   public void HasErrors_WhenIssuesAreEmpty_ReturnsFalse()
   {
      var result = new TestResult
      {
         Issues = IssueInfoListFactory.Empty()
      };

      var actual = result.HasErrors();

      Assert.False(actual);
   }

   [Theory]
   [InlineData(IssueSeverity.None, false)]
   [InlineData(IssueSeverity.Trace, false)]
   [InlineData(IssueSeverity.Debug, false)]
   [InlineData(IssueSeverity.Information, false)]
   [InlineData(IssueSeverity.Warning, false)]
   [InlineData(IssueSeverity.Error, true)]
   [InlineData(IssueSeverity.Critical, true)]
   [InlineData(IssueSeverity.Fatal, true)]
   public void HasErrors_ReturnsExpectedResult(
       IssueSeverity severity,
       bool expected)
   {
      var result = new TestResult
      {
         Issues =
          [
              new IssueInfo
                {
                    Code = $"AFW_{severity}",
                    Message = $"Issue with severity {severity}.",
                    Severity = severity
                }
          ]
      };

      var actual = result.HasErrors();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasMessage_WhenResultIsNull_ThrowsArgumentNullException()
   {
      IResult? result = null;

      Assert.Throws<ArgumentNullException>(() =>
          result!.HasMessage());
   }

   [Theory]
   [InlineData("", false)]
   [InlineData("   ", false)]
   [InlineData("Operation completed.", true)]
   [InlineData(" message ", true)]
   public void HasMessage_ReturnsExpectedResult(
       string message,
       bool expected)
   {
      var result = new TestResult
      {
         Message = message
      };

      var actual = result.HasMessage();

      Assert.Equal(expected, actual);
   }


   [Fact]
   public void HasIssueCode_WhenResultIsNull_ThrowsArgumentNullException()
   {
      IResult? result = null;

      Assert.Throws<ArgumentNullException>(() =>
          result!.HasIssueCode("AFW_WARNING"));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void HasIssueCode_WhenCodeIsInvalid_ThrowsArgumentException(
       string? code)
   {
      var result = new TestResult();

      Assert.ThrowsAny<ArgumentException>(() =>
          result.HasIssueCode(code!));
   }

   [Fact]
   public void HasIssueCode_WhenIssuesAreEmpty_ReturnsFalse()
   {
      var result = new TestResult
      {
         Issues = IssueInfoListFactory.Empty()
      };

      var actual = result.HasIssueCode("AFW_WARNING");

      Assert.False(actual);
   }

   [Fact]
   public void HasIssueCode_WhenCodeDoesNotExist_ReturnsFalse()
   {
      var result = new TestResult
      {
         Issues =
          [
              IssueInfoFactory.Warning(
                "AFW_WARNING",
                "Warning message.")
          ]
      };

      var actual = result.HasIssueCode("AFW_ERROR");

      Assert.False(actual);
   }

   [Fact]
   public void HasIssueCode_WhenCodeExists_ReturnsTrue()
   {
      var result = new TestResult
      {
         Issues =
          [
              IssueInfoFactory.Warning(
                "AFW_WARNING",
                "Warning message.")
          ]
      };

      var actual = result.HasIssueCode("AFW_WARNING");

      Assert.True(actual);
   }

   [Fact]
   public void HasIssueCode_UsesCaseInsensitiveComparison()
   {
      var result = new TestResult
      {
         Issues =
          [
              IssueInfoFactory.Warning(
                "AFW_WARNING",
                "Warning message.")
          ]
      };

      var actual = result.HasIssueCode("afw_warning");

      Assert.True(actual);
   }

   [Fact]
   public void TryGetIssueByCode_WhenResultIsNull_ThrowsArgumentNullException()
   {
      IResult? result = null;

      Assert.Throws<ArgumentNullException>(() =>
          result!.TryGetIssueByCode("AFW_WARNING", out _));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void TryGetIssueByCode_WhenCodeIsInvalid_ThrowsArgumentException(
       string? code)
   {
      var result = new TestResult();

      Assert.ThrowsAny<ArgumentException>(() =>
          result.TryGetIssueByCode(code!, out _));
   }

   [Fact]
   public void TryGetIssueByCode_WhenIssuesAreEmpty_ReturnsFalseAndNullIssue()
   {
      var result = new TestResult
      {
         Issues = IssueInfoListFactory.Empty()
      };

      var actual = result.TryGetIssueByCode(
          "AFW_WARNING",
          out var issue);

      Assert.False(actual);
      Assert.Null(issue);
   }

   [Fact]
   public void TryGetIssueByCode_WhenCodeDoesNotExist_ReturnsFalseAndNullIssue()
   {
      var result = new TestResult
      {
         Issues =
          [
              IssueInfoFactory.Warning(
                "AFW_WARNING",
                "Warning message.")
          ]
      };

      var actual = result.TryGetIssueByCode(
          "AFW_ERROR",
          out var issue);

      Assert.False(actual);
      Assert.Null(issue);
   }

   [Fact]
   public void TryGetIssueByCode_WhenCodeExists_ReturnsTrueAndIssue()
   {
      var expectedIssue = IssueInfoFactory.Warning(
          "AFW_WARNING",
          "Warning message.");

      var result = new TestResult
      {
         Issues =
          [
              expectedIssue
          ]
      };

      var actual = result.TryGetIssueByCode(
          "AFW_WARNING",
          out var issue);

      Assert.True(actual);
      Assert.Same(expectedIssue, issue);
   }

   [Fact]
   public void TryGetIssueByCode_UsesCaseInsensitiveComparison()
   {
      var expectedIssue = IssueInfoFactory.Warning(
          "AFW_WARNING",
          "Warning message.");

      var result = new TestResult
      {
         Issues =
          [
              expectedIssue
          ]
      };

      var actual = result.TryGetIssueByCode(
          "afw_warning",
          out var issue);

      Assert.True(actual);
      Assert.Same(expectedIssue, issue);
   }

   [Fact]
   public void TryGetIssueByCode_WhenMultipleIssuesMatch_ReturnsFirstMatch()
   {
      var first = IssueInfoFactory.Warning(
          "AFW_DUPLICATE",
          "First warning.");

      var second = IssueInfoFactory.Error(
          "AFW_DUPLICATE",
          "Second error.");

      var result = new TestResult
      {
         Issues =
          [
              first,
            second
          ]
      };

      var actual = result.TryGetIssueByCode(
          "AFW_DUPLICATE",
          out var issue);

      Assert.True(actual);
      Assert.Same(first, issue);
   }

   [Fact]
   public void IsCleanSuccess_WhenResultIsNull_ThrowsArgumentNullException()
   {
      IResult? result = null;

      Assert.Throws<ArgumentNullException>(() =>
          result!.IsCleanSuccess());
   }

   [Fact]
   public void IsCleanSuccess_WhenResultIsSuccessAndHasNoIssues_ReturnsTrue()
   {
      var result = new TestResult
      {
         Status = ResultStatus.Success,
         Issues = IssueInfoListFactory.Empty()
      };

      var actual = result.IsCleanSuccess();

      Assert.True(actual);
   }

   [Fact]
   public void IsCleanSuccess_WhenResultIsSuccessWithInformationIssue_ReturnsFalse()
   {
      var result = new TestResult
      {
         Status = ResultStatus.Success,
         Issues =
          [
              IssueInfoFactory.Information(
                "AFW_INFO",
                "Information message.")
          ]
      };

      var actual = result.IsCleanSuccess();

      Assert.False(actual);
   }

   [Fact]
   public void IsCleanSuccess_WhenResultIsSuccessWithWarningsStatus_ReturnsFalse()
   {
      var result = new TestResult
      {
         Status = ResultStatus.SuccessWithWarnings,
         Issues = IssueInfoListFactory.Empty()
      };

      var actual = result.IsCleanSuccess();

      Assert.False(actual);
   }

   [Fact]
   public void IsCleanSuccess_WhenResultIsSuccessWithWarningIssue_ReturnsFalse()
   {
      var result = new TestResult
      {
         Status = ResultStatus.Success,
         Issues =
          [
              IssueInfoFactory.Warning(
                "AFW_WARNING",
                "Warning message.")
          ]
      };

      var actual = result.IsCleanSuccess();

      Assert.False(actual);
   }

   [Theory]
   [InlineData(ResultStatus.Unknown)]
   [InlineData(ResultStatus.Partial)]
   [InlineData(ResultStatus.Failed)]
   [InlineData(ResultStatus.Invalid)]
   [InlineData(ResultStatus.NotSupported)]
   [InlineData(ResultStatus.Cancelled)]
   [InlineData(ResultStatus.NotFound)]
   public void IsCleanSuccess_WhenResultIsNotSuccessful_ReturnsFalse(
       ResultStatus status)
   {
      var result = new TestResult
      {
         Status = status,
         Issues = IssueInfoListFactory.Empty()
      };

      var actual = result.IsCleanSuccess();

      Assert.False(actual);
   }

   [Fact]
   public void IsCleanSuccess_WhenResultIsFailedWithoutIssues_ReturnsFalse()
   {
      var result = new TestResult
      {
         Status = ResultStatus.Failed,
         Issues = IssueInfoListFactory.Empty()
      };

      var actual = result.IsCleanSuccess();

      Assert.False(actual);
   }

   [Fact]
   public void IsCleanSuccess_WhenResultIsSuccessWithErrorIssue_ReturnsFalse()
   {
      var result = new TestResult
      {
         Status = ResultStatus.Success,
         Issues =
          [
              IssueInfoFactory.Error(
                "AFW_ERROR",
                "Error message.")
          ]
      };

      var actual = result.IsCleanSuccess();

      Assert.False(actual);
   }
   [Fact]
   public void IsDirtySuccess_WhenResultIsNull_ThrowsArgumentNullException()
   {
      IResult? result = null;

      Assert.Throws<ArgumentNullException>(() =>
          result!.IsDirtySuccess());
   }

   [Fact]
   public void IsDirtySuccess_WhenResultIsSuccessAndHasNoIssues_ReturnsFalse()
   {
      var result = new TestResult
      {
         Status = ResultStatus.Success,
         Issues = IssueInfoListFactory.Empty()
      };

      var actual = result.IsDirtySuccess();

      Assert.False(actual);
   }

   [Fact]
   public void IsDirtySuccess_WhenResultIsSuccessWithInformationIssue_ReturnsTrue()
   {
      var result = new TestResult
      {
         Status = ResultStatus.Success,
         Issues =
          [
              IssueInfoFactory.Information(
                "AFW_INFO",
                "Information message.")
          ]
      };

      var actual = result.IsDirtySuccess();

      Assert.True(actual);
   }

   [Fact]
   public void IsDirtySuccess_WhenResultIsSuccessWithWarningsStatus_ReturnsTrue()
   {
      var result = new TestResult
      {
         Status = ResultStatus.SuccessWithWarnings,
         Issues = IssueInfoListFactory.Empty()
      };

      var actual = result.IsDirtySuccess();

      Assert.True(actual);
   }

   [Fact]
   public void IsDirtySuccess_WhenResultIsSuccessWithWarningIssue_ReturnsTrue()
   {
      var result = new TestResult
      {
         Status = ResultStatus.Success,
         Issues =
          [
              IssueInfoFactory.Warning(
                "AFW_WARNING",
                "Warning message.")
          ]
      };

      var actual = result.IsDirtySuccess();

      Assert.True(actual);
   }

   [Fact]
   public void IsDirtySuccess_WhenResultIsSuccessWithErrorIssue_ReturnsTrue()
   {
      var result = new TestResult
      {
         Status = ResultStatus.Success,
         Issues =
          [
              IssueInfoFactory.Error(
                "AFW_ERROR",
                "Error message.")
          ]
      };

      var actual = result.IsDirtySuccess();

      Assert.True(actual);
   }

   [Theory]
   [InlineData(ResultStatus.Unknown)]
   [InlineData(ResultStatus.Partial)]
   [InlineData(ResultStatus.Failed)]
   [InlineData(ResultStatus.Invalid)]
   [InlineData(ResultStatus.NotSupported)]
   [InlineData(ResultStatus.Cancelled)]
   [InlineData(ResultStatus.NotFound)]
   public void IsDirtySuccess_WhenResultIsNotSuccessful_ReturnsFalse(
       ResultStatus status)
   {
      var result = new TestResult
      {
         Status = status,
         Issues =
          [
              IssueInfoFactory.Warning(
                "AFW_WARNING",
                "Warning message.")
          ]
      };

      var actual = result.IsDirtySuccess();

      Assert.False(actual);
   }

   [Fact]
   public void IsDirtySuccess_WhenResultIsFailedWithoutIssues_ReturnsFalse()
   {
      var result = new TestResult
      {
         Status = ResultStatus.Failed,
         Issues = IssueInfoListFactory.Empty()
      };

      var actual = result.IsDirtySuccess();

      Assert.False(actual);
   }
   [Fact]
   public void NeedsAttention_WhenResultIsNull_ThrowsArgumentNullException()
   {
      IResult? result = null;

      Assert.Throws<ArgumentNullException>(() =>
          result!.NeedsAttention());
   }

   [Fact]
   public void NeedsAttention_WhenResultIsCleanSuccess_ReturnsFalse()
   {
      var result = new TestResult
      {
         Status = ResultStatus.Success,
         Issues = IssueInfoListFactory.Empty()
      };

      var actual = result.NeedsAttention();

      Assert.False(actual);
   }

   [Fact]
   public void NeedsAttention_WhenResultIsSuccessWithInformationIssue_ReturnsTrue()
   {
      var result = new TestResult
      {
         Status = ResultStatus.Success,
         Issues =
          [
              IssueInfoFactory.Information(
                "AFW_INFO",
                "Information message.")
          ]
      };

      var actual = result.NeedsAttention();

      Assert.True(actual);
   }

   [Fact]
   public void NeedsAttention_WhenResultIsSuccessWithWarningsStatus_ReturnsTrue()
   {
      var result = new TestResult
      {
         Status = ResultStatus.SuccessWithWarnings,
         Issues = IssueInfoListFactory.Empty()
      };

      var actual = result.NeedsAttention();

      Assert.True(actual);
   }

   [Fact]
   public void NeedsAttention_WhenResultIsSuccessWithWarningIssue_ReturnsTrue()
   {
      var result = new TestResult
      {
         Status = ResultStatus.Success,
         Issues =
          [
              IssueInfoFactory.Warning(
                "AFW_WARNING",
                "Warning message.")
          ]
      };

      var actual = result.NeedsAttention();

      Assert.True(actual);
   }

   [Fact]
   public void NeedsAttention_WhenResultIsSuccessWithErrorIssue_ReturnsTrue()
   {
      var result = new TestResult
      {
         Status = ResultStatus.Success,
         Issues =
          [
              IssueInfoFactory.Error(
                "AFW_ERROR",
                "Error message.")
          ]
      };

      var actual = result.NeedsAttention();

      Assert.True(actual);
   }

   [Theory]
   [InlineData(ResultStatus.Failed)]
   [InlineData(ResultStatus.Invalid)]
   [InlineData(ResultStatus.NotSupported)]
   [InlineData(ResultStatus.Cancelled)]
   public void NeedsAttention_WhenResultIsFailure_ReturnsTrue(
   ResultStatus status)
   {
      var result = new TestResult
      {
         Status = status,
         Issues = IssueInfoListFactory.Empty()
      };

      var actual = result.NeedsAttention();

      Assert.True(actual);
   }
   [Fact]
   public void NeedsAttention_WhenResultIsNotFoundWithoutIssues_ReturnsFalse()
   {
      var result = new TestResult
      {
         Status = ResultStatus.NotFound,
         Issues = IssueInfoListFactory.Empty()
      };

      var actual = result.NeedsAttention();

      Assert.False(actual);
   }

   [Fact]
   public void NeedsAttention_WhenResultIsUnknownWithoutIssues_ReturnsFalse()
   {
      var result = new TestResult
      {
         Status = ResultStatus.Unknown,
         Issues = IssueInfoListFactory.Empty()
      };

      var actual = result.NeedsAttention();

      Assert.False(actual);
   }

   [Fact]
   public void NeedsAttention_WhenResultIsUnknownWithIssues_ReturnsFalse()
   {
      var result = new TestResult
      {
         Status = ResultStatus.Unknown,
         Issues =
          [
              IssueInfoFactory.Warning(
                "AFW_WARNING",
                "Warning message.")
          ]
      };

      var actual = result.NeedsAttention();

      Assert.False(actual);
   }
   [Fact]
   public void HasWarningOrHigherIssues_WhenResultIsNull_ThrowsArgumentNullException()
   {
      IResult? result = null;

      Assert.Throws<ArgumentNullException>(() =>
          result!.HasWarningOrHigherIssues());
   }

   [Fact]
   public void HasWarningOrHigherIssues_WhenIssuesAreEmpty_ReturnsFalse()
   {
      var result = new TestResult
      {
         Issues = IssueInfoListFactory.Empty()
      };

      var actual = result.HasWarningOrHigherIssues();

      Assert.False(actual);
   }

   [Theory]
   [InlineData(IssueSeverity.None, false)]
   [InlineData(IssueSeverity.Trace, false)]
   [InlineData(IssueSeverity.Debug, false)]
   [InlineData(IssueSeverity.Information, false)]
   [InlineData(IssueSeverity.Warning, true)]
   [InlineData(IssueSeverity.Error, true)]
   [InlineData(IssueSeverity.Critical, true)]
   [InlineData(IssueSeverity.Fatal, true)]
   public void HasWarningOrHigherIssues_ReturnsExpectedResult(
       IssueSeverity severity,
       bool expected)
   {
      var result = new TestResult
      {
         Issues =
          [
              CreateIssue(severity)
          ]
      };

      var actual = result.HasWarningOrHigherIssues();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasErrorOrHigherIssues_WhenResultIsNull_ThrowsArgumentNullException()
   {
      IResult? result = null;

      Assert.Throws<ArgumentNullException>(() =>
          result!.HasErrorOrHigherIssues());
   }

   [Fact]
   public void HasErrorOrHigherIssues_WhenIssuesAreEmpty_ReturnsFalse()
   {
      var result = new TestResult
      {
         Issues = IssueInfoListFactory.Empty()
      };

      var actual = result.HasErrorOrHigherIssues();

      Assert.False(actual);
   }

   [Theory]
   [InlineData(IssueSeverity.None, false)]
   [InlineData(IssueSeverity.Trace, false)]
   [InlineData(IssueSeverity.Debug, false)]
   [InlineData(IssueSeverity.Information, false)]
   [InlineData(IssueSeverity.Warning, false)]
   [InlineData(IssueSeverity.Error, true)]
   [InlineData(IssueSeverity.Critical, true)]
   [InlineData(IssueSeverity.Fatal, true)]
   public void HasErrorOrHigherIssues_ReturnsExpectedResult(
       IssueSeverity severity,
       bool expected)
   {
      var result = new TestResult
      {
         Issues =
          [
              CreateIssue(severity)
          ]
      };

      var actual = result.HasErrorOrHigherIssues();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasCriticalOrHigherIssues_WhenResultIsNull_ThrowsArgumentNullException()
   {
      IResult? result = null;

      Assert.Throws<ArgumentNullException>(() =>
          result!.HasCriticalOrHigherIssues());
   }

   [Fact]
   public void HasCriticalOrHigherIssues_WhenIssuesAreEmpty_ReturnsFalse()
   {
      var result = new TestResult
      {
         Issues = IssueInfoListFactory.Empty()
      };

      var actual = result.HasCriticalOrHigherIssues();

      Assert.False(actual);
   }

   [Theory]
   [InlineData(IssueSeverity.None, false)]
   [InlineData(IssueSeverity.Trace, false)]
   [InlineData(IssueSeverity.Debug, false)]
   [InlineData(IssueSeverity.Information, false)]
   [InlineData(IssueSeverity.Warning, false)]
   [InlineData(IssueSeverity.Error, false)]
   [InlineData(IssueSeverity.Critical, true)]
   [InlineData(IssueSeverity.Fatal, true)]
   public void HasCriticalOrHigherIssues_ReturnsExpectedResult(
       IssueSeverity severity,
       bool expected)
   {
      var result = new TestResult
      {
         Issues =
          [
              CreateIssue(severity)
          ]
      };

      var actual = result.HasCriticalOrHigherIssues();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasWarningOrHigherIssues_WhenAnyIssueMatches_ReturnsTrue()
   {
      var result = new TestResult
      {
         Issues =
          [
              CreateIssue(IssueSeverity.Information),
            CreateIssue(IssueSeverity.Warning)
          ]
      };

      var actual = result.HasWarningOrHigherIssues();

      Assert.True(actual);
   }

   [Fact]
   public void HasErrorOrHigherIssues_WhenAnyIssueMatches_ReturnsTrue()
   {
      var result = new TestResult
      {
         Issues =
          [
              CreateIssue(IssueSeverity.Warning),
            CreateIssue(IssueSeverity.Error)
          ]
      };

      var actual = result.HasErrorOrHigherIssues();

      Assert.True(actual);
   }

   [Fact]
   public void HasCriticalOrHigherIssues_WhenAnyIssueMatches_ReturnsTrue()
   {
      var result = new TestResult
      {
         Issues =
          [
              CreateIssue(IssueSeverity.Error),
            CreateIssue(IssueSeverity.Critical)
          ]
      };

      var actual = result.HasCriticalOrHigherIssues();

      Assert.True(actual);
   }

   private static IssueInfo CreateIssue(IssueSeverity severity)
   {
      return new IssueInfo
      {
         Code = $"AFW_{severity}",
         Message = $"Issue with severity {severity}.",
         Severity = severity
      };
   }
   [Fact]
   public void GetHighestIssueSeverity_WhenResultIsNull_ThrowsArgumentNullException()
   {
      IResult? result = null;

      Assert.Throws<ArgumentNullException>(() =>
          result!.GetHighestIssueSeverity());
   }

   [Fact]
   public void GetHighestIssueSeverity_WhenIssuesAreEmpty_ReturnsNone()
   {
      var result = new TestResult
      {
         Issues = IssueInfoListFactory.Empty()
      };

      var actual = result.GetHighestIssueSeverity();

      Assert.Equal(IssueSeverity.None, actual);
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
   public void GetHighestIssueSeverity_WhenResultContainsSingleIssue_ReturnsIssueSeverity(
       IssueSeverity severity)
   {
      var result = new TestResult
      {
         Issues =
          [
              CreateIssue(severity)
          ]
      };

      var actual = result.GetHighestIssueSeverity();

      Assert.Equal(severity, actual);
   }

   [Fact]
   public void GetHighestIssueSeverity_WhenResultContainsMultipleIssues_ReturnsHighestSeverity()
   {
      var result = new TestResult
      {
         Issues =
          [
              CreateIssue(IssueSeverity.Information),
            CreateIssue(IssueSeverity.Warning),
            CreateIssue(IssueSeverity.Error),
            CreateIssue(IssueSeverity.Debug)
          ]
      };

      var actual = result.GetHighestIssueSeverity();

      Assert.Equal(IssueSeverity.Error, actual);
   }

   [Fact]
   public void GetHighestIssueSeverity_WhenResultContainsFatal_ReturnsFatal()
   {
      var result = new TestResult
      {
         Issues =
          [
              CreateIssue(IssueSeverity.Information),
            CreateIssue(IssueSeverity.Fatal),
            CreateIssue(IssueSeverity.Warning),
            CreateIssue(IssueSeverity.Critical)
          ]
      };

      var actual = result.GetHighestIssueSeverity();

      Assert.Equal(IssueSeverity.Fatal, actual);
   }

   [Fact]
   public void GetHighestIssueSeverity_DoesNotDependOnIssueOrder()
   {
      var firstResult = new TestResult
      {
         Issues =
          [
              CreateIssue(IssueSeverity.Fatal),
            CreateIssue(IssueSeverity.Information),
            CreateIssue(IssueSeverity.Warning)
          ]
      };

      var secondResult = new TestResult
      {
         Issues =
          [
              CreateIssue(IssueSeverity.Information),
            CreateIssue(IssueSeverity.Warning),
            CreateIssue(IssueSeverity.Fatal)
          ]
      };

      var firstActual = firstResult.GetHighestIssueSeverity();
      var secondActual = secondResult.GetHighestIssueSeverity();

      Assert.Equal(IssueSeverity.Fatal, firstActual);
      Assert.Equal(IssueSeverity.Fatal, secondActual);
   }
   [Fact]
   public void GetStatusFromIssues_WhenResultIsNull_ThrowsArgumentNullException()
   {
      IResult? result = null;

      Assert.Throws<ArgumentNullException>(() =>
          result!.GetStatusFromIssues());
   }

   [Fact]
   public void GetStatusFromIssues_WhenIssuesAreEmpty_ReturnsSuccess()
   {
      var result = new TestResult
      {
         Status = ResultStatus.Unknown,
         Issues = IssueInfoListFactory.Empty()
      };

      var actual = result.GetStatusFromIssues();

      Assert.Equal(ResultStatus.Success, actual);
   }

   [Theory]
   [InlineData(IssueSeverity.None, ResultStatus.Success)]
   [InlineData(IssueSeverity.Trace, ResultStatus.Success)]
   [InlineData(IssueSeverity.Debug, ResultStatus.Success)]
   [InlineData(IssueSeverity.Information, ResultStatus.Success)]
   [InlineData(IssueSeverity.Warning, ResultStatus.SuccessWithWarnings)]
   [InlineData(IssueSeverity.Error, ResultStatus.Failed)]
   [InlineData(IssueSeverity.Critical, ResultStatus.Failed)]
   [InlineData(IssueSeverity.Fatal, ResultStatus.Failed)]
   public void GetStatusFromIssues_ReturnsExpectedStatus(
       IssueSeverity severity,
       ResultStatus expected)
   {
      var result = new TestResult
      {
         Status = ResultStatus.Unknown,
         Issues =
          [
              CreateIssue(severity)
          ]
      };

      var actual = result.GetStatusFromIssues();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void GetStatusFromIssues_WhenIssuesContainWarning_ReturnsSuccessWithWarnings()
   {
      var result = new TestResult
      {
         Status = ResultStatus.Success,
         Issues =
          [
              CreateIssue(IssueSeverity.Information),
            CreateIssue(IssueSeverity.Warning)
          ]
      };

      var actual = result.GetStatusFromIssues();

      Assert.Equal(ResultStatus.SuccessWithWarnings, actual);
   }

   [Fact]
   public void GetStatusFromIssues_WhenIssuesContainError_ReturnsFailed()
   {
      var result = new TestResult
      {
         Status = ResultStatus.SuccessWithWarnings,
         Issues =
          [
              CreateIssue(IssueSeverity.Information),
            CreateIssue(IssueSeverity.Warning),
            CreateIssue(IssueSeverity.Error)
          ]
      };

      var actual = result.GetStatusFromIssues();

      Assert.Equal(ResultStatus.Failed, actual);
   }

   [Fact]
   public void GetStatusFromIssues_WhenIssuesContainFatal_ReturnsFailed()
   {
      var result = new TestResult
      {
         Status = ResultStatus.Success,
         Issues =
          [
              CreateIssue(IssueSeverity.Information),
            CreateIssue(IssueSeverity.Fatal),
            CreateIssue(IssueSeverity.Warning)
          ]
      };

      var actual = result.GetStatusFromIssues();

      Assert.Equal(ResultStatus.Failed, actual);
   }

   [Fact]
   public void GetStatusFromIssues_DoesNotDependOnCurrentResultStatus()
   {
      var result = new TestResult
      {
         Status = ResultStatus.NotFound,
         Issues =
          [
              CreateIssue(IssueSeverity.Information)
          ]
      };

      var actual = result.GetStatusFromIssues();

      Assert.Equal(ResultStatus.Success, actual);
   }

   [Fact]
   public void GetStatusFromIssues_DoesNotModifyResultStatus()
   {
      var result = new TestResult
      {
         Status = ResultStatus.NotFound,
         Issues =
          [
              CreateIssue(IssueSeverity.Information)
          ]
      };

      _ = result.GetStatusFromIssues();

      Assert.Equal(ResultStatus.NotFound, result.Status);
   }
   [Fact]
   public void HasStatusMatchingIssues_WhenResultIsNull_ThrowsArgumentNullException()
   {
      IResult? result = null;

      Assert.Throws<ArgumentNullException>(() =>
          result!.HasStatusMatchingIssues());
   }

   [Fact]
   public void HasStatusMatchingIssues_WhenStatusMatchesEmptyIssues_ReturnsTrue()
   {
      var result = new TestResult
      {
         Status = ResultStatus.Success,
         Issues = IssueInfoListFactory.Empty()
      };

      var actual = result.HasStatusMatchingIssues();

      Assert.True(actual);
   }

   [Fact]
   public void HasStatusMatchingIssues_WhenStatusDoesNotMatchEmptyIssues_ReturnsFalse()
   {
      var result = new TestResult
      {
         Status = ResultStatus.Unknown,
         Issues = IssueInfoListFactory.Empty()
      };

      var actual = result.HasStatusMatchingIssues();

      Assert.False(actual);
   }

   [Theory]
   [InlineData(IssueSeverity.None, ResultStatus.Success, true)]
   [InlineData(IssueSeverity.Trace, ResultStatus.Success, true)]
   [InlineData(IssueSeverity.Debug, ResultStatus.Success, true)]
   [InlineData(IssueSeverity.Information, ResultStatus.Success, true)]
   [InlineData(IssueSeverity.Warning, ResultStatus.SuccessWithWarnings, true)]
   [InlineData(IssueSeverity.Error, ResultStatus.Failed, true)]
   [InlineData(IssueSeverity.Critical, ResultStatus.Failed, true)]
   [InlineData(IssueSeverity.Fatal, ResultStatus.Failed, true)]
   public void HasStatusMatchingIssues_WhenStatusMatchesInferredStatus_ReturnsTrue(
       IssueSeverity severity,
       ResultStatus status,
       bool expected)
   {
      var result = new TestResult
      {
         Status = status,
         Issues =
          [
              CreateIssue(severity)
          ]
      };

      var actual = result.HasStatusMatchingIssues();

      Assert.Equal(expected, actual);
   }

   [Theory]
   [InlineData(IssueSeverity.Information, ResultStatus.Failed)]
   [InlineData(IssueSeverity.Warning, ResultStatus.Success)]
   [InlineData(IssueSeverity.Error, ResultStatus.SuccessWithWarnings)]
   [InlineData(IssueSeverity.Critical, ResultStatus.Invalid)]
   [InlineData(IssueSeverity.Fatal, ResultStatus.NotFound)]
   public void HasStatusMatchingIssues_WhenStatusDoesNotMatchInferredStatus_ReturnsFalse(
       IssueSeverity severity,
       ResultStatus status)
   {
      var result = new TestResult
      {
         Status = status,
         Issues =
          [
              CreateIssue(severity)
          ]
      };

      var actual = result.HasStatusMatchingIssues();

      Assert.False(actual);
   }

   [Fact]
   public void HasStatusMatchingIssues_WhenWarningIssueAndSuccessWithWarningsStatus_ReturnsTrue()
   {
      var result = new TestResult
      {
         Status = ResultStatus.SuccessWithWarnings,
         Issues =
          [
              CreateIssue(IssueSeverity.Information),
            CreateIssue(IssueSeverity.Warning)
          ]
      };

      var actual = result.HasStatusMatchingIssues();

      Assert.True(actual);
   }

   [Fact]
   public void HasStatusMatchingIssues_WhenErrorIssueAndFailedStatus_ReturnsTrue()
   {
      var result = new TestResult
      {
         Status = ResultStatus.Failed,
         Issues =
          [
              CreateIssue(IssueSeverity.Information),
            CreateIssue(IssueSeverity.Warning),
            CreateIssue(IssueSeverity.Error)
          ]
      };

      var actual = result.HasStatusMatchingIssues();

      Assert.True(actual);
   }

   [Fact]
   public void HasStatusMatchingIssues_WhenErrorIssueAndSuccessWithWarningsStatus_ReturnsFalse()
   {
      var result = new TestResult
      {
         Status = ResultStatus.SuccessWithWarnings,
         Issues =
          [
              CreateIssue(IssueSeverity.Error)
          ]
      };

      var actual = result.HasStatusMatchingIssues();

      Assert.False(actual);
   }

   [Fact]
   public void HasStatusMatchingIssues_WhenInformationIssueAndNotFoundStatus_ReturnsFalse()
   {
      var result = new TestResult
      {
         Status = ResultStatus.NotFound,
         Issues =
          [
              CreateIssue(IssueSeverity.Information)
          ]
      };

      var actual = result.HasStatusMatchingIssues();

      Assert.False(actual);
   }
   [Fact]
   public void HasStatusMismatchWithIssues_WhenResultIsNull_ThrowsArgumentNullException()
   {
      IResult? result = null;

      Assert.Throws<ArgumentNullException>(() =>
          result!.HasStatusMismatchWithIssues());
   }

   [Fact]
   public void HasStatusMismatchWithIssues_WhenStatusMatchesEmptyIssues_ReturnsFalse()
   {
      var result = new TestResult
      {
         Status = ResultStatus.Success,
         Issues = IssueInfoListFactory.Empty()
      };

      var actual = result.HasStatusMismatchWithIssues();

      Assert.False(actual);
   }

   [Fact]
   public void HasStatusMismatchWithIssues_WhenStatusDoesNotMatchEmptyIssues_ReturnsTrue()
   {
      var result = new TestResult
      {
         Status = ResultStatus.Unknown,
         Issues = IssueInfoListFactory.Empty()
      };

      var actual = result.HasStatusMismatchWithIssues();

      Assert.True(actual);
   }

   [Theory]
   [InlineData(IssueSeverity.None, ResultStatus.Success)]
   [InlineData(IssueSeverity.Trace, ResultStatus.Success)]
   [InlineData(IssueSeverity.Debug, ResultStatus.Success)]
   [InlineData(IssueSeverity.Information, ResultStatus.Success)]
   [InlineData(IssueSeverity.Warning, ResultStatus.SuccessWithWarnings)]
   [InlineData(IssueSeverity.Error, ResultStatus.Failed)]
   [InlineData(IssueSeverity.Critical, ResultStatus.Failed)]
   [InlineData(IssueSeverity.Fatal, ResultStatus.Failed)]
   public void HasStatusMismatchWithIssues_WhenStatusMatchesInferredStatus_ReturnsFalse(
       IssueSeverity severity,
       ResultStatus status)
   {
      var result = new TestResult
      {
         Status = status,
         Issues =
          [
              CreateIssue(severity)
          ]
      };

      var actual = result.HasStatusMismatchWithIssues();

      Assert.False(actual);
   }

   [Theory]
   [InlineData(IssueSeverity.Information, ResultStatus.Failed)]
   [InlineData(IssueSeverity.Warning, ResultStatus.Success)]
   [InlineData(IssueSeverity.Error, ResultStatus.SuccessWithWarnings)]
   [InlineData(IssueSeverity.Critical, ResultStatus.Invalid)]
   [InlineData(IssueSeverity.Fatal, ResultStatus.NotFound)]
   public void HasStatusMismatchWithIssues_WhenStatusDoesNotMatchInferredStatus_ReturnsTrue(
       IssueSeverity severity,
       ResultStatus status)
   {
      var result = new TestResult
      {
         Status = status,
         Issues =
          [
              CreateIssue(severity)
          ]
      };

      var actual = result.HasStatusMismatchWithIssues();

      Assert.True(actual);
   }

   [Fact]
   public void HasStatusMismatchWithIssues_WhenWarningIssueAndSuccessWithWarningsStatus_ReturnsFalse()
   {
      var result = new TestResult
      {
         Status = ResultStatus.SuccessWithWarnings,
         Issues =
          [
              CreateIssue(IssueSeverity.Information),
            CreateIssue(IssueSeverity.Warning)
          ]
      };

      var actual = result.HasStatusMismatchWithIssues();

      Assert.False(actual);
   }

   [Fact]
   public void HasStatusMismatchWithIssues_WhenErrorIssueAndFailedStatus_ReturnsFalse()
   {
      var result = new TestResult
      {
         Status = ResultStatus.Failed,
         Issues =
          [
              CreateIssue(IssueSeverity.Information),
            CreateIssue(IssueSeverity.Warning),
            CreateIssue(IssueSeverity.Error)
          ]
      };

      var actual = result.HasStatusMismatchWithIssues();

      Assert.False(actual);
   }

   [Fact]
   public void HasStatusMismatchWithIssues_WhenErrorIssueAndSuccessWithWarningsStatus_ReturnsTrue()
   {
      var result = new TestResult
      {
         Status = ResultStatus.SuccessWithWarnings,
         Issues =
          [
              CreateIssue(IssueSeverity.Error)
          ]
      };

      var actual = result.HasStatusMismatchWithIssues();

      Assert.True(actual);
   }

   [Fact]
   public void HasStatusMismatchWithIssues_WhenInformationIssueAndNotFoundStatus_ReturnsTrue()
   {
      var result = new TestResult
      {
         Status = ResultStatus.NotFound,
         Issues =
          [
              CreateIssue(IssueSeverity.Information)
          ]
      };

      var actual = result.HasStatusMismatchWithIssues();

      Assert.True(actual);
   }

   [Theory]
   [InlineData(ResultStatus.Unknown, true)]
   [InlineData(ResultStatus.Success, false)]
   [InlineData(ResultStatus.SuccessWithWarnings, false)]
   [InlineData(ResultStatus.Partial, false)]
   [InlineData(ResultStatus.Failed, false)]
   [InlineData(ResultStatus.Invalid, false)]
   [InlineData(ResultStatus.NotSupported, false)]
   [InlineData(ResultStatus.Cancelled, false)]
   [InlineData(ResultStatus.NotFound, false)]
   public void IsUnknown_WhenResultStatusIsUnknown_ReturnsExpected(
       ResultStatus status,
       bool expected)
   {
      var actual = status.IsUnknown();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void IsPartial_WhenResultIsNull_ThrowsArgumentNullException()
   {
      IResult? result = null;

      Assert.Throws<ArgumentNullException>(() =>
          result!.IsPartial());
   }

   [Theory]
   [InlineData(ResultStatus.Unknown, false)]
   [InlineData(ResultStatus.Success, false)]
   [InlineData(ResultStatus.SuccessWithWarnings, false)]
   [InlineData(ResultStatus.Partial, true)]
   [InlineData(ResultStatus.Failed, false)]
   [InlineData(ResultStatus.Invalid, false)]
   [InlineData(ResultStatus.NotSupported, false)]
   [InlineData(ResultStatus.Cancelled, false)]
   [InlineData(ResultStatus.NotFound, false)]
   public void IsPartial_ReturnsExpectedResult(
       ResultStatus status,
       bool expected)
   {
      var result = new TestResult
      {
         Status = status
      };

      var actual = result.IsPartial();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void IsFailed_WhenResultIsNull_ThrowsArgumentNullException()
   {
      IResult? result = null;

      Assert.Throws<ArgumentNullException>(() =>
          result!.IsFailed());
   }

   [Theory]
   [InlineData(ResultStatus.Unknown, false)]
   [InlineData(ResultStatus.Success, false)]
   [InlineData(ResultStatus.SuccessWithWarnings, false)]
   [InlineData(ResultStatus.Partial, false)]
   [InlineData(ResultStatus.Failed, true)]
   [InlineData(ResultStatus.Invalid, false)]
   [InlineData(ResultStatus.NotSupported, false)]
   [InlineData(ResultStatus.Cancelled, false)]
   [InlineData(ResultStatus.NotFound, false)]
   public void IsFailed_ReturnsExpectedResult(
       ResultStatus status,
       bool expected)
   {
      var result = new TestResult
      {
         Status = status
      };

      var actual = result.IsFailed();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void IsNotSupported_WhenResultIsNull_ThrowsArgumentNullException()
   {
      IResult? result = null;

      Assert.Throws<ArgumentNullException>(() =>
          result!.IsNotSupported());
   }

   [Theory]
   [InlineData(ResultStatus.Unknown, false)]
   [InlineData(ResultStatus.Success, false)]
   [InlineData(ResultStatus.SuccessWithWarnings, false)]
   [InlineData(ResultStatus.Partial, false)]
   [InlineData(ResultStatus.Failed, false)]
   [InlineData(ResultStatus.Invalid, false)]
   [InlineData(ResultStatus.NotSupported, true)]
   [InlineData(ResultStatus.Cancelled, false)]
   [InlineData(ResultStatus.NotFound, false)]
   public void IsNotSupported_ReturnsExpectedResult(
       ResultStatus status,
       bool expected)
   {
      var result = new TestResult
      {
         Status = status
      };

      var actual = result.IsNotSupported();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void IsCancelled_WhenResultIsNull_ThrowsArgumentNullException()
   {
      IResult? result = null;

      Assert.Throws<ArgumentNullException>(() =>
          result!.IsCancelled());
   }

   [Theory]
   [InlineData(ResultStatus.Unknown, false)]
   [InlineData(ResultStatus.Success, false)]
   [InlineData(ResultStatus.SuccessWithWarnings, false)]
   [InlineData(ResultStatus.Partial, false)]
   [InlineData(ResultStatus.Failed, false)]
   [InlineData(ResultStatus.Invalid, false)]
   [InlineData(ResultStatus.NotSupported, false)]
   [InlineData(ResultStatus.Cancelled, true)]
   [InlineData(ResultStatus.NotFound, false)]
   public void IsCancelled_ReturnsExpectedResult(
       ResultStatus status,
       bool expected)
   {
      var result = new TestResult
      {
         Status = status
      };

      var actual = result.IsCancelled();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void IsFinal_WhenResultIsNull_ThrowsArgumentNullException()
   {
      IResult? result = null;

      Assert.Throws<ArgumentNullException>(() =>
          result!.IsFinal());
   }

   [Theory]
   [InlineData(ResultStatus.Unknown, false)]
   [InlineData(ResultStatus.Success, true)]
   [InlineData(ResultStatus.SuccessWithWarnings, true)]
   [InlineData(ResultStatus.Partial, true)]
   [InlineData(ResultStatus.Failed, true)]
   [InlineData(ResultStatus.Invalid, true)]
   [InlineData(ResultStatus.NotSupported, true)]
   [InlineData(ResultStatus.Cancelled, true)]
   [InlineData(ResultStatus.NotFound, true)]
   public void IsFinal_ReturnsExpectedResult(
       ResultStatus status,
       bool expected)
   {
      var result = new TestResult
      {
         Status = status
      };

      var actual = result.IsFinal();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void IsNonSuccess_WhenResultIsNull_ThrowsArgumentNullException()
   {
      IResult? result = null;

      Assert.Throws<ArgumentNullException>(() =>
          result!.IsNonSuccess());
   }

   [Theory]
   [InlineData(ResultStatus.Unknown, true)]
   [InlineData(ResultStatus.Success, false)]
   [InlineData(ResultStatus.SuccessWithWarnings, false)]
   [InlineData(ResultStatus.Partial, true)]
   [InlineData(ResultStatus.Failed, true)]
   [InlineData(ResultStatus.Invalid, true)]
   [InlineData(ResultStatus.NotSupported, true)]
   [InlineData(ResultStatus.Cancelled, true)]
   [InlineData(ResultStatus.NotFound, true)]
   public void IsNonSuccess_ReturnsExpectedResult(
       ResultStatus status,
       bool expected)
   {
      var result = new TestResult
      {
         Status = status
      };

      var actual = result.IsNonSuccess();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasIssues_WhenIssuesPropertyIsNull_ReturnsFalse()
   {
      var result = new TestResult
      {
         Issues = null!
      };

      var actual = result.HasIssues();

      Assert.False(actual);
   }

   [Fact]
   public void HasErrors_WhenIssuesPropertyIsNull_ReturnsFalse()
   {
      var result = new TestResult
      {
         Issues = null!
      };

      var actual = result.HasErrors();

      Assert.False(actual);
   }

   [Fact]
   public void HasIssueCode_WhenIssuesPropertyIsNull_ReturnsFalse()
   {
      var result = new TestResult
      {
         Issues = null!
      };

      var actual = result.HasIssueCode("AFW_WARNING");

      Assert.False(actual);
   }

   [Fact]
   public void TryGetIssueByCode_WhenIssuesPropertyIsNull_ReturnsFalseAndNullIssue()
   {
      var result = new TestResult
      {
         Issues = null!
      };

      var actual = result.TryGetIssueByCode(
         "AFW_WARNING",
         out var issue);

      Assert.False(actual);
      Assert.Null(issue);
   }

   [Fact]
   public void IsCleanSuccess_WhenIssuesPropertyIsNullAndResultIsSuccess_ReturnsTrue()
   {
      var result = new TestResult
      {
         Status = ResultStatus.Success,
         Issues = null!
      };

      var actual = result.IsCleanSuccess();

      Assert.True(actual);
   }

   [Fact]
   public void IsDirtySuccess_WhenIssuesPropertyIsNullAndResultIsSuccess_ReturnsFalse()
   {
      var result = new TestResult
      {
         Status = ResultStatus.Success,
         Issues = null!
      };

      var actual = result.IsDirtySuccess();

      Assert.False(actual);
   }

   [Fact]
   public void HasWarningOrHigherIssues_WhenIssuesPropertyIsNull_ReturnsFalse()
   {
      var result = new TestResult
      {
         Issues = null!
      };

      var actual = result.HasWarningOrHigherIssues();

      Assert.False(actual);
   }

   [Fact]
   public void HasErrorOrHigherIssues_WhenIssuesPropertyIsNull_ReturnsFalse()
   {
      var result = new TestResult
      {
         Issues = null!
      };

      var actual = result.HasErrorOrHigherIssues();

      Assert.False(actual);
   }

   [Fact]
   public void HasCriticalOrHigherIssues_WhenIssuesPropertyIsNull_ReturnsFalse()
   {
      var result = new TestResult
      {
         Issues = null!
      };

      var actual = result.HasCriticalOrHigherIssues();

      Assert.False(actual);
   }

   [Fact]
   public void GetHighestIssueSeverity_WhenIssuesPropertyIsNull_ReturnsNone()
   {
      var result = new TestResult
      {
         Issues = null!
      };

      var actual = result.GetHighestIssueSeverity();

      Assert.Equal(IssueSeverity.None, actual);
   }

   [Fact]
   public void GetStatusFromIssues_WhenIssuesPropertyIsNull_ReturnsSuccess()
   {
      var result = new TestResult
      {
         Issues = null!
      };

      var actual = result.GetStatusFromIssues();

      Assert.Equal(ResultStatus.Success, actual);
   }

   [Fact]
   public void HasStatusMatchingIssues_WhenIssuesPropertyIsNullAndStatusIsSuccess_ReturnsTrue()
   {
      var result = new TestResult
      {
         Status = ResultStatus.Success,
         Issues = null!
      };

      var actual = result.HasStatusMatchingIssues();

      Assert.True(actual);
   }

   [Fact]
   public void HasStatusMismatchWithIssues_WhenIssuesPropertyIsNullAndStatusIsUnknown_ReturnsTrue()
   {
      var result = new TestResult
      {
         Status = ResultStatus.Unknown,
         Issues = null!
      };

      var actual = result.HasStatusMismatchWithIssues();

      Assert.True(actual);
   }
   private sealed class TestResult : IResult
   {
      public ResultStatus Status { get; set; } = ResultStatus.Unknown;

      public string Message { get; set; } = string.Empty;

      public IReadOnlyList<IssueInfo> Issues { get; set; } = IssueInfoListFactory.Empty();

      public MetadataBag Metadata { get; set; } = MetadataBagFactory.Empty();

      public bool IsSuccess =>
  Status.IsSuccess();

      public bool IsFailure =>
          Status.IsFailure();

      public bool HasWarnings =>
 Status.HasWarnings()
 || Issues?.HasWarningOrHigherIssues() == true;
   }
}
