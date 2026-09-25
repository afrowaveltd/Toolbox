using Afrowave.Toolbox.Essentials.Extensions;
using Afrowave.Toolbox.Essentials.Issues;
using Afrowave.Toolbox.Essentials.Metadata;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Runtime;

/// <summary>
/// Captures detached combined data from one context publication record.
/// </summary>
public static class ErrorCatalogPublishedCombinedSnapshotExtensions
{
    /// <summary>
    /// Captures detached definitions, categories and recorded validation
    /// together with their store-scoped publication identity.
    /// </summary>
    /// <remarks>
    /// Requires the optional IErrorCatalogRuntimePublicationReader capability.
    /// Never guesses an identity for runtimes or stores without that capability.
    /// A generation identifies a store publication, not an atomic runtime
    /// status update or a transaction against mutation of the live context.
    /// </remarks>
    public static Response<ErrorCatalogPublishedCombinedSnapshot>
        GetPublishedCombinedSnapshot(this IErrorCatalogRuntime runtime)
    {
        ArgumentNullException.ThrowIfNull(runtime);

        if (runtime is not IErrorCatalogRuntimePublicationReader reader)
        {
            return Response<ErrorCatalogPublishedCombinedSnapshot>.NotSupported(
                code: "WIF_PUBLISHED_SNAPSHOT_NOT_SUPPORTED",
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
            return Response<ErrorCatalogPublishedCombinedSnapshot>.Invalid(
                code: "WIF_PUBLISHED_SNAPSHOT_PUBLICATION_RESPONSE_NULL",
                message: "The runtime returned a null publication response.");
        }

        if (!publicationResponse.IsSuccess)
        {
            return new Response<ErrorCatalogPublishedCombinedSnapshot>
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
            return Response<ErrorCatalogPublishedCombinedSnapshot>.Invalid(
                code: "WIF_PUBLISHED_SNAPSHOT_PUBLICATION_NULL",
                message: "The runtime returned success without a context publication.");
        }

        try
        {
            Response<ErrorCatalogCombinedSnapshot> captured =
                ErrorCatalogCombinedSnapshotExtensions.CaptureFromContext(
                    publication.Context);

            if (!captured.IsSuccess || captured.Data is null)
            {
                return new Response<ErrorCatalogPublishedCombinedSnapshot>
                {
                    Status = captured.Status,
                    Message = captured.Message,
                    Issues = captured.Issues ?? Array.Empty<IssueInfo>(),
                    Metadata = captured.Metadata?.Copy() ?? new MetadataBag()
                };
            }

            ErrorCatalogPublishedCombinedSnapshot snapshot = new(
                publication.StoreId,
                publication.Generation,
                captured.Data);

            return new Response<ErrorCatalogPublishedCombinedSnapshot>
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

    private static Response<ErrorCatalogPublishedCombinedSnapshot>
        CaptureFailed() =>
        Response<ErrorCatalogPublishedCombinedSnapshot>.Fail(
            code: "WIF_PUBLISHED_SNAPSHOT_FAILED",
            message: "The active context publication could not be captured.");
}
