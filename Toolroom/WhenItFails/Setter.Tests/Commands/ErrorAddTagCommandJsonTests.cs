using System.Text.Json;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

[Collection("Console output")]
public sealed class ErrorAddTagCommandJsonTests
{
    private const string TestErrorId = "AFW_NET_0001";

    [Fact]
    public async Task ExecuteAsync_WithJsonOutput_AddsNormalizedTagAndWritesStableEnvelope()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "error-add-tag",
            workspace.ProjectRootPath,
            TestErrorId,
            " support visible ",
            "--json"
        ]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement root = document.RootElement;
        Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("error-add-tag", root.GetProperty("command").GetString());

        JsonElement data = root.GetProperty("data");
        Assert.True(data.GetProperty("updated").GetBoolean());
        Assert.Equal("SUPPORT_VISIBLE", data.GetProperty("addedTag").GetString());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("failureCode").ValueKind);
        Assert.Contains(
            data.GetProperty("error").GetProperty("tags").EnumerateArray(),
            tag => string.Equals(tag.GetString(), "SUPPORT_VISIBLE", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(backupsBefore + 1, CountErrorBackups(workspace.WhenItFailsJsonsPath));

        ErrorDefinition saved = await LoadErrorAsync(
            workspace.WhenItFailsJsonsPath,
            TestErrorId);
        Assert.Contains("SUPPORT_VISIBLE", saved.Tags, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_WithJsonBeforeTag_AddsTag()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "error-add-tag",
            workspace.ProjectRootPath,
            TestErrorId,
            "--json",
            "json-before-tag"
        ]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        Assert.Equal(
            "JSON_BEFORE_TAG",
            document.RootElement
                .GetProperty("data")
                .GetProperty("addedTag")
                .GetString());
    }

    [Fact]
    public async Task ExecuteAsync_WithDuplicateTagAndJson_WritesStructuredFailureWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        Assert.Equal(
            0,
            await ErrorAddTagCommand.ExecuteAsync(
            [
                "error-add-tag",
                workspace.ProjectRootPath,
                TestErrorId,
                "duplicate-json-tag"
            ]));
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "error-add-tag",
            workspace.ProjectRootPath,
            TestErrorId,
            "DUPLICATE_JSON_TAG",
            "--json"
        ]);

        Assert.Equal(2, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement data = document.RootElement.GetProperty("data");
        Assert.False(data.GetProperty("updated").GetBoolean());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("error").ValueKind);
        Assert.Equal(JsonValueKind.Null, data.GetProperty("addedTag").ValueKind);
        Assert.False(string.IsNullOrWhiteSpace(data.GetProperty("failureCode").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(data.GetProperty("failureMessage").GetString()));
        Assert.Equal(backupsBefore, CountErrorBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ExecuteAsync_WithDuplicateJsonSwitch_ReturnsInputErrorWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);

        int exitCode = await ErrorAddTagCommand.ExecuteAsync(
        [
            "error-add-tag",
            workspace.ProjectRootPath,
            TestErrorId,
            "TAG",
            "--json",
            "--json"
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
            int exitCode = await ErrorAddTagCommand.ExecuteAsync(args);
            return (exitCode, output.ToString());
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }

    private static async Task<ErrorDefinition> LoadErrorAsync(
        string jsonsPath,
        string errorId)
    {
        Response<ErrorCatalogDocument> response =
            await new JsonErrorCatalogLoader().LoadFromFileAsync(
                Path.Combine(jsonsPath, "errors.en.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);

        ErrorDefinition? error = new ErrorCatalogDocumentNormalizer()
            .Normalize(response.Data)
            .Errors
            .FirstOrDefault(candidate => string.Equals(
                candidate.Id,
                errorId,
                StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(error);
        return error;
    }

    private static int CountErrorBackups(string jsonsPath) => Directory.GetFiles(
        jsonsPath,
        "errors.en.*.bak.json",
        SearchOption.TopDirectoryOnly).Length;
}
