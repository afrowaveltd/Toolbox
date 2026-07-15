using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

public sealed class ExplainProfileCommandTests
{
    [Fact]
    public async Task ExecuteAsync_WithExistingProfile_ReturnsSuccessWithoutChangingWorkspace()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorProfileDefinition profile = await LoadFirstProfileAsync(workspace.WhenItFailsJsonsPath);
        Dictionary<string, string> before = await ReadCatalogFilesAsync(workspace.WhenItFailsJsonsPath);
        int backupsBefore = CountBackups(workspace.WhenItFailsJsonsPath);

        int exitCode = await ExplainProfileCommand.ExecuteAsync(
        [
            "explain-profile",
            workspace.ProjectRootPath,
            profile.DisplayName,
            "--plain"
        ]);

        Assert.Equal(0, exitCode);
        Assert.Equal(backupsBefore, CountBackups(workspace.WhenItFailsJsonsPath));
        Dictionary<string, string> after = await ReadCatalogFilesAsync(workspace.WhenItFailsJsonsPath);
        Assert.Equal(before, after);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownProfile_ReturnsExplanationFailure()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        int exitCode = await ExplainProfileCommand.ExecuteAsync(
        [
            "explain-profile",
            workspace.ProjectRootPath,
            "DOES_NOT_EXIST"
        ]);

        Assert.Equal(2, exitCode);
    }

    public static TheoryData<string[]> InvalidArgumentCases =>
        new()
        {
            new[] { "explain-profile" },
            new[] { "explain-profile", "." },
            new[] { "explain-profile", ".", "WEB", "--unknown" },
            new[] { "explain-profile", ".", "WEB", "--plain", "--plain" },
            new[] { "explain-profile", ".", "WEB", "EXTRA" }
        };

    [Theory]
    [MemberData(nameof(InvalidArgumentCases))]
    public async Task ExecuteAsync_WithInvalidArguments_ReturnsCommandInputError(string[] args)
    {
        Assert.Equal(1, await ExplainProfileCommand.ExecuteAsync(args));
    }

    private static async Task<ErrorProfileDefinition> LoadFirstProfileAsync(string jsonsPath)
    {
        Response<ErrorProfileCatalogDocument> response =
            await new JsonErrorProfileCatalogLoader().LoadFromFileAsync(
                Path.Combine(jsonsPath, "profiles.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);

        ErrorProfileCatalogDocument catalog =
            new ErrorProfileCatalogDocumentNormalizer().Normalize(response.Data);
        ErrorProfileDefinition? profile = catalog.Profiles.FirstOrDefault();
        Assert.NotNull(profile);
        return profile;
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
