using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Extensions;
using Afrowave.Toolbox.Essentials.Issues;

namespace Afrowave.Toolbox.Essentials.Tests.Extensions;

public sealed class IssueInfoCollectionExtensionsTests
{
   [Fact]
   public void HasErrors_WhenIssuesIsNull_ThrowsArgumentNullException()
   {
      IEnumerable<IssueInfo>? issues = null;

      Assert.Throws<ArgumentNullException>(() => issues!.HasErrors());
   }

   [Fact]
   public void HasErrors_WhenCollectionIsEmpty_ReturnsFalse()
   {
      var issues = Array.Empty<IssueInfo>();

      var actual = issues.HasErrors();

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
      var issues = new[]
      {
            CreateIssue(severity)
        };

      var actual = issues.HasErrors();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasWarningsOrErrors_WhenIssuesIsNull_ThrowsArgumentNullException()
   {
      IEnumerable<IssueInfo>? issues = null;

      Assert.Throws<ArgumentNullException>(() => issues!.HasWarningsOrErrors());
   }

   [Fact]
   public void HasWarningsOrErrors_WhenCollectionIsEmpty_ReturnsFalse()
   {
      var issues = Array.Empty<IssueInfo>();

      var actual = issues.HasWarningsOrErrors();

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
      var issues = new[]
      {
            CreateIssue(severity)
        };

      var actual = issues.HasWarningsOrErrors();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void Errors_WhenIssuesIsNull_ThrowsArgumentNullException()
   {
      IEnumerable<IssueInfo>? issues = null;

      Assert.Throws<ArgumentNullException>(() => issues!.Errors().ToArray());
   }

   [Fact]
   public void Errors_ReturnsOnlyErrorOrHigherSeverityIssues()
   {
      var issues = new[]
      {
            CreateIssue(IssueSeverity.None, "none"),
            CreateIssue(IssueSeverity.Trace, "trace"),
            CreateIssue(IssueSeverity.Debug, "debug"),
            CreateIssue(IssueSeverity.Information, "information"),
            CreateIssue(IssueSeverity.Warning, "warning"),
            CreateIssue(IssueSeverity.Error, "error"),
            CreateIssue(IssueSeverity.Critical, "critical"),
            CreateIssue(IssueSeverity.Fatal, "fatal")
        };

      var actual = issues.Errors().Select(issue => issue.Code).ToArray();

      Assert.Equal(
          new[] { "error", "critical", "fatal" },
          actual);
   }

   [Fact]
   public void Warnings_WhenIssuesIsNull_ThrowsArgumentNullException()
   {
      IEnumerable<IssueInfo>? issues = null;

      Assert.Throws<ArgumentNullException>(() => issues!.Warnings().ToArray());
   }

   [Fact]
   public void Warnings_ReturnsOnlyWarningSeverityIssues()
   {
      var issues = new[]
      {
            CreateIssue(IssueSeverity.None, "none"),
            CreateIssue(IssueSeverity.Trace, "trace"),
            CreateIssue(IssueSeverity.Debug, "debug"),
            CreateIssue(IssueSeverity.Information, "information"),
            CreateIssue(IssueSeverity.Warning, "warning"),
            CreateIssue(IssueSeverity.Error, "error"),
            CreateIssue(IssueSeverity.Critical, "critical"),
            CreateIssue(IssueSeverity.Fatal, "fatal")
        };

      var actual = issues.Warnings().Select(issue => issue.Code).ToArray();

      Assert.Equal(
          new[] { "warning" },
          actual);
   }

   [Fact]
   public void Informational_WhenIssuesIsNull_ThrowsArgumentNullException()
   {
      IEnumerable<IssueInfo>? issues = null;

      Assert.Throws<ArgumentNullException>(() => issues!.Informational().ToArray());
   }

   [Fact]
   public void Informational_ReturnsOnlyInformationOrLowerSeverityIssues()
   {
      var issues = new[]
      {
            CreateIssue(IssueSeverity.None, "none"),
            CreateIssue(IssueSeverity.Trace, "trace"),
            CreateIssue(IssueSeverity.Debug, "debug"),
            CreateIssue(IssueSeverity.Information, "information"),
            CreateIssue(IssueSeverity.Warning, "warning"),
            CreateIssue(IssueSeverity.Error, "error"),
            CreateIssue(IssueSeverity.Critical, "critical"),
            CreateIssue(IssueSeverity.Fatal, "fatal")
        };

      var actual = issues.Informational().Select(issue => issue.Code).ToArray();

      Assert.Equal(
          new[] { "none", "trace", "debug", "information" },
          actual);
   }

   [Fact]
   public void WithSeverity_WhenIssuesIsNull_ThrowsArgumentNullException()
   {
      IEnumerable<IssueInfo>? issues = null;

      Assert.Throws<ArgumentNullException>(() => issues!.WithSeverity(IssueSeverity.Warning).ToArray());
   }

   [Theory]
   [InlineData(IssueSeverity.None, "none")]
   [InlineData(IssueSeverity.Trace, "trace")]
   [InlineData(IssueSeverity.Debug, "debug")]
   [InlineData(IssueSeverity.Information, "information")]
   [InlineData(IssueSeverity.Warning, "warning")]
   [InlineData(IssueSeverity.Error, "error")]
   [InlineData(IssueSeverity.Critical, "critical")]
   [InlineData(IssueSeverity.Fatal, "fatal")]
   public void WithSeverity_ReturnsOnlyIssuesWithSpecifiedSeverity(
       IssueSeverity severity,
       string expectedCode)
   {
      var issues = new[]
      {
            CreateIssue(IssueSeverity.None, "none"),
            CreateIssue(IssueSeverity.Trace, "trace"),
            CreateIssue(IssueSeverity.Debug, "debug"),
            CreateIssue(IssueSeverity.Information, "information"),
            CreateIssue(IssueSeverity.Warning, "warning"),
            CreateIssue(IssueSeverity.Error, "error"),
            CreateIssue(IssueSeverity.Critical, "critical"),
            CreateIssue(IssueSeverity.Fatal, "fatal")
        };

      var actual = issues
          .WithSeverity(severity)
          .Select(issue => issue.Code)
          .ToArray();

      Assert.Equal(
          new[] { expectedCode },
          actual);
   }

   private static IssueInfo CreateIssue(
       IssueSeverity severity,
       string? code = null)
   {
      return new IssueInfo
      {
         Code = code ?? severity.ToString().ToLowerInvariant(),
         Message = $"Test issue with severity {severity}.",
         Severity = severity
      };
   }
}