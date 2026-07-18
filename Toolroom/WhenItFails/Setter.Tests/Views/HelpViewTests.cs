using System.Text.RegularExpressions;
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
            HashSet<string> words = Regex.Matches(
                    renderedHelp,
                    "[A-Za-z]+(?:-[A-Za-z]+)*")
                .Select(match => match.Value)
                .ToHashSet(StringComparer.Ordinal);

            Assert.Contains("check-doc-keys", words);
            Assert.Contains("unique", words);
            Assert.Contains("non-empty", words);
            Assert.Contains("canonical", words);
            Assert.Contains("documentation", words);
            Assert.Contains("key", words);

            Assert.Contains("add-error", words);
            Assert.Contains("first", words);
            Assert.Contains("available", words);

            Assert.Contains("set-documentation-key", words);
            Assert.Contains("value", words);
        }
        finally
        {
            AnsiConsole.Console = originalConsole;
        }
    }
}
