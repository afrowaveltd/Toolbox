using System.Text.Json;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

[Collection(ConsoleOutputTestCollection.Name)]
public sealed class CheckDocKeysFailureJsonTests
{
    [Fact]
    public async Task ExecuteAsync_WithMissingDirectoryAndJson_WritesInvalidValidationIssue()
    {
        string missingPath = Path.Combine(
            Path.GetTempPath(),
            "WhenItFailsSetterTests",
            Guid.NewGuid().ToString("N"));

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "check-doc-keys",
            missingPath,
            "--json"
        ]);

        Assert.Equal(2, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement data = document.RootElement.GetProperty("data");
        JsonElement validation = data.GetProperty("validation");

        Assert.False(validation.GetProperty("isValid").GetBoolean());
        JsonElement issue = Assert.Single(
            validation.GetProperty("issues").EnumerateArray());
        Assert.Equal("CheckDocKeysFailed", issue.GetProperty("code").GetString());
        Assert.False(string.IsNullOrWhiteSpace(issue.GetProperty("message").GetString()));
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
