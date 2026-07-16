using System.Text.Json;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

[Collection(ConsoleOutputTestCollection.Name)]
public sealed class AddProfileCommandJsonTests
{
    [Fact]
    public async Task ExecuteAsync_WithJsonOutput_WritesStableEnvelopeAndAddsProfile()
    {
        using TemporaryWhenItFailsWorkspace workspace = await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        int backupsBefore = CountProfileBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync([
            "add-profile", workspace.ProjectRootPath, " dita json ", " DiTa JSON ", " Test profile. ", "--json"]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement root = document.RootElement;
        JsonElement data = root.GetProperty("data");
        Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("add-profile", root.GetProperty("command").GetString());
        Assert.True(data.GetProperty("added").GetBoolean());
        Assert.Equal("DITA_JSON", data.GetProperty("profile").GetProperty("name").GetString());
        Assert.Equal("DiTa JSON", data.GetProperty("profile").GetProperty("displayName").GetString());
        Assert.Equal("Test profile.", data.GetProperty("profile").GetProperty("description").GetString());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("failureCode").ValueKind);
        Assert.Equal(backupsBefore + 1, CountProfileBackups(workspace.WhenItFailsJsonsPath));

        ErrorProfileDefinition saved = await LoadProfileAsync(workspace.WhenItFailsJsonsPath, "DITA_JSON");
        Assert.Equal("DiTa JSON", saved.DisplayName);
    }

    [Fact]
    public async Task ExecuteAsync_WithJsonBeforeDescription_AddsProfile()
    {
        using TemporaryWhenItFailsWorkspace workspace = await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync([
            "add-profile", workspace.ProjectRootPath, "JSON_BEFORE", "JSON Before", "--json", "Description"]);
        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        Assert.Equal("JSON_BEFORE", document.RootElement.GetProperty("data").GetProperty("profile").GetProperty("name").GetString());
    }

    [Fact]
    public async Task ExecuteAsync_WithDuplicateProfileAndJson_WritesStructuredFailureWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace = await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        int backupsBefore = CountProfileBackups(workspace.WhenItFailsJsonsPath);
        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync([
            "add-profile", workspace.ProjectRootPath, "WEB", "Duplicate Web", "--json"]);
        Assert.Equal(2, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement data = document.RootElement.GetProperty("data");
        Assert.False(data.GetProperty("added").GetBoolean());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("profile").ValueKind);
        Assert.False(string.IsNullOrWhiteSpace(data.GetProperty("failureCode").GetString()));
        Assert.Equal(backupsBefore, CountProfileBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ExecuteAsync_WithDuplicateJsonSwitch_ReturnsInputErrorWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace = await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        int backupsBefore = CountProfileBackups(workspace.WhenItFailsJsonsPath);
        int exitCode = await AddProfileCommand.ExecuteAsync([
            "add-profile", workspace.ProjectRootPath, "DUPLICATE_JSON", "Duplicate JSON", "--json", "--json"]);
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
            return (await AddProfileCommand.ExecuteAsync(args), output.ToString());
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }

    private static async Task<ErrorProfileDefinition> LoadProfileAsync(string jsonsPath, string profileName)
    {
        ErrorProfileCatalogDocument document = (await new JsonErrorProfileCatalogLoader().LoadFromFileAsync(
            Path.Combine(jsonsPath, "profiles.json"))).Data!;
        ErrorProfileDefinition? profile = new ErrorProfileCatalogDocumentNormalizer().Normalize(document).Profiles
            .FirstOrDefault(candidate => string.Equals(candidate.Name, profileName, StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(profile);
        return profile;
    }

    private static int CountProfileBackups(string jsonsPath) => Directory.GetFiles(
        jsonsPath, "profiles.*.bak.json", SearchOption.TopDirectoryOnly).Length;
}
