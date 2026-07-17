using System.Text.Json;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

[Collection(ConsoleOutputTestCollection.Name)]
public sealed class ShowProfileCommandJsonTests
{
    [Fact]
    public async Task ExecuteAsync_WithJsonOutput_WritesStableEnvelopeAndCompleteProfile()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        int backupsBefore = CountProfileBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "show-profile",
            workspace.ProjectRootPath,
            "web",
            "--json"
        ]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement root = document.RootElement;
        JsonElement data = root.GetProperty("data");

        Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("show-profile", root.GetProperty("command").GetString());
        Assert.True(data.GetProperty("loaded").GetBoolean());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("validation").ValueKind);
        Assert.Equal(JsonValueKind.Null, data.GetProperty("failureCode").ValueKind);

        JsonElement profile = data.GetProperty("profile");
        Assert.Equal("WEB", profile.GetProperty("name").GetString());
        Assert.False(string.IsNullOrWhiteSpace(profile.GetProperty("displayName").GetString()));
        Assert.True(profile.TryGetProperty("includeOwners", out _));
        Assert.True(profile.TryGetProperty("defaultMappings", out _));
        Assert.True(profile.TryGetProperty("metadata", out _));
        Assert.Equal(backupsBefore, CountProfileBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ExecuteAsync_WithDisplayNameAndJson_FindsProfile()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "show-profile",
            workspace.ProjectRootPath,
            "Web",
            "--json"
        ]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        Assert.Equal(
            "WEB",
            document.RootElement.GetProperty("data").GetProperty("profile").GetProperty("name").GetString());
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownProfileAndJson_WritesStructuredFailureWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        int backupsBefore = CountProfileBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "show-profile",
            workspace.ProjectRootPath,
            "DOES_NOT_EXIST",
            "--json"
        ]);

        Assert.Equal(2, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement data = document.RootElement.GetProperty("data");

        Assert.True(data.GetProperty("loaded").GetBoolean());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("profile").ValueKind);
        Assert.Equal(JsonValueKind.Null, data.GetProperty("validation").ValueKind);
        Assert.Equal("UnknownProfile", data.GetProperty("failureCode").GetString());
        Assert.False(string.IsNullOrWhiteSpace(data.GetProperty("failureMessage").GetString()));
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
            "show-profile",
            workspace.ProjectRootPath,
            "WEB",
            "--json"
        ]);

        Assert.Equal(2, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement data = document.RootElement.GetProperty("data");

        Assert.False(data.GetProperty("loaded").GetBoolean());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("profile").ValueKind);
        Assert.Equal(JsonValueKind.Object, data.GetProperty("validation").ValueKind);
        Assert.Equal(JsonValueKind.Null, data.GetProperty("failureCode").ValueKind);
        Assert.Equal(backupsBefore, CountProfileBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ExecuteAsync_WithDuplicateJsonSwitch_ReturnsInputErrorWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        int backupsBefore = CountProfileBackups(workspace.WhenItFailsJsonsPath);

        int exitCode = await ShowProfileCommand.ExecuteAsync(
        [
            "show-profile",
            workspace.ProjectRootPath,
            "WEB",
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
            int exitCode = await ShowProfileCommand.ExecuteAsync(args);
            return (exitCode, output.ToString());
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }

    private static int CountProfileBackups(string jsonsPath)
    {
        return Directory.Exists(jsonsPath)
            ? Directory.GetFiles(
                jsonsPath,
                "profiles.*.bak.json",
                SearchOption.TopDirectoryOnly).Length
            : 0;
    }
}
