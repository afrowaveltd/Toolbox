using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class BuiltInErrorCatalogContextProviderTemplateProviderExceptionContractTests
{
    [Fact]
    public async Task LoadAsync_WhenTemplateProviderThrows_ReturnsStableFailureWithoutExceptionDetail()
    {
        BuiltInErrorCatalogContextProvider provider = new(
            new ThrowingTemplateProvider(),
            new UnusedContextProvider());

        Response<ErrorCatalogContext> response =
            await provider.LoadAsync();

        Assert.NotNull(response);
        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Failed, response.Status);
        Assert.Null(response.Data);
        Assert.Equal(
            "The bundled WhenItFails catalog context could not be loaded.",
            response.Message);
        Assert.DoesNotContain(
            "Sensitive built-in template provider detail must not escape.",
            response.Message,
            StringComparison.Ordinal);

        Assert.Collection(
            response.Issues,
            issue =>
            {
                Assert.Equal(
                    "WIF_BUILT_IN_CONTEXT_LOAD_FAILED",
                    issue.Code);
                Assert.Equal(
                    "The bundled WhenItFails catalog context could not be loaded.",
                    issue.Message);
            });
    }

    [Fact]
    public async Task LoadAsync_WhenTemplateProviderCancels_RethrowsSameOperationCanceledException()
    {
        OperationCanceledException cancellation = new(
            "Built-in template provider cancellation must propagate unchanged.");

        BuiltInErrorCatalogContextProvider provider = new(
            new CancellingTemplateProvider(cancellation),
            new UnusedContextProvider());

        OperationCanceledException thrown =
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => provider.LoadAsync());

        Assert.Same(cancellation, thrown);
    }

    private sealed class ThrowingTemplateProvider : IJsonsTemplateProvider
    {
        public IReadOnlyList<JsonsTemplateFile> GetTemplateFiles(
            JsonsOptions options)
        {
            throw new InvalidOperationException(
                "Sensitive built-in template provider detail must not escape.");
        }
    }

    private sealed class CancellingTemplateProvider : IJsonsTemplateProvider
    {
        private readonly OperationCanceledException _cancellation;

        public CancellingTemplateProvider(
            OperationCanceledException cancellation)
        {
            _cancellation = cancellation;
        }

        public IReadOnlyList<JsonsTemplateFile> GetTemplateFiles(
            JsonsOptions options)
        {
            throw _cancellation;
        }
    }

    private sealed class UnusedContextProvider : IErrorCatalogContextProvider
    {
        public Task<Response<ErrorCatalogContext>> LoadFromJsonsAsync(
            JsonsOptions options,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "The context provider must not run after the template provider throws or cancels.");
        }
    }
}
