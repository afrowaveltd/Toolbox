using Afrowave.Toolbox.Essentials.Extensions;
using Afrowave.Toolbox.Essentials.Issues;
using Afrowave.Toolbox.Essentials.Metadata;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Runtime;

/// <summary>
/// Captures related read-only projections from one selected active context reference.
/// </summary>
public static class ErrorCatalogCombinedSnapshotExtensions
{
    /// <summary>
    /// Captures main definitions, the category catalog and recorded cross-validation
    /// findings from a single active-context read.
    /// </summary>
    /// <remarks>
    /// The three projections are independently detached from the same selected
    /// context reference. This does not guarantee transactional capture if
    /// another caller mutates the already published context in place.
    /// No activation-generation ID or independent runtime status is included.
    /// </remarks>
    public static Response<ErrorCatalogCombinedSnapshot>
        GetCombinedSnapshot(this IErrorCatalogRuntime runtime)
    {
        ArgumentNullException.ThrowIfNull(runtime);

        Response<ErrorCatalogContext>? response;

        try
        {
            response = runtime.GetCurrentContext();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return CaptureFailed();
        }

        if (response is null)
        {
            return Response<ErrorCatalogCombinedSnapshot>.Invalid(
                code: "WIF_COMBINED_SNAPSHOT_CONTEXT_RESPONSE_NULL",
                message: "The runtime returned a null context response.");
        }

        if (!response.IsSuccess)
        {
            return new Response<ErrorCatalogCombinedSnapshot>
            {
                Status = response.Status,
                Message = response.Message,
                Issues = response.Issues ?? Array.Empty<IssueInfo>(),
                Metadata = response.Metadata?.Copy() ?? new MetadataBag()
            };
        }

        ErrorCatalogContext? context = response.Data;

        if (context?.ErrorCatalog is null)
        {
            return Response<ErrorCatalogCombinedSnapshot>.Invalid(
                code: "WIF_COMBINED_SNAPSHOT_ERROR_CATALOG_NULL",
                message: "The active context does not contain an error catalog.");
        }

        if (context.CategoryCatalog is null)
        {
            return Response<ErrorCatalogCombinedSnapshot>.Invalid(
                code: "WIF_COMBINED_SNAPSHOT_CATEGORY_CATALOG_NULL",
                message: "The active context does not contain a category catalog.");
        }

        if (context.CrossValidationResult is null)
        {
            return Response<ErrorCatalogCombinedSnapshot>.Invalid(
                code: "WIF_COMBINED_SNAPSHOT_VALIDATION_RESULT_NULL",
                message: "The active context does not contain a cross-validation result.");
        }

        try
        {
            IReadOnlyList<ErrorDefinition>? definitions =
                context.ErrorCatalog.GetAll();

            if (definitions is null)
            {
                return Response<ErrorCatalogCombinedSnapshot>.Invalid(
                    code: "WIF_COMBINED_SNAPSHOT_DEFINITIONS_NULL",
                    message: "The active error catalog returned a null definition list.");
            }

            IReadOnlyList<ErrorDefinitionSnapshot> detachedDefinitions =
                Array.AsReadOnly(
                    definitions.Select(definition =>
                        new ErrorDefinitionSnapshot(definition)).ToArray());

            ErrorCatalogCombinedSnapshot snapshot = new(
                detachedDefinitions,
                new ErrorCategoryCatalogSnapshot(context.CategoryCatalog),
                new ErrorCatalogValidationSnapshot(context.CrossValidationResult));

            return new Response<ErrorCatalogCombinedSnapshot>
            {
                Status = response.Status,
                Message = response.Message,
                Data = snapshot,
                Issues = response.Issues ?? Array.Empty<IssueInfo>(),
                Metadata = response.Metadata?.Copy() ?? new MetadataBag()
            };
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return CaptureFailed();
        }
    }

    private static Response<ErrorCatalogCombinedSnapshot> CaptureFailed() =>
        Response<ErrorCatalogCombinedSnapshot>.Fail(
            code: "WIF_COMBINED_SNAPSHOT_FAILED",
            message: "The active catalog data could not be captured.");
}
