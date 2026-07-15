using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;

/// <summary>
/// Finds the next available numeric code and structured error id without modifying the workspace.
/// </summary>
internal sealed class WhenItFailsNextCodeFinder
{
    /// <summary>
    /// Finds the first available code in the intersection of an owner and code-group range.
    /// </summary>
    public async Task<Response<NextCodeSuggestion>> FindAsync(
        string inputPath,
        string ownerName,
        string codeGroupName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);

        if (string.IsNullOrWhiteSpace(ownerName))
        {
            return Response<NextCodeSuggestion>.Invalid(
                code: "OwnerNameIsEmpty",
                message: "Owner name or alias cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(codeGroupName))
        {
            return Response<NextCodeSuggestion>.Invalid(
                code: "CodeGroupNameIsEmpty",
                message: "Code group name or prefix cannot be empty.");
        }

        JsonsOptions options = WhenItFailsWorkspacePathResolver.ResolveJsonsOptions(inputPath);

        Response<ErrorCatalogDocument> errorResponse =
            await new JsonErrorCatalogLoader().LoadFromFileAsync(
                options.ErrorCatalogFilePath,
                cancellationToken);
        if (!errorResponse.IsSuccess || errorResponse.Data is null)
        {
            return LoadFailure<ErrorCatalogDocument>(
                errorResponse,
                "ErrorCatalogLoadFailed",
                $"Error catalog could not be loaded: {options.ErrorCatalogFilePath}");
        }

        ErrorCatalogDocument errors =
            new ErrorCatalogDocumentNormalizer().Normalize(errorResponse.Data);
        if (!new ErrorCatalogValidator().Validate(errors).IsValid)
        {
            return Response<NextCodeSuggestion>.Invalid(
                code: "ErrorCatalogIsInvalid",
                message: "The error catalog is invalid and cannot be inspected safely.");
        }

        Response<ErrorOwnerCatalogDocument> ownerResponse =
            await new JsonErrorOwnerCatalogLoader().LoadFromFileAsync(
                options.OwnerCatalogFilePath,
                cancellationToken);
        if (!ownerResponse.IsSuccess || ownerResponse.Data is null)
        {
            return LoadFailure<ErrorOwnerCatalogDocument>(
                ownerResponse,
                "OwnerCatalogLoadFailed",
                $"Owner catalog could not be loaded: {options.OwnerCatalogFilePath}");
        }

        ErrorOwnerCatalogDocument owners =
            new ErrorOwnerCatalogDocumentNormalizer().Normalize(ownerResponse.Data);
        if (!new ErrorOwnerCatalogValidator().Validate(owners).IsValid)
        {
            return Response<NextCodeSuggestion>.Invalid(
                code: "OwnerCatalogIsInvalid",
                message: "The owner catalog is invalid and cannot be inspected safely.");
        }

        Response<ErrorCodeGroupCatalogDocument> groupResponse =
            await new JsonErrorCodeGroupCatalogLoader().LoadFromFileAsync(
                options.CodeGroupCatalogFilePath,
                cancellationToken);
        if (!groupResponse.IsSuccess || groupResponse.Data is null)
        {
            return LoadFailure<ErrorCodeGroupCatalogDocument>(
                groupResponse,
                "CodeGroupCatalogLoadFailed",
                $"Code group catalog could not be loaded: {options.CodeGroupCatalogFilePath}");
        }

        ErrorCodeGroupCatalogDocument groups =
            new ErrorCodeGroupCatalogDocumentNormalizer().Normalize(groupResponse.Data);
        if (!new ErrorCodeGroupCatalogValidator().Validate(groups).IsValid)
        {
            return Response<NextCodeSuggestion>.Invalid(
                code: "CodeGroupCatalogIsInvalid",
                message: "The code group catalog is invalid and cannot be inspected safely.");
        }

        string normalizedOwner = TextKeyNormalizer.NormalizeKey(ownerName);
        ErrorOwnerDefinition? owner = owners.Owners.FirstOrDefault(candidate =>
            string.Equals(candidate.Name, normalizedOwner, StringComparison.OrdinalIgnoreCase)
            || candidate.Aliases.Any(alias => string.Equals(
                alias,
                normalizedOwner,
                StringComparison.OrdinalIgnoreCase)));
        if (owner is null)
        {
            return Response<NextCodeSuggestion>.NotFound(
                code: "OwnerNotFound",
                message: $"Owner '{normalizedOwner}' was not found.");
        }

        string normalizedGroup = TextKeyNormalizer.NormalizeKey(codeGroupName);
        ErrorCodeGroupDefinition? group = groups.CodeGroups.FirstOrDefault(candidate =>
            string.Equals(candidate.Name, normalizedGroup, StringComparison.OrdinalIgnoreCase)
            || string.Equals(candidate.CodePrefix, normalizedGroup, StringComparison.OrdinalIgnoreCase));
        if (group is null)
        {
            return Response<NextCodeSuggestion>.NotFound(
                code: "CodeGroupNotFound",
                message: $"Code group '{normalizedGroup}' was not found.");
        }

        int rangeFrom = Math.Max(owner.CodeFrom, group.CodeFrom);
        int rangeTo = Math.Min(owner.CodeTo, group.CodeTo);
        if (rangeFrom > rangeTo)
        {
            return Response<NextCodeSuggestion>.Invalid(
                code: "OwnerAndCodeGroupRangesDoNotIntersect",
                message: $"Owner '{owner.Name}' and code group '{group.Name}' have no shared numeric range.");
        }

        HashSet<int> usedCodes = errors.Errors.Select(error => error.Code).ToHashSet();
        int? nextCode = FindFirstFreeCode(rangeFrom, rangeTo, usedCodes);
        if (nextCode is null)
        {
            return Response<NextCodeSuggestion>.Invalid(
                code: "NoAvailableCode",
                message: $"No free error code exists in range {rangeFrom}-{rangeTo} for owner '{owner.Name}' and code group '{group.Name}'.");
        }

        string ownerKey = TextKeyNormalizer.NormalizeKey(owner.Name);
        string codePrefix = TextKeyNormalizer.NormalizeKey(group.CodePrefix);
        HashSet<string> usedIds = errors.Errors
            .Select(error => TextKeyNormalizer.NormalizeKey(error.Id))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        int sequence = FindFirstFreeSequence(ownerKey, codePrefix, usedIds);
        int sequenceWidth = Math.Max(4, sequence.ToString().Length);
        string suggestedId = $"{ownerKey}_{codePrefix}_{sequence.ToString($"D{sequenceWidth}")}";

        NextCodeSuggestion suggestion = new(
            Owner: owner.Name,
            CodeGroup: group.Name,
            CodePrefix: codePrefix,
            Code: nextCode.Value,
            Id: suggestedId,
            Sequence: sequence,
            RangeFrom: rangeFrom,
            RangeTo: rangeTo);

        return Response<NextCodeSuggestion>.Ok(
            suggestion,
            $"Next available code for owner '{owner.Name}' and code group '{group.Name}' is {nextCode.Value}.");
    }

    private static int? FindFirstFreeCode(int rangeFrom, int rangeTo, HashSet<int> usedCodes)
    {
        for (int code = rangeFrom; code <= rangeTo; code++)
        {
            if (!usedCodes.Contains(code))
            {
                return code;
            }

            if (code == int.MaxValue)
            {
                break;
            }
        }

        return null;
    }

    private static int FindFirstFreeSequence(
        string owner,
        string codePrefix,
        HashSet<string> usedIds)
    {
        for (int sequence = 1; sequence < int.MaxValue; sequence++)
        {
            int width = Math.Max(4, sequence.ToString().Length);
            string candidate = $"{owner}_{codePrefix}_{sequence.ToString($"D{width}")}";
            if (!usedIds.Contains(candidate))
            {
                return sequence;
            }
        }

        throw new InvalidOperationException(
            $"No structured id sequence is available for '{owner}_{codePrefix}'.");
    }

    private static Response<NextCodeSuggestion> LoadFailure<T>(
        Response<T> response,
        string fallbackCode,
        string fallbackMessage)
    {
        return Response<NextCodeSuggestion>.Fail(
            code: response.Issues.Count > 0 ? response.Issues[0].Code : fallbackCode,
            message: string.IsNullOrWhiteSpace(response.Message)
                ? fallbackMessage
                : response.Message);
    }
}

/// <summary>
/// Describes the next available numeric code and structured identifier.
/// </summary>
internal sealed record NextCodeSuggestion(
    string Owner,
    string CodeGroup,
    string CodePrefix,
    int Code,
    string Id,
    int Sequence,
    int RangeFrom,
    int RangeTo);
