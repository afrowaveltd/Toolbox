using System.Text.Json;
using System.Text.Json.Nodes;
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

public sealed class SemanticallyInvalidPartialWorkspaceRecoveryContractTests
{
    [Fact]
    public async Task StrictFirstStart_WhenExistingCatalogReferencesMissingCodeGroup_RejectsContextUntilCallerRepairsReference()
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

            JsonsBootstrapper partialBootstrapper = new(
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
                await partialBootstrapper.EnsureWorkspaceAsync(options);

            Assert.False(partial.IsSuccess);
            Assert.Equal(2, writes);
            Assert.True(File.Exists(options.ErrorCatalogFilePath));
            Assert.False(File.Exists(options.CategoryCatalogFilePath));

            byte[] originalErrorBytes =
                await File.ReadAllBytesAsync(options.ErrorCatalogFilePath);

            JsonObject document =
                Assert.IsType<JsonObject>(
                    JsonNode.Parse(originalErrorBytes));

            JsonArray errors =
                Assert.IsType<JsonArray>(document["errors"]);

            JsonObject firstError =
                Assert.IsType<JsonObject>(errors[0]);

            Assert.Equal("GENERAL", firstError["codeGroup"]?.GetValue<string>());

            // The JSON remains well formed and the error retains a valid
            // individual shape, but its code group cannot be resolved
            // against the bundled supporting code-group catalog.
            firstError["codeGroup"] = "MISSING_GROUP";

            byte[] semanticallyInvalidBytes =
                System.Text.Encoding.UTF8.GetBytes(
                    document.ToJsonString(
                        new JsonSerializerOptions
                        {
                            WriteIndented = true
                        }));

            await File.WriteAllBytesAsync(
                options.ErrorCatalogFilePath,
                semanticallyInvalidBytes);

            using ServiceProvider services =
                CreateStrictServiceProvider(options);

            IErrorCatalogRuntime runtime =
                services.GetRequiredService<IErrorCatalogRuntime>();

            IErrorCatalogContextStore store =
                services.GetRequiredService<IErrorCatalogContextStore>();

            Response<ErrorCatalogInitializationPayload> rejected =
                await runtime.InitializeAsync(options);

            Assert.False(rejected.IsSuccess);
            Assert.Equal(ResultStatus.Invalid, rejected.Status);
            Assert.Null(rejected.Data);
            Assert.Contains(
                rejected.Issues,
                issue => issue.Code == "UnknownErrorCodeGroup");

            Assert.False(store.IsInitialized);
            Assert.Null(store.Current);
            Assert.False(runtime.GetCurrentContext().IsSuccess);
            Assert.False(runtime.GetStatus().IsSuccess);

            string[] catalogPaths =
            [
                options.ErrorCatalogFilePath,
                options.CategoryCatalogFilePath,
                options.CodeGroupCatalogFilePath,
                options.OwnerCatalogFilePath,
                options.ProfilesFilePath
            ];

            Assert.All(
                catalogPaths,
                path => Assert.True(File.Exists(path)));

            Assert.Equal(
                semanticallyInvalidBytes,
                await File.ReadAllBytesAsync(options.ErrorCatalogFilePath));

            byte[][] otherCatalogBytes = new byte[4][];
            for (int i = 1; i < catalogPaths.Length; i++)
            {
                otherCatalogBytes[i - 1] =
                    await File.ReadAllBytesAsync(catalogPaths[i]);
            }

            AssertNoTemporaryFiles(options);

            // Only a caller repairs the already-existing catalog. The next
            // bootstrap pass must skip all five complete project files.
            await File.WriteAllBytesAsync(
                options.ErrorCatalogFilePath,
                originalErrorBytes);

            Response<ErrorCatalogInitializationPayload> repaired =
                await runtime.InitializeAsync(options);

            Assert.True(repaired.IsSuccess);
            Assert.Equal(ResultStatus.Success, repaired.Status);
            Assert.NotNull(repaired.Data);
            Assert.Equal(
                ErrorCatalogContextSource.ProjectCatalog,
                repaired.Data.ContextSource);
            Assert.False(repaired.Data.IsDegraded);
            Assert.True(repaired.Data.Context.CrossValidationResult.IsValid);
            Assert.Same(repaired.Data.Context, store.Current);

            Assert.Equal(5, repaired.Data.Bootstrap.Files.Count);
            Assert.All(
                repaired.Data.Bootstrap.Files,
                file =>
                {
                    Assert.True(file.AlreadyExisted);
                    Assert.True(file.Skipped);
                    Assert.False(file.Created);
                });

            Response<ErrorCatalogRuntimeStatus> status = runtime.GetStatus();
            Assert.True(status.IsSuccess);
            Assert.NotNull(status.Data);
            Assert.Equal(
                ErrorCatalogRuntimeState.ProjectCatalog,
                status.Data.State);
            Assert.True(status.Data.IsConsistent);
            Assert.False(status.Data.IsDegraded);
            Assert.Null(status.Data.RecoveryReasonCode);
            Assert.Null(status.Data.RecoveryStatus);
            Assert.Null(status.Data.RecoveryMessage);

            Assert.Equal(
                originalErrorBytes,
                await File.ReadAllBytesAsync(options.ErrorCatalogFilePath));

            for (int i = 1; i < catalogPaths.Length; i++)
            {
                Assert.Equal(
                    otherCatalogBytes[i - 1],
                    await File.ReadAllBytesAsync(catalogPaths[i]));
            }

            var descriptor = runtime.FromId("AFW-GEN-0001");
            Assert.True(descriptor.IsSuccess);
            Assert.NotNull(descriptor.Data);
            Assert.Equal("AFW_GEN_0001", descriptor.Data.Id);
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

    private static void AssertNoTemporaryFiles(JsonsOptions options)
    {
        Assert.Empty(
            Directory.GetFiles(
                options.PackageDirectoryPath,
                "*.tmp",
                SearchOption.AllDirectories));
    }
}
