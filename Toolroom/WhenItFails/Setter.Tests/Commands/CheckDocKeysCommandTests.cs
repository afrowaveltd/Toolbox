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

    [Theory]
    [InlineData(null)]
    [InlineData("--plain")]
    [InlineData("--json")]
    public async Task ExecuteAsync_WithNonCanonicalKey_ReturnsCheckFailure(string? outputOption)
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        string catalogPath = Path.Combine(
            workspace.WhenItFailsJsonsPath,
            "errors.en.json");
        string json = await File.ReadAllTextAsync(catalogPath);
        json = json.Replace(
            "when-it-fails/errors/general/unknown-error",
            "When-It-Fails/errors/general/unknown-error",
            StringComparison.Ordinal);
        await File.WriteAllTextAsync(catalogPath, json);

        List<string> args =
        [
            "check-doc-keys",
            workspace.ProjectRootPath
        ];
        if (outputOption is not null)
        {
            args.Add(outputOption);
        }

        int exitCode = await CheckDocKeysCommand.ExecuteAsync(args.ToArray());

        Assert.Equal(2, exitCode);
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
