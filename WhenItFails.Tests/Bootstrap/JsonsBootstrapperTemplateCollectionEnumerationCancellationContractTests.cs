using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Bootstrap;

public sealed class JsonsBootstrapperTemplateCollectionEnumerationCancellationContractTests
{
    [Fact]
    public async Task EnsureWorkspaceAsync_WhenTemplateCollectionEnumerationCancels_RethrowsSameOperationCanceledException()
    {
        OperationCanceledException cancellation = new(
            "Template collection enumeration cancellation must propagate unchanged.");

        string rootDirectory = Path.Combine(
            Path.GetTempPath(),
            "Afrowave",
            "WhenItFails.Tests",
            Guid.NewGuid().ToString("N"));

        string targetFilePath = Path.Combine(
            rootDirectory,
            "WhenItFails",
            "errors.en.json");

        try
        {
            JsonsBootstrapper bootstrapper = new(
                new CancelingEnumerationTemplateProvider(cancellation));

            OperationCanceledException thrown =
                await Assert.ThrowsAsync<OperationCanceledException>(
                    () => bootstrapper.EnsureWorkspaceAsync(
                        new JsonsOptions
                        {
                            RootDirectory = rootDirectory,
                            PackageDirectoryName = "WhenItFails"
                        }));

            Assert.Same(cancellation, thrown);
            Assert.False(File.Exists(targetFilePath));
        }
        finally
        {
            if (Directory.Exists(rootDirectory))
            {
                Directory.Delete(rootDirectory, recursive: true);
            }
        }
    }

    private sealed class CancelingEnumerationTemplateProvider
        : IJsonsTemplateProvider
    {
        private readonly OperationCanceledException _cancellation;

        public CancelingEnumerationTemplateProvider(
            OperationCanceledException cancellation)
        {
            _cancellation = cancellation;
        }

        public IReadOnlyList<JsonsTemplateFile> GetTemplateFiles(
            JsonsOptions options)
        {
            return new CancelingTemplateCollection(_cancellation);
        }
    }

    private sealed class CancelingTemplateCollection
        : IReadOnlyList<JsonsTemplateFile>
    {
        private readonly OperationCanceledException _cancellation;

        public CancelingTemplateCollection(
            OperationCanceledException cancellation)
        {
            _cancellation = cancellation;
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
            throw _cancellation;
        }

        System.Collections.IEnumerator
            System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
