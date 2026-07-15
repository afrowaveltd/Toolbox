using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Planning;

public sealed class WhenItFailsNextCodeFinderTests
{
    [Fact]
    public async Task FindAsync_ShouldReturnFirstFreeCodeAndStructuredIdWithoutWriting()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        ErrorCatalogDocument errors = await LoadErrorsAsync(workspace.WhenItFailsJsonsPath);
        ErrorOwnerCatalogDocument owners = await LoadOwnersAsync(workspace.WhenItFailsJsonsPath);
        ErrorCodeGroupCatalogDocument groups = await LoadGroupsAsync(workspace.WhenItFailsJsonsPath);
        (ErrorOwnerDefinition owner, ErrorCodeGroupDefinition group) =
            FindCompatiblePair(owners, groups);
        DateTime lastWriteBefore = File.GetLastWriteTimeUtc(
            Path.Combine(workspace.WhenItFailsJsonsPath, "errors.en.json"));
        int backupsBefore = CountBackups(workspace.WhenItFailsJsonsPath);

        int rangeFrom = Math.Max(owner.CodeFrom, group.CodeFrom);
        int rangeTo = Math.Min(owner.CodeTo, group.CodeTo);
        HashSet<int> usedCodes = errors.Errors.Select(error => error.Code).ToHashSet();
        int expectedCode = Enumerable.Range(rangeFrom, rangeTo - rangeFrom + 1)
            .First(code => !usedCodes.Contains(code));
        string ownerKey = TextKeyNormalizer.NormalizeKey(owner.Name);
        string prefix = TextKeyNormalizer.NormalizeKey(group.CodePrefix);
        HashSet<string> usedIds = errors.Errors
            .Select(error => TextKeyNormalizer.NormalizeKey(error.Id))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        int expectedSequence = Enumerable.Range(1, 100000)
            .First(sequence => !usedIds.Contains(
                $"{ownerKey}_{prefix}_{sequence:D4}"));

        string ownerLookup = owner.Aliases.FirstOrDefault() ?? owner.Name;
        Response<NextCodeSuggestion> response =
            await new WhenItFailsNextCodeFinder().FindAsync(
                workspace.ProjectRootPath,
                ownerLookup.ToLowerInvariant(),
                group.CodePrefix.ToLowerInvariant());

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.Equal(owner.Name, response.Data.Owner);
        Assert.Equal(group.Name, response.Data.CodeGroup);
        Assert.Equal(prefix, response.Data.CodePrefix);
        Assert.Equal(expectedCode, response.Data.Code);
        Assert.Equal(expectedSequence, response.Data.Sequence);
        Assert.Equal($"{ownerKey}_{prefix}_{expectedSequence:D4}", response.Data.Id);
        Assert.Equal(rangeFrom, response.Data.RangeFrom);
        Assert.Equal(rangeTo, response.Data.RangeTo);
        Assert.Equal(backupsBefore, CountBackups(workspace.WhenItFailsJsonsPath));
        Assert.Equal(
            lastWriteBefore,
            File.GetLastWriteTimeUtc(Path.Combine(workspace.WhenItFailsJsonsPath, "errors.en.json")));
    }

    [Fact]
    public async Task FindAsync_ShouldReturnNotFoundForUnknownOwner()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        Response<NextCodeSuggestion> response =
            await new WhenItFailsNextCodeFinder().FindAsync(
                workspace.ProjectRootPath,
                "DOES_NOT_EXIST",
                "NETWORK");

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == "OwnerNotFound");
        Assert.Equal(0, CountBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task FindAsync_ShouldReturnNotFoundForUnknownCodeGroup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorOwnerCatalogDocument owners = await LoadOwnersAsync(workspace.WhenItFailsJsonsPath);
        ErrorOwnerDefinition owner = owners.Owners.First();

        Response<NextCodeSuggestion> response =
            await new WhenItFailsNextCodeFinder().FindAsync(
                workspace.ProjectRootPath,
                owner.Name,
                "DOES_NOT_EXIST");

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == "CodeGroupNotFound");
        Assert.Equal(0, CountBackups(workspace.WhenItFailsJsonsPath));
    }

    [Theory]
    [InlineData("", "NETWORK", "OwnerNameIsEmpty")]
    [InlineData("AFW", "", "CodeGroupNameIsEmpty")]
    public async Task FindAsync_ShouldRejectEmptyValues(
        string ownerName,
        string codeGroupName,
        string issueCode)
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        Response<NextCodeSuggestion> response =
            await new WhenItFailsNextCodeFinder().FindAsync(
                workspace.ProjectRootPath,
                ownerName,
                codeGroupName);

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == issueCode);
        Assert.Equal(0, CountBackups(workspace.WhenItFailsJsonsPath));
    }

    private static (ErrorOwnerDefinition Owner, ErrorCodeGroupDefinition Group)
        FindCompatiblePair(
            ErrorOwnerCatalogDocument owners,
            ErrorCodeGroupCatalogDocument groups)
    {
        foreach (ErrorOwnerDefinition owner in owners.Owners)
        {
            foreach (ErrorCodeGroupDefinition group in groups.CodeGroups)
            {
                if (Math.Max(owner.CodeFrom, group.CodeFrom)
                    <= Math.Min(owner.CodeTo, group.CodeTo))
                {
                    return (owner, group);
                }
            }
        }

        throw new InvalidOperationException(
            "The test workspace contains no compatible owner and code-group pair.");
    }

    private static int CountBackups(string jsonsPath) => Directory.GetFiles(
        jsonsPath,
        "errors.en.*.bak.json",
        SearchOption.TopDirectoryOnly).Length;

    private static async Task<ErrorCatalogDocument> LoadErrorsAsync(string jsonsPath)
    {
        Response<ErrorCatalogDocument> response =
            await new JsonErrorCatalogLoader().LoadFromFileAsync(
                Path.Combine(jsonsPath, "errors.en.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        return new ErrorCatalogDocumentNormalizer().Normalize(response.Data);
    }

    private static async Task<ErrorOwnerCatalogDocument> LoadOwnersAsync(string jsonsPath)
    {
        Response<ErrorOwnerCatalogDocument> response =
            await new JsonErrorOwnerCatalogLoader().LoadFromFileAsync(
                Path.Combine(jsonsPath, "owners.en.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        return new ErrorOwnerCatalogDocumentNormalizer().Normalize(response.Data);
    }

    private static async Task<ErrorCodeGroupCatalogDocument> LoadGroupsAsync(string jsonsPath)
    {
        Response<ErrorCodeGroupCatalogDocument> response =
            await new JsonErrorCodeGroupCatalogLoader().LoadFromFileAsync(
                Path.Combine(jsonsPath, "code-groups.en.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        return new ErrorCodeGroupCatalogDocumentNormalizer().Normalize(response.Data);
    }
}
