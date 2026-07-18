using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Views;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Views;

[Collection("Console output")]
public sealed class HelpViewTests
{
    [Fact]
    public void Show_DescribesDocumentationKeyValidationAndGeneration()
    {
        IAnsiConsole originalConsole = AnsiConsole.Console;
        using StringWriter output = new();
        IAnsiConsole testConsole = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Ansi = AnsiSupport.No,
            ColorSystem = ColorSystemSupport.NoColors,
            Out = new FixedWidthAnsiConsoleOutput(output, width: 240)
        });

        try
        {
            AnsiConsole.Console = testConsole;

            HelpView.Show();

            string renderedHelp = output.ToString();
            Assert.Contains("check-doc-keys", renderedHelp, StringComparison.Ordinal);
            Assert.Contains(
                "unique, non-empty, canonical documentation key",
                renderedHelp,
                StringComparison.Ordinal);

            Assert.Contains("suggest-doc-key", renderedHelp, StringComparison.Ordinal);
            Assert.Contains(
                "Read-only suggestion of the first available canonical documentation key",
                renderedHelp,
                StringComparison.Ordinal);

            Assert.Contains("add-error", renderedHelp, StringComparison.Ordinal);
            Assert.Contains(
                "generate its first available canonical documentation key",
                renderedHelp,
                StringComparison.Ordinal);

            Assert.Contains("set-documentation-key", renderedHelp, StringComparison.Ordinal);
            Assert.Contains(
                "the value must be unique and canonical",
                renderedHelp,
                StringComparison.Ordinal);
        }
        finally
        {
            AnsiConsole.Console = originalConsole;
        }
    }

    private sealed class FixedWidthAnsiConsoleOutput(TextWriter writer, int width) : IAnsiConsoleOutput
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
