using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

public sealed class SetOwnerCommandTests
{
    [Fact]
    public async Task ExecuteAsync_WithCompatibleOwnerAlias_ChangesOwnerAndId()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorDefinition error = await LoadFirstErrorAsync(workspace.WhenItFailsJsonsPath);
        ErrorOwnerDefinition owner = await AddOwnerAsync(
            workspace.WhenItFailsJsonsPath,
            "SETTER_OWNER",
            error.Code,
            error.Code,
            "SETTER");

        int exitCode = await SetOwnerCommand.ExecuteAsync(
        [
            "set-owner",
            workspace.ProjectRootPath,
            error.Name,
            owner.Aliases[0].ToLowerInvariant()
        ]);

        Assert.Equal(0, exitCode);

        string expectedId = ReplaceOwnerPrefix(error.Id, error.Owner, owner.Name);
        ErrorDefinition saved = await LoadErrorAsync(
            workspace.WhenItFailsJsonsPath,
            expectedId);
        Assert.Equal(owner.Name, saved.Owner);
        Assert.Equal(expectedId, saved.Id);
        Assert.Equal(error.Code, saved.Code);
    }

    [Fact]
    public async Task ExecuteAsync_WithOwnerOutsideCodeRange_ReturnsEditFailure()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorDefinition error = await LoadFirstErrorAsync(workspace.WhenItFailsJsonsPath);
        ErrorOwnerDefinition owner = await AddOwnerAsync(
            workspace.WhenItFailsJsonsPath,
            "OTHER_OWNER",
            error.Code + 1,
            error.Code + 100,
            "OTHER");

        int exitCode = await SetOwnerCommand.ExecuteAsync(
        [
            "set-owner",
            workspace.ProjectRootPath,
            error.Id,
            owner.Name
        ]);

        Assert.Equal(2, exitCode);
    }

    public static TheoryData<string[]> InvalidArgumentCases =>
        new()
        {
            new[] { "set-owner" },
            new[] { "set-owner", "." },
            new[] { "set-owner", ".", "AFW_NET_0001" },
            new[] { "set-owner", ".", "AFW_NET_0001", "APP", "EXTRA" }
        };

    [Theory]
    [MemberData(nameof(InvalidArgumentCases))]
    public async Task ExecuteAsync_WithInvalidArguments_ReturnsCommandInputError(string[] args)
    {
        Assert.Equal(1, await SetOwnerCommand.ExecuteAsync(args));
    }

    private static string ReplaceOwnerPrefix(
        string errorId,
        string currentOwner,
        string newOwner)
    {
        string normalizedId = TextKeyNormalizer.NormalizeKey(errorId);
        string currentPrefix = $"{TextKeyNormalizer.NormalizeKey(currentOwner)}_";
        Assert.StartsWith(currentPrefix, normalizedId, StringComparison.Ordinal);
        return $"{newOwner}_{normalizedId[currentPrefix.Length..]}";
    }

    private static async Task<ErrorOwnerDefinition> AddOwnerAsync(
        string jsonsPath,
        string ownerName,
        int codeFrom,
        int codeTo,
        string alias)
    {
        string filePath = Path.Combine(jsonsPath, "owners.en.json");
        Response<ErrorOwnerCatalogDocument> loadResponse =
            await new JsonErrorOwnerCatalogLoader().LoadFromFileAsync(filePath);
        Assert.True(loadResponse.IsSuccess);
        Assert.NotNull(loadResponse.Data);

        ErrorOwnerCatalogDocument catalog =
            new ErrorOwnerCatalogDocumentNormalizer().Normalize(loadResponse.Data);
        ErrorOwnerDefinition owner = new()
        {
            Name = ownerName,
            DisplayName = "Setter command test owner",
            CodeFrom = codeFrom,
            CodeTo = codeTo,
            IsBuiltIn = false,
            Aliases = [alias]
        };
        catalog.Owners.Add(owner);

        Response saveResponse =
            await new JsonCatalogDocumentWriter().SaveToFileAsync(catalog, filePath);
        Assert.True(saveResponse.IsSuccess);
        return owner;
    }

    private static async Task<ErrorDefinition> LoadFirstErrorAsync(string jsonsPath)
    {
        ErrorCatalogDocument catalog = await LoadErrorCatalogAsync(jsonsPath);
        ErrorDefinition? error = catalog.Errors.FirstOrDefault();
        Assert.NotNull(error);
        return error;
    }

    private static async Task<ErrorDefinition> LoadErrorAsync(
        string jsonsPath,
        string errorId)
    {
        ErrorCatalogDocument catalog = await LoadErrorCatalogAsync(jsonsPath);
        ErrorDefinition? error = catalog.Errors.FirstOrDefault(candidate =>
            string.Equals(candidate.Id, errorId, StringComparison.OrdinalIgnoreCase));
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
}
