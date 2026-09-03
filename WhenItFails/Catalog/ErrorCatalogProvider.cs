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

        Response<ErrorCatalogDocument>? loadResponse =
            await _loader.LoadFromFileAsync(filePath, cancellationToken);

        if (loadResponse is null)
        {
            return Response<ErrorCatalogProviderPayload>.Invalid(
                code: "WIF_ERROR_CATALOG_LOADER_RESPONSE_NULL",
                message: "The error catalog loader returned a null response.");
        }

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

        ErrorCatalogDocument? normalizedDocument =
            _normalizer.Normalize(loadResponse.Data);

        if (normalizedDocument is null)
        {
            return Response<ErrorCatalogProviderPayload>.Invalid(
                code: "WIF_ERROR_CATALOG_NORMALIZER_RESULT_NULL",
                message:
                    "The error catalog document normalizer returned a null result.");
        }

        ErrorCatalogValidationResult? validationResult =
            _validator.Validate(normalizedDocument);

        if (validationResult is null)
        {
            return Response<ErrorCatalogProviderPayload>.Invalid(
                code: "WIF_ERROR_CATALOG_VALIDATOR_RESULT_NULL",
                message: "The error catalog validator returned a null result.");
        }

        if (!validationResult.IsValid)
        {
            return Response<ErrorCatalogProviderPayload>.Invalid(
                code: "CatalogValidationFailed",
                message: "Error catalog validation failed.");
        }

        IErrorCatalog? catalog = _factory.Create(normalizedDocument);

        if (catalog is null)
        {
            return Response<ErrorCatalogProviderPayload>.Invalid(
                code: "WIF_ERROR_CATALOG_FACTORY_RESULT_NULL",
                message: "The error catalog factory returned a null result.");
        }

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
        return response.Issues?
            .FirstOrDefault(issue => issue is not null)?
            .Code
            ?? fallbackCode;
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
