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

public sealed class RepairedPartialWorkspaceInitializationContractTests
{
    [Fact]
    public async Task StrictFirstStart_AfterMalformedPartialWorkspaceIsRepaired_PublishesProjectWithoutRewritingOtherCatalogs()
    {
        string rootDirectory = CreateTemporaryRootDirectory();
        JsonsOptions options = CreateOptions(rootDirectory);

        try
        {
            await CreateOneFilePartialWorkspaceAsync(options);

            byte[] validErrorBytes =
                await File.ReadAllBytesAsync(options.ErrorCatalogFilePath);

            await File.WriteAllTextAsync(
                options.ErrorCatalogFilePath,
                "{ \"errors\": [ malformed catalog");

            using ServiceProvider services = CreateStrictServiceProvider(options);
            IErrorCatalogRuntime runtime =
                services.GetRequiredService<IErrorCatalogRuntime>();
            IErrorCatalogContextStore store =
                services.GetRequiredService<IErrorCatalogContextStore>();

            Response<ErrorCatalogInitializationPayload> failed =
                await runtime.InitializeAsync(options);

            Assert.False(failed.IsSuccess);
            Assert.Null(failed.Data);
            Assert.False(store.IsInitialized);
            Assert.False(runtime.GetStatus().IsSuccess);

            string[] paths = GetCatalogPaths(options);
            Assert.All(paths, path => Assert.True(File.Exists(path)));
            byte[][] otherCatalogBytes =
                await ReadFilesAsync(paths[1..]);

            // Only the user repairs the existing malformed project file.
            await File.WriteAllBytesAsync(
                options.ErrorCatalogFilePath,
                validErrorBytes);

            Response<ErrorCatalogInitializationPayload> recovered =
                await runtime.InitializeAsync(options);

            Assert.True(recovered.IsSuccess);
            Assert.NotNull(recovered.Data);
            Assert.Equal(
                ErrorCatalogContextSource.ProjectCatalog,
                recovered.Data.ContextSource);
            Assert.False(recovered.Data.IsDegraded);
            Assert.False(recovered.Data.UsedFallback);
            Assert.False(recovered.Data.KeptPreviousContext);
            Assert.True(recovered.Data.Context.CrossValidationResult.IsValid);
            Assert.Equal(5, recovered.Data.Bootstrap.Files.Count);
            Assert.All(
                recovered.Data.Bootstrap.Files,
                file =>
                {
                    Assert.True(file.AlreadyExisted);
                    Assert.True(file.Skipped);
                    Assert.False(file.Created);
                });

            Assert.Equal(
                validErrorBytes,
                await File.ReadAllBytesAsync(options.ErrorCatalogFilePath));
            await AssertBytesUnchangedAsync(paths[1..], otherCatalogBytes);
            Assert.Same(recovered.Data.Context, store.Current);

            Response<ErrorCatalogRuntimeStatus> status = runtime.GetStatus();
            Assert.True(status.IsSuccess);
            Assert.NotNull(status.Data);
            Assert.Equal(
                ErrorCatalogRuntimeState.ProjectCatalog,
                status.Data.State);
            Assert.True(status.Data.IsConsistent);
            Assert.False(status.Data.IsDegraded);

            var descriptor = runtime.FromId("AFW-GEN-0001");
            Assert.True(descriptor.IsSuccess);
            Assert.NotNull(descriptor.Data);
            Assert.Equal("AFW_GEN_0001", descriptor.Data.Id);
            AssertNoTemporaryFiles(options);
        }
        finally
        {
            DeleteDirectoryIfExists(rootDirectory);
        }
    }

    [Fact]
    public async Task StrictReinitialization_AfterManualRepair_PublishesNewContextAndPreservesAllProjectFiles()
    {
        string rootDirectory = CreateTemporaryRootDirectory();
        JsonsOptions options = CreateOptions(rootDirectory);

        try
        {
            using ServiceProvider services = CreateStrictServiceProvider(options);
            IErrorCatalogRuntime runtime =
                services.GetRequiredService<IErrorCatalogRuntime>();
            IErrorCatalogContextStore store =
                services.GetRequiredService<IErrorCatalogContextStore>();

            Response<ErrorCatalogInitializationPayload> initial =
                await runtime.InitializeAsync(options);

            Assert.True(initial.IsSuccess);
            Assert.NotNull(initial.Data);

            ErrorCatalogContext previousContext =
                Assert.IsType<ErrorCatalogContext>(store.Current);
            ErrorCatalogRuntimeStatus previousStatus =
                Assert.IsType<ErrorCatalogRuntimeStatus>(
                    runtime.GetStatus().Data);

            byte[] originalErrorBytes =
                await File.ReadAllBytesAsync(options.ErrorCatalogFilePath);

            await File.WriteAllTextAsync(
                options.ErrorCatalogFilePath,
                "{ \"errors\": [ malformed catalog");

            Response<ErrorCatalogInitializationPayload> failed =
                await runtime.InitializeAsync(options);

            Assert.False(failed.IsSuccess);
            Assert.Null(failed.Data);
            Assert.Same(previousContext, store.Current);
            Assert.Same(previousStatus, runtime.GetStatus().Data);

            await File.WriteAllBytesAsync(
                options.ErrorCatalogFilePath,
                originalErrorBytes);

            string[] paths = GetCatalogPaths(options);
            byte[][] bytesBeforeRetry = await ReadFilesAsync(paths);

            Response<ErrorCatalogInitializationPayload> recovered =
                await runtime.InitializeAsync(options);

            Assert.True(recovered.IsSuccess);
            Assert.NotNull(recovered.Data);
            Assert.False(recovered.Data.IsDegraded);
            Assert.Equal(
                ErrorCatalogContextSource.ProjectCatalog,
                recovered.Data.ContextSource);
            Assert.True(recovered.Data.Context.CrossValidationResult.IsValid);
            Assert.Equal(5, recovered.Data.Bootstrap.Files.Count);
            Assert.All(
                recovered.Data.Bootstrap.Files,
                file =>
                {
                    Assert.True(file.AlreadyExisted);
                    Assert.True(file.Skipped);
                    Assert.False(file.Created);
                });

            Assert.NotSame(previousContext, recovered.Data.Context);
            Assert.Same(recovered.Data.Context, store.Current);
            Assert.NotSame(previousStatus, runtime.GetStatus().Data);

            Response<ErrorCatalogRuntimeStatus> status = runtime.GetStatus();
            Assert.True(status.IsSuccess);
            Assert.NotNull(status.Data);
            Assert.Equal(
                ErrorCatalogRuntimeState.ProjectCatalog,
                status.Data.State);
            Assert.True(status.Data.IsConsistent);
            Assert.False(status.Data.IsDegraded);
            Assert.False(status.Data.KeptPreviousContext);
            Assert.False(status.Data.UsedFallback);

            var descriptor = runtime.FromName("UnknownError");
            Assert.True(descriptor.IsSuccess);
            Assert.NotNull(descriptor.Data);
            Assert.Equal("UNKNOWNERROR", descriptor.Data.Name);
            Assert.Equal("AFW_GEN_0001", descriptor.Data.Id);

            await AssertBytesUnchangedAsync(paths, bytesBeforeRetry);
            AssertNoTemporaryFiles(options);
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

        Response<JsonsBootstrapPayload> response =
            await bootstrapper.EnsureWorkspaceAsync(options);

        Assert.False(response.IsSuccess);
        Assert.Equal(2, writes);
        Assert.True(File.Exists(options.ErrorCatalogFilePath));
        Assert.False(File.Exists(options.CategoryCatalogFilePath));
        AssertNoTemporaryFiles(options);
    }

    private static string[] GetCatalogPaths(JsonsOptions options)
    {
        return
        [
            options.ErrorCatalogFilePath,
            options.CategoryCatalogFilePath,
            options.CodeGroupCatalogFilePath,
            options.OwnerCatalogFilePath,
            options.ProfilesFilePath
        ];
    }

    private static async Task<byte[][]> ReadFilesAsync(string[] paths)
    {
        byte[][] contents = new byte[paths.Length][];

        for (int i = 0; i < paths.Length; i++)
        {
            contents[i] = await File.ReadAllBytesAsync(paths[i]);
        }

        return contents;
    }

    private static async Task AssertBytesUnchangedAsync(
        string[] paths,
        byte[][] expectedContents)
    {
        Assert.Equal(paths.Length, expectedContents.Length);

        for (int i = 0; i < paths.Length; i++)
        {
            Assert.Equal(
                expectedContents[i],
                await File.ReadAllBytesAsync(paths[i]));
        }
    }

    private static void AssertNoTemporaryFiles(JsonsOptions options)
    {
        Assert.Empty(
            Directory.GetFiles(
                options.PackageDirectoryPath,
                "*.tmp",
                SearchOption.AllDirectories));
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

    private static JsonsOptions CreateOptions(string rootDirectory)
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

    private static void DeleteDirectoryIfExists(string directoryPath)
    {
        if (Directory.Exists(directoryPath))
        {
            Directory.Delete(directoryPath, recursive: true);
        }
    }
}
