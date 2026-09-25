using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Bootstrap;

public sealed class JsonsBootstrapperPartialWorkspaceContractTests
{
    [Fact]
    public async Task EnsureWorkspaceAsync_WhenSecondTemplateIsCancelled_PreservesFirstPublishedFileAndLeavesNoPartialSecondTarget()
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

        int writeCallCount = 0;

        try
        {
            JsonsBootstrapper bootstrapper = new(
                new TwoTemplateProvider(),
                async (temporaryFilePath, content, cancellationToken) =>
                {
                    writeCallCount++;

                    if (writeCallCount == 1)
                    {
                        await File.WriteAllTextAsync(
                            temporaryFilePath,
                            content,
                            cancellationToken);

                        return;
                    }

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

            Assert.Equal(2, writeCallCount);

            Assert.True(File.Exists(firstTargetPath));
            Assert.Equal(
                TwoTemplateProvider.FirstContent,
                await File.ReadAllTextAsync(firstTargetPath));

            Assert.False(File.Exists(secondTargetPath));

            Assert.Empty(
                Directory.GetFiles(
                    packageDirectoryPath,
                    "*.tmp",
                    SearchOption.AllDirectories));

            string[] remainingFiles =
                Directory.GetFiles(
                    packageDirectoryPath,
                    "*",
                    SearchOption.AllDirectories);

            Assert.Single(remainingFiles);
            Assert.Equal(
                firstTargetPath,
                remainingFiles[0]);
        }
        finally
        {
            DeleteDirectoryIfExists(rootDirectory);
        }
    }

    [Fact]
    public async Task EnsureWorkspaceAsync_WhenSecondTemplateWriteFails_PreservesFirstPublishedFileAndCleansSecondTemporaryFile()
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

        int writeCallCount = 0;

        try
        {
            JsonsBootstrapper bootstrapper = new(
                new TwoTemplateProvider(),
                async (temporaryFilePath, content, cancellationToken) =>
                {
                    writeCallCount++;

                    if (writeCallCount == 1)
                    {
                        await File.WriteAllTextAsync(
                            temporaryFilePath,
                            content,
                            cancellationToken);

                        return;
                    }

                    await File.WriteAllTextAsync(
                        temporaryFilePath,
                        "{ \"partial\": true }",
                        cancellationToken);

                    throw new IOException(
                        "Deterministic second-template write failure.");
                });

            Response<JsonsBootstrapPayload> response =
                await bootstrapper.EnsureWorkspaceAsync(
                    CreateOptions(rootDirectory));

            Assert.False(response.IsSuccess);
            Assert.Equal(
                ResultStatus.Failed,
                response.Status);
            Assert.Null(response.Data);

            var issue = Assert.Single(response.Issues);
            Assert.Equal(
                "JsonsWorkspaceInputOutputError",
                issue.Code);

            Assert.Equal(2, writeCallCount);

            Assert.True(File.Exists(firstTargetPath));
            Assert.Equal(
                TwoTemplateProvider.FirstContent,
                await File.ReadAllTextAsync(firstTargetPath));

            Assert.False(File.Exists(secondTargetPath));

            Assert.Empty(
                Directory.GetFiles(
                    packageDirectoryPath,
                    "*.tmp",
                    SearchOption.AllDirectories));

            string[] remainingFiles =
                Directory.GetFiles(
                    packageDirectoryPath,
                    "*",
                    SearchOption.AllDirectories);

            Assert.Single(remainingFiles);
            Assert.Equal(
                firstTargetPath,
                remainingFiles[0]);
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
