using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Enums;
using Afrowave.Toolbox.WhenItFails.Initialization;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Runtime;
using Microsoft.Extensions.DependencyInjection;

namespace Afrowave.Toolbox.WhenItFails.Tests.Initialization;

public sealed class PartialWorkspaceRecoveryIntegrationContractTests
{
    [Fact]
    public async Task Initializer_AfterPartialBootstrapFailure_CompletesMissingCatalogsAndPublishesProjectContext()
    {
        string rootDirectory = CreateTemporaryRootDirectory();
        JsonsOptions options = CreateOptions(rootDirectory);

        try
        {
            string preservedFilePath =
                await CreatePartialWorkspaceByIoFailureAsync(options);

            byte[] preservedBytes =
                await AddHarmlessUserWhitespaceAsync(
                    preservedFilePath);

            ServiceCollection services = new();
            services.AddWhenItFails();

            using ServiceProvider serviceProvider =
                services.BuildServiceProvider(
                    new ServiceProviderOptions
                    {
                        ValidateOnBuild = true,
                        ValidateScopes = true
                    });

            IErrorCatalogInitializer initializer =
                serviceProvider.GetRequiredService<IErrorCatalogInitializer>();

            IErrorCatalogContextStore contextStore =
                serviceProvider.GetRequiredService<IErrorCatalogContextStore>();

            Response<ErrorCatalogInitializationPayload> response =
                await initializer.InitializeAsync(options);

            Assert.True(response.IsSuccess);
            Assert.NotNull(response.Data);

            Assert.Equal(
                ErrorCatalogContextSource.ProjectCatalog,
                response.Data.ContextSource);
            Assert.False(response.Data.IsDegraded);
            Assert.False(response.Data.KeptPreviousContext);
            Assert.False(response.Data.UsedFallback);

            AssertBootstrapCompletedPartialWorkspace(
                response.Data.Bootstrap,
                preservedFilePath);

            AssertAllCatalogFilesExist(options);

            Assert.Equal(
                preservedBytes,
                await File.ReadAllBytesAsync(preservedFilePath));

            Assert.True(
                response.Data.Context.CrossValidationResult.IsValid);

            Assert.NotNull(response.Data.Context.ErrorCatalog);
            Assert.NotNull(response.Data.Context.ErrorCatalogDocument);
            Assert.NotNull(response.Data.Context.CategoryCatalog);
            Assert.NotNull(response.Data.Context.CodeGroupCatalog);
            Assert.NotNull(response.Data.Context.OwnerCatalog);
            Assert.NotNull(response.Data.Context.ProfileCatalog);

            Response<ErrorCatalogContext> stored =
                contextStore.GetCurrent();

            Assert.True(stored.IsSuccess);
            Assert.Same(
                response.Data.Context,
                stored.Data);

            Assert.Empty(
                Directory.GetFiles(
                    options.PackageDirectoryPath,
                    "*.tmp",
                    SearchOption.AllDirectories));
        }
        finally
        {
            DeleteDirectoryIfExists(rootDirectory);
        }
    }

    [Fact]
    public async Task Runtime_AfterPartialBootstrapCancellation_CompletesWorkspaceAndActivatesUsableProjectCatalog()
    {
        string rootDirectory = CreateTemporaryRootDirectory();
        JsonsOptions options = CreateOptions(rootDirectory);

        try
        {
            string preservedFilePath =
                await CreatePartialWorkspaceByCancellationAsync(options);

            byte[] preservedBytes =
                await AddHarmlessUserWhitespaceAsync(
                    preservedFilePath);

            ServiceCollection services = new();
            services.AddWhenItFails();

            using ServiceProvider serviceProvider =
                services.BuildServiceProvider(
                    new ServiceProviderOptions
                    {
                        ValidateOnBuild = true,
                        ValidateScopes = true
                    });

            IErrorCatalogRuntime runtime =
                serviceProvider.GetRequiredService<IErrorCatalogRuntime>();

            Response<ErrorCatalogInitializationPayload> initialization =
                await runtime.InitializeAsync(options);

            Assert.True(initialization.IsSuccess);
            Assert.NotNull(initialization.Data);

            Assert.Equal(
                ErrorCatalogContextSource.ProjectCatalog,
                initialization.Data.ContextSource);
            Assert.False(initialization.Data.IsDegraded);
            Assert.False(initialization.Data.KeptPreviousContext);
            Assert.False(initialization.Data.UsedFallback);

            AssertBootstrapCompletedPartialWorkspace(
                initialization.Data.Bootstrap,
                preservedFilePath);

            AssertAllCatalogFilesExist(options);

            Assert.Equal(
                preservedBytes,
                await File.ReadAllBytesAsync(preservedFilePath));

            Response<ErrorCatalogRuntimeStatus> statusResponse =
                runtime.GetStatus();

            Assert.True(statusResponse.IsSuccess);
            Assert.NotNull(statusResponse.Data);
            Assert.Equal(
                ErrorCatalogRuntimeState.ProjectCatalog,
                statusResponse.Data.State);
            Assert.Equal(
                ErrorCatalogContextSource.ProjectCatalog,
                statusResponse.Data.ContextSource);
            Assert.True(statusResponse.Data.IsConsistent);
            Assert.False(statusResponse.Data.IsDegraded);
            Assert.False(statusResponse.Data.KeptPreviousContext);
            Assert.False(statusResponse.Data.UsedFallback);
            Assert.Equal(
                options.PackageDirectoryPath,
                statusResponse.Data.PackageDirectoryPath);

            var descriptorById =
                runtime.FromId("AFW-GEN-0001");

            Assert.True(descriptorById.IsSuccess);
            Assert.NotNull(descriptorById.Data);
            Assert.Equal(
                "UnknownError",
                descriptorById.Data.Name);

            var descriptorByName =
                runtime.FromName("UnknownError");

            Assert.True(descriptorByName.IsSuccess);
            Assert.NotNull(descriptorByName.Data);
            Assert.Equal(
                "AFW-GEN-0001",
                descriptorByName.Data.Id);

            Assert.Empty(
                Directory.GetFiles(
                    options.PackageDirectoryPath,
                    "*.tmp",
                    SearchOption.AllDirectories));
        }
        finally
        {
            DeleteDirectoryIfExists(rootDirectory);
        }
    }

    private static async Task<string> CreatePartialWorkspaceByIoFailureAsync(
        JsonsOptions options)
    {
        int writeCallCount = 0;

        JsonsBootstrapper bootstrapper = new(
            new DefaultJsonsTemplateProvider(),
            async (temporaryFilePath, content, cancellationToken) =>
            {
                writeCallCount++;

                await File.WriteAllTextAsync(
                    temporaryFilePath,
                    content,
                    cancellationToken);

                if (writeCallCount == 2)
                {
                    throw new IOException(
                        "Deterministic second-template write failure.");
                }
            });

        Response<JsonsBootstrapPayload> response =
            await bootstrapper.EnsureWorkspaceAsync(options);

        Assert.False(response.IsSuccess);
        Assert.Null(response.Data);
        Assert.Equal(2, writeCallCount);

        return AssertSinglePublishedCatalog(options);
    }

    private static async Task<string> CreatePartialWorkspaceByCancellationAsync(
        JsonsOptions options)
    {
        using CancellationTokenSource cancellationSource = new();

        int writeCallCount = 0;

        JsonsBootstrapper bootstrapper = new(
            new DefaultJsonsTemplateProvider(),
            async (temporaryFilePath, content, cancellationToken) =>
            {
                writeCallCount++;

                await File.WriteAllTextAsync(
                    temporaryFilePath,
                    content,
                    CancellationToken.None);

                if (writeCallCount == 2)
                {
                    cancellationSource.Cancel();
                }
            });

        OperationCanceledException exception =
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => bootstrapper.EnsureWorkspaceAsync(
                    options,
                    cancellationSource.Token));

        Assert.Equal(
            cancellationSource.Token,
            exception.CancellationToken);

        Assert.Equal(2, writeCallCount);

        return AssertSinglePublishedCatalog(options);
    }

    private static string AssertSinglePublishedCatalog(
        JsonsOptions options)
    {
        Assert.True(
            Directory.Exists(
                options.PackageDirectoryPath));

        Assert.Empty(
            Directory.GetFiles(
                options.PackageDirectoryPath,
                "*.tmp",
                SearchOption.AllDirectories));

        string[] files =
            Directory.GetFiles(
                options.PackageDirectoryPath,
                "*",
                SearchOption.AllDirectories);

        return Assert.Single(files);
    }

    private static async Task<byte[]> AddHarmlessUserWhitespaceAsync(
        string filePath)
    {
        string content =
            await File.ReadAllTextAsync(filePath);

        string editedContent =
            content + Environment.NewLine + "   " + Environment.NewLine;

        await File.WriteAllTextAsync(
            filePath,
            editedContent);

        return await File.ReadAllBytesAsync(filePath);
    }

    private static void AssertBootstrapCompletedPartialWorkspace(
        JsonsBootstrapPayload bootstrap,
        string preservedFilePath)
    {
        Assert.Equal(5, bootstrap.Files.Count);

        JsonsBootstrapFileResult preserved =
            Assert.Single(
                bootstrap.Files,
                file => file.TargetFilePath == preservedFilePath);

        Assert.True(preserved.AlreadyExisted);
        Assert.False(preserved.Created);
        Assert.True(preserved.Skipped);

        JsonsBootstrapFileResult[] created =
            bootstrap.Files
                .Where(file => file.TargetFilePath != preservedFilePath)
                .ToArray();

        Assert.Equal(4, created.Length);

        Assert.All(
            created,
            file =>
            {
                Assert.False(file.AlreadyExisted);
                Assert.True(file.Created);
                Assert.False(file.Skipped);
            });
    }

    private static void AssertAllCatalogFilesExist(
        JsonsOptions options)
    {
        Assert.True(File.Exists(options.ErrorCatalogFilePath));
        Assert.True(File.Exists(options.CategoryCatalogFilePath));
        Assert.True(File.Exists(options.CodeGroupCatalogFilePath));
        Assert.True(File.Exists(options.OwnerCatalogFilePath));
        Assert.True(File.Exists(options.ProfilesFilePath));
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
}
