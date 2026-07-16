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
public sealed class ProfileRemoveSubcategoryCommandJsonTests
{
    [Fact]
    public async Task ExecuteAsync_WithJsonOutput_WritesStableEnvelopeAndRemovesCanonicalSubcategory()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorProfileDefinition profile = await AddTestProfileAsync(workspace.ProjectRootPath);
        string subcategory = await LoadFirstSubcategoryAsync(workspace.WhenItFailsJsonsPath);
        string canonical = TextKeyNormalizer.NormalizeKey(subcategory);
        Assert.True((await new WhenItFailsProfileWorkspaceEditor().ProfileAddSubcategoryAsync(
            workspace.ProjectRootPath,
            profile.Name,
            subcategory)).IsSuccess);
        int backupsBefore = CountProfileBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "profile-remove-subcategory",
            workspace.ProjectRootPath,
            profile.Name,
            subcategory.ToLowerInvariant(),
            "--json"
        ]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement root = document.RootElement;
        JsonElement data = root.GetProperty("data");
        Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("profile-remove-subcategory", root.GetProperty("command").GetString());
        Assert.True(data.GetProperty("updated").GetBoolean());
        Assert.Equal(canonical, data.GetProperty("removedSubcategory").GetString());
        Assert.DoesNotContain(
            data.GetProperty("profile").GetProperty("includeSubcategories").EnumerateArray(),
            item => string.Equals(item.GetString(), canonical, StringComparison.OrdinalIgnoreCase));
        Assert.Equal(backupsBefore + 1, CountProfileBackups(workspace.WhenItFailsJsonsPath));

        ErrorProfileDefinition saved = await LoadProfileAsync(workspace.WhenItFailsJsonsPath, profile.Name);
        Assert.DoesNotContain(canonical, saved.IncludeSubcategories, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_WithJsonBeforeSubcategory_RemovesSubcategory()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorProfileDefinition profile = await AddTestProfileAsync(workspace.ProjectRootPath);
        string subcategory = await LoadFirstSubcategoryAsync(workspace.WhenItFailsJsonsPath);
        Assert.True((await new WhenItFailsProfileWorkspaceEditor().ProfileAddSubcategoryAsync(
            workspace.ProjectRootPath,
            profile.Name,
            subcategory)).IsSuccess);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "profile-remove-subcategory",
            workspace.ProjectRootPath,
            profile.DisplayName,
            "--json",
            subcategory
        ]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        Assert.Equal(
            TextKeyNormalizer.NormalizeKey(subcategory),
            document.RootElement.GetProperty("data").GetProperty("removedSubcategory").GetString());
    }

    [Fact]
    public async Task ExecuteAsync_WithSubcategoryNotIncludedAndJson_WritesStructuredFailureWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorProfileDefinition profile = await AddTestProfileAsync(workspace.ProjectRootPath);
        string subcategory = await LoadFirstSubcategoryAsync(workspace.WhenItFailsJsonsPath);
        int backupsBefore = CountProfileBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "profile-remove-subcategory",
            workspace.ProjectRootPath,
            profile.Name,
            subcategory,
            "--json"
        ]);

        Assert.Equal(2, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement data = document.RootElement.GetProperty("data");
        Assert.False(data.GetProperty("updated").GetBoolean());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("profile").ValueKind);
        Assert.Equal(JsonValueKind.Null, data.GetProperty("removedSubcategory").ValueKind);
        Assert.False(string.IsNullOrWhiteSpace(data.GetProperty("failureCode").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(data.GetProperty("failureMessage").GetString()));
        Assert.Equal(backupsBefore, CountProfileBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ExecuteAsync_WithDuplicateJsonSwitch_ReturnsInputErrorWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorProfileDefinition profile = await AddTestProfileAsync(workspace.ProjectRootPath);
        string subcategory = await LoadFirstSubcategoryAsync(workspace.WhenItFailsJsonsPath);
        int backupsBefore = CountProfileBackups(workspace.WhenItFailsJsonsPath);

        int exitCode = await ProfileRemoveSubcategoryCommand.ExecuteAsync(
        [
            "profile-remove-subcategory",
            workspace.ProjectRootPath,
            profile.Name,
            subcategory,
            "--json",
            "--json"
        ]);

        Assert.Equal(1, exitCode);
        Assert.Equal(backupsBefore, CountProfileBackups(workspace.WhenItFailsJsonsPath));
    }

    private static async Task<(int ExitCode, string Output)> ExecuteWithCapturedOutputAsync(string[] args)
    {
        TextWriter originalOutput = Console.Out;
        using StringWriter output = new();

        try
        {
            Console.SetOut(output);
            int exitCode = await ProfileRemoveSubcategoryCommand.ExecuteAsync(args);
            return (exitCode, output.ToString());
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }

    private static async Task<ErrorProfileDefinition> AddTestProfileAsync(string projectRootPath)
    {
        Response<ErrorProfileDefinition> response =
            await new WhenItFailsProfileWorkspaceEditor().AddProfileAsync(
                projectRootPath,
                "JSON_REMOVE_SUBCATEGORY_TEST",
                "JSON Remove Subcategory Test");
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        return response.Data;
    }

    private static async Task<string> LoadFirstSubcategoryAsync(string jsonsPath)
    {
        Response<ErrorCatalogDocument> response =
            await new JsonErrorCatalogLoader().LoadFromFileAsync(Path.Combine(jsonsPath, "errors.en.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);

        string? subcategory = new ErrorCatalogDocumentNormalizer()
            .Normalize(response.Data)
            .Errors
            .SelectMany(error => error.Subcategories)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
        Assert.False(string.IsNullOrWhiteSpace(subcategory));
        return subcategory;
    }

    private static async Task<ErrorProfileDefinition> LoadProfileAsync(string jsonsPath, string profileName)
    {
        Response<ErrorProfileCatalogDocument> response =
            await new JsonErrorProfileCatalogLoader().LoadFromFileAsync(Path.Combine(jsonsPath, "profiles.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);

        ErrorProfileDefinition? profile = new ErrorProfileCatalogDocumentNormalizer()
            .Normalize(response.Data)
            .Profiles
            .FirstOrDefault(candidate => string.Equals(candidate.Name, profileName, StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(profile);
        return profile;
    }

    private static int CountProfileBackups(string jsonsPath) => Directory.GetFiles(
        jsonsPath,
        "profiles.*.bak.json",
        SearchOption.TopDirectoryOnly).Length;
}
