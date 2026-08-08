using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Bootstrap;

public sealed class JsonsBootstrapperNullTemplateResultContractTests
{
    [Fact]
    public async Task EnsureWorkspaceAsync_ShouldReturnInvalidResponse_WhenTemplateProviderReturnsNullCollection()
    {
        JsonsBootstrapper bootstrapper = new(
            new NullCollectionTemplateProvider());

        Response<JsonsBootstrapPayload> response =
            await bootstrapper.EnsureWorkspaceAsync(CreateOptions());

        AssertInvalidResponse(
            response,
            "WIF_JSONS_TEMPLATE_COLLECTION_NULL",
            "The JSON template provider returned a null template collection.");
    }

    [Fact]
    public async Task EnsureWorkspaceAsync_ShouldReturnInvalidResponse_WhenTemplateProviderReturnsNullItem()
    {
        JsonsBootstrapper bootstrapper = new(
            new NullItemTemplateProvider());

        Response<JsonsBootstrapPayload> response =
            await bootstrapper.EnsureWorkspaceAsync(CreateOptions());

        AssertInvalidResponse(
            response,
            "WIF_JSONS_TEMPLATE_ITEM_NULL",
            "The JSON template provider returned a null template item.");
    }

    private static void AssertInvalidResponse(
        Response<JsonsBootstrapPayload> response,
        string code,
        string message)
    {
        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);

        var issue = Assert.Single(response.Issues);
        Assert.Equal(code, issue.Code);
        Assert.Equal(message, response.Message);
    }

    private static JsonsOptions CreateOptions()
    {
        return new JsonsOptions
        {
            RootDirectory = Path.Combine(
                Path.GetTempPath(),
                "Afrowave",
                "WhenItFails.Tests",
                Guid.NewGuid().ToString("N")),
            PackageDirectoryName = "WhenItFails"
        };
    }

    private sealed class NullCollectionTemplateProvider
        : IJsonsTemplateProvider
    {
        public IReadOnlyList<JsonsTemplateFile> GetTemplateFiles(
            JsonsOptions options)
        {
            return null!;
        }
    }

    private sealed class NullItemTemplateProvider
        : IJsonsTemplateProvider
    {
        public IReadOnlyList<JsonsTemplateFile> GetTemplateFiles(
            JsonsOptions options)
        {
            return [null!];
        }
    }
}
