using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Descriptors;
using Afrowave.Toolbox.WhenItFails.Initialization;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Afrowave.Toolbox.WhenItFails.Tests.Integration;

public sealed class ErrorCatalogRuntimeIntegrationTests
{
    [Fact]
    public async Task Runtime_ShouldInitializeAndProvideCatalogOperations()
    {
        string rootDirectory = CreateTemporaryRootDirectory();

        try
        {
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
                serviceProvider.GetRequiredService<
                    IErrorCatalogRuntime>();

            JsonsOptions options = new()
            {
                RootDirectory = rootDirectory,
                PackageDirectoryName = "WhenItFails"
            };

            Response<ErrorCatalogInitializationPayload>
                initializationResponse =
                    await runtime.InitializeAsync(options);

            Assert.True(initializationResponse.IsSuccess);
            Assert.Equal(
                ResultStatus.Success,
                initializationResponse.Status);

            Assert.NotNull(initializationResponse.Data);
            Assert.NotNull(initializationResponse.Data.Bootstrap);
            Assert.NotNull(initializationResponse.Data.Context);

            Assert.True(
                Directory.Exists(
                    initializationResponse
                        .Data
                        .Bootstrap
                        .PackageDirectoryPath));

            Response<ErrorDescriptor> descriptorResponse =
                runtime.FromId("afw net 0001");

            Assert.True(descriptorResponse.IsSuccess);
            Assert.Equal(
                ResultStatus.Success,
                descriptorResponse.Status);

            Assert.NotNull(descriptorResponse.Data);

            Assert.Equal(
                "AFW_NET_0001",
                descriptorResponse.Data.Id);

            Assert.Equal(
                "NETWORKUNAVAILABLE",
                descriptorResponse.Data.Name);

            Assert.Equal(
                "NETWORK",
                descriptorResponse.Data.PrimaryCategory);

            Response<IReadOnlyList<ErrorDefinition>>
                profileResponse =
                    runtime.ResolveProfile("web");

            Assert.True(profileResponse.IsSuccess);
            Assert.Equal(
                ResultStatus.Success,
                profileResponse.Status);

            Assert.NotNull(profileResponse.Data);
            Assert.NotEmpty(profileResponse.Data);

            Assert.Contains(
                profileResponse.Data,
                error =>
                    error.Id == "AFW_NET_0001");

            Assert.DoesNotContain(
                profileResponse.Data,
                error =>
                    error.Tags.Contains(
                        "INTERNAL_ONLY",
                        StringComparer.OrdinalIgnoreCase));
        }
        finally
        {
            DeleteDirectoryIfExists(rootDirectory);
        }
    }

    [Fact]
    public async Task Runtime_ShouldInitializeFromRegisteredOptions()
    {
        string rootDirectory = CreateTemporaryRootDirectory();

        try
        {
            ServiceCollection services = new();

            services.AddWhenItFails(options =>
            {
                options.Jsons.RootDirectory =
                    rootDirectory;

                options.Jsons.PackageDirectoryName =
                    "ConfiguredWhenItFails";
            });

            using ServiceProvider serviceProvider =
                services.BuildServiceProvider(
                    new ServiceProviderOptions
                    {
                        ValidateOnBuild = true,
                        ValidateScopes = true
                    });

            IErrorCatalogRuntime runtime =
                serviceProvider.GetRequiredService<
                    IErrorCatalogRuntime>();

            Response<ErrorCatalogInitializationPayload>
                initializationResponse =
                    await runtime.InitializeAsync();

            Assert.True(
                initializationResponse.IsSuccess);

            Assert.Equal(
                ResultStatus.Success,
                initializationResponse.Status);

            Assert.NotNull(
                initializationResponse.Data);

            Assert.NotNull(
                initializationResponse.Data.Bootstrap);

            Assert.NotNull(
                initializationResponse.Data.Context);

            string expectedPackageDirectoryPath =
                Path.Combine(
                    rootDirectory,
                    "ConfiguredWhenItFails");

            Assert.Equal(
                expectedPackageDirectoryPath,
                initializationResponse
                    .Data
                    .Bootstrap
                    .PackageDirectoryPath);

            Assert.True(
                Directory.Exists(
                    expectedPackageDirectoryPath));

            Response<ErrorDescriptor> descriptorResponse =
                runtime.FromId(
                    "afw net 0001");

            Assert.True(
                descriptorResponse.IsSuccess);

            Assert.NotNull(
                descriptorResponse.Data);

            Assert.Equal(
                "AFW_NET_0001",
                descriptorResponse.Data.Id);

            Assert.Equal(
                "NETWORKUNAVAILABLE",
                descriptorResponse.Data.Name);

            Response<IReadOnlyList<ErrorDefinition>>
                profileResponse =
                    runtime.ResolveProfile(
                        "web");

            Assert.True(
                profileResponse.IsSuccess);

            Assert.NotNull(
                profileResponse.Data);

            Assert.Contains(
                profileResponse.Data,
                error =>
                    error.Id == "AFW_NET_0001");
        }
        finally
        {
            DeleteDirectoryIfExists(
                rootDirectory);
        }
    }


    private static string CreateTemporaryRootDirectory()
    {
        return Path.Combine(
            Path.GetTempPath(),
            $"when-it-fails-runtime-{Guid.NewGuid():N}");
    }

    private static void DeleteDirectoryIfExists(
        string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            return;
        }

        Directory.Delete(
            directoryPath,
            recursive: true);
    }
}
