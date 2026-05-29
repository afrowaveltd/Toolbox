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

    private sealed class TestResult : IResult
    {
        public ResultStatus Status { get; set; } = ResultStatus.Unknown;

        public string Message { get; set; } = string.Empty;

        public IReadOnlyList<IssueInfo> Issues { get; set; } = IssueInfoListFactory.Empty();

        public MetadataBag Metadata { get; set; } = MetadataBagFactory.Empty();

        public bool IsSuccess =>
            Status is ResultStatus.Success or ResultStatus.SuccessWithWarnings;

        public bool IsFailure =>
            Status is ResultStatus.Failed or ResultStatus.Invalid or ResultStatus.NotFound;

        public bool HasWarnings =>
            Status == ResultStatus.SuccessWithWarnings
            || Issues.HasWarningsOrErrors();
    }
}