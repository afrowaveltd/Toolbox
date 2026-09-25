using Afrowave.Toolbox.Essentials.Extensions;
using Afrowave.Toolbox.Essentials.Issues;
using Afrowave.Toolbox.Essentials.Metadata;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Runtime;

/// <summary>
/// Adds publication identity to a detached capture of all supporting catalogs.
/// </summary>
public static class ErrorCatalogPublishedSupportingCatalogsSnapshotExtensions
{
    /// <summary>
    /// Captures four supporting catalogs from one actual store publication
    /// without independently reading context or runtime status.
    /// </summary>
    /// <remarks>
    /// Requires the optional IErrorCatalogRuntimePublicationReader capability.
    /// Generation is store-scoped, not a synchronized activation/status ID.
    /// This copy is not transactional against external in-place mutation of
    /// already published catalog documents.
    /// </remarks>
    public static Response<ErrorCatalogPublishedSupportingCatalogsSnapshot>
        GetPublishedSupportingCatalogsSnapshot(this IErrorCatalogRuntime runtime)
    {
        ArgumentNullException.ThrowIfNull(runtime);

        if (runtime is not IErrorCatalogRuntimePublicationReader reader)
        {
            return Response<ErrorCatalogPublishedSupportingCatalogsSnapshot>.NotSupported(
                data: null,
                code: "WIF_PUBLISHED_SUPPORTING_SNAPSHOT_NOT_SUPPORTED",
                message: "The runtime does not support context publication identity.");
        }

        Response<ErrorCatalogContextPublication>? publicationResponse;

        try
        {
            publicationResponse = reader.GetCurrentPublication();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return CaptureFailed();
        }

        if (publicationResponse is null)
        {
            return Response<ErrorCatalogPublishedSupportingCatalogsSnapshot>.Invalid(
                code: "WIF_PUBLISHED_SUPPORTING_SNAPSHOT_PUBLICATION_RESPONSE_NULL",
                message: "The runtime returned a null publication response.");
        }

        if (!publicationResponse.IsSuccess)
        {
            return new Response<ErrorCatalogPublishedSupportingCatalogsSnapshot>
            {
                Status = publicationResponse.Status,
                Message = publicationResponse.Message,
                Issues = publicationResponse.Issues ?? Array.Empty<IssueInfo>(),
                Metadata = publicationResponse.Metadata?.Copy() ?? new MetadataBag()
            };
        }

        ErrorCatalogContextPublication? publication = publicationResponse.Data;

        if (publication is null)
        {
            return Response<ErrorCatalogPublishedSupportingCatalogsSnapshot>.Invalid(
                code: "WIF_PUBLISHED_SUPPORTING_SNAPSHOT_PUBLICATION_NULL",
                message: "The runtime returned success without a context publication.");
        }

        try
        {
            Response<ErrorSupportingCatalogsSnapshot> captured =
                ErrorSupportingCatalogsSnapshotExtensions.CaptureFromContext(
                    publication.Context);

            if (!captured.IsSuccess || captured.Data is null)
            {
                return new Response<ErrorCatalogPublishedSupportingCatalogsSnapshot>
                {
                    Status = captured.Status,
                    Message = captured.Message,
                    Issues = captured.Issues ?? Array.Empty<IssueInfo>(),
                    Metadata = captured.Metadata?.Copy() ?? new MetadataBag()
                };
            }

            ErrorCatalogPublishedSupportingCatalogsSnapshot snapshot = new(
                publication.StoreId,
                publication.Generation,
                captured.Data);

            return new Response<ErrorCatalogPublishedSupportingCatalogsSnapshot>
            {
                Status = publicationResponse.Status,
                Message = publicationResponse.Message,
                Data = snapshot,
                Issues = publicationResponse.Issues ?? Array.Empty<IssueInfo>(),
                Metadata = publicationResponse.Metadata?.Copy() ?? new MetadataBag()
            };
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return CaptureFailed();
        }
    }

    private static Response<ErrorCatalogPublishedSupportingCatalogsSnapshot>
        CaptureFailed() =>
        Response<ErrorCatalogPublishedSupportingCatalogsSnapshot>.Fail(
            code: "WIF_PUBLISHED_SUPPORTING_SNAPSHOT_FAILED",
            message: "The published supporting catalogs could not be captured.");
}
