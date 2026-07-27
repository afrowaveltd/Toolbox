using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

[Collection(ConsoleOutputTestCollection.Name)]
public sealed class CheckDocKeysDuplicateHumanOutputTests
{
    [Fact]
    public async Task ExecuteAsync_WithDuplicateKey_WritesReadableFailureReport()
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
            workspace.ProjectRootPath
        ]);

        Assert.Equal(2, exitCode);
        Assert.Contains("Errors checked:", output, StringComparison.Ordinal);
        Assert.Contains(
            "Duplicate documentation keys:",
            output,
            StringComparison.Ordinal);
        Assert.Contains(duplicateDocumentationKey, output, StringComparison.Ordinal);
        Assert.Contains("AFW_GEN_0001", output, StringComparison.Ordinal);
        Assert.Contains("AFW_GEN_0002", output, StringComparison.Ordinal);
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
            AnsiConsole.Console = AnsiConsole.Create(
                new AnsiConsoleSettings
                {
                    Ansi = AnsiSupport.No,
                    ColorSystem = ColorSystemSupport.NoColors,
                    Out = new FixedWidthAnsiConsoleOutput(output, width: 240)
                });

            int exitCode = await CheckDocKeysCommand.ExecuteAsync(args);
            return (exitCode, output.ToString());
        }
        finally
        {
            Console.SetOut(originalOutput);
            AnsiConsole.Console = originalConsole;
        }
    }

    private sealed class FixedWidthAnsiConsoleOutput(TextWriter writer, int width)
        : IAnsiConsoleOutput
    {
        public TextWriter Writer { get; } = writer;

        public bool IsTerminal => false;

        public int Width { get; } = width;

        public int Height => 100;

        public void SetEncoding(System.Text.Encoding encoding)
        {
        }
    }
}
