using System.Text.Json;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

[Collection("Console output")]
public sealed class SetCodeGroupCommandJsonTests
{
    [Fact]
    public async Task ExecuteAsync_WithJsonOutput_UpdatesGroupCodePrefixCodeAndId()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        (ErrorDefinition error, ErrorCodeGroupDefinition targetGroup, int expectedCode) =
            await FindCompatibleMoveAsync(workspace.WhenItFailsJsonsPath);
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "set-code-group",
            workspace.ProjectRootPath,
            error.Name,
            targetGroup.CodePrefix.ToLowerInvariant(),
            "--json"
        ]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement root = document.RootElement;
        Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("set-code-group", root.GetProperty("command").GetString());

        JsonElement data = root.GetProperty("data");
        Assert.True(data.GetProperty("updated").GetBoolean());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("failureCode").ValueKind);
        JsonElement changedError = data.GetProperty("error");
        Assert.Equal(targetGroup.Name, changedError.GetProperty("codeGroup").GetString());
        Assert.Equal(targetGroup.CodePrefix, changedError.GetProperty("codePrefix").GetString());
        Assert.Equal(expectedCode, changedError.GetProperty("code").GetInt32());
        string changedId = changedError.GetProperty("id").GetString()!;
        Assert.StartsWith(
            $"{TextKeyNormalizer.NormalizeKey(error.Owner)}_{targetGroup.CodePrefix}_",
            changedId,
            StringComparison.Ordinal);
        Assert.Equal(backupsBefore + 1, CountErrorBackups(workspace.WhenItFailsJsonsPath));

        ErrorDefinition saved = await LoadErrorByCodeAsync(
            workspace.WhenItFailsJsonsPath,
            expectedCode);
        Assert.Equal(changedId, saved.Id);
        Assert.Equal(targetGroup.Name, saved.CodeGroup);
        Assert.Equal(targetGroup.CodePrefix, saved.CodePrefix);
    }

    [Fact]
    public async Task ExecuteAsync_WithJsonBeforeGroup_UpdatesCodeGroup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        var move = await FindCompatibleMoveAsync(workspace.WhenItFailsJsonsPath);
        ErrorDefinition error = move.Error;
        ErrorCodeGroupDefinition targetGroup = move.TargetGroup;

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "set-code-group",
            workspace.ProjectRootPath,
            error.Id,
            "--json",
            targetGroup.Name
        ]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        Assert.Equal(
            targetGroup.Name,
            document.RootElement
                .GetProperty("data")
                .GetProperty("error")
                .GetProperty("codeGroup")
                .GetString());
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownGroupAndJson_WritesStructuredFailureWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorDefinition error = await LoadFirstErrorAsync(workspace.WhenItFailsJsonsPath);
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "set-code-group",
            workspace.ProjectRootPath,
            error.Code.ToString(),
            "DOES_NOT_EXIST",
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

        int exitCode = await SetCodeGroupCommand.ExecuteAsync(
        [
            "set-code-group",
            workspace.ProjectRootPath,
            error.Id,
            "NETWORK",
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
            int exitCode = await SetCodeGroupCommand.ExecuteAsync(args);
            return (exitCode, output.ToString());
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }

    private static async Task<(ErrorDefinition Error, ErrorCodeGroupDefinition TargetGroup, int ExpectedCode)>
        FindCompatibleMoveAsync(string jsonsPath)
    {
        ErrorCatalogDocument errors = await LoadErrorCatalogAsync(jsonsPath);
        ErrorCodeGroupCatalogDocument groups = await LoadCodeGroupCatalogAsync(jsonsPath);
        ErrorOwnerCatalogDocument owners = await LoadOwnerCatalogAsync(jsonsPath);
        HashSet<int> usedCodes = errors.Errors.Select(error => error.Code).ToHashSet();
        HashSet<string> usedIds = errors.Errors
            .Select(error => error.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (ErrorDefinition error in errors.Errors)
        {
            ErrorOwnerDefinition? owner = owners.Owners.FirstOrDefault(candidate =>
                string.Equals(candidate.Name, error.Owner, StringComparison.OrdinalIgnoreCase));
            if (owner is null)
            {
                continue;
            }

            string currentPrefix = TextKeyNormalizer.NormalizeKey(error.CodePrefix);
            string currentId = TextKeyNormalizer.NormalizeKey(error.Id);
            string expectedPrefix = $"{TextKeyNormalizer.NormalizeKey(error.Owner)}_{currentPrefix}_";
            if (!currentId.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string suffix = currentId[expectedPrefix.Length..];
            foreach (ErrorCodeGroupDefinition group in groups.CodeGroups)
            {
                if (string.Equals(group.Name, error.CodeGroup, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                int from = Math.Max(group.CodeFrom, owner.CodeFrom);
                int to = Math.Min(group.CodeTo, owner.CodeTo);
                if (from > to)
                {
                    continue;
                }

                int? freeCode = Enumerable.Range(from, to - from + 1)
                    .Cast<int?>()
                    .FirstOrDefault(candidate => candidate.HasValue && !usedCodes.Contains(candidate.Value));
                if (freeCode is null)
                {
                    continue;
                }

                string predictedId =
                    $"{TextKeyNormalizer.NormalizeKey(error.Owner)}_{group.CodePrefix}_{suffix}";
                if (!usedIds.Contains(predictedId))
                {
                    return (error, group, freeCode.Value);
                }
            }
        }

        throw new InvalidOperationException("The test workspace contains no compatible code group move.");
    }

    private static async Task<ErrorDefinition> LoadFirstErrorAsync(string jsonsPath)
    {
        ErrorCatalogDocument catalog = await LoadErrorCatalogAsync(jsonsPath);
        ErrorDefinition? error = catalog.Errors.FirstOrDefault();
        Assert.NotNull(error);
        return error;
    }

    private static async Task<ErrorDefinition> LoadErrorByCodeAsync(string jsonsPath, int code)
    {
        ErrorCatalogDocument catalog = await LoadErrorCatalogAsync(jsonsPath);
        ErrorDefinition? error = catalog.Errors.FirstOrDefault(candidate => candidate.Code == code);
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

    private static async Task<ErrorCodeGroupCatalogDocument> LoadCodeGroupCatalogAsync(string jsonsPath)
    {
        Response<ErrorCodeGroupCatalogDocument> response =
            await new JsonErrorCodeGroupCatalogLoader().LoadFromFileAsync(
                Path.Combine(jsonsPath, "code-groups.en.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        return new ErrorCodeGroupCatalogDocumentNormalizer().Normalize(response.Data);
    }

    private static async Task<ErrorOwnerCatalogDocument> LoadOwnerCatalogAsync(string jsonsPath)
    {
        Response<ErrorOwnerCatalogDocument> response =
            await new JsonErrorOwnerCatalogLoader().LoadFromFileAsync(
                Path.Combine(jsonsPath, "owners.en.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        return new ErrorOwnerCatalogDocumentNormalizer().Normalize(response.Data);
    }

    private static int CountErrorBackups(string jsonsPath) => Directory.GetFiles(
        jsonsPath,
        "errors.en.*.bak.json",
        SearchOption.TopDirectoryOnly).Length;
}
