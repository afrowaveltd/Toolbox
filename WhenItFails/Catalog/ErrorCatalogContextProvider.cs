using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Issues;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Enums;
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

        Response<ErrorCatalogContext>? invalidOptionsResponse =
            ValidateJsonsOptions(options);

        if (invalidOptionsResponse is not null)
        {
            return invalidOptionsResponse;
        }

        List<IssueInfo> providerIssues = [];

        Response<ErrorCatalogProviderPayload>? errorCatalogResponse =
            await _errorCatalogProvider.LoadFromFileAsync(
                options.ErrorCatalogFilePath,
                cancellationToken);

        if (errorCatalogResponse is null)
        {
            return CreateNullProviderResponse(
                "WIF_ERROR_CATALOG_PROVIDER_RESPONSE_NULL",
                "The error catalog provider returned a null response.");
        }

        if (!errorCatalogResponse.IsSuccess)
        {
            return CreateFailedContextResponse(
                errorCatalogResponse,
                "ErrorCatalogContextErrorCatalogLoadFailed",
                "Error catalog loading failed while creating catalog context.");
        }

        if (errorCatalogResponse.Data is null
            || errorCatalogResponse.Data.Catalog is null
            || errorCatalogResponse.Data.Document is null)
        {
            return CreateNullPayloadResponse();
        }

        AddProviderIssues(providerIssues, errorCatalogResponse.Issues);

        Response<ErrorCategoryCatalogProviderPayload>? categoryCatalogResponse =
            await _categoryCatalogProvider.LoadFromFileAsync(
                options.CategoryCatalogFilePath,
                cancellationToken);

        if (categoryCatalogResponse is null)
        {
            return CreateNullProviderResponse(
                "WIF_CATEGORY_CATALOG_PROVIDER_RESPONSE_NULL",
                "The error category catalog provider returned a null response.");
        }

        if (!categoryCatalogResponse.IsSuccess)
        {
            return CreateFailedContextResponse(
                categoryCatalogResponse,
                "ErrorCatalogContextCategoryCatalogLoadFailed",
                "Category catalog loading failed while creating catalog context.");
        }

        if (categoryCatalogResponse.Data is null)
        {
            return CreateNullPayloadResponse();
        }

        AddProviderIssues(providerIssues, categoryCatalogResponse.Issues);

        Response<ErrorCodeGroupCatalogProviderPayload>? codeGroupCatalogResponse =
            await _codeGroupCatalogProvider.LoadFromFileAsync(
                options.CodeGroupCatalogFilePath,
                cancellationToken);

        if (codeGroupCatalogResponse is null)
        {
            return CreateNullProviderResponse(
                "WIF_CODE_GROUP_CATALOG_PROVIDER_RESPONSE_NULL",
                "The error code group catalog provider returned a null response.");
        }

        if (!codeGroupCatalogResponse.IsSuccess)
        {
            return CreateFailedContextResponse(
                codeGroupCatalogResponse,
                "ErrorCatalogContextCodeGroupCatalogLoadFailed",
                "Code group catalog loading failed while creating catalog context.");
        }

        if (codeGroupCatalogResponse.Data is null)
        {
            return CreateNullPayloadResponse();
        }

        AddProviderIssues(providerIssues, codeGroupCatalogResponse.Issues);

        Response<ErrorOwnerCatalogProviderPayload>? ownerCatalogResponse =
            await _ownerCatalogProvider.LoadFromFileAsync(
                options.OwnerCatalogFilePath,
                cancellationToken);

        if (ownerCatalogResponse is null)
        {
            return CreateNullProviderResponse(
                "WIF_OWNER_CATALOG_PROVIDER_RESPONSE_NULL",
                "The error owner catalog provider returned a null response.");
        }

        if (!ownerCatalogResponse.IsSuccess)
        {
            return CreateFailedContextResponse(
                ownerCatalogResponse,
                "ErrorCatalogContextOwnerCatalogLoadFailed",
                "Owner catalog loading failed while creating catalog context.");
        }

        if (ownerCatalogResponse.Data is null)
        {
            return CreateNullPayloadResponse();
        }

        AddProviderIssues(providerIssues, ownerCatalogResponse.Issues);

        Response<ErrorProfileCatalogProviderPayload>? profileCatalogResponse =
            await _profileCatalogProvider.LoadFromFileAsync(
                options.ProfilesFilePath,
                cancellationToken);

        if (profileCatalogResponse is null)
        {
            return CreateNullProviderResponse(
                "WIF_PROFILE_CATALOG_PROVIDER_RESPONSE_NULL",
                "The error profile catalog provider returned a null response.");
        }

        if (!profileCatalogResponse.IsSuccess)
        {
            return CreateFailedContextResponse(
                profileCatalogResponse,
                "ErrorCatalogContextProfileCatalogLoadFailed",
                "Profile catalog loading failed while creating catalog context.");
        }

        if (profileCatalogResponse.Data is null)
        {
            return CreateNullPayloadResponse();
        }

        AddProviderIssues(providerIssues, profileCatalogResponse.Issues);

        cancellationToken.ThrowIfCancellationRequested();

        ErrorCatalogCrossValidator crossValidator = new();

        ErrorCatalogValidationResult crossValidationResult = crossValidator.Validate(
            errorCatalogResponse.Data.Document,
            ownerCatalogResponse.Data.Document,
            codeGroupCatalogResponse.Data.Document,
            categoryCatalogResponse.Data.Document,
            profileCatalogResponse.Data.Document);

        if (!crossValidationResult.IsValid)
        {
            var errorIssue = crossValidationResult.Issues.FirstOrDefault(
                issue => issue.Severity == ErrorCatalogValidationSeverity.Error);

            string issueCode = errorIssue?.Code
                ?? "ErrorCatalogContextCrossValidationFailed";

            string issueMessage = errorIssue?.Message
                ?? "Error catalog cross-validation failed while creating catalog context.";

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

        bool hasWarnings = providerIssues.Any(
            issue => issue.Severity >= IssueSeverity.Warning);

        return hasWarnings
            ? Response<ErrorCatalogContext>.OkWithWarnings(context, providerIssues)
            : new Response<ErrorCatalogContext>
            {
                Status = ResultStatus.Success,
                Data = context,
                Issues = providerIssues
            };
    }

    private static Response<ErrorCatalogContext>? ValidateJsonsOptions(
        JsonsOptions options)
    {
        Response<ErrorCatalogContext>? response = ValidateFileName(
            options.ErrorCatalogFileName,
            "WIF_JSONS_ERROR_CATALOG_FILE_NAME_NULL",
            "The error catalog file name cannot be null.",
            "WIF_JSONS_ERROR_CATALOG_FILE_NAME_EMPTY",
            "The error catalog file name cannot be empty.");

        if (response is not null)
        {
            return response;
        }

        response = ValidateFileName(
            options.CategoryCatalogFileName,
            "WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_NULL",
            "The category catalog file name cannot be null.",
            "WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_EMPTY",
            "The category catalog file name cannot be empty.");

        if (response is not null)
        {
            return response;
        }

        response = ValidateFileName(
            options.CodeGroupCatalogFileName,
            "WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_NULL",
            "The code group catalog file name cannot be null.",
            "WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_EMPTY",
            "The code group catalog file name cannot be empty.");

        if (response is not null)
        {
            return response;
        }

        response = ValidateFileName(
            options.OwnerCatalogFileName,
            "WIF_JSONS_OWNER_CATALOG_FILE_NAME_NULL",
            "The owner catalog file name cannot be null.",
            "WIF_JSONS_OWNER_CATALOG_FILE_NAME_EMPTY",
            "The owner catalog file name cannot be empty.");

        if (response is not null)
        {
            return response;
        }

        return ValidateFileName(
            options.ProfilesFileName,
            "WIF_JSONS_PROFILE_CATALOG_FILE_NAME_NULL",
            "The profile catalog file name cannot be null.",
            "WIF_JSONS_PROFILE_CATALOG_FILE_NAME_EMPTY",
            "The profile catalog file name cannot be empty.");
    }

    private static Response<ErrorCatalogContext>? ValidateFileName(
        string? fileName,
        string nullCode,
        string nullMessage,
        string emptyCode,
        string emptyMessage)
    {
        if (fileName is null)
        {
            return Response<ErrorCatalogContext>.Invalid(
                code: nullCode,
                message: nullMessage);
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            return Response<ErrorCatalogContext>.Invalid(
                code: emptyCode,
                message: emptyMessage);
        }

        return null;
    }

    private static void AddProviderIssues(
        List<IssueInfo> target,
        IReadOnlyList<IssueInfo>? issues)
    {
        if (issues is null)
        {
            return;
        }

        foreach (IssueInfo? issue in issues)
        {
            if (issue is not null)
            {
                target.Add(issue);
            }
        }
    }

    private static Response<ErrorCatalogContext> CreateFailedContextResponse<TPayload>(
        Response<TPayload> sourceResponse,
        string fallbackCode,
        string fallbackMessage)
        where TPayload : class
    {
        IssueInfo? sourceIssue = sourceResponse.Issues?
            .FirstOrDefault(issue => issue is not null);

        string issueCode = string.IsNullOrWhiteSpace(sourceIssue?.Code)
            ? fallbackCode
            : sourceIssue.Code;

        string message = string.IsNullOrWhiteSpace(sourceResponse.Message)
            ? fallbackMessage
            : sourceResponse.Message;

        return Response<ErrorCatalogContext>.WithStatus(
            Response<ErrorCatalogContext>.Fail(
                code: issueCode,
                message: message),
            sourceResponse.Status);
    }

    private static Response<ErrorCatalogContext> CreateNullProviderResponse(
        string code,
        string message)
    {
        return Response<ErrorCatalogContext>.Invalid(
            code: code,
            message: message);
    }

    private static Response<ErrorCatalogContext> CreateNullPayloadResponse()
    {
        return Response<ErrorCatalogContext>.Invalid(
            code: "ErrorCatalogContextPayloadIsNull",
            message: "One or more catalog provider responses succeeded without payload data.");
    }
}
