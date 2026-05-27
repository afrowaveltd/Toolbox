using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Extensions;
using Afrowave.Toolbox.Essentials.Issues;

namespace Afrowave.Toolbox.Essentials.Tests.Extensions;

public sealed class IssueInfoExtensionsTests
{
   [Fact]
   public void HasCode_WhenIssueIsNull_ThrowsArgumentNullException()
   {
      IssueInfo? issue = null;

      Assert.Throws<ArgumentNullException>(() =>
          issue!.HasCode());
   }

   [Theory]
   [InlineData("", false)]
   [InlineData("   ", false)]
   [InlineData("AFW001", true)]
   [InlineData(" issue.code ", true)]
   public void HasCode_ReturnsExpectedResult(
       string code,
       bool expected)
   {
      var issue = new IssueInfo
      {
         Code = code
      };

      var actual = issue.HasCode();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasNumber_WhenIssueIsNull_ThrowsArgumentNullException()
   {
      IssueInfo? issue = null;

      Assert.Throws<ArgumentNullException>(() =>
          issue!.HasNumber());
   }

   [Theory]
   [InlineData(null, false)]
   [InlineData(0, true)]
   [InlineData(1, true)]
   [InlineData(42, true)]
   [InlineData(-1, true)]
   public void HasNumber_ReturnsExpectedResult(
       int? number,
       bool expected)
   {
      var issue = new IssueInfo
      {
         Number = number
      };

      var actual = issue.HasNumber();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasMessage_WhenIssueIsNull_ThrowsArgumentNullException()
   {
      IssueInfo? issue = null;

      Assert.Throws<ArgumentNullException>(() =>
          issue!.HasMessage());
   }

   [Theory]
   [InlineData("", false)]
   [InlineData("   ", false)]
   [InlineData("Issue message.", true)]
   [InlineData(" message ", true)]
   public void HasMessage_ReturnsExpectedResult(
       string message,
       bool expected)
   {
      var issue = new IssueInfo
      {
         Message = message
      };

      var actual = issue.HasMessage();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasDetails_WhenIssueIsNull_ThrowsArgumentNullException()
   {
      IssueInfo? issue = null;

      Assert.Throws<ArgumentNullException>(() =>
          issue!.HasDetails());
   }

   [Theory]
   [InlineData(null, false)]
   [InlineData("", false)]
   [InlineData("   ", false)]
   [InlineData("Detailed issue information.", true)]
   [InlineData(" details ", true)]
   public void HasDetails_ReturnsExpectedResult(
       string? details,
       bool expected)
   {
      var issue = new IssueInfo
      {
         Details = details
      };

      var actual = issue.HasDetails();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void IsErrorOrHigher_WhenIssueIsNull_ThrowsArgumentNullException()
   {
      IssueInfo? issue = null;

      Assert.Throws<ArgumentNullException>(() =>
          issue!.IsErrorOrHigher());
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
   public void IsErrorOrHigher_ReturnsExpectedResult(
       IssueSeverity severity,
       bool expected)
   {
      var issue = CreateIssue(severity);

      var actual = issue.IsErrorOrHigher();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void IsWarningOrHigher_WhenIssueIsNull_ThrowsArgumentNullException()
   {
      IssueInfo? issue = null;

      Assert.Throws<ArgumentNullException>(() =>
          issue!.IsWarningOrHigher());
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
   public void IsWarningOrHigher_ReturnsExpectedResult(
       IssueSeverity severity,
       bool expected)
   {
      var issue = CreateIssue(severity);

      var actual = issue.IsWarningOrHigher();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void IsInformationOrLower_WhenIssueIsNull_ThrowsArgumentNullException()
   {
      IssueInfo? issue = null;

      Assert.Throws<ArgumentNullException>(() =>
          issue!.IsInformationOrLower());
   }

   [Theory]
   [InlineData(IssueSeverity.None, true)]
   [InlineData(IssueSeverity.Trace, true)]
   [InlineData(IssueSeverity.Debug, true)]
   [InlineData(IssueSeverity.Information, true)]
   [InlineData(IssueSeverity.Warning, false)]
   [InlineData(IssueSeverity.Error, false)]
   [InlineData(IssueSeverity.Critical, false)]
   [InlineData(IssueSeverity.Fatal, false)]
   public void IsInformationOrLower_ReturnsExpectedResult(
       IssueSeverity severity,
       bool expected)
   {
      var issue = CreateIssue(severity);

      var actual = issue.IsInformationOrLower();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void IsCriticalOrHigher_WhenIssueIsNull_ThrowsArgumentNullException()
   {
      IssueInfo? issue = null;

      Assert.Throws<ArgumentNullException>(() =>
          issue!.IsCriticalOrHigher());
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
   public void IsCriticalOrHigher_ReturnsExpectedResult(
       IssueSeverity severity,
       bool expected)
   {
      var issue = CreateIssue(severity);

      var actual = issue.IsCriticalOrHigher();

      Assert.Equal(expected, actual);
   }

   private static IssueInfo CreateIssue(IssueSeverity severity)
   {
      return new IssueInfo
      {
         Code = severity.ToString(),
         Message = $"Test issue with severity {severity}.",
         Severity = severity
      };
   }
}