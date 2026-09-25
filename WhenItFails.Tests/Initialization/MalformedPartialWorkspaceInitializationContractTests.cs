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

public sealed class MalformedPartialWorkspaceInitializationContractTests
{
    [Fact]
    public async Task StrictFirstStart_WhenPreservedPartialCatalogIsMalformed_CompletesMissingFilesWithoutPublishingContext()
    {
        string rootDirectory = CreateTemporaryRootDirectory();
        JsonsOptions options = CreateOptions(rootDirectory);

        try
        {
            await CreateOneFilePartialWorkspaceAsync(options);

            byte[] malformedBytes =
                "{\"errors\": [ this is intentionally invalid JSON"u8.ToArray();

            await File.WriteAllBytesAsync(
                options.ErrorCatalogFilePath,
                malformedBytes);

            using ServiceProvider serviceProvider =
                CreateStrictServiceProvider(options);

            IErrorCatalogRuntime runtime =
                serviceProvider.GetRequiredService<IErrorCatalogRuntime>();

            IErrorCatalogContextStore store =
                serviceProvider.GetRequiredService<IErrorCatalogContextStore>();

            Response<ErrorCatalogInitializationPayload> result =
                await runtime.InitializeAsync(options);

            Assert.False(result.IsSuccess);
            Assert.Null(result.Data);
            Assert.False(store.IsInitialized);
            Assert.Null(store.Current);

            Assert.False(runtime.GetCurrentContext().IsSuccess);
            Assert.False(runtime.GetStatus().IsSuccess);

            Assert.Equal(
                malformedBytes,
                await File.ReadAllBytesAsync(options.ErrorCatalogFilePath));

            AssertOtherFourCatalogsExist(options);
            AssertNoTemporaryFiles(options);
        }
        finally
        {
            DeleteDirectoryIfExists(rootDirectory);
        }
    }

    [Fact]
    public async Task StrictReinitialization_WhenPreservedPartialCatalogIsMalformed_RetainsPreviousContextAndStatus()
    {
        string rootDirectory = CreateTemporaryRootDirectory();
        JsonsOptions options = CreateOptions(rootDirectory);

        try
        {
            using ServiceProvider serviceProvider =
                CreateStrictServiceProvider(options);

            IErrorCatalogRuntime runtime =
                serviceProvider.GetRequiredService<IErrorCatalogRuntime>();

            IErrorCatalogContextStore store =
                serviceProvider.GetRequiredService<IErrorCatalogContextStore>();

            Response<ErrorCatalogInitializationPayload> initial =
                await runtime.InitializeAsync(options);

            Assert.True(initial.IsSuccess);
            Assert.NotNull(initial.Data);

            ErrorCatalogContext previousContext =
                Assert.IsType<ErrorCatalogContext>(store.Current);

            ErrorCatalogRuntimeStatus previousStatus =
                Assert.IsType<ErrorCatalogRuntimeStatus>(
                    runtime.GetStatus().Data);

            Assert.Equal(
                ErrorCatalogRuntimeState.ProjectCatalog,
                previousStatus.State);

            // Model a subsequent partial project workspace: preserve the
            // existing catalog, remove only the four other project files.
            File.Delete(options.CategoryCatalogFilePath);
            File.Delete(options.CodeGroupCatalogFilePath);
            File.Delete(options.OwnerCatalogFilePath);
            File.Delete(options.ProfilesFilePath);

            byte[] malformedBytes =
                "{\"errors\": [ this is intentionally invalid JSON"u8.ToArray();

            await File.WriteAllBytesAsync(
                options.ErrorCatalogFilePath,
                malformedBytes);

            Response<ErrorCatalogInitializationPayload> failed =
                await runtime.InitializeAsync(options);

            Assert.False(failed.IsSuccess);
            Assert.Null(failed.Data);

            Assert.Same(previousContext, store.Current);
            Assert.Same(previousContext, runtime.GetCurrentContext().Data);
            Assert.Same(previousStatus, runtime.GetStatus().Data);

            Assert.Equal(
                malformedBytes,
                await File.ReadAllBytesAsync(options.ErrorCatalogFilePath));

            AssertOtherFourCatalogsExist(options);
            AssertNoTemporaryFiles(options);

            var descriptor = runtime.FromId("AFW-GEN-0001");

            Assert.True(descriptor.IsSuccess);
            Assert.NotNull(descriptor.Data);
            Assert.Equal("AFW_GEN_0001", descriptor.Data.Id);
        }
        finally
        {
            DeleteDirectoryIfExists(rootDirectory);
        }
    }

    private static async Task CreateOneFilePartialWorkspaceAsync(
        JsonsOptions options)
    {
        int writes = 0;

        JsonsBootstrapper bootstrapper = new(
            new DefaultJsonsTemplateProvider(),
            async (temporaryFilePath, content, cancellationToken) =>
            {
                writes++;

                await File.WriteAllTextAsync(
                    temporaryFilePath,
                    content,
                    cancellationToken);

                if (writes == 2)
                {
                    throw new IOException(
                        "Deterministic second-template interruption.");
                }
            });

        Response<JsonsBootstrapPayload> result =
            await bootstrapper.EnsureWorkspaceAsync(options);

        Assert.False(result.IsSuccess);
        Assert.Equal(2, writes);

        Assert.True(File.Exists(options.ErrorCatalogFilePath));
        Assert.False(File.Exists(options.CategoryCatalogFilePath));
        Assert.False(File.Exists(options.CodeGroupCatalogFilePath));
        Assert.False(File.Exists(options.OwnerCatalogFilePath));
        Assert.False(File.Exists(options.ProfilesFilePath));
        AssertNoTemporaryFiles(options);
    }

    private static ServiceProvider CreateStrictServiceProvider(
        JsonsOptions options)
    {
        ServiceCollection services = new();

        services.AddWhenItFails(
            new WhenItFailsOptions
            {
                InitializationMode = ErrorCatalogInitializationMode.Strict,
                Jsons = options
            });

        return services.BuildServiceProvider(
            new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });
    }

    private static void AssertOtherFourCatalogsExist(
        JsonsOptions options)
    {
        Assert.True(File.Exists(options.CategoryCatalogFilePath));
        Assert.True(File.Exists(options.CodeGroupCatalogFilePath));
        Assert.True(File.Exists(options.OwnerCatalogFilePath));
        Assert.True(File.Exists(options.ProfilesFilePath));
    }

    private static void AssertNoTemporaryFiles(
        JsonsOptions options)
    {
        Assert.Empty(
            Directory.GetFiles(
                options.PackageDirectoryPath,
                "*.tmp",
                SearchOption.AllDirectories));
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
