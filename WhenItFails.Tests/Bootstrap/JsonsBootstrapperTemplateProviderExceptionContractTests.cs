using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Bootstrap;

public sealed class JsonsBootstrapperTemplateProviderExceptionContractTests
{
    [Fact]
    public async Task EnsureWorkspaceAsync_WhenTemplateProviderThrows_ReturnsStableFailureWithoutExceptionDetail()
    {
        const string sensitiveDetail =
            "Sensitive JSON template provider detail must not escape.";

        string rootDirectory = Path.Combine(
            Path.GetTempPath(),
            "Afrowave",
            "WhenItFails.Tests",
            Guid.NewGuid().ToString("N"));

        try
        {
            JsonsBootstrapper bootstrapper = new(
                new ThrowingTemplateProvider(sensitiveDetail));

            JsonsOptions options = new()
            {
                RootDirectory = rootDirectory,
                PackageDirectoryName = "WhenItFails"
            };

            Response<JsonsBootstrapPayload> response =
                await bootstrapper.EnsureWorkspaceAsync(options);

            Assert.NotNull(response);
            Assert.False(response.IsSuccess);
            Assert.Equal(ResultStatus.Failed, response.Status);
            Assert.Null(response.Data);
            Assert.Equal(
                "The JSON template provider failed.",
                response.Message);
            Assert.DoesNotContain(sensitiveDetail, response.Message);

            Assert.Collection(
                response.Issues,
                issue =>
                {
                    Assert.Equal(
                        "WIF_JSONS_TEMPLATE_PROVIDER_FAILED",
                        issue.Code);
                    Assert.Equal(
                        "The JSON template provider failed.",
                        issue.Message);
                });
        }
        finally
        {
            if (Directory.Exists(rootDirectory))
            {
                Directory.Delete(rootDirectory, recursive: true);
            }
        }
    }

    private sealed class ThrowingTemplateProvider : IJsonsTemplateProvider
    {
        private readonly string _detail;

        public ThrowingTemplateProvider(string detail)
        {
            _detail = detail;
        }

        public IReadOnlyList<JsonsTemplateFile> GetTemplateFiles(
            JsonsOptions options)
        {
            throw new InvalidOperationException(_detail);
        }
    }
}
