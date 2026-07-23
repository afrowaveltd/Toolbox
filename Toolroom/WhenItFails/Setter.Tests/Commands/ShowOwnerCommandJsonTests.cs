using System.Text.Json;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

[Collection(ConsoleOutputTestCollection.Name)]
public sealed class ShowOwnerCommandJsonTests
{
    [Fact]
    public async Task ExecuteAsync_WithOwnerNameAndJson_WritesStableEnvelopeAndCompleteOwner()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        int backupsBefore = CountOwnerBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "show-owner",
            workspace.ProjectRootPath,
            "AFW",
            "--json"
        ]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement root = document.RootElement;
        JsonElement data = root.GetProperty("data");
        JsonElement owner = data.GetProperty("owner");

        Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("show-owner", root.GetProperty("command").GetString());
        Assert.True(data.GetProperty("found").GetBoolean());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("failureCode").ValueKind);
        Assert.Equal(JsonValueKind.Null, data.GetProperty("validation").ValueKind);
        Assert.False(string.IsNullOrWhiteSpace(owner.GetProperty("name").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(owner.GetProperty("displayName").GetString()));
        Assert.True(owner.TryGetProperty("codeFrom", out _));
        Assert.True(owner.TryGetProperty("codeTo", out _));
        Assert.True(owner.TryGetProperty("isBuiltIn", out _));
        Assert.True(owner.TryGetProperty("aliases", out _));
        Assert.Equal(backupsBefore, CountOwnerBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ExecuteAsync_WithOwnerAliasAndJson_ReturnsMatchingOwner()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "show-owner",
            workspace.ProjectRootPath,
            "AFROWAVE",
            "--json"
        ]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement data = document.RootElement.GetProperty("data");

        Assert.True(data.GetProperty("found").GetBoolean());
        Assert.Equal("AFW", data.GetProperty("owner").GetProperty("name").GetString());
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownOwnerAndJson_WritesStructuredLookupFailure()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        int backupsBefore = CountOwnerBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "show-owner",
            workspace.ProjectRootPath,
            "DOES_NOT_EXIST",
            "--json"
        ]);

        Assert.Equal(2, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement root = document.RootElement;
        JsonElement data = root.GetProperty("data");

        Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("show-owner", root.GetProperty("command").GetString());
        Assert.False(data.GetProperty("found").GetBoolean());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("owner").ValueKind);
        Assert.Equal("UnknownOwner", data.GetProperty("failureCode").GetString());
        Assert.False(string.IsNullOrWhiteSpace(data.GetProperty("failureMessage").GetString()));
        Assert.Equal(JsonValueKind.Null, data.GetProperty("validation").ValueKind);
        Assert.Equal(backupsBefore, CountOwnerBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidWorkspaceAndJson_WritesStructuredValidationFailure()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            TemporaryWhenItFailsWorkspace.CreateEmpty();
        int backupsBefore = CountOwnerBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "show-owner",
            workspace.ProjectRootPath,
            "AFW",
            "--json"
        ]);

        Assert.Equal(2, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement root = document.RootElement;
        JsonElement data = root.GetProperty("data");

        Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("show-owner", root.GetProperty("command").GetString());
        Assert.False(data.GetProperty("found").GetBoolean());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("owner").ValueKind);
        Assert.Equal(JsonValueKind.Null, data.GetProperty("failureCode").ValueKind);
        Assert.Equal(JsonValueKind.Object, data.GetProperty("validation").ValueKind);
        Assert.Equal(backupsBefore, CountOwnerBackups(workspace.WhenItFailsJsonsPath));
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
        int backupsBefore = CountOwnerBackups(workspace.WhenItFailsJsonsPath);

        int exitCode = await ShowOwnerCommand.ExecuteAsync(
        [
            "show-owner",
            workspace.ProjectRootPath,
            "AFW",
            first,
            second
        ]);

        Assert.Equal(1, exitCode);
        Assert.Equal(backupsBefore, CountOwnerBackups(workspace.WhenItFailsJsonsPath));
    }

    private static async Task<(int ExitCode, string Output)> ExecuteWithCapturedOutputAsync(string[] args)
    {
        TextWriter originalOutput = Console.Out;
        using StringWriter output = new();

        try
        {
            Console.SetOut(output);
            int exitCode = await ShowOwnerCommand.ExecuteAsync(args);
            return (exitCode, output.ToString());
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }

    private static int CountOwnerBackups(string jsonsPath)
    {
        return Directory.Exists(jsonsPath)
            ? Directory.GetFiles(
                jsonsPath,
                "owners.*.bak.json",
                SearchOption.TopDirectoryOnly).Length
            : 0;
    }
}
