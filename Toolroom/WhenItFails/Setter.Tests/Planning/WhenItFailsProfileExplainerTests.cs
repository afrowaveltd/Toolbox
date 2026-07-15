using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Resolution;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Planning;

public sealed class WhenItFailsProfileExplainerTests
{
    [Fact]
    public async Task ExplainAsync_ShouldMatchResolverForEveryErrorWithoutChangingCatalogs()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorCatalogDocument errors = await LoadErrorsAsync(workspace.WhenItFailsJsonsPath);
        ErrorProfileCatalogDocument profiles = await LoadProfilesAsync(workspace.WhenItFailsJsonsPath);
        ErrorProfileDefinition profile = profiles.Profiles.First();
        Dictionary<string, string> before = await ReadCatalogFilesAsync(workspace.WhenItFailsJsonsPath);
        int backupsBefore = CountBackups(workspace.WhenItFailsJsonsPath);

        Response<ProfileExplanation> response =
            await new WhenItFailsProfileExplainer().ExplainAsync(
                workspace.ProjectRootPath,
                profile.DisplayName);

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        HashSet<string> expectedIncludedIds = new ErrorProfileResolver()
            .Resolve(errors, profile)
            .Select(error => error.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.Equal(errors.Errors.Count, response.Data.TotalErrors);
        Assert.Equal(expectedIncludedIds.Count, response.Data.IncludedErrors);
        Assert.Equal(errors.Errors.Count - expectedIncludedIds.Count, response.Data.ExcludedErrors);
        Assert.Equal(errors.Errors.Count, response.Data.Errors.Count);

        foreach (ProfileErrorExplanation explanation in response.Data.Errors)
        {
            Assert.Equal(
                expectedIncludedIds.Contains(explanation.Id),
                explanation.IsIncluded);
            Assert.NotEmpty(explanation.IncludeReasons);

            if (!explanation.IsIncluded
                && !explanation.IncludeReasons.Contains("none:no-include-filter-matched"))
            {
                Assert.NotEmpty(explanation.ExclusionReasons);
            }
        }

        Assert.Equal(backupsBefore, CountBackups(workspace.WhenItFailsJsonsPath));
        Dictionary<string, string> after = await ReadCatalogFilesAsync(workspace.WhenItFailsJsonsPath);
        Assert.Equal(before, after);
    }

    [Fact]
    public async Task ExplainAsync_WithExplicitExclusion_ShouldReportFinalVeto()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorCatalogDocument errors = await LoadErrorsAsync(workspace.WhenItFailsJsonsPath);
        ErrorProfileCatalogDocument profiles = await LoadProfilesAsync(workspace.WhenItFailsJsonsPath);
        ErrorDefinition error = errors.Errors.First();
        ErrorProfileDefinition profile = profiles.Profiles.First();

        profile.IncludeErrors.Clear();
        profile.IncludeErrors.Add(error.Id);
        profile.ExcludeErrors.Clear();
        profile.ExcludeErrors.Add(error.Id);
        profile.IncludeOwners.Clear();
        profile.IncludeCodeGroups.Clear();
        profile.IncludeCategories.Clear();
        profile.IncludeSubcategories.Clear();
        profile.IncludeTags.Clear();
        profile.ExcludeTags.Clear();
        await SaveProfilesAsync(workspace.WhenItFailsJsonsPath, profiles);

        Response<ProfileExplanation> response =
            await new WhenItFailsProfileExplainer().ExplainAsync(
                workspace.WhenItFailsJsonsPath,
                profile.Name);

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        ProfileErrorExplanation explanation = response.Data.Errors.Single(item =>
            string.Equals(item.Id, error.Id, StringComparison.OrdinalIgnoreCase));
        Assert.False(explanation.IsIncluded);
        Assert.Contains($"explicit-error:{error.Id}", explanation.IncludeReasons);
        Assert.Contains($"explicit-error:{error.Id}", explanation.ExclusionReasons);
    }

    [Fact]
    public async Task ExplainAsync_WithUnknownProfile_ShouldReturnNotFound()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        Response<ProfileExplanation> response =
            await new WhenItFailsProfileExplainer().ExplainAsync(
                workspace.ProjectRootPath,
                "DOES_NOT_EXIST");

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == "ProfileNotFound");
    }

    [Theory]
    [InlineData("", "WEB", "ProfileExplanationPathIsEmpty")]
    [InlineData(".", "", "ProfileNameIsEmpty")]
    public async Task ExplainAsync_WithEmptyArguments_ShouldReturnValidationFailure(
        string path,
        string profileName,
        string expectedCode)
    {
        Response<ProfileExplanation> response =
            await new WhenItFailsProfileExplainer().ExplainAsync(path, profileName);

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == expectedCode);
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
        return new ErrorProfileCatalogDocumentNormalizer().Normalize(response.Data);
    }

    private static async Task SaveProfilesAsync(
        string jsonsPath,
        ErrorProfileCatalogDocument profiles)
    {
        Response response = await new JsonCatalogDocumentWriter().SaveToFileAsync(
            profiles,
            Path.Combine(jsonsPath, "profiles.json"));
        Assert.True(response.IsSuccess);
    }

    private static async Task<Dictionary<string, string>> ReadCatalogFilesAsync(string jsonsPath)
    {
        string[] names =
        [
            "errors.en.json",
            "categories.en.json",
            "code-groups.en.json",
            "owners.en.json",
            "profiles.json"
        ];

        Dictionary<string, string> contents = new(StringComparer.OrdinalIgnoreCase);
        foreach (string name in names)
        {
            contents[name] = await File.ReadAllTextAsync(Path.Combine(jsonsPath, name));
        }

        return contents;
    }

    private static int CountBackups(string jsonsPath) => Directory.GetFiles(
        jsonsPath,
        "*.bak.json",
        SearchOption.TopDirectoryOnly).Length;
}