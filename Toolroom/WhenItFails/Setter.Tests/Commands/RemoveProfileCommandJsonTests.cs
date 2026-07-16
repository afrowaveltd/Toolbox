using System.Text.Json;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

[Collection(ConsoleOutputTestCollection.Name)]
public sealed class RemoveProfileCommandJsonTests
{
    [Fact]
    public async Task ExecuteAsync_WithJsonOutput_WritesStableEnvelopeAndRemovesProfile()
    {
        using TemporaryWhenItFailsWorkspace workspace = await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorProfileDefinition added = (await new WhenItFailsProfileWorkspaceEditor().AddProfileAsync(
            workspace.ProjectRootPath, "REMOVE_JSON", "Remove JSON", "Temporary profile")).Data!;
        int backupsBefore = CountProfileBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync([
            "remove-profile", workspace.ProjectRootPath, added.DisplayName, "--json"]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement root = document.RootElement;
        JsonElement data = root.GetProperty("data");
        Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("remove-profile", root.GetProperty("command").GetString());
        Assert.True(data.GetProperty("removed").GetBoolean());
        Assert.Equal("REMOVE_JSON", data.GetProperty("profile").GetProperty("name").GetString());
        Assert.Equal("Remove JSON", data.GetProperty("profile").GetProperty("displayName").GetString());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("failureCode").ValueKind);
        Assert.Equal(backupsBefore + 1, CountProfileBackups(workspace.WhenItFailsJsonsPath));
        Assert.Null(await FindProfileAsync(workspace.WhenItFailsJsonsPath, "REMOVE_JSON"));
    }

    [Fact]
    public async Task ExecuteAsync_WithJsonBeforeProfileName_RemovesProfile()
    {
        using TemporaryWhenItFailsWorkspace workspace = await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        Assert.True((await new WhenItFailsProfileWorkspaceEditor().AddProfileAsync(
            workspace.ProjectRootPath, "JSON_BEFORE_REMOVE", "JSON Before Remove")).IsSuccess);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync([
            "remove-profile", workspace.ProjectRootPath, "--json", "JSON_BEFORE_REMOVE"]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        Assert.Equal("JSON_BEFORE_REMOVE", document.RootElement.GetProperty("data").GetProperty("profile").GetProperty("name").GetString());
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownProfileAndJson_WritesStructuredFailureWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace = await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        int backupsBefore = CountProfileBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync([
            "remove-profile", workspace.ProjectRootPath, "DOES_NOT_EXIST", "--json"]);

        Assert.Equal(2, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement data = document.RootElement.GetProperty("data");
        Assert.False(data.GetProperty("removed").GetBoolean());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("profile").ValueKind);
        Assert.False(string.IsNullOrWhiteSpace(data.GetProperty("failureCode").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(data.GetProperty("failureMessage").GetString()));
        Assert.Equal(backupsBefore, CountProfileBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ExecuteAsync_WithDuplicateJsonSwitch_ReturnsInputErrorWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace = await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        int backupsBefore = CountProfileBackups(workspace.WhenItFailsJsonsPath);
        int exitCode = await RemoveProfileCommand.ExecuteAsync([
            "remove-profile", workspace.ProjectRootPath, "WEB", "--json", "--json"]);
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
            return (await RemoveProfileCommand.ExecuteAsync(args), output.ToString());
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }

    private static async Task<ErrorProfileDefinition?> FindProfileAsync(string jsonsPath, string profileName)
    {
        ErrorProfileCatalogDocument document = (await new JsonErrorProfileCatalogLoader().LoadFromFileAsync(
            Path.Combine(jsonsPath, "profiles.json"))).Data!;
        return new ErrorProfileCatalogDocumentNormalizer().Normalize(document).Profiles
            .FirstOrDefault(candidate => string.Equals(candidate.Name, profileName, StringComparison.OrdinalIgnoreCase));
    }

    private static int CountProfileBackups(string jsonsPath) => Directory.GetFiles(
        jsonsPath, "profiles.*.bak.json", SearchOption.TopDirectoryOnly).Length;
}
