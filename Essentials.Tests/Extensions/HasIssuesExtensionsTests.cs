using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Extensions;
using Afrowave.Toolbox.Essentials.Interfaces;
using Afrowave.Toolbox.Essentials.Issues;

namespace Afrowave.Toolbox.Essentials.Tests.Extensions;

public sealed class HasIssuesExtensionsTests
{
   [Fact]
   public void HasErrors_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasIssues? value = null;

      Assert.Throws<ArgumentNullException>(() => value!.HasErrors());
   }

   [Fact]
   public void HasErrors_WhenIssuesAreEmpty_ReturnsFalse()
   {
      var value = new TestHasIssues([]);

      var actual = value.HasErrors();

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
      var value = new TestHasIssues(
      [
          CreateIssue(severity)
      ]);

      var actual = value.HasErrors();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasWarningsOrErrors_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasIssues? value = null;

      Assert.Throws<ArgumentNullException>(() => value!.HasWarningsOrErrors());
   }

   [Fact]
   public void HasWarningsOrErrors_WhenIssuesAreEmpty_ReturnsFalse()
   {
      var value = new TestHasIssues([]);

      var actual = value.HasWarningsOrErrors();

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
   public void HasWarningsOrErrors_ReturnsExpectedResult(
       IssueSeverity severity,
       bool expected)
   {
      var value = new TestHasIssues(
      [
          CreateIssue(severity)
      ]);

      var actual = value.HasWarningsOrErrors();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasAnyIssues_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasIssues? value = null;

      Assert.Throws<ArgumentNullException>(() => value!.HasAnyIssues());
   }

   [Fact]
   public void HasAnyIssues_WhenIssuesAreEmpty_ReturnsFalse()
   {
      var value = new TestHasIssues([]);

      var actual = value.HasAnyIssues();

      Assert.False(actual);
   }

   [Fact]
   public void HasAnyIssues_WhenIssuesContainOneItem_ReturnsTrue()
   {
      var value = new TestHasIssues(
      [
          CreateIssue(IssueSeverity.Information)
      ]);

      var actual = value.HasAnyIssues();

      Assert.True(actual);
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

   private sealed class TestHasIssues : IHasIssues
   {
      public TestHasIssues(IReadOnlyList<IssueInfo> issues)
      {
         Issues = issues;
      }

      public IReadOnlyList<IssueInfo> Issues { get; }
   }

   [Fact]
   public void HasWarningOrHigherIssues_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasIssues? value = null;

      Assert.Throws<ArgumentNullException>(() =>
         value!.HasWarningOrHigherIssues());
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
      var value = new TestIssuesSource
      {
         Issues =
         [
            CreateIssueInfo(severity)
         ]
      };

      var actual = value.HasWarningOrHigherIssues();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasErrorOrHigherIssues_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasIssues? value = null;

      Assert.Throws<ArgumentNullException>(() =>
         value!.HasErrorOrHigherIssues());
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
      var value = new TestIssuesSource
      {
         Issues =
         [
            CreateIssueInfo(severity)
         ]
      };

      var actual = value.HasErrorOrHigherIssues();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasCriticalOrHigherIssues_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasIssues? value = null;

      Assert.Throws<ArgumentNullException>(() =>
         value!.HasCriticalOrHigherIssues());
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
      var value = new TestIssuesSource
      {
         Issues =
         [
            CreateIssueInfo(severity)
         ]
      };

      var actual = value.HasCriticalOrHigherIssues();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasOnlyInformationalOrLowerIssues_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasIssues? value = null;

      Assert.Throws<ArgumentNullException>(() =>
         value!.HasOnlyInformationalOrLowerIssues());
   }

   [Fact]
   public void HasOnlyInformationalOrLowerIssues_WhenIssuesAreEmpty_ReturnsTrue()
   {
      var value = new TestIssuesSource
      {
         Issues = []
      };

      var actual = value.HasOnlyInformationalOrLowerIssues();

      Assert.True(actual);
   }

   [Theory]
   [InlineData(IssueSeverity.None)]
   [InlineData(IssueSeverity.Trace)]
   [InlineData(IssueSeverity.Debug)]
   [InlineData(IssueSeverity.Information)]
   public void HasOnlyInformationalOrLowerIssues_WhenSingleIssueIsInformationalOrLower_ReturnsTrue(
      IssueSeverity severity)
   {
      var value = new TestIssuesSource
      {
         Issues =
         [
            CreateIssueInfo(severity)
         ]
      };

      var actual = value.HasOnlyInformationalOrLowerIssues();

      Assert.True(actual);
   }

   [Theory]
   [InlineData(IssueSeverity.Warning)]
   [InlineData(IssueSeverity.Error)]
   [InlineData(IssueSeverity.Critical)]
   [InlineData(IssueSeverity.Fatal)]
   public void HasOnlyInformationalOrLowerIssues_WhenSingleIssueIsWarningOrHigher_ReturnsFalse(
      IssueSeverity severity)
   {
      var value = new TestIssuesSource
      {
         Issues =
         [
            CreateIssueInfo(severity)
         ]
      };

      var actual = value.HasOnlyInformationalOrLowerIssues();

      Assert.False(actual);
   }

   [Fact]
   public void GetHighestIssueSeverity_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasIssues? value = null;

      Assert.Throws<ArgumentNullException>(() =>
         value!.GetHighestIssueSeverity());
   }

   [Fact]
   public void GetHighestIssueSeverity_WhenIssuesAreEmpty_ReturnsNone()
   {
      var value = new TestIssuesSource
      {
         Issues = []
      };

      var actual = value.GetHighestIssueSeverity();

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
   public void GetHighestIssueSeverity_WhenContainsSingleIssue_ReturnsIssueSeverity(
      IssueSeverity severity)
   {
      var value = new TestIssuesSource
      {
         Issues =
         [
            CreateIssueInfo(severity)
         ]
      };

      var actual = value.GetHighestIssueSeverity();

      Assert.Equal(severity, actual);
   }

   [Fact]
   public void GetHighestIssueSeverity_WhenContainsMultipleIssues_ReturnsHighestSeverity()
   {
      var value = new TestIssuesSource
      {
         Issues =
         [
            CreateIssueInfo(IssueSeverity.Information),
         CreateIssueInfo(IssueSeverity.Warning),
         CreateIssueInfo(IssueSeverity.Error),
         CreateIssueInfo(IssueSeverity.Debug)
         ]
      };

      var actual = value.GetHighestIssueSeverity();

      Assert.Equal(IssueSeverity.Error, actual);
   }

   [Fact]
   public void GetHighestIssueSeverity_WhenContainsFatal_ReturnsFatal()
   {
      var value = new TestIssuesSource
      {
         Issues =
         [
            CreateIssueInfo(IssueSeverity.Information),
         CreateIssueInfo(IssueSeverity.Fatal),
         CreateIssueInfo(IssueSeverity.Warning),
         CreateIssueInfo(IssueSeverity.Critical)
         ]
      };

      var actual = value.GetHighestIssueSeverity();

      Assert.Equal(IssueSeverity.Fatal, actual);
   }

   private static IssueInfo CreateIssueInfo(IssueSeverity severity)
   {
      return new IssueInfo
      {
         Code = $"AFW_{severity}",
         Message = $"Issue with severity {severity}.",
         Severity = severity
      };
   }

   private sealed class TestIssuesSource : IHasIssues
   {
      public IReadOnlyList<IssueInfo> Issues { get; set; } = [];
   }
}