using Afrowave.Toolbox.Essentials.Extensions;
using Afrowave.Toolbox.Essentials.Issues;
using Afrowave.Toolbox.Essentials.Metadata;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Runtime;

/// <summary>
/// Provides detached supporting-category projections without changing the runtime interface.
/// </summary>
public static class ErrorCategoryCatalogSnapshotExtensions
{
    /// <summary>
    /// Captures the active supporting category catalog into a read-only projection.
    /// </summary>
    /// <remarks>
    /// Reads the current context once. The projection does not share mutable
    /// category documents, definitions, collections or metadata with that context.
    /// Concurrent in-place mutation of the source is not a supported transaction.
    /// </remarks>
    public static Response<ErrorCategoryCatalogSnapshot>
        GetCategoryCatalogSnapshot(this IErrorCatalogRuntime runtime)
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
            return Response<ErrorCategoryCatalogSnapshot>.Invalid(
                code: "WIF_CATEGORY_SNAPSHOT_CONTEXT_RESPONSE_NULL",
                message: "The runtime returned a null context response.");
        }

        if (!response.IsSuccess)
        {
            return new Response<ErrorCategoryCatalogSnapshot>
            {
                Status = response.Status,
                Message = response.Message,
                Issues = response.Issues ?? Array.Empty<IssueInfo>(),
                Metadata = response.Metadata?.Copy() ?? new MetadataBag()
            };
        }

        if (response.Data?.CategoryCatalog is null)
        {
            return Response<ErrorCategoryCatalogSnapshot>.Invalid(
                code: "WIF_CATEGORY_SNAPSHOT_CATALOG_NULL",
                message: "The active context does not contain a category catalog.");
        }

        try
        {
            ErrorCategoryCatalogSnapshot snapshot =
                new(response.Data.CategoryCatalog);

            return new Response<ErrorCategoryCatalogSnapshot>
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

    private static Response<ErrorCategoryCatalogSnapshot> CaptureFailed() =>
        Response<ErrorCategoryCatalogSnapshot>.Fail(
            code: "WIF_CATEGORY_SNAPSHOT_FAILED",
            message: "The active category catalog could not be captured.");
}
