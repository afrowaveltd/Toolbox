using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Descriptors;
using Afrowave.Toolbox.WhenItFails.Enums;
using Afrowave.Toolbox.WhenItFails.Initialization;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Runtime;
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

    [Fact]
    public async Task Runtime_ShouldActivateBuiltInDefaults_WhenProjectCatalogIsInvalid()
    {
        string rootDirectory = CreateTemporaryRootDirectory();

        JsonsOptions jsonsOptions = new()
        {
            RootDirectory = rootDirectory,
            PackageDirectoryName = "BrokenWhenItFails"
        };

        try
        {
            Directory.CreateDirectory(
                jsonsOptions.PackageDirectoryPath);

            const string invalidProjectCatalog =
                "{ this is not valid json";

            await File.WriteAllTextAsync(
                jsonsOptions.ErrorCatalogFilePath,
                invalidProjectCatalog);

            ServiceCollection services = new();

            services.AddWhenItFails(
                new WhenItFailsOptions
                {
                    Jsons = jsonsOptions,

                    InitializationMode =
                        ErrorCatalogInitializationMode.Flexible
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
                ResultStatus.SuccessWithWarnings,
                initializationResponse.Status);

            Assert.True(
                initializationResponse.HasWarnings);

            Assert.NotNull(
                initializationResponse.Data);

            Assert.Equal(
                ErrorCatalogContextSource.BuiltInDefaults,
                initializationResponse.Data.ContextSource);

            Assert.True(
                initializationResponse.Data.UsedFallback);

            Assert.False(
                initializationResponse.Data.KeptPreviousContext);

            Assert.True(
                initializationResponse.Data.IsDegraded);

            Assert.Equal(
                "WIF_DEFAULT_FALLBACK_ACTIVATED",
                initializationResponse.Issues[0].Code);

            Assert.True(
                initializationResponse.Metadata.TryGet(
                    "WhenItFails.RecoveryReasonCode",
                    out string? recoveryReasonCode));

            Assert.False(
                string.IsNullOrWhiteSpace(
                    recoveryReasonCode));

            Assert.True(
                initializationResponse.Metadata.TryGet(
                    "WhenItFails.RecoveryStatus",
                    out string? recoveryStatus));

            Assert.False(
                string.IsNullOrWhiteSpace(
                    recoveryStatus));

            Assert.True(
                initializationResponse.Metadata.TryGet(
                    "WhenItFails.RecoveryMessage",
                    out string? recoveryMessage));

            Assert.False(
                string.IsNullOrWhiteSpace(
                    recoveryMessage));

            Response<ErrorCatalogRuntimeStatus> statusResponse =
                runtime.GetStatus();

            Assert.True(
                statusResponse.IsSuccess);

            Assert.Equal(
                ResultStatus.Success,
                statusResponse.Status);

            Assert.NotNull(
                statusResponse.Data);

            Assert.Equal(
                ErrorCatalogContextSource.BuiltInDefaults,
                statusResponse.Data.ContextSource);

            Assert.True(
                statusResponse.Data.IsDegraded);

            Assert.False(
                statusResponse.Data.KeptPreviousContext);

            Assert.True(
                statusResponse.Data.UsedFallback);

            Assert.Equal(
                recoveryReasonCode,
                statusResponse.Data.RecoveryReasonCode);

            Assert.Equal(
                recoveryStatus,
                statusResponse.Data.RecoveryStatus?.ToString());

            Assert.Equal(
                recoveryMessage,
                statusResponse.Data.RecoveryMessage);

            Assert.Equal(
                jsonsOptions.PackageDirectoryPath,
                statusResponse.Data.PackageDirectoryPath);

            Assert.NotEqual(
                default,
                statusResponse.Data.ActivatedAtUtc);




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

            string preservedProjectCatalog =
                await File.ReadAllTextAsync(
                    jsonsOptions.ErrorCatalogFilePath);

            Assert.Equal(
                invalidProjectCatalog,
                preservedProjectCatalog);
        }
        finally
        {
            DeleteDirectoryIfExists(
                rootDirectory);
        }
    }


    [Fact]
    public async Task Runtime_ShouldResetToBuiltInDefaults_WithoutChangingProjectFiles()
    {
        string rootDirectory = CreateTemporaryRootDirectory();

        JsonsOptions jsonsOptions = new()
        {
            RootDirectory = rootDirectory,
            PackageDirectoryName = "ResetTestWhenItFails"
        };

        try
        {
            ServiceCollection services = new();

            services.AddWhenItFails(
                new WhenItFailsOptions
                {
                    Jsons = jsonsOptions,
                    InitializationMode =
                        ErrorCatalogInitializationMode.Flexible
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

            Assert.Equal(
                ErrorCatalogContextSource.ProjectCatalog,
                initializationResponse.Data.ContextSource);

            Dictionary<string, string> originalFileContents =
                Directory
                    .EnumerateFiles(
                        jsonsOptions.PackageDirectoryPath,
                        "*.json",
                        SearchOption.TopDirectoryOnly)
                    .ToDictionary(
                        filePath =>
                            Path.GetFileName(filePath),
                        filePath =>
                            File.ReadAllText(filePath),
                        StringComparer.OrdinalIgnoreCase);

            Assert.NotEmpty(
                originalFileContents);

            Response<ErrorCatalogInitializationPayload>
                resetResponse =
                    await runtime.ResetToDefaultsAsync();

            Assert.True(
                resetResponse.IsSuccess);

            Assert.Equal(
                ResultStatus.Success,
                resetResponse.Status);

            Assert.NotNull(
                resetResponse.Data);

            Assert.Equal(
                ErrorCatalogContextSource.BuiltInDefaults,
                resetResponse.Data.ContextSource);

            Assert.False(
                resetResponse.Data.KeptPreviousContext);

            Assert.False(
                resetResponse.Data.UsedFallback);

            Assert.False(
                resetResponse.Data.IsDegraded);

            foreach (KeyValuePair<string, string> originalFile
                in originalFileContents)
            {
                string currentFilePath =
                    Path.Combine(
                        jsonsOptions.PackageDirectoryPath,
                        originalFile.Key);

                Assert.True(
                    File.Exists(currentFilePath));

                string currentContent =
                    await File.ReadAllTextAsync(
                        currentFilePath);

                Assert.Equal(
                    originalFile.Value,
                    currentContent);
            }

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
        }
        finally
        {
            DeleteDirectoryIfExists(
                rootDirectory);
        }
    }


    [Fact]
    public async Task Runtime_ShouldReturnFailureAndKeepPreviousState_InStrictMode()
    {
        string rootDirectory = CreateTemporaryRootDirectory();

        JsonsOptions jsonsOptions = new()
        {
            RootDirectory = rootDirectory,
            PackageDirectoryName = "StrictModeWhenItFails"
        };

        try
        {
            ServiceCollection services = new();

            services.AddWhenItFails(
                new WhenItFailsOptions
                {
                    Jsons = jsonsOptions,

                    InitializationMode =
                        ErrorCatalogInitializationMode.Strict
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
                firstInitializationResponse =
                    await runtime.InitializeAsync();

            Assert.True(
                firstInitializationResponse.IsSuccess);

            Assert.Equal(
                ResultStatus.Success,
                firstInitializationResponse.Status);

            Assert.NotNull(
                firstInitializationResponse.Data);

            Assert.Equal(
                ErrorCatalogContextSource.ProjectCatalog,
                firstInitializationResponse.Data.ContextSource);

            Response<ErrorCatalogContext> contextBeforeFailure =
                runtime.GetCurrentContext();

            Assert.True(
                contextBeforeFailure.IsSuccess);

            Assert.NotNull(
                contextBeforeFailure.Data);

            Response<ErrorCatalogRuntimeStatus> statusBeforeFailure =
                runtime.GetStatus();

            Assert.True(
                statusBeforeFailure.IsSuccess);

            Assert.NotNull(
                statusBeforeFailure.Data);

            const string invalidProjectCatalog =
                "{ strict mode catalog is now damaged";

            await File.WriteAllTextAsync(
                jsonsOptions.ErrorCatalogFilePath,
                invalidProjectCatalog);

            Response<ErrorCatalogInitializationPayload>
                secondInitializationResponse =
                    await runtime.InitializeAsync();

            Assert.False(
                secondInitializationResponse.IsSuccess);

            Assert.NotEqual(
                ResultStatus.Success,
                secondInitializationResponse.Status);

            Assert.NotEmpty(
                secondInitializationResponse.Issues);

            Response<ErrorCatalogContext> contextAfterFailure =
                runtime.GetCurrentContext();

            Assert.True(
                contextAfterFailure.IsSuccess);

            Assert.NotNull(
                contextAfterFailure.Data);

            Assert.Same(
                contextBeforeFailure.Data,
                contextAfterFailure.Data);

            Response<ErrorCatalogRuntimeStatus> statusAfterFailure =
                runtime.GetStatus();

            Assert.True(
                statusAfterFailure.IsSuccess);

            Assert.NotNull(
                statusAfterFailure.Data);

            Assert.Same(
                statusBeforeFailure.Data,
                statusAfterFailure.Data);

            Assert.Equal(
                ErrorCatalogContextSource.ProjectCatalog,
                statusAfterFailure.Data.ContextSource);

            Assert.False(
                statusAfterFailure.Data.IsDegraded);

            Assert.False(
                statusAfterFailure.Data.KeptPreviousContext);

            Assert.False(
                statusAfterFailure.Data.UsedFallback);

            Assert.Null(
                statusAfterFailure.Data.RecoveryReasonCode);

            Assert.Null(
                statusAfterFailure.Data.RecoveryStatus);

            Assert.Null(
                statusAfterFailure.Data.RecoveryMessage);

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

            string preservedInvalidCatalog =
                await File.ReadAllTextAsync(
                    jsonsOptions.ErrorCatalogFilePath);

            Assert.Equal(
                invalidProjectCatalog,
                preservedInvalidCatalog);
        }
        finally
        {
            DeleteDirectoryIfExists(
                rootDirectory);
        }
    }

    [Fact]
    public async Task Runtime_ShouldReturnToProjectCatalog_AfterDamagedCatalogIsRestored()
    {
        string rootDirectory = CreateTemporaryRootDirectory();

        JsonsOptions jsonsOptions = new()
        {
            RootDirectory = rootDirectory,
            PackageDirectoryName = "RecoveredWhenItFails"
        };

        try
        {
            ServiceCollection services = new();

            services.AddWhenItFails(
                new WhenItFailsOptions
                {
                    Jsons = jsonsOptions,

                    InitializationMode =
                        ErrorCatalogInitializationMode.Flexible
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
                firstInitializationResponse =
                    await runtime.InitializeAsync();

            Assert.True(
                firstInitializationResponse.IsSuccess);

            Assert.NotNull(
                firstInitializationResponse.Data);

            Assert.Equal(
                ErrorCatalogContextSource.ProjectCatalog,
                firstInitializationResponse.Data.ContextSource);

            string validProjectCatalog =
                await File.ReadAllTextAsync(
                    jsonsOptions.ErrorCatalogFilePath);

            Response<ErrorCatalogContext> originalContextResponse =
                runtime.GetCurrentContext();

            Assert.True(
                originalContextResponse.IsSuccess);

            Assert.NotNull(
                originalContextResponse.Data);

            await File.WriteAllTextAsync(
                jsonsOptions.ErrorCatalogFilePath,
                "{ damaged project catalog");

            Response<ErrorCatalogInitializationPayload>
                degradedInitializationResponse =
                    await runtime.InitializeAsync();

            Assert.True(
                degradedInitializationResponse.IsSuccess);

            Assert.NotNull(
                degradedInitializationResponse.Data);

            Assert.Equal(
                ResultStatus.SuccessWithWarnings,
                degradedInitializationResponse.Status);

            Assert.Equal(
                ErrorCatalogContextSource.PreviousContext,
                degradedInitializationResponse.Data.ContextSource);

            Assert.True(
                degradedInitializationResponse.Data.IsDegraded);

            Response<ErrorCatalogRuntimeStatus> degradedStatusResponse =
                runtime.GetStatus();

            Assert.True(
                degradedStatusResponse.IsSuccess);

            Assert.NotNull(
                degradedStatusResponse.Data);

            Assert.True(
                degradedStatusResponse.Data.IsDegraded);

            Assert.Equal(
                ErrorCatalogContextSource.PreviousContext,
                degradedStatusResponse.Data.ContextSource);

            Assert.False(
                string.IsNullOrWhiteSpace(
                    degradedStatusResponse.Data.RecoveryReasonCode));

            await File.WriteAllTextAsync(
                jsonsOptions.ErrorCatalogFilePath,
                validProjectCatalog);

            Response<ErrorCatalogInitializationPayload>
                recoveredInitializationResponse =
                    await runtime.InitializeAsync();

            Assert.True(
                recoveredInitializationResponse.IsSuccess);

            Assert.Equal(
                ResultStatus.Success,
                recoveredInitializationResponse.Status);

            Assert.NotNull(
                recoveredInitializationResponse.Data);

            Assert.Equal(
                ErrorCatalogContextSource.ProjectCatalog,
                recoveredInitializationResponse.Data.ContextSource);

            Assert.False(
                recoveredInitializationResponse.Data.IsDegraded);

            Assert.False(
                recoveredInitializationResponse.Data.KeptPreviousContext);

            Assert.False(
                recoveredInitializationResponse.Data.UsedFallback);

            Response<ErrorCatalogContext> recoveredContextResponse =
                runtime.GetCurrentContext();

            Assert.True(
                recoveredContextResponse.IsSuccess);

            Assert.NotNull(
                recoveredContextResponse.Data);

            Assert.NotSame(
                originalContextResponse.Data,
                recoveredContextResponse.Data);

            Response<ErrorCatalogRuntimeStatus> recoveredStatusResponse =
                runtime.GetStatus();

            Assert.True(
                recoveredStatusResponse.IsSuccess);

            Assert.NotNull(
                recoveredStatusResponse.Data);

            Assert.Equal(
                ErrorCatalogContextSource.ProjectCatalog,
                recoveredStatusResponse.Data.ContextSource);

            Assert.False(
                recoveredStatusResponse.Data.IsDegraded);

            Assert.False(
                recoveredStatusResponse.Data.KeptPreviousContext);

            Assert.False(
                recoveredStatusResponse.Data.UsedFallback);

            Assert.Null(
                recoveredStatusResponse.Data.RecoveryReasonCode);

            Assert.Null(
                recoveredStatusResponse.Data.RecoveryStatus);

            Assert.Null(
                recoveredStatusResponse.Data.RecoveryMessage);

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
