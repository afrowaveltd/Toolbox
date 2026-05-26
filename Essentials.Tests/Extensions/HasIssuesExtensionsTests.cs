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
}