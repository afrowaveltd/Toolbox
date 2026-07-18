using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

public sealed class CheckDocLinksCommandTests
{
    [Fact]
    public async Task ExecuteAsync_WithValidLinks_ReturnsSuccess()
    {
        using TemporarySetterDocumentation documentation = new();
        await documentation.WriteAsync("Docs/Target.md", "# Target");
        await documentation.WriteAsync("README.md", "[Target](Docs/Target.md)");

        int exitCode = await CheckDocLinksCommand.ExecuteAsync(
        [
            "check-doc-links",
            documentation.SetterPath
        ]);

        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidLinksAndPlainOutput_ReturnsSuccess()
    {
        using TemporarySetterDocumentation documentation = new();
        await documentation.WriteAsync("Docs/Target.md", "# Target");
        await documentation.WriteAsync("README.md", "[Target](Docs/Target.md)");

        int exitCode = await CheckDocLinksCommand.ExecuteAsync(
        [
            "check-doc-links",
            documentation.SetterPath,
            "--plain"
        ]);

        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidLinksAndJsonOutput_ReturnsSuccess()
    {
        using TemporarySetterDocumentation documentation = new();
        await documentation.WriteAsync("Docs/Target.md", "# Target");
        await documentation.WriteAsync("README.md", "[Target](Docs/Target.md)");

        int exitCode = await CheckDocLinksCommand.ExecuteAsync(
        [
            "check-doc-links",
            documentation.SetterPath,
            "--json"
        ]);

        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithBrokenLink_ReturnsCheckFailure()
    {
        using TemporarySetterDocumentation documentation = new();
        await documentation.WriteAsync("README.md", "[Missing](Docs/Missing.md)");

        int exitCode = await CheckDocLinksCommand.ExecuteAsync(
        [
            "check-doc-links",
            documentation.SetterPath
        ]);

        Assert.Equal(2, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingDirectory_ReturnsCheckFailure()
    {
        string missingPath = Path.Combine(
            Path.GetTempPath(),
            "WhenItFailsSetterTests",
            Guid.NewGuid().ToString("N"));

        int exitCode = await CheckDocLinksCommand.ExecuteAsync(
        [
            "check-doc-links",
            missingPath
        ]);

        Assert.Equal(2, exitCode);
    }

    public static TheoryData<string[]> InvalidArgumentCases =>
        new()
        {
            new[] { "check-doc-links" },
            new[] { "check-doc-links", ".", "--unknown" },
            new[] { "check-doc-links", ".", "--plain", "--json" },
            new[] { "check-doc-links", ".", "--plain", "--plain" },
            new[] { "check-doc-links", ".", "--json", "--json" }
        };

    [Theory]
    [MemberData(nameof(InvalidArgumentCases))]
    public async Task ExecuteAsync_WithInvalidArguments_ReturnsCommandInputError(string[] args)
    {
        Assert.Equal(1, await CheckDocLinksCommand.ExecuteAsync(args));
    }

    private sealed class TemporarySetterDocumentation : IDisposable
    {
        public TemporarySetterDocumentation()
        {
            SetterPath = Path.Combine(
                Path.GetTempPath(),
                "WhenItFailsSetterTests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(SetterPath, "Docs"));
        }

        public string SetterPath { get; }

        public async Task WriteAsync(string relativePath, string content)
        {
            string fullPath = Path.Combine(
                SetterPath,
                relativePath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            await File.WriteAllTextAsync(fullPath, content);
        }

        public void Dispose()
        {
            if (Directory.Exists(SetterPath))
            {
                Directory.Delete(SetterPath, recursive: true);
            }
        }
    }
}
