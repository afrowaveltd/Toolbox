using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Editing;

public sealed class WhenItFailsProfileWorkspaceEditorExcludedErrorTests
{
    public static TheoryData<Func<ErrorDefinition, string>> ValidLookupSelectors =>
        new()
        {
            error => error.Id.ToLowerInvariant(),
            error => error.Code.ToString(),
            error => error.Name.ToLowerInvariant()
        };

    [Theory]
    [MemberData(nameof(ValidLookupSelectors))]
    public async Task ProfileAddExcludedErrorAsync_WithExistingError_AddsCanonicalId(
        Func<ErrorDefinition, string> lookupSelector)
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

        ErrorDefinition error = await LoadFirstErrorAsync(
            temporaryWorkspace.WhenItFailsJsonsPath);

        string[] backupsBeforeAdd = Directory.GetFiles(
            temporaryWorkspace.WhenItFailsJsonsPath,
            "profiles.*.bak.json");

        Response<ErrorProfileDefinition> response =
            await editor.ProfileAddExcludedErrorAsync(
                temporaryWorkspace.ProjectRootPath,
                " dita test ",
                lookupSelector(error));

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);

        string canonicalErrorId = TextKeyNormalizer.NormalizeKey(error.Id);
        Assert.Contains(canonicalErrorId, response.Data.ExcludeErrors);

        ErrorProfileDefinition savedProfile = await LoadProfileAsync(
            temporaryWorkspace.WhenItFailsJsonsPath,
            "DITA_TEST");

        Assert.Contains(canonicalErrorId, savedProfile.ExcludeErrors);

        string[] backupsAfterAdd = Directory.GetFiles(
            temporaryWorkspace.WhenItFailsJsonsPath,
            "profiles.*.bak.json");

        Assert.Equal(backupsBeforeAdd.Length + 1, backupsAfterAdd.Length);
    }

    [Fact]
    public async Task ProfileAddExcludedErrorAsync_WithDuplicate_ReturnsInvalidWithoutBackup()
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

        ErrorDefinition error = await LoadFirstErrorAsync(
            temporaryWorkspace.WhenItFailsJsonsPath);

        Response<ErrorProfileDefinition> firstResponse =
            await editor.ProfileAddExcludedErrorAsync(
                temporaryWorkspace.ProjectRootPath,
                "DITA_TEST",
                error.Id);

        Assert.True(firstResponse.IsSuccess);

        string[] backupsBeforeSecondAttempt = Directory.GetFiles(
            temporaryWorkspace.WhenItFailsJsonsPath,
            "profiles.*.bak.json");

        Response<ErrorProfileDefinition> secondResponse =
            await editor.ProfileAddExcludedErrorAsync(
                temporaryWorkspace.ProjectRootPath,
                "DITA_TEST",
                error.Code.ToString());

        Assert.False(secondResponse.IsSuccess);
        Assert.Contains(
            secondResponse.Issues,
            issue => issue.Code == "ProfileErrorAlreadyExcluded");

        string[] backupsAfterSecondAttempt = Directory.GetFiles(
            temporaryWorkspace.WhenItFailsJsonsPath,
            "profiles.*.bak.json");

        Assert.Equal(
            backupsBeforeSecondAttempt.Length,
            backupsAfterSecondAttempt.Length);
    }

    [Fact]
    public async Task ProfileAddExcludedErrorAsync_WithUnknownError_ReturnsNotFoundWithoutBackup()
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

        string[] backupsBeforeAttempt = Directory.GetFiles(
            temporaryWorkspace.WhenItFailsJsonsPath,
            "profiles.*.bak.json");

        Response<ErrorProfileDefinition> response =
            await editor.ProfileAddExcludedErrorAsync(
                temporaryWorkspace.ProjectRootPath,
                "DITA_TEST",
                "DOES_NOT_EXIST");

        Assert.False(response.IsSuccess);
        Assert.Contains(
            response.Issues,
            issue => issue.Code == "ErrorDefinitionNotFound");

        string[] backupsAfterAttempt = Directory.GetFiles(
            temporaryWorkspace.WhenItFailsJsonsPath,
            "profiles.*.bak.json");

        Assert.Equal(backupsBeforeAttempt.Length, backupsAfterAttempt.Length);
    }

    [Fact]
    public async Task ProfileAddExcludedErrorAsync_WithUnknownProfile_ReturnsNotFound()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        ErrorDefinition error = await LoadFirstErrorAsync(
            temporaryWorkspace.WhenItFailsJsonsPath);

        Response<ErrorProfileDefinition> response =
            await new WhenItFailsProfileWorkspaceEditor().ProfileAddExcludedErrorAsync(
                temporaryWorkspace.ProjectRootPath,
                "DOES_NOT_EXIST",
                error.Id);

        Assert.False(response.IsSuccess);
        Assert.Contains(
            response.Issues,
            issue => issue.Code == "ProfileNotFound");
    }

    [Theory]
    [InlineData("", "AFW-GEN-0001")]
    [InlineData("DITA_TEST", "")]
    public async Task ProfileAddExcludedErrorAsync_WithEmptyValue_ReturnsInvalid(
        string profileName,
        string errorLookup)
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        Response<ErrorProfileDefinition> response =
            await new WhenItFailsProfileWorkspaceEditor().ProfileAddExcludedErrorAsync(
                temporaryWorkspace.ProjectRootPath,
                profileName,
                errorLookup);

        Assert.False(response.IsSuccess);
    }

    private static async Task<ErrorDefinition> LoadFirstErrorAsync(
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

        ErrorDefinition? error = errorCatalog.Errors.FirstOrDefault(candidate =>
            !string.IsNullOrWhiteSpace(candidate.Id)
            && !string.IsNullOrWhiteSpace(candidate.Name)
            && candidate.Code > 0);

        Assert.NotNull(error);
        return error;
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
