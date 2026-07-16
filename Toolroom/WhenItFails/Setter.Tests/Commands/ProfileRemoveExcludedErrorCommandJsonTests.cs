using System.Text.Json;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

[Collection(ConsoleOutputTestCollection.Name)]
public sealed class ProfileRemoveExcludedErrorCommandJsonTests
{
    [Fact]
    public async Task ExecuteAsync_WithJsonOutput_RemovesCanonicalErrorId()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorProfileDefinition profile = await AddProfileAsync(workspace.ProjectRootPath);
        ErrorDefinition error = await LoadFirstErrorAsync(workspace.WhenItFailsJsonsPath);
        Assert.True((await new WhenItFailsProfileWorkspaceEditor().ProfileAddExcludedErrorAsync(
            workspace.ProjectRootPath, profile.Name, error.Id)).IsSuccess);
        int backupsBefore = CountBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await CaptureAsync([
            "profile-remove-excluded-error",
            workspace.ProjectRootPath,
            profile.Name,
            error.Code.ToString(),
            "--json"]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement root = document.RootElement;
        JsonElement data = root.GetProperty("data");
        Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("profile-remove-excluded-error", root.GetProperty("command").GetString());
        Assert.True(data.GetProperty("updated").GetBoolean());
        Assert.Equal(error.Id, data.GetProperty("removedExcludedError").GetString());
        Assert.DoesNotContain(error.Id, data.GetProperty("profile").GetProperty("excludeErrors")
            .EnumerateArray().Select(value => value.GetString()));
        Assert.Equal(backupsBefore + 1, CountBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ExecuteAsync_WithJsonBeforeLookup_RemovesError()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorProfileDefinition profile = await AddProfileAsync(workspace.ProjectRootPath);
        ErrorDefinition error = await LoadFirstErrorAsync(workspace.WhenItFailsJsonsPath);
        Assert.True((await new WhenItFailsProfileWorkspaceEditor().ProfileAddExcludedErrorAsync(
            workspace.ProjectRootPath, profile.Name, error.Id)).IsSuccess);

        (int exitCode, string output) = await CaptureAsync([
            "profile-remove-excluded-error",
            workspace.ProjectRootPath,
            profile.DisplayName,
            "--json",
            error.Name]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        Assert.Equal(error.Id,
            document.RootElement.GetProperty("data").GetProperty("removedExcludedError").GetString());
    }

    [Fact]
    public async Task ExecuteAsync_WithErrorNotExcluded_WritesFailureWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorProfileDefinition profile = await AddProfileAsync(workspace.ProjectRootPath);
        ErrorDefinition error = await LoadFirstErrorAsync(workspace.WhenItFailsJsonsPath);
        int backupsBefore = CountBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await CaptureAsync([
            "profile-remove-excluded-error",
            workspace.ProjectRootPath,
            profile.Name,
            error.Id,
            "--json"]);

        Assert.Equal(2, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement data = document.RootElement.GetProperty("data");
        Assert.False(data.GetProperty("updated").GetBoolean());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("profile").ValueKind);
        Assert.Equal(JsonValueKind.Null, data.GetProperty("removedExcludedError").ValueKind);
        Assert.False(string.IsNullOrWhiteSpace(data.GetProperty("failureCode").GetString()));
        Assert.Equal(backupsBefore, CountBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ExecuteAsync_WithDuplicateJsonSwitch_ReturnsInputError()
    {
        int exitCode = await ProfileRemoveExcludedErrorCommand.ExecuteAsync([
            "profile-remove-excluded-error", ".", "WEB", "AFW_NET_0001", "--json", "--json"]);
        Assert.Equal(1, exitCode);
    }

    private static async Task<(int ExitCode, string Output)> CaptureAsync(string[] args)
    {
        TextWriter original = Console.Out;
        using StringWriter writer = new();
        try
        {
            Console.SetOut(writer);
            return (await ProfileRemoveExcludedErrorCommand.ExecuteAsync(args), writer.ToString());
        }
        finally
        {
            Console.SetOut(original);
        }
    }

    private static async Task<ErrorProfileDefinition> AddProfileAsync(string path)
    {
        Response<ErrorProfileDefinition> response =
            await new WhenItFailsProfileWorkspaceEditor().AddProfileAsync(
                path, "JSON_REMOVE_EXCLUDED_ERROR_TEST", "JSON Remove Excluded Error Test");
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        return response.Data;
    }

    private static async Task<ErrorDefinition> LoadFirstErrorAsync(string path)
    {
        Response<ErrorCatalogDocument> response =
            await new JsonErrorCatalogLoader().LoadFromFileAsync(Path.Combine(path, "errors.en.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        ErrorDefinition? error = new ErrorCatalogDocumentNormalizer().Normalize(response.Data).Errors
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value.Id)
                && !string.IsNullOrWhiteSpace(value.Name) && value.Code > 0);
        Assert.NotNull(error);
        return error;
    }

    private static int CountBackups(string path) =>
        Directory.GetFiles(path, "profiles.*.bak.json", SearchOption.TopDirectoryOnly).Length;
}
