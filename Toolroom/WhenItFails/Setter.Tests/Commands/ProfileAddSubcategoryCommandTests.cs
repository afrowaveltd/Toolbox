using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

public sealed class ProfileAddSubcategoryCommandTests
{
    [Fact]
    public async Task ExecuteAsync_WithValidArguments_AddsSubcategoryAndReturnsSuccess()
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

        string subcategory = await LoadFirstSubcategoryAsync(
            temporaryWorkspace.WhenItFailsJsonsPath);

        int exitCode = await ProfileAddSubcategoryCommand.ExecuteAsync(
            [
                "profile-add-subcategory",
                temporaryWorkspace.ProjectRootPath,
                " dita test ",
                subcategory.ToLowerInvariant()
            ]);

        Assert.Equal(0, exitCode);

        ErrorProfileDefinition savedProfile = await LoadProfileAsync(
            temporaryWorkspace.WhenItFailsJsonsPath,
            "DITA_TEST");

        Assert.Contains(
            TextKeyNormalizer.NormalizeKey(subcategory),
            savedProfile.IncludeSubcategories);
    }

    [Fact]
    public async Task ExecuteAsync_WithSubcategoryAlreadyIncluded_ReturnsEditFailure()
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

        string subcategory = await LoadFirstSubcategoryAsync(
            temporaryWorkspace.WhenItFailsJsonsPath);

        Response<ErrorProfileDefinition> addSubcategoryResponse =
            await editor.ProfileAddSubcategoryAsync(
                temporaryWorkspace.ProjectRootPath,
                "DITA_TEST",
                subcategory);
        Assert.True(addSubcategoryResponse.IsSuccess);

        int exitCode = await ProfileAddSubcategoryCommand.ExecuteAsync(
            [
                "profile-add-subcategory",
                temporaryWorkspace.ProjectRootPath,
                "DITA_TEST",
                subcategory
            ]);

        Assert.Equal(2, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownSubcategory_ReturnsEditFailure()
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

        int exitCode = await ProfileAddSubcategoryCommand.ExecuteAsync(
            [
                "profile-add-subcategory",
                temporaryWorkspace.ProjectRootPath,
                "DITA_TEST",
                "DOES_NOT_EXIST"
            ]);

        Assert.Equal(2, exitCode);
    }

    public static TheoryData<string[]> MissingArgumentCases =>
        new()
        {
            new[] { "profile-add-subcategory" },
            new[] { "profile-add-subcategory", "." },
            new[] { "profile-add-subcategory", ".", "DITA_TEST" }
        };

    [Theory]
    [MemberData(nameof(MissingArgumentCases))]
    public async Task ExecuteAsync_WithMissingRequiredArgument_ReturnsCommandInputError(
        string[] args)
    {
        int exitCode = await ProfileAddSubcategoryCommand.ExecuteAsync(args);
        Assert.Equal(1, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithTooManyArguments_ReturnsCommandInputError()
    {
        int exitCode = await ProfileAddSubcategoryCommand.ExecuteAsync(
            [
                "profile-add-subcategory",
                ".",
                "DITA_TEST",
                "NETWORK",
                "Unexpected"
            ]);

        Assert.Equal(1, exitCode);
    }

    private static async Task<string> LoadFirstSubcategoryAsync(
        string whenItFailsJsonsPath)
    {
        string errorCatalogFilePath = Path.Combine(
            whenItFailsJsonsPath,
            "errors.en.json");

        Response<ErrorCatalogDocument> loadResponse =
            await new JsonErrorCatalogLoader().LoadFromFileAsync(errorCatalogFilePath);

        Assert.True(loadResponse.IsSuccess);
        Assert.NotNull(loadResponse.Data);

        ErrorCatalogDocument errorCatalog =
            new ErrorCatalogDocumentNormalizer().Normalize(loadResponse.Data);

        string? subcategory = errorCatalog.Errors
            .SelectMany(error => error.Subcategories)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

        Assert.False(string.IsNullOrWhiteSpace(subcategory));
        return subcategory;
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
