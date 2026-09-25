using Afrowave.Toolbox.Essentials.Enums;
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

public sealed class FlexiblePartialWorkspaceFallbackRepairContractTests
{
    [Fact]
    public async Task Runtime_AfterPartialWorkspaceFallbackAndManualRepair_ActivatesCleanProjectContextWithoutRewritingCatalogs()
    {
        string rootDirectory = Path.Combine(
            Path.GetTempPath(),
            "Afrowave",
            "WhenItFails.Tests",
            Guid.NewGuid().ToString("N"));

        JsonsOptions options = new()
        {
            RootDirectory = rootDirectory,
            PackageDirectoryName = "WhenItFails"
        };

        try
        {
            int writes = 0;

            JsonsBootstrapper firstBootstrapper = new(
                new DefaultJsonsTemplateProvider(),
                async (temporaryPath, content, cancellationToken) =>
                {
                    writes++;

                    await File.WriteAllTextAsync(
                        temporaryPath,
                        content,
                        cancellationToken);

                    if (writes == 2)
                    {
                        throw new IOException(
                            "Deterministic second-template interruption.");
                    }
                });

            Response<JsonsBootstrapPayload> partial =
                await firstBootstrapper.EnsureWorkspaceAsync(options);

            Assert.False(partial.IsSuccess);
            Assert.Equal(2, writes);
            Assert.True(File.Exists(options.ErrorCatalogFilePath));
            Assert.False(File.Exists(options.CategoryCatalogFilePath));

            byte[] validErrorCatalog =
                await File.ReadAllBytesAsync(options.ErrorCatalogFilePath);

            byte[] invalidErrorCatalog =
                "{ \"errors\": [ malformed existing project file"u8.ToArray();

            await File.WriteAllBytesAsync(
                options.ErrorCatalogFilePath,
                invalidErrorCatalog);

            ServiceCollection services = new();
            services.AddWhenItFails(
                new WhenItFailsOptions
                {
                    Jsons = options,
                    InitializationMode = ErrorCatalogInitializationMode.Flexible
                });

            using ServiceProvider serviceProvider =
                services.BuildServiceProvider(
                    new ServiceProviderOptions
                    {
                        ValidateOnBuild = true,
                        ValidateScopes = true
                    });

            IErrorCatalogRuntime runtime =
                serviceProvider.GetRequiredService<IErrorCatalogRuntime>();

            IErrorCatalogContextStore contextStore =
                serviceProvider.GetRequiredService<IErrorCatalogContextStore>();

            Response<ErrorCatalogInitializationPayload> fallback =
                await runtime.InitializeAsync(options);

            Assert.True(fallback.IsSuccess);
            Assert.Equal(ResultStatus.SuccessWithWarnings, fallback.Status);
            Assert.NotNull(fallback.Data);
            Assert.Equal(
                ErrorCatalogContextSource.BuiltInDefaults,
                fallback.Data.ContextSource);
            Assert.True(fallback.Data.IsDegraded);
            Assert.True(fallback.Data.UsedFallback);
            Assert.False(fallback.Data.KeptPreviousContext);

            ErrorCatalogContext fallbackContext =
                Assert.IsType<ErrorCatalogContext>(contextStore.Current);

            Response<ErrorCatalogRuntimeStatus> fallbackStatusResponse =
                runtime.GetStatus();

            Assert.True(fallbackStatusResponse.IsSuccess);
            ErrorCatalogRuntimeStatus fallbackStatus =
                Assert.IsType<ErrorCatalogRuntimeStatus>(
                    fallbackStatusResponse.Data);

            Assert.Equal(
                ErrorCatalogRuntimeState.BuiltInFallback,
                fallbackStatus.State);
            Assert.True(fallbackStatus.IsConsistent);
            Assert.True(fallbackStatus.IsDegraded);
            Assert.False(string.IsNullOrWhiteSpace(fallbackStatus.RecoveryReasonCode));
            Assert.NotNull(fallbackStatus.RecoveryStatus);
            Assert.False(string.IsNullOrWhiteSpace(fallbackStatus.RecoveryMessage));

            Assert.Equal(
                invalidErrorCatalog,
                await File.ReadAllBytesAsync(options.ErrorCatalogFilePath));

            string[] projectPaths =
            [
                options.ErrorCatalogFilePath,
                options.CategoryCatalogFilePath,
                options.CodeGroupCatalogFilePath,
                options.OwnerCatalogFilePath,
                options.ProfilesFilePath
            ];

            Assert.All(projectPaths, path => Assert.True(File.Exists(path)));
            Assert.Empty(
                Directory.GetFiles(
                    options.PackageDirectoryPath,
                    "*.tmp",
                    SearchOption.AllDirectories));

            // Only a caller performs this repair. Built-in fallback must not
            // silently replace the existing malformed project catalog.
            await File.WriteAllBytesAsync(
                options.ErrorCatalogFilePath,
                validErrorCatalog);

            byte[][] projectBytesBeforeRetry = new byte[projectPaths.Length][];
            for (int i = 0; i < projectPaths.Length; i++)
            {
                projectBytesBeforeRetry[i] =
                    await File.ReadAllBytesAsync(projectPaths[i]);
            }

            Response<ErrorCatalogInitializationPayload> restored =
                await runtime.InitializeAsync(options);

            Assert.True(restored.IsSuccess);
            Assert.Equal(ResultStatus.Success, restored.Status);
            Assert.NotNull(restored.Data);
            Assert.Equal(
                ErrorCatalogContextSource.ProjectCatalog,
                restored.Data.ContextSource);
            Assert.False(restored.Data.IsDegraded);
            Assert.False(restored.Data.UsedFallback);
            Assert.False(restored.Data.KeptPreviousContext);
            Assert.True(restored.Data.Context.CrossValidationResult.IsValid);
            Assert.NotSame(fallbackContext, restored.Data.Context);
            Assert.Same(restored.Data.Context, contextStore.Current);

            Assert.Equal(5, restored.Data.Bootstrap.Files.Count);
            Assert.All(
                restored.Data.Bootstrap.Files,
                file =>
                {
                    Assert.True(file.AlreadyExisted);
                    Assert.True(file.Skipped);
                    Assert.False(file.Created);
                });

            Response<ErrorCatalogRuntimeStatus> projectStatusResponse =
                runtime.GetStatus();

            Assert.True(projectStatusResponse.IsSuccess);
            ErrorCatalogRuntimeStatus projectStatus =
                Assert.IsType<ErrorCatalogRuntimeStatus>(
                    projectStatusResponse.Data);

            Assert.NotSame(fallbackStatus, projectStatus);
            Assert.Equal(
                ErrorCatalogContextSource.ProjectCatalog,
                projectStatus.ContextSource);
            Assert.Equal(
                ErrorCatalogRuntimeState.ProjectCatalog,
                projectStatus.State);
            Assert.True(projectStatus.IsConsistent);
            Assert.False(projectStatus.IsDegraded);
            Assert.False(projectStatus.KeptPreviousContext);
            Assert.False(projectStatus.UsedFallback);
            Assert.Null(projectStatus.RecoveryReasonCode);
            Assert.Null(projectStatus.RecoveryStatus);
            Assert.Null(projectStatus.RecoveryMessage);

            var descriptor = runtime.FromId("AFW-GEN-0001");
            Assert.True(descriptor.IsSuccess);
            Assert.NotNull(descriptor.Data);
            Assert.Equal("AFW_GEN_0001", descriptor.Data.Id);
            Assert.Equal("UNKNOWNERROR", descriptor.Data.Name);

            for (int i = 0; i < projectPaths.Length; i++)
            {
                Assert.Equal(
                    projectBytesBeforeRetry[i],
                    await File.ReadAllBytesAsync(projectPaths[i]));
            }

            Assert.Empty(
                Directory.GetFiles(
                    options.PackageDirectoryPath,
                    "*.tmp",
                    SearchOption.AllDirectories));
        }
        finally
        {
            if (Directory.Exists(rootDirectory))
            {
                Directory.Delete(rootDirectory, recursive: true);
            }
        }
    }
}
