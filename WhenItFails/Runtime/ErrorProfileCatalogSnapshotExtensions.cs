using Afrowave.Toolbox.Essentials.Extensions;
using Afrowave.Toolbox.Essentials.Issues;
using Afrowave.Toolbox.Essentials.Metadata;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Runtime;

/// <summary>Provides a detached profile catalog view without modifying IErrorCatalogRuntime.</summary>
public static class ErrorProfileCatalogSnapshotExtensions
{
    /// <summary>Captures the active profile catalog into a detached read-only projection.</summary>
    /// <remarks>
    /// Selects one active context. This does not provide a transaction
    /// against concurrent in-place mutation of its nested documents.
    /// </remarks>
    public static Response<ErrorProfileCatalogSnapshot>
        GetProfileCatalogSnapshot(this IErrorCatalogRuntime runtime)
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
            return Response<ErrorProfileCatalogSnapshot>.Invalid(
                code: "WIF_PROFILE_SNAPSHOT_CONTEXT_RESPONSE_NULL",
                message: "The runtime returned a null context response.");
        }

        if (!response.IsSuccess)
        {
            return new Response<ErrorProfileCatalogSnapshot>
            {
                Status = response.Status,
                Message = response.Message,
                Issues = response.Issues ?? Array.Empty<IssueInfo>(),
                Metadata = response.Metadata?.Copy() ?? new MetadataBag()
            };
        }

        if (response.Data?.ProfileCatalog is null)
        {
            return Response<ErrorProfileCatalogSnapshot>.Invalid(
                code: "WIF_PROFILE_SNAPSHOT_CATALOG_NULL",
                message: "The active context does not contain a profile catalog.");
        }

        try
        {
            ErrorProfileCatalogSnapshot snapshot =
                new(response.Data.ProfileCatalog);

            return new Response<ErrorProfileCatalogSnapshot>
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

    private static Response<ErrorProfileCatalogSnapshot> CaptureFailed() =>
        Response<ErrorProfileCatalogSnapshot>.Fail(
            code: "WIF_PROFILE_SNAPSHOT_FAILED",
            message: "The active profile catalog could not be captured.");
}
