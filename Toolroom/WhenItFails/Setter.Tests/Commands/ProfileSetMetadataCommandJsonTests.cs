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
public sealed class ProfileSetMetadataCommandJsonTests
{
    [Fact]
    public async Task ExecuteAsync_WithJsonOutput_WritesStableEnvelopeAndPersistsNormalizedMetadata()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorProfileDefinition profile = await AddTestProfileAsync(workspace.ProjectRootPath);
        int backupsBefore = CountProfileBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "profile-set-metadata",
            workspace.ProjectRootPath,
            profile.DisplayName,
            "documentation.owner",
            " DiTa Team ",
            "--json"
        ]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement root = document.RootElement;
        JsonElement data = root.GetProperty("data");
        string canonicalKey = TextKeyNormalizer.NormalizeKey("documentation.owner");

        Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("profile-set-metadata", root.GetProperty("command").GetString());
        Assert.True(data.GetProperty("updated").GetBoolean());
        Assert.Equal(profile.Name, data.GetProperty("profile").GetProperty("name").GetString());
        Assert.Equal(canonicalKey, data.GetProperty("metadataKey").GetString());
        Assert.Equal("DiTa Team", data.GetProperty("metadataValue").GetString());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("failureCode").ValueKind);
        Assert.Equal(backupsBefore + 1, CountProfileBackups(workspace.WhenItFailsJsonsPath));

        ErrorProfileDefinition saved = await LoadProfileAsync(
            workspace.WhenItFailsJsonsPath,
            profile.Name);
        Assert.True(saved.Metadata.TryGet(canonicalKey, out string? savedValue));
        Assert.Equal("DiTa Team", savedValue);
    }

    [Fact]
    public async Task ExecuteAsync_WithJsonBeforeMetadataArguments_SetsMetadata()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorProfileDefinition profile = await AddTestProfileAsync(workspace.ProjectRootPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "profile-set-metadata",
            workspace.ProjectRootPath,
            profile.Name,
            "--json",
            "release.channel",
            "preview"
        ]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        Assert.Equal(
            "preview",
            document.RootElement.GetProperty("data").GetProperty("metadataValue").GetString());
    }

    [Fact]
    public async Task ExecuteAsync_WithSameMetadataAndJson_WritesStructuredFailureWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorProfileDefinition profile = await AddTestProfileAsync(workspace.ProjectRootPath);
        Assert.True((await new WhenItFailsProfileWorkspaceEditor().ProfileSetMetadataAsync(
            workspace.ProjectRootPath,
            profile.Name,
            "documentation.owner",
            "DiTa Team")).IsSuccess);
        int backupsBefore = CountProfileBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "profile-set-metadata",
            workspace.ProjectRootPath,
            profile.Name,
            "DOCUMENTATION_OWNER",
            "DiTa Team",
            "--json"
        ]);

        Assert.Equal(2, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement data = document.RootElement.GetProperty("data");
        Assert.False(data.GetProperty("updated").GetBoolean());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("profile").ValueKind);
        Assert.Equal(JsonValueKind.Null, data.GetProperty("metadataKey").ValueKind);
        Assert.Equal(JsonValueKind.Null, data.GetProperty("metadataValue").ValueKind);
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
        int backupsBefore = CountProfileBackups(workspace.WhenItFailsJsonsPath);

        int exitCode = await ProfileSetMetadataCommand.ExecuteAsync(
        [
            "profile-set-metadata",
            workspace.ProjectRootPath,
            profile.Name,
            "KEY",
            "VALUE",
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
            int exitCode = await ProfileSetMetadataCommand.ExecuteAsync(args);
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
                "JSON_SET_METADATA_TEST",
                "JSON Set Metadata Test");
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        return response.Data;
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
