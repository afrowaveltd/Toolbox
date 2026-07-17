using System.Text.Json;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

[Collection(ConsoleOutputTestCollection.Name)]
public sealed class InitCommandJsonTests
{
    [Fact]
    public void TryParseOptions_WithoutJson_ReturnsTrueAndJsonFalse()
    {
        bool result = InitCommand.TryParseOptions(
            ["init", "."],
            out bool useJsonOutput);

        Assert.True(result);
        Assert.False(useJsonOutput);
    }

    [Fact]
    public void TryParseOptions_WithCaseInsensitiveJson_ReturnsTrueAndJsonTrue()
    {
        bool result = InitCommand.TryParseOptions(
            ["init", ".", "--JSON"],
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
        List<string> args = ["init", ".", first];

        if (second is not null)
        {
            args.Add(second);
        }

        bool result = InitCommand.TryParseOptions(args.ToArray(), out _);

        Assert.False(result);
    }

    [Fact]
    public async Task ExecuteAsync_WithJson_CreatesWorkspaceAndWritesStableEnvelope()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            TemporaryWhenItFailsWorkspace.CreateEmpty();

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "init",
            workspace.ProjectRootPath,
            "--json"
        ]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement root = document.RootElement;
        JsonElement data = root.GetProperty("data");
        JsonElement bootstrap = data.GetProperty("bootstrap");

        Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("init", root.GetProperty("command").GetString());
        Assert.True(data.GetProperty("initialized").GetBoolean());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("failureCode").ValueKind);
        Assert.False(bootstrap.GetProperty("packageDirectoryAlreadyExisted").GetBoolean());
        Assert.True(bootstrap.GetProperty("packageDirectoryCreated").GetBoolean());
        Assert.Equal(workspace.WhenItFailsJsonsPath, bootstrap.GetProperty("packageDirectoryPath").GetString());

        JsonElement files = bootstrap.GetProperty("files");
        Assert.Equal(JsonValueKind.Array, files.ValueKind);
        Assert.NotEmpty(files.EnumerateArray());
        Assert.True(Directory.Exists(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ExecuteAsync_WithJsonTwice_ReportsExistingWorkspaceOnSecondRun()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            TemporaryWhenItFailsWorkspace.CreateEmpty();

        int firstExitCode = await InitCommand.ExecuteAsync(
        [
            "init",
            workspace.ProjectRootPath
        ]);

        Assert.Equal(0, firstExitCode);

        (int secondExitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "init",
            workspace.ProjectRootPath,
            "--json"
        ]);

        Assert.Equal(0, secondExitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement bootstrap = document.RootElement
            .GetProperty("data")
            .GetProperty("bootstrap");

        Assert.True(bootstrap.GetProperty("packageDirectoryAlreadyExisted").GetBoolean());
        Assert.False(bootstrap.GetProperty("packageDirectoryCreated").GetBoolean());
        Assert.NotEmpty(bootstrap.GetProperty("files").EnumerateArray());
    }

    [Theory]
    [InlineData("--json", "--json")]
    [InlineData("--unknown", null)]
    public async Task ExecuteAsync_WithInvalidSwitches_ReturnsInputError(
        string first,
        string? second)
    {
        using TemporaryWhenItFailsWorkspace workspace =
            TemporaryWhenItFailsWorkspace.CreateEmpty();
        List<string> args = ["init", workspace.ProjectRootPath, first];

        if (second is not null)
        {
            args.Add(second);
        }

        int exitCode = await InitCommand.ExecuteAsync(args.ToArray());

        Assert.Equal(1, exitCode);
        Assert.False(Directory.Exists(workspace.WhenItFailsJsonsPath));
    }

    private static async Task<(int ExitCode, string Output)> ExecuteWithCapturedOutputAsync(string[] args)
    {
        TextWriter originalOutput = Console.Out;
        using StringWriter output = new();

        try
        {
            Console.SetOut(output);
            int exitCode = await InitCommand.ExecuteAsync(args);
            return (exitCode, output.ToString());
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }
}
