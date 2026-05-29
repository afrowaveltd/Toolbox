using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Issues;
using Afrowave.Toolbox.Essentials.Metadata;
using Afrowave.Toolbox.Essentials.Results;

namespace Afrowave.Toolbox.Essentials.Tests.Results;

public sealed class ResultTests
{
    [Fact]
    public void Constructor_CreatesUnknownEmptyResult()
    {
        var result = new Result();

        Assert.Equal(ResultStatus.Unknown, result.Status);
        Assert.Equal(string.Empty, result.Message);
        Assert.NotNull(result.Issues);
        Assert.Empty(result.Issues);
        Assert.NotNull(result.Metadata);
        Assert.True(result.Metadata.IsEmpty);
        Assert.False(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.False(result.HasWarnings);
    }

    [Theory]
    [InlineData(ResultStatus.Unknown, false)]
    [InlineData(ResultStatus.Success, true)]
    [InlineData(ResultStatus.SuccessWithWarnings, true)]
    [InlineData(ResultStatus.Failed, false)]
    [InlineData(ResultStatus.Invalid, false)]
    [InlineData(ResultStatus.NotFound, false)]
    public void IsSuccess_ReturnsExpectedResult(
        ResultStatus status,
        bool expected)
    {
        var result = new Result
        {
            Status = status
        };

        Assert.Equal(expected, result.IsSuccess);
    }

    [Theory]
    [InlineData(ResultStatus.Unknown, false)]
    [InlineData(ResultStatus.Success, false)]
    [InlineData(ResultStatus.SuccessWithWarnings, false)]
    [InlineData(ResultStatus.Failed, true)]
    [InlineData(ResultStatus.Invalid, true)]
    [InlineData(ResultStatus.NotFound, true)]
    public void IsFailure_ReturnsExpectedResult(
        ResultStatus status,
        bool expected)
    {
        var result = new Result
        {
            Status = status
        };

        Assert.Equal(expected, result.IsFailure);
    }

    [Fact]
    public void HasWarnings_WhenStatusIsSuccessWithWarnings_ReturnsTrue()
    {
        var result = new Result
        {
            Status = ResultStatus.SuccessWithWarnings
        };

        Assert.True(result.HasWarnings);
    }

    [Fact]
    public void HasWarnings_WhenIssuesContainWarning_ReturnsTrue()
    {
        var result = new Result
        {
            Status = ResultStatus.Success,
            Issues =
            [
                IssueInfoFactory.Warning(
                    "AFW_WARNING",
                    "Warning message.")
            ]
        };

        Assert.True(result.HasWarnings);
    }

    [Fact]
    public void HasWarnings_WhenIssuesContainOnlyInformation_ReturnsFalse()
    {
        var result = new Result
        {
            Status = ResultStatus.Success,
            Issues =
            [
                IssueInfoFactory.Information(
                    "AFW_INFO",
                    "Information message.")
            ]
        };

        Assert.False(result.HasWarnings);
    }

    [Fact]
    public void Ok_CreatesSuccessResult()
    {
        var result = Result.Ok();

        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.Equal(string.Empty, result.Message);
        Assert.Empty(result.Issues);
        Assert.True(result.Metadata.IsEmpty);
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.False(result.HasWarnings);
    }

    [Fact]
    public void Ok_WithMessage_CreatesSuccessResultWithMessage()
    {
        var result = Result.Ok("Operation completed.");

        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.Equal("Operation completed.", result.Message);
        Assert.Empty(result.Issues);
        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Ok_WithMessage_WhenMessageIsInvalid_ThrowsArgumentException(
        string? message)
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            Result.Ok(message!));
    }

    [Fact]
    public void OkWithWarnings_WhenIssuesIsNull_ThrowsArgumentNullException()
    {
        IReadOnlyList<IssueInfo>? issues = null;

        Assert.Throws<ArgumentNullException>(() =>
            Result.OkWithWarnings(issues!));
    }

    [Fact]
    public void OkWithWarnings_CreatesSuccessWithWarningsResult()
    {
        IReadOnlyList<IssueInfo> issues =
        [
            IssueInfoFactory.Warning(
                "AFW_WARNING",
                "Warning message.")
        ];

        var result = Result.OkWithWarnings(issues);

        Assert.Equal(ResultStatus.SuccessWithWarnings, result.Status);
        Assert.Equal(string.Empty, result.Message);
        Assert.Same(issues, result.Issues);
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.True(result.HasWarnings);
    }

    [Fact]
    public void Fail_WithCodeAndMessage_CreatesFailedResult()
    {
        var result = Result.Fail(
            "AFW_ERROR",
            "Something failed.");

        Assert.Equal(ResultStatus.Failed, result.Status);
        Assert.Equal("Something failed.", result.Message);
        Assert.Single(result.Issues);
        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);

        var issue = result.Issues[0];

        Assert.Equal("AFW_ERROR", issue.Code);
        Assert.Equal("Something failed.", issue.Message);
        Assert.Equal(IssueSeverity.Error, issue.Severity);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Fail_WithCodeAndMessage_WhenCodeIsInvalid_ThrowsArgumentException(
        string? code)
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            Result.Fail(code!, "Something failed."));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Fail_WithCodeAndMessage_WhenMessageIsInvalid_ThrowsArgumentException(
        string? message)
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            Result.Fail("AFW_ERROR", message!));
    }

    [Fact]
    public void Fail_WithIssues_WhenIssuesIsNull_ThrowsArgumentNullException()
    {
        IReadOnlyList<IssueInfo>? issues = null;

        Assert.Throws<ArgumentNullException>(() =>
            Result.Fail(issues!));
    }

    [Fact]
    public void Fail_WithIssues_CreatesFailedResultWithIssues()
    {
        IReadOnlyList<IssueInfo> issues =
        [
            IssueInfoFactory.Error(
                "AFW_ERROR",
                "Something failed.")
        ];

        var result = Result.Fail(issues);

        Assert.Equal(ResultStatus.Failed, result.Status);
        Assert.Equal(string.Empty, result.Message);
        Assert.Same(issues, result.Issues);
        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Invalid_CreatesInvalidResult()
    {
        var result = Result.Invalid(
            "AFW_INVALID",
            "Input is invalid.");

        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Equal("Input is invalid.", result.Message);
        Assert.Single(result.Issues);
        Assert.True(result.IsFailure);

        var issue = result.Issues[0];

        Assert.Equal("AFW_INVALID", issue.Code);
        Assert.Equal("Input is invalid.", issue.Message);
        Assert.Equal(IssueSeverity.Error, issue.Severity);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Invalid_WhenCodeIsInvalid_ThrowsArgumentException(
        string? code)
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            Result.Invalid(code!, "Input is invalid."));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Invalid_WhenMessageIsInvalid_ThrowsArgumentException(
        string? message)
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            Result.Invalid("AFW_INVALID", message!));
    }

    [Fact]
    public void NotFound_CreatesNotFoundResult()
    {
        var result = Result.NotFound(
            "AFW_NOT_FOUND",
            "Item was not found.");

        Assert.Equal(ResultStatus.NotFound, result.Status);
        Assert.Equal("Item was not found.", result.Message);
        Assert.Single(result.Issues);
        Assert.True(result.IsFailure);

        var issue = result.Issues[0];

        Assert.Equal("AFW_NOT_FOUND", issue.Code);
        Assert.Equal("Item was not found.", issue.Message);
        Assert.Equal(IssueSeverity.Error, issue.Severity);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void NotFound_WhenCodeIsInvalid_ThrowsArgumentException(
        string? code)
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            Result.NotFound(code!, "Item was not found."));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void NotFound_WhenMessageIsInvalid_ThrowsArgumentException(
        string? message)
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            Result.NotFound("AFW_NOT_FOUND", message!));
    }

    [Fact]
    public void Result_CanCarryMetadata()
    {
        var metadata = new MetadataBag();
        metadata.Set("source", "unit-test");

        var result = new Result
        {
            Status = ResultStatus.Success,
            Metadata = metadata
        };

        Assert.Same(metadata, result.Metadata);
        Assert.True(result.Metadata.TryGet("source", out var value));
        Assert.Equal("unit-test", value);
    }

    [Fact]
public void WithMessage_WhenResultIsNull_ThrowsArgumentNullException()
{
    Result? result = null;

    Assert.Throws<ArgumentNullException>(() =>
        Result.WithMessage(result!, "New message."));
}

[Theory]
[InlineData(null)]
[InlineData("")]
[InlineData("   ")]
public void WithMessage_WhenMessageIsInvalid_ThrowsArgumentException(
    string? message)
{
    var result = Result.Ok();

    Assert.ThrowsAny<ArgumentException>(() =>
        Result.WithMessage(result, message!));
}

[Fact]
public void WithMessage_CreatesCopyWithSpecifiedMessage()
{
    var metadata = new MetadataBag();
    metadata.Set("source", "unit-test");

    IReadOnlyList<IssueInfo> issues =
    [
        IssueInfoFactory.Warning(
            "AFW_WARNING",
            "Warning message.")
    ];

    var result = new Result
    {
        Status = ResultStatus.SuccessWithWarnings,
        Message = "Original message.",
        Issues = issues,
        Metadata = metadata
    };

    var copy = Result.WithMessage(
        result,
        "New message.");

    Assert.NotSame(result, copy);

    Assert.Equal(result.Status, copy.Status);
    Assert.Equal("New message.", copy.Message);
    Assert.Same(result.Issues, copy.Issues);
    Assert.Same(result.Metadata, copy.Metadata);
}

[Fact]
public void WithMessage_DoesNotModifyOriginalResult()
{
    var result = new Result
    {
        Status = ResultStatus.Success,
        Message = "Original message."
    };

    var copy = Result.WithMessage(
        result,
        "New message.");

    Assert.Equal("Original message.", result.Message);
    Assert.Equal("New message.", copy.Message);
}

[Fact]
public void WithStatus_WhenResultIsNull_ThrowsArgumentNullException()
{
    Result? result = null;

    Assert.Throws<ArgumentNullException>(() =>
        Result.WithStatus(result!, ResultStatus.Failed));
}

[Theory]
[InlineData(ResultStatus.Unknown)]
[InlineData(ResultStatus.Success)]
[InlineData(ResultStatus.SuccessWithWarnings)]
[InlineData(ResultStatus.Failed)]
[InlineData(ResultStatus.Invalid)]
[InlineData(ResultStatus.NotFound)]
public void WithStatus_CreatesCopyWithSpecifiedStatus(
    ResultStatus status)
{
    var metadata = new MetadataBag();
    metadata.Set("source", "unit-test");

    IReadOnlyList<IssueInfo> issues =
    [
        IssueInfoFactory.Warning(
            "AFW_WARNING",
            "Warning message.")
    ];

    var result = new Result
    {
        Status = ResultStatus.Success,
        Message = "Original message.",
        Issues = issues,
        Metadata = metadata
    };

    var copy = Result.WithStatus(result, status);

    Assert.NotSame(result, copy);

    Assert.Equal(status, copy.Status);
    Assert.Equal(result.Message, copy.Message);
    Assert.Same(result.Issues, copy.Issues);
    Assert.Same(result.Metadata, copy.Metadata);
}

[Fact]
public void WithStatus_DoesNotModifyOriginalResult()
{
    var result = new Result
    {
        Status = ResultStatus.Success,
        Message = "Original message."
    };

    var copy = Result.WithStatus(
        result,
        ResultStatus.Failed);

    Assert.Equal(ResultStatus.Success, result.Status);
    Assert.Equal(ResultStatus.Failed, copy.Status);
}

[Fact]
public void WithIssues_WhenResultIsNull_ThrowsArgumentNullException()
{
    Result? result = null;

    IReadOnlyList<IssueInfo> issues = [];

    Assert.Throws<ArgumentNullException>(() =>
        Result.WithIssues(result!, issues));
}

[Fact]
public void WithIssues_WhenIssuesIsNull_ThrowsArgumentNullException()
{
    var result = Result.Ok();

    IReadOnlyList<IssueInfo>? issues = null;

    Assert.Throws<ArgumentNullException>(() =>
        Result.WithIssues(result, issues!));
}

[Fact]
public void WithIssues_CreatesCopyWithSpecifiedIssues()
{
    var metadata = new MetadataBag();
    metadata.Set("source", "unit-test");

    var originalIssues = IssueInfoListFactory.Information(
        "AFW_INFO",
        "Information message.");

    var newIssues = IssueInfoListFactory.Error(
        "AFW_ERROR",
        "Error message.");

    var result = new Result
    {
        Status = ResultStatus.Success,
        Message = "Original message.",
        Issues = originalIssues,
        Metadata = metadata
    };

    var copy = Result.WithIssues(result, newIssues);

    Assert.NotSame(result, copy);

    Assert.Equal(result.Status, copy.Status);
    Assert.Equal(result.Message, copy.Message);
    Assert.Same(newIssues, copy.Issues);
    Assert.Same(result.Metadata, copy.Metadata);
}

[Fact]
public void WithIssues_DoesNotModifyOriginalResult()
{
    var originalIssues = IssueInfoListFactory.Information(
        "AFW_INFO",
        "Information message.");

    var newIssues = IssueInfoListFactory.Error(
        "AFW_ERROR",
        "Error message.");

    var result = new Result
    {
        Status = ResultStatus.Success,
        Issues = originalIssues
    };

    var copy = Result.WithIssues(result, newIssues);

    Assert.Same(originalIssues, result.Issues);
    Assert.Same(newIssues, copy.Issues);
}

[Fact]
public void WithMetadata_WhenResultIsNull_ThrowsArgumentNullException()
{
    Result? result = null;
    var metadata = new MetadataBag();

    Assert.Throws<ArgumentNullException>(() =>
        Result.WithMetadata(result!, metadata));
}

[Fact]
public void WithMetadata_WhenMetadataIsNull_ThrowsArgumentNullException()
{
    var result = Result.Ok();

    MetadataBag? metadata = null;

    Assert.Throws<ArgumentNullException>(() =>
        Result.WithMetadata(result, metadata!));
}

[Fact]
public void WithMetadata_CreatesCopyWithSpecifiedMetadata()
{
    var originalMetadata = new MetadataBag();
    originalMetadata.Set("original", "yes");

    var newMetadata = new MetadataBag();
    newMetadata.Set("new", "yes");

    IReadOnlyList<IssueInfo> issues =
    [
        IssueInfoFactory.Warning(
            "AFW_WARNING",
            "Warning message.")
    ];

    var result = new Result
    {
        Status = ResultStatus.SuccessWithWarnings,
        Message = "Original message.",
        Issues = issues,
        Metadata = originalMetadata
    };

    var copy = Result.WithMetadata(result, newMetadata);

    Assert.NotSame(result, copy);

    Assert.Equal(result.Status, copy.Status);
    Assert.Equal(result.Message, copy.Message);
    Assert.Same(result.Issues, copy.Issues);
    Assert.Same(newMetadata, copy.Metadata);

    Assert.True(copy.Metadata.TryGet("new", out var value));
    Assert.Equal("yes", value);
}

[Fact]
public void WithMetadata_DoesNotModifyOriginalResult()
{
    var originalMetadata = new MetadataBag();
    originalMetadata.Set("original", "yes");

    var newMetadata = new MetadataBag();
    newMetadata.Set("new", "yes");

    var result = new Result
    {
        Status = ResultStatus.Success,
        Metadata = originalMetadata
    };

    var copy = Result.WithMetadata(result, newMetadata);

    Assert.Same(originalMetadata, result.Metadata);
    Assert.Same(newMetadata, copy.Metadata);

    Assert.True(result.Metadata.TryGet("original", out var originalValue));
    Assert.False(result.Metadata.TryGet("new", out _));
    Assert.Equal("yes", originalValue);

    Assert.True(copy.Metadata.TryGet("new", out var newValue));
    Assert.False(copy.Metadata.TryGet("original", out _));
    Assert.Equal("yes", newValue);
}
}