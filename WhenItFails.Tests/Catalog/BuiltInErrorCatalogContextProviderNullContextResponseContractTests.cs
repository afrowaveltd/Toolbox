using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class BuiltInErrorCatalogContextProviderNullContextResponseContractTests
{
    [Fact]
    public async Task LoadAsync_ShouldReturnInvalidResponse_WhenContextProviderReturnsNull()
    {
        BuiltInErrorCatalogContextProvider provider = new(
            new ValidTemplateProvider(),
            new NullResponseContextProvider());

        Response<ErrorCatalogContext> response =
            await provider.LoadAsync();

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);

        var issue = Assert.Single(response.Issues);
        Assert.Equal(
            "WIF_BUILT_IN_CONTEXT_PROVIDER_RESPONSE_NULL",
            issue.Code);
        Assert.Equal(
            "The error catalog context provider returned a null response while loading bundled defaults.",
            response.Message);
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

    private sealed class NullResponseContextProvider
        : IErrorCatalogContextProvider
    {
        public Task<Response<ErrorCatalogContext>> LoadFromJsonsAsync(
            JsonsOptions options,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Response<ErrorCatalogContext>>(null!);
        }
    }
}
