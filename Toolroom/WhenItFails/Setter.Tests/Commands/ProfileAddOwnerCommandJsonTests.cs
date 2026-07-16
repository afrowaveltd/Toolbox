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
public sealed class ProfileAddOwnerCommandJsonTests
{
    [Fact]
    public async Task ExecuteAsync_WithJsonOutput_WritesStableEnvelopeAndPersistsCanonicalOwner()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorProfileDefinition profile = await AddTestProfileAsync(workspace.ProjectRootPath);
        ErrorOwnerDefinition owner = await LoadFirstOwnerAsync(workspace.WhenItFailsJsonsPath);
        string ownerLookup = owner.Aliases.FirstOrDefault() ?? owner.Name.ToLowerInvariant();
        int backupsBefore = CountProfileBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "profile-add-owner",
            workspace.ProjectRootPath,
            profile.Name,
            ownerLookup,
            "--json"
        ]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement root = document.RootElement;
        JsonElement data = root.GetProperty("data");

        Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("profile-add-owner", root.GetProperty("command").GetString());
        Assert.True(data.GetProperty("updated").GetBoolean());
        Assert.Equal(owner.Name, data.GetProperty("addedOwner").GetString());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("failureCode").ValueKind);
        Assert.Contains(
            data.GetProperty("profile").GetProperty("includeOwners").EnumerateArray(),
            item => string.Equals(item.GetString(), owner.Name, StringComparison.OrdinalIgnoreCase));
        Assert.Equal(backupsBefore + 1, CountProfileBackups(workspace.WhenItFailsJsonsPath));

        ErrorProfileDefinition saved = await LoadProfileAsync(
            workspace.WhenItFailsJsonsPath,
            profile.Name);
        Assert.Contains(owner.Name, saved.IncludeOwners, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_WithJsonBeforeOwner_AddsOwner()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorProfileDefinition profile = await AddTestProfileAsync(workspace.ProjectRootPath);
        ErrorOwnerDefinition owner = await LoadFirstOwnerAsync(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "profile-add-owner",
            workspace.ProjectRootPath,
            profile.DisplayName,
            "--json",
            owner.Name
        ]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        Assert.Equal(
            owner.Name,
            document.RootElement.GetProperty("data").GetProperty("addedOwner").GetString());
    }

    [Fact]
    public async Task ExecuteAsync_WithDuplicateOwnerAndJson_WritesStructuredFailureWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorProfileDefinition profile = await AddTestProfileAsync(workspace.ProjectRootPath);
        ErrorOwnerDefinition owner = await LoadFirstOwnerAsync(workspace.WhenItFailsJsonsPath);
        Assert.True((await new WhenItFailsProfileWorkspaceEditor().ProfileAddOwnerAsync(
            workspace.ProjectRootPath,
            profile.Name,
            owner.Name)).IsSuccess);
        int backupsBefore = CountProfileBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "profile-add-owner",
            workspace.ProjectRootPath,
            profile.Name,
            owner.Name,
            "--json"
        ]);

        Assert.Equal(2, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement data = document.RootElement.GetProperty("data");
        Assert.False(data.GetProperty("updated").GetBoolean());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("profile").ValueKind);
        Assert.Equal(JsonValueKind.Null, data.GetProperty("addedOwner").ValueKind);
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
        ErrorOwnerDefinition owner = await LoadFirstOwnerAsync(workspace.WhenItFailsJsonsPath);
        int backupsBefore = CountProfileBackups(workspace.WhenItFailsJsonsPath);

        int exitCode = await ProfileAddOwnerCommand.ExecuteAsync(
        [
            "profile-add-owner",
            workspace.ProjectRootPath,
            profile.Name,
            owner.Name,
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
            int exitCode = await ProfileAddOwnerCommand.ExecuteAsync(args);
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
                "JSON_OWNER_TEST",
                "JSON Owner Test");
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        return response.Data;
    }

    private static async Task<ErrorOwnerDefinition> LoadFirstOwnerAsync(string jsonsPath)
    {
        Response<ErrorOwnerCatalogDocument> response =
            await new JsonErrorOwnerCatalogLoader().LoadFromFileAsync(
                Path.Combine(jsonsPath, "owners.en.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);

        ErrorOwnerDefinition? owner = new ErrorOwnerCatalogDocumentNormalizer()
            .Normalize(response.Data)
            .Owners
            .FirstOrDefault();
        Assert.NotNull(owner);
        return owner;
    }

    private static async Task<ErrorProfileDefinition> LoadProfileAsync(
        string jsonsPath,
        string profileName)
    {
        Response<ErrorProfileCatalogDocument> response =
            await new JsonErrorProfileCatalogLoader().LoadFromFileAsync(
                Path.Combine(jsonsPath, "profiles.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);

        ErrorProfileDefinition? profile = new ErrorProfileCatalogDocumentNormalizer()
            .Normalize(response.Data)
            .Profiles
            .FirstOrDefault(candidate => string.Equals(
                candidate.Name,
                profileName,
                StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(profile);
        return profile;
    }

    private static int CountProfileBackups(string jsonsPath) => Directory.GetFiles(
        jsonsPath,
        "profiles.*.bak.json",
        SearchOption.TopDirectoryOnly).Length;
}
