using System.Text.Json;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

[Collection(ConsoleOutputTestCollection.Name)]
public sealed class ListCodeGroupsCommandJsonTests
{
    [Fact]
    public async Task ExecuteAsync_WithJsonOutput_WritesStableEnvelopeAndCompleteCodeGroups()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        int backupsBefore = CountCodeGroupBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "list-code-groups",
            workspace.ProjectRootPath,
            "--json"
        ]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement root = document.RootElement;
        JsonElement data = root.GetProperty("data");

        Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("list-code-groups", root.GetProperty("command").GetString());
        Assert.True(data.GetProperty("loaded").GetBoolean());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("validation").ValueKind);

        JsonElement codeGroups = data.GetProperty("codeGroups");
        Assert.Equal(JsonValueKind.Array, codeGroups.ValueKind);
        Assert.NotEmpty(codeGroups.EnumerateArray());

        JsonElement firstCodeGroup = codeGroups.EnumerateArray().First();
        Assert.False(string.IsNullOrWhiteSpace(firstCodeGroup.GetProperty("name").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(firstCodeGroup.GetProperty("displayName").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(firstCodeGroup.GetProperty("codePrefix").GetString()));
        Assert.True(firstCodeGroup.TryGetProperty("codeFrom", out _));
        Assert.True(firstCodeGroup.TryGetProperty("codeTo", out _));
        Assert.True(firstCodeGroup.TryGetProperty("defaultCategories", out _));
        Assert.True(firstCodeGroup.TryGetProperty("defaultTags", out _));
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
            "list-code-groups",
            workspace.ProjectRootPath,
            "--json"
        ]);

        Assert.Equal(2, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement root = document.RootElement;
        JsonElement data = root.GetProperty("data");

        Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("list-code-groups", root.GetProperty("command").GetString());
        Assert.False(data.GetProperty("loaded").GetBoolean());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("codeGroups").ValueKind);
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

        int exitCode = await ListCodeGroupsCommand.ExecuteAsync(
        [
            "list-code-groups",
            workspace.ProjectRootPath,
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
            int exitCode = await ListCodeGroupsCommand.ExecuteAsync(args);
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
