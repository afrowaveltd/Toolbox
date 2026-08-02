using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Enums;
using Afrowave.Toolbox.WhenItFails.Initialization;

namespace Afrowave.Toolbox.WhenItFails.Tests.Initialization;

public sealed class ErrorCatalogInitializationPayloadContractTests
{
    [Fact]
    public void NewPayload_ShouldUseProjectCatalogDefaults()
    {
        ErrorCatalogInitializationPayload payload = new();

        Assert.Null(payload.Bootstrap);
        Assert.Null(payload.Context);
        Assert.Equal(
            ErrorCatalogContextSource.ProjectCatalog,
            payload.ContextSource);
        Assert.False(payload.KeptPreviousContext);
        Assert.False(payload.UsedFallback);
        Assert.False(payload.IsDegraded);
    }

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, true)]
    public void IsDegraded_ShouldReflectRecoveryFlags(
        bool keptPreviousContext,
        bool usedFallback,
        bool expected)
    {
        JsonsBootstrapPayload bootstrap = new();
        ErrorCatalogContext context = new();

        ErrorCatalogInitializationPayload payload = new()
        {
            Bootstrap = bootstrap,
            Context = context,
            KeptPreviousContext = keptPreviousContext,
            UsedFallback = usedFallback
        };

        Assert.Same(bootstrap, payload.Bootstrap);
        Assert.Same(context, payload.Context);
        Assert.Equal(expected, payload.IsDegraded);
    }
}
