using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Extensions;
using Afrowave.Toolbox.Essentials.Interfaces;
using Afrowave.Toolbox.Essentials.Issues;
using Afrowave.Toolbox.Essentials.Metadata;

namespace Afrowave.Toolbox.Essentials.Tests.Extensions;

public sealed class ResponseExtensionsTests
{
    [Fact]
    public void HasNoData_WhenResponseIsNull_ThrowsArgumentNullException()
    {
        IResponse? response = null;

        Assert.Throws<ArgumentNullException>(() =>
            response!.HasNoData());
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void HasNoData_ReturnsExpectedResult(
        bool hasData,
        bool expected)
    {
        var response = new TestResponse
        {
            HasData = hasData
        };

        var actual = response.HasNoData();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void IsSuccessWithData_WhenResponseIsNull_ThrowsArgumentNullException()
    {
        IResponse? response = null;

        Assert.Throws<ArgumentNullException>(() =>
            response!.IsSuccessWithData());
    }

    [Theory]
    [InlineData(ResultStatus.Unknown, false, false)]
    [InlineData(ResultStatus.Success, false, false)]
    [InlineData(ResultStatus.Success, true, true)]
    [InlineData(ResultStatus.SuccessWithWarnings, false, false)]
    [InlineData(ResultStatus.SuccessWithWarnings, true, true)]
    [InlineData(ResultStatus.Failed, true, false)]
    [InlineData(ResultStatus.Invalid, true, false)]
    [InlineData(ResultStatus.NotFound, true, false)]
    public void IsSuccessWithData_ReturnsExpectedResult(
        ResultStatus status,
        bool hasData,
        bool expected)
    {
        var response = new TestResponse
        {
            Status = status,
            HasData = hasData
        };

        var actual = response.IsSuccessWithData();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void IsSuccessWithoutData_WhenResponseIsNull_ThrowsArgumentNullException()
    {
        IResponse? response = null;

        Assert.Throws<ArgumentNullException>(() =>
            response!.IsSuccessWithoutData());
    }

    [Theory]
    [InlineData(ResultStatus.Unknown, false, false)]
    [InlineData(ResultStatus.Success, false, true)]
    [InlineData(ResultStatus.Success, true, false)]
    [InlineData(ResultStatus.SuccessWithWarnings, false, true)]
    [InlineData(ResultStatus.SuccessWithWarnings, true, false)]
    [InlineData(ResultStatus.Failed, false, false)]
    [InlineData(ResultStatus.Invalid, false, false)]
    [InlineData(ResultStatus.NotFound, false, false)]
    public void IsSuccessWithoutData_ReturnsExpectedResult(
        ResultStatus status,
        bool hasData,
        bool expected)
    {
        var response = new TestResponse
        {
            Status = status,
            HasData = hasData
        };

        var actual = response.IsSuccessWithoutData();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void IsFailureWithoutData_WhenResponseIsNull_ThrowsArgumentNullException()
    {
        IResponse? response = null;

        Assert.Throws<ArgumentNullException>(() =>
            response!.IsFailureWithoutData());
    }

    [Theory]
    [InlineData(ResultStatus.Unknown, false, false)]
    [InlineData(ResultStatus.Success, false, false)]
    [InlineData(ResultStatus.SuccessWithWarnings, false, false)]
    [InlineData(ResultStatus.Partial, false, false)]
    [InlineData(ResultStatus.Failed, false, true)]
    [InlineData(ResultStatus.Failed, true, false)]
    [InlineData(ResultStatus.Invalid, false, true)]
    [InlineData(ResultStatus.Invalid, true, false)]
    [InlineData(ResultStatus.NotSupported, false, true)]
    [InlineData(ResultStatus.NotSupported, true, false)]
    [InlineData(ResultStatus.Cancelled, false, true)]
    [InlineData(ResultStatus.Cancelled, true, false)]
    [InlineData(ResultStatus.NotFound, false, false)]
    [InlineData(ResultStatus.NotFound, true, false)]
    public void IsFailureWithoutData_ReturnsExpectedResult(
    ResultStatus status,
    bool hasData,
    bool expected)
    {
        var response = new TestResponse
        {
            Status = status,
            HasData = hasData
        };

        var actual = response.IsFailureWithoutData();

        Assert.Equal(expected, actual);
    }

    private sealed class TestResponse : IResponse
    {
        public ResultStatus Status { get; set; } = ResultStatus.Unknown;

        public string Message { get; set; } = string.Empty;

        public IReadOnlyList<IssueInfo> Issues { get; set; } = IssueInfoListFactory.Empty();

        public MetadataBag Metadata { get; set; } = MetadataBagFactory.Empty();

        public bool HasData { get; set; }

        public bool IsSuccess =>
    Status.IsSuccess();

        public bool IsFailure =>
            Status.IsFailure();

        public bool HasWarnings =>
            Status.HasWarnings()
            || Issues.HasWarningOrHigherIssues();
    }
}