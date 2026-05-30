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
   [Fact]
public void AppendIssue_WhenIssuesIsNull_ThrowsArgumentNullException()
{
    IReadOnlyList<IssueInfo>? issues = null;

    var issue = IssueInfoFactory.Warning(
        "AFW_WARNING",
        "Warning message.");

    Assert.Throws<ArgumentNullException>(() =>
        issues!.AppendIssue(issue));
}

[Fact]
public void AppendIssue_WhenIssueIsNull_ThrowsArgumentNullException()
{
    IReadOnlyList<IssueInfo> issues = [];

    IssueInfo? issue = null;

    Assert.Throws<ArgumentNullException>(() =>
        issues.AppendIssue(issue!));
}

[Fact]
public void AppendIssue_WhenIssuesAreEmpty_ReturnsListWithAppendedIssue()
{
    IReadOnlyList<IssueInfo> issues = [];

    var issue = IssueInfoFactory.Warning(
        "AFW_WARNING",
        "Warning message.");

    var result = issues.AppendIssue(issue);

    Assert.Single(result);
    Assert.Same(issue, result[0]);
}

[Fact]
public void AppendIssue_WhenIssuesContainItems_ReturnsListWithOriginalItemsAndAppendedIssue()
{
    var first = IssueInfoFactory.Information(
        "AFW_INFO",
        "Information message.");

    var second = IssueInfoFactory.Warning(
        "AFW_WARNING",
        "Warning message.");

    IReadOnlyList<IssueInfo> issues =
    [
        first
    ];

    var result = issues.AppendIssue(second);

    Assert.Equal(2, result.Count);
    Assert.Same(first, result[0]);
    Assert.Same(second, result[1]);
}

[Fact]
public void AppendIssue_ReturnsNewList()
{
    var first = IssueInfoFactory.Information(
        "AFW_INFO",
        "Information message.");

    var second = IssueInfoFactory.Warning(
        "AFW_WARNING",
        "Warning message.");

    IReadOnlyList<IssueInfo> issues =
    [
        first
    ];

    var result = issues.AppendIssue(second);

    Assert.NotSame(issues, result);
}

[Fact]
public void AppendIssue_DoesNotModifyOriginalList()
{
    var first = IssueInfoFactory.Information(
        "AFW_INFO",
        "Information message.");

    var second = IssueInfoFactory.Warning(
        "AFW_WARNING",
        "Warning message.");

    IReadOnlyList<IssueInfo> issues =
    [
        first
    ];

    var result = issues.AppendIssue(second);

    Assert.Single(issues);
    Assert.Same(first, issues[0]);

    Assert.Equal(2, result.Count);
    Assert.Same(second, result[1]);
}

[Fact]
public void AppendIssues_WhenIssuesIsNull_ThrowsArgumentNullException()
{
    IReadOnlyList<IssueInfo>? issues = null;

    IReadOnlyList<IssueInfo> additionalIssues =
    [
        IssueInfoFactory.Warning(
            "AFW_WARNING",
            "Warning message.")
    ];

    Assert.Throws<ArgumentNullException>(() =>
        issues!.AppendIssues(additionalIssues));
}

[Fact]
public void AppendIssues_WhenAdditionalIssuesIsNull_ThrowsArgumentNullException()
{
    IReadOnlyList<IssueInfo> issues = [];

    IEnumerable<IssueInfo>? additionalIssues = null;

    Assert.Throws<ArgumentNullException>(() =>
        issues.AppendIssues(additionalIssues!));
}

[Fact]
public void AppendIssues_WhenBothCollectionsAreEmpty_ReturnsEmptyList()
{
    IReadOnlyList<IssueInfo> issues = [];
    IReadOnlyList<IssueInfo> additionalIssues = [];

    var result = issues.AppendIssues(additionalIssues);

    Assert.NotNull(result);
    Assert.Empty(result);
}

[Fact]
public void AppendIssues_WhenSourceIsEmpty_ReturnsAdditionalIssues()
{
    IReadOnlyList<IssueInfo> issues = [];

    var first = IssueInfoFactory.Warning(
        "AFW_WARNING",
        "Warning message.");

    var second = IssueInfoFactory.Error(
        "AFW_ERROR",
        "Error message.");

    IReadOnlyList<IssueInfo> additionalIssues =
    [
        first,
        second
    ];

    var result = issues.AppendIssues(additionalIssues);

    Assert.Equal(2, result.Count);
    Assert.Same(first, result[0]);
    Assert.Same(second, result[1]);
}

[Fact]
public void AppendIssues_WhenAdditionalIssuesAreEmpty_ReturnsOriginalIssues()
{
    var first = IssueInfoFactory.Information(
        "AFW_INFO",
        "Information message.");

    IReadOnlyList<IssueInfo> issues =
    [
        first
    ];

    IReadOnlyList<IssueInfo> additionalIssues = [];

    var result = issues.AppendIssues(additionalIssues);

    Assert.Single(result);
    Assert.Same(first, result[0]);
}

[Fact]
public void AppendIssues_ReturnsListWithOriginalAndAdditionalIssues()
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

    IReadOnlyList<IssueInfo> issues =
    [
        first
    ];

    IReadOnlyList<IssueInfo> additionalIssues =
    [
        second,
        third
    ];

    var result = issues.AppendIssues(additionalIssues);

    Assert.Equal(3, result.Count);
    Assert.Same(first, result[0]);
    Assert.Same(second, result[1]);
    Assert.Same(third, result[2]);
}

[Fact]
public void AppendIssues_ReturnsNewList()
{
    var first = IssueInfoFactory.Information(
        "AFW_INFO",
        "Information message.");

    IReadOnlyList<IssueInfo> issues =
    [
        first
    ];

    IReadOnlyList<IssueInfo> additionalIssues = [];

    var result = issues.AppendIssues(additionalIssues);

    Assert.NotSame(issues, result);
}

[Fact]
public void AppendIssues_ReturnsSnapshotOfAdditionalIssues()
{
    var first = IssueInfoFactory.Information(
        "AFW_INFO",
        "Information message.");

    var second = IssueInfoFactory.Warning(
        "AFW_WARNING",
        "Warning message.");

    IReadOnlyList<IssueInfo> issues =
    [
        first
    ];

    var additionalIssues = new List<IssueInfo>
    {
        second
    };

    var result = issues.AppendIssues(additionalIssues);

    additionalIssues.Clear();

    Assert.Equal(2, result.Count);
    Assert.Same(first, result[0]);
    Assert.Same(second, result[1]);
}
}