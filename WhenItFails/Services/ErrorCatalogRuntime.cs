using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Descriptors;
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
    /// 
    public Task<Response<ErrorCatalogInitializationPayload>> InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        JsonsOptions jsonsOptions =
            _options.Jsons ?? new JsonsOptions();

        return _initializer.InitializeAsync(
            jsonsOptions,
            cancellationToken);
    }
    /// <inheritdoc />
    public Task<Response<ErrorCatalogInitializationPayload>> InitializeAsync(
        JsonsOptions options,
        CancellationToken cancellationToken = default)
    {
        return _initializer.InitializeAsync(
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
}