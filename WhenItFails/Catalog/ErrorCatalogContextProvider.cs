using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Catalog;

/// <summary>
/// Default implementation that loads all JSON catalogs and creates a combined catalog context.
/// </summary>
public sealed class ErrorCatalogContextProvider : IErrorCatalogContextProvider
{
    private readonly IErrorCatalogProvider _errorCatalogProvider;
    private readonly IErrorCategoryCatalogProvider _categoryCatalogProvider;
    private readonly IErrorCodeGroupCatalogProvider _codeGroupCatalogProvider;
    private readonly IErrorOwnerCatalogProvider _ownerCatalogProvider;
    private readonly IErrorProfileCatalogProvider _profileCatalogProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorCatalogContextProvider"/> class.
    /// </summary>
    public ErrorCatalogContextProvider(
        IErrorCatalogProvider errorCatalogProvider,
        IErrorCategoryCatalogProvider categoryCatalogProvider,
        IErrorCodeGroupCatalogProvider codeGroupCatalogProvider,
        IErrorOwnerCatalogProvider ownerCatalogProvider,
        IErrorProfileCatalogProvider profileCatalogProvider)
    {
        _errorCatalogProvider = errorCatalogProvider
            ?? throw new ArgumentNullException(nameof(errorCatalogProvider));
        _categoryCatalogProvider = categoryCatalogProvider
            ?? throw new ArgumentNullException(nameof(categoryCatalogProvider));
        _codeGroupCatalogProvider = codeGroupCatalogProvider
            ?? throw new ArgumentNullException(nameof(codeGroupCatalogProvider));
        _ownerCatalogProvider = ownerCatalogProvider
            ?? throw new ArgumentNullException(nameof(ownerCatalogProvider));
        _profileCatalogProvider = profileCatalogProvider
            ?? throw new ArgumentNullException(nameof(profileCatalogProvider));
    }

    /// <inheritdoc />
    public async Task<Response<ErrorCatalogContext>> LoadFromJsonsAsync(
        JsonsOptions options,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(options);

        Response<ErrorCatalogProviderPayload> errorCatalogResponse =
            await _errorCatalogProvider.LoadFromFileAsync(
                options.ErrorCatalogFilePath,
                cancellationToken);

        if (!errorCatalogResponse.IsSuccess)
        {
            return CreateFailedContextResponse(
                errorCatalogResponse,
                "ErrorCatalogContextErrorCatalogLoadFailed",
                "Error catalog loading failed while creating catalog context.");
        }

        Response<ErrorCategoryCatalogProviderPayload> categoryCatalogResponse =
            await _categoryCatalogProvider.LoadFromFileAsync(
                options.CategoryCatalogFilePath,
                cancellationToken);

        if (!categoryCatalogResponse.IsSuccess)
        {
            return CreateFailedContextResponse(
                categoryCatalogResponse,
                "ErrorCatalogContextCategoryCatalogLoadFailed",
                "Category catalog loading failed while creating catalog context.");
        }

        Response<ErrorCodeGroupCatalogProviderPayload> codeGroupCatalogResponse =
            await _codeGroupCatalogProvider.LoadFromFileAsync(
                options.CodeGroupCatalogFilePath,
                cancellationToken);

        if (!codeGroupCatalogResponse.IsSuccess)
        {
            return CreateFailedContextResponse(
                codeGroupCatalogResponse,
                "ErrorCatalogContextCodeGroupCatalogLoadFailed",
                "Code group catalog loading failed while creating catalog context.");
        }

        Response<ErrorOwnerCatalogProviderPayload> ownerCatalogResponse =
            await _ownerCatalogProvider.LoadFromFileAsync(
                options.OwnerCatalogFilePath,
                cancellationToken);

        if (!ownerCatalogResponse.IsSuccess)
        {
            return CreateFailedContextResponse(
                ownerCatalogResponse,
                "ErrorCatalogContextOwnerCatalogLoadFailed",
                "Owner catalog loading failed while creating catalog context.");
        }

        Response<ErrorProfileCatalogProviderPayload> profileCatalogResponse =
            await _profileCatalogProvider.LoadFromFileAsync(
                options.ProfilesFilePath,
                cancellationToken);

        if (!profileCatalogResponse.IsSuccess)
        {
            return CreateFailedContextResponse(
                profileCatalogResponse,
                "ErrorCatalogContextProfileCatalogLoadFailed",
                "Profile catalog loading failed while creating catalog context.");
        }

        if (errorCatalogResponse.Data is null
            || categoryCatalogResponse.Data is null
            || codeGroupCatalogResponse.Data is null
            || ownerCatalogResponse.Data is null
            || profileCatalogResponse.Data is null)
        {
            return Response<ErrorCatalogContext>.Invalid(
                code: "ErrorCatalogContextPayloadIsNull",
                message: "One or more catalog provider responses succeeded without payload data.");
        }

        ErrorCatalogCrossValidator crossValidator = new();

        ErrorCatalogValidationResult crossValidationResult = crossValidator.Validate(
    errorCatalogResponse.Data.Document,
    ownerCatalogResponse.Data.Document,
    codeGroupCatalogResponse.Data.Document,
    categoryCatalogResponse.Data.Document,
    profileCatalogResponse.Data.Document);

        if (!crossValidationResult.IsValid)
        {
            string issueCode = crossValidationResult.Issues.Count > 0
                ? crossValidationResult.Issues[0].Code
                : "ErrorCatalogContextCrossValidationFailed";

            string issueMessage = crossValidationResult.Issues.Count > 0
                ? crossValidationResult.Issues[0].Message
                : "Error catalog cross-validation failed while creating catalog context.";

            return Response<ErrorCatalogContext>.Invalid(
                code: issueCode,
                message: issueMessage);
        }

        ErrorCatalogContext context = new()
        {
            ErrorCatalog = errorCatalogResponse.Data.Catalog,
            ErrorCatalogDocument = errorCatalogResponse.Data.Document,
            CategoryCatalog = categoryCatalogResponse.Data.Document,
            CodeGroupCatalog = codeGroupCatalogResponse.Data.Document,
            OwnerCatalog = ownerCatalogResponse.Data.Document,
            ProfileCatalog = profileCatalogResponse.Data.Document,
            CrossValidationResult = crossValidationResult
        };

        return Response<ErrorCatalogContext>.Ok(context);
    }

    private static Response<ErrorCatalogContext> CreateFailedContextResponse<TPayload>(
        Response<TPayload> sourceResponse,
        string fallbackCode,
        string fallbackMessage)
        where TPayload : class
    {
        string issueCode = sourceResponse.Issues.Count > 0
            ? sourceResponse.Issues[0].Code
            : fallbackCode;

        string message = string.IsNullOrWhiteSpace(sourceResponse.Message)
            ? fallbackMessage
            : sourceResponse.Message;

        return Response<ErrorCatalogContext>.WithStatus(
            Response<ErrorCatalogContext>.Fail(
                code: issueCode,
                message: message),
            sourceResponse.Status);
    }
}