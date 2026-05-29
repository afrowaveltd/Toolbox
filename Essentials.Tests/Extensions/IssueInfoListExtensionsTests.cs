using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Extensions;
using Afrowave.Toolbox.Essentials.Issues;

namespace Afrowave.Toolbox.Essentials.Tests.Extensions;

public sealed class IssueInfoListExtensionsTests
{
    [Fact]
    public void HasAnyIssues_WhenIssuesIsNull_ThrowsArgumentNullException()
    {
        IReadOnlyList<IssueInfo>? issues = null;

        Assert.Throws<ArgumentNullException>(() =>
            issues!.HasAnyIssues());
    }

    [Fact]
    public void HasAnyIssues_WhenIssuesAreEmpty_ReturnsFalse()
    {
        IReadOnlyList<IssueInfo> issues = [];

        var actual = issues.HasAnyIssues();

        Assert.False(actual);
    }

    [Fact]
    public void HasAnyIssues_WhenIssuesContainItem_ReturnsTrue()
    {
        IReadOnlyList<IssueInfo> issues =
        [
            IssueInfoFactory.Warning(
                "AFW_WARNING",
                "Warning message.")
        ];

        var actual = issues.HasAnyIssues();

        Assert.True(actual);
    }

    [Fact]
    public void HasWarningsOrErrors_WhenIssuesIsNull_ThrowsArgumentNullException()
    {
        IReadOnlyList<IssueInfo>? issues = null;

        Assert.Throws<ArgumentNullException>(() =>
            issues!.HasWarningsOrErrors());
    }

    [Fact]
    public void HasWarningsOrErrors_WhenIssuesAreEmpty_ReturnsFalse()
    {
        IReadOnlyList<IssueInfo> issues = [];

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
        IReadOnlyList<IssueInfo> issues =
        [
            CreateIssue(severity)
        ];

        var actual = issues.HasWarningsOrErrors();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void HasErrors_WhenIssuesIsNull_ThrowsArgumentNullException()
    {
        IReadOnlyList<IssueInfo>? issues = null;

        Assert.Throws<ArgumentNullException>(() =>
            issues!.HasErrors());
    }

    [Fact]
    public void HasErrors_WhenIssuesAreEmpty_ReturnsFalse()
    {
        IReadOnlyList<IssueInfo> issues = [];

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
        IReadOnlyList<IssueInfo> issues =
        [
            CreateIssue(severity)
        ];

        var actual = issues.HasErrors();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void HasCriticalOrFatalIssues_WhenIssuesIsNull_ThrowsArgumentNullException()
    {
        IReadOnlyList<IssueInfo>? issues = null;

        Assert.Throws<ArgumentNullException>(() =>
            issues!.HasCriticalOrFatalIssues());
    }

    [Fact]
    public void HasCriticalOrFatalIssues_WhenIssuesAreEmpty_ReturnsFalse()
    {
        IReadOnlyList<IssueInfo> issues = [];

        var actual = issues.HasCriticalOrFatalIssues();

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
    public void HasCriticalOrFatalIssues_ReturnsExpectedResult(
        IssueSeverity severity,
        bool expected)
    {
        IReadOnlyList<IssueInfo> issues =
        [
            CreateIssue(severity)
        ];

        var actual = issues.HasCriticalOrFatalIssues();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void HasIssueWithSeverity_WhenIssuesIsNull_ThrowsArgumentNullException()
    {
        IReadOnlyList<IssueInfo>? issues = null;

        Assert.Throws<ArgumentNullException>(() =>
            issues!.HasIssueWithSeverity(IssueSeverity.Warning));
    }

    [Fact]
    public void HasIssueWithSeverity_WhenIssuesAreEmpty_ReturnsFalse()
    {
        IReadOnlyList<IssueInfo> issues = [];

        var actual = issues.HasIssueWithSeverity(IssueSeverity.Warning);

        Assert.False(actual);
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
    public void HasIssueWithSeverity_WhenMatchingSeverityExists_ReturnsTrue(
        IssueSeverity severity)
    {
        IReadOnlyList<IssueInfo> issues =
        [
            CreateIssue(severity)
        ];

        var actual = issues.HasIssueWithSeverity(severity);

        Assert.True(actual);
    }

    [Fact]
    public void HasIssueWithSeverity_WhenMatchingSeverityDoesNotExist_ReturnsFalse()
    {
        IReadOnlyList<IssueInfo> issues =
        [
            CreateIssue(IssueSeverity.Information),
            CreateIssue(IssueSeverity.Warning)
        ];

        var actual = issues.HasIssueWithSeverity(IssueSeverity.Error);

        Assert.False(actual);
    }

    [Fact]
    public void HasWarningsOrErrors_WhenAnyIssueMatches_ReturnsTrue()
    {
        IReadOnlyList<IssueInfo> issues =
        [
            CreateIssue(IssueSeverity.Information),
            CreateIssue(IssueSeverity.Warning)
        ];

        var actual = issues.HasWarningsOrErrors();

        Assert.True(actual);
    }

    [Fact]
    public void HasErrors_WhenAnyIssueMatches_ReturnsTrue()
    {
        IReadOnlyList<IssueInfo> issues =
        [
            CreateIssue(IssueSeverity.Information),
            CreateIssue(IssueSeverity.Error)
        ];

        var actual = issues.HasErrors();

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
}