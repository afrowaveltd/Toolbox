using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Editing;

public sealed class WhenItFailsProfileWorkspaceEditorErrorRemovalTests
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
    public async Task ProfileRemoveErrorAsync_WithIncludedError_RemovesCanonicalId(
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

        Response<ErrorProfileDefinition> addErrorResponse =
            await editor.ProfileAddErrorAsync(
                temporaryWorkspace.ProjectRootPath,
                "DITA_TEST",
                error.Id);

        Assert.True(addErrorResponse.IsSuccess);

        string[] backupsBeforeRemove = Directory.GetFiles(
            temporaryWorkspace.WhenItFailsJsonsPath,
            "profiles.*.bak.json");

        Response<ErrorProfileDefinition> removeResponse =
            await editor.ProfileRemoveErrorAsync(
                temporaryWorkspace.ProjectRootPath,
                " dita test ",
                lookupSelector(error));

        Assert.True(removeResponse.IsSuccess);
        Assert.NotNull(removeResponse.Data);

        string canonicalErrorId = TextKeyNormalizer.NormalizeKey(error.Id);
        Assert.DoesNotContain(canonicalErrorId, removeResponse.Data.IncludeErrors);

        ErrorProfileDefinition savedProfile = await LoadProfileAsync(
            temporaryWorkspace.WhenItFailsJsonsPath,
            "DITA_TEST");

        Assert.DoesNotContain(canonicalErrorId, savedProfile.IncludeErrors);

        string[] backupsAfterRemove = Directory.GetFiles(
            temporaryWorkspace.WhenItFailsJsonsPath,
            "profiles.*.bak.json");

        Assert.Equal(backupsBeforeRemove.Length + 1, backupsAfterRemove.Length);
    }

    [Fact]
    public async Task ProfileRemoveErrorAsync_WithErrorNotIncluded_ReturnsNotFoundWithoutBackup()
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

        string[] backupsBeforeAttempt = Directory.GetFiles(
            temporaryWorkspace.WhenItFailsJsonsPath,
            "profiles.*.bak.json");

        Response<ErrorProfileDefinition> response =
            await editor.ProfileRemoveErrorAsync(
                temporaryWorkspace.ProjectRootPath,
                "DITA_TEST",
                error.Code.ToString());

        Assert.False(response.IsSuccess);
        Assert.Contains(
            response.Issues,
            issue => issue.Code == "ProfileErrorNotIncluded");

        string[] backupsAfterAttempt = Directory.GetFiles(
            temporaryWorkspace.WhenItFailsJsonsPath,
            "profiles.*.bak.json");

        Assert.Equal(backupsBeforeAttempt.Length, backupsAfterAttempt.Length);
    }

    [Fact]
    public async Task ProfileRemoveErrorAsync_WithUnknownError_ReturnsNotFoundWithoutBackup()
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
            await editor.ProfileRemoveErrorAsync(
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
    public async Task ProfileRemoveErrorAsync_WithUnknownProfile_ReturnsNotFound()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        ErrorDefinition error = await LoadFirstErrorAsync(
            temporaryWorkspace.WhenItFailsJsonsPath);

        Response<ErrorProfileDefinition> response =
            await new WhenItFailsProfileWorkspaceEditor().ProfileRemoveErrorAsync(
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
    public async Task ProfileRemoveErrorAsync_WithEmptyValue_ReturnsInvalid(
        string profileName,
        string errorLookup)
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        Response<ErrorProfileDefinition> response =
            await new WhenItFailsProfileWorkspaceEditor().ProfileRemoveErrorAsync(
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
