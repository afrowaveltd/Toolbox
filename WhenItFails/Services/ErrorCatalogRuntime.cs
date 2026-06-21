using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Descriptors;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Services;

/// <summary>
/// Default high-level facade over the initialized WhenItFails runtime.
/// </summary>
/// <remarks>
/// Initializes a new instance of the
/// <see cref="ErrorCatalogRuntime"/> class.
/// </remarks>
public sealed class ErrorCatalogRuntime(
    IErrorCatalogContextStore contextStore,
    IErrorDescriptorService descriptorService,
    IErrorProfileSelectionService profileSelectionService) : IErrorCatalogRuntime
{
    private readonly IErrorCatalogContextStore _contextStore = contextStore
            ?? throw new ArgumentNullException(nameof(contextStore));
    private readonly IErrorDescriptorService _descriptorService = descriptorService
            ?? throw new ArgumentNullException(nameof(descriptorService));
    private readonly IErrorProfileSelectionService _profileSelectionService = profileSelectionService
            ?? throw new ArgumentNullException(nameof(profileSelectionService));

    /// <inheritdoc />
    public Response<ErrorDescriptor> FromId(string errorId)
    {
        Response<ErrorCatalogContext> contextResponse =
            _contextStore.GetCurrent();

        if (!contextResponse.IsSuccess)
        {
            return ForwardFailure<ErrorDescriptor>(contextResponse);
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
            return ForwardFailure<ErrorDescriptor>(contextResponse);
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
            return ForwardFailure<ErrorDescriptor>(contextResponse);
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
            return ForwardFailure<IReadOnlyList<ErrorDefinition>>(
                contextResponse);
        }

        return _profileSelectionService.ResolveByProfileName(
            contextResponse.Data!,
            profileName);
    }

    private static Response<TTarget> ForwardFailure<TTarget>(
        Response<ErrorCatalogContext> sourceResponse)
    {
        string issueCode = sourceResponse.Issues.Count > 0
            ? sourceResponse.Issues[0].Code
            : "ErrorCatalogContextUnavailable";

        string message = string.IsNullOrWhiteSpace(sourceResponse.Message)
            ? "The initialized error catalog context is unavailable."
            : sourceResponse.Message;

        return Response<TTarget>.WithStatus(
            Response<TTarget>.Fail(
                code: issueCode,
                message: message),
            sourceResponse.Status);
    }
}