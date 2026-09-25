using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Bootstrap;

public sealed class JsonsBootstrapperPartialWorkspaceReentryContractTests
{
    [Fact]
    public async Task EnsureWorkspaceAsync_AfterCancelledPartialRun_PreservesPublishedUserFileAndCreatesOnlyMissingTemplate()
    {
        string rootDirectory = CreateTemporaryRootDirectory();
        string packageDirectoryPath = Path.Combine(
            rootDirectory,
            "WhenItFails");

        string firstTargetPath = Path.Combine(
            packageDirectoryPath,
            "errors.en.json");

        string secondTargetPath = Path.Combine(
            packageDirectoryPath,
            "categories.en.json");

        using CancellationTokenSource cancellationSource = new();

        int firstRunWriteCallCount = 0;

        try
        {
            TwoTemplateProvider templateProvider = new();

            JsonsBootstrapper cancellingBootstrapper = new(
                templateProvider,
                async (temporaryFilePath, content, cancellationToken) =>
                {
                    firstRunWriteCallCount++;

                    await File.WriteAllTextAsync(
                        temporaryFilePath,
                        content,
                        CancellationToken.None);

                    if (firstRunWriteCallCount == 2)
                    {
                        cancellationSource.Cancel();
                    }
                });

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => cancellingBootstrapper.EnsureWorkspaceAsync(
                    CreateOptions(rootDirectory),
                    cancellationSource.Token));

            Assert.Equal(2, firstRunWriteCallCount);
            Assert.True(File.Exists(firstTargetPath));
            Assert.False(File.Exists(secondTargetPath));

            const string userEditedFirstContent =
                "{ \"catalogId\": \"user-edited-after-cancellation\" }";

            await File.WriteAllTextAsync(
                firstTargetPath,
                userEditedFirstContent);

            JsonsBootstrapper retryBootstrapper = new(
                templateProvider);

            Response<JsonsBootstrapPayload> retryResponse =
                await retryBootstrapper.EnsureWorkspaceAsync(
                    CreateOptions(rootDirectory));

            Assert.True(retryResponse.IsSuccess);
            Assert.NotNull(retryResponse.Data);
            Assert.Equal(2, retryResponse.Data.Files.Count);

            JsonsBootstrapFileResult firstResult =
                Assert.Single(
                    retryResponse.Data.Files,
                    file => file.TargetFilePath == firstTargetPath);

            Assert.True(firstResult.AlreadyExisted);
            Assert.False(firstResult.Created);
            Assert.True(firstResult.Skipped);

            JsonsBootstrapFileResult secondResult =
                Assert.Single(
                    retryResponse.Data.Files,
                    file => file.TargetFilePath == secondTargetPath);

            Assert.False(secondResult.AlreadyExisted);
            Assert.True(secondResult.Created);
            Assert.False(secondResult.Skipped);

            Assert.Equal(
                userEditedFirstContent,
                await File.ReadAllTextAsync(firstTargetPath));

            Assert.Equal(
                TwoTemplateProvider.SecondContent,
                await File.ReadAllTextAsync(secondTargetPath));

            Assert.Empty(
                Directory.GetFiles(
                    packageDirectoryPath,
                    "*.tmp",
                    SearchOption.AllDirectories));
        }
        finally
        {
            DeleteDirectoryIfExists(rootDirectory);
        }
    }

    [Fact]
    public async Task EnsureWorkspaceAsync_AfterFailedPartialRun_PreservesPublishedUserFileAndCreatesOnlyMissingTemplate()
    {
        string rootDirectory = CreateTemporaryRootDirectory();
        string packageDirectoryPath = Path.Combine(
            rootDirectory,
            "WhenItFails");

        string firstTargetPath = Path.Combine(
            packageDirectoryPath,
            "errors.en.json");

        string secondTargetPath = Path.Combine(
            packageDirectoryPath,
            "categories.en.json");

        int firstRunWriteCallCount = 0;

        try
        {
            TwoTemplateProvider templateProvider = new();

            JsonsBootstrapper failingBootstrapper = new(
                templateProvider,
                async (temporaryFilePath, content, cancellationToken) =>
                {
                    firstRunWriteCallCount++;

                    await File.WriteAllTextAsync(
                        temporaryFilePath,
                        content,
                        cancellationToken);

                    if (firstRunWriteCallCount == 2)
                    {
                        throw new IOException(
                            "Deterministic second-template write failure.");
                    }
                });

            Response<JsonsBootstrapPayload> firstResponse =
                await failingBootstrapper.EnsureWorkspaceAsync(
                    CreateOptions(rootDirectory));

            Assert.False(firstResponse.IsSuccess);
            Assert.Null(firstResponse.Data);
            Assert.Equal(2, firstRunWriteCallCount);

            Assert.True(File.Exists(firstTargetPath));
            Assert.False(File.Exists(secondTargetPath));

            const string userEditedFirstContent =
                "{ \"catalogId\": \"user-edited-after-io-failure\" }";

            await File.WriteAllTextAsync(
                firstTargetPath,
                userEditedFirstContent);

            JsonsBootstrapper retryBootstrapper = new(
                templateProvider);

            Response<JsonsBootstrapPayload> retryResponse =
                await retryBootstrapper.EnsureWorkspaceAsync(
                    CreateOptions(rootDirectory));

            Assert.True(retryResponse.IsSuccess);
            Assert.NotNull(retryResponse.Data);
            Assert.Equal(2, retryResponse.Data.Files.Count);

            JsonsBootstrapFileResult firstResult =
                Assert.Single(
                    retryResponse.Data.Files,
                    file => file.TargetFilePath == firstTargetPath);

            Assert.True(firstResult.AlreadyExisted);
            Assert.False(firstResult.Created);
            Assert.True(firstResult.Skipped);

            JsonsBootstrapFileResult secondResult =
                Assert.Single(
                    retryResponse.Data.Files,
                    file => file.TargetFilePath == secondTargetPath);

            Assert.False(secondResult.AlreadyExisted);
            Assert.True(secondResult.Created);
            Assert.False(secondResult.Skipped);

            Assert.Equal(
                userEditedFirstContent,
                await File.ReadAllTextAsync(firstTargetPath));

            Assert.Equal(
                TwoTemplateProvider.SecondContent,
                await File.ReadAllTextAsync(secondTargetPath));

            Assert.Empty(
                Directory.GetFiles(
                    packageDirectoryPath,
                    "*.tmp",
                    SearchOption.AllDirectories));
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

    private sealed class TwoTemplateProvider
        : IJsonsTemplateProvider
    {
        internal const string FirstContent =
            "{ \"catalogId\": \"errors\" }";

        internal const string SecondContent =
            "{ \"catalogId\": \"categories\" }";

        public IReadOnlyList<JsonsTemplateFile> GetTemplateFiles(
            JsonsOptions options)
        {
            return
            [
                new JsonsTemplateFile
                {
                    Name = "Error catalog",
                    TargetFileName = "errors.en.json",
                    Content = FirstContent
                },
                new JsonsTemplateFile
                {
                    Name = "Category catalog",
                    TargetFileName = "categories.en.json",
                    Content = SecondContent
                }
            ];
        }
    }
}
