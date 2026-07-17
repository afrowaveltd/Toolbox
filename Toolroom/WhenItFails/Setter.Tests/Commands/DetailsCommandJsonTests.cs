using System.Text.Json;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

[Collection(ConsoleOutputTestCollection.Name)]
public sealed class DetailsCommandJsonTests
{
    [Fact]
    public void TryParseOptions_WithJsonSwitch_ReturnsJsonOutput()
    {
        bool result = DetailsCommand.TryParseOptions(
            ["details", ".", "AFW_NET_0001", "--json"],
            out bool usePlainOutput,
            out bool useJsonOutput);

        Assert.True(result);
        Assert.False(usePlainOutput);
        Assert.True(useJsonOutput);
    }

    [Theory]
    [InlineData("--json", "--json")]
    [InlineData("--plain", "--json")]
    [InlineData("--unknown", "--json")]
    public void TryParseOptions_WithInvalidOutputArguments_ReturnsFalse(
        string first,
        string second)
    {
        bool result = DetailsCommand.TryParseOptions(
            ["details", ".", "AFW_NET_0001", first, second],
            out _,
            out _);

        Assert.False(result);
    }

    [Fact]
    public async Task ExecuteAsync_WithJsonOutput_WritesStableEnvelopeAndCompleteError()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "details",
            workspace.ProjectRootPath,
            "AFW_NET_0001",
            "--json"
        ]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement root = document.RootElement;
        JsonElement data = root.GetProperty("data");
        JsonElement error = data.GetProperty("error");

        Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("details", root.GetProperty("command").GetString());
        Assert.True(data.GetProperty("found").GetBoolean());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("failureCode").ValueKind);
        Assert.Equal(JsonValueKind.Null, data.GetProperty("validation").ValueKind);
        Assert.Equal("AFW_NET_0001", error.GetProperty("id").GetString());
        Assert.False(string.IsNullOrWhiteSpace(error.GetProperty("name").GetString()));
        Assert.True(error.TryGetProperty("code", out _));
        Assert.True(error.TryGetProperty("categories", out _));
        Assert.True(error.TryGetProperty("metadata", out _));
        Assert.Equal(backupsBefore, CountErrorBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ExecuteAsync_WithDetailAliasAndJson_UsesCanonicalEnvelopeCommand()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "detail",
            workspace.ProjectRootPath,
            "AFW_NET_0001",
            "--json"
        ]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);

        Assert.Equal("details", document.RootElement.GetProperty("command").GetString());
        Assert.True(document.RootElement.GetProperty("data").GetProperty("found").GetBoolean());
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownErrorAndJson_WritesStructuredLookupFailure()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "details",
            workspace.ProjectRootPath,
            "DOES_NOT_EXIST",
            "--json"
        ]);

        Assert.Equal(1, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement data = document.RootElement.GetProperty("data");

        Assert.False(data.GetProperty("found").GetBoolean());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("error").ValueKind);
        Assert.Equal("ErrorDefinitionNotFound", data.GetProperty("failureCode").GetString());
        Assert.False(string.IsNullOrWhiteSpace(data.GetProperty("failureMessage").GetString()));
        Assert.Equal(JsonValueKind.Null, data.GetProperty("validation").ValueKind);
        Assert.Equal(backupsBefore, CountErrorBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidWorkspaceAndJson_WritesStructuredValidationFailure()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            TemporaryWhenItFailsWorkspace.CreateEmpty();
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "details",
            workspace.ProjectRootPath,
            "AFW_NET_0001",
            "--json"
        ]);

        Assert.Equal(2, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement data = document.RootElement.GetProperty("data");

        Assert.False(data.GetProperty("found").GetBoolean());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("error").ValueKind);
        Assert.Equal(JsonValueKind.Null, data.GetProperty("failureCode").ValueKind);
        Assert.Equal(JsonValueKind.Object, data.GetProperty("validation").ValueKind);
        Assert.Equal(backupsBefore, CountErrorBackups(workspace.WhenItFailsJsonsPath));
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
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);

        int exitCode = await DetailsCommand.ExecuteAsync(
        [
            "details",
            workspace.ProjectRootPath,
            "AFW_NET_0001",
            first,
            second
        ]);

        Assert.Equal(1, exitCode);
        Assert.Equal(backupsBefore, CountErrorBackups(workspace.WhenItFailsJsonsPath));
    }

    private static async Task<(int ExitCode, string Output)> ExecuteWithCapturedOutputAsync(string[] args)
    {
        TextWriter originalOutput = Console.Out;
        using StringWriter output = new();

        try
        {
            Console.SetOut(output);
            int exitCode = await DetailsCommand.ExecuteAsync(args);
            return (exitCode, output.ToString());
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }

    private static int CountErrorBackups(string jsonsPath)
    {
        return Directory.Exists(jsonsPath)
            ? Directory.GetFiles(
                jsonsPath,
                "errors.*.bak.json",
                SearchOption.TopDirectoryOnly).Length
            : 0;
    }
}
