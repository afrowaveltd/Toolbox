using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

public sealed class ProfileAddCategoryCommandTests
{
    [Fact]
    public async Task ExecuteAsync_WithValidArguments_AddsCategoryAndReturnsSuccess()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        WhenItFailsProfileWorkspaceEditor editor = new();
        Response<ErrorProfileDefinition> addProfileResponse =
            await editor.AddProfileAsync(
                temporaryWorkspace.ProjectRootPath,
                "DITA_TEST",
                "DiTa Test");

        Assert.True(addProfileResponse.IsSuccess);

        ErrorCategoryDefinition category =
            await LoadFirstCategoryAsync(temporaryWorkspace.WhenItFailsJsonsPath);

        int exitCode = await ProfileAddCategoryCommand.ExecuteAsync(
            [
                "profile-add-category",
                temporaryWorkspace.ProjectRootPath,
                "dita test",
                category.Name.ToLowerInvariant()
            ]);

        Assert.Equal(0, exitCode);

        ErrorProfileDefinition savedProfile = await LoadProfileAsync(
            temporaryWorkspace.WhenItFailsJsonsPath,
            "DITA_TEST");

        Assert.Contains(
            savedProfile.IncludeCategories,
            includedCategory => string.Equals(
                includedCategory,
                category.Name,
                StringComparison.Ordinal));
    }

    [Fact]
    public async Task ExecuteAsync_WithCategoryAlreadyIncluded_ReturnsEditFailure()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        WhenItFailsProfileWorkspaceEditor editor = new();
        Response<ErrorProfileDefinition> addProfileResponse =
            await editor.AddProfileAsync(
                temporaryWorkspace.ProjectRootPath,
                "DITA_TEST",
                "DiTa Test");

        Assert.True(addProfileResponse.IsSuccess);

        ErrorCategoryDefinition category =
            await LoadFirstCategoryAsync(temporaryWorkspace.WhenItFailsJsonsPath);

        Response<ErrorProfileDefinition> addCategoryResponse =
            await editor.ProfileAddCategoryAsync(
                temporaryWorkspace.ProjectRootPath,
                "DITA_TEST",
                category.Name);

        Assert.True(addCategoryResponse.IsSuccess);

        int exitCode = await ProfileAddCategoryCommand.ExecuteAsync(
            [
                "profile-add-category",
                temporaryWorkspace.ProjectRootPath,
                "DITA_TEST",
                category.Name
            ]);

        Assert.Equal(2, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownCategory_ReturnsEditFailure()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        WhenItFailsProfileWorkspaceEditor editor = new();
        Response<ErrorProfileDefinition> addProfileResponse =
            await editor.AddProfileAsync(
                temporaryWorkspace.ProjectRootPath,
                "DITA_TEST",
                "DiTa Test");

        Assert.True(addProfileResponse.IsSuccess);

        int exitCode = await ProfileAddCategoryCommand.ExecuteAsync(
            [
                "profile-add-category",
                temporaryWorkspace.ProjectRootPath,
                "DITA_TEST",
                "DOES_NOT_EXIST"
            ]);

        Assert.Equal(2, exitCode);
    }

    public static TheoryData<string[]> MissingArgumentCases =>
        new()
        {
            new[] { "profile-add-category" },
            new[] { "profile-add-category", "." },
            new[] { "profile-add-category", ".", "DITA_TEST" }
        };

    [Theory]
    [MemberData(nameof(MissingArgumentCases))]
    public async Task ExecuteAsync_WithMissingRequiredArgument_ReturnsCommandInputError(
        string[] args)
    {
        int exitCode = await ProfileAddCategoryCommand.ExecuteAsync(args);

        Assert.Equal(1, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithTooManyArguments_ReturnsCommandInputError()
    {
        int exitCode = await ProfileAddCategoryCommand.ExecuteAsync(
            [
                "profile-add-category",
                ".",
                "DITA_TEST",
                "GENERAL",
                "Unexpected"
            ]);

        Assert.Equal(1, exitCode);
    }

    private static async Task<ErrorCategoryDefinition> LoadFirstCategoryAsync(
        string whenItFailsJsonsPath)
    {
        string categoryCatalogFilePath = Path.Combine(
            whenItFailsJsonsPath,
            "categories.en.json");

        Response<ErrorCategoryCatalogDocument> loadResponse =
            await new JsonErrorCategoryCatalogLoader().LoadFromFileAsync(categoryCatalogFilePath);

        Assert.True(loadResponse.IsSuccess);
        Assert.NotNull(loadResponse.Data);

        ErrorCategoryCatalogDocument categoryCatalog =
            new ErrorCategoryCatalogDocumentNormalizer().Normalize(loadResponse.Data);

        ErrorCategoryDefinition? category = categoryCatalog.Categories.FirstOrDefault();
        Assert.NotNull(category);
        return category;
    }

    private static async Task<ErrorProfileDefinition> LoadProfileAsync(
        string whenItFailsJsonsPath,
        string profileName)
    {
        string profileCatalogFilePath = Path.Combine(
            whenItFailsJsonsPath,
            "profiles.json");

        Response<ErrorProfileCatalogDocument> loadResponse =
            await new JsonErrorProfileCatalogLoader().LoadFromFileAsync(profileCatalogFilePath);

        Assert.True(loadResponse.IsSuccess);
        Assert.NotNull(loadResponse.Data);

        ErrorProfileCatalogDocument profileCatalog =
            new ErrorProfileCatalogDocumentNormalizer().Normalize(loadResponse.Data);

        ErrorProfileDefinition? profile = profileCatalog.Profiles.FirstOrDefault(candidate =>
            string.Equals(
                candidate.Name,
                profileName,
                StringComparison.OrdinalIgnoreCase));

        Assert.NotNull(profile);
        return profile;
    }
}
