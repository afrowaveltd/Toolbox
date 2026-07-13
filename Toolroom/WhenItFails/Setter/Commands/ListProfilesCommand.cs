using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.WhenItFails.Validation;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Views;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'list-profiles' command: lists profiles from a validated WhenItFails workspace.
/// </summary>
internal static class ListProfilesCommand
{
    /// <summary>
    /// Executes the list-profiles command.
    /// </summary>
    /// <param name="args">The full command-line arguments (args[0] is "list-profiles").</param>
    /// <returns>Exit code: 0 on success, 1 on invalid command input, 2 on validation errors.</returns>
    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            CommandInputError.Show(
                code: "MissingListProfilesPath",
                message: "The list-profiles command requires a project root or Jsons/WhenItFails directory path.",
                path: "list-profiles <path> [--plain]");

            return 1;
        }

        if (!TryParseOptions(args, out bool usePlainOutput))
        {
            CommandInputError.Show(
                code: "InvalidListProfilesArguments",
                message: "The list-profiles command accepts only a path and the optional --plain switch.",
                path: "list-profiles <path> [--plain]");

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

        WhenItFailsWorkspaceSummary summary =
            await summarizer.LoadAsync(inputPath);

        if (usePlainOutput)
        {
            ProfilesView.ShowPlain(summary);
        }
        else
        {
            ProfilesView.Show(summary);
        }

        return 0;
    }

    /// <summary>
    /// Parses the optional list-profiles switches.
    /// </summary>
    /// <param name="args">The full command-line arguments.</param>
    /// <param name="usePlainOutput">True when --plain was supplied.</param>
    /// <returns>True when all arguments are valid for this command.</returns>
    public static bool TryParseOptions(
        string[] args,
        out bool usePlainOutput)
    {
        return PlainOutputOptionParser.TryParse(
            args,
            optionStartIndex: 2,
            out usePlainOutput);
    }
}
