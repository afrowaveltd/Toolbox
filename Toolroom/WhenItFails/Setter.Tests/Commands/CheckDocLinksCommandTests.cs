using System.Text.Json;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

[Collection(ConsoleOutputTestCollection.Name)]
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
    public async Task ExecuteAsync_WithValidLinksAndJsonOutput_WritesStableEnvelopeAndCompleteReport()
    {
        using TemporarySetterDocumentation documentation = new();
        await documentation.WriteAsync("Docs/Target.md", "# Target");
        await documentation.WriteAsync("README.md", "[Target](Docs/Target.md)");

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "check-doc-links",
            documentation.SetterPath,
            "--json"
        ]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement root = document.RootElement;
        JsonElement data = root.GetProperty("data");

        Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("check-doc-links", root.GetProperty("command").GetString());
        Assert.False(string.IsNullOrWhiteSpace(data.GetProperty("setterPath").GetString()));
        Assert.True(data.GetProperty("markdownFilesChecked").GetInt32() > 0);
        Assert.True(data.GetProperty("localLinksChecked").GetInt32() > 0);
        Assert.Empty(data.GetProperty("brokenLinks").EnumerateArray());
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
    public async Task ExecuteAsync_WithBrokenLinkAndJson_WritesStableFailureReport()
    {
        using TemporarySetterDocumentation documentation = new();
        await documentation.WriteAsync("README.md", "[Missing](Docs/Missing.md)");

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "check-doc-links",
            documentation.SetterPath,
            "--json"
        ]);

        Assert.Equal(2, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement root = document.RootElement;
        JsonElement data = root.GetProperty("data");
        JsonElement brokenLinks = data.GetProperty("brokenLinks");

        Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("check-doc-links", root.GetProperty("command").GetString());
        Assert.False(string.IsNullOrWhiteSpace(data.GetProperty("setterPath").GetString()));
        Assert.True(data.GetProperty("markdownFilesChecked").GetInt32() > 0);
        Assert.True(data.GetProperty("localLinksChecked").GetInt32() > 0);
        Assert.Single(brokenLinks.EnumerateArray());

        JsonElement brokenLink = brokenLinks.EnumerateArray().Single();
        Assert.Equal("README.md", brokenLink.GetProperty("sourceFile").GetString());
        Assert.Equal("Docs/Missing.md", brokenLink.GetProperty("target").GetString());
        Assert.False(string.IsNullOrWhiteSpace(brokenLink.GetProperty("resolvedPath").GetString()));
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

    [Fact]
    public async Task ExecuteAsync_WithMissingDirectoryAndJson_WritesStableFailureEnvelope()
    {
        string missingPath = Path.Combine(
            Path.GetTempPath(),
            "WhenItFailsSetterTests",
            Guid.NewGuid().ToString("N"));

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "check-doc-links",
            missingPath,
            "--json"
        ]);

        Assert.Equal(2, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement root = document.RootElement;
        JsonElement data = root.GetProperty("data");

        Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("check-doc-links", root.GetProperty("command").GetString());
        Assert.False(data.GetProperty("checked").GetBoolean());
        Assert.Equal(JsonValueKind.Object, data.GetProperty("validation").ValueKind);
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

    private static async Task<(int ExitCode, string Output)> ExecuteWithCapturedOutputAsync(string[] args)
    {
        TextWriter originalOutput = Console.Out;
        using StringWriter output = new();

        try
        {
            Console.SetOut(output);
            int exitCode = await CheckDocLinksCommand.ExecuteAsync(args);
            return (exitCode, output.ToString());
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }

    private sealed class TemporarySetterDocumentation : IDisposable
    {
        public TemporarySetterDocumentation()
        {
            SetterPath = Path.Combine(
                Path.GetTempPath(),
                "WhenItFailsSetterTests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(SetterPath, "Commands"));
            Directory.CreateDirectory(Path.Combine(SetterPath, "Docs"));
            File.WriteAllText(Path.Combine(SetterPath, "Program.cs"), "// Test Setter entry point");
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
