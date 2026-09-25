using Afrowave.Toolbox.Essentials.Extensions;
using Afrowave.Toolbox.Essentials.Issues;
using Afrowave.Toolbox.Essentials.Metadata;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Runtime;

/// <summary>Provides a detached supporting code-group view without changing the runtime interface.</summary>
public static class ErrorCodeGroupCatalogSnapshotExtensions
{
    /// <summary>Captures the active code-group catalog as independently copied, read-only data.</summary>
    /// <remarks>
    /// This call selects one active context. It is not transactional against
    /// concurrent in-place mutation of that context's nested data.
    /// </remarks>
    public static Response<ErrorCodeGroupCatalogSnapshot>
        GetCodeGroupCatalogSnapshot(this IErrorCatalogRuntime runtime)
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
            return Response<ErrorCodeGroupCatalogSnapshot>.Invalid(
                code: "WIF_CODE_GROUP_SNAPSHOT_CONTEXT_RESPONSE_NULL",
                message: "The runtime returned a null context response.");
        }

        if (!response.IsSuccess)
        {
            return new Response<ErrorCodeGroupCatalogSnapshot>
            {
                Status = response.Status,
                Message = response.Message,
                Issues = response.Issues ?? Array.Empty<IssueInfo>(),
                Metadata = response.Metadata?.Copy() ?? new MetadataBag()
            };
        }

        if (response.Data?.CodeGroupCatalog is null)
        {
            return Response<ErrorCodeGroupCatalogSnapshot>.Invalid(
                code: "WIF_CODE_GROUP_SNAPSHOT_CATALOG_NULL",
                message: "The active context does not contain a code group catalog.");
        }

        try
        {
            ErrorCodeGroupCatalogSnapshot snapshot =
                new(response.Data.CodeGroupCatalog);

            return new Response<ErrorCodeGroupCatalogSnapshot>
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

    private static Response<ErrorCodeGroupCatalogSnapshot> CaptureFailed() =>
        Response<ErrorCodeGroupCatalogSnapshot>.Fail(
            code: "WIF_CODE_GROUP_SNAPSHOT_FAILED",
            message: "The active code group catalog could not be captured.");
}
