using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

public sealed class CheckDocKeysCommandTests
{
    [Fact]
    public async Task ExecuteAsync_WithInitializedWorkspace_ReturnsSuccess()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        int exitCode = await CheckDocKeysCommand.ExecuteAsync(
        [
            "check-doc-keys",
            workspace.ProjectRootPath
        ]);

        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithPlainOutput_ReturnsSuccess()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        int exitCode = await CheckDocKeysCommand.ExecuteAsync(
        [
            "check-doc-keys",
            workspace.ProjectRootPath,
            "--plain"
        ]);

        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithJsonOutput_ReturnsSuccess()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        int exitCode = await CheckDocKeysCommand.ExecuteAsync(
        [
            "check-doc-keys",
            workspace.ProjectRootPath,
            "--json"
        ]);

        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingDirectory_ReturnsCheckFailure()
    {
        string missingPath = Path.Combine(
            Path.GetTempPath(),
            "WhenItFailsSetterTests",
            Guid.NewGuid().ToString("N"));

        int exitCode = await CheckDocKeysCommand.ExecuteAsync(
        [
            "check-doc-keys",
            missingPath
        ]);

        Assert.Equal(2, exitCode);
    }

    public static TheoryData<string[]> InvalidArgumentCases =>
        new()
        {
            new[] { "check-doc-keys" },
            new[] { "check-doc-keys", ".", "--unknown" },
            new[] { "check-doc-keys", ".", "--plain", "--json" },
            new[] { "check-doc-keys", ".", "--plain", "--plain" },
            new[] { "check-doc-keys", ".", "--json", "--json" }
        };

    [Theory]
    [MemberData(nameof(InvalidArgumentCases))]
    public async Task ExecuteAsync_WithInvalidArguments_ReturnsCommandInputError(string[] args)
    {
        Assert.Equal(1, await CheckDocKeysCommand.ExecuteAsync(args));
    }
}
