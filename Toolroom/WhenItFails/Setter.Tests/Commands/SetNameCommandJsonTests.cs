using System.Text.Json;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

[Collection("Console output")]
public sealed class SetNameCommandJsonTests
{
    [Fact]
    public async Task ExecuteAsync_WithJsonOutput_RenamesErrorAndWritesStableEnvelope()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorDefinition target = (await LoadErrorsAsync(workspace.WhenItFailsJsonsPath)).Errors.First();
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "set-name",
            workspace.ProjectRootPath,
            target.Id,
            "JSON renamed error",
            "--json"
        ]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement root = document.RootElement;
        Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("set-name", root.GetProperty("command").GetString());

        JsonElement data = root.GetProperty("data");
        Assert.True(data.GetProperty("updated").GetBoolean());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("failureCode").ValueKind);
        Assert.Equal(JsonValueKind.Null, data.GetProperty("failureMessage").ValueKind);
        Assert.Equal(target.Id, data.GetProperty("error").GetProperty("id").GetString());
        Assert.Equal("JSON_RENAMED_ERROR", data.GetProperty("error").GetProperty("name").GetString());
        Assert.Equal(backupsBefore + 1, CountErrorBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ExecuteAsync_WithJsonOutputAndDuplicateName_WritesStructuredFailureWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorCatalogDocument catalog = await LoadErrorsAsync(workspace.WhenItFailsJsonsPath);
        ErrorDefinition target = catalog.Errors[0];
        ErrorDefinition existing = catalog.Errors[1];
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "set-name",
            workspace.ProjectRootPath,
            target.Id,
            "--json",
            existing.Name
        ]);

        Assert.Equal(2, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement data = document.RootElement.GetProperty("data");
        Assert.False(data.GetProperty("updated").GetBoolean());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("error").ValueKind);
        Assert.Equal("ErrorNameAlreadyExists", data.GetProperty("failureCode").GetString());
        Assert.False(string.IsNullOrWhiteSpace(data.GetProperty("failureMessage").GetString()));
        Assert.Equal(backupsBefore, CountErrorBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ExecuteAsync_WithDuplicateJsonSwitch_ReturnsCommandInputErrorWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorDefinition target = (await LoadErrorsAsync(workspace.WhenItFailsJsonsPath)).Errors.First();
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);

        int exitCode = await SetNameCommand.ExecuteAsync(
        [
            "set-name",
            workspace.ProjectRootPath,
            target.Id,
            "Renamed error",
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
            int exitCode = await SetNameCommand.ExecuteAsync(args);
            return (exitCode, output.ToString());
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }

    private static async Task<ErrorCatalogDocument> LoadErrorsAsync(string jsonsPath)
    {
        Response<ErrorCatalogDocument> response =
            await new JsonErrorCatalogLoader().LoadFromFileAsync(
                Path.Combine(jsonsPath, "errors.en.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        return new ErrorCatalogDocumentNormalizer().Normalize(response.Data);
    }

    private static int CountErrorBackups(string jsonsPath) => Directory.GetFiles(
        jsonsPath,
        "errors.en.*.bak.json",
        SearchOption.TopDirectoryOnly).Length;
}
