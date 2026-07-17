using System.Text.Json;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

[Collection(ConsoleOutputTestCollection.Name)]
public sealed class ErrorsCommandJsonTests
{
    [Fact]
    public async Task ExecuteAsync_WithJsonOutput_WritesStableEnvelopeAndCompleteErrors()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        int backupsBefore = CountBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "errors",
            workspace.ProjectRootPath,
            "--json"
        ]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement root = document.RootElement;
        JsonElement data = root.GetProperty("data");

        Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("errors", root.GetProperty("command").GetString());
        Assert.True(data.GetProperty("loaded").GetBoolean());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("failureCode").ValueKind);
        Assert.Equal(JsonValueKind.Null, data.GetProperty("validation").ValueKind);

        JsonElement errors = data.GetProperty("errors");
        Assert.Equal(JsonValueKind.Array, errors.ValueKind);
        JsonElement firstError = errors.EnumerateArray().First();
        Assert.False(string.IsNullOrWhiteSpace(firstError.GetProperty("id").GetString()));
        Assert.True(firstError.TryGetProperty("code", out _));
        Assert.False(string.IsNullOrWhiteSpace(firstError.GetProperty("name").GetString()));
        Assert.Equal(backupsBefore, CountBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ExecuteAsync_WithCodeGroupAliasAndJson_ReturnsFilteredErrorsAndOptions()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "errors",
            workspace.ProjectRootPath,
            "--code-group",
            "NETWORK",
            "--json"
        ]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement data = document.RootElement.GetProperty("data");
        JsonElement options = data.GetProperty("options");
        JsonElement errors = data.GetProperty("errors");

        Assert.Equal("NETWORK", options.GetProperty("codeGroup").GetString());
        Assert.True(options.GetProperty("useJsonOutput").GetBoolean());
        Assert.All(
            errors.EnumerateArray(),
            error => Assert.Equal("NETWORK", error.GetProperty("codeGroup").GetString()));
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownProfileAndJson_WritesStructuredFailure()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        int backupsBefore = CountBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "errors",
            workspace.ProjectRootPath,
            "--profile",
            "DOES_NOT_EXIST",
            "--json"
        ]);

        Assert.Equal(1, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement data = document.RootElement.GetProperty("data");

        Assert.True(data.GetProperty("loaded").GetBoolean());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("errors").ValueKind);
        Assert.Equal("UnknownProfileFilter", data.GetProperty("failureCode").GetString());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("validation").ValueKind);
        Assert.Equal(backupsBefore, CountBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidWorkspaceAndJson_WritesStructuredValidationFailure()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            TemporaryWhenItFailsWorkspace.CreateEmpty();
        int backupsBefore = CountBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "errors",
            workspace.ProjectRootPath,
            "--json"
        ]);

        Assert.Equal(2, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement data = document.RootElement.GetProperty("data");

        Assert.False(data.GetProperty("loaded").GetBoolean());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("errors").ValueKind);
        Assert.Equal(JsonValueKind.Object, data.GetProperty("validation").ValueKind);
        Assert.Equal(backupsBefore, CountBackups(workspace.WhenItFailsJsonsPath));
    }

    [Theory]
    [InlineData("--json", "--json")]
    [InlineData("--plain", "--json")]
    [InlineData("--group", "NETWORK", "--code-group", "NETWORK")]
    public async Task ExecuteAsync_WithDuplicateOrConflictingOptions_ReturnsInputError(
        params string[] options)
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        int backupsBefore = CountBackups(workspace.WhenItFailsJsonsPath);
        string[] args = ["errors", workspace.ProjectRootPath, .. options];

        int exitCode = await ErrorsCommand.ExecuteAsync(args);

        Assert.Equal(1, exitCode);
        Assert.Equal(backupsBefore, CountBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public void TryParseErrorListOptions_WithMissingFilterValue_ReturnsFalse()
    {
        bool result = ErrorsCommand.TryParseErrorListOptions(
            ["errors", ".", "--owner"],
            out _);

        Assert.False(result);
    }

    private static async Task<(int ExitCode, string Output)> ExecuteWithCapturedOutputAsync(
        string[] args)
    {
        TextWriter originalOutput = Console.Out;
        using StringWriter output = new();

        try
        {
            Console.SetOut(output);
            int exitCode = await ErrorsCommand.ExecuteAsync(args);
            return (exitCode, output.ToString());
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }

    private static int CountBackups(string jsonsPath)
    {
        return Directory.Exists(jsonsPath)
            ? Directory.GetFiles(
                jsonsPath,
                "*.bak.json",
                SearchOption.TopDirectoryOnly).Length
            : 0;
    }
}
