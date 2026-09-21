using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Bootstrap;

public sealed class JsonsBootstrapperTemplateCollectionEnumerationExceptionContractTests
{
    [Fact]
    public async Task EnsureWorkspaceAsync_WhenTemplateCollectionEnumerationThrows_ReturnsStableProviderFailureWithoutExceptionDetail()
    {
        const string sensitiveDetail =
            "Sensitive template collection enumeration detail must not escape.";

        string rootDirectory = Path.Combine(
            Path.GetTempPath(),
            "Afrowave",
            "WhenItFails.Tests",
            Guid.NewGuid().ToString("N"));

        try
        {
            JsonsBootstrapper bootstrapper = new(
                new ThrowingEnumerationTemplateProvider(sensitiveDetail));

            Response<JsonsBootstrapPayload> response =
                await bootstrapper.EnsureWorkspaceAsync(
                    new JsonsOptions
                    {
                        RootDirectory = rootDirectory,
                        PackageDirectoryName = "WhenItFails"
                    });

            Assert.False(response.IsSuccess);
            Assert.Equal(ResultStatus.Failed, response.Status);
            Assert.Null(response.Data);
            Assert.Equal(
                "The JSON template provider failed.",
                response.Message);
            Assert.DoesNotContain(
                sensitiveDetail,
                response.Message);

            var issue = Assert.Single(response.Issues);
            Assert.Equal(
                "WIF_JSONS_TEMPLATE_PROVIDER_FAILED",
                issue.Code);
            Assert.Equal(
                "The JSON template provider failed.",
                issue.Message);
            Assert.DoesNotContain(
                sensitiveDetail,
                issue.Message);
        }
        finally
        {
            if (Directory.Exists(rootDirectory))
            {
                Directory.Delete(rootDirectory, recursive: true);
            }
        }
    }

    private sealed class ThrowingEnumerationTemplateProvider
        : IJsonsTemplateProvider
    {
        private readonly string _detail;

        public ThrowingEnumerationTemplateProvider(string detail)
        {
            _detail = detail;
        }

        public IReadOnlyList<JsonsTemplateFile> GetTemplateFiles(
            JsonsOptions options)
        {
            return new ThrowingTemplateCollection(_detail);
        }
    }

    private sealed class ThrowingTemplateCollection
        : IReadOnlyList<JsonsTemplateFile>
    {
        private readonly string _detail;

        public ThrowingTemplateCollection(string detail)
        {
            _detail = detail;
        }

        public int Count => 1;

        public JsonsTemplateFile this[int index] =>
            new()
            {
                Name = "Errors",
                TargetFileName = "errors.en.json",
                Content = "{}"
            };

        public IEnumerator<JsonsTemplateFile> GetEnumerator()
        {
            throw new InvalidOperationException(_detail);
        }

        System.Collections.IEnumerator
            System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
