using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Extensions;
using Afrowave.Toolbox.Essentials.Interfaces;
using Afrowave.Toolbox.Essentials.Issues;
using Afrowave.Toolbox.Essentials.Metadata;
using Afrowave.Toolbox.Essentials.Results;

namespace Afrowave.Toolbox.Essentials.Tests.Extensions;

public sealed class ResultConversionExtensionsTests
{
    [Fact]
    public void ToResponse_WhenResultIsNull_ThrowsArgumentNullException()
    {
        IResult? result = null;

        Assert.Throws<ArgumentNullException>(() =>
            result!.ToResponse());
    }

    [Fact]
    public void ToResponse_CreatesResponseWithCopiedValues()
    {
        var metadata = new MetadataBag();
        metadata.Set("source", "unit-test");

        IReadOnlyList<IssueInfo> issues =
        [
            IssueInfoFactory.Warning(
                "AFW_WARNING",
                "Warning message.")
        ];

        var result = new TestResult
        {
            Status = ResultStatus.SuccessWithWarnings,
            Message = "Operation completed with warnings.",
            Issues = issues,
            Metadata = metadata
        };

        var response = result.ToResponse();

        Assert.Equal(result.Status, response.Status);
        Assert.Equal(result.Message, response.Message);
        Assert.Same(result.Issues, response.Issues);
        Assert.Same(result.Metadata, response.Metadata);
        Assert.False(response.HasData);
        Assert.True(response.IsSuccess);
        Assert.True(response.HasWarnings);
    }

    [Fact]
    public void ToTypedResponse_WhenResultIsNull_ThrowsArgumentNullException()
    {
        IResult? result = null;

        Assert.Throws<ArgumentNullException>(() =>
            result!.ToResponse("payload"));
    }

    [Fact]
    public void ToTypedResponse_CreatesResponseWithCopiedValuesAndData()
    {
        var metadata = new MetadataBag();
        metadata.Set("source", "unit-test");

        IReadOnlyList<IssueInfo> issues =
        [
            IssueInfoFactory.Warning(
                "AFW_WARNING",
                "Warning message.")
        ];

        var result = new TestResult
        {
            Status = ResultStatus.SuccessWithWarnings,
            Message = "Operation completed with warnings.",
            Issues = issues,
            Metadata = metadata
        };

        var response = result.ToResponse("payload");

        Assert.Equal(result.Status, response.Status);
        Assert.Equal(result.Message, response.Message);
        Assert.Equal("payload", response.Data);
        Assert.True(response.HasData);
        Assert.Same(result.Issues, response.Issues);
        Assert.Same(result.Metadata, response.Metadata);
        Assert.True(response.IsSuccess);
        Assert.True(response.HasWarnings);
    }

    [Fact]
    public void ToTypedResponse_WithNullData_CreatesResponseWithoutData()
    {
        var result = new TestResult
        {
            Status = ResultStatus.Success,
            Message = "Operation completed."
        };

        var response = result.ToResponse<string>(null);

        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.Equal("Operation completed.", response.Message);
        Assert.Null(response.Data);
        Assert.False(response.HasData);
    }

    [Fact]
    public void ToTypedResponse_WithValueTypeDefaultData_HasDataReturnsTrue()
    {
        var result = new TestResult
        {
            Status = ResultStatus.Success
        };

        var response = result.ToResponse(0);

        Assert.Equal(0, response.Data);
        Assert.True(response.HasData);
    }

    [Fact]
    public void ToResult_WhenResponseIsNull_ThrowsArgumentNullException()
    {
        IResponse? response = null;

        Assert.Throws<ArgumentNullException>(() =>
            response!.ToResult());
    }

    [Fact]
    public void ToResult_FromNonGenericResponse_CreatesResultWithCopiedValues()
    {
        var metadata = new MetadataBag();
        metadata.Set("source", "unit-test");

        IReadOnlyList<IssueInfo> issues =
        [
            IssueInfoFactory.Error(
                "AFW_ERROR",
                "Error message.")
        ];

        var response = new TestResponse
        {
            Status = ResultStatus.Failed,
            Message = "Operation failed.",
            Issues = issues,
            Metadata = metadata,
            HasData = false
        };

        var result = response.ToResult();

        Assert.Equal(response.Status, result.Status);
        Assert.Equal(response.Message, result.Message);
        Assert.Same(response.Issues, result.Issues);
        Assert.Same(response.Metadata, result.Metadata);
        Assert.True(result.IsFailure);
        Assert.True(result.HasErrors());
    }

    [Fact]
    public void ToResult_FromTypedResponse_DropsDataAndCopiesResultValues()
    {
        var metadata = new MetadataBag();
        metadata.Set("source", "unit-test");

        IReadOnlyList<IssueInfo> issues =
        [
            IssueInfoFactory.Warning(
                "AFW_WARNING",
                "Warning message.")
        ];

        var response = new TestTypedResponse<string>
        {
            Status = ResultStatus.SuccessWithWarnings,
            Message = "Operation completed with warnings.",
            Data = "payload",
            HasData = true,
            Issues = issues,
            Metadata = metadata
        };

        var result = response.ToResult();

        Assert.Equal(response.Status, result.Status);
        Assert.Equal(response.Message, result.Message);
        Assert.Same(response.Issues, result.Issues);
        Assert.Same(response.Metadata, result.Metadata);
        Assert.True(result.IsSuccess);
        Assert.True(result.HasWarnings);
    }

    [Fact]
    public void ToResponse_DoesNotModifyOriginalResult()
    {
        var result = new TestResult
        {
            Status = ResultStatus.Success,
            Message = "Original message."
        };

        var response = result.ToResponse();

        response.Message = "Changed message.";

        Assert.Equal("Original message.", result.Message);
        Assert.Equal("Changed message.", response.Message);
    }

    [Fact]
    public void ToResult_DoesNotModifyOriginalResponse()
    {
        var response = new TestResponse
        {
            Status = ResultStatus.Success,
            Message = "Original message."
        };

        var result = response.ToResult();

        result.Message = "Changed message.";

        Assert.Equal("Original message.", response.Message);
        Assert.Equal("Changed message.", result.Message);
    }


    [Fact]
    public void ToTypedResponse_WhenResponseIsNull_ThrowsArgumentNullException()
    {
        IResponse? response = null;

        Assert.Throws<ArgumentNullException>(() =>
            response!.ToTypedResponse("payload"));
    }

    [Fact]
    public void ToTypedResponse_CreatesTypedResponseWithCopiedValuesAndData()
    {
        var metadata = new MetadataBag();
        metadata.Set("source", "unit-test");

        IReadOnlyList<IssueInfo> issues =
        [
            IssueInfoFactory.Warning(
            "AFW_WARNING",
            "Warning message.")
        ];

        var response = new TestResponse
        {
            Status = ResultStatus.SuccessWithWarnings,
            Message = "Operation completed with warnings.",
            HasData = false,
            Issues = issues,
            Metadata = metadata
        };

        var typedResponse = response.ToTypedResponse("payload");

        Assert.Equal(response.Status, typedResponse.Status);
        Assert.Equal(response.Message, typedResponse.Message);
        Assert.Equal("payload", typedResponse.Data);
        Assert.True(typedResponse.HasData);
        Assert.Same(response.Issues, typedResponse.Issues);
        Assert.Same(response.Metadata, typedResponse.Metadata);
        Assert.True(typedResponse.IsSuccess);
        Assert.True(typedResponse.HasWarnings);
    }

    [Fact]
    public void ToTypedResponse_WithNullData_CreatesTypedResponseWithoutData()
    {
        var response = new TestResponse
        {
            Status = ResultStatus.Success,
            Message = "Operation completed."
        };

        var typedResponse = response.ToTypedResponse<string>(null);

        Assert.Equal(response.Status, typedResponse.Status);
        Assert.Equal(response.Message, typedResponse.Message);
        Assert.Null(typedResponse.Data);
        Assert.False(typedResponse.HasData);
    }


    [Fact]
    public void ToNonGenericResponse_WhenResponseIsNull_ThrowsArgumentNullException()
    {
        IResponse<string>? response = null;

        Assert.Throws<ArgumentNullException>(() =>
            response!.ToNonGenericResponse());
    }

    [Fact]
    public void ToNonGenericResponse_DropsDataAndCopiesValues()
    {
        var metadata = new MetadataBag();
        metadata.Set("source", "unit-test");

        IReadOnlyList<IssueInfo> issues =
        [
            IssueInfoFactory.Warning(
            "AFW_WARNING",
            "Warning message.")
        ];

        var response = new TestTypedResponse<string>
        {
            Status = ResultStatus.SuccessWithWarnings,
            Message = "Operation completed with warnings.",
            Data = "payload",
            HasData = true,
            Issues = issues,
            Metadata = metadata
        };

        var nonGenericResponse = response.ToNonGenericResponse();

        Assert.Equal(response.Status, nonGenericResponse.Status);
        Assert.Equal(response.Message, nonGenericResponse.Message);
        Assert.False(nonGenericResponse.HasData);
        Assert.Same(response.Issues, nonGenericResponse.Issues);
        Assert.Same(response.Metadata, nonGenericResponse.Metadata);
        Assert.True(nonGenericResponse.IsSuccess);
        Assert.True(nonGenericResponse.HasWarnings);
    }

    [Fact]
    public void ToTypedResponse_DoesNotModifyOriginalResponse()
    {
        var response = new TestResponse
        {
            Status = ResultStatus.Success,
            Message = "Original message."
        };

        var typedResponse = response.ToTypedResponse("payload");

        typedResponse.Message = "Changed message.";

        Assert.Equal("Original message.", response.Message);
        Assert.Equal("Changed message.", typedResponse.Message);
    }

    [Fact]
    public void ToNonGenericResponse_DoesNotModifyOriginalTypedResponse()
    {
        var response = new TestTypedResponse<string>
        {
            Status = ResultStatus.Success,
            Message = "Original message.",
            Data = "payload",
            HasData = true
        };

        var nonGenericResponse = response.ToNonGenericResponse();

        nonGenericResponse.Message = "Changed message.";

        Assert.Equal("Original message.", response.Message);
        Assert.Equal("Changed message.", nonGenericResponse.Message);
    }

    [Fact]
    public void ToTypedResponse_FromResponseWithValueTypeDefaultData_HasDataReturnsTrue()
    {
        var response = new TestResponse
        {
            Status = ResultStatus.Success
        };

        var typedResponse = response.ToTypedResponse(0);

        Assert.Equal(0, typedResponse.Data);
        Assert.True(typedResponse.HasData);
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

    private sealed class TestTypedResponse<T> : IResponse<T>
    {
        public ResultStatus Status { get; set; } = ResultStatus.Unknown;

        public string Message { get; set; } = string.Empty;

        public IReadOnlyList<IssueInfo> Issues { get; set; } = IssueInfoListFactory.Empty();

        public MetadataBag Metadata { get; set; } = MetadataBagFactory.Empty();

        public bool HasData { get; set; }

        public T? Data { get; set; }

        public bool IsSuccess =>
            Status is ResultStatus.Success or ResultStatus.SuccessWithWarnings;

        public bool IsFailure =>
            Status is ResultStatus.Failed or ResultStatus.Invalid or ResultStatus.NotFound;

        public bool HasWarnings =>
            Status == ResultStatus.SuccessWithWarnings
            || Issues.HasWarningsOrErrors();
    }
}