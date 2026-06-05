using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Catalog;

/// <summary>
/// Default implementation that loads, normalizes and validates error profile catalog documents.
/// </summary>
public sealed class ErrorProfileCatalogProvider : IErrorProfileCatalogProvider
{
    private readonly IErrorProfileCatalogLoader _loader;
    private readonly ErrorProfileCatalogDocumentNormalizer _normalizer;
    private readonly IErrorProfileCatalogValidator _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorProfileCatalogProvider"/> class.
    /// </summary>
    public ErrorProfileCatalogProvider(
        IErrorProfileCatalogLoader loader,
        ErrorProfileCatalogDocumentNormalizer normalizer,
        IErrorProfileCatalogValidator validator)
    {
        _loader = loader ?? throw new ArgumentNullException(nameof(loader));
        _normalizer = normalizer ?? throw new ArgumentNullException(nameof(normalizer));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    /// <inheritdoc />
    public async Task<Response<ErrorProfileCatalogProviderPayload>> LoadFromFileAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Response<ErrorProfileCatalogDocument> loadResponse =
            await _loader.LoadFromFileAsync(filePath, cancellationToken);

        if (!loadResponse.IsSuccess)
        {
            return Response<ErrorProfileCatalogProviderPayload>.WithStatus(
                Response<ErrorProfileCatalogProviderPayload>.Fail(
                    code: GetFirstIssueCode(loadResponse, "ProfileCatalogLoadFailed"),
                    message: GetResponseMessage(loadResponse, "Error profile catalog loading failed.")),
                loadResponse.Status);
        }

        if (loadResponse.Data is null)
        {
            return Response<ErrorProfileCatalogProviderPayload>.Invalid(
                code: "LoadedProfileCatalogDocumentIsNull",
                message: "Error profile catalog loader returned success, but document is null.");
        }

        ErrorProfileCatalogDocument normalizedDocument =
            _normalizer.Normalize(loadResponse.Data);

        ErrorCatalogValidationResult validationResult =
            _validator.Validate(normalizedDocument);

        if (!validationResult.IsValid)
        {
            return Response<ErrorProfileCatalogProviderPayload>.Invalid(
                code: "ProfileCatalogValidationFailed",
                message: "Error profile catalog validation failed.");
        }

        ErrorProfileCatalogProviderPayload payload = new()
        {
            Document = normalizedDocument,
            ValidationResult = validationResult
        };

        return Response<ErrorProfileCatalogProviderPayload>.Ok(payload);
    }

    private static string GetFirstIssueCode(
        Response<ErrorProfileCatalogDocument> response,
        string fallbackCode)
    {
        return response.Issues.Count > 0
            ? response.Issues[0].Code
            : fallbackCode;
    }

    private static string GetResponseMessage(
        Response<ErrorProfileCatalogDocument> response,
        string fallbackMessage)
    {
        return string.IsNullOrWhiteSpace(response.Message)
            ? fallbackMessage
            : response.Message;
    }
}