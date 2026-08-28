using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class BuiltInErrorCatalogContextProviderNullTemplatesCollectionContractTests
{
    [Fact]
    public async Task LoadAsync_ShouldReturnInvalid_WhenTemplateCollectionIsNull()
    {
        BuiltInErrorCatalogContextProvider provider = new(
            new NullTemplateCollectionProvider(),
            new ThrowingContextProvider());

        Response<ErrorCatalogContext> response =
            await provider.LoadAsync();

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);

        var issue = Assert.Single(response.Issues);
        Assert.Equal("WIF_BUILT_IN_TEMPLATES_NULL", issue.Code);
        Assert.Equal(
            "The bundled WhenItFails catalog template provider returned a null collection.",
            response.Message);
    }

    private sealed class NullTemplateCollectionProvider
        : IJsonsTemplateProvider
    {
        public IReadOnlyList<JsonsTemplateFile> GetTemplateFiles(
            JsonsOptions options)
        {
            return null!;
        }
    }

    private sealed class ThrowingContextProvider
        : IErrorCatalogContextProvider
    {
        public Task<Response<ErrorCatalogContext>> LoadFromJsonsAsync(
            JsonsOptions options,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "Context provider must not run when the template collection is null.");
        }
    }
}
