using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;

/// <summary>
/// Provides safe creation of error definitions.
/// </summary>
internal static class WhenItFailsWorkspaceEditorAddErrorExtensions
{
    private static readonly HashSet<string> AllowedSeverities =
    [
        "Trace",
        "Debug",
        "Information",
        "Warning",
        "Error",
        "Critical"
    ];

    /// <summary>
    /// Adds one complete error definition using the next available identity.
    /// </summary>
    public static async Task<Response<ErrorDefinition>> AddErrorAsync(
        this WhenItFailsWorkspaceEditor editor,
        string inputPath,
        AddErrorRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(editor);
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);
        ArgumentNullException.ThrowIfNull(request);

        Response<ErrorDefinition>? inputFailure = ValidateRequest(request);
        if (inputFailure is not null)
        {
            return inputFailure;
        }

        string normalizedName = TextKeyNormalizer.NormalizeKey(request.Name);
        string normalizedSeverity = NormalizeSeverity(request.DefaultSeverity);

        Response<NextCodeSuggestion> identityResponse =
            await new WhenItFailsNextCodeFinder().FindAsync(
                inputPath,
                request.Owner,
                request.CodeGroup,
                cancellationToken);
        if (!identityResponse.IsSuccess || identityResponse.Data is null)
        {
            return Response<ErrorDefinition>.Fail(
                code: identityResponse.Issues.Count > 0
                    ? identityResponse.Issues[0].Code
                    : "NextErrorIdentityFailed",
                message: string.IsNullOrWhiteSpace(identityResponse.Message)
                    ? "The next error identity could not be determined."
                    : identityResponse.Message);
        }

        JsonsOptions options = WhenItFailsWorkspacePathResolver.ResolveJsonsOptions(inputPath);
        Response<ErrorCatalogDocument> errorLoadResponse =
            await new JsonErrorCatalogLoader().LoadFromFileAsync(
                options.ErrorCatalogFilePath,
                cancellationToken);
        if (!errorLoadResponse.IsSuccess || errorLoadResponse.Data is null)
        {
            return Response<ErrorDefinition>.Fail(
                code: "ErrorCatalogLoadFailed",
                message: string.IsNullOrWhiteSpace(errorLoadResponse.Message)
                    ? $"Error catalog could not be loaded: {options.ErrorCatalogFilePath}"
                    : errorLoadResponse.Message);
        }

        ErrorCatalogDocument errorCatalog =
            new ErrorCatalogDocumentNormalizer().Normalize(errorLoadResponse.Data);

        if (errorCatalog.Errors.Any(error => string.Equals(
                error.Name,
                normalizedName,
                StringComparison.OrdinalIgnoreCase)))
        {
            return Response<ErrorDefinition>.Invalid(
                code: "ErrorNameAlreadyExists",
                message: $"An error named '{normalizedName}' already exists.");
        }

        NextCodeSuggestion identity = identityResponse.Data;
        if (errorCatalog.Errors.Any(error =>
                error.Code == identity.Code
                || string.Equals(error.Id, identity.Id, StringComparison.OrdinalIgnoreCase)))
        {
            return Response<ErrorDefinition>.Invalid(
                code: "SuggestedErrorIdentityAlreadyExists",
                message: $"Suggested error identity '{identity.Id}' / {identity.Code} is already in use.");
        }

        Response<ErrorCategoryCatalogDocument> categoryLoadResponse =
            await new JsonErrorCategoryCatalogLoader().LoadFromFileAsync(
                options.CategoryCatalogFilePath,
                cancellationToken);
        if (!categoryLoadResponse.IsSuccess || categoryLoadResponse.Data is null)
        {
            return Response<ErrorDefinition>.Fail(
                code: "CategoryCatalogLoadFailed",
                message: string.IsNullOrWhiteSpace(categoryLoadResponse.Message)
                    ? $"Category catalog could not be loaded: {options.CategoryCatalogFilePath}"
                    : categoryLoadResponse.Message);
        }

        ErrorCategoryCatalogDocument categoryCatalog =
            new ErrorCategoryCatalogDocumentNormalizer().Normalize(categoryLoadResponse.Data);
        string normalizedCategory = TextKeyNormalizer.NormalizeKey(request.PrimaryCategory);
        ErrorCategoryDefinition? category = categoryCatalog.Categories.FirstOrDefault(candidate =>
            string.Equals(candidate.Name, normalizedCategory, StringComparison.OrdinalIgnoreCase)
            || candidate.Aliases.Any(alias => string.Equals(
                alias,
                normalizedCategory,
                StringComparison.OrdinalIgnoreCase)));
        if (category is null)
        {
            return Response<ErrorDefinition>.NotFound(
                code: "CategoryNotFound",
                message: $"Category '{normalizedCategory}' was not found.");
        }

        ErrorDefinition errorDefinition = new()
        {
            Id = identity.Id,
            Code = identity.Code,
            Name = normalizedName,
            Owner = identity.Owner,
            CodePrefix = identity.CodePrefix,
            CodeGroup = identity.CodeGroup,
            PrimaryCategory = category.Name,
            Categories = [category.Name],
            Title = request.Title.Trim(),
            Message = request.Message.Trim(),
            DefaultSeverity = normalizedSeverity
        };

        errorCatalog.Errors.Add(errorDefinition);
        ErrorCatalogValidationResult validationResult =
            new ErrorCatalogValidator().Validate(errorCatalog);
        if (!validationResult.IsValid)
        {
            errorCatalog.Errors.Remove(errorDefinition);
            return Response<ErrorDefinition>.Invalid(
                code: "EditedErrorCatalogIsInvalid",
                message: "The error catalog with the new definition is invalid and was not saved.");
        }

        Response saveResponse = await new JsonCatalogDocumentWriter().SaveToFileAsync(
            errorCatalog,
            options.ErrorCatalogFilePath,
            cancellationToken);
        if (!saveResponse.IsSuccess)
        {
            errorCatalog.Errors.Remove(errorDefinition);
            return Response<ErrorDefinition>.Fail(
                code: saveResponse.Issues.Count > 0
                    ? saveResponse.Issues[0].Code
                    : "ErrorCatalogSaveFailed",
                message: string.IsNullOrWhiteSpace(saveResponse.Message)
                    ? "Error catalog could not be saved."
                    : saveResponse.Message);
        }

        return Response<ErrorDefinition>.Ok(
            errorDefinition,
            $"Added error '{errorDefinition.Id}' ({errorDefinition.Code}).");
    }

    private static Response<ErrorDefinition>? ValidateRequest(AddErrorRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Owner))
        {
            return Invalid("OwnerNameIsEmpty", "Owner name or alias cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(request.CodeGroup))
        {
            return Invalid("CodeGroupNameIsEmpty", "Code group name or prefix cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(request.PrimaryCategory))
        {
            return Invalid("PrimaryCategoryNameIsEmpty", "Primary category name or alias cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Invalid("ErrorNameIsEmpty", "Error name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return Invalid("ErrorTitleIsEmpty", "Error title cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return Invalid("ErrorMessageIsEmpty", "Error message cannot be empty.");
        }

        string severity = NormalizeSeverity(request.DefaultSeverity);
        if (!AllowedSeverities.Contains(severity))
        {
            return Invalid(
                "DefaultSeverityIsInvalid",
                "Default severity must be Trace, Debug, Information, Warning, Error, or Critical.");
        }

        return null;
    }

    private static string NormalizeSeverity(string? severity)
    {
        string value = string.IsNullOrWhiteSpace(severity)
            ? "Error"
            : severity.Trim();

        return AllowedSeverities.FirstOrDefault(candidate => string.Equals(
                   candidate,
                   value,
                   StringComparison.OrdinalIgnoreCase))
               ?? value;
    }

    private static Response<ErrorDefinition> Invalid(string code, string message) =>
        Response<ErrorDefinition>.Invalid(code: code, message: message);
}

/// <summary>
/// Contains the required values for creating one error definition.
/// </summary>
internal sealed record AddErrorRequest(
    string Owner,
    string CodeGroup,
    string PrimaryCategory,
    string Name,
    string Title,
    string Message,
    string DefaultSeverity = "Error");
