using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

public sealed class ProfileRemoveCodeGroupCommandTests
{
    [Fact]
    public async Task ExecuteAsync_WithIncludedPrefix_RemovesCodeGroupAndReturnsSuccess()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        WhenItFailsProfileWorkspaceEditor editor = new();
        Assert.True((await editor.AddProfileAsync(workspace.ProjectRootPath, "DITA_TEST", "DiTa Test")).IsSuccess);

        ErrorCodeGroupDefinition codeGroup = await LoadFirstCodeGroupAsync(workspace.WhenItFailsJsonsPath);
        Assert.True((await editor.ProfileAddCodeGroupAsync(workspace.ProjectRootPath, "DITA_TEST", codeGroup.Name)).IsSuccess);

        int exitCode = await ProfileRemoveCodeGroupCommand.ExecuteAsync(
            [
                "profile-remove-code-group",
                workspace.ProjectRootPath,
                "dita test",
                codeGroup.CodePrefix.ToLowerInvariant()
            ]);

        Assert.Equal(0, exitCode);

        ErrorProfileDefinition savedProfile =
            await LoadProfileAsync(workspace.WhenItFailsJsonsPath, "DITA_TEST");

        Assert.DoesNotContain(codeGroup.Name, savedProfile.IncludeCodeGroups);
    }

    [Fact]
    public async Task ExecuteAsync_WithCodeGroupNotIncluded_ReturnsEditFailure()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        WhenItFailsProfileWorkspaceEditor editor = new();
        Assert.True((await editor.AddProfileAsync(workspace.ProjectRootPath, "DITA_TEST", "DiTa Test")).IsSuccess);

        ErrorCodeGroupDefinition codeGroup = await LoadFirstCodeGroupAsync(workspace.WhenItFailsJsonsPath);

        int exitCode = await ProfileRemoveCodeGroupCommand.ExecuteAsync(
            ["profile-remove-code-group", workspace.ProjectRootPath, "DITA_TEST", codeGroup.Name]);

        Assert.Equal(2, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownCodeGroup_ReturnsEditFailure()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        WhenItFailsProfileWorkspaceEditor editor = new();
        Assert.True((await editor.AddProfileAsync(workspace.ProjectRootPath, "DITA_TEST", "DiTa Test")).IsSuccess);

        int exitCode = await ProfileRemoveCodeGroupCommand.ExecuteAsync(
            ["profile-remove-code-group", workspace.ProjectRootPath, "DITA_TEST", "DOES_NOT_EXIST"]);

        Assert.Equal(2, exitCode);
    }

    public static TheoryData<string[]> MissingArgumentCases =>
        new()
        {
            new[] { "profile-remove-code-group" },
            new[] { "profile-remove-code-group", "." },
            new[] { "profile-remove-code-group", ".", "DITA_TEST" }
        };

    [Theory]
    [MemberData(nameof(MissingArgumentCases))]
    public async Task ExecuteAsync_WithMissingRequiredArgument_ReturnsCommandInputError(string[] args)
    {
        Assert.Equal(1, await ProfileRemoveCodeGroupCommand.ExecuteAsync(args));
    }

    [Fact]
    public async Task ExecuteAsync_WithTooManyArguments_ReturnsCommandInputError()
    {
        int exitCode = await ProfileRemoveCodeGroupCommand.ExecuteAsync(
            ["profile-remove-code-group", ".", "DITA_TEST", "NET", "Unexpected"]);

        Assert.Equal(1, exitCode);
    }

    private static async Task<ErrorCodeGroupDefinition> LoadFirstCodeGroupAsync(string jsonsPath)
    {
        Response<ErrorCodeGroupCatalogDocument> response =
            await new JsonErrorCodeGroupCatalogLoader().LoadFromFileAsync(
                Path.Combine(jsonsPath, "code-groups.en.json"));

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);

        ErrorCodeGroupDefinition? codeGroup =
            new ErrorCodeGroupCatalogDocumentNormalizer().Normalize(response.Data).CodeGroups.FirstOrDefault();

        Assert.NotNull(codeGroup);
        return codeGroup;
    }

    private static async Task<ErrorProfileDefinition> LoadProfileAsync(string jsonsPath, string profileName)
    {
        Response<ErrorProfileCatalogDocument> response =
            await new JsonErrorProfileCatalogLoader().LoadFromFileAsync(Path.Combine(jsonsPath, "profiles.json"));

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);

        ErrorProfileDefinition? profile =
            new ErrorProfileCatalogDocumentNormalizer().Normalize(response.Data).Profiles.FirstOrDefault(candidate =>
                string.Equals(candidate.Name, profileName, StringComparison.OrdinalIgnoreCase));

        Assert.NotNull(profile);
        return profile;
    }
}
