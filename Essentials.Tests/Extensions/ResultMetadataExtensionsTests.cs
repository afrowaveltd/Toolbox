using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Extensions;
using Afrowave.Toolbox.Essentials.Interfaces;
using Afrowave.Toolbox.Essentials.Issues;
using Afrowave.Toolbox.Essentials.Metadata;

namespace Afrowave.Toolbox.Essentials.Tests.Extensions;

public sealed class ResultMetadataExtensionsTests
{
    [Fact]
    public void HasMetadata_WhenResultIsNull_ThrowsArgumentNullException()
    {
        IResult? result = null;

        Assert.Throws<ArgumentNullException>(() =>
            result!.HasMetadata());
    }

    [Fact]
    public void HasMetadata_WhenMetadataIsEmpty_ReturnsFalse()
    {
        var result = new TestResult
        {
            Metadata = MetadataBagFactory.Empty()
        };

        var actual = result.HasMetadata();

        Assert.False(actual);
    }

    [Fact]
    public void HasMetadata_WhenMetadataContainsValue_ReturnsTrue()
    {
        var metadata = new MetadataBag();
        metadata.Set("source", "unit-test");

        var result = new TestResult
        {
            Metadata = metadata
        };

        var actual = result.HasMetadata();

        Assert.True(actual);
    }

    [Fact]
    public void HasMetadataKey_WhenResultIsNull_ThrowsArgumentNullException()
    {
        IResult? result = null;

        Assert.Throws<ArgumentNullException>(() =>
            result!.HasMetadataKey("source"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void HasMetadataKey_WhenKeyIsInvalid_ThrowsArgumentException(
        string? key)
    {
        var result = new TestResult();

        Assert.ThrowsAny<ArgumentException>(() =>
            result.HasMetadataKey(key!));
    }

    [Fact]
    public void HasMetadataKey_WhenKeyDoesNotExist_ReturnsFalse()
    {
        var metadata = new MetadataBag();
        metadata.Set("source", "unit-test");

        var result = new TestResult
        {
            Metadata = metadata
        };

        var actual = result.HasMetadataKey("missing");

        Assert.False(actual);
    }

    [Fact]
    public void HasMetadataKey_WhenKeyExists_ReturnsTrue()
    {
        var metadata = new MetadataBag();
        metadata.Set("source", "unit-test");

        var result = new TestResult
        {
            Metadata = metadata
        };

        var actual = result.HasMetadataKey("source");

        Assert.True(actual);
    }

    [Fact]
    public void HasMetadataKey_UsesCaseInsensitiveLookup()
    {
        var metadata = new MetadataBag();
        metadata.Set("OriginalKey", "value");

        var result = new TestResult
        {
            Metadata = metadata
        };

        var actual = result.HasMetadataKey("originalkey");

        Assert.True(actual);
    }

    [Fact]
    public void TryGetMetadata_WhenResultIsNull_ThrowsArgumentNullException()
    {
        IResult? result = null;

        Assert.Throws<ArgumentNullException>(() =>
            result!.TryGetMetadata("source", out _));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryGetMetadata_WhenKeyIsInvalid_ThrowsArgumentException(
        string? key)
    {
        var result = new TestResult();

        Assert.ThrowsAny<ArgumentException>(() =>
            result.TryGetMetadata(key!, out _));
    }

    [Fact]
    public void TryGetMetadata_WhenKeyDoesNotExist_ReturnsFalseAndNullValue()
    {
        var metadata = new MetadataBag();
        metadata.Set("source", "unit-test");

        var result = new TestResult
        {
            Metadata = metadata
        };

        var actual = result.TryGetMetadata("missing", out var value);

        Assert.False(actual);
        Assert.Null(value);
    }

    [Fact]
    public void TryGetMetadata_WhenKeyExists_ReturnsTrueAndValue()
    {
        var metadata = new MetadataBag();
        metadata.Set("source", "unit-test");

        var result = new TestResult
        {
            Metadata = metadata
        };

        var actual = result.TryGetMetadata("source", out var value);

        Assert.True(actual);
        Assert.Equal("unit-test", value);
    }

    [Fact]
    public void TryGetMetadata_UsesCaseInsensitiveLookup()
    {
        var metadata = new MetadataBag();
        metadata.Set("OriginalKey", "value");

        var result = new TestResult
        {
            Metadata = metadata
        };

        var actual = result.TryGetMetadata("originalkey", out var value);

        Assert.True(actual);
        Assert.Equal("value", value);
    }
    [Fact]
    public void GetMetadataOrDefault_WhenResultIsNull_ThrowsArgumentNullException()
    {
        IResult? result = null;

        Assert.Throws<ArgumentNullException>(() =>
           result!.GetMetadataOrDefault("source"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetMetadataOrDefault_WhenKeyIsInvalid_ThrowsArgumentException(
       string? key)
    {
        var result = new TestResult
        {
            Metadata = new MetadataBag()
        };

        Assert.ThrowsAny<ArgumentException>(() =>
           result.GetMetadataOrDefault(key!));
    }

    [Fact]
    public void GetMetadataOrDefault_WhenKeyExists_ReturnsMetadataValue()
    {
        var result = new TestResult
        {
            Metadata = new MetadataBag()
        };

        result.Metadata.Set("source", "unit-test");

        var actual = result.GetMetadataOrDefault("source");

        Assert.Equal("unit-test", actual);
    }

    [Fact]
    public void GetMetadataOrDefault_WhenKeyExists_IgnoresFallback()
    {
        var result = new TestResult
        {
            Metadata = new MetadataBag()
        };

        result.Metadata.Set("source", "unit-test");

        var actual = result.GetMetadataOrDefault(
           "source",
           "fallback");

        Assert.Equal("unit-test", actual);
    }

    [Fact]
    public void GetMetadataOrDefault_WhenKeyDoesNotExist_ReturnsNullByDefault()
    {
        var result = new TestResult
        {
            Metadata = new MetadataBag()
        };

        var actual = result.GetMetadataOrDefault("missing");

        Assert.Null(actual);
    }

    [Fact]
    public void GetMetadataOrDefault_WhenKeyDoesNotExist_ReturnsFallback()
    {
        var result = new TestResult
        {
            Metadata = new MetadataBag()
        };

        var actual = result.GetMetadataOrDefault(
           "missing",
           "fallback");

        Assert.Equal("fallback", actual);
    }

    [Fact]
    public void GetMetadataOrDefault_UsesCaseInsensitiveLookup()
    {
        var result = new TestResult
        {
            Metadata = new MetadataBag()
        };

        result.Metadata.Set("OriginalKey", "value");

        var actual = result.GetMetadataOrDefault("originalkey");

        Assert.Equal("value", actual);
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