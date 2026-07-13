using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.WhenItFails.Validation;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Views;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'list-code-groups' command: lists code groups from a validated WhenItFails workspace.
/// </summary>
internal static class ListCodeGroupsCommand
{
    /// <summary>
    /// Executes the list-code-groups command.
    /// </summary>
    /// <param name="args">The full command-line arguments (args[0] is "list-code-groups").</param>
    /// <returns>Exit code: 0 on success, 1 on invalid command input, 2 on validation errors.</returns>
    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            ShowCommandInputError(
                code: "MissingListCodeGroupsPath",
                message: "The list-code-groups command requires a project root or Jsons/WhenItFails directory path.",
                path: "list-code-groups <path> [--plain]");

            return 1;
        }

        if (!TryParseOptions(args, out bool usePlainOutput))
        {
            ShowCommandInputError(
                code: "InvalidListCodeGroupsArguments",
                message: "The list-code-groups command accepts only a path and the optional --plain switch.",
                path: "list-code-groups <path> [--plain]");

            return 1;
        }

        string inputPath = args[1];
        WhenItFailsWorkspaceValidator validator = new();
        WhenItFailsWorkspaceValidationOutcome validationOutcome =
            await validator.ValidateAsync(inputPath);

        if (!validationOutcome.ValidationResult.IsValid)
        {
            new ConsoleValidationResultShow().Show(
                validationOutcome.ValidationResult,
                new ConsoleShowOptions
                {
                    SourcePath = validationOutcome.DisplayPath
                });

            return 2;
        }

        WhenItFailsWorkspaceSummarizer summarizer = new();
        WhenItFailsWorkspaceSummary summary = await summarizer.LoadAsync(inputPath);

        if (usePlainOutput)
        {
            CodeGroupsView.ShowPlain(summary);
        }
        else
        {
            CodeGroupsView.Show(summary);
        }

        return 0;
    }

    public static bool TryParseOptions(string[] args, out bool usePlainOutput)
    {
        usePlainOutput = false;

        for (int index = 2; index < args.Length; index++)
        {
            if (!string.Equals(args[index], "--plain", StringComparison.OrdinalIgnoreCase)
                || usePlainOutput)
            {
                return false;
            }

            usePlainOutput = true;
        }

        return true;
    }

    private static void ShowCommandInputError(string code, string message, string path)
    {
        ErrorCatalogValidationResult validationResult = new();
        validationResult.AddError(code: code, message: message, path: path);

        new ConsoleValidationResultShow().Show(
            validationResult,
            new ConsoleShowOptions
            {
                SourcePath = "command line"
            });
    }
}
