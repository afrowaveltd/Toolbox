using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Views;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'show-profile' command: shows one profile from a validated WhenItFails workspace.
/// </summary>
internal static class ShowProfileCommand
{
    private const string Usage =
        "show-profile <path> <profile-name> [--plain|--json]";

    /// <summary>
    /// Executes the show-profile command.
    /// </summary>
    /// <param name="args">The full command-line arguments (args[0] is "show-profile").</param>
    /// <returns>Exit code: 0 on success, 1 on invalid command input, 2 on validation or lookup errors.</returns>
    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            CommandInputError.Show(
                code: "MissingShowProfilePath",
                message: "The show-profile command requires a project root or Jsons/WhenItFails directory path.",
                path: Usage);

            return 1;
        }

        if (args.Length < 3 || string.IsNullOrWhiteSpace(args[2]))
        {
            CommandInputError.Show(
                code: "MissingShowProfileName",
                message: "The show-profile command requires a profile name.",
                path: Usage);

            return 1;
        }

        if (!TryParseOptions(args, out bool usePlainOutput, out bool useJsonOutput))
        {
            ShowInvalidOutputArguments();
            return 1;
        }

        string inputPath = args[1];
        string profileName = args[2];
        WhenItFailsWorkspaceValidationOutcome validationOutcome =
            await new WhenItFailsWorkspaceValidator().ValidateAsync(inputPath);

        if (!validationOutcome.ValidationResult.IsValid)
        {
            if (useJsonOutput)
            {
                CommandJsonOutput.Write(
                    "show-profile",
                    new ShowProfileResult(
                        Loaded: false,
                        Profile: null,
                        Validation: validationOutcome.ValidationResult,
                        FailureCode: null,
                        FailureMessage: null));
            }
            else
            {
                ShowValidationFailure(validationOutcome);
            }

            return 2;
        }

        WhenItFailsWorkspaceSummary summary =
            await new WhenItFailsWorkspaceSummarizer().LoadAsync(inputPath);
        ErrorProfileDefinition? profile = FindProfile(summary, profileName);

        if (profile is null)
        {
            const string failureCode = "UnknownProfile";
            string failureMessage = $"The profile '{profileName}' does not exist.";

            if (useJsonOutput)
            {
                CommandJsonOutput.Write(
                    "show-profile",
                    new ShowProfileResult(
                        Loaded: true,
                        Profile: null,
                        Validation: null,
                        FailureCode: failureCode,
                        FailureMessage: failureMessage));
            }
            else
            {
                ErrorCatalogValidationResult unknownProfileResult = new();
                unknownProfileResult.AddError(
                    code: failureCode,
                    message: failureMessage,
                    path: profileName);
                new ConsoleValidationResultShow().Show(
                    unknownProfileResult,
                    new ConsoleShowOptions
                    {
                        SourcePath = summary.DisplayPath
                    });
            }

            return 2;
        }

        if (usePlainOutput)
        {
            ProfileView.ShowPlain(summary, profile);
        }
        else if (useJsonOutput)
        {
            CommandJsonOutput.Write(
                "show-profile",
                new ShowProfileResult(
                    Loaded: true,
                    Profile: profile,
                    Validation: null,
                    FailureCode: null,
                    FailureMessage: null));
        }
        else
        {
            ProfileView.Show(summary, profile);
        }

        return 0;
    }

    /// <summary>
    /// Parses the optional show-profile switches.
    /// </summary>
    public static bool TryParseOptions(
        string[] args,
        out bool usePlainOutput,
        out bool useJsonOutput)
    {
        usePlainOutput = false;
        useJsonOutput = false;

        for (int index = 3; index < args.Length; index++)
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

    /// <summary>
    /// Finds a profile by normalized name or display name.
    /// </summary>
    public static ErrorProfileDefinition? FindProfile(
        WhenItFailsWorkspaceSummary summary,
        string profileName)
    {
        return summary.ProfileCatalog.Profiles.FirstOrDefault(profile =>
            string.Equals(
                profile.Name,
                profileName,
                StringComparison.OrdinalIgnoreCase)
            || string.Equals(
                profile.DisplayName,
                profileName,
                StringComparison.OrdinalIgnoreCase));
    }

    private static void ShowInvalidOutputArguments()
    {
        CommandInputError.Show(
            code: "InvalidShowProfileOutputArguments",
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

    private sealed record ShowProfileResult(
        bool Loaded,
        ErrorProfileDefinition? Profile,
        ErrorCatalogValidationResult? Validation,
        string? FailureCode,
        string? FailureMessage);
}
