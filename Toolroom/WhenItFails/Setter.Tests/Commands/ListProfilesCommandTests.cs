using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

public sealed class ListProfilesCommandTests
{
    [Fact]
    public void TryParseOptions_WithPathOnly_ReturnsRichOutput()
    {
        string[] args = ["list-profiles", "."];

        bool isValid = ListProfilesCommand.TryParseOptions(
            args,
            out bool usePlainOutput);

        Assert.True(isValid);
        Assert.False(usePlainOutput);
    }

    [Theory]
    [InlineData("--plain")]
    [InlineData("--PLAIN")]
    public void TryParseOptions_WithPlainSwitch_ReturnsPlainOutput(string plainSwitch)
    {
        string[] args = ["list-profiles", ".", plainSwitch];

        bool isValid = ListProfilesCommand.TryParseOptions(
            args,
            out bool usePlainOutput);

        Assert.True(isValid);
        Assert.True(usePlainOutput);
    }

    [Theory]
    [InlineData("--json")]
    [InlineData("unexpected")]
    public void TryParseOptions_WithUnknownArgument_ReturnsInvalid(string unknownArgument)
    {
        string[] args = ["list-profiles", ".", unknownArgument];

        bool isValid = ListProfilesCommand.TryParseOptions(
            args,
            out bool usePlainOutput);

        Assert.False(isValid);
        Assert.False(usePlainOutput);
    }

    [Fact]
    public void TryParseOptions_WithDuplicatePlainSwitch_ReturnsInvalid()
    {
        string[] args = ["list-profiles", ".", "--plain", "--plain"];

        bool isValid = ListProfilesCommand.TryParseOptions(
            args,
            out bool usePlainOutput);

        Assert.False(isValid);
        Assert.True(usePlainOutput);
    }

    [Fact]
    public async Task ExecuteAsync_WithInitializedWorkspaceAndPlainOutput_ReturnsSuccess()
    {
        using TemporaryWorkspace temporaryWorkspace =
            await TemporaryWorkspace.CreateAsync();

        int exitCode = await ListProfilesCommand.ExecuteAsync(
            ["list-profiles", temporaryWorkspace.ProjectRootPath, "--plain"]);

        Assert.Equal(0, exitCode);
    }

    private sealed class TemporaryWorkspace : IDisposable
    {
        private TemporaryWorkspace(string projectRootPath)
        {
            ProjectRootPath = projectRootPath;
        }

        public string ProjectRootPath { get; }

        public static async Task<TemporaryWorkspace> CreateAsync()
        {
            string projectRootPath = Path.Combine(
                Path.GetTempPath(),
                "afrowave-whenitfails-setter-command-tests",
                Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(projectRootPath);

            WhenItFailsWorkspaceInitializer initializer = new();
            var initializeResponse =
                await initializer.InitializeAsync(projectRootPath);

            Assert.True(initializeResponse.IsSuccess);

            return new TemporaryWorkspace(projectRootPath);
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
