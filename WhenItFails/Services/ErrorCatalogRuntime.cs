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
using Afrowave.Toolbox.WhenItFails.Runtime;

namespace Afrowave.Toolbox.WhenItFails.Services;

/// <summary>
/// Default high-level facade over the complete WhenItFails runtime.
/// </summary>
public sealed class ErrorCatalogRuntime : IErrorCatalogRuntime, IErrorCatalogRuntimePublicationReader, IErrorCatalogRuntimeActivationReader, IErrorCatalogRuntimeCombinedObservationReader, IErrorCatalogRuntimeSupportingObservationReader, IErrorCatalogRuntimeFullObservationReader
{
    private readonly IErrorCatalogInitializer _initializer;
    private readonly WhenItFailsOptions _options;
    private readonly IErrorCatalogContextStore _contextStore;
    private readonly IBuiltInErrorCatalogContextProvider
        _builtInContextProvider;
    private readonly IErrorDescriptorService _descriptorService;
    private readonly IErrorProfileSelectionService _profileSelectionService;
    // Serializes activation operations on this runtime instance; readers and
    // direct writes to an injected store are not blocked by this gate.
    private readonly SemaphoreSlim _activationGate = new(1, 1);
    private ErrorCatalogRuntimeStatus? _currentStatus;
    private long _activationSequence;
    private CompletedActivation? _completedActivation;

    private sealed record CompletedActivation(
        long Sequence,
        ErrorCatalogContextPublication Publication,
        ErrorCatalogRuntimeStatus Status);

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
        await _activationGate.WaitAsync(cancellationToken);
        try
        {
            return await ResetToDefaultsCoreAsync(cancellationToken);
        }
        finally
        {
            _activationGate.Release();
        }
    }

    private async Task<Response<ErrorCatalogInitializationPayload>>
        ResetToDefaultsCoreAsync(
            CancellationToken cancellationToken)
    {
        Response<ErrorCatalogContext>? builtInResponse;

        try
        {
            builtInResponse =
                await _builtInContextProvider.LoadAsync(
                    cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return Response<ErrorCatalogInitializationPayload>.Fail(
                code: "WIF_BUILT_IN_CONTEXT_PROVIDER_FAILED",
                message: "The bundled default catalog provider failed.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        if (builtInResponse is null)
        {
            return Response<ErrorCatalogInitializationPayload>.Invalid(
                code: "WIF_BUILT_IN_CONTEXT_RESPONSE_NULL",
                message:
                    "The bundled default catalog provider returned "
                    + "a null response.");
        }

        if (!builtInResponse.IsSuccess
            || builtInResponse.Data is null)
        {
            return CreateResetToDefaultsFailureResponse(
                builtInResponse);
        }

        cancellationToken.ThrowIfCancellationRequested();

        ErrorCatalogContextPublication? ownedPublication;

        try
        {
            ownedPublication = PublishContext(builtInResponse.Data);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return Response<ErrorCatalogInitializationPayload>.Fail(
                code: "WIF_CONTEXT_STORE_FAILED",
                message: "The error catalog context store failed.");
        }

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
            UsedFallback = false,
            OwnedPublication = ownedPublication
        };
        RecordStatus(
            payload);
        return Response<ErrorCatalogInitializationPayload>.Ok(
            payload,
            "The bundled default error catalog was activated.");
    }

    /// <inheritdoc />
    public Response<ErrorCatalogContext> GetCurrentContext()
    {
        return GetCurrentContextResponse();
    }

    /// <inheritdoc />
    public Response<ErrorCatalogContextPublication> GetCurrentPublication()
    {
        if (_contextStore is not IErrorCatalogContextPublicationReader reader)
        {
            return Response<ErrorCatalogContextPublication>.NotSupported(
                data: null,
                code: "WIF_CONTEXT_PUBLICATION_NOT_SUPPORTED",
                message: "The configured context store does not support publication identity.");
        }

        try
        {
            return reader.GetCurrentPublication()
                ?? Response<ErrorCatalogContextPublication>.Invalid(
                    code: "WIF_CONTEXT_PUBLICATION_RESPONSE_NULL",
                    message: "The context store returned a null publication response.");
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return Response<ErrorCatalogContextPublication>.Fail(
                code: "WIF_CONTEXT_PUBLICATION_FAILED",
                message: "The active context publication could not be read.");
        }
    }

    /// <inheritdoc />
    public Response<ErrorCatalogActivationStatusSnapshot> GetCompletedActivation()
    {
        if (_contextStore is not IErrorCatalogContextPublicationReader reader)
        {
            return Response<ErrorCatalogActivationStatusSnapshot>.NotSupported(
                data: null,
                code: "WIF_ACTIVATION_STATUS_NOT_SUPPORTED",
                message: "The configured context store does not support publication identity.");
        }

        CompletedActivation? completed =
            Volatile.Read(ref _completedActivation);

        if (completed is null)
        {
            return Response<ErrorCatalogActivationStatusSnapshot>.Invalid(
                code: "WIF_ACTIVATION_STATUS_UNAVAILABLE",
                message: "No completed activation status observation is available.");
        }

        if (!ReferenceEquals(
            completed.Status,
            Volatile.Read(ref _currentStatus)))
        {
            return Response<ErrorCatalogActivationStatusSnapshot>.Invalid(
                code: "WIF_ACTIVATION_STATUS_PENDING",
                message: "The runtime status has changed since the selected activation observation.");
        }

        Response<ErrorCatalogContextPublication>? publicationResponse;

        try
        {
            publicationResponse = reader.GetCurrentPublication();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return Response<ErrorCatalogActivationStatusSnapshot>.Fail(
                code: "WIF_ACTIVATION_PUBLICATION_READ_FAILED",
                message: "The active context publication could not be read.");
        }

        if (publicationResponse is not { IsSuccess: true, Data: { } publication }
            || !ReferenceEquals(publication, completed.Publication))
        {
            return Response<ErrorCatalogActivationStatusSnapshot>.Invalid(
                code: "WIF_ACTIVATION_PUBLICATION_CHANGED",
                message: "The active context publication has changed since the recorded status.");
        }

        // A separate store writer can still publish immediately after this
        // check. The observation identifies the selected completed record;
        // it is not a lock on the active store or its mutable context.
        return Response<ErrorCatalogActivationStatusSnapshot>.Ok(
            new ErrorCatalogActivationStatusSnapshot(
                completed.Publication.StoreId,
                completed.Publication.Generation,
                completed.Sequence,
                completed.Status));
    }

    /// <inheritdoc />
    public Response<ErrorCatalogCompletedCombinedSnapshot>
        GetCompletedCombinedSnapshot()
    {
        if (_contextStore is not IErrorCatalogContextPublicationReader reader)
        {
            return Response<ErrorCatalogCompletedCombinedSnapshot>.NotSupported(
                data: null,
                code: "WIF_COMPLETED_COMBINED_NOT_SUPPORTED",
                message: "The context store does not support publication identity.");
        }

        // Select a completed status and its exact associated publication
        // record once. Do not independently select a later active context.
        CompletedActivation? selected = Volatile.Read(ref _completedActivation);

        if (selected is null)
        {
            return Response<ErrorCatalogCompletedCombinedSnapshot>.Invalid(
                code: "WIF_COMPLETED_COMBINED_UNAVAILABLE",
                message: "No completed catalog and status observation is available.");
        }

        if (!ReferenceEquals(selected.Status, Volatile.Read(ref _currentStatus)))
        {
            return Response<ErrorCatalogCompletedCombinedSnapshot>.Invalid(
                code: "WIF_COMPLETED_COMBINED_STATUS_CHANGED",
                message: "The recorded runtime status has changed.");
        }

        try
        {
            Response<ErrorCatalogContextPublication>? before =
                reader.GetCurrentPublication();

            if (before is not { IsSuccess: true, Data: { } current }
                || !ReferenceEquals(current, selected.Publication))
            {
                return Response<ErrorCatalogCompletedCombinedSnapshot>.Invalid(
                    code: "WIF_COMPLETED_COMBINED_PUBLICATION_CHANGED",
                    message: "The selected context publication is no longer current.");
            }

            Response<ErrorCatalogCombinedSnapshot> capture =
                ErrorCatalogCombinedSnapshotExtensions.CaptureFromContext(
                    selected.Publication.Context);

            if (!capture.IsSuccess || capture.Data is null)
            {
                return new Response<ErrorCatalogCompletedCombinedSnapshot>
                {
                    Status = capture.Status,
                    Message = capture.Message,
                    Issues = capture.Issues,
                    Metadata = capture.Metadata
                };
            }

            // A second read is a consistency check against the SAME selected
            // record, not an independent selection of a new catalog.
            Response<ErrorCatalogContextPublication>? after =
                reader.GetCurrentPublication();

            if (after is not { IsSuccess: true, Data: { } latest }
                || !ReferenceEquals(latest, selected.Publication))
            {
                return Response<ErrorCatalogCompletedCombinedSnapshot>.Invalid(
                    code: "WIF_COMPLETED_COMBINED_PUBLICATION_CHANGED",
                    message: "The context publication changed during capture.");
            }

            if (!ReferenceEquals(selected, Volatile.Read(ref _completedActivation))
                || !ReferenceEquals(selected.Status, Volatile.Read(ref _currentStatus)))
            {
                return Response<ErrorCatalogCompletedCombinedSnapshot>.Invalid(
                    code: "WIF_COMPLETED_COMBINED_STATUS_CHANGED",
                    message: "The runtime status changed during capture.");
            }

            return Response<ErrorCatalogCompletedCombinedSnapshot>.Ok(
                new ErrorCatalogCompletedCombinedSnapshot(
                    selected.Publication.StoreId,
                    selected.Publication.Generation,
                    selected.Sequence,
                    selected.Status,
                    capture.Data));
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return Response<ErrorCatalogCompletedCombinedSnapshot>.Fail(
                code: "WIF_COMPLETED_COMBINED_FAILED",
                message: "The combined catalog and status could not be captured.");
        }
    }

    /// <inheritdoc />
    public Response<ErrorCatalogCompletedSupportingCatalogsSnapshot>
        GetCompletedSupportingCatalogsSnapshot()
    {
        if (_contextStore is not IErrorCatalogContextPublicationReader reader)
        {
            return Response<ErrorCatalogCompletedSupportingCatalogsSnapshot>.NotSupported(
                data: null,
                code: "WIF_COMPLETED_SUPPORTING_NOT_SUPPORTED",
                message: "The context store does not support publication identity.");
        }

        // Select the completed activation with its owned publication record.
        // A new, independent context or status read would break this pairing.
        CompletedActivation? selected = Volatile.Read(ref _completedActivation);

        if (selected is null)
        {
            return Response<ErrorCatalogCompletedSupportingCatalogsSnapshot>.Invalid(
                code: "WIF_COMPLETED_SUPPORTING_UNAVAILABLE",
                message: "No completed supporting catalog and status observation is available.");
        }

        if (!ReferenceEquals(selected.Status, Volatile.Read(ref _currentStatus)))
        {
            return Response<ErrorCatalogCompletedSupportingCatalogsSnapshot>.Invalid(
                code: "WIF_COMPLETED_SUPPORTING_STATUS_CHANGED",
                message: "The recorded runtime status has changed.");
        }

        try
        {
            Response<ErrorCatalogContextPublication>? before =
                reader.GetCurrentPublication();

            if (before is not { IsSuccess: true, Data: { } current }
                || !ReferenceEquals(current, selected.Publication))
            {
                return Response<ErrorCatalogCompletedSupportingCatalogsSnapshot>.Invalid(
                    code: "WIF_COMPLETED_SUPPORTING_PUBLICATION_CHANGED",
                    message: "The selected context publication is no longer current.");
            }

            Response<ErrorSupportingCatalogsSnapshot> captured =
                ErrorSupportingCatalogsSnapshotExtensions.CaptureFromContext(
                    selected.Publication.Context);

            if (!captured.IsSuccess || captured.Data is null)
            {
                return new Response<ErrorCatalogCompletedSupportingCatalogsSnapshot>
                {
                    Status = captured.Status,
                    Message = captured.Message,
                    Issues = captured.Issues,
                    Metadata = captured.Metadata
                };
            }

            // Recheck the very same publication record after copying all four
            // catalogs. This second read is not a new context selection.
            Response<ErrorCatalogContextPublication>? after =
                reader.GetCurrentPublication();

            if (after is not { IsSuccess: true, Data: { } latest }
                || !ReferenceEquals(latest, selected.Publication))
            {
                return Response<ErrorCatalogCompletedSupportingCatalogsSnapshot>.Invalid(
                    code: "WIF_COMPLETED_SUPPORTING_PUBLICATION_CHANGED",
                    message: "The context publication changed during capture.");
            }

            if (!ReferenceEquals(selected, Volatile.Read(ref _completedActivation))
                || !ReferenceEquals(selected.Status, Volatile.Read(ref _currentStatus)))
            {
                return Response<ErrorCatalogCompletedSupportingCatalogsSnapshot>.Invalid(
                    code: "WIF_COMPLETED_SUPPORTING_STATUS_CHANGED",
                    message: "The runtime status changed during capture.");
            }

            return Response<ErrorCatalogCompletedSupportingCatalogsSnapshot>.Ok(
                new ErrorCatalogCompletedSupportingCatalogsSnapshot(
                    selected.Publication.StoreId,
                    selected.Publication.Generation,
                    selected.Sequence,
                    selected.Status,
                    captured.Data));
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return Response<ErrorCatalogCompletedSupportingCatalogsSnapshot>.Fail(
                code: "WIF_COMPLETED_SUPPORTING_FAILED",
                message: "The supporting catalog and status observation could not be captured.");
        }
    }

    /// <inheritdoc />
    public Response<ErrorCatalogCompletedFullSnapshot> GetCompletedFullSnapshot()
    {
        if (_contextStore is not IErrorCatalogContextPublicationReader reader)
        {
            return Response<ErrorCatalogCompletedFullSnapshot>.NotSupported(
                data: null,
                code: "WIF_COMPLETED_FULL_NOT_SUPPORTED",
                message: "The context store does not support publication identity.");
        }

        // Select one completed activation, not independently observed status,
        // data or publication generations.
        CompletedActivation? selected = Volatile.Read(ref _completedActivation);

        if (selected is null)
        {
            return Response<ErrorCatalogCompletedFullSnapshot>.Invalid(
                code: "WIF_COMPLETED_FULL_UNAVAILABLE",
                message: "No completed catalog and status observation is available.");
        }

        if (!ReferenceEquals(selected.Status, Volatile.Read(ref _currentStatus)))
        {
            return Response<ErrorCatalogCompletedFullSnapshot>.Invalid(
                code: "WIF_COMPLETED_FULL_STATUS_CHANGED",
                message: "The recorded runtime status has changed.");
        }

        try
        {
            Response<ErrorCatalogContextPublication>? before =
                reader.GetCurrentPublication();

            if (before is not { IsSuccess: true, Data: { } current }
                || !ReferenceEquals(current, selected.Publication))
            {
                return Response<ErrorCatalogCompletedFullSnapshot>.Invalid(
                    code: "WIF_COMPLETED_FULL_PUBLICATION_CHANGED",
                    message: "The selected context publication is no longer current.");
            }

            ErrorCatalogContext context = selected.Publication.Context;

            Response<ErrorCatalogCombinedSnapshot> main =
                ErrorCatalogCombinedSnapshotExtensions.CaptureFromContext(context);

            if (!main.IsSuccess || main.Data is null)
            {
                return new Response<ErrorCatalogCompletedFullSnapshot>
                {
                    Status = main.Status,
                    Message = main.Message,
                    Issues = main.Issues,
                    Metadata = main.Metadata
                };
            }

            // Share the already detached category projection rather than
            // copying its source document again for the supporting view.
            Response<ErrorSupportingCatalogsSnapshot> supporting =
                ErrorSupportingCatalogsSnapshotExtensions.CaptureFromContext(
                    context, main.Data.CategoryCatalog);

            if (!supporting.IsSuccess || supporting.Data is null)
            {
                return new Response<ErrorCatalogCompletedFullSnapshot>
                {
                    Status = supporting.Status,
                    Message = supporting.Message,
                    Issues = supporting.Issues,
                    Metadata = supporting.Metadata
                };
            }

            // Confirm publication and status are still associated with the
            // selected activation AFTER capturing every catalog and issue.
            Response<ErrorCatalogContextPublication>? after =
                reader.GetCurrentPublication();

            if (after is not { IsSuccess: true, Data: { } latest }
                || !ReferenceEquals(latest, selected.Publication))
            {
                return Response<ErrorCatalogCompletedFullSnapshot>.Invalid(
                    code: "WIF_COMPLETED_FULL_PUBLICATION_CHANGED",
                    message: "The context publication changed during capture.");
            }

            if (!ReferenceEquals(selected, Volatile.Read(ref _completedActivation))
                || !ReferenceEquals(selected.Status, Volatile.Read(ref _currentStatus)))
            {
                return Response<ErrorCatalogCompletedFullSnapshot>.Invalid(
                    code: "WIF_COMPLETED_FULL_STATUS_CHANGED",
                    message: "The runtime status changed during capture.");
            }

            ErrorCatalogFullSnapshot detached = new(main.Data, supporting.Data);

            return Response<ErrorCatalogCompletedFullSnapshot>.Ok(
                new ErrorCatalogCompletedFullSnapshot(
                    selected.Publication.StoreId,
                    selected.Publication.Generation,
                    selected.Sequence,
                    selected.Status,
                    detached));
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return Response<ErrorCatalogCompletedFullSnapshot>.Fail(
                code: "WIF_COMPLETED_FULL_FAILED",
                message: "The full catalog and status observation could not be captured.");
        }
    }

    /// <inheritdoc />
    public Response<ErrorCatalogRuntimeStatus> GetStatus()
    {
        ErrorCatalogRuntimeStatus? currentStatus =
            Volatile.Read(
                ref _currentStatus);

        return currentStatus is null
            ? Response<ErrorCatalogRuntimeStatus>.Invalid(
                code: "WIF_RUNTIME_STATUS_UNAVAILABLE",
                message:
                    "The error catalog runtime has not activated "
                    + "a catalog context yet.")
            : Response<ErrorCatalogRuntimeStatus>.Ok(
                currentStatus);
    }

    /// <inheritdoc />
    public Response<ErrorDescriptor> FromId(
        string errorId)
    {
        Response<ErrorCatalogContext> contextResponse =
            GetCurrentContextResponse();

        if (!contextResponse.IsSuccess
            || contextResponse.Data is null)
        {
            return ForwardContextFailure<ErrorDescriptor>(
                contextResponse);
        }

        return ResolveDescriptor(
            () => _descriptorService.FromId(
                contextResponse.Data,
                errorId));
    }

    /// <inheritdoc />
    public Response<ErrorDescriptor> FromName(
        string errorName)
    {
        Response<ErrorCatalogContext> contextResponse =
            GetCurrentContextResponse();

        if (!contextResponse.IsSuccess
            || contextResponse.Data is null)
        {
            return ForwardContextFailure<ErrorDescriptor>(
                contextResponse);
        }

        return ResolveDescriptor(
            () => _descriptorService.FromName(
                contextResponse.Data,
                errorName));
    }

    /// <inheritdoc />
    public Response<ErrorDescriptor> FromCode(
        int code)
    {
        Response<ErrorCatalogContext> contextResponse =
            GetCurrentContextResponse();

        if (!contextResponse.IsSuccess
            || contextResponse.Data is null)
        {
            return ForwardContextFailure<ErrorDescriptor>(
                contextResponse);
        }

        return ResolveDescriptor(
            () => _descriptorService.FromCode(
                contextResponse.Data,
                code));
    }

    private static Response<ErrorDescriptor> ResolveDescriptor(
        Func<Response<ErrorDescriptor>> resolveDescriptor)
    {
        Response<ErrorDescriptor>? response;

        try
        {
            response = resolveDescriptor();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return Response<ErrorDescriptor>.Fail(
                code: "WIF_DESCRIPTOR_SERVICE_FAILED",
                message: "The error descriptor service failed.");
        }

        return response
            ?? CreateNullDescriptorServiceResponse();
    }

    /// <inheritdoc />
    public Response<IReadOnlyList<ErrorDefinition>>
        ResolveProfile(
            string profileName)
    {
        Response<ErrorCatalogContext> contextResponse =
            GetCurrentContextResponse();

        if (!contextResponse.IsSuccess
            || contextResponse.Data is null)
        {
            return ForwardContextFailure<
                IReadOnlyList<ErrorDefinition>>(
                    contextResponse);
        }

        Response<IReadOnlyList<ErrorDefinition>>? response;

        try
        {
            response = _profileSelectionService.ResolveByProfileName(
                contextResponse.Data,
                profileName);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return Response<IReadOnlyList<ErrorDefinition>>.Fail(
                code: "WIF_PROFILE_SELECTION_FAILED",
                message: "The error profile selection service failed.");
        }

        return response
            ?? Response<IReadOnlyList<ErrorDefinition>>.Invalid(
                code: "WIF_PROFILE_SELECTION_RESPONSE_NULL",
                message:
                    "The error profile selection service returned "
                    + "a null response.");
    }

    private Response<ErrorCatalogContext>
        GetCurrentContextResponse()
    {
        Response<ErrorCatalogContext>? response;

        try
        {
            response = _contextStore.GetCurrent();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return Response<ErrorCatalogContext>.Fail(
                code: "WIF_CONTEXT_STORE_FAILED",
                message: "The error catalog context store failed.");
        }

        return response
            ?? Response<ErrorCatalogContext>.Invalid(
                code: "WIF_CONTEXT_STORE_RESPONSE_NULL",
                message:
                    "The error catalog context store returned "
                    + "a null response.");
    }

    private static Response<ErrorDescriptor>
        CreateNullDescriptorServiceResponse()
    {
        return Response<ErrorDescriptor>.Invalid(
            code: "WIF_DESCRIPTOR_SERVICE_RESPONSE_NULL",
            message:
                "The error descriptor service returned a null response.");
    }

    private static Response<ErrorCatalogInitializationPayload>
    CreateResetToDefaultsFailureResponse(
        Response<ErrorCatalogContext> builtInResponse)
    {
        string failureCode =
            builtInResponse.Issues is { Count: > 0 }
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
        await _activationGate.WaitAsync(cancellationToken);
        try
        {
            return await InitializeCoreLockedAsync(options, cancellationToken);
        }
        finally
        {
            _activationGate.Release();
        }
    }

    private async Task<Response<ErrorCatalogInitializationPayload>>
        InitializeCoreLockedAsync(
            JsonsOptions options,
            CancellationToken cancellationToken)
    {
        Response<ErrorCatalogInitializationPayload>?
            initializationResponse;

        try
        {
            initializationResponse =
                await _initializer.InitializeAsync(
                    options,
                    cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return Response<ErrorCatalogInitializationPayload>.Fail(
                code: "WIF_INITIALIZER_FAILED",
                message: "The error catalog initializer failed.");
        }

        if (initializationResponse is null)
        {
            return Response<ErrorCatalogInitializationPayload>.Invalid(
                code: "WIF_INITIALIZER_RESPONSE_NULL",
                message:
                    "The error catalog initializer returned a null response.");
        }

        if (initializationResponse.IsSuccess)
        {
            if (initializationResponse.Data is null)
            {
                return Response<ErrorCatalogInitializationPayload>.Invalid(
                    code: "WIF_INITIALIZATION_PAYLOAD_NULL",
                    message:
                        "The error catalog initializer returned success "
                        + "with a null payload.");
            }

            if (initializationResponse.Data.Bootstrap is null)
            {
                return Response<ErrorCatalogInitializationPayload>.Invalid(
                    code: "WIF_INITIALIZATION_BOOTSTRAP_NULL",
                    message:
                        "The successful error catalog initialization payload "
                        + "has a null bootstrap value.");
            }

            if (initializationResponse.Data.Context is null)
            {
                return Response<ErrorCatalogInitializationPayload>.Invalid(
                    code: "WIF_INITIALIZATION_CONTEXT_NULL",
                    message:
                        "The successful error catalog initialization payload "
                        + "has a null context value.");
            }

            RecordStatus(
                initializationResponse.Data);

            return initializationResponse;
        }

        if (_options.InitializationMode
            != ErrorCatalogInitializationMode.Flexible)
        {
            return initializationResponse;
        }

        // Select a complete context + generation record in one read.
        // A later store read may already belong to another writer, even if
        // both publications contain the exact same context object.
        if (_contextStore is IErrorCatalogContextPublicationReader publicationReader)
        {
            try
            {
                Response<ErrorCatalogContextPublication>? selectedResponse =
                    publicationReader.GetCurrentPublication();

                if (selectedResponse is { IsSuccess: true, Data: { } selected })
                {
                    return CreatePreviousContextRecoveryResponse(
                        options,
                        selected.Context,
                        initializationResponse,
                        selected);
                }
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                // Preserve the existing legacy recovery path for custom
                // stores with an unavailable or failing optional reader.
                // Such a path must not claim strict publication selection.
            }
        }

        Response<ErrorCatalogContext> previousContextResponse =
            GetCurrentContextResponse();

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
                initializationResponse,
            ErrorCatalogContextPublication? selectedPublication = null)
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
            UsedFallback = false,
            SelectedPublication = selectedPublication
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

        RecordStatus(
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
        Response<ErrorCatalogContext>? fallbackResponse;

        try
        {
            fallbackResponse =
                await _builtInContextProvider.LoadAsync(
                    cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            fallbackResponse = Response<ErrorCatalogContext>.Fail(
                code: "WIF_BUILT_IN_CONTEXT_PROVIDER_FAILED",
                message: "The bundled default catalog provider failed.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        if (fallbackResponse is null)
        {
            fallbackResponse = Response<ErrorCatalogContext>.Invalid(
                code: "WIF_BUILT_IN_CONTEXT_RESPONSE_NULL",
                message:
                    "The bundled default catalog provider returned "
                    + "a null response.");
        }

        if (!fallbackResponse.IsSuccess
            || fallbackResponse.Data is null)
        {
            return CreateBuiltInFallbackFailureResponse(
                initializationResponse,
                fallbackResponse);
        }

        cancellationToken.ThrowIfCancellationRequested();

        ErrorCatalogContextPublication? ownedPublication;

        try
        {
            ownedPublication = PublishContext(fallbackResponse.Data);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            Response<ErrorCatalogContext> storeFailure =
                Response<ErrorCatalogContext>.Fail(
                    code: "WIF_CONTEXT_STORE_FAILED",
                    message: "The error catalog context store failed.");

            return CreateBuiltInFallbackFailureResponse(
                initializationResponse,
                storeFailure);
        }

        ErrorCatalogInitializationPayload fallbackPayload = new()
        {
            Bootstrap =
                CreateBootstrapSnapshot(options),

            Context =
                fallbackResponse.Data,

            ContextSource =
                ErrorCatalogContextSource.BuiltInDefaults,

            KeptPreviousContext = false,
            UsedFallback = true,
            OwnedPublication = ownedPublication
        };
        RecordStatus(
      fallbackPayload,
      initializationResponse);

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
            fallbackResponse.Issues is { Count: > 0 }
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
            initializationResponse.Issues is { Count: > 0 }
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
            failureResponse.Issues is { Count: > 0 }
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
        if (initializationResponse.Issues is not { Count: > 0 })
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
        if (sourceResponse.IsSuccess
            && sourceResponse.Data is null)
        {
            return Response<TTarget>.Invalid(
                code: "WIF_CURRENT_CONTEXT_PAYLOAD_NULL",
                message:
                    "The current error catalog context payload is null.");
        }

        string issueCode =
            sourceResponse.Issues is { Count: > 0 }
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

    private ErrorCatalogContextPublication? PublishContext(
        ErrorCatalogContext context)
    {
        if (_contextStore is IErrorCatalogContextPublisher publisher)
        {
            return publisher.Publish(context);
        }

        _contextStore.Set(context);
        return null;
    }

    private void RecordStatus(
        ErrorCatalogInitializationPayload payload,
        Response<ErrorCatalogInitializationPayload>?
            recoveryReason = null)
    {
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentNullException.ThrowIfNull(payload.Bootstrap);

        string? recoveryReasonCode =
            recoveryReason is null
                ? null
                : recoveryReason.Issues is { Count: > 0 }
                    ? recoveryReason.Issues[0].Code
                    : "WIF_INITIALIZATION_FAILED";

        string? recoveryMessage =
            recoveryReason is null
                ? null
                : string.IsNullOrWhiteSpace(
                    recoveryReason.Message)
                        ? recoveryReason.Issues is { Count: > 0 }
                            ? recoveryReason.Issues[0].Message
                            : "The requested error catalog "
                              + "initialization failed."
                        : recoveryReason.Message;

        ErrorCatalogRuntimeStatus status = new()
        {
            ContextSource =
           payload.ContextSource,

            IsDegraded =
           payload.IsDegraded,

            KeptPreviousContext =
           payload.KeptPreviousContext,

            UsedFallback =
           payload.UsedFallback,

            RecoveryReasonCode =
           recoveryReasonCode,

            RecoveryStatus =
           recoveryReason?.Status,

            RecoveryMessage =
           recoveryMessage,

            ActivatedAtUtc =
           DateTimeOffset.UtcNow,

            PackageDirectoryPath =
           payload.Bootstrap.PackageDirectoryPath
        };

        if (!status.IsConsistent)
        {
            throw new InvalidOperationException(
                "The error catalog runtime attempted to record "
                + "an internally inconsistent status snapshot.");
        }

        // The legacy status remains available independently. The optional
        // completed observation is published only after a matching context
        // publication has been selected, never by inferring an ID from time.
        Volatile.Write(ref _completedActivation, null);
        Volatile.Write(ref _currentStatus, status);

        if (_contextStore is not IErrorCatalogContextPublicationReader reader)
        {
            return;
        }

        try
        {
            // OwnedPublication identifies an exact write by this operation.
            // SelectedPublication identifies the exact existing record
            // chosen for no-write recovery. Neither may be replaced by
            // a later read that could belong to another writer.
            ErrorCatalogContextPublication? publication =
                payload.OwnedPublication ?? payload.SelectedPublication;

            if (publication is not null)
            {
                if (!ReferenceEquals(publication.Context, payload.Context))
                {
                    return;
                }
            }
            else
            {
                // Legacy/custom initializer or recovery when the optional
                // reader was unavailable: this remains a best-effort
                // association, NOT proof of write or selection ownership.
                Response<ErrorCatalogContextPublication>? response =
                    reader.GetCurrentPublication();

                if (response?.IsSuccess != true
                    || response.Data is not { } current
                    || !ReferenceEquals(current.Context, payload.Context))
                {
                    return;
                }

                publication = current;
            }

            long sequence = Interlocked.Increment(ref _activationSequence);
            Volatile.Write(
                ref _completedActivation,
                new CompletedActivation(sequence, publication, status));
        }
        catch (Exception)
        {
            // Optional observation must not change the existing success or
            // failure contract of initialization, reset, or recovery.
            // GetCompletedActivation returns a non-success result instead.
        }
    }

}
