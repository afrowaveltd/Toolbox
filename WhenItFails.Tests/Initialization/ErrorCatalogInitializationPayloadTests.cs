using Afrowave.Toolbox.WhenItFails.Enums;
using Afrowave.Toolbox.WhenItFails.Initialization;

namespace Afrowave.Toolbox.WhenItFails.Tests.Initialization;

public sealed class ErrorCatalogInitializationPayloadTests
{
    [Fact]
    public void Constructor_ShouldUseProjectCatalogAsDefaultContextSource()
    {
        ErrorCatalogInitializationPayload payload = new();

        Assert.Equal(
            ErrorCatalogContextSource.ProjectCatalog,
            payload.ContextSource);
    }

    [Fact]
    public void Constructor_ShouldNotReportRecoveryByDefault()
    {
        ErrorCatalogInitializationPayload payload = new();

        Assert.False(
            payload.KeptPreviousContext);

        Assert.False(
            payload.UsedFallback);

        Assert.False(
            payload.IsDegraded);
    }

    [Fact]
    public void IsDegraded_ShouldBeTrue_WhenPreviousContextWasKept()
    {
        ErrorCatalogInitializationPayload payload = new()
        {
            ContextSource =
                ErrorCatalogContextSource.PreviousContext,

            KeptPreviousContext = true
        };

        Assert.True(
            payload.IsDegraded);

        Assert.True(
            payload.KeptPreviousContext);

        Assert.False(
            payload.UsedFallback);
    }

    [Fact]
    public void IsDegraded_ShouldBeTrue_WhenFallbackWasUsed()
    {
        ErrorCatalogInitializationPayload payload = new()
        {
            ContextSource =
                ErrorCatalogContextSource.BuiltInDefaults,

            UsedFallback = true
        };

        Assert.True(
            payload.IsDegraded);

        Assert.False(
            payload.KeptPreviousContext);

        Assert.True(
            payload.UsedFallback);
    }

    [Fact]
    public void ContextSource_ShouldAcceptAllSupportedValues()
    {
        ErrorCatalogInitializationPayload payload = new();

        payload.ContextSource =
            ErrorCatalogContextSource.ProjectCatalog;

        Assert.Equal(
            ErrorCatalogContextSource.ProjectCatalog,
            payload.ContextSource);

        payload.ContextSource =
            ErrorCatalogContextSource.PreviousContext;

        Assert.Equal(
            ErrorCatalogContextSource.PreviousContext,
            payload.ContextSource);

        payload.ContextSource =
            ErrorCatalogContextSource.BuiltInDefaults;

        Assert.Equal(
            ErrorCatalogContextSource.BuiltInDefaults,
            payload.ContextSource);
    }

    [Theory]
    [InlineData(ErrorCatalogContextSource.ProjectCatalog, 0)]
    [InlineData(ErrorCatalogContextSource.PreviousContext, 1)]
    [InlineData(ErrorCatalogContextSource.BuiltInDefaults, 2)]
    public void ContextSource_ShouldHaveStableNumericValue(
        ErrorCatalogContextSource source,
        int expectedValue)
    {
        Assert.Equal(
            expectedValue,
            (int)source);
    }
}