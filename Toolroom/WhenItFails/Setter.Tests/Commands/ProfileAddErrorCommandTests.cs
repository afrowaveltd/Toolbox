using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

public sealed class ProfileAddErrorCommandTests
{
    [Fact]
    public async Task ExecuteAsync_WithValidArguments_AddsCanonicalErrorId()
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

        int exitCode = await ProfileAddErrorCommand.ExecuteAsync(
            [
                "profile-add-error",
                temporaryWorkspace.ProjectRootPath,
                " dita test ",
                error.Code.ToString()
            ]);

        Assert.Equal(0, exitCode);

        ErrorProfileDefinition savedProfile = await LoadProfileAsync(
            temporaryWorkspace.WhenItFailsJsonsPath,
            "DITA_TEST");

        Assert.Contains(
            TextKeyNormalizer.NormalizeKey(error.Id),
            savedProfile.IncludeErrors);
    }

    [Fact]
    public async Task ExecuteAsync_WithErrorAlreadyIncluded_ReturnsEditFailure()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        WhenItFailsProfileWorkspaceEditor editor = new();
        Assert.True((await editor.AddProfileAsync(
            temporaryWorkspace.ProjectRootPath,
            "DITA_TEST",
            "DiTa Test")).IsSuccess);

        ErrorDefinition error = await LoadFirstErrorAsync(
            temporaryWorkspace.WhenItFailsJsonsPath);
        Assert.True((await editor.ProfileAddErrorAsync(
            temporaryWorkspace.ProjectRootPath,
            "DITA_TEST",
            error.Id)).IsSuccess);

        int exitCode = await ProfileAddErrorCommand.ExecuteAsync(
            [
                "profile-add-error",
                temporaryWorkspace.ProjectRootPath,
                "DITA_TEST",
                error.Name
            ]);

        Assert.Equal(2, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownError_ReturnsEditFailure()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        Assert.True((await new WhenItFailsProfileWorkspaceEditor().AddProfileAsync(
            temporaryWorkspace.ProjectRootPath,
            "DITA_TEST",
            "DiTa Test")).IsSuccess);

        int exitCode = await ProfileAddErrorCommand.ExecuteAsync(
            [
                "profile-add-error",
                temporaryWorkspace.ProjectRootPath,
                "DITA_TEST",
                "DOES_NOT_EXIST"
            ]);

        Assert.Equal(2, exitCode);
    }

    public static TheoryData<string[]> MissingArgumentCases =>
        new()
        {
            new[] { "profile-add-error" },
            new[] { "profile-add-error", "." },
            new[] { "profile-add-error", ".", "DITA_TEST" }
        };

    [Theory]
    [MemberData(nameof(MissingArgumentCases))]
    public async Task ExecuteAsync_WithMissingRequiredArgument_ReturnsCommandInputError(
        string[] args)
    {
        Assert.Equal(1, await ProfileAddErrorCommand.ExecuteAsync(args));
    }

    [Fact]
    public async Task ExecuteAsync_WithTooManyArguments_ReturnsCommandInputError()
    {
        Assert.Equal(
            1,
            await ProfileAddErrorCommand.ExecuteAsync(
                ["profile-add-error", ".", "DITA_TEST", "AFW_GEN_0001", "Unexpected"]));
    }

    private static async Task<ErrorDefinition> LoadFirstErrorAsync(string jsonsPath)
    {
        Response<ErrorCatalogDocument> response =
            await new JsonErrorCatalogLoader().LoadFromFileAsync(
                Path.Combine(jsonsPath, "errors.en.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);

        ErrorDefinition? error = new ErrorCatalogDocumentNormalizer()
            .Normalize(response.Data)
            .Errors
            .FirstOrDefault(candidate =>
                !string.IsNullOrWhiteSpace(candidate.Id)
                && !string.IsNullOrWhiteSpace(candidate.Name)
                && candidate.Code > 0);

        Assert.NotNull(error);
        return error;
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
}
