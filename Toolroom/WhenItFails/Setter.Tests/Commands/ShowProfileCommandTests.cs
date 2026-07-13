using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

public sealed class ShowProfileCommandTests
{
    [Fact]
    public void TryParseOptions_WithoutSwitch_ReturnsFalsePlainOutput()
    {
        string[] args = ["show-profile", ".", "WEB"];

        bool result = ShowProfileCommand.TryParseOptions(
            args,
            out bool usePlainOutput);

        Assert.True(result);
        Assert.False(usePlainOutput);
    }

    [Fact]
    public void TryParseOptions_WithPlainSwitch_ReturnsTruePlainOutput()
    {
        string[] args = ["show-profile", ".", "WEB", "--plain"];

        bool result = ShowProfileCommand.TryParseOptions(
            args,
            out bool usePlainOutput);

        Assert.True(result);
        Assert.True(usePlainOutput);
    }

    [Fact]
    public void TryParseOptions_WithCaseInsensitivePlainSwitch_ReturnsTrue()
    {
        string[] args = ["show-profile", ".", "WEB", "--PLAIN"];

        bool result = ShowProfileCommand.TryParseOptions(
            args,
            out bool usePlainOutput);

        Assert.True(result);
        Assert.True(usePlainOutput);
    }

    [Fact]
    public void TryParseOptions_WithUnknownArgument_ReturnsFalse()
    {
        string[] args = ["show-profile", ".", "WEB", "--json"];

        bool result = ShowProfileCommand.TryParseOptions(
            args,
            out _);

        Assert.False(result);
    }

    [Fact]
    public void TryParseOptions_WithDuplicatePlainSwitch_ReturnsFalse()
    {
        string[] args = ["show-profile", ".", "WEB", "--plain", "--plain"];

        bool result = ShowProfileCommand.TryParseOptions(
            args,
            out _);

        Assert.False(result);
    }

    [Fact]
    public void FindProfile_FindsProfileByNormalizedNameIgnoringCase()
    {
        WhenItFailsWorkspaceSummary summary = CreateSummary();

        ErrorProfileDefinition? profile = ShowProfileCommand.FindProfile(
            summary,
            "web");

        Assert.NotNull(profile);
        Assert.Equal("WEB", profile.Name);
    }

    [Fact]
    public void FindProfile_FindsProfileByDisplayNameIgnoringCase()
    {
        WhenItFailsWorkspaceSummary summary = CreateSummary();

        ErrorProfileDefinition? profile = ShowProfileCommand.FindProfile(
            summary,
            "web application");

        Assert.NotNull(profile);
        Assert.Equal("WEB", profile.Name);
    }

    [Fact]
    public void FindProfile_WithUnknownName_ReturnsNull()
    {
        WhenItFailsWorkspaceSummary summary = CreateSummary();

        ErrorProfileDefinition? profile = ShowProfileCommand.FindProfile(
            summary,
            "UNKNOWN");

        Assert.Null(profile);
    }

    [Fact]
    public async Task ExecuteAsync_WithInitializedWorkspaceAndPlainOutput_ReturnsSuccess()
    {
        using TemporaryWorkspace temporaryWorkspace =
            await TemporaryWorkspace.CreateInitializedAsync();

        int exitCode = await ShowProfileCommand.ExecuteAsync(
            ["show-profile", temporaryWorkspace.ProjectRootPath, "WEB", "--plain"]);

        Assert.Equal(0, exitCode);
    }

    private static WhenItFailsWorkspaceSummary CreateSummary()
    {
        return new WhenItFailsWorkspaceSummary
        {
            ProfileCatalog = new ErrorProfileCatalogDocument
            {
                Profiles =
                [
                    new ErrorProfileDefinition
                    {
                        Name = "WEB",
                        DisplayName = "Web Application"
                    }
                ]
            }
        };
    }

    private sealed class TemporaryWorkspace : IDisposable
    {
        private TemporaryWorkspace(string projectRootPath)
        {
            ProjectRootPath = projectRootPath;
        }

        public string ProjectRootPath { get; }

        public static async Task<TemporaryWorkspace> CreateInitializedAsync()
        {
            string projectRootPath = Path.Combine(
                Path.GetTempPath(),
                "afrowave-whenitfails-setter-command-tests",
                Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(projectRootPath);

            TemporaryWorkspace temporaryWorkspace = new(projectRootPath);
            WhenItFailsWorkspaceInitializer initializer = new();
            var initializeResponse =
                await initializer.InitializeAsync(projectRootPath);

            Assert.True(initializeResponse.IsSuccess);

            return temporaryWorkspace;
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(ProjectRootPath))
                {
                    Directory.Delete(ProjectRootPath, recursive: true);
                }
            }
            catch
            {
                // Test cleanup should not hide the real test result.
            }
        }
    }
}
