using Afrowave.Toolbox.Essentials.Extensions;
using Afrowave.Toolbox.Essentials.Issues;
using Afrowave.Toolbox.Essentials.Metadata;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Runtime;

/// <summary>
/// Captures all four supporting catalogs from one selected active context.
/// </summary>
public static class ErrorSupportingCatalogsSnapshotExtensions
{
    /// <summary>
    /// Returns detached category, owner, code group and profile catalog
    /// projections from one GetCurrentContext() call.
    /// </summary>
    /// <remarks>
    /// The context reference is selected once, but the copy is not transactional
    /// against concurrent in-place mutation of its nested documents. No runtime
    /// activation status or store publication generation is included.
    /// </remarks>
    public static Response<ErrorSupportingCatalogsSnapshot>
        GetSupportingCatalogsSnapshot(this IErrorCatalogRuntime runtime)
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
            return Response<ErrorSupportingCatalogsSnapshot>.Invalid(
                code: "WIF_SUPPORTING_SNAPSHOT_CONTEXT_RESPONSE_NULL",
                message: "The runtime returned a null context response.");
        }

        if (!response.IsSuccess)
        {
            return new Response<ErrorSupportingCatalogsSnapshot>
            {
                Status = response.Status,
                Message = response.Message,
                Issues = response.Issues ?? Array.Empty<IssueInfo>(),
                Metadata = response.Metadata?.Copy() ?? new MetadataBag()
            };
        }

        ErrorCatalogContext? context = response.Data;
        if (context?.CategoryCatalog is null)
        {
            return Missing("CATEGORY_CATALOG",
                "The active context does not contain a category catalog.");
        }

        if (context.OwnerCatalog is null)
        {
            return Missing("OWNER_CATALOG",
                "The active context does not contain an owner catalog.");
        }

        if (context.CodeGroupCatalog is null)
        {
            return Missing("CODE_GROUP_CATALOG",
                "The active context does not contain a code group catalog.");
        }

        if (context.ProfileCatalog is null)
        {
            return Missing("PROFILE_CATALOG",
                "The active context does not contain a profile catalog.");
        }

        // Preserve the selected document references. Never invoke another runtime
        // snapshot extension, which would select the active context again.
        ErrorCategoryCatalogDocument categories = context.CategoryCatalog;
        ErrorOwnerCatalogDocument owners = context.OwnerCatalog;
        ErrorCodeGroupCatalogDocument codeGroups = context.CodeGroupCatalog;
        ErrorProfileCatalogDocument profiles = context.ProfileCatalog;

        try
        {
            ErrorSupportingCatalogsSnapshot snapshot = new(
                new ErrorCategoryCatalogSnapshot(categories),
                new ErrorOwnerCatalogSnapshot(owners),
                new ErrorCodeGroupCatalogSnapshot(codeGroups),
                new ErrorProfileCatalogSnapshot(profiles));

            return new Response<ErrorSupportingCatalogsSnapshot>
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

    private static Response<ErrorSupportingCatalogsSnapshot> Missing(
        string suffix, string message) =>
        Response<ErrorSupportingCatalogsSnapshot>.Invalid(
            code: "WIF_SUPPORTING_SNAPSHOT_" + suffix + "_NULL",
            message: message);

    private static Response<ErrorSupportingCatalogsSnapshot> CaptureFailed() =>
        Response<ErrorSupportingCatalogsSnapshot>.Fail(
            code: "WIF_SUPPORTING_SNAPSHOT_FAILED",
            message: "The active supporting catalogs could not be captured.");
}
