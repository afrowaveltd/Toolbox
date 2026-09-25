using System.Text.Json;
using System.Text.Json.Nodes;
using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Enums;
using Afrowave.Toolbox.WhenItFails.Initialization;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Runtime;
using Microsoft.Extensions.DependencyInjection;

namespace Afrowave.Toolbox.WhenItFails.Tests.Initialization;

public sealed class FlexibleSemanticPartialWorkspaceRecoveryContractTests
{
    [Fact]
    public async Task Runtime_WhenPartialWorkspaceHasUnknownCodeGroup_RetainsPreviousContextUntilManualRepair()
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

            IErrorCatalogContextStore store =
                serviceProvider.GetRequiredService<IErrorCatalogContextStore>();

            Response<ErrorCatalogInitializationPayload> initial =
                await runtime.InitializeAsync(options);

            Assert.True(initial.IsSuccess);
            Assert.NotNull(initial.Data);
            Assert.Equal(
                ErrorCatalogContextSource.ProjectCatalog,
                initial.Data.ContextSource);

            ErrorCatalogContext previousContext =
                Assert.IsType<ErrorCatalogContext>(store.Current);

            string[] catalogPaths =
            [
                options.ErrorCatalogFilePath,
                options.CategoryCatalogFilePath,
                options.CodeGroupCatalogFilePath,
                options.OwnerCatalogFilePath,
                options.ProfilesFilePath
            ];

            byte[] originalErrorBytes =
                await File.ReadAllBytesAsync(options.ErrorCatalogFilePath);

            // Keep a valid JSON document and error definition, but break
            // the relationship to the supporting code-group catalog.
            JsonObject document =
                Assert.IsType<JsonObject>(
                    JsonNode.Parse(originalErrorBytes));

            JsonArray errors =
                Assert.IsType<JsonArray>(document["errors"]);

            JsonObject firstError =
                Assert.IsType<JsonObject>(errors[0]);

            Assert.Equal(
                "GENERAL",
                firstError["codeGroup"]?.GetValue<string>());

            firstError["codeGroup"] = "MISSING_GROUP";

            byte[] inconsistentErrorBytes =
                System.Text.Encoding.UTF8.GetBytes(
                    document.ToJsonString(
                        new JsonSerializerOptions
                        {
                            WriteIndented = true
                        }));

            await File.WriteAllBytesAsync(
                options.ErrorCatalogFilePath,
                inconsistentErrorBytes);

            // A partial workspace is completed by bootstrap on retry,
            // but the invalid existing error catalog must not be replaced.
            File.Delete(options.CategoryCatalogFilePath);
            File.Delete(options.CodeGroupCatalogFilePath);
            File.Delete(options.OwnerCatalogFilePath);
            File.Delete(options.ProfilesFilePath);

            Response<ErrorCatalogInitializationPayload> recovered =
                await runtime.InitializeAsync(options);

            Assert.True(recovered.IsSuccess);
            Assert.Equal(ResultStatus.SuccessWithWarnings, recovered.Status);
            Assert.NotNull(recovered.Data);
            Assert.Equal(
                ErrorCatalogContextSource.PreviousContext,
                recovered.Data.ContextSource);
            Assert.True(recovered.Data.IsDegraded);
            Assert.True(recovered.Data.KeptPreviousContext);
            Assert.False(recovered.Data.UsedFallback);
            Assert.Same(previousContext, recovered.Data.Context);
            Assert.Same(previousContext, store.Current);

            Response<ErrorCatalogRuntimeStatus> degradedResponse =
                runtime.GetStatus();

            Assert.True(degradedResponse.IsSuccess);
            ErrorCatalogRuntimeStatus degraded =
                Assert.IsType<ErrorCatalogRuntimeStatus>(
                    degradedResponse.Data);

            Assert.True(degraded.IsConsistent);
            Assert.Equal(
                ErrorCatalogRuntimeState.PreviousContextRecovery,
                degraded.State);
            Assert.Equal(
                ErrorCatalogContextSource.PreviousContext,
                degraded.ContextSource);
            Assert.True(degraded.IsDegraded);
            Assert.True(degraded.KeptPreviousContext);
            Assert.False(degraded.UsedFallback);
            Assert.Equal(
                "UnknownErrorCodeGroup",
                degraded.RecoveryReasonCode);
            Assert.Equal(ResultStatus.Invalid, degraded.RecoveryStatus);
            Assert.False(string.IsNullOrWhiteSpace(degraded.RecoveryMessage));

            Assert.All(catalogPaths, path => Assert.True(File.Exists(path)));
            Assert.Equal(
                inconsistentErrorBytes,
                await File.ReadAllBytesAsync(options.ErrorCatalogFilePath));

            var retainedDescriptor = runtime.FromId("AFW-GEN-0001");

            Assert.True(retainedDescriptor.IsSuccess);
            Assert.NotNull(retainedDescriptor.Data);
            Assert.Equal("AFW_GEN_0001", retainedDescriptor.Data.Id);

            AssertNoTemporaryFiles(options);

            // An explicit caller repair is the only change permitted to
            // the invalid existing file before the next activation.
            await File.WriteAllBytesAsync(
                options.ErrorCatalogFilePath,
                originalErrorBytes);

            byte[][] expectedCatalogBytes = new byte[catalogPaths.Length][];
            for (int i = 0; i < catalogPaths.Length; i++)
            {
                expectedCatalogBytes[i] =
                    await File.ReadAllBytesAsync(catalogPaths[i]);
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
            Assert.False(restored.Data.KeptPreviousContext);
            Assert.False(restored.Data.UsedFallback);
            Assert.True(restored.Data.Context.CrossValidationResult.IsValid);
            Assert.NotSame(previousContext, restored.Data.Context);
            Assert.Same(restored.Data.Context, store.Current);

            Assert.Equal(5, restored.Data.Bootstrap.Files.Count);
            Assert.All(
                restored.Data.Bootstrap.Files,
                file =>
                {
                    Assert.True(file.AlreadyExisted);
                    Assert.True(file.Skipped);
                    Assert.False(file.Created);
                });

            Response<ErrorCatalogRuntimeStatus> restoredResponse =
                runtime.GetStatus();

            Assert.True(restoredResponse.IsSuccess);
            ErrorCatalogRuntimeStatus status =
                Assert.IsType<ErrorCatalogRuntimeStatus>(
                    restoredResponse.Data);

            Assert.NotSame(degraded, status);
            Assert.True(status.IsConsistent);
            Assert.Equal(ErrorCatalogRuntimeState.ProjectCatalog, status.State);
            Assert.Equal(
                ErrorCatalogContextSource.ProjectCatalog,
                status.ContextSource);
            Assert.False(status.IsDegraded);
            Assert.False(status.KeptPreviousContext);
            Assert.False(status.UsedFallback);
            Assert.Null(status.RecoveryReasonCode);
            Assert.Null(status.RecoveryStatus);
            Assert.Null(status.RecoveryMessage);

            for (int i = 0; i < catalogPaths.Length; i++)
            {
                Assert.Equal(
                    expectedCatalogBytes[i],
                    await File.ReadAllBytesAsync(catalogPaths[i]));
            }

            AssertNoTemporaryFiles(options);
        }
        finally
        {
            if (Directory.Exists(rootDirectory))
            {
                Directory.Delete(rootDirectory, recursive: true);
            }
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
}
