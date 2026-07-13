using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

public sealed class ProfileAddOwnerCommandTests
{
    [Fact]
    public async Task ExecuteAsync_WithValidArguments_AddsOwnerAndReturnsSuccess()
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

        ErrorOwnerDefinition owner =
            await LoadFirstOwnerAsync(temporaryWorkspace.WhenItFailsJsonsPath);

        int exitCode = await ProfileAddOwnerCommand.ExecuteAsync(
            [
                "profile-add-owner",
                temporaryWorkspace.ProjectRootPath,
                "dita test",
                owner.Name.ToLowerInvariant()
            ]);

        Assert.Equal(0, exitCode);

        ErrorProfileDefinition savedProfile = await LoadProfileAsync(
            temporaryWorkspace.WhenItFailsJsonsPath,
            "DITA_TEST");

        Assert.Contains(
            savedProfile.IncludeOwners,
            includedOwner => string.Equals(
                includedOwner,
                owner.Name,
                StringComparison.Ordinal));
    }

    [Fact]
    public async Task ExecuteAsync_WithOwnerAlreadyIncluded_ReturnsEditFailure()
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

        ErrorOwnerDefinition owner =
            await LoadFirstOwnerAsync(temporaryWorkspace.WhenItFailsJsonsPath);

        Response<ErrorProfileDefinition> addOwnerResponse =
            await editor.ProfileAddOwnerAsync(
                temporaryWorkspace.ProjectRootPath,
                "DITA_TEST",
                owner.Name);

        Assert.True(addOwnerResponse.IsSuccess);

        int exitCode = await ProfileAddOwnerCommand.ExecuteAsync(
            [
                "profile-add-owner",
                temporaryWorkspace.ProjectRootPath,
                "DITA_TEST",
                owner.Name
            ]);

        Assert.Equal(2, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownOwner_ReturnsEditFailure()
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

        int exitCode = await ProfileAddOwnerCommand.ExecuteAsync(
            [
                "profile-add-owner",
                temporaryWorkspace.ProjectRootPath,
                "DITA_TEST",
                "DOES_NOT_EXIST"
            ]);

        Assert.Equal(2, exitCode);
    }

    public static TheoryData<string[]> MissingArgumentCases =>
        new()
        {
            new[] { "profile-add-owner" },
            new[] { "profile-add-owner", "." },
            new[] { "profile-add-owner", ".", "DITA_TEST" }
        };

    [Theory]
    [MemberData(nameof(MissingArgumentCases))]
    public async Task ExecuteAsync_WithMissingRequiredArgument_ReturnsCommandInputError(
        string[] args)
    {
        int exitCode = await ProfileAddOwnerCommand.ExecuteAsync(args);

        Assert.Equal(1, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithTooManyArguments_ReturnsCommandInputError()
    {
        int exitCode = await ProfileAddOwnerCommand.ExecuteAsync(
            [
                "profile-add-owner",
                ".",
                "DITA_TEST",
                "AFW",
                "Unexpected"
            ]);

        Assert.Equal(1, exitCode);
    }

    private static async Task<ErrorOwnerDefinition> LoadFirstOwnerAsync(
        string whenItFailsJsonsPath)
    {
        string ownerCatalogFilePath = Path.Combine(
            whenItFailsJsonsPath,
            "owners.en.json");

        Response<ErrorOwnerCatalogDocument> loadResponse =
            await new JsonErrorOwnerCatalogLoader().LoadFromFileAsync(ownerCatalogFilePath);

        Assert.True(loadResponse.IsSuccess);
        Assert.NotNull(loadResponse.Data);

        ErrorOwnerCatalogDocument ownerCatalog =
            new ErrorOwnerCatalogDocumentNormalizer().Normalize(loadResponse.Data);

        ErrorOwnerDefinition? owner = ownerCatalog.Owners.FirstOrDefault();
        Assert.NotNull(owner);
        return owner;
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
