using System.Text.Json;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

[Collection(ConsoleOutputTestCollection.Name)]
public sealed class ListProfilesCommandJsonTests
{
    [Fact]
    public async Task ExecuteAsync_WithJsonOutput_WritesStableEnvelopeAndCompleteProfiles()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        int backupsBefore = CountProfileBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "list-profiles",
            workspace.ProjectRootPath,
            "--json"
        ]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement root = document.RootElement;
        JsonElement data = root.GetProperty("data");

        Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("list-profiles", root.GetProperty("command").GetString());
        Assert.True(data.GetProperty("loaded").GetBoolean());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("validation").ValueKind);

        JsonElement profiles = data.GetProperty("profiles");
        Assert.Equal(JsonValueKind.Array, profiles.ValueKind);
        Assert.NotEmpty(profiles.EnumerateArray());

        JsonElement firstProfile = profiles.EnumerateArray().First();
        Assert.False(string.IsNullOrWhiteSpace(firstProfile.GetProperty("name").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(firstProfile.GetProperty("displayName").GetString()));
        Assert.True(firstProfile.TryGetProperty("includeOwners", out _));
        Assert.True(firstProfile.TryGetProperty("defaultMappings", out _));
        Assert.True(firstProfile.TryGetProperty("metadata", out _));
        Assert.Equal(backupsBefore, CountProfileBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidWorkspaceAndJson_WritesStructuredValidationFailure()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            TemporaryWhenItFailsWorkspace.CreateEmpty();
        int backupsBefore = CountProfileBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "list-profiles",
            workspace.ProjectRootPath,
            "--json"
        ]);

        Assert.Equal(2, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement root = document.RootElement;
        JsonElement data = root.GetProperty("data");

        Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("list-profiles", root.GetProperty("command").GetString());
        Assert.False(data.GetProperty("loaded").GetBoolean());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("profiles").ValueKind);
        Assert.Equal(JsonValueKind.Object, data.GetProperty("validation").ValueKind);
        Assert.Equal(backupsBefore, CountProfileBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ExecuteAsync_WithDuplicateJsonSwitch_ReturnsInputErrorWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        int backupsBefore = CountProfileBackups(workspace.WhenItFailsJsonsPath);

        int exitCode = await ListProfilesCommand.ExecuteAsync(
        [
            "list-profiles",
            workspace.ProjectRootPath,
            "--json",
            "--json"
        ]);

        Assert.Equal(1, exitCode);
        Assert.Equal(backupsBefore, CountProfileBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ExecuteAsync_WithPlainAndJsonSwitches_ReturnsInputErrorWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        int backupsBefore = CountProfileBackups(workspace.WhenItFailsJsonsPath);

        int exitCode = await ListProfilesCommand.ExecuteAsync(
        [
            "list-profiles",
            workspace.ProjectRootPath,
            "--plain",
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
            int exitCode = await ListProfilesCommand.ExecuteAsync(args);
            return (exitCode, output.ToString());
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }

    private static int CountProfileBackups(string jsonsPath)
    {
        if (!Directory.Exists(jsonsPath))
        {
            return 0;
        }

        return Directory.GetFiles(
            jsonsPath,
            "profiles.*.bak.json",
            SearchOption.TopDirectoryOnly).Length;
    }
}
