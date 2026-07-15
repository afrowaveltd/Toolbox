using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;

/// <summary>
/// Finds explicit profile references to one error definition without modifying the workspace.
/// </summary>
internal sealed class WhenItFailsErrorReferenceFinder
{
    /// <summary>
    /// Finds explicit include and exclude references to one error.
    /// </summary>
    public async Task<Response<ErrorReferenceReport>> FindAsync(
        string inputPath,
        string lookupValue,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);

        if (string.IsNullOrWhiteSpace(lookupValue))
        {
            return Response<ErrorReferenceReport>.Invalid(
                code: "ErrorLookupIsEmpty",
                message: "Error id, code, or name cannot be empty.");
        }

        JsonsOptions options = WhenItFailsWorkspacePathResolver.ResolveJsonsOptions(inputPath);

        Response<ErrorCatalogDocument> errorLoadResponse =
            await new JsonErrorCatalogLoader().LoadFromFileAsync(
                options.ErrorCatalogFilePath,
                cancellationToken);
        if (!errorLoadResponse.IsSuccess || errorLoadResponse.Data is null)
        {
            return Response<ErrorReferenceReport>.Fail(
                code: "ErrorCatalogLoadFailed",
                message: string.IsNullOrWhiteSpace(errorLoadResponse.Message)
                    ? $"Error catalog could not be loaded: {options.ErrorCatalogFilePath}"
                    : errorLoadResponse.Message);
        }

        ErrorCatalogDocument errorCatalog =
            new ErrorCatalogDocumentNormalizer().Normalize(errorLoadResponse.Data);
        if (!new ErrorCatalogValidator().Validate(errorCatalog).IsValid)
        {
            return Response<ErrorReferenceReport>.Invalid(
                code: "ErrorCatalogIsInvalid",
                message: "The error catalog is invalid and cannot be inspected safely.");
        }

        ErrorDefinition? errorDefinition = FindErrorDefinition(errorCatalog, lookupValue.Trim());
        if (errorDefinition is null)
        {
            return Response<ErrorReferenceReport>.NotFound(
                code: "ErrorDefinitionNotFound",
                message: $"No error definition was found for '{lookupValue}'. Search by id, code, or name.");
        }

        Response<ErrorProfileCatalogDocument> profileLoadResponse =
            await new JsonErrorProfileCatalogLoader().LoadFromFileAsync(
                options.ProfilesFilePath,
                cancellationToken);
        if (!profileLoadResponse.IsSuccess || profileLoadResponse.Data is null)
        {
            return Response<ErrorReferenceReport>.Fail(
                code: "ProfileCatalogLoadFailed",
                message: string.IsNullOrWhiteSpace(profileLoadResponse.Message)
                    ? $"Profile catalog could not be loaded: {options.ProfilesFilePath}"
                    : profileLoadResponse.Message);
        }

        ErrorProfileCatalogDocument profileCatalog =
            new ErrorProfileCatalogDocumentNormalizer().Normalize(profileLoadResponse.Data);
        if (!new ErrorProfileCatalogValidator().Validate(profileCatalog).IsValid)
        {
            return Response<ErrorReferenceReport>.Invalid(
                code: "ProfileCatalogIsInvalid",
                message: "The profile catalog is invalid and cannot be inspected safely.");
        }

        string normalizedErrorId = TextKeyNormalizer.NormalizeKey(errorDefinition.Id);
        List<ErrorProfileReference> references = [];

        foreach (ErrorProfileDefinition profile in profileCatalog.Profiles)
        {
            if (profile.IncludeErrors.Any(value => Matches(value, normalizedErrorId)))
            {
                references.Add(new ErrorProfileReference(
                    profile.Name,
                    profile.DisplayName,
                    "Include"));
            }

            if (profile.ExcludeErrors.Any(value => Matches(value, normalizedErrorId)))
            {
                references.Add(new ErrorProfileReference(
                    profile.Name,
                    profile.DisplayName,
                    "Exclude"));
            }
        }

        references = references
            .OrderBy(reference => reference.ProfileName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(reference => reference.ReferenceKind, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Response<ErrorReferenceReport>.Ok(
            new ErrorReferenceReport(
                errorDefinition.Id,
                errorDefinition.Code,
                errorDefinition.Name,
                references.Count(reference => reference.ReferenceKind == "Include"),
                references.Count(reference => reference.ReferenceKind == "Exclude"),
                references),
            references.Count == 0
                ? $"Error '{errorDefinition.Id}' has no explicit profile references."
                : $"Found {references.Count} explicit profile reference(s) for error '{errorDefinition.Id}'.");
    }

    private static bool Matches(string value, string normalizedErrorId) =>
        string.Equals(
            TextKeyNormalizer.NormalizeKey(value),
            normalizedErrorId,
            StringComparison.OrdinalIgnoreCase);

    private static ErrorDefinition? FindErrorDefinition(
        ErrorCatalogDocument catalog,
        string lookupValue)
    {
        if (int.TryParse(lookupValue, out int numericCode))
        {
            ErrorDefinition? byCode = catalog.Errors.FirstOrDefault(error => error.Code == numericCode);
            if (byCode is not null)
            {
                return byCode;
            }
        }

        return catalog.Errors.FirstOrDefault(error =>
            string.Equals(error.Id, lookupValue, StringComparison.OrdinalIgnoreCase)
            || string.Equals(error.Name, lookupValue, StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>
/// Describes explicit profile references to one error definition.
/// </summary>
internal sealed record ErrorReferenceReport(
    string ErrorId,
    int ErrorCode,
    string ErrorName,
    int IncludedByProfiles,
    int ExcludedByProfiles,
    IReadOnlyList<ErrorProfileReference> References);

/// <summary>
/// Describes one explicit include or exclude reference from a profile.
/// </summary>
internal sealed record ErrorProfileReference(
    string ProfileName,
    string DisplayName,
    string ReferenceKind);
