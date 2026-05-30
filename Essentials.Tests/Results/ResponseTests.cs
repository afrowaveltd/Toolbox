using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Issues;
using Afrowave.Toolbox.Essentials.Metadata;
using Afrowave.Toolbox.Essentials.Results;

namespace Afrowave.Toolbox.Essentials.Tests.Results;

public sealed class ResponseTests
{
    [Fact]
    public void Constructor_CreatesUnknownEmptyResponse()
    {
        var response = new Response();

        Assert.Equal(ResultStatus.Unknown, response.Status);
        Assert.Equal(string.Empty, response.Message);
        Assert.NotNull(response.Issues);
        Assert.Empty(response.Issues);
        Assert.NotNull(response.Metadata);
        Assert.True(response.Metadata.IsEmpty);
        Assert.False(response.HasData);
        Assert.False(response.IsSuccess);
        Assert.False(response.IsFailure);
        Assert.False(response.HasWarnings);
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
        var response = new Response
        {
            Status = status
        };

        Assert.Equal(expected, response.IsSuccess);
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
        var response = new Response
        {
            Status = status
        };

        Assert.Equal(expected, response.IsFailure);
    }

    [Fact]
    public void HasWarnings_WhenStatusIsSuccessWithWarnings_ReturnsTrue()
    {
        var response = new Response
        {
            Status = ResultStatus.SuccessWithWarnings
        };

        Assert.True(response.HasWarnings);
    }

    [Fact]
    public void HasWarnings_WhenIssuesContainWarning_ReturnsTrue()
    {
        var response = new Response
        {
            Status = ResultStatus.Success,
            Issues =
            [
                IssueInfoFactory.Warning(
                    "AFW_WARNING",
                    "Warning message.")
            ]
        };

        Assert.True(response.HasWarnings);
    }

    [Fact]
    public void HasWarnings_WhenIssuesContainOnlyInformation_ReturnsFalse()
    {
        var response = new Response
        {
            Status = ResultStatus.Success,
            Issues =
            [
                IssueInfoFactory.Information(
                    "AFW_INFO",
                    "Information message.")
            ]
        };

        Assert.False(response.HasWarnings);
    }

    [Fact]
    public void Ok_CreatesSuccessResponse()
    {
        var response = Response.Ok();

        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.Equal(string.Empty, response.Message);
        Assert.Empty(response.Issues);
        Assert.True(response.Metadata.IsEmpty);
        Assert.False(response.HasData);
        Assert.True(response.IsSuccess);
        Assert.False(response.IsFailure);
        Assert.False(response.HasWarnings);
    }

    [Fact]
    public void Ok_WithMessage_CreatesSuccessResponseWithMessage()
    {
        var response = Response.Ok("Operation completed.");

        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.Equal("Operation completed.", response.Message);
        Assert.Empty(response.Issues);
        Assert.False(response.HasData);
        Assert.True(response.IsSuccess);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Ok_WithMessage_WhenMessageIsInvalid_ThrowsArgumentException(
        string? message)
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            Response.Ok(message!));
    }

    [Fact]
    public void OkWithWarnings_WhenIssuesIsNull_ThrowsArgumentNullException()
    {
        IReadOnlyList<IssueInfo>? issues = null;

        Assert.Throws<ArgumentNullException>(() =>
            Response.OkWithWarnings(issues!));
    }

    [Fact]
    public void OkWithWarnings_CreatesSuccessWithWarningsResponse()
    {
        IReadOnlyList<IssueInfo> issues =
        [
            IssueInfoFactory.Warning(
                "AFW_WARNING",
                "Warning message.")
        ];

        var response = Response.OkWithWarnings(issues);

        Assert.Equal(ResultStatus.SuccessWithWarnings, response.Status);
        Assert.Equal(string.Empty, response.Message);
        Assert.Same(issues, response.Issues);
        Assert.False(response.HasData);
        Assert.True(response.IsSuccess);
        Assert.False(response.IsFailure);
        Assert.True(response.HasWarnings);
    }

    [Fact]
    public void Fail_WithCodeAndMessage_CreatesFailedResponse()
    {
        var response = Response.Fail(
            "AFW_ERROR",
            "Something failed.");

        Assert.Equal(ResultStatus.Failed, response.Status);
        Assert.Equal("Something failed.", response.Message);
        Assert.Single(response.Issues);
        Assert.False(response.HasData);
        Assert.True(response.IsFailure);
        Assert.False(response.IsSuccess);

        var issue = response.Issues[0];

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
            Response.Fail(code!, "Something failed."));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Fail_WithCodeAndMessage_WhenMessageIsInvalid_ThrowsArgumentException(
        string? message)
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            Response.Fail("AFW_ERROR", message!));
    }

    [Fact]
    public void Fail_WithIssues_WhenIssuesIsNull_ThrowsArgumentNullException()
    {
        IReadOnlyList<IssueInfo>? issues = null;

        Assert.Throws<ArgumentNullException>(() =>
            Response.Fail(issues!));
    }

    [Fact]
    public void Fail_WithIssues_CreatesFailedResponseWithIssues()
    {
        IReadOnlyList<IssueInfo> issues =
        [
            IssueInfoFactory.Error(
                "AFW_ERROR",
                "Something failed.")
        ];

        var response = Response.Fail(issues);

        Assert.Equal(ResultStatus.Failed, response.Status);
        Assert.Equal(string.Empty, response.Message);
        Assert.Same(issues, response.Issues);
        Assert.False(response.HasData);
        Assert.True(response.IsFailure);
        Assert.False(response.IsSuccess);
    }

    [Fact]
    public void Invalid_CreatesInvalidResponse()
    {
        var response = Response.Invalid(
            "AFW_INVALID",
            "Input is invalid.");

        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Equal("Input is invalid.", response.Message);
        Assert.Single(response.Issues);
        Assert.False(response.HasData);
        Assert.True(response.IsFailure);

        var issue = response.Issues[0];

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
            Response.Invalid(code!, "Input is invalid."));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Invalid_WhenMessageIsInvalid_ThrowsArgumentException(
        string? message)
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            Response.Invalid("AFW_INVALID", message!));
    }

    [Fact]
    public void NotFound_CreatesNotFoundResponse()
    {
        var response = Response.NotFound(
            "AFW_NOT_FOUND",
            "Item was not found.");

        Assert.Equal(ResultStatus.NotFound, response.Status);
        Assert.Equal("Item was not found.", response.Message);
        Assert.Single(response.Issues);
        Assert.False(response.HasData);
        Assert.True(response.IsFailure);

        var issue = response.Issues[0];

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
            Response.NotFound(code!, "Item was not found."));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void NotFound_WhenMessageIsInvalid_ThrowsArgumentException(
        string? message)
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            Response.NotFound("AFW_NOT_FOUND", message!));
    }

    [Fact]
    public void Response_CanCarryMetadata()
    {
        var metadata = new MetadataBag();
        metadata.Set("source", "unit-test");

        var response = new Response
        {
            Status = ResultStatus.Success,
            Metadata = metadata
        };

        Assert.Same(metadata, response.Metadata);
        Assert.True(response.Metadata.TryGet("source", out var value));
        Assert.Equal("unit-test", value);
    }
    [Fact]
    public void WithMessage_WhenResponseIsNull_ThrowsArgumentNullException()
    {
        Response? response = null;

        Assert.Throws<ArgumentNullException>(() =>
            Response.WithMessage(response!, "New message."));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void WithMessage_WhenMessageIsInvalid_ThrowsArgumentException(
        string? message)
    {
        var response = Response.Ok();

        Assert.ThrowsAny<ArgumentException>(() =>
            Response.WithMessage(response, message!));
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

        var response = new Response
        {
            Status = ResultStatus.SuccessWithWarnings,
            Message = "Original message.",
            Issues = issues,
            Metadata = metadata
        };

        var copy = Response.WithMessage(
            response,
            "New message.");

        Assert.NotSame(response, copy);

        Assert.Equal(response.Status, copy.Status);
        Assert.Equal("New message.", copy.Message);
        Assert.Same(response.Issues, copy.Issues);
        Assert.Same(response.Metadata, copy.Metadata);
    }

    [Fact]
    public void WithMessage_DoesNotModifyOriginalResponse()
    {
        var response = new Response
        {
            Status = ResultStatus.Success,
            Message = "Original message."
        };

        var copy = Response.WithMessage(
            response,
            "New message.");

        Assert.Equal("Original message.", response.Message);
        Assert.Equal("New message.", copy.Message);
    }

    [Fact]
    public void WithStatus_WhenResponseIsNull_ThrowsArgumentNullException()
    {
        Response? response = null;

        Assert.Throws<ArgumentNullException>(() =>
            Response.WithStatus(response!, ResultStatus.Failed));
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

        var response = new Response
        {
            Status = ResultStatus.Success,
            Message = "Original message.",
            Issues = issues,
            Metadata = metadata
        };

        var copy = Response.WithStatus(response, status);

        Assert.NotSame(response, copy);

        Assert.Equal(status, copy.Status);
        Assert.Equal(response.Message, copy.Message);
        Assert.Same(response.Issues, copy.Issues);
        Assert.Same(response.Metadata, copy.Metadata);
    }

    [Fact]
    public void WithStatus_DoesNotModifyOriginalResponse()
    {
        var response = new Response
        {
            Status = ResultStatus.Success,
            Message = "Original message."
        };

        var copy = Response.WithStatus(
            response,
            ResultStatus.Failed);

        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.Equal(ResultStatus.Failed, copy.Status);
    }

    [Fact]
    public void WithIssues_WhenResponseIsNull_ThrowsArgumentNullException()
    {
        Response? response = null;

        IReadOnlyList<IssueInfo> issues = [];

        Assert.Throws<ArgumentNullException>(() =>
            Response.WithIssues(response!, issues));
    }

    [Fact]
    public void WithIssues_WhenIssuesIsNull_ThrowsArgumentNullException()
    {
        var response = Response.Ok();

        IReadOnlyList<IssueInfo>? issues = null;

        Assert.Throws<ArgumentNullException>(() =>
            Response.WithIssues(response, issues!));
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

        var response = new Response
        {
            Status = ResultStatus.Success,
            Message = "Original message.",
            Issues = originalIssues,
            Metadata = metadata
        };

        var copy = Response.WithIssues(response, newIssues);

        Assert.NotSame(response, copy);

        Assert.Equal(response.Status, copy.Status);
        Assert.Equal(response.Message, copy.Message);
        Assert.Same(newIssues, copy.Issues);
        Assert.Same(response.Metadata, copy.Metadata);
    }

    [Fact]
    public void WithIssues_DoesNotModifyOriginalResponse()
    {
        var originalIssues = IssueInfoListFactory.Information(
            "AFW_INFO",
            "Information message.");

        var newIssues = IssueInfoListFactory.Error(
            "AFW_ERROR",
            "Error message.");

        var response = new Response
        {
            Status = ResultStatus.Success,
            Issues = originalIssues
        };

        var copy = Response.WithIssues(response, newIssues);

        Assert.Same(originalIssues, response.Issues);
        Assert.Same(newIssues, copy.Issues);
    }

    [Fact]
    public void WithMetadata_WhenResponseIsNull_ThrowsArgumentNullException()
    {
        Response? response = null;
        var metadata = new MetadataBag();

        Assert.Throws<ArgumentNullException>(() =>
            Response.WithMetadata(response!, metadata));
    }

    [Fact]
    public void WithMetadata_WhenMetadataIsNull_ThrowsArgumentNullException()
    {
        var response = Response.Ok();

        MetadataBag? metadata = null;

        Assert.Throws<ArgumentNullException>(() =>
            Response.WithMetadata(response, metadata!));
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

        var response = new Response
        {
            Status = ResultStatus.SuccessWithWarnings,
            Message = "Original message.",
            Issues = issues,
            Metadata = originalMetadata
        };

        var copy = Response.WithMetadata(response, newMetadata);

        Assert.NotSame(response, copy);

        Assert.Equal(response.Status, copy.Status);
        Assert.Equal(response.Message, copy.Message);
        Assert.Same(response.Issues, copy.Issues);
        Assert.Same(newMetadata, copy.Metadata);

        Assert.True(copy.Metadata.TryGet("new", out var value));
        Assert.Equal("yes", value);
    }

    [Fact]
    public void WithMetadata_DoesNotModifyOriginalResponse()
    {
        var originalMetadata = new MetadataBag();
        originalMetadata.Set("original", "yes");

        var newMetadata = new MetadataBag();
        newMetadata.Set("new", "yes");

        var response = new Response
        {
            Status = ResultStatus.Success,
            Metadata = originalMetadata
        };

        var copy = Response.WithMetadata(response, newMetadata);

        Assert.Same(originalMetadata, response.Metadata);
        Assert.Same(newMetadata, copy.Metadata);

        Assert.True(response.Metadata.TryGet("original", out var originalValue));
        Assert.False(response.Metadata.TryGet("new", out _));
        Assert.Equal("yes", originalValue);

        Assert.True(copy.Metadata.TryGet("new", out var newValue));
        Assert.False(copy.Metadata.TryGet("original", out _));
        Assert.Equal("yes", newValue);
    }
    [Fact]
    public void AddIssue_WhenResponseIsNull_ThrowsArgumentNullException()
    {
        Response? response = null;

        var issue = IssueInfoFactory.Warning(
            "AFW_WARNING",
            "Warning message.");

        Assert.Throws<ArgumentNullException>(() =>
            Response.AddIssue(response!, issue));
    }

    [Fact]
    public void AddIssue_WhenIssueIsNull_ThrowsArgumentNullException()
    {
        var response = Response.Ok();

        IssueInfo? issue = null;

        Assert.Throws<ArgumentNullException>(() =>
            Response.AddIssue(response, issue!));
    }

    [Fact]
    public void AddIssue_AppendsIssue()
    {
        var first = IssueInfoFactory.Information(
            "AFW_INFO",
            "Information message.");

        var second = IssueInfoFactory.Warning(
            "AFW_WARNING",
            "Warning message.");

        var response = new Response
        {
            Status = ResultStatus.Success,
            Message = "Original message.",
            Issues =
            [
                first
            ]
        };

        var copy = Response.AddIssue(response, second);

        Assert.NotSame(response, copy);

        Assert.Equal(response.Status, copy.Status);
        Assert.Equal(response.Message, copy.Message);
        Assert.Equal(2, copy.Issues.Count);
        Assert.Same(first, copy.Issues[0]);
        Assert.Same(second, copy.Issues[1]);
        Assert.Same(response.Metadata, copy.Metadata);
    }

    [Fact]
    public void AddIssue_DoesNotModifyOriginalResponse()
    {
        var first = IssueInfoFactory.Information(
            "AFW_INFO",
            "Information message.");

        var second = IssueInfoFactory.Warning(
            "AFW_WARNING",
            "Warning message.");

        var response = new Response
        {
            Status = ResultStatus.Success,
            Issues =
            [
                first
            ]
        };

        var copy = Response.AddIssue(response, second);

        Assert.Single(response.Issues);
        Assert.Same(first, response.Issues[0]);

        Assert.Equal(2, copy.Issues.Count);
        Assert.Same(second, copy.Issues[1]);
    }

    [Fact]
    public void AddIssue_WhenOriginalIssuesAreEmpty_CreatesResponseWithOneIssue()
    {
        var issue = IssueInfoFactory.Warning(
            "AFW_WARNING",
            "Warning message.");

        var response = Response.Ok();

        var copy = Response.AddIssue(response, issue);

        Assert.Single(copy.Issues);
        Assert.Same(issue, copy.Issues[0]);
    }

    [Fact]
    public void AddIssue_PreservesHasDataAsFalse()
    {
        var issue = IssueInfoFactory.Warning(
            "AFW_WARNING",
            "Warning message.");

        var response = Response.Ok();

        var copy = Response.AddIssue(response, issue);

        Assert.False(copy.HasData);
    }

    [Fact]
    public void AddIssues_WhenResponseIsNull_ThrowsArgumentNullException()
    {
        Response? response = null;

        IReadOnlyList<IssueInfo> issues =
        [
            IssueInfoFactory.Warning(
            "AFW_WARNING",
            "Warning message.")
        ];

        Assert.Throws<ArgumentNullException>(() =>
            Response.AddIssues(response!, issues));
    }

    [Fact]
    public void AddIssues_WhenIssuesIsNull_ThrowsArgumentNullException()
    {
        var response = Response.Ok();

        IEnumerable<IssueInfo>? issues = null;

        Assert.Throws<ArgumentNullException>(() =>
            Response.AddIssues(response, issues!));
    }

    [Fact]
    public void AddIssues_AppendsIssues()
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

        var response = new Response
        {
            Status = ResultStatus.Success,
            Message = "Original message.",
            Issues =
            [
                first
            ]
        };

        IReadOnlyList<IssueInfo> additionalIssues =
        [
            second,
        third
        ];

        var copy = Response.AddIssues(response, additionalIssues);

        Assert.NotSame(response, copy);

        Assert.Equal(response.Status, copy.Status);
        Assert.Equal(response.Message, copy.Message);
        Assert.Equal(3, copy.Issues.Count);
        Assert.Same(first, copy.Issues[0]);
        Assert.Same(second, copy.Issues[1]);
        Assert.Same(third, copy.Issues[2]);
        Assert.Same(response.Metadata, copy.Metadata);
    }

    [Fact]
    public void AddIssues_DoesNotModifyOriginalResponse()
    {
        var first = IssueInfoFactory.Information(
            "AFW_INFO",
            "Information message.");

        var second = IssueInfoFactory.Warning(
            "AFW_WARNING",
            "Warning message.");

        var response = new Response
        {
            Status = ResultStatus.Success,
            Issues =
            [
                first
            ]
        };

        IReadOnlyList<IssueInfo> additionalIssues =
        [
            second
        ];

        var copy = Response.AddIssues(response, additionalIssues);

        Assert.Single(response.Issues);
        Assert.Same(first, response.Issues[0]);

        Assert.Equal(2, copy.Issues.Count);
        Assert.Same(second, copy.Issues[1]);
    }

    [Fact]
    public void AddIssues_WhenAdditionalIssuesAreEmpty_ReturnsCopyWithOriginalIssues()
    {
        var first = IssueInfoFactory.Information(
            "AFW_INFO",
            "Information message.");

        var response = new Response
        {
            Status = ResultStatus.Success,
            Issues =
            [
                first
            ]
        };

        IReadOnlyList<IssueInfo> additionalIssues = [];

        var copy = Response.AddIssues(response, additionalIssues);

        Assert.NotSame(response, copy);
        Assert.Single(copy.Issues);
        Assert.Same(first, copy.Issues[0]);
    }

    [Fact]
    public void AddIssues_ReturnsSnapshotOfAdditionalIssues()
    {
        var first = IssueInfoFactory.Information(
            "AFW_INFO",
            "Information message.");

        var second = IssueInfoFactory.Warning(
            "AFW_WARNING",
            "Warning message.");

        var response = new Response
        {
            Status = ResultStatus.Success,
            Issues =
            [
                first
            ]
        };

        var additionalIssues = new List<IssueInfo>
    {
        second
    };

        var copy = Response.AddIssues(response, additionalIssues);

        additionalIssues.Clear();

        Assert.Equal(2, copy.Issues.Count);
        Assert.Same(first, copy.Issues[0]);
        Assert.Same(second, copy.Issues[1]);
    }

    [Fact]
    public void AddIssues_PreservesHasDataAsFalse()
    {
        var response = Response.Ok();

        IReadOnlyList<IssueInfo> additionalIssues =
        [
            IssueInfoFactory.Warning(
            "AFW_WARNING",
            "Warning message.")
        ];

        var copy = Response.AddIssues(response, additionalIssues);

        Assert.False(copy.HasData);
    }

}