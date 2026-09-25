using Afrowave.Toolbox.Essentials.Extensions;
using Afrowave.Toolbox.Essentials.Issues;
using Afrowave.Toolbox.Essentials.Metadata;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Runtime;

/// <summary>
/// Adds a detached cross-validation projection without changing the runtime interface.
/// </summary>
public static class ErrorCatalogValidationSnapshotExtensions
{
    /// <summary>
    /// Captures the currently active context's cross-validation issues.
    /// </summary>
    /// <remarks>
    /// The result represents the historical validation findings recorded in
    /// the active context. It does not revalidate mutable catalog documents.
    /// Callers must not mutate the live context during capture.
    /// </remarks>
    public static Response<ErrorCatalogValidationSnapshot>
        GetCrossValidationSnapshot(this IErrorCatalogRuntime runtime)
    {
        ArgumentNullException.ThrowIfNull(runtime);

        Response<ErrorCatalogContext>? contextResponse;

        try
        {
            contextResponse = runtime.GetCurrentContext();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return Response<ErrorCatalogValidationSnapshot>.Fail(
                code: "WIF_VALIDATION_SNAPSHOT_FAILED",
                message: "The active catalog validation result could not be captured.");
        }

        if (contextResponse is null)
        {
            return Response<ErrorCatalogValidationSnapshot>.Invalid(
                code: "WIF_VALIDATION_SNAPSHOT_CONTEXT_RESPONSE_NULL",
                message: "The runtime returned a null context response.");
        }

        if (!contextResponse.IsSuccess)
        {
            return ForwardContextFailure(contextResponse);
        }

        if (contextResponse.Data?.CrossValidationResult is null)
        {
            return Response<ErrorCatalogValidationSnapshot>.Invalid(
                code: "WIF_VALIDATION_SNAPSHOT_RESULT_NULL",
                message: "The active context does not contain a cross-validation result.");
        }

        try
        {
            ErrorCatalogValidationSnapshot snapshot =
                new(contextResponse.Data.CrossValidationResult);

            return new Response<ErrorCatalogValidationSnapshot>
            {
                Status = contextResponse.Status,
                Message = contextResponse.Message,
                Data = snapshot,
                Issues = contextResponse.Issues ?? Array.Empty<IssueInfo>(),
                Metadata = contextResponse.Metadata?.Copy() ?? new MetadataBag()
            };
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return Response<ErrorCatalogValidationSnapshot>.Fail(
                code: "WIF_VALIDATION_SNAPSHOT_FAILED",
                message: "The active catalog validation result could not be captured.");
        }
    }

    private static Response<ErrorCatalogValidationSnapshot>
        ForwardContextFailure(Response<ErrorCatalogContext> source)
    {
        return new Response<ErrorCatalogValidationSnapshot>
        {
            Status = source.Status,
            Message = source.Message,
            Issues = source.Issues ?? Array.Empty<IssueInfo>(),
            Metadata = source.Metadata?.Copy() ?? new MetadataBag()
        };
    }
}
