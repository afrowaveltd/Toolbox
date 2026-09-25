using Afrowave.Toolbox.Essentials.Extensions;
using Afrowave.Toolbox.Essentials.Issues;
using Afrowave.Toolbox.Essentials.Metadata;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Runtime;

/// <summary>
/// Adds a detached owner catalog view without modifying IErrorCatalogRuntime.
/// </summary>
public static class ErrorOwnerCatalogSnapshotExtensions
{
    /// <summary>Captures the active owner catalog as detached read-only data.</summary>
    /// <remarks>
    /// Gets the active context once. It does not provide a transaction
    /// against concurrent in-place mutations of the selected context.
    /// </remarks>
    public static Response<ErrorOwnerCatalogSnapshot>
        GetOwnerCatalogSnapshot(this IErrorCatalogRuntime runtime)
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
            return Response<ErrorOwnerCatalogSnapshot>.Invalid(
                code: "WIF_OWNER_SNAPSHOT_CONTEXT_RESPONSE_NULL",
                message: "The runtime returned a null context response.");
        }

        if (!response.IsSuccess)
        {
            return new Response<ErrorOwnerCatalogSnapshot>
            {
                Status = response.Status,
                Message = response.Message,
                Issues = response.Issues ?? Array.Empty<IssueInfo>(),
                Metadata = response.Metadata?.Copy() ?? new MetadataBag()
            };
        }

        if (response.Data?.OwnerCatalog is null)
        {
            return Response<ErrorOwnerCatalogSnapshot>.Invalid(
                code: "WIF_OWNER_SNAPSHOT_CATALOG_NULL",
                message: "The active context does not contain an owner catalog.");
        }

        try
        {
            ErrorOwnerCatalogSnapshot snapshot =
                new(response.Data.OwnerCatalog);

            return new Response<ErrorOwnerCatalogSnapshot>
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

    private static Response<ErrorOwnerCatalogSnapshot> CaptureFailed() =>
        Response<ErrorOwnerCatalogSnapshot>.Fail(
            code: "WIF_OWNER_SNAPSHOT_FAILED",
            message: "The active owner catalog could not be captured.");
}
