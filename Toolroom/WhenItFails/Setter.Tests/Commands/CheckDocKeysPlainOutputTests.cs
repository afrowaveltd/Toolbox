using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

[Collection(ConsoleOutputTestCollection.Name)]
public sealed class CheckDocKeysPlainOutputTests
{
    [Fact]
    public async Task ExecuteAsync_WithValidCatalogAndPlainOutput_WritesNothing()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "check-doc-keys",
            workspace.ProjectRootPath,
            "--plain"
        ]);

        Assert.Equal(0, exitCode);
        Assert.Equal(string.Empty, output);
    }

    [Fact]
    public async Task ExecuteAsync_WithDirectWhenItFailsJsonsPathAndPlainOutput_WritesNothing()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "check-doc-keys",
            workspace.WhenItFailsJsonsPath,
            "--plain"
        ]);

        Assert.Equal(0, exitCode);
        Assert.Equal(string.Empty, output);
    }

    [Theory]
    [InlineData("--plain")]
    [InlineData("--PLAIN")]
    public async Task ExecuteAsync_WithNonCanonicalKeyAndPlainOutput_WritesCompleteInvalidFormatRow(
        string outputSwitch)
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        string catalogPath = Path.Combine(
            workspace.WhenItFailsJsonsPath,
            "errors.en.json");
        string json = await File.ReadAllTextAsync(catalogPath);
        const string invalidDocumentationKey =
            "When-It-Fails/errors/general/unknown-error";
        json = json.Replace(
            "when-it-fails/errors/general/unknown-error",
            invalidDocumentationKey,
            StringComparison.Ordinal);
        await File.WriteAllTextAsync(catalogPath, json);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "check-doc-keys",
            workspace.ProjectRootPath,
            outputSwitch
        ]);

        Assert.Equal(2, exitCode);
        string invalidFormatRow = Assert.Single(
            output.Split(
                Environment.NewLine,
                StringSplitOptions.RemoveEmptyEntries),
            line => line.StartsWith("invalid-format\t", StringComparison.Ordinal));
        string[] fields = invalidFormatRow.Split('\t');

        Assert.Equal(5, fields.Length);
        Assert.Equal("invalid-format", fields[0]);
        Assert.Equal(invalidDocumentationKey, fields[1]);
        Assert.True(int.TryParse(fields[2], out _));
        Assert.False(string.IsNullOrWhiteSpace(fields[3]));
        Assert.False(string.IsNullOrWhiteSpace(fields[4]));
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingKeyAndPlainOutput_WritesCompleteMissingRow()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        string catalogPath = Path.Combine(
            workspace.WhenItFailsJsonsPath,
            "errors.en.json");
        string json = await File.ReadAllTextAsync(catalogPath);
        json = json.Replace(
            "\"documentationKey\": \"when-it-fails/errors/general/unknown-error\"",
            "\"documentationKey\": \"\"",
            StringComparison.Ordinal);
        await File.WriteAllTextAsync(catalogPath, json);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "check-doc-keys",
            workspace.ProjectRootPath,
            "--plain"
        ]);

        Assert.Equal(2, exitCode);
        string missingRow = Assert.Single(
            output.Split(
                Environment.NewLine,
                StringSplitOptions.RemoveEmptyEntries),
            line => line.StartsWith("missing\t", StringComparison.Ordinal));
        string[] fields = missingRow.Split('\t');

        Assert.Equal(4, fields.Length);
        Assert.Equal("missing", fields[0]);
        Assert.True(int.TryParse(fields[1], out _));
        Assert.False(string.IsNullOrWhiteSpace(fields[2]));
        Assert.False(string.IsNullOrWhiteSpace(fields[3]));
    }

    [Fact]
    public async Task ExecuteAsync_WithDuplicateKeyAndPlainOutput_WritesCompleteDuplicateRows()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        string catalogPath = Path.Combine(
            workspace.WhenItFailsJsonsPath,
            "errors.en.json");
        string json = await File.ReadAllTextAsync(catalogPath);
        const string duplicateDocumentationKey =
            "when-it-fails/errors/general/unknown-error";
        json = json.Replace(
            "when-it-fails/errors/general/operation-failed",
            duplicateDocumentationKey,
            StringComparison.Ordinal);
        await File.WriteAllTextAsync(catalogPath, json);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "check-doc-keys",
            workspace.ProjectRootPath,
            "--plain"
        ]);

        Assert.Equal(2, exitCode);
        string[] duplicateRows = output.Split(
                Environment.NewLine,
                StringSplitOptions.RemoveEmptyEntries)
            .Where(line => line.StartsWith("duplicate\t", StringComparison.Ordinal))
            .ToArray();

        Assert.Equal(2, duplicateRows.Length);

        foreach (string duplicateRow in duplicateRows)
        {
            string[] fields = duplicateRow.Split('\t');

            Assert.Equal(5, fields.Length);
            Assert.Equal("duplicate", fields[0]);
            Assert.Equal(duplicateDocumentationKey, fields[1]);
            Assert.True(int.TryParse(fields[2], out _));
            Assert.False(string.IsNullOrWhiteSpace(fields[3]));
            Assert.False(string.IsNullOrWhiteSpace(fields[4]));
        }
    }

    private static async Task<(int ExitCode, string Output)> ExecuteWithCapturedOutputAsync(
        string[] args)
    {
        IAnsiConsole originalConsole = AnsiConsole.Console;
        TextWriter originalOutput = Console.Out;
        using StringWriter output = new();

        try
        {
            Console.SetOut(output);
            int exitCode = await CheckDocKeysCommand.ExecuteAsync(args);
            return (exitCode, output.ToString());
        }
        finally
        {
            Console.SetOut(originalOutput);
            AnsiConsole.Console = originalConsole;
        }
    }
}
