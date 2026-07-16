using System.Text.Json;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

[Collection(ConsoleOutputTestCollection.Name)]
public sealed class ProfileRemoveCodeGroupCommandJsonTests
{
    [Fact]
    public async Task ExecuteAsync_WithJsonOutput_WritesStableEnvelopeAndRemovesCanonicalCodeGroup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorProfileDefinition profile = await AddTestProfileAsync(workspace.ProjectRootPath);
        ErrorCodeGroupDefinition codeGroup = await LoadFirstCodeGroupAsync(workspace.WhenItFailsJsonsPath);
        Assert.True((await new WhenItFailsProfileWorkspaceEditor().ProfileAddCodeGroupAsync(
            workspace.ProjectRootPath,
            profile.Name,
            codeGroup.Name)).IsSuccess);
        int backupsBefore = CountProfileBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "profile-remove-code-group",
            workspace.ProjectRootPath,
            profile.Name,
            codeGroup.CodePrefix.ToLowerInvariant(),
            "--json"
        ]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement root = document.RootElement;
        JsonElement data = root.GetProperty("data");

        Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("profile-remove-code-group", root.GetProperty("command").GetString());
        Assert.True(data.GetProperty("updated").GetBoolean());
        Assert.Equal(codeGroup.Name, data.GetProperty("removedCodeGroup").GetString());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("failureCode").ValueKind);
        Assert.DoesNotContain(
            data.GetProperty("profile").GetProperty("includeCodeGroups").EnumerateArray(),
            item => string.Equals(item.GetString(), codeGroup.Name, StringComparison.OrdinalIgnoreCase));
        Assert.Equal(backupsBefore + 1, CountProfileBackups(workspace.WhenItFailsJsonsPath));

        ErrorProfileDefinition saved = await LoadProfileAsync(
            workspace.WhenItFailsJsonsPath,
            profile.Name);
        Assert.DoesNotContain(codeGroup.Name, saved.IncludeCodeGroups, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_WithJsonBeforeCodeGroup_RemovesCodeGroup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorProfileDefinition profile = await AddTestProfileAsync(workspace.ProjectRootPath);
        ErrorCodeGroupDefinition codeGroup = await LoadFirstCodeGroupAsync(workspace.WhenItFailsJsonsPath);
        Assert.True((await new WhenItFailsProfileWorkspaceEditor().ProfileAddCodeGroupAsync(
            workspace.ProjectRootPath,
            profile.Name,
            codeGroup.Name)).IsSuccess);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "profile-remove-code-group",
            workspace.ProjectRootPath,
            profile.DisplayName,
            "--json",
            codeGroup.Name
        ]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        Assert.Equal(
            codeGroup.Name,
            document.RootElement.GetProperty("data").GetProperty("removedCodeGroup").GetString());
    }

    [Fact]
    public async Task ExecuteAsync_WithCodeGroupNotIncludedAndJson_WritesStructuredFailureWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorProfileDefinition profile = await AddTestProfileAsync(workspace.ProjectRootPath);
        ErrorCodeGroupDefinition codeGroup = await LoadFirstCodeGroupAsync(workspace.WhenItFailsJsonsPath);
        int backupsBefore = CountProfileBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "profile-remove-code-group",
            workspace.ProjectRootPath,
            profile.Name,
            codeGroup.Name,
            "--json"
        ]);

        Assert.Equal(2, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement data = document.RootElement.GetProperty("data");
        Assert.False(data.GetProperty("updated").GetBoolean());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("profile").ValueKind);
        Assert.Equal(JsonValueKind.Null, data.GetProperty("removedCodeGroup").ValueKind);
        Assert.False(string.IsNullOrWhiteSpace(data.GetProperty("failureCode").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(data.GetProperty("failureMessage").GetString()));
        Assert.Equal(backupsBefore, CountProfileBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ExecuteAsync_WithDuplicateJsonSwitch_ReturnsInputErrorWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorProfileDefinition profile = await AddTestProfileAsync(workspace.ProjectRootPath);
        ErrorCodeGroupDefinition codeGroup = await LoadFirstCodeGroupAsync(workspace.WhenItFailsJsonsPath);
        int backupsBefore = CountProfileBackups(workspace.WhenItFailsJsonsPath);

        int exitCode = await ProfileRemoveCodeGroupCommand.ExecuteAsync(
        [
            "profile-remove-code-group",
            workspace.ProjectRootPath,
            profile.Name,
            codeGroup.Name,
            "--json",
            "--json"
        ]);

        Assert.Equal(1, exitCode);
        Assert.Equal(backupsBefore, CountProfileBackups(workspace.WhenItFailsJsonsPath));
    }

    private static async Task<(int ExitCode, string Output)> ExecuteWithCapturedOutputAsync(string[] args)
    {
        TextWriter originalOutput = Console.Out;
        using StringWriter output = new();

        try
        {
            Console.SetOut(output);
            int exitCode = await ProfileRemoveCodeGroupCommand.ExecuteAsync(args);
            return (exitCode, output.ToString());
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }

    private static async Task<ErrorProfileDefinition> AddTestProfileAsync(string projectRootPath)
    {
        Response<ErrorProfileDefinition> response =
            await new WhenItFailsProfileWorkspaceEditor().AddProfileAsync(
                projectRootPath,
                "JSON_REMOVE_CODE_GROUP_TEST",
                "JSON Remove Code Group Test");
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        return response.Data;
    }

    private static async Task<ErrorCodeGroupDefinition> LoadFirstCodeGroupAsync(string jsonsPath)
    {
        Response<ErrorCodeGroupCatalogDocument> response =
            await new JsonErrorCodeGroupCatalogLoader().LoadFromFileAsync(
                Path.Combine(jsonsPath, "code-groups.en.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);

        ErrorCodeGroupDefinition? codeGroup = new ErrorCodeGroupCatalogDocumentNormalizer()
            .Normalize(response.Data)
            .CodeGroups
            .FirstOrDefault();
        Assert.NotNull(codeGroup);
        return codeGroup;
    }

    private static async Task<ErrorProfileDefinition> LoadProfileAsync(
        string jsonsPath,
        string profileName)
    {
        Response<ErrorProfileCatalogDocument> response =
            await new JsonErrorProfileCatalogLoader().LoadFromFileAsync(
                Path.Combine(jsonsPath, "profiles.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);

        ErrorProfileDefinition? profile = new ErrorProfileCatalogDocumentNormalizer()
            .Normalize(response.Data)
            .Profiles
            .FirstOrDefault(candidate => string.Equals(
                candidate.Name,
                profileName,
                StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(profile);
        return profile;
    }

    private static int CountProfileBackups(string jsonsPath) => Directory.GetFiles(
        jsonsPath,
        "profiles.*.bak.json",
        SearchOption.TopDirectoryOnly).Length;
}
