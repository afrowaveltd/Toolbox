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
            Out = new AnsiConsoleOutput(output)
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
}
