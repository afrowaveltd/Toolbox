using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Planning;

public sealed class WhenItFailsErrorReferenceFinderTests
{
    [Fact]
    public async Task FindAsync_ShouldReportExplicitIncludeAndExcludeReferencesWithoutChangingWorkspace()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorCatalogDocument errors = await LoadErrorsAsync(workspace.WhenItFailsJsonsPath);
        ErrorProfileCatalogDocument profiles = await LoadProfilesAsync(workspace.WhenItFailsJsonsPath);
        ErrorDefinition error = errors.Errors.First();
        ErrorProfileDefinition includeProfile = profiles.Profiles[0];
        ErrorProfileDefinition excludeProfile = profiles.Profiles[1];
        WhenItFailsProfileWorkspaceEditor profileEditor = new();

        Response<ErrorProfileDefinition> includeResponse =
            await profileEditor.ProfileAddErrorAsync(
                workspace.ProjectRootPath,
                includeProfile.Name,
                error.Id);
        Response<ErrorProfileDefinition> excludeResponse =
            await profileEditor.ProfileAddExcludedErrorAsync(
                workspace.ProjectRootPath,
                excludeProfile.Name,
                error.Id);
        Assert.True(includeResponse.IsSuccess);
        Assert.True(excludeResponse.IsSuccess);

        Dictionary<string, string> before = await ReadAllFilesAsync(workspace.WhenItFailsJsonsPath);

        Response<ErrorReferenceReport> response =
            await new WhenItFailsErrorReferenceFinder().FindAsync(
                workspace.ProjectRootPath,
                error.Code.ToString());

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.Equal(error.Id, response.Data.ErrorId);
        Assert.Equal(error.Code, response.Data.ErrorCode);
        Assert.Equal(error.Name, response.Data.ErrorName);
        Assert.Equal(1, response.Data.IncludedByProfiles);
        Assert.Equal(1, response.Data.ExcludedByProfiles);
        Assert.Collection(
            response.Data.References,
            reference =>
            {
                Assert.Equal(includeProfile.Name, reference.ProfileName);
                Assert.Equal("Include", reference.ReferenceKind);
            },
            reference =>
            {
                Assert.Equal(excludeProfile.Name, reference.ProfileName);
                Assert.Equal("Exclude", reference.ReferenceKind);
            });

        Dictionary<string, string> after = await ReadAllFilesAsync(workspace.WhenItFailsJsonsPath);
        Assert.Equal(before, after);
    }

    [Fact]
    public async Task FindAsync_WithUnreferencedError_ShouldReturnEmptyReferenceList()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorDefinition error = (await LoadErrorsAsync(workspace.WhenItFailsJsonsPath)).Errors.First();

        Response<ErrorReferenceReport> response =
            await new WhenItFailsErrorReferenceFinder().FindAsync(
                workspace.ProjectRootPath,
                error.Name);

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.Empty(response.Data.References);
        Assert.Equal(0, response.Data.IncludedByProfiles);
        Assert.Equal(0, response.Data.ExcludedByProfiles);
    }

    [Fact]
    public async Task FindAsync_WithUnknownError_ShouldReturnNotFound()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        Response<ErrorReferenceReport> response =
            await new WhenItFailsErrorReferenceFinder().FindAsync(
                workspace.ProjectRootPath,
                "DOES_NOT_EXIST");

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == "ErrorDefinitionNotFound");
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

    private static async Task<ErrorProfileCatalogDocument> LoadProfilesAsync(string jsonsPath)
    {
        Response<ErrorProfileCatalogDocument> response =
            await new JsonErrorProfileCatalogLoader().LoadFromFileAsync(
                Path.Combine(jsonsPath, "profiles.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        ErrorProfileCatalogDocument catalog =
            new ErrorProfileCatalogDocumentNormalizer().Normalize(response.Data);
        Assert.True(catalog.Profiles.Count >= 2);
        return catalog;
    }

    private static async Task<Dictionary<string, string>> ReadAllFilesAsync(string directoryPath)
    {
        Dictionary<string, string> contents = new(StringComparer.OrdinalIgnoreCase);
        foreach (string filePath in Directory.EnumerateFiles(
                     directoryPath,
                     "*",
                     SearchOption.TopDirectoryOnly))
        {
            contents[Path.GetFileName(filePath)] = await File.ReadAllTextAsync(filePath);
        }

        return contents;
    }
}
