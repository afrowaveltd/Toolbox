using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

public sealed class NextCodeCommandTests
{
    [Fact]
    public async Task ExecuteAsync_WithCompatibleOwnerAndGroup_ReturnsSuccessWithoutChangingCatalogs()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        (ErrorOwnerDefinition owner, ErrorCodeGroupDefinition group) =
            await FindCompatiblePairAsync(workspace.ProjectRootPath, workspace.WhenItFailsJsonsPath);
        Dictionary<string, string> before = await ReadCatalogFilesAsync(workspace.WhenItFailsJsonsPath);
        int backupsBefore = CountBackups(workspace.WhenItFailsJsonsPath);

        int exitCode = await NextCodeCommand.ExecuteAsync(
        [
            "next-code",
            workspace.ProjectRootPath,
            owner.Aliases.FirstOrDefault() ?? owner.Name,
            group.CodePrefix,
            "--plain"
        ]);

        Assert.Equal(0, exitCode);
        Assert.Equal(backupsBefore, CountBackups(workspace.WhenItFailsJsonsPath));
        Dictionary<string, string> after = await ReadCatalogFilesAsync(workspace.WhenItFailsJsonsPath);
        Assert.Equal(before, after);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownOwner_ReturnsPlanningFailure()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorCodeGroupDefinition group = await LoadFirstCodeGroupAsync(workspace.WhenItFailsJsonsPath);

        int exitCode = await NextCodeCommand.ExecuteAsync(
        [
            "next-code",
            workspace.ProjectRootPath,
            "DOES_NOT_EXIST",
            group.Name
        ]);

        Assert.Equal(2, exitCode);
    }

    public static TheoryData<string[]> InvalidArgumentCases =>
        new()
        {
            new[] { "next-code" },
            new[] { "next-code", "." },
            new[] { "next-code", ".", "AFW" },
            new[] { "next-code", ".", "AFW", "NETWORK", "--unknown" },
            new[] { "next-code", ".", "AFW", "NETWORK", "--plain", "EXTRA" }
        };

    [Theory]
    [MemberData(nameof(InvalidArgumentCases))]
    public async Task ExecuteAsync_WithInvalidArguments_ReturnsCommandInputError(string[] args)
    {
        Assert.Equal(1, await NextCodeCommand.ExecuteAsync(args));
    }

    private static async Task<(ErrorOwnerDefinition Owner, ErrorCodeGroupDefinition Group)>
        FindCompatiblePairAsync(string projectRootPath, string jsonsPath)
    {
        ErrorOwnerCatalogDocument owners = await LoadOwnersAsync(jsonsPath);
        ErrorCodeGroupCatalogDocument groups = await LoadCodeGroupsAsync(jsonsPath);
        WhenItFailsNextCodeFinder finder = new();

        foreach (ErrorOwnerDefinition owner in owners.Owners)
        {
            foreach (ErrorCodeGroupDefinition group in groups.CodeGroups)
            {
                Response<NextCodeSuggestion> response = await finder.FindAsync(
                    projectRootPath,
                    owner.Name,
                    group.Name);
                if (response.IsSuccess)
                {
                    return (owner, group);
                }
            }
        }

        throw new InvalidOperationException("The test workspace contains no compatible owner and code-group pair.");
    }

    private static async Task<ErrorCodeGroupDefinition> LoadFirstCodeGroupAsync(string jsonsPath)
    {
        ErrorCodeGroupCatalogDocument catalog = await LoadCodeGroupsAsync(jsonsPath);
        ErrorCodeGroupDefinition? group = catalog.CodeGroups.FirstOrDefault();
        Assert.NotNull(group);
        return group;
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

    private static async Task<ErrorCodeGroupCatalogDocument> LoadCodeGroupsAsync(string jsonsPath)
    {
        Response<ErrorCodeGroupCatalogDocument> response =
            await new JsonErrorCodeGroupCatalogLoader().LoadFromFileAsync(
                Path.Combine(jsonsPath, "code-groups.en.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        return new ErrorCodeGroupCatalogDocumentNormalizer().Normalize(response.Data);
    }

    private static async Task<Dictionary<string, string>> ReadCatalogFilesAsync(string jsonsPath)
    {
        string[] names =
        [
            "errors.en.json",
            "owners.en.json",
            "code-groups.en.json"
        ];

        Dictionary<string, string> contents = new(StringComparer.OrdinalIgnoreCase);
        foreach (string name in names)
        {
            contents[name] = await File.ReadAllTextAsync(Path.Combine(jsonsPath, name));
        }

        return contents;
    }

    private static int CountBackups(string jsonsPath) => Directory.GetFiles(
        jsonsPath,
        "*.bak.json",
        SearchOption.TopDirectoryOnly).Length;
}
