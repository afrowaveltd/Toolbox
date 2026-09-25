using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Bootstrap;

public sealed class JsonsBootstrapperSafeTemplateWriteContractTests
{
    [Fact]
    public async Task EnsureWorkspaceAsync_WhenCancellationArrivesAfterTemporaryWrite_DoesNotPublishPartialTarget()
    {
        string rootDirectory = CreateTemporaryRootDirectory();
        string packageDirectoryPath = Path.Combine(
            rootDirectory,
            "WhenItFails");
        string targetFilePath = Path.Combine(
            packageDirectoryPath,
            "errors.en.json");

        using CancellationTokenSource cancellationSource = new();

        string? observedTemporaryFilePath = null;

        try
        {
            JsonsBootstrapper bootstrapper = new(
                new SingleTemplateProvider(),
                async (temporaryFilePath, content, cancellationToken) =>
                {
                    observedTemporaryFilePath = temporaryFilePath;

                    await File.WriteAllTextAsync(
                        temporaryFilePath,
                        "{ \"partial\": true }",
                        CancellationToken.None);

                    cancellationSource.Cancel();
                });

            OperationCanceledException exception =
                await Assert.ThrowsAnyAsync<OperationCanceledException>(
                    () => bootstrapper.EnsureWorkspaceAsync(
                        CreateOptions(rootDirectory),
                        cancellationSource.Token));

            Assert.Equal(
                cancellationSource.Token,
                exception.CancellationToken);

            Assert.NotNull(observedTemporaryFilePath);
            Assert.False(File.Exists(observedTemporaryFilePath));
            Assert.False(File.Exists(targetFilePath));

            Assert.True(Directory.Exists(packageDirectoryPath));
            Assert.Empty(Directory.GetFiles(packageDirectoryPath));
        }
        finally
        {
            DeleteDirectoryIfExists(rootDirectory);
        }
    }

    [Fact]
    public async Task EnsureWorkspaceAsync_WhenTargetAppearsBeforePublish_PreservesConcurrentFileAndReportsSkipped()
    {
        string rootDirectory = CreateTemporaryRootDirectory();
        string packageDirectoryPath = Path.Combine(
            rootDirectory,
            "WhenItFails");
        string targetFilePath = Path.Combine(
            packageDirectoryPath,
            "errors.en.json");

        const string concurrentContent =
            "{ \"catalogId\": \"concurrent-writer\" }";

        string? observedTemporaryFilePath = null;

        try
        {
            JsonsBootstrapper bootstrapper = new(
                new SingleTemplateProvider(),
                async (temporaryFilePath, content, cancellationToken) =>
                {
                    observedTemporaryFilePath = temporaryFilePath;

                    await File.WriteAllTextAsync(
                        temporaryFilePath,
                        content,
                        cancellationToken);

                    await File.WriteAllTextAsync(
                        targetFilePath,
                        concurrentContent,
                        cancellationToken);
                });

            Response<JsonsBootstrapPayload> response =
                await bootstrapper.EnsureWorkspaceAsync(
                    CreateOptions(rootDirectory));

            Assert.True(response.IsSuccess);
            Assert.NotNull(response.Data);

            JsonsBootstrapFileResult fileResult =
                Assert.Single(response.Data.Files);

            Assert.True(fileResult.AlreadyExisted);
            Assert.False(fileResult.Created);
            Assert.True(fileResult.Skipped);
            Assert.Equal(
                targetFilePath,
                fileResult.TargetFilePath);

            Assert.Equal(
                concurrentContent,
                await File.ReadAllTextAsync(targetFilePath));

            Assert.NotNull(observedTemporaryFilePath);
            Assert.False(File.Exists(observedTemporaryFilePath));

            string[] remainingFiles =
                Directory.GetFiles(packageDirectoryPath);

            Assert.Single(remainingFiles);
            Assert.Equal(
                targetFilePath,
                remainingFiles[0]);
        }
        finally
        {
            DeleteDirectoryIfExists(rootDirectory);
        }
    }

    [Fact]
    public async Task EnsureWorkspaceAsync_WhenTemporaryWriteFails_RemovesTemporaryArtifactWithoutPublishingTarget()
    {
        string rootDirectory = CreateTemporaryRootDirectory();
        string packageDirectoryPath = Path.Combine(
            rootDirectory,
            "WhenItFails");
        string targetFilePath = Path.Combine(
            packageDirectoryPath,
            "errors.en.json");

        string? observedTemporaryFilePath = null;

        try
        {
            JsonsBootstrapper bootstrapper = new(
                new SingleTemplateProvider(),
                async (temporaryFilePath, content, cancellationToken) =>
                {
                    observedTemporaryFilePath = temporaryFilePath;

                    await File.WriteAllTextAsync(
                        temporaryFilePath,
                        "{ \"partial\": true }",
                        cancellationToken);

                    throw new IOException(
                        "Deterministic test write failure.");
                });

            Response<JsonsBootstrapPayload> response =
                await bootstrapper.EnsureWorkspaceAsync(
                    CreateOptions(rootDirectory));

            Assert.False(response.IsSuccess);
            Assert.Equal(
                ResultStatus.Failed,
                response.Status);
            Assert.Equal(
                "JsonsWorkspaceInputOutputError",
                response.Issues[0].Code);

            Assert.NotNull(observedTemporaryFilePath);
            Assert.False(File.Exists(observedTemporaryFilePath));
            Assert.False(File.Exists(targetFilePath));

            Assert.True(Directory.Exists(packageDirectoryPath));
            Assert.Empty(Directory.GetFiles(packageDirectoryPath));
        }
        finally
        {
            DeleteDirectoryIfExists(rootDirectory);
        }
    }

    private static JsonsOptions CreateOptions(
        string rootDirectory)
    {
        return new JsonsOptions
        {
            RootDirectory = rootDirectory,
            PackageDirectoryName = "WhenItFails"
        };
    }

    private static string CreateTemporaryRootDirectory()
    {
        return Path.Combine(
            Path.GetTempPath(),
            "Afrowave",
            "WhenItFails.Tests",
            Guid.NewGuid().ToString("N"));
    }

    private static void DeleteDirectoryIfExists(
        string directoryPath)
    {
        if (Directory.Exists(directoryPath))
        {
            Directory.Delete(
                directoryPath,
                recursive: true);
        }
    }

    private sealed class SingleTemplateProvider
        : IJsonsTemplateProvider
    {
        public IReadOnlyList<JsonsTemplateFile> GetTemplateFiles(
            JsonsOptions options)
        {
            return
            [
                new JsonsTemplateFile
                {
                    Name = "Error catalog",
                    TargetFileName = "errors.en.json",
                    Content = "{ \"catalogId\": \"template\" }"
                }
            ];
        }
    }
}
