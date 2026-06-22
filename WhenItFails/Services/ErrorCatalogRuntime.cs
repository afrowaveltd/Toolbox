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
    private readonly IErrorCatalogContextStore _contextStore;
    private readonly IErrorDescriptorService _descriptorService;
    private readonly IErrorProfileSelectionService _profileSelectionService;
    private readonly WhenItFailsOptions _options;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="ErrorCatalogRuntime"/> class.
    /// </summary>
    public ErrorCatalogRuntime(
    IErrorCatalogInitializer initializer,
    WhenItFailsOptions options,
    IErrorCatalogContextStore contextStore,
    IErrorDescriptorService descriptorService,
    IErrorProfileSelectionService profileSelectionService)
    {
        _initializer = initializer
            ?? throw new ArgumentNullException(nameof(initializer));

        _options = options
            ?? throw new ArgumentNullException(nameof(options));

        _contextStore = contextStore
            ?? throw new ArgumentNullException(nameof(contextStore));

        _descriptorService = descriptorService
            ?? throw new ArgumentNullException(nameof(descriptorService));

        _profileSelectionService = profileSelectionService
            ?? throw new ArgumentNullException(nameof(profileSelectionService));
    }

    /// <inheritdoc />
    public Task<Response<ErrorCatalogInitializationPayload>> InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        JsonsOptions jsonsOptions =
            _options.Jsons ?? new JsonsOptions();

        return InitializeCoreAsync(
            jsonsOptions,
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<Response<ErrorCatalogInitializationPayload>> InitializeAsync(
        JsonsOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        return InitializeCoreAsync(
            options,
            cancellationToken);
    }



    /// <inheritdoc />
    public Response<ErrorDescriptor> FromId(string errorId)
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
    public Response<ErrorDescriptor> FromName(string errorName)
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
    public Response<ErrorDescriptor> FromCode(int code)
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
    public Response<IReadOnlyList<ErrorDefinition>> ResolveProfile(
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

    private static Response<TTarget> ForwardContextFailure<TTarget>(
        Response<ErrorCatalogContext> sourceResponse)
    {
        string issueCode = sourceResponse.Issues.Count > 0
            ? sourceResponse.Issues[0].Code
            : "ErrorCatalogContextUnavailable";

        string message = string.IsNullOrWhiteSpace(
            sourceResponse.Message)
                ? "The initialized error catalog context is unavailable."
                : sourceResponse.Message;

        return Response<TTarget>.WithStatus(
            Response<TTarget>.Fail(
                code: issueCode,
                message: message),
            sourceResponse.Status);
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

        if (!previousContextResponse.IsSuccess
            || previousContextResponse.Data is null)
        {
            return initializationResponse;
        }

        ErrorCatalogInitializationPayload recoveryPayload = new()
        {
            Bootstrap =
                CreateBootstrapSnapshot(options),

            Context =
                previousContextResponse.Data,

            ContextSource =
                ErrorCatalogContextSource.PreviousContext,

            KeptPreviousContext = true,
            UsedFallback = false
        };

        Response<ErrorCatalogInitializationPayload> recoveryResponse =
            _options.HideRecoverableFailures == true
                ? Response<ErrorCatalogInitializationPayload>.Ok(
                    recoveryPayload,
                    "The previous valid error catalog context was retained.")
                : CreateRecoveryWarningResponse(
                    recoveryPayload,
                    initializationResponse);

        return AddRecoveryMetadata(
            recoveryResponse,
            initializationResponse);
    }

    private static Response<ErrorCatalogInitializationPayload>
        CreateRecoveryWarningResponse(
            ErrorCatalogInitializationPayload payload,
            Response<ErrorCatalogInitializationPayload>
                initializationResponse)
    {
        IssueInfo warning = new()
        {
            Code = "WIF_PREVIOUS_CONTEXT_RETAINED",
            Message =
                "The new error catalog context could not be activated. "
                + "The previous valid context remains active.",
            Details =
                CreateRecoveryDetails(initializationResponse),
            Severity = IssueSeverity.Warning
        };

        Response<ErrorCatalogInitializationPayload> response =
            Response<ErrorCatalogInitializationPayload>
                .OkWithWarnings(
                    payload,
                    [warning]);

        return Response<ErrorCatalogInitializationPayload>
            .WithMessage(
                response,
                "The previous valid error catalog context was retained.");
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
                    ? "The requested error catalog initialization failed."
                    : initializationResponse.Message;

        Response<ErrorCatalogInitializationPayload> result =
            Response<ErrorCatalogInitializationPayload>.AddMetadata(
                response,
                "WhenItFails.RecoveryReasonCode",
                failureCode);

        result =
            Response<ErrorCatalogInitializationPayload>.AddMetadata(
                result,
                "WhenItFails.RecoveryStatus",
                initializationResponse.Status.ToString());

        return Response<ErrorCatalogInitializationPayload>.AddMetadata(
            result,
            "WhenItFails.RecoveryMessage",
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
                    ? "No additional initialization diagnostics were provided."
                    : initializationResponse.Message;
        }

        return string.Join(
            " | ",
            initializationResponse.Issues.Select(
                issue =>
                    $"{issue.Code}: {issue.Message}"));
    }

    private static JsonsBootstrapPayload CreateBootstrapSnapshot(
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

}