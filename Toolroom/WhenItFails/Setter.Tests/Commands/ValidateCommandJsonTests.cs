using System.Text.Json;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

[Collection(ConsoleOutputTestCollection.Name)]
public sealed class ValidateCommandJsonTests
{
    [Fact]
    public void TryParseOptions_WithoutJson_ReturnsTrueAndFalse()
    {
        bool result = ValidateCommand.TryParseOptions(
            ["validate", "."],
            out bool useJsonOutput);

        Assert.True(result);
        Assert.False(useJsonOutput);
    }

    [Fact]
    public void TryParseOptions_WithJson_ReturnsTrueAndTrue()
    {
        bool result = ValidateCommand.TryParseOptions(
            ["validate", ".", "--JSON"],
            out bool useJsonOutput);

        Assert.True(result);
        Assert.True(useJsonOutput);
    }

    [Theory]
    [InlineData("--json", "--json")]
    [InlineData("--unknown", null)]
    public void TryParseOptions_WithInvalidArguments_ReturnsFalse(
        string first,
        string? second)
    {
        string[] args = second is null
            ? ["validate", ".", first]
            : ["validate", ".", first, second];

        Assert.False(ValidateCommand.TryParseOptions(args, out _));
    }

    [Fact]
    public async Task ExecuteAsync_WithValidWorkspaceAndJson_WritesStableValidationEnvelope()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        int backupsBefore = CountAllBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "validate",
            workspace.ProjectRootPath,
            "--json"
        ]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement root = document.RootElement;
        JsonElement data = root.GetProperty("data");

        Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("validate", root.GetProperty("command").GetString());
        Assert.True(data.GetProperty("isValid").GetBoolean());
        Assert.False(string.IsNullOrWhiteSpace(data.GetProperty("displayPath").GetString()));
        Assert.Equal(JsonValueKind.Object, data.GetProperty("validation").ValueKind);
        Assert.Equal(backupsBefore, CountAllBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidWorkspaceAndJson_WritesStructuredValidationFailure()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            TemporaryWhenItFailsWorkspace.CreateEmpty();
        int backupsBefore = CountAllBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "validate",
            workspace.ProjectRootPath,
            "--json"
        ]);

        Assert.Equal(2, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement data = document.RootElement.GetProperty("data");

        Assert.False(data.GetProperty("isValid").GetBoolean());
        Assert.False(string.IsNullOrWhiteSpace(data.GetProperty("displayPath").GetString()));
        Assert.Equal(JsonValueKind.Object, data.GetProperty("validation").ValueKind);
        Assert.Equal(backupsBefore, CountAllBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ExecuteAsync_WithDuplicateJson_ReturnsInputErrorWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        int backupsBefore = CountAllBackups(workspace.WhenItFailsJsonsPath);

        int exitCode = await ValidateCommand.ExecuteAsync(
        [
            "validate",
            workspace.ProjectRootPath,
            "--json",
            "--json"
        ]);

        Assert.Equal(1, exitCode);
        Assert.Equal(backupsBefore, CountAllBackups(workspace.WhenItFailsJsonsPath));
    }

    private static async Task<(int ExitCode, string Output)> ExecuteWithCapturedOutputAsync(string[] args)
    {
        TextWriter originalOutput = Console.Out;
        using StringWriter output = new();

        try
        {
            Console.SetOut(output);
            int exitCode = await ValidateCommand.ExecuteAsync(args);
            return (exitCode, output.ToString());
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }

    private static int CountAllBackups(string jsonsPath)
    {
        return Directory.Exists(jsonsPath)
            ? Directory.GetFiles(
                jsonsPath,
                "*.bak.json",
                SearchOption.TopDirectoryOnly).Length
            : 0;
    }
}
