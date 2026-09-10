using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class BuiltInErrorCatalogContextProviderContextProviderExceptionContractTests
{
    [Fact]
    public async Task LoadAsync_WhenContextProviderThrows_ReturnsStableFailureWithoutExceptionDetail()
    {
        BuiltInErrorCatalogContextProvider provider = new(
            new ValidTemplateProvider(),
            new ThrowingContextProvider());

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
            "Sensitive built-in context provider detail must not escape.",
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
    public async Task LoadAsync_WhenContextProviderCancels_RethrowsSameOperationCanceledException()
    {
        OperationCanceledException cancellation = new(
            "Built-in context provider cancellation must propagate unchanged.");

        BuiltInErrorCatalogContextProvider provider = new(
            new ValidTemplateProvider(),
            new CancellingContextProvider(cancellation));

        OperationCanceledException thrown =
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => provider.LoadAsync());

        Assert.Same(cancellation, thrown);
    }

    private sealed class ValidTemplateProvider : IJsonsTemplateProvider
    {
        public IReadOnlyList<JsonsTemplateFile> GetTemplateFiles(
            JsonsOptions options)
        {
            return
            [
                new JsonsTemplateFile
                {
                    TargetFileName = "errors.json",
                    Content = "{}"
                }
            ];
        }
    }

    private sealed class ThrowingContextProvider : IErrorCatalogContextProvider
    {
        public Task<Response<ErrorCatalogContext>> LoadFromJsonsAsync(
            JsonsOptions options,
            CancellationToken cancellationToken = default)
        {
            return Task.FromException<Response<ErrorCatalogContext>>(
                new InvalidOperationException(
                    "Sensitive built-in context provider detail must not escape."));
        }
    }

    private sealed class CancellingContextProvider : IErrorCatalogContextProvider
    {
        private readonly OperationCanceledException _cancellation;

        public CancellingContextProvider(
            OperationCanceledException cancellation)
        {
            _cancellation = cancellation;
        }

        public Task<Response<ErrorCatalogContext>> LoadFromJsonsAsync(
            JsonsOptions options,
            CancellationToken cancellationToken = default)
        {
            return Task.FromException<Response<ErrorCatalogContext>>(
                _cancellation);
        }
    }
}
