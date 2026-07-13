using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

public sealed class RemoveProfileCommandTests
{
    [Fact]
    public async Task ExecuteAsync_WithValidArguments_RemovesProfileAndReturnsSuccess()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        WhenItFailsProfileWorkspaceEditor editor = new();
        Response<ErrorProfileDefinition> addResponse =
            await editor.AddProfileAsync(
                temporaryWorkspace.ProjectRootPath,
                "DITA_TEST",
                "DiTa Test");

        Assert.True(addResponse.IsSuccess);

        int exitCode = await RemoveProfileCommand.ExecuteAsync(
            [
                "remove-profile",
                temporaryWorkspace.ProjectRootPath,
                "dita test"
            ]);

        Assert.Equal(0, exitCode);

        ErrorProfileCatalogDocument catalog =
            await LoadProfileCatalogAsync(temporaryWorkspace.WhenItFailsJsonsPath);

        Assert.DoesNotContain(
            catalog.Profiles,
            profile => string.Equals(
                profile.Name,
                "DITA_TEST",
                StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownProfile_ReturnsEditFailure()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        int exitCode = await RemoveProfileCommand.ExecuteAsync(
            [
                "remove-profile",
                temporaryWorkspace.ProjectRootPath,
                "DOES_NOT_EXIST"
            ]);

        Assert.Equal(2, exitCode);
    }

    public static TheoryData<string[]> MissingRequiredArguments =>
        new()
        {
            new[] { "remove-profile" },
            new[] { "remove-profile", "." }
        };

    [Theory]
    [MemberData(nameof(MissingRequiredArguments))]
    public async Task ExecuteAsync_WithMissingRequiredArgument_ReturnsCommandInputError(
        string[] args)
    {
        int exitCode = await RemoveProfileCommand.ExecuteAsync(args);

        Assert.Equal(1, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithTooManyArguments_ReturnsCommandInputError()
    {
        int exitCode = await RemoveProfileCommand.ExecuteAsync(
            [
                "remove-profile",
                ".",
                "DITA",
                "Unexpected"
            ]);

        Assert.Equal(1, exitCode);
    }

    private static async Task<ErrorProfileCatalogDocument> LoadProfileCatalogAsync(
        string whenItFailsJsonsPath)
    {
        string profileCatalogFilePath = Path.Combine(
            whenItFailsJsonsPath,
            "profiles.json");

        Response<ErrorProfileCatalogDocument> loadResponse =
            await new JsonErrorProfileCatalogLoader().LoadFromFileAsync(profileCatalogFilePath);

        Assert.True(loadResponse.IsSuccess);
        Assert.NotNull(loadResponse.Data);

        return new ErrorProfileCatalogDocumentNormalizer().Normalize(loadResponse.Data);
    }
}
