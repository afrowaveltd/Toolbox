using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Views;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'list-profiles' command: lists profiles from a validated WhenItFails workspace.
/// </summary>
internal static class ListProfilesCommand
{
    private const string Usage = "list-profiles <path> [--plain|--json]";

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
                path: Usage);

            return 1;
        }

        if (!TryParseOptions(args, out bool usePlainOutput, out bool useJsonOutput))
        {
            ShowInvalidOutputArguments();
            return 1;
        }

        string inputPath = args[1];
        WhenItFailsWorkspaceValidationOutcome validationOutcome =
            await new WhenItFailsWorkspaceValidator().ValidateAsync(inputPath);

        if (!validationOutcome.ValidationResult.IsValid)
        {
            if (useJsonOutput)
            {
                CommandJsonOutput.Write(
                    "list-profiles",
                    new ListProfilesResult(
                        Loaded: false,
                        Profiles: null,
                        Validation: validationOutcome.ValidationResult));
            }
            else
            {
                ShowValidationFailure(validationOutcome);
            }

            return 2;
        }

        WhenItFailsWorkspaceSummary summary =
            await new WhenItFailsWorkspaceSummarizer().LoadAsync(inputPath);
        IReadOnlyList<ErrorProfileDefinition> profiles = summary.ProfileCatalog.Profiles;

        if (usePlainOutput)
        {
            ProfilesView.ShowPlain(summary);
        }
        else if (useJsonOutput)
        {
            CommandJsonOutput.Write(
                "list-profiles",
                new ListProfilesResult(
                    Loaded: true,
                    Profiles: profiles,
                    Validation: null));
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
    public static bool TryParseOptions(
        string[] args,
        out bool usePlainOutput,
        out bool useJsonOutput)
    {
        usePlainOutput = false;
        useJsonOutput = false;

        for (int index = 2; index < args.Length; index++)
        {
            if (string.Equals(args[index], "--plain", StringComparison.OrdinalIgnoreCase))
            {
                if (usePlainOutput || useJsonOutput)
                {
                    return false;
                }

                usePlainOutput = true;
                continue;
            }

            if (string.Equals(args[index], "--json", StringComparison.OrdinalIgnoreCase))
            {
                if (useJsonOutput || usePlainOutput)
                {
                    return false;
                }

                useJsonOutput = true;
                continue;
            }

            return false;
        }

        return true;
    }

    /// <summary>
    /// Retains the original parser shape for callers interested only in plain output.
    /// </summary>
    public static bool TryParseOptions(
        string[] args,
        out bool usePlainOutput)
    {
        return TryParseOptions(args, out usePlainOutput, out _);
    }

    private static void ShowInvalidOutputArguments()
    {
        CommandInputError.Show(
            code: "InvalidListProfilesOutputArguments",
            message: "The --plain and --json switches are mutually exclusive and may be specified only once.",
            path: Usage);
    }

    private static void ShowValidationFailure(
        WhenItFailsWorkspaceValidationOutcome validationOutcome)
    {
        new ConsoleValidationResultShow().Show(
            validationOutcome.ValidationResult,
            new ConsoleShowOptions
            {
                SourcePath = validationOutcome.DisplayPath
            });
    }

    private sealed record ListProfilesResult(
        bool Loaded,
        IReadOnlyList<ErrorProfileDefinition>? Profiles,
        ErrorCatalogValidationResult? Validation);
}
