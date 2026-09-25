using Afrowave.Toolbox.Essentials.Extensions;
using Afrowave.Toolbox.Essentials.Issues;
using Afrowave.Toolbox.Essentials.Metadata;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Runtime;

/// <summary>
/// Adds detached, read-only definition projections without changing
/// the existing IErrorCatalogRuntime interface.
/// </summary>
public static class ErrorCatalogSnapshotExtensions
{
    /// <summary>
    /// Captures the active indexed error definitions as a detached, read-only list.
    /// </summary>
    /// <remarks>
    /// This method copies only main error definitions, including their lists and metadata.
    /// It does not snapshot supporting catalogs, cross-validation results or runtime status.
    /// Callers must not mutate the live context while a capture is in progress:
    /// atomic publication of the context reference is not a deep read lock.
    /// </remarks>
    public static Response<IReadOnlyList<ErrorDefinitionSnapshot>>
        GetErrorDefinitionSnapshots(this IErrorCatalogRuntime runtime)
    {
        ArgumentNullException.ThrowIfNull(runtime);

        Response<ErrorCatalogContext>? contextResponse;

        try
        {
            contextResponse = runtime.GetCurrentContext();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return Response<IReadOnlyList<ErrorDefinitionSnapshot>>.Fail(
                code: "WIF_ERROR_DEFINITION_SNAPSHOT_FAILED",
                message: "The active error definitions could not be captured.");
        }

        if (contextResponse is null)
        {
            return Response<IReadOnlyList<ErrorDefinitionSnapshot>>.Invalid(
                code: "WIF_ERROR_DEFINITION_SNAPSHOT_CONTEXT_RESPONSE_NULL",
                message: "The runtime returned a null context response.");
        }

        if (!contextResponse.IsSuccess)
        {
            return ForwardContextFailure(contextResponse);
        }

        if (contextResponse.Data?.ErrorCatalog is null)
        {
            return Response<IReadOnlyList<ErrorDefinitionSnapshot>>.Invalid(
                code: "WIF_ERROR_DEFINITION_SNAPSHOT_CATALOG_NULL",
                message: "The active context does not contain an error catalog.");
        }

        try
        {
            IReadOnlyList<ErrorDefinition>? definitions =
                contextResponse.Data.ErrorCatalog.GetAll();

            if (definitions is null)
            {
                return Response<IReadOnlyList<ErrorDefinitionSnapshot>>.Invalid(
                    code: "WIF_ERROR_DEFINITION_SNAPSHOT_DEFINITIONS_NULL",
                    message: "The active catalog returned a null definition list.");
            }

            IReadOnlyList<ErrorDefinitionSnapshot> snapshots =
                Array.AsReadOnly(
                    definitions.Select(
                        definition => new ErrorDefinitionSnapshot(definition))
                        .ToArray());

            return new Response<IReadOnlyList<ErrorDefinitionSnapshot>>
            {
                Status = contextResponse.Status,
                Message = contextResponse.Message,
                Data = snapshots,
                Issues = contextResponse.Issues ?? Array.Empty<IssueInfo>(),
                Metadata = contextResponse.Metadata?.Copy() ?? new MetadataBag()
            };
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return Response<IReadOnlyList<ErrorDefinitionSnapshot>>.Fail(
                code: "WIF_ERROR_DEFINITION_SNAPSHOT_FAILED",
                message: "The active error definitions could not be captured.");
        }
    }

    private static Response<IReadOnlyList<ErrorDefinitionSnapshot>>
        ForwardContextFailure(Response<ErrorCatalogContext> source)
    {
        return new Response<IReadOnlyList<ErrorDefinitionSnapshot>>
        {
            Status = source.Status,
            Message = source.Message,
            Issues = source.Issues ?? Array.Empty<IssueInfo>(),
            Metadata = source.Metadata?.Copy() ?? new MetadataBag()
        };
    }
}
