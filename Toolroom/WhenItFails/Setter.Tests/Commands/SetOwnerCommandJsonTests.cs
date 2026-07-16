using System.Text.Json;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

[Collection("Console output")]
public sealed class SetOwnerCommandJsonTests
{
    [Fact]
    public async Task ExecuteAsync_WithCompatibleOwnerAndJson_UpdatesOwnerAndStructuredId()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorDefinition error = await LoadFirstErrorAsync(workspace.WhenItFailsJsonsPath);
        ErrorOwnerDefinition owner = await AddOwnerAsync(
            workspace.WhenItFailsJsonsPath,
            "JSON_OWNER",
            error.Code,
            error.Code,
            "JSONOWNER");
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "set-owner",
            workspace.ProjectRootPath,
            error.Name,
            owner.Aliases[0].ToLowerInvariant(),
            "--json"
        ]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement root = document.RootElement;
        Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("set-owner", root.GetProperty("command").GetString());

        JsonElement data = root.GetProperty("data");
        Assert.True(data.GetProperty("updated").GetBoolean());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("failureCode").ValueKind);
        Assert.Equal(JsonValueKind.Null, data.GetProperty("failureMessage").ValueKind);

        string expectedId = ReplaceOwnerPrefix(error.Id, error.Owner, owner.Name);
        JsonElement updatedError = data.GetProperty("error");
        Assert.Equal(owner.Name, updatedError.GetProperty("owner").GetString());
        Assert.Equal(expectedId, updatedError.GetProperty("id").GetString());
        Assert.Equal(error.Code, updatedError.GetProperty("code").GetInt32());
        Assert.Equal(backupsBefore + 1, CountErrorBackups(workspace.WhenItFailsJsonsPath));

        ErrorDefinition saved = await LoadErrorAsync(workspace.WhenItFailsJsonsPath, expectedId);
        Assert.Equal(owner.Name, saved.Owner);
        Assert.Equal(expectedId, saved.Id);
    }

    [Fact]
    public async Task ExecuteAsync_WithJsonBeforeOwner_UpdatesOwner()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorDefinition error = await LoadFirstErrorAsync(workspace.WhenItFailsJsonsPath);
        ErrorOwnerDefinition owner = await AddOwnerAsync(
            workspace.WhenItFailsJsonsPath,
            "ORDER_OWNER",
            error.Code,
            error.Code,
            "ORDEROWNER");

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "set-owner",
            workspace.ProjectRootPath,
            error.Id,
            "--json",
            owner.Name
        ]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        Assert.Equal(
            owner.Name,
            document.RootElement
                .GetProperty("data")
                .GetProperty("error")
                .GetProperty("owner")
                .GetString());
    }

    [Fact]
    public async Task ExecuteAsync_WithOwnerOutsideCodeRangeAndJson_WritesStructuredFailureWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorDefinition error = await LoadFirstErrorAsync(workspace.WhenItFailsJsonsPath);
        ErrorOwnerDefinition owner = await AddOwnerAsync(
            workspace.WhenItFailsJsonsPath,
            "OUTSIDE_OWNER",
            error.Code + 1,
            error.Code + 100,
            "OUTSIDE");
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "set-owner",
            workspace.ProjectRootPath,
            error.Id,
            owner.Name,
            "--json"
        ]);

        Assert.Equal(2, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement data = document.RootElement.GetProperty("data");
        Assert.False(data.GetProperty("updated").GetBoolean());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("error").ValueKind);
        Assert.False(string.IsNullOrWhiteSpace(data.GetProperty("failureCode").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(data.GetProperty("failureMessage").GetString()));
        Assert.Equal(backupsBefore, CountErrorBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ExecuteAsync_WithDuplicateJsonSwitch_ReturnsInputErrorWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorDefinition error = await LoadFirstErrorAsync(workspace.WhenItFailsJsonsPath);
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);

        int exitCode = await SetOwnerCommand.ExecuteAsync(
        [
            "set-owner",
            workspace.ProjectRootPath,
            error.Id,
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
            int exitCode = await SetOwnerCommand.ExecuteAsync(args);
            return (exitCode, output.ToString());
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }

    private static string ReplaceOwnerPrefix(
        string errorId,
        string currentOwner,
        string newOwner)
    {
        string normalizedId = TextKeyNormalizer.NormalizeKey(errorId);
        string currentPrefix = $"{TextKeyNormalizer.NormalizeKey(currentOwner)}_";
        Assert.StartsWith(currentPrefix, normalizedId, StringComparison.Ordinal);
        return $"{newOwner}_{normalizedId[currentPrefix.Length..]}";
    }

    private static async Task<ErrorOwnerDefinition> AddOwnerAsync(
        string jsonsPath,
        string ownerName,
        int codeFrom,
        int codeTo,
        string alias)
    {
        string filePath = Path.Combine(jsonsPath, "owners.en.json");
        Response<ErrorOwnerCatalogDocument> loadResponse =
            await new JsonErrorOwnerCatalogLoader().LoadFromFileAsync(filePath);
        Assert.True(loadResponse.IsSuccess);
        Assert.NotNull(loadResponse.Data);

        ErrorOwnerCatalogDocument catalog =
            new ErrorOwnerCatalogDocumentNormalizer().Normalize(loadResponse.Data);
        ErrorOwnerDefinition owner = new()
        {
            Name = ownerName,
            DisplayName = "JSON test owner",
            CodeFrom = codeFrom,
            CodeTo = codeTo,
            IsBuiltIn = false,
            Aliases = [alias]
        };
        catalog.Owners.Add(owner);

        Response saveResponse =
            await new JsonCatalogDocumentWriter().SaveToFileAsync(catalog, filePath);
        Assert.True(saveResponse.IsSuccess);
        return owner;
    }

    private static async Task<ErrorDefinition> LoadFirstErrorAsync(string jsonsPath)
    {
        ErrorCatalogDocument catalog = await LoadErrorCatalogAsync(jsonsPath);
        ErrorDefinition? error = catalog.Errors.FirstOrDefault();
        Assert.NotNull(error);
        return error;
    }

    private static async Task<ErrorDefinition> LoadErrorAsync(
        string jsonsPath,
        string errorId)
    {
        ErrorCatalogDocument catalog = await LoadErrorCatalogAsync(jsonsPath);
        ErrorDefinition? error = catalog.Errors.FirstOrDefault(candidate =>
            string.Equals(candidate.Id, errorId, StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(error);
        return error;
    }

    private static async Task<ErrorCatalogDocument> LoadErrorCatalogAsync(string jsonsPath)
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
