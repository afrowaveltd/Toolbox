using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Extensions;
using Afrowave.Toolbox.Essentials.Interfaces;
using Afrowave.Toolbox.Essentials.Issues;
using Afrowave.Toolbox.Essentials.Metadata;

namespace Afrowave.Toolbox.Essentials.Tests.Extensions;

public sealed class ResponseOfTExtensionsTests
{
    [Fact]
    public void GetDataOrThrow_WhenResponseIsNull_ThrowsArgumentNullException()
    {
        IResponse<string>? response = null;

        Assert.Throws<ArgumentNullException>(() =>
            response!.GetDataOrThrow());
    }

    [Fact]
    public void GetDataOrThrow_WhenResponseHasNoData_ThrowsInvalidOperationException()
    {
        var response = new TestResponse<string>
        {
            Data = null,
            HasData = false
        };

        Assert.Throws<InvalidOperationException>(() =>
            response.GetDataOrThrow());
    }

    [Fact]
    public void GetDataOrThrow_WhenHasDataIsTrueButDataIsNull_ThrowsInvalidOperationException()
    {
        var response = new TestResponse<string>
        {
            Data = null,
            HasData = true
        };

        Assert.Throws<InvalidOperationException>(() =>
            response.GetDataOrThrow());
    }

    [Fact]
    public void GetDataOrThrow_WhenResponseHasData_ReturnsData()
    {
        var response = new TestResponse<string>
        {
            Data = "payload",
            HasData = true
        };

        var actual = response.GetDataOrThrow();

        Assert.Equal("payload", actual);
    }

    [Fact]
    public void GetDataOrThrow_WithValueTypeDefaultValue_ReturnsData()
    {
        var response = new TestResponse<int>
        {
            Data = 0,
            HasData = true
        };

        var actual = response.GetDataOrThrow();

        Assert.Equal(0, actual);
    }

    [Fact]
    public void GetDataOrDefault_WhenResponseIsNull_ThrowsArgumentNullException()
    {
        IResponse<string>? response = null;

        Assert.Throws<ArgumentNullException>(() =>
            response!.GetDataOrDefault("fallback"));
    }

    [Fact]
    public void GetDataOrDefault_WhenResponseHasNoData_ReturnsFallback()
    {
        var response = new TestResponse<string>
        {
            Data = null,
            HasData = false
        };

        var actual = response.GetDataOrDefault("fallback");

        Assert.Equal("fallback", actual);
    }

    [Fact]
    public void GetDataOrDefault_WhenResponseHasData_ReturnsData()
    {
        var response = new TestResponse<string>
        {
            Data = "payload",
            HasData = true
        };

        var actual = response.GetDataOrDefault("fallback");

        Assert.Equal("payload", actual);
    }

    [Fact]
    public void GetDataOrDefault_WhenHasDataIsTrueButDataIsNull_ReturnsNull()
    {
        var response = new TestResponse<string>
        {
            Data = null,
            HasData = true
        };

        var actual = response.GetDataOrDefault("fallback");

        Assert.Null(actual);
    }

    [Fact]
    public void GetDataOrDefault_WithValueTypeDefaultValue_ReturnsData()
    {
        var response = new TestResponse<int>
        {
            Data = 0,
            HasData = true
        };

        var actual = response.GetDataOrDefault(42);

        Assert.Equal(0, actual);
    }

    [Fact]
    public void IsSuccessWithData_WhenResponseIsNull_ThrowsArgumentNullException()
    {
        IResponse<string>? response = null;

        Assert.Throws<ArgumentNullException>(() =>
            response!.IsSuccessWithData(text => text.Length > 0));
    }

    [Fact]
    public void IsSuccessWithData_WhenPredicateIsNull_ThrowsArgumentNullException()
    {
        var response = new TestResponse<string>
        {
            Status = ResultStatus.Success,
            Data = "payload",
            HasData = true
        };

        Func<string, bool>? predicate = null;

        Assert.Throws<ArgumentNullException>(() =>
            response.IsSuccessWithData(predicate!));
    }

    [Theory]
    [InlineData(ResultStatus.Unknown, true, "payload", false)]
    [InlineData(ResultStatus.Success, true, "payload", true)]
    [InlineData(ResultStatus.SuccessWithWarnings, true, "payload", true)]
    [InlineData(ResultStatus.Partial, true, "payload", false)]
    [InlineData(ResultStatus.Failed, true, "payload", false)]
    [InlineData(ResultStatus.Invalid, true, "payload", false)]
    [InlineData(ResultStatus.NotSupported, true, "payload", false)]
    [InlineData(ResultStatus.Cancelled, true, "payload", false)]
    [InlineData(ResultStatus.NotFound, true, "payload", false)]
    [InlineData(ResultStatus.Success, false, "payload", false)]
    [InlineData(ResultStatus.Success, true, null, false)]
    [InlineData(ResultStatus.Success, true, "", false)]
    public void IsSuccessWithData_ReturnsExpectedResult(
     ResultStatus status,
     bool hasData,
     string? data,
     bool expected)
    {
        var response = new TestResponse<string>
        {
            Status = status,
            Data = data,
            HasData = hasData
        };

        var actual = response.IsSuccessWithData(text => text.Length > 0);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void IsSuccessWithData_WhenPredicateReturnsFalse_ReturnsFalse()
    {
        var response = new TestResponse<string>
        {
            Status = ResultStatus.Success,
            Data = "abc",
            HasData = true
        };

        var actual = response.IsSuccessWithData(text => text.Length > 10);

        Assert.False(actual);
    }

    [Fact]
    public void HasDataMatching_WhenResponseIsNull_ThrowsArgumentNullException()
    {
        IResponse<string>? response = null;

        Assert.Throws<ArgumentNullException>(() =>
            response!.HasDataMatching(text => text.Length > 0));
    }

    [Fact]
    public void HasDataMatching_WhenPredicateIsNull_ThrowsArgumentNullException()
    {
        var response = new TestResponse<string>
        {
            Data = "payload",
            HasData = true
        };

        Func<string, bool>? predicate = null;

        Assert.Throws<ArgumentNullException>(() =>
            response.HasDataMatching(predicate!));
    }

    [Theory]
    [InlineData(true, "payload", true)]
    [InlineData(false, "payload", false)]
    [InlineData(true, null, false)]
    [InlineData(true, "", false)]
    public void HasDataMatching_ReturnsExpectedResult(
        bool hasData,
        string? data,
        bool expected)
    {
        var response = new TestResponse<string>
        {
            Data = data,
            HasData = hasData
        };

        var actual = response.HasDataMatching(text => text.Length > 0);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void HasDataMatching_WhenPredicateReturnsFalse_ReturnsFalse()
    {
        var response = new TestResponse<string>
        {
            Data = "abc",
            HasData = true
        };

        var actual = response.HasDataMatching(text => text.Length > 10);

        Assert.False(actual);
    }

    private sealed class TestResponse<T> : IResponse<T>
    {
        public ResultStatus Status { get; set; } = ResultStatus.Unknown;

        public string Message { get; set; } = string.Empty;

        public IReadOnlyList<IssueInfo> Issues { get; set; } = IssueInfoListFactory.Empty();

        public MetadataBag Metadata { get; set; } = MetadataBagFactory.Empty();

        public bool HasData { get; set; }

        public T? Data { get; set; }

        public bool IsSuccess =>
    Status.IsSuccess();

        public bool IsFailure =>
            Status.IsFailure();

        public bool HasWarnings =>
            Status.HasWarnings()
            || Issues.HasWarningOrHigherIssues();
    }
}