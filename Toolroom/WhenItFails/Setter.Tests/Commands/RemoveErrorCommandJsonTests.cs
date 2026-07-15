using System.Text.Json;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

[Collection("Console output")]
public sealed class RemoveErrorCommandJsonTests
{
    [Fact]
    public async Task ExecuteAsync_WithJsonOutput_RemovesErrorAndWritesStableEnvelope()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorDefinition added = await AddTemporaryErrorAsync(workspace, "JSON removable error");
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);
        StringWriter writer = new();
        TextWriter originalOutput = Console.Out;

        try
        {
            Console.SetOut(writer);
            int exitCode = await RemoveErrorCommand.ExecuteAsync(
            [
                "remove-error",
                workspace.ProjectRootPath,
                added.Id,
                "--json"
            ]);

            Assert.Equal(0, exitCode);
        }
        finally
        {
            Console.SetOut(originalOutput);
        }

        using JsonDocument document = JsonDocument.Parse(writer.ToString());
        JsonElement root = document.RootElement;
        Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("remove-error", root.GetProperty("command").GetString());
        JsonElement data = root.GetProperty("data");
        Assert.True(data.GetProperty("removed").GetBoolean());
        Assert.Equal(added.Id, data.GetProperty("error").GetProperty("id").GetString());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("failureCode").ValueKind);
        Assert.Equal(0, data.GetProperty("references").GetArrayLength());
        Assert.Equal(backupsBefore + 1, CountErrorBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ExecuteAsync_WithReferencedErrorAndJsonOutput_WritesBlockingReferences()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorDefinition error = (await LoadErrorsAsync(workspace.WhenItFailsJsonsPath)).Errors.First();
        ErrorProfileDefinition profile = (await LoadProfilesAsync(workspace.WhenItFailsJsonsPath)).Profiles.First();
        Response<ErrorProfileDefinition> addReferenceResponse =
            await new WhenItFailsProfileWorkspaceEditor().ProfileAddErrorAsync(
                workspace.ProjectRootPath,
                profile.Name,
                error.Id);
        Assert.True(addReferenceResponse.IsSuccess);
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);
        StringWriter writer = new();
        TextWriter originalOutput = Console.Out;

        try
        {
            Console.SetOut(writer);
            int exitCode = await RemoveErrorCommand.ExecuteAsync(
            [
                "remove-error",
                workspace.ProjectRootPath,
                error.Id,
                "--json"
            ]);

            Assert.Equal(2, exitCode);
        }
        finally
        {
            Console.SetOut(originalOutput);
        }

        using JsonDocument document = JsonDocument.Parse(writer.ToString());
        JsonElement data = document.RootElement.GetProperty("data");
        Assert.False(data.GetProperty("removed").GetBoolean());
        Assert.Equal("ErrorIsReferencedByProfiles", data.GetProperty("failureCode").GetString());
        JsonElement references = data.GetProperty("references");
        Assert.Equal(1, references.GetArrayLength());
        Assert.Equal(profile.Name, references[0].GetProperty("profileName").GetString());
        Assert.Equal("Include", references[0].GetProperty("referenceKind").GetString());
        Assert.Equal(backupsBefore, CountErrorBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownErrorAndJsonOutput_WritesStructuredFailure()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        StringWriter writer = new();
        TextWriter originalOutput = Console.Out;

        try
        {
            Console.SetOut(writer);
            int exitCode = await RemoveErrorCommand.ExecuteAsync(
            [
                "remove-error",
                workspace.ProjectRootPath,
                "DOES_NOT_EXIST",
                "--json"
            ]);

            Assert.Equal(2, exitCode);
        }
        finally
        {
            Console.SetOut(originalOutput);
        }

        using JsonDocument document = JsonDocument.Parse(writer.ToString());
        JsonElement data = document.RootElement.GetProperty("data");
        Assert.False(data.GetProperty("removed").GetBoolean());
        Assert.Equal("ErrorDefinitionNotFound", data.GetProperty("failureCode").GetString());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("error").ValueKind);
        Assert.Equal(0, data.GetProperty("references").GetArrayLength());
    }

    private static async Task<ErrorDefinition> AddTemporaryErrorAsync(
        TemporaryWhenItFailsWorkspace workspace,
        string name)
    {
        (ErrorOwnerDefinition owner, ErrorCodeGroupDefinition group) =
            await FindCompatibleIdentityAsync(workspace.ProjectRootPath, workspace.WhenItFailsJsonsPath);
        ErrorCategoryDefinition category = await LoadFirstCategoryAsync(workspace.WhenItFailsJsonsPath);
        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().AddErrorAsync(
                workspace.ProjectRootPath,
                new AddErrorRequest(
                    owner.Name,
                    group.Name,
                    category.Name,
                    name,
                    name,
                    "A temporary JSON test error occurred."));

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        return response.Data;
    }

    private static async Task<(ErrorOwnerDefinition Owner, ErrorCodeGroupDefinition Group)>
        FindCompatibleIdentityAsync(string projectRootPath, string jsonsPath)
    {
        ErrorOwnerCatalogDocument owners = await LoadOwnersAsync(jsonsPath);
        ErrorCodeGroupCatalogDocument groups = await LoadGroupsAsync(jsonsPath);
        WhenItFailsNextCodeFinder finder = new();

        foreach (ErrorOwnerDefinition owner in owners.Owners)
        {
            foreach (ErrorCodeGroupDefinition group in groups.CodeGroups)
            {
                Response<NextCodeSuggestion> response = await finder.FindAsync(
                    projectRootPath,
                    owner.Name,
                    group.Name);
                if (response.IsSuccess)
                {
                    return (owner, group);
                }
            }
        }

        throw new InvalidOperationException("The test workspace contains no compatible owner and code-group pair.");
    }

    private static async Task<ErrorCatalogDocument> LoadErrorsAsync(string jsonsPath)
    {
        Response<ErrorCatalogDocument> response =
            await new JsonErrorCatalogLoader().LoadFromFileAsync(Path.Combine(jsonsPath, "errors.en.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        return new ErrorCatalogDocumentNormalizer().Normalize(response.Data);
    }

    private static async Task<ErrorProfileCatalogDocument> LoadProfilesAsync(string jsonsPath)
    {
        Response<ErrorProfileCatalogDocument> response =
            await new JsonErrorProfileCatalogLoader().LoadFromFileAsync(Path.Combine(jsonsPath, "profiles.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        return new ErrorProfileCatalogDocumentNormalizer().Normalize(response.Data);
    }

    private static async Task<ErrorOwnerCatalogDocument> LoadOwnersAsync(string jsonsPath)
    {
        Response<ErrorOwnerCatalogDocument> response =
            await new JsonErrorOwnerCatalogLoader().LoadFromFileAsync(Path.Combine(jsonsPath, "owners.en.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        return new ErrorOwnerCatalogDocumentNormalizer().Normalize(response.Data);
    }

    private static async Task<ErrorCodeGroupCatalogDocument> LoadGroupsAsync(string jsonsPath)
    {
        Response<ErrorCodeGroupCatalogDocument> response =
            await new JsonErrorCodeGroupCatalogLoader().LoadFromFileAsync(Path.Combine(jsonsPath, "code-groups.en.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        return new ErrorCodeGroupCatalogDocumentNormalizer().Normalize(response.Data);
    }

    private static async Task<ErrorCategoryDefinition> LoadFirstCategoryAsync(string jsonsPath)
    {
        Response<ErrorCategoryCatalogDocument> response =
            await new JsonErrorCategoryCatalogLoader().LoadFromFileAsync(Path.Combine(jsonsPath, "categories.en.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        ErrorCategoryCatalogDocument catalog =
            new ErrorCategoryCatalogDocumentNormalizer().Normalize(response.Data);
        ErrorCategoryDefinition? category = catalog.Categories.FirstOrDefault();
        Assert.NotNull(category);
        return category;
    }

    private static int CountErrorBackups(string jsonsPath) => Directory.GetFiles(
        jsonsPath,
        "errors.en.*.bak.json",
        SearchOption.TopDirectoryOnly).Length;
}
