using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

public sealed class ProfileRemoveExcludedErrorCommandTests
{
    [Fact]
    public async Task ExecuteAsync_WithValidArguments_RemovesExcludedErrorAndReturnsSuccess()
    {
        using TemporaryWhenItFailsWorkspace workspace = await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        WhenItFailsProfileWorkspaceEditor editor = new();
        Assert.True((await editor.AddProfileAsync(workspace.ProjectRootPath, "DITA_TEST", "DiTa Test")).IsSuccess);
        ErrorDefinition error = await LoadFirstErrorAsync(workspace.WhenItFailsJsonsPath);
        Assert.True((await editor.ProfileAddExcludedErrorAsync(workspace.ProjectRootPath, "DITA_TEST", error.Id)).IsSuccess);

        int exitCode = await ProfileRemoveExcludedErrorCommand.ExecuteAsync([
            "profile-remove-excluded-error", workspace.ProjectRootPath, " dita test ", error.Name.ToLowerInvariant()]);

        Assert.Equal(0, exitCode);
        ErrorProfileDefinition profile = await LoadProfileAsync(workspace.WhenItFailsJsonsPath, "DITA_TEST");
        Assert.DoesNotContain(TextKeyNormalizer.NormalizeKey(error.Id), profile.ExcludeErrors);
    }

    [Fact]
    public async Task ExecuteAsync_WithErrorNotExcluded_ReturnsEditFailure()
    {
        using TemporaryWhenItFailsWorkspace workspace = await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        WhenItFailsProfileWorkspaceEditor editor = new();
        Assert.True((await editor.AddProfileAsync(workspace.ProjectRootPath, "DITA_TEST", "DiTa Test")).IsSuccess);
        ErrorDefinition error = await LoadFirstErrorAsync(workspace.WhenItFailsJsonsPath);

        int exitCode = await ProfileRemoveExcludedErrorCommand.ExecuteAsync([
            "profile-remove-excluded-error", workspace.ProjectRootPath, "DITA_TEST", error.Code.ToString()]);

        Assert.Equal(2, exitCode);
    }

    public static TheoryData<string[]> InvalidArgumentCases => new()
    {
        new[] { "profile-remove-excluded-error" },
        new[] { "profile-remove-excluded-error", "." },
        new[] { "profile-remove-excluded-error", ".", "DITA_TEST" },
        new[] { "profile-remove-excluded-error", ".", "DITA_TEST", "ERROR", "extra" }
    };

    [Theory]
    [MemberData(nameof(InvalidArgumentCases))]
    public async Task ExecuteAsync_WithInvalidArguments_ReturnsCommandInputError(string[] args)
    {
        Assert.Equal(1, await ProfileRemoveExcludedErrorCommand.ExecuteAsync(args));
    }

    private static async Task<ErrorDefinition> LoadFirstErrorAsync(string jsonsPath)
    {
        Response<ErrorCatalogDocument> response = await new JsonErrorCatalogLoader().LoadFromFileAsync(Path.Combine(jsonsPath, "errors.en.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        ErrorDefinition? error = new ErrorCatalogDocumentNormalizer().Normalize(response.Data).Errors
            .FirstOrDefault(candidate => !string.IsNullOrWhiteSpace(candidate.Id) && !string.IsNullOrWhiteSpace(candidate.Name) && candidate.Code > 0);
        Assert.NotNull(error);
        return error;
    }

    private static async Task<ErrorProfileDefinition> LoadProfileAsync(string jsonsPath, string name)
    {
        Response<ErrorProfileCatalogDocument> response = await new JsonErrorProfileCatalogLoader().LoadFromFileAsync(Path.Combine(jsonsPath, "profiles.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        ErrorProfileDefinition? profile = new ErrorProfileCatalogDocumentNormalizer().Normalize(response.Data).Profiles
            .FirstOrDefault(candidate => string.Equals(candidate.Name, name, StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(profile);
        return profile;
    }
}
