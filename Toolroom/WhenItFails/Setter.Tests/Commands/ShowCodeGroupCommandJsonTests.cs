using System.Text.Json;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

[Collection(ConsoleOutputTestCollection.Name)]
public sealed class ShowCodeGroupCommandJsonTests
{
    [Theory]
    [InlineData("NETWORK")]
    [InlineData("NET")]
    public async Task ExecuteAsync_WithJsonOutput_WritesStableEnvelopeAndCompleteCodeGroup(
        string lookupValue)
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        int backupsBefore = CountCodeGroupBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "show-code-group",
            workspace.ProjectRootPath,
            lookupValue,
            "--json"
        ]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement root = document.RootElement;
        JsonElement data = root.GetProperty("data");

        Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("show-code-group", root.GetProperty("command").GetString());
        Assert.True(data.GetProperty("found").GetBoolean());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("failureCode").ValueKind);
        Assert.Equal(JsonValueKind.Null, data.GetProperty("failureMessage").ValueKind);
        Assert.Equal(JsonValueKind.Null, data.GetProperty("validation").ValueKind);

        JsonElement codeGroup = data.GetProperty("codeGroup");
        Assert.Equal("NETWORK", codeGroup.GetProperty("name").GetString());
        Assert.Equal("NET", codeGroup.GetProperty("codePrefix").GetString());
        Assert.True(codeGroup.TryGetProperty("codeFrom", out _));
        Assert.True(codeGroup.TryGetProperty("codeTo", out _));
        Assert.True(codeGroup.TryGetProperty("defaultCategories", out _));
        Assert.True(codeGroup.TryGetProperty("defaultTags", out _));
        Assert.Equal(backupsBefore, CountCodeGroupBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownCodeGroupAndJson_WritesStructuredFailure()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        int backupsBefore = CountCodeGroupBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "show-code-group",
            workspace.ProjectRootPath,
            "DOES_NOT_EXIST",
            "--json"
        ]);

        Assert.Equal(2, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement root = document.RootElement;
        JsonElement data = root.GetProperty("data");

        Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("show-code-group", root.GetProperty("command").GetString());
        Assert.False(data.GetProperty("found").GetBoolean());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("codeGroup").ValueKind);
        Assert.Equal("UnknownCodeGroup", data.GetProperty("failureCode").GetString());
        Assert.False(string.IsNullOrWhiteSpace(data.GetProperty("failureMessage").GetString()));
        Assert.Equal(JsonValueKind.Null, data.GetProperty("validation").ValueKind);
        Assert.Equal(backupsBefore, CountCodeGroupBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidWorkspaceAndJson_WritesStructuredValidationFailure()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            TemporaryWhenItFailsWorkspace.CreateEmpty();
        int backupsBefore = CountCodeGroupBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "show-code-group",
            workspace.ProjectRootPath,
            "NETWORK",
            "--json"
        ]);

        Assert.Equal(2, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement root = document.RootElement;
        JsonElement data = root.GetProperty("data");

        Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("show-code-group", root.GetProperty("command").GetString());
        Assert.False(data.GetProperty("found").GetBoolean());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("codeGroup").ValueKind);
        Assert.Equal(JsonValueKind.Null, data.GetProperty("failureCode").ValueKind);
        Assert.Equal(JsonValueKind.Null, data.GetProperty("failureMessage").ValueKind);
        Assert.Equal(JsonValueKind.Object, data.GetProperty("validation").ValueKind);
        Assert.Equal(backupsBefore, CountCodeGroupBackups(workspace.WhenItFailsJsonsPath));
    }

    [Theory]
    [InlineData("--json", "--json")]
    [InlineData("--plain", "--json")]
    public async Task ExecuteAsync_WithConflictingOutputSwitches_ReturnsInputErrorWithoutBackup(
        string first,
        string second)
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        int backupsBefore = CountCodeGroupBackups(workspace.WhenItFailsJsonsPath);

        int exitCode = await ShowCodeGroupCommand.ExecuteAsync(
        [
            "show-code-group",
            workspace.ProjectRootPath,
            "NETWORK",
            first,
            second
        ]);

        Assert.Equal(1, exitCode);
        Assert.Equal(backupsBefore, CountCodeGroupBackups(workspace.WhenItFailsJsonsPath));
    }

    private static async Task<(int ExitCode, string Output)> ExecuteWithCapturedOutputAsync(string[] args)
    {
        TextWriter originalOutput = Console.Out;
        using StringWriter output = new();

        try
        {
            Console.SetOut(output);
            int exitCode = await ShowCodeGroupCommand.ExecuteAsync(args);
            return (exitCode, output.ToString());
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }

    private static int CountCodeGroupBackups(string jsonsPath)
    {
        return Directory.Exists(jsonsPath)
            ? Directory.GetFiles(
                jsonsPath,
                "code-groups.*.bak.json",
                SearchOption.TopDirectoryOnly).Length
            : 0;
    }
}
