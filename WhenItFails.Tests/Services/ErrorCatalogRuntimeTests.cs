using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Descriptors;
using Afrowave.Toolbox.WhenItFails.Enums;
using Afrowave.Toolbox.WhenItFails.Initialization;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Runtime;
using Afrowave.Toolbox.WhenItFails.Services;

namespace Afrowave.Toolbox.WhenItFails.Tests.Services;

public sealed class ErrorCatalogRuntimeTests
{
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenInitializerIsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ErrorCatalogRuntime(
                null!,
                new WhenItFailsOptions(),
                new FakeContextStore(),
                new FakeBuiltInContextProvider(),
                new FakeDescriptorService(),
                new FakeProfileSelectionService()));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenOptionsIsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ErrorCatalogRuntime(
                new FakeInitializer(),
                null!,
                new FakeContextStore(),
                new FakeBuiltInContextProvider(),
                new FakeDescriptorService(),
                new FakeProfileSelectionService()));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenContextStoreIsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ErrorCatalogRuntime(
                new FakeInitializer(),
                new WhenItFailsOptions(),
                null!,
                new FakeBuiltInContextProvider(),
                new FakeDescriptorService(),
                new FakeProfileSelectionService()));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenBuiltInContextProviderIsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ErrorCatalogRuntime(
                new FakeInitializer(),
                new WhenItFailsOptions(),
                new FakeContextStore(),
                null!,
                new FakeDescriptorService(),
                new FakeProfileSelectionService()));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenDescriptorServiceIsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ErrorCatalogRuntime(
                new FakeInitializer(),
                new WhenItFailsOptions(),
                new FakeContextStore(),
                new FakeBuiltInContextProvider(),
                null!,
                new FakeProfileSelectionService()));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenProfileSelectionServiceIsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ErrorCatalogRuntime(
                new FakeInitializer(),
                new WhenItFailsOptions(),
                new FakeContextStore(),
                new FakeBuiltInContextProvider(),
                new FakeDescriptorService(),
                null!));
    }

    [Fact]
    public async Task InitializeAsync_ShouldUseRegisteredJsonsOptions()
    {
        JsonsOptions jsonsOptions = new()
        {
            RootDirectory = "RegisteredJsons",
            PackageDirectoryName = "RegisteredWhenItFails",
            ErrorCatalogFileName = "registered-errors.json"
        };

        WhenItFailsOptions options = new()
        {
            Jsons = jsonsOptions
        };

        FakeInitializer initializer = new();

        ErrorCatalogRuntime runtime = new(
            initializer,
            options,
            new FakeContextStore(),
            new FakeBuiltInContextProvider(),
            new FakeDescriptorService(),
            new FakeProfileSelectionService());

        using CancellationTokenSource cancellationTokenSource = new();

        Response<ErrorCatalogInitializationPayload> response =
            await runtime.InitializeAsync(
                cancellationTokenSource.Token);

        Assert.True(response.IsSuccess);

        Assert.Same(
            jsonsOptions,
            initializer.LastOptions);

        Assert.Equal(
            cancellationTokenSource.Token,
            initializer.LastCancellationToken);
    }

    [Fact]
    public async Task InitializeAsync_ShouldUseDefaultJsonsOptions_WhenRegisteredJsonsIsNull()
    {
        WhenItFailsOptions options = new()
        {
            Jsons = null!
        };

        FakeInitializer initializer = new();

        ErrorCatalogRuntime runtime = new(
            initializer,
            options,
            new FakeContextStore(),
            new FakeBuiltInContextProvider(),
            new FakeDescriptorService(),
            new FakeProfileSelectionService());

        Response<ErrorCatalogInitializationPayload> response =
            await runtime.InitializeAsync();

        Assert.True(response.IsSuccess);
        Assert.NotNull(initializer.LastOptions);

        Assert.Equal(
            "Jsons",
            initializer.LastOptions.RootDirectory);

        Assert.Equal(
            "WhenItFails",
            initializer.LastOptions.PackageDirectoryName);
    }

    [Fact]
    public async Task InitializeAsync_ShouldDelegateToInitializer()
    {
        JsonsOptions options = new()
        {
            RootDirectory = "CustomJsons",
            PackageDirectoryName = "CustomWhenItFails"
        };

        ErrorCatalogInitializationPayload payload = new()
        {
            Bootstrap = new JsonsBootstrapPayload(),
            Context = new ErrorCatalogContext()
        };

        FakeInitializer initializer = new(
            Response<ErrorCatalogInitializationPayload>.Ok(
                payload));

        ErrorCatalogRuntime runtime = new(
            initializer,
            new WhenItFailsOptions(),
            new FakeContextStore(),
            new FakeBuiltInContextProvider(),
            new FakeDescriptorService(),
            new FakeProfileSelectionService());

        Response<ErrorCatalogInitializationPayload> response =
            await runtime.InitializeAsync(
                options);

        Assert.True(response.IsSuccess);
        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.Same(payload, response.Data);
        Assert.Same(options, initializer.LastOptions);
    }

    [Fact]
    public async Task InitializeAsync_ShouldThrowArgumentNullException_WhenExplicitOptionsAreNull()
    {
        ErrorCatalogRuntime runtime = new(
            new FakeInitializer(),
            new WhenItFailsOptions(),
            new FakeContextStore(),
            new FakeBuiltInContextProvider(),
            new FakeDescriptorService(),
            new FakeProfileSelectionService());

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => runtime.InitializeAsync(
                null!));
    }

    [Fact]
    public void GetCurrentContext_ShouldReturnFailure_WhenStoreIsNotInitialized()
    {
        ErrorCatalogRuntime runtime = new(
            new FakeInitializer(),
            new WhenItFailsOptions(),
            new FakeContextStore(),
            new FakeBuiltInContextProvider(),
            new FakeDescriptorService(),
            new FakeProfileSelectionService());

        Response<ErrorCatalogContext> response =
            runtime.GetCurrentContext();

        Assert.False(
            response.IsSuccess);

        Assert.Equal(
            ResultStatus.Invalid,
            response.Status);

        Assert.Equal(
            "ErrorCatalogContextNotInitialized",
            response.Issues[0].Code);
    }

    [Fact]
    public void GetCurrentContext_ShouldReturnActiveContext()
    {
        ErrorCatalogContext context = new();

        ErrorCatalogRuntime runtime = new(
            new FakeInitializer(),
            new WhenItFailsOptions(),
            new FakeContextStore(
                context),
            new FakeBuiltInContextProvider(),
            new FakeDescriptorService(),
            new FakeProfileSelectionService());

        Response<ErrorCatalogContext> response =
            runtime.GetCurrentContext();

        Assert.True(
            response.IsSuccess);

        Assert.Equal(
            ResultStatus.Success,
            response.Status);

        Assert.Same(
            context,
            response.Data);
    }




    [Fact]
    public void FromId_ShouldReturnContextFailure_WhenStoreIsNotInitialized()
    {
        ErrorCatalogRuntime runtime = new(
            new FakeInitializer(),
            new WhenItFailsOptions(),
            new FakeContextStore(),
            new FakeBuiltInContextProvider(),
            new FakeDescriptorService(),
            new FakeProfileSelectionService());

        Response<ErrorDescriptor> response =
            runtime.FromId(
                "AFW-GEN-0001");

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);

        Assert.Equal(
            "ErrorCatalogContextNotInitialized",
            response.Issues[0].Code);
    }

    [Fact]
    public void FromId_ShouldDelegateToDescriptorService()
    {
        ErrorCatalogContext context = new();
        ErrorDescriptor descriptor = CreateDescriptor();

        FakeDescriptorService descriptorService = new(
            Response<ErrorDescriptor>.Ok(
                descriptor));

        ErrorCatalogRuntime runtime = new(
            new FakeInitializer(),
            new WhenItFailsOptions(),
            new FakeContextStore(
                context),
            new FakeBuiltInContextProvider(),
            descriptorService,
            new FakeProfileSelectionService());

        Response<ErrorDescriptor> response =
            runtime.FromId(
                "AFW-GEN-0001");

        Assert.True(response.IsSuccess);
        Assert.Same(descriptor, response.Data);

        Assert.Equal(
            "FromId",
            descriptorService.LastCalledMethod);

        Assert.Same(
            context,
            descriptorService.LastContext);

        Assert.Equal(
            "AFW-GEN-0001",
            descriptorService.LastTextValue);
    }

    [Fact]
    public void FromName_ShouldDelegateToDescriptorService()
    {
        ErrorCatalogContext context = new();

        FakeDescriptorService descriptorService = new();

        ErrorCatalogRuntime runtime = new(
            new FakeInitializer(),
            new WhenItFailsOptions(),
            new FakeContextStore(
                context),
            new FakeBuiltInContextProvider(),
            descriptorService,
            new FakeProfileSelectionService());

        runtime.FromName(
            "UnknownError");

        Assert.Equal(
            "FromName",
            descriptorService.LastCalledMethod);

        Assert.Same(
            context,
            descriptorService.LastContext);

        Assert.Equal(
            "UnknownError",
            descriptorService.LastTextValue);
    }

    [Fact]
    public void FromCode_ShouldDelegateToDescriptorService()
    {
        ErrorCatalogContext context = new();

        FakeDescriptorService descriptorService = new();

        ErrorCatalogRuntime runtime = new(
            new FakeInitializer(),
            new WhenItFailsOptions(),
            new FakeContextStore(
                context),
            new FakeBuiltInContextProvider(),
            descriptorService,
            new FakeProfileSelectionService());

        runtime.FromCode(
            100001);

        Assert.Equal(
            "FromCode",
            descriptorService.LastCalledMethod);

        Assert.Same(
            context,
            descriptorService.LastContext);

        Assert.Equal(
            100001,
            descriptorService.LastCode);
    }

    [Fact]
    public void ResolveProfile_ShouldDelegateToProfileSelectionService()
    {
        ErrorCatalogContext context = new();

        IReadOnlyList<ErrorDefinition> errors =
        [
            new ErrorDefinition
            {
                Id = "AFW_GEN_0001",
                Code = 100001,
                Name = "UNKNOWN_ERROR"
            }
        ];

        FakeProfileSelectionService profileService = new(
            Response<IReadOnlyList<ErrorDefinition>>.Ok(
                errors));

        ErrorCatalogRuntime runtime = new(
            new FakeInitializer(),
            new WhenItFailsOptions(),
            new FakeContextStore(
                context),
            new FakeBuiltInContextProvider(),
            new FakeDescriptorService(),
            profileService);

        Response<IReadOnlyList<ErrorDefinition>> response =
            runtime.ResolveProfile(
                "WEB");

        Assert.True(response.IsSuccess);
        Assert.Same(errors, response.Data);

        Assert.Same(
            context,
            profileService.LastContext);

        Assert.Equal(
            "WEB",
            profileService.LastProfileName);
    }

    [Fact]
    public async Task InitializeAsync_ShouldKeepPreviousContext_WhenFlexibleInitializationFails()
    {
        ErrorCatalogContext previousContext = new();

        FakeInitializer initializer = new(
            Response<ErrorCatalogInitializationPayload>.Invalid(
                code: "CatalogDocumentsInvalid",
                message: "Catalog documents are invalid."));

        FakeBuiltInContextProvider builtInProvider = new();

        ErrorCatalogRuntime runtime = new(
            initializer,
            new WhenItFailsOptions
            {
                InitializationMode =
                    ErrorCatalogInitializationMode.Flexible
            },
            new FakeContextStore(
                previousContext),
            builtInProvider,
            new FakeDescriptorService(),
            new FakeProfileSelectionService());

        Response<ErrorCatalogInitializationPayload> response =
            await runtime.InitializeAsync(
                new JsonsOptions
                {
                    RootDirectory = "BrokenJsons",
                    PackageDirectoryName = "BrokenCatalog"
                });

        Assert.True(response.IsSuccess);

        Assert.Equal(
            ResultStatus.SuccessWithWarnings,
            response.Status);

        Assert.True(
            response.HasWarnings);

        Assert.NotNull(
            response.Data);

        Assert.Same(
            previousContext,
            response.Data.Context);

        Assert.Equal(
            ErrorCatalogContextSource.PreviousContext,
            response.Data.ContextSource);

        Assert.True(
            response.Data.KeptPreviousContext);

        Assert.False(
            response.Data.UsedFallback);

        Assert.True(
            response.Data.IsDegraded);

        Assert.Equal(
            "WIF_PREVIOUS_CONTEXT_RETAINED",
            response.Issues[0].Code);

        Assert.Equal(
            "CatalogDocumentsInvalid",
            response.Metadata[
                "WhenItFails.RecoveryReasonCode"]);

        Assert.False(
            builtInProvider.WasCalled);
    }

    [Fact]
    public async Task InitializeAsync_ShouldReturnOriginalFailure_WhenStrictInitializationFails()
    {
        ErrorCatalogContext previousContext = new();

        FakeInitializer initializer = new(
            Response<ErrorCatalogInitializationPayload>.Invalid(
                code: "CatalogDocumentsInvalid",
                message: "Catalog documents are invalid."));

        FakeContextStore contextStore = new(
            previousContext);

        FakeBuiltInContextProvider builtInProvider = new();

        ErrorCatalogRuntime runtime = new(
            initializer,
            new WhenItFailsOptions
            {
                InitializationMode =
                    ErrorCatalogInitializationMode.Strict
            },
            contextStore,
            builtInProvider,
            new FakeDescriptorService(),
            new FakeProfileSelectionService());

        Response<ErrorCatalogInitializationPayload> response =
            await runtime.InitializeAsync(
                new JsonsOptions());

        Assert.False(response.IsSuccess);

        Assert.Equal(
            ResultStatus.Invalid,
            response.Status);

        Assert.Equal(
            "CatalogDocumentsInvalid",
            response.Issues[0].Code);

        Assert.Same(
            previousContext,
            contextStore.Current);

        Assert.False(
            builtInProvider.WasCalled);
    }

    [Fact]
    public async Task InitializeAsync_ShouldHideRecoveredFailure_WhenConfigured()
    {
        ErrorCatalogContext previousContext = new();

        FakeInitializer initializer = new(
            Response<ErrorCatalogInitializationPayload>.Invalid(
                code: "CatalogDocumentsInvalid",
                message: "Catalog documents are invalid."));

        FakeBuiltInContextProvider builtInProvider = new();

        ErrorCatalogRuntime runtime = new(
            initializer,
            new WhenItFailsOptions
            {
                InitializationMode =
                    ErrorCatalogInitializationMode.Flexible,

                HideRecoverableFailures = true
            },
            new FakeContextStore(
                previousContext),
            builtInProvider,
            new FakeDescriptorService(),
            new FakeProfileSelectionService());

        Response<ErrorCatalogInitializationPayload> response =
            await runtime.InitializeAsync(
                new JsonsOptions());

        Assert.True(response.IsSuccess);

        Assert.Equal(
            ResultStatus.Success,
            response.Status);

        Assert.Empty(
            response.Issues);

        Assert.NotNull(
            response.Data);

        Assert.True(
            response.Data.IsDegraded);

        Assert.True(
            response.Data.KeptPreviousContext);

        Assert.False(
            response.Data.UsedFallback);

        Assert.Equal(
            ErrorCatalogContextSource.PreviousContext,
            response.Data.ContextSource);

        Assert.Equal(
            "CatalogDocumentsInvalid",
            response.Metadata[
                "WhenItFails.RecoveryReasonCode"]);

        Assert.Equal(
            ResultStatus.Invalid.ToString(),
            response.Metadata[
                "WhenItFails.RecoveryStatus"]);

        Assert.False(
            builtInProvider.WasCalled);
    }

    [Fact]
    public async Task InitializeAsync_ShouldNotUseRecovery_WhenInitializationSucceeds()
    {
        ErrorCatalogContext newContext = new();

        ErrorCatalogInitializationPayload payload = new()
        {
            Bootstrap = new JsonsBootstrapPayload(),
            Context = newContext,
            ContextSource =
                ErrorCatalogContextSource.ProjectCatalog
        };

        FakeInitializer initializer = new(
            Response<ErrorCatalogInitializationPayload>.Ok(
                payload));

        FakeBuiltInContextProvider builtInProvider = new();

        ErrorCatalogRuntime runtime = new(
            initializer,
            new WhenItFailsOptions
            {
                InitializationMode =
                    ErrorCatalogInitializationMode.Flexible,

                HideRecoverableFailures = true
            },
            new FakeContextStore(
                new ErrorCatalogContext()),
            builtInProvider,
            new FakeDescriptorService(),
            new FakeProfileSelectionService());

        Response<ErrorCatalogInitializationPayload> response =
            await runtime.InitializeAsync(
                new JsonsOptions());

        Assert.True(response.IsSuccess);

        Assert.Equal(
            ResultStatus.Success,
            response.Status);

        Assert.Same(
            payload,
            response.Data);

        Assert.False(
            response.Data!.IsDegraded);

        Assert.True(
            response.Metadata.IsEmpty);

        Assert.False(
            builtInProvider.WasCalled);
    }

    [Fact]
    public async Task InitializeAsync_ShouldActivateBuiltInDefaults_WhenFlexibleModeHasNoPreviousContext()
    {
        ErrorCatalogContext fallbackContext = new();

        FakeInitializer initializer = new(
            Response<ErrorCatalogInitializationPayload>.Invalid(
                code: "CatalogDocumentsInvalid",
                message: "Catalog documents are invalid."));

        FakeContextStore contextStore = new();

        FakeBuiltInContextProvider builtInProvider = new(
            Response<ErrorCatalogContext>.Ok(
                fallbackContext));

        ErrorCatalogRuntime runtime = new(
            initializer,
            new WhenItFailsOptions
            {
                InitializationMode =
                    ErrorCatalogInitializationMode.Flexible
            },
            contextStore,
            builtInProvider,
            new FakeDescriptorService(),
            new FakeProfileSelectionService());

        Response<ErrorCatalogInitializationPayload> response =
            await runtime.InitializeAsync(
                new JsonsOptions());

        Assert.True(response.IsSuccess);

        Assert.Equal(
            ResultStatus.SuccessWithWarnings,
            response.Status);

        Assert.True(
            response.HasWarnings);

        Assert.True(
            builtInProvider.WasCalled);

        Assert.NotNull(
            response.Data);

        Assert.Same(
            fallbackContext,
            response.Data.Context);

        Assert.Same(
            fallbackContext,
            contextStore.Current);

        Response<ErrorCatalogContext> currentContextResponse =
    runtime.GetCurrentContext();

        Assert.True(
            currentContextResponse.IsSuccess);

        Assert.Same(
            fallbackContext,
            currentContextResponse.Data);

        Assert.Equal(
            ErrorCatalogContextSource.BuiltInDefaults,
            response.Data.ContextSource);

        Assert.False(
            response.Data.KeptPreviousContext);

        Assert.True(
            response.Data.UsedFallback);

        Assert.True(
            response.Data.IsDegraded);

        Assert.Equal(
            "WIF_DEFAULT_FALLBACK_ACTIVATED",
            response.Issues[0].Code);

        Assert.Equal(
            "CatalogDocumentsInvalid",
            response.Metadata[
                "WhenItFails.RecoveryReasonCode"]);
    }

    [Fact]
    public async Task InitializeAsync_ShouldHideBuiltInFallbackWarning_WhenConfigured()
    {
        ErrorCatalogContext fallbackContext = new();

        FakeBuiltInContextProvider builtInProvider = new(
            Response<ErrorCatalogContext>.Ok(
                fallbackContext));

        FakeContextStore contextStore = new();

        ErrorCatalogRuntime runtime = new(
            new FakeInitializer(
                Response<ErrorCatalogInitializationPayload>.Invalid(
                    code: "CatalogDocumentsInvalid",
                    message: "Catalog documents are invalid.")),
            new WhenItFailsOptions
            {
                InitializationMode =
                    ErrorCatalogInitializationMode.Flexible,

                HideRecoverableFailures = true
            },
            contextStore,
            builtInProvider,
            new FakeDescriptorService(),
            new FakeProfileSelectionService());

        Response<ErrorCatalogInitializationPayload> response =
            await runtime.InitializeAsync(
                new JsonsOptions());

        Assert.True(response.IsSuccess);

        Assert.Equal(
            ResultStatus.Success,
            response.Status);

        Assert.Empty(
            response.Issues);

        Assert.NotNull(
            response.Data);

        Assert.Same(
            fallbackContext,
            response.Data.Context);

        Assert.Same(
            fallbackContext,
            contextStore.Current);

        Assert.True(
            response.Data.UsedFallback);

        Assert.True(
            response.Data.IsDegraded);

        Assert.Equal(
            ErrorCatalogContextSource.BuiltInDefaults,
            response.Data.ContextSource);

        Assert.Equal(
            "CatalogDocumentsInvalid",
            response.Metadata[
                "WhenItFails.RecoveryReasonCode"]);

        Assert.Equal(
            ResultStatus.Invalid.ToString(),
            response.Metadata[
                "WhenItFails.RecoveryStatus"]);
    }

    [Fact]
    public async Task InitializeAsync_ShouldPreferPreviousContextOverBuiltInDefaults()
    {
        ErrorCatalogContext previousContext = new();

        FakeBuiltInContextProvider builtInProvider = new();

        ErrorCatalogRuntime runtime = new(
            new FakeInitializer(
                Response<ErrorCatalogInitializationPayload>.Invalid(
                    code: "CatalogDocumentsInvalid",
                    message: "Catalog documents are invalid.")),
            new WhenItFailsOptions
            {
                InitializationMode =
                    ErrorCatalogInitializationMode.Flexible
            },
            new FakeContextStore(
                previousContext),
            builtInProvider,
            new FakeDescriptorService(),
            new FakeProfileSelectionService());

        Response<ErrorCatalogInitializationPayload> response =
            await runtime.InitializeAsync(
                new JsonsOptions());

        Assert.True(response.IsSuccess);

        Assert.NotNull(
            response.Data);

        Assert.Same(
            previousContext,
            response.Data.Context);

        Assert.Equal(
            ErrorCatalogContextSource.PreviousContext,
            response.Data.ContextSource);

        Assert.False(
            response.Data.UsedFallback);

        Assert.False(
            builtInProvider.WasCalled);
    }

    [Fact]
    public async Task InitializeAsync_ShouldReturnFailure_WhenBuiltInFallbackFails()
    {
        FakeBuiltInContextProvider builtInProvider = new(
            Response<ErrorCatalogContext>.Invalid(
                code: "BuiltInCatalogInvalid",
                message: "Built-in catalog is invalid."));

        FakeContextStore contextStore = new();

        ErrorCatalogRuntime runtime = new(
            new FakeInitializer(
                Response<ErrorCatalogInitializationPayload>.Invalid(
                    code: "ProjectCatalogInvalid",
                    message: "Project catalog is invalid.")),
            new WhenItFailsOptions
            {
                InitializationMode =
                    ErrorCatalogInitializationMode.Flexible
            },
            contextStore,
            builtInProvider,
            new FakeDescriptorService(),
            new FakeProfileSelectionService());

        Response<ErrorCatalogInitializationPayload> response =
            await runtime.InitializeAsync(
                new JsonsOptions());

        Assert.False(response.IsSuccess);

        Assert.Equal(
            ResultStatus.Invalid,
            response.Status);

        Assert.Equal(
            "WIF_DEFAULT_FALLBACK_FAILED",
            response.Issues[0].Code);

        Assert.Null(
            contextStore.Current);

        Assert.True(
            builtInProvider.WasCalled);

        Assert.Equal(
            "ProjectCatalogInvalid",
            response.Metadata[
                "WhenItFails.ProjectFailure.Code"]);

        Assert.Equal(
            ResultStatus.Invalid.ToString(),
            response.Metadata[
                "WhenItFails.ProjectFailure.Status"]);

        Assert.Equal(
            "BuiltInCatalogInvalid",
            response.Metadata[
                "WhenItFails.FallbackFailure.Code"]);

        Assert.Equal(
            ResultStatus.Invalid.ToString(),
            response.Metadata[
                "WhenItFails.FallbackFailure.Status"]);
    }

    [Fact]
    public async Task InitializeAsync_ShouldReturnInvalid_WhenBuiltInFallbackReturnsNullContext()
    {
        FakeBuiltInContextProvider builtInProvider = new(
            Response<ErrorCatalogContext>.Ok(
                null));

        FakeContextStore contextStore = new();

        ErrorCatalogRuntime runtime = new(
            new FakeInitializer(
                Response<ErrorCatalogInitializationPayload>.Invalid(
                    code: "ProjectCatalogInvalid",
                    message: "Project catalog is invalid.")),
            new WhenItFailsOptions
            {
                InitializationMode =
                    ErrorCatalogInitializationMode.Flexible
            },
            contextStore,
            builtInProvider,
            new FakeDescriptorService(),
            new FakeProfileSelectionService());

        Response<ErrorCatalogInitializationPayload> response =
            await runtime.InitializeAsync(
                new JsonsOptions());

        Assert.False(response.IsSuccess);

        Assert.Equal(
            ResultStatus.Invalid,
            response.Status);

        Assert.Equal(
            "WIF_DEFAULT_FALLBACK_FAILED",
            response.Issues[0].Code);

        Assert.Null(
            contextStore.Current);

        Assert.Equal(
            "WIF_BUILT_IN_CONTEXT_PAYLOAD_NULL",
            response.Metadata[
                "WhenItFails.FallbackFailure.Code"]);

        Assert.Equal(
            ResultStatus.Invalid.ToString(),
            response.Metadata[
                "WhenItFails.FallbackFailure.Status"]);
    }

    [Fact]
    public async Task InitializeAsync_ShouldPassCancellationTokenToBuiltInFallback()
    {
        FakeBuiltInContextProvider builtInProvider = new();

        ErrorCatalogRuntime runtime = new(
            new FakeInitializer(
                Response<ErrorCatalogInitializationPayload>.Invalid(
                    code: "ProjectCatalogInvalid",
                    message: "Project catalog is invalid.")),
            new WhenItFailsOptions
            {
                InitializationMode =
                    ErrorCatalogInitializationMode.Flexible
            },
            new FakeContextStore(),
            builtInProvider,
            new FakeDescriptorService(),
            new FakeProfileSelectionService());

        using CancellationTokenSource cancellationTokenSource = new();

        Response<ErrorCatalogInitializationPayload> response =
            await runtime.InitializeAsync(
                new JsonsOptions(),
                cancellationTokenSource.Token);

        Assert.True(response.IsSuccess);

        Assert.True(
            builtInProvider.WasCalled);

        Assert.Equal(
            cancellationTokenSource.Token,
            builtInProvider.LastCancellationToken);
    }

    [Fact]
    public async Task ResetToDefaultsAsync_ShouldActivateBuiltInContext()
    {
        ErrorCatalogContext previousContext = new();
        ErrorCatalogContext builtInContext = new();

        FakeContextStore contextStore = new(
            previousContext);

        FakeBuiltInContextProvider builtInProvider = new(
            Response<ErrorCatalogContext>.Ok(
                builtInContext));

        ErrorCatalogRuntime runtime = new(
            new FakeInitializer(),
            new WhenItFailsOptions(),
            contextStore,
            builtInProvider,
            new FakeDescriptorService(),
            new FakeProfileSelectionService());

        Response<ErrorCatalogInitializationPayload> response =
            await runtime.ResetToDefaultsAsync();

        Assert.True(
            response.IsSuccess);

        Assert.Equal(
            ResultStatus.Success,
            response.Status);

        Assert.True(
            builtInProvider.WasCalled);

        Assert.NotNull(
            response.Data);

        Assert.Same(
            builtInContext,
            response.Data.Context);

        Assert.Same(
            builtInContext,
            contextStore.Current);

        Assert.Equal(
            ErrorCatalogContextSource.BuiltInDefaults,
            response.Data.ContextSource);

        Assert.False(
            response.Data.KeptPreviousContext);

        Assert.False(
            response.Data.UsedFallback);

        Assert.False(
            response.Data.IsDegraded);
    }

    [Fact]
    public async Task ResetToDefaultsAsync_ShouldKeepCurrentContext_WhenBuiltInLoadingFails()
    {
        ErrorCatalogContext previousContext = new();

        FakeContextStore contextStore = new(
            previousContext);

        FakeBuiltInContextProvider builtInProvider = new(
            Response<ErrorCatalogContext>.Invalid(
                code: "BuiltInCatalogInvalid",
                message: "Built-in catalog is invalid."));

        ErrorCatalogRuntime runtime = new(
            new FakeInitializer(),
            new WhenItFailsOptions(),
            contextStore,
            builtInProvider,
            new FakeDescriptorService(),
            new FakeProfileSelectionService());

        Response<ErrorCatalogInitializationPayload> response =
            await runtime.ResetToDefaultsAsync();

        Assert.False(
            response.IsSuccess);

        Assert.Equal(
            ResultStatus.Invalid,
            response.Status);

        Assert.Equal(
            "WIF_RESET_TO_DEFAULTS_FAILED",
            response.Issues[0].Code);

        Assert.Same(
            previousContext,
            contextStore.Current);

        Assert.Equal(
            "BuiltInCatalogInvalid",
            response.Metadata[
                "WhenItFails.ResetFailure.Code"]);
    }

    [Fact]
    public async Task ResetToDefaultsAsync_ShouldReturnInvalid_WhenBuiltInProviderReturnsNullContext()
    {
        ErrorCatalogContext previousContext = new();

        FakeContextStore contextStore = new(
            previousContext);

        FakeBuiltInContextProvider builtInProvider = new(
            Response<ErrorCatalogContext>.Ok(
                null));

        ErrorCatalogRuntime runtime = new(
            new FakeInitializer(),
            new WhenItFailsOptions(),
            contextStore,
            builtInProvider,
            new FakeDescriptorService(),
            new FakeProfileSelectionService());

        Response<ErrorCatalogInitializationPayload> response =
            await runtime.ResetToDefaultsAsync();

        Assert.False(
            response.IsSuccess);

        Assert.Equal(
            ResultStatus.Invalid,
            response.Status);

        Assert.Equal(
            "WIF_RESET_TO_DEFAULTS_FAILED",
            response.Issues[0].Code);

        Assert.Same(
            previousContext,
            contextStore.Current);

        Assert.Equal(
            "WIF_BUILT_IN_CONTEXT_PAYLOAD_NULL",
            response.Metadata[
                "WhenItFails.ResetFailure.Code"]);
    }

    [Fact]
    public async Task ResetToDefaultsAsync_ShouldPassCancellationTokenToBuiltInProvider()
    {
        FakeBuiltInContextProvider builtInProvider = new();

        ErrorCatalogRuntime runtime = new(
            new FakeInitializer(),
            new WhenItFailsOptions(),
            new FakeContextStore(),
            builtInProvider,
            new FakeDescriptorService(),
            new FakeProfileSelectionService());

        using CancellationTokenSource cancellationTokenSource = new();

        Response<ErrorCatalogInitializationPayload> response =
            await runtime.ResetToDefaultsAsync(
                cancellationTokenSource.Token);

        Assert.True(
            response.IsSuccess);

        Assert.Equal(
            cancellationTokenSource.Token,
            builtInProvider.LastCancellationToken);
    }

    [Fact]
    public void GetStatus_ShouldReturnFailure_WhenNoContextWasActivated()
    {
        ErrorCatalogRuntime runtime = new(
            new FakeInitializer(),
            new WhenItFailsOptions(),
            new FakeContextStore(),
            new FakeBuiltInContextProvider(),
            new FakeDescriptorService(),
            new FakeProfileSelectionService());

        Response<ErrorCatalogRuntimeStatus> response =
            runtime.GetStatus();

        Assert.False(
            response.IsSuccess);

        Assert.Equal(
            ResultStatus.Invalid,
            response.Status);

        Assert.Equal(
            "WIF_RUNTIME_STATUS_UNAVAILABLE",
            response.Issues[0].Code);
    }

    [Fact]
    public async Task GetStatus_ShouldDescribeProjectCatalogAfterSuccessfulInitialization()
    {
        ErrorCatalogInitializationPayload payload = new()
        {
            Bootstrap = new JsonsBootstrapPayload
            {
                PackageDirectoryPath =
                    "Jsons/WhenItFails"
            },

            Context =
                new ErrorCatalogContext(),

            ContextSource =
                ErrorCatalogContextSource.ProjectCatalog,

            KeptPreviousContext = false,
            UsedFallback = false
        };

        ErrorCatalogRuntime runtime = new(
            new FakeInitializer(
                Response<ErrorCatalogInitializationPayload>.Ok(
                    payload)),
            new WhenItFailsOptions(),
            new FakeContextStore(),
            new FakeBuiltInContextProvider(),
            new FakeDescriptorService(),
            new FakeProfileSelectionService());

        DateTimeOffset beforeInitialization =
            DateTimeOffset.UtcNow;

        Response<ErrorCatalogInitializationPayload>
            initializationResponse =
                await runtime.InitializeAsync(
                    new JsonsOptions());

        DateTimeOffset afterInitialization =
            DateTimeOffset.UtcNow;

        Assert.True(
            initializationResponse.IsSuccess);

        Response<ErrorCatalogRuntimeStatus> statusResponse =
            runtime.GetStatus();

        Assert.True(
            statusResponse.IsSuccess);

        Assert.NotNull(
            statusResponse.Data);

        Assert.Equal(
            ErrorCatalogContextSource.ProjectCatalog,
            statusResponse.Data.ContextSource);

        Assert.False(
            statusResponse.Data.IsDegraded);

        Assert.False(
            statusResponse.Data.KeptPreviousContext);

        Assert.False(
            statusResponse.Data.UsedFallback);

        Assert.Equal(
            "Jsons/WhenItFails",
            statusResponse.Data.PackageDirectoryPath);

        Assert.InRange(
            statusResponse.Data.ActivatedAtUtc,
            beforeInitialization,
            afterInitialization);
    }
    [Fact]
    public async Task GetStatus_ShouldDescribeAutomaticBuiltInFallback()
    {
        ErrorCatalogContext fallbackContext = new();

        ErrorCatalogRuntime runtime = new(
            new FakeInitializer(
                Response<ErrorCatalogInitializationPayload>.Invalid(
                    code: "ProjectCatalogInvalid",
                    message: "Project catalog is invalid.")),
            new WhenItFailsOptions
            {
                InitializationMode =
                    ErrorCatalogInitializationMode.Flexible
            },
            new FakeContextStore(),
            new FakeBuiltInContextProvider(
                Response<ErrorCatalogContext>.Ok(
                    fallbackContext)),
            new FakeDescriptorService(),
            new FakeProfileSelectionService());

        Response<ErrorCatalogInitializationPayload>
            initializationResponse =
                await runtime.InitializeAsync(
                    new JsonsOptions
                    {
                        RootDirectory = "Jsons",
                        PackageDirectoryName = "WhenItFails"
                    });

        Assert.True(
            initializationResponse.IsSuccess);

        Response<ErrorCatalogRuntimeStatus> statusResponse =
            runtime.GetStatus();

        Assert.True(
            statusResponse.IsSuccess);

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
    }
    [Fact]
    public async Task GetStatus_ShouldDescribeExplicitResetAsNonDegraded()
    {
        ErrorCatalogRuntime runtime = new(
            new FakeInitializer(),
            new WhenItFailsOptions(),
            new FakeContextStore(),
            new FakeBuiltInContextProvider(
                Response<ErrorCatalogContext>.Ok(
                    new ErrorCatalogContext())),
            new FakeDescriptorService(),
            new FakeProfileSelectionService());

        Response<ErrorCatalogInitializationPayload> resetResponse =
            await runtime.ResetToDefaultsAsync();

        Assert.True(
            resetResponse.IsSuccess);

        Response<ErrorCatalogRuntimeStatus> statusResponse =
            runtime.GetStatus();

        Assert.True(
            statusResponse.IsSuccess);

        Assert.NotNull(
            statusResponse.Data);

        Assert.Equal(
            ErrorCatalogContextSource.BuiltInDefaults,
            statusResponse.Data.ContextSource);

        Assert.False(
            statusResponse.Data.IsDegraded);

        Assert.False(
            statusResponse.Data.KeptPreviousContext);

        Assert.False(
            statusResponse.Data.UsedFallback);
    }


[Fact]
public async Task ResetToDefaultsAsync_ShouldKeepPreviousStatus_WhenResetFails()
    {
        ErrorCatalogInitializationPayload projectPayload = new()
        {
            Bootstrap = new JsonsBootstrapPayload
            {
                PackageDirectoryPath =
                    "Jsons/WhenItFails"
            },

            Context =
                new ErrorCatalogContext(),

            ContextSource =
                ErrorCatalogContextSource.ProjectCatalog,

            KeptPreviousContext = false,
            UsedFallback = false
        };

        FakeBuiltInContextProvider builtInProvider = new(
            Response<ErrorCatalogContext>.Invalid(
                code: "BuiltInCatalogInvalid",
                message: "Built-in catalog is invalid."));

        ErrorCatalogRuntime runtime = new(
            new FakeInitializer(
                Response<ErrorCatalogInitializationPayload>.Ok(
                    projectPayload)),
            new WhenItFailsOptions(),
            new FakeContextStore(),
            builtInProvider,
            new FakeDescriptorService(),
            new FakeProfileSelectionService());

        Response<ErrorCatalogInitializationPayload>
            initializationResponse =
                await runtime.InitializeAsync(
                    new JsonsOptions());

        Assert.True(
            initializationResponse.IsSuccess);

        Response<ErrorCatalogRuntimeStatus> statusBeforeReset =
            runtime.GetStatus();

        Assert.True(
            statusBeforeReset.IsSuccess);

        Assert.NotNull(
            statusBeforeReset.Data);

        Response<ErrorCatalogInitializationPayload> resetResponse =
            await runtime.ResetToDefaultsAsync();

        Assert.False(
            resetResponse.IsSuccess);

        Assert.Equal(
            "WIF_RESET_TO_DEFAULTS_FAILED",
            resetResponse.Issues[0].Code);

        Response<ErrorCatalogRuntimeStatus> statusAfterReset =
            runtime.GetStatus();

        Assert.True(
            statusAfterReset.IsSuccess);

        Assert.NotNull(
            statusAfterReset.Data);

        Assert.Same(
            statusBeforeReset.Data,
            statusAfterReset.Data);

        Assert.Equal(
            ErrorCatalogContextSource.ProjectCatalog,
            statusAfterReset.Data.ContextSource);

        Assert.False(
            statusAfterReset.Data.IsDegraded);

        Assert.Equal(
            "Jsons/WhenItFails",
            statusAfterReset.Data.PackageDirectoryPath);
    }



    private static ErrorDescriptor CreateDescriptor()
    {
        return new ErrorDescriptor
        {
            Id = "AFW_GEN_0001",
            Code = 100001,
            Name = "UNKNOWN_ERROR"
        };
    }

    private sealed class FakeInitializer
        : IErrorCatalogInitializer
    {
        private readonly Response<ErrorCatalogInitializationPayload>
            _response;

        public FakeInitializer(
            Response<ErrorCatalogInitializationPayload>? response = null)
        {
            _response = response
                ?? Response<ErrorCatalogInitializationPayload>.Ok(
                    new ErrorCatalogInitializationPayload
                    {
                        Bootstrap = new JsonsBootstrapPayload(),
                        Context = new ErrorCatalogContext()
                    });
        }

        public JsonsOptions? LastOptions { get; private set; }

        public CancellationToken LastCancellationToken { get; private set; }

        public Task<Response<ErrorCatalogInitializationPayload>>
            InitializeAsync(
                JsonsOptions options,
                CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            LastOptions = options;
            LastCancellationToken = cancellationToken;

            return Task.FromResult(
                _response);
        }
    }

    private sealed class FakeContextStore
        : IErrorCatalogContextStore
    {
        public FakeContextStore(
            ErrorCatalogContext? context = null)
        {
            Current = context;
        }

        public bool IsInitialized =>
            Current is not null;

        public ErrorCatalogContext? Current { get; private set; }

        public Response<ErrorCatalogContext> GetCurrent()
        {
            return Current is null
                ? Response<ErrorCatalogContext>.Invalid(
                    code: "ErrorCatalogContextNotInitialized",
                    message:
                        "Error catalog context has not been initialized.")
                : Response<ErrorCatalogContext>.Ok(
                    Current);
        }

        public void Set(
            ErrorCatalogContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            Current = context;
        }
    }

    private sealed class FakeBuiltInContextProvider
        : IBuiltInErrorCatalogContextProvider
    {
        private readonly Response<ErrorCatalogContext>
            _response;

        public FakeBuiltInContextProvider(
            Response<ErrorCatalogContext>? response = null)
        {
            _response = response
                ?? Response<ErrorCatalogContext>.Ok(
                    new ErrorCatalogContext());
        }

        public bool WasCalled { get; private set; }

        public CancellationToken LastCancellationToken { get; private set; }

        public Task<Response<ErrorCatalogContext>> LoadAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            WasCalled = true;
            LastCancellationToken = cancellationToken;

            return Task.FromResult(
                _response);
        }
    }

    private sealed class FakeDescriptorService
        : IErrorDescriptorService
    {
        private readonly Response<ErrorDescriptor>
            _response;

        public FakeDescriptorService(
            Response<ErrorDescriptor>? response = null)
        {
            _response = response
                ?? Response<ErrorDescriptor>.Ok(
                    CreateDescriptor());
        }

        public string? LastCalledMethod { get; private set; }

        public ErrorCatalogContext? LastContext { get; private set; }

        public string? LastTextValue { get; private set; }

        public int? LastCode { get; private set; }

        public Response<ErrorDescriptor> FromId(
            ErrorCatalogContext? context,
            string errorId)
        {
            LastCalledMethod = "FromId";
            LastContext = context;
            LastTextValue = errorId;
            LastCode = null;

            return _response;
        }

        public Response<ErrorDescriptor> FromName(
            ErrorCatalogContext? context,
            string errorName)
        {
            LastCalledMethod = "FromName";
            LastContext = context;
            LastTextValue = errorName;
            LastCode = null;

            return _response;
        }

        public Response<ErrorDescriptor> FromCode(
            ErrorCatalogContext? context,
            int code)
        {
            LastCalledMethod = "FromCode";
            LastContext = context;
            LastTextValue = null;
            LastCode = code;

            return _response;
        }
    }

    private sealed class FakeProfileSelectionService
        : IErrorProfileSelectionService
    {
        private readonly Response<IReadOnlyList<ErrorDefinition>>
            _response;

        public FakeProfileSelectionService(
            Response<IReadOnlyList<ErrorDefinition>>? response = null)
        {
            _response = response
                ?? Response<IReadOnlyList<ErrorDefinition>>.Ok(
                    []);
        }

        public ErrorCatalogContext? LastContext { get; private set; }

        public string? LastProfileName { get; private set; }

        public Response<IReadOnlyList<ErrorDefinition>>
            ResolveByProfileName(
                ErrorCatalogContext? context,
                string profileName)
        {
            LastContext = context;
            LastProfileName = profileName;

            return _response;
        }
    }
}

