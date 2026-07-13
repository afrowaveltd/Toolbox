using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.WhenItFails.Validation;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Views;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'list-owners' command: lists owners from a validated WhenItFails workspace.
/// </summary>
internal static class ListOwnersCommand
{
    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            ShowCommandInputError(
                code: "MissingListOwnersPath",
                message: "The list-owners command requires a project root or Jsons/WhenItFails directory path.",
                path: "list-owners <path> [--plain]");
            return 1;
        }

        if (!TryParseOptions(args, out bool usePlainOutput))
        {
            ShowCommandInputError(
                code: "InvalidListOwnersArguments",
                message: "The list-owners command accepts only a path and the optional --plain switch.",
                path: "list-owners <path> [--plain]");
            return 1;
        }

        string inputPath = args[1];
        WhenItFailsWorkspaceValidator validator = new();
        WhenItFailsWorkspaceValidationOutcome validationOutcome = await validator.ValidateAsync(inputPath);

        if (!validationOutcome.ValidationResult.IsValid)
        {
            new ConsoleValidationResultShow().Show(
                validationOutcome.ValidationResult,
                new ConsoleShowOptions { SourcePath = validationOutcome.DisplayPath });
            return 2;
        }

        WhenItFailsWorkspaceSummary summary = await new WhenItFailsWorkspaceSummarizer().LoadAsync(inputPath);

        if (usePlainOutput)
        {
            OwnersView.ShowPlain(summary);
        }
        else
        {
            OwnersView.Show(summary);
        }

        return 0;
    }

    public static bool TryParseOptions(string[] args, out bool usePlainOutput)
    {
        return PlainOutputOptionParser.TryParse(args, optionStartIndex: 2, out usePlainOutput);
    }

    private static void ShowCommandInputError(string code, string message, string path)
    {
        ErrorCatalogValidationResult validationResult = new();
        validationResult.AddError(code: code, message: message, path: path);
        new ConsoleValidationResultShow().Show(
            validationResult,
            new ConsoleShowOptions { SourcePath = "command line" });
    }
}
