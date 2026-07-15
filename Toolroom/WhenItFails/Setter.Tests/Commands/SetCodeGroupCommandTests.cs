using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

public sealed class SetCodeGroupCommandTests
{
    [Fact]
    public async Task ExecuteAsync_WithValidArguments_ChangesGroupCodePrefixCodeAndId()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        (ErrorDefinition error, ErrorCodeGroupDefinition targetGroup, int expectedCode) =
            await FindCompatibleMoveAsync(workspace.WhenItFailsJsonsPath);

        int exitCode = await SetCodeGroupCommand.ExecuteAsync(
        [
            "set-code-group",
            workspace.ProjectRootPath,
            error.Name,
            targetGroup.CodePrefix.ToLowerInvariant()
        ]);

        Assert.Equal(0, exitCode);

        ErrorDefinition saved = await LoadErrorByCodeAsync(
            workspace.WhenItFailsJsonsPath,
            expectedCode);
        Assert.Equal(targetGroup.Name, saved.CodeGroup);
        Assert.Equal(targetGroup.CodePrefix, saved.CodePrefix);
        Assert.Equal(expectedCode, saved.Code);
        Assert.StartsWith(
            $"{TextKeyNormalizer.NormalizeKey(error.Owner)}_{targetGroup.CodePrefix}_",
            saved.Id,
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExecuteAsync_WithCurrentGroup_ReturnsEditFailure()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorDefinition error = await LoadFirstErrorAsync(workspace.WhenItFailsJsonsPath);

        int exitCode = await SetCodeGroupCommand.ExecuteAsync(
        [
            "set-code-group",
            workspace.ProjectRootPath,
            error.Id,
            error.CodePrefix
        ]);

        Assert.Equal(2, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownGroup_ReturnsEditFailure()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorDefinition error = await LoadFirstErrorAsync(workspace.WhenItFailsJsonsPath);

        int exitCode = await SetCodeGroupCommand.ExecuteAsync(
        [
            "set-code-group",
            workspace.ProjectRootPath,
            error.Code.ToString(),
            "DOES_NOT_EXIST"
        ]);

        Assert.Equal(2, exitCode);
    }

    public static TheoryData<string[]> InvalidArgumentCases =>
        new()
        {
            new[] { "set-code-group" },
            new[] { "set-code-group", "." },
            new[] { "set-code-group", ".", "AFW_NET_0001" },
            new[] { "set-code-group", ".", "AFW_NET_0001", "NETWORK", "EXTRA" }
        };

    [Theory]
    [MemberData(nameof(InvalidArgumentCases))]
    public async Task ExecuteAsync_WithInvalidArguments_ReturnsCommandInputError(string[] args)
    {
        Assert.Equal(1, await SetCodeGroupCommand.ExecuteAsync(args));
    }

    private static async Task<(ErrorDefinition Error, ErrorCodeGroupDefinition TargetGroup, int ExpectedCode)>
        FindCompatibleMoveAsync(string jsonsPath)
    {
        ErrorCatalogDocument errors = await LoadErrorCatalogAsync(jsonsPath);
        ErrorCodeGroupCatalogDocument groups = await LoadCodeGroupCatalogAsync(jsonsPath);
        ErrorOwnerCatalogDocument owners = await LoadOwnerCatalogAsync(jsonsPath);
        HashSet<int> usedCodes = errors.Errors.Select(error => error.Code).ToHashSet();
        HashSet<string> usedIds = errors.Errors
            .Select(error => error.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (ErrorDefinition error in errors.Errors)
        {
            ErrorOwnerDefinition? owner = owners.Owners.FirstOrDefault(candidate =>
                string.Equals(candidate.Name, error.Owner, StringComparison.OrdinalIgnoreCase));
            if (owner is null)
            {
                continue;
            }

            string currentPrefix = TextKeyNormalizer.NormalizeKey(error.CodePrefix);
            string currentId = TextKeyNormalizer.NormalizeKey(error.Id);
            string expectedPrefix = $"{TextKeyNormalizer.NormalizeKey(error.Owner)}_{currentPrefix}_";
            if (!currentId.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string suffix = currentId[expectedPrefix.Length..];
            foreach (ErrorCodeGroupDefinition group in groups.CodeGroups)
            {
                if (string.Equals(group.Name, error.CodeGroup, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                int from = Math.Max(group.CodeFrom, owner.CodeFrom);
                int to = Math.Min(group.CodeTo, owner.CodeTo);
                if (from > to)
                {
                    continue;
                }

                int? freeCode = Enumerable.Range(from, to - from + 1)
                    .Cast<int?>()
                    .FirstOrDefault(candidate => candidate.HasValue && !usedCodes.Contains(candidate.Value));
                if (freeCode is null)
                {
                    continue;
                }

                string predictedId =
                    $"{TextKeyNormalizer.NormalizeKey(error.Owner)}_{group.CodePrefix}_{suffix}";
                if (usedIds.Contains(predictedId))
                {
                    continue;
                }

                return (error, group, freeCode.Value);
            }
        }

        throw new InvalidOperationException("The test workspace contains no compatible code group move.");
    }

    private static async Task<ErrorDefinition> LoadFirstErrorAsync(string jsonsPath)
    {
        ErrorCatalogDocument catalog = await LoadErrorCatalogAsync(jsonsPath);
        ErrorDefinition? error = catalog.Errors.FirstOrDefault();
        Assert.NotNull(error);
        return error;
    }

    private static async Task<ErrorDefinition> LoadErrorByCodeAsync(
        string jsonsPath,
        int code)
    {
        ErrorCatalogDocument catalog = await LoadErrorCatalogAsync(jsonsPath);
        ErrorDefinition? error = catalog.Errors.FirstOrDefault(candidate => candidate.Code == code);
        Assert.NotNull(error);
        return error;
    }

    private static async Task<ErrorCatalogDocument> LoadErrorCatalogAsync(string jsonsPath)
    {
        Response<ErrorCatalogDocument> response =
            await new JsonErrorCatalogLoader().LoadFromFileAsync(
                Path.Combine(jsonsPath, "errors.en.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        return new ErrorCatalogDocumentNormalizer().Normalize(response.Data);
    }

    private static async Task<ErrorCodeGroupCatalogDocument> LoadCodeGroupCatalogAsync(string jsonsPath)
    {
        Response<ErrorCodeGroupCatalogDocument> response =
            await new JsonErrorCodeGroupCatalogLoader().LoadFromFileAsync(
                Path.Combine(jsonsPath, "code-groups.en.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        return new ErrorCodeGroupCatalogDocumentNormalizer().Normalize(response.Data);
    }

    private static async Task<ErrorOwnerCatalogDocument> LoadOwnerCatalogAsync(string jsonsPath)
    {
        Response<ErrorOwnerCatalogDocument> response =
            await new JsonErrorOwnerCatalogLoader().LoadFromFileAsync(
                Path.Combine(jsonsPath, "owners.en.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        return new ErrorOwnerCatalogDocumentNormalizer().Normalize(response.Data);
    }
}
