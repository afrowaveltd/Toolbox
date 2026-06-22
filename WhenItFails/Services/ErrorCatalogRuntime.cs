using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Issues;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Descriptors;
using Afrowave.Toolbox.WhenItFails.Enums;
using Afrowave.Toolbox.WhenItFails.Initialization;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Services;

/// <summary>
/// Default high-level facade over the complete WhenItFails runtime.
/// </summary>
public sealed class ErrorCatalogRuntime : IErrorCatalogRuntime
{
    private readonly IErrorCatalogInitializer _initializer;
    private readonly WhenItFailsOptions _options;
    private readonly IErrorCatalogContextStore _contextStore;
    private readonly IBuiltInErrorCatalogContextProvider
        _builtInContextProvider;
    private readonly IErrorDescriptorService _descriptorService;
    private readonly IErrorProfileSelectionService
        _profileSelectionService;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="ErrorCatalogRuntime"/> class.
    /// </summary>
    public ErrorCatalogRuntime(
        IErrorCatalogInitializer initializer,
        WhenItFailsOptions options,
        IErrorCatalogContextStore contextStore,
        IBuiltInErrorCatalogContextProvider builtInContextProvider,
        IErrorDescriptorService descriptorService,
        IErrorProfileSelectionService profileSelectionService)
    {
        _initializer = initializer
            ?? throw new ArgumentNullException(
                nameof(initializer));

        _options = options
            ?? throw new ArgumentNullException(
                nameof(options));

        _contextStore = contextStore
            ?? throw new ArgumentNullException(
                nameof(contextStore));

        _builtInContextProvider = builtInContextProvider
            ?? throw new ArgumentNullException(
                nameof(builtInContextProvider));

        _descriptorService = descriptorService
            ?? throw new ArgumentNullException(
                nameof(descriptorService));

        _profileSelectionService = profileSelectionService
            ?? throw new ArgumentNullException(
                nameof(profileSelectionService));
    }

    /// <inheritdoc />
    public Task<Response<ErrorCatalogInitializationPayload>>
        InitializeAsync(
            CancellationToken cancellationToken = default)
    {
        JsonsOptions jsonsOptions =
            _options.Jsons ?? new JsonsOptions();

        return InitializeCoreAsync(
            jsonsOptions,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<Response<ErrorCatalogInitializationPayload>>
        InitializeAsync(
            JsonsOptions options,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        return InitializeCoreAsync(
            options,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Response<ErrorCatalogInitializationPayload>>
        ResetToDefaultsAsync(
            CancellationToken cancellationToken = default)
    {
        Response<ErrorCatalogContext> builtInResponse =
            await _builtInContextProvider.LoadAsync(
                cancellationToken);

        if (!builtInResponse.IsSuccess
            || builtInResponse.Data is null)
        {
            return CreateResetToDefaultsFailureResponse(
                builtInResponse);
        }

        _contextStore.Set(
            builtInResponse.Data);

        JsonsOptions jsonsOptions =
            _options.Jsons ?? new JsonsOptions();

        ErrorCatalogInitializationPayload payload = new()
        {
            Bootstrap =
                CreateBootstrapSnapshot(
                    jsonsOptions),

            Context =
                builtInResponse.Data,

            ContextSource =
                ErrorCatalogContextSource.BuiltInDefaults,

            KeptPreviousContext = false,

            // This was an explicit user operation,
            // not an automatic recovery fallback.
            UsedFallback = false
        };

        return Response<ErrorCatalogInitializationPayload>.Ok(
            payload,
            "The bundled default error catalog was activated.");
    }

    /// <inheritdoc />
    public Response<ErrorDescriptor> FromId(
        string errorId)
    {
        Response<ErrorCatalogContext> contextResponse =
            _contextStore.GetCurrent();

        if (!contextResponse.IsSuccess)
        {
            return ForwardContextFailure<ErrorDescriptor>(
                contextResponse);
        }

        return _descriptorService.FromId(
            contextResponse.Data!,
            errorId);
    }

    /// <inheritdoc />
    public Response<ErrorDescriptor> FromName(
        string errorName)
    {
        Response<ErrorCatalogContext> contextResponse =
            _contextStore.GetCurrent();

        if (!contextResponse.IsSuccess)
        {
            return ForwardContextFailure<ErrorDescriptor>(
                contextResponse);
        }

        return _descriptorService.FromName(
            contextResponse.Data!,
            errorName);
    }

    /// <inheritdoc />
    public Response<ErrorDescriptor> FromCode(
        int code)
    {
        Response<ErrorCatalogContext> contextResponse =
            _contextStore.GetCurrent();

        if (!contextResponse.IsSuccess)
        {
            return ForwardContextFailure<ErrorDescriptor>(
                contextResponse);
        }

        return _descriptorService.FromCode(
            contextResponse.Data!,
            code);
    }

    /// <inheritdoc />
    public Response<IReadOnlyList<ErrorDefinition>>
        ResolveProfile(
            string profileName)
    {
        Response<ErrorCatalogContext> contextResponse =
            _contextStore.GetCurrent();

        if (!contextResponse.IsSuccess)
        {
            return ForwardContextFailure<
                IReadOnlyList<ErrorDefinition>>(
                    contextResponse);
        }

        return _profileSelectionService.ResolveByProfileName(
            contextResponse.Data!,
            profileName);
    }

    private static Response<ErrorCatalogInitializationPayload>
    CreateResetToDefaultsFailureResponse(
        Response<ErrorCatalogContext> builtInResponse)
    {
        string failureCode =
            builtInResponse.Issues.Count > 0
                ? builtInResponse.Issues[0].Code
                : builtInResponse.Data is null
                    ? "WIF_BUILT_IN_CONTEXT_PAYLOAD_NULL"
                    : "WIF_BUILT_IN_CONTEXT_LOAD_FAILED";

        string failureMessage =
            string.IsNullOrWhiteSpace(
                builtInResponse.Message)
                    ? builtInResponse.Data is null
                        ? "The bundled default catalog provider "
                          + "returned no context."
                        : "The bundled default error catalog "
                          + "could not be loaded."
                    : builtInResponse.Message;

        ResultStatus failureStatus =
            builtInResponse.IsSuccess
                ? ResultStatus.Invalid
                : builtInResponse.Status;

        Response<ErrorCatalogInitializationPayload> response =
            Response<ErrorCatalogInitializationPayload>
                .WithStatus(
                    Response<
                        ErrorCatalogInitializationPayload>.Fail(
                            code: "WIF_RESET_TO_DEFAULTS_FAILED",
                            message:
                                "The bundled default error catalog "
                                + "could not be activated."),
                    failureStatus);

        response =
            Response<ErrorCatalogInitializationPayload>
                .AddMetadata(
                    response,
                    "WhenItFails.ResetFailure.Code",
                    failureCode);

        response =
            Response<ErrorCatalogInitializationPayload>
                .AddMetadata(
                    response,
                    "WhenItFails.ResetFailure.Status",
                    failureStatus.ToString());

        return Response<ErrorCatalogInitializationPayload>
            .AddMetadata(
                response,
                "WhenItFails.ResetFailure.Message",
                failureMessage);
    }

    private async Task<Response<ErrorCatalogInitializationPayload>>
        InitializeCoreAsync(
            JsonsOptions options,
            CancellationToken cancellationToken)
    {
        Response<ErrorCatalogInitializationPayload>
            initializationResponse =
                await _initializer.InitializeAsync(
                    options,
                    cancellationToken);

        if (initializationResponse.IsSuccess)
        {
            return initializationResponse;
        }

        if (_options.InitializationMode
            != ErrorCatalogInitializationMode.Flexible)
        {
            return initializationResponse;
        }

        Response<ErrorCatalogContext> previousContextResponse =
            _contextStore.GetCurrent();

        if (previousContextResponse.IsSuccess
            && previousContextResponse.Data is not null)
        {
            return CreatePreviousContextRecoveryResponse(
                options,
                previousContextResponse.Data,
                initializationResponse);
        }

        return await CreateBuiltInFallbackResponseAsync(
            options,
            initializationResponse,
            cancellationToken);
    }

    private Response<ErrorCatalogInitializationPayload>
        CreatePreviousContextRecoveryResponse(
            JsonsOptions options,
            ErrorCatalogContext previousContext,
            Response<ErrorCatalogInitializationPayload>
                initializationResponse)
    {
        ErrorCatalogInitializationPayload recoveryPayload = new()
        {
            Bootstrap =
                CreateBootstrapSnapshot(options),

            Context =
                previousContext,

            ContextSource =
                ErrorCatalogContextSource.PreviousContext,

            KeptPreviousContext = true,
            UsedFallback = false
        };

        Response<ErrorCatalogInitializationPayload>
            recoveryResponse =
                _options.HideRecoverableFailures == true
                    ? Response<
                        ErrorCatalogInitializationPayload>.Ok(
                            recoveryPayload,
                            "The previous valid error catalog "
                            + "context was retained.")
                    : CreatePreviousContextWarningResponse(
                        recoveryPayload,
                        initializationResponse);

        return AddRecoveryMetadata(
            recoveryResponse,
            initializationResponse);
    }

    private async Task<Response<ErrorCatalogInitializationPayload>>
        CreateBuiltInFallbackResponseAsync(
            JsonsOptions options,
            Response<ErrorCatalogInitializationPayload>
                initializationResponse,
            CancellationToken cancellationToken)
    {
        Response<ErrorCatalogContext> fallbackResponse =
            await _builtInContextProvider.LoadAsync(
                cancellationToken);

        if (!fallbackResponse.IsSuccess
            || fallbackResponse.Data is null)
        {
            return CreateBuiltInFallbackFailureResponse(
                initializationResponse,
                fallbackResponse);
        }

        _contextStore.Set(
            fallbackResponse.Data);

        ErrorCatalogInitializationPayload fallbackPayload = new()
        {
            Bootstrap =
                CreateBootstrapSnapshot(options),

            Context =
                fallbackResponse.Data,

            ContextSource =
                ErrorCatalogContextSource.BuiltInDefaults,

            KeptPreviousContext = false,
            UsedFallback = true
        };

        Response<ErrorCatalogInitializationPayload> response =
            _options.HideRecoverableFailures == true
                ? Response<
                    ErrorCatalogInitializationPayload>.Ok(
                        fallbackPayload,
                        "The bundled default error catalog "
                        + "was activated.")
                : CreateBuiltInFallbackWarningResponse(
                    fallbackPayload,
                    initializationResponse);

        return AddRecoveryMetadata(
            response,
            initializationResponse);
    }

    private static Response<ErrorCatalogInitializationPayload>
        CreatePreviousContextWarningResponse(
            ErrorCatalogInitializationPayload payload,
            Response<ErrorCatalogInitializationPayload>
                initializationResponse)
    {
        IssueInfo warning = new()
        {
            Code = "WIF_PREVIOUS_CONTEXT_RETAINED",

            Message =
                "The new error catalog context could not "
                + "be activated. The previous valid context "
                + "remains active.",

            Details =
                CreateRecoveryDetails(
                    initializationResponse),

            Severity =
                IssueSeverity.Warning
        };

        Response<ErrorCatalogInitializationPayload> response =
            Response<ErrorCatalogInitializationPayload>
                .OkWithWarnings(
                    payload,
                    [warning]);

        return Response<ErrorCatalogInitializationPayload>
            .WithMessage(
                response,
                "The previous valid error catalog "
                + "context was retained.");
    }

    private static Response<ErrorCatalogInitializationPayload>
        CreateBuiltInFallbackWarningResponse(
            ErrorCatalogInitializationPayload payload,
            Response<ErrorCatalogInitializationPayload>
                initializationResponse)
    {
        IssueInfo warning = new()
        {
            Code = "WIF_DEFAULT_FALLBACK_ACTIVATED",

            Message =
                "The configured error catalog could not "
                + "be activated. The bundled Afrowave "
                + "default catalog is active.",

            Details =
                CreateRecoveryDetails(
                    initializationResponse),

            Severity =
                IssueSeverity.Warning
        };

        Response<ErrorCatalogInitializationPayload> response =
            Response<ErrorCatalogInitializationPayload>
                .OkWithWarnings(
                    payload,
                    [warning]);

        return Response<ErrorCatalogInitializationPayload>
            .WithMessage(
                response,
                "The bundled default error catalog "
                + "was activated.");
    }

    private static Response<ErrorCatalogInitializationPayload>
        CreateBuiltInFallbackFailureResponse(
            Response<ErrorCatalogInitializationPayload>
                initializationResponse,
            Response<ErrorCatalogContext> fallbackResponse)
    {
        string fallbackCode =
            fallbackResponse.Issues.Count > 0
                ? fallbackResponse.Issues[0].Code
                : fallbackResponse.Data is null
                    ? "WIF_BUILT_IN_CONTEXT_PAYLOAD_NULL"
                    : "WIF_BUILT_IN_CONTEXT_LOAD_FAILED";

        string fallbackMessage =
            string.IsNullOrWhiteSpace(
                fallbackResponse.Message)
                    ? fallbackResponse.Data is null
                        ? "The bundled default catalog "
                          + "provider returned no context."
                        : "The bundled default error catalog "
                          + "could not be loaded."
                    : fallbackResponse.Message;

        ResultStatus failureStatus =
            fallbackResponse.IsSuccess
                ? ResultStatus.Invalid
                : fallbackResponse.Status;

        Response<ErrorCatalogInitializationPayload> response =
            Response<ErrorCatalogInitializationPayload>
                .WithStatus(
                    Response<
                        ErrorCatalogInitializationPayload>.Fail(
                            code:
                                "WIF_DEFAULT_FALLBACK_FAILED",

                            message:
                                "The configured error catalog "
                                + "failed and the bundled default "
                                + "catalog could not be activated."),

                    failureStatus);

        response =
            AddInitializationFailureMetadata(
                response,
                initializationResponse,
                prefix:
                    "WhenItFails.ProjectFailure");

        response =
            Response<ErrorCatalogInitializationPayload>
                .AddMetadata(
                    response,
                    "WhenItFails.FallbackFailure.Code",
                    fallbackCode);

        response =
            Response<ErrorCatalogInitializationPayload>
                .AddMetadata(
                    response,
                    "WhenItFails.FallbackFailure.Status",
                    failureStatus.ToString());

        return Response<ErrorCatalogInitializationPayload>
            .AddMetadata(
                response,
                "WhenItFails.FallbackFailure.Message",
                fallbackMessage);
    }

    private static Response<ErrorCatalogInitializationPayload>
        AddRecoveryMetadata(
            Response<ErrorCatalogInitializationPayload> response,
            Response<ErrorCatalogInitializationPayload>
                initializationResponse)
    {
        string failureCode =
            initializationResponse.Issues.Count > 0
                ? initializationResponse.Issues[0].Code
                : "WIF_INITIALIZATION_FAILED";

        string failureMessage =
            string.IsNullOrWhiteSpace(
                initializationResponse.Message)
                    ? "The requested error catalog "
                      + "initialization failed."
                    : initializationResponse.Message;

        Response<ErrorCatalogInitializationPayload> result =
            Response<ErrorCatalogInitializationPayload>
                .AddMetadata(
                    response,
                    "WhenItFails.RecoveryReasonCode",
                    failureCode);

        result =
            Response<ErrorCatalogInitializationPayload>
                .AddMetadata(
                    result,
                    "WhenItFails.RecoveryStatus",
                    initializationResponse.Status.ToString());

        return Response<ErrorCatalogInitializationPayload>
            .AddMetadata(
                result,
                "WhenItFails.RecoveryMessage",
                failureMessage);
    }

    private static Response<ErrorCatalogInitializationPayload>
        AddInitializationFailureMetadata(
            Response<ErrorCatalogInitializationPayload> response,
            Response<ErrorCatalogInitializationPayload>
                failureResponse,
            string prefix)
    {
        string failureCode =
            failureResponse.Issues.Count > 0
                ? failureResponse.Issues[0].Code
                : "WIF_INITIALIZATION_FAILED";

        string failureMessage =
            string.IsNullOrWhiteSpace(
                failureResponse.Message)
                    ? "The requested error catalog "
                      + "initialization failed."
                    : failureResponse.Message;

        Response<ErrorCatalogInitializationPayload> result =
            Response<ErrorCatalogInitializationPayload>
                .AddMetadata(
                    response,
                    $"{prefix}.Code",
                    failureCode);

        result =
            Response<ErrorCatalogInitializationPayload>
                .AddMetadata(
                    result,
                    $"{prefix}.Status",
                    failureResponse.Status.ToString());

        return Response<ErrorCatalogInitializationPayload>
            .AddMetadata(
                result,
                $"{prefix}.Message",
                failureMessage);
    }

    private static string CreateRecoveryDetails(
        Response<ErrorCatalogInitializationPayload>
            initializationResponse)
    {
        if (initializationResponse.Issues.Count == 0)
        {
            return string.IsNullOrWhiteSpace(
                initializationResponse.Message)
                    ? "No additional initialization "
                      + "diagnostics were provided."
                    : initializationResponse.Message;
        }

        return string.Join(
            " | ",
            initializationResponse.Issues.Select(
                issue =>
                    $"{issue.Code}: {issue.Message}"));
    }

    private static JsonsBootstrapPayload
        CreateBootstrapSnapshot(
            JsonsOptions options)
    {
        return new JsonsBootstrapPayload
        {
            RootDirectory =
                options.RootDirectory,

            PackageDirectoryPath =
                options.PackageDirectoryPath
        };
    }

    private static Response<TTarget>
        ForwardContextFailure<TTarget>(
            Response<ErrorCatalogContext> sourceResponse)
    {
        string issueCode =
            sourceResponse.Issues.Count > 0
                ? sourceResponse.Issues[0].Code
                : "ErrorCatalogContextUnavailable";

        string message =
            string.IsNullOrWhiteSpace(
                sourceResponse.Message)
                    ? "The initialized error catalog "
                      + "context is unavailable."
                    : sourceResponse.Message;

        return Response<TTarget>.WithStatus(
            Response<TTarget>.Fail(
                code: issueCode,
                message: message),
            sourceResponse.Status);
    }
}

