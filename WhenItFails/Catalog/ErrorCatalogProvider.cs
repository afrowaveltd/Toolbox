using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Catalog;

/// <summary>
/// Default implementation that loads, normalizes, validates and creates runtime error catalogs.
/// </summary>
public sealed class ErrorCatalogProvider : IErrorCatalogProvider
{
    private readonly IErrorCatalogLoader _loader;
    private readonly IErrorCatalogDocumentNormalizer _normalizer;
    private readonly IErrorCatalogValidator _validator;
    private readonly IErrorCatalogFactory _factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorCatalogProvider"/> class.
    /// </summary>
    public ErrorCatalogProvider(
        IErrorCatalogLoader loader,
        IErrorCatalogDocumentNormalizer normalizer,
        IErrorCatalogValidator validator,
        IErrorCatalogFactory factory)
    {
        _loader = loader ?? throw new ArgumentNullException(nameof(loader));
        _normalizer = normalizer ?? throw new ArgumentNullException(nameof(normalizer));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    /// <inheritdoc />
    public async Task<Response<ErrorCatalogProviderPayload>> LoadFromFileAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Response<ErrorCatalogDocument> loadResponse =
            await _loader.LoadFromFileAsync(filePath, cancellationToken);

        if (!loadResponse.IsSuccess)
        {
            return Response<ErrorCatalogProviderPayload>.WithStatus(
                Response<ErrorCatalogProviderPayload>.Fail(
                    code: GetFirstIssueCode(loadResponse, "CatalogLoadFailed"),
                    message: GetResponseMessage(loadResponse, "Error catalog loading failed.")),
                loadResponse.Status);
        }

        if (loadResponse.Data is null)
        {
            return Response<ErrorCatalogProviderPayload>.Invalid(
                code: "LoadedCatalogDocumentIsNull",
                message: "Error catalog loader returned success, but document is null.");
        }

        ErrorCatalogDocument normalizedDocument =
            _normalizer.Normalize(loadResponse.Data);

        ErrorCatalogValidationResult validationResult =
            _validator.Validate(normalizedDocument);

        if (!validationResult.IsValid)
        {
            return Response<ErrorCatalogProviderPayload>.Invalid(
                code: "CatalogValidationFailed",
                message: "Error catalog validation failed.");
        }

        IErrorCatalog catalog = _factory.Create(normalizedDocument);

        ErrorCatalogProviderPayload payload = new()
        {
            Catalog = catalog,
            Document = normalizedDocument,
            ValidationResult = validationResult
        };

        return Response<ErrorCatalogProviderPayload>.Ok(payload);
    }

    private static string GetFirstIssueCode(
        Response<ErrorCatalogDocument> response,
        string fallbackCode)
    {
        return response.Issues.Count > 0
            ? response.Issues[0].Code
            : fallbackCode;
    }

    private static string GetResponseMessage(
        Response<ErrorCatalogDocument> response,
        string fallbackMessage)
    {
        return string.IsNullOrWhiteSpace(response.Message)
            ? fallbackMessage
            : response.Message;
    }
}