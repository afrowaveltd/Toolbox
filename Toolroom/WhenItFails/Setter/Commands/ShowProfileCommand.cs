using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Views;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'show-profile' command: shows one profile from a validated WhenItFails workspace.
/// </summary>
internal static class ShowProfileCommand
{
    /// <summary>
    /// Executes the show-profile command.
    /// </summary>
    /// <param name="args">The full command-line arguments (args[0] is "show-profile").</param>
    /// <returns>Exit code: 0 on success, 1 on invalid command input, 2 on validation or lookup errors.</returns>
    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            ShowCommandInputError(
                code: "MissingShowProfilePath",
                message: "The show-profile command requires a project root or Jsons/WhenItFails directory path.",
                path: "show-profile <path> <profile-name> [--plain]");

            return 1;
        }

        if (args.Length < 3 || string.IsNullOrWhiteSpace(args[2]))
        {
            ShowCommandInputError(
                code: "MissingShowProfileName",
                message: "The show-profile command requires a profile name.",
                path: "show-profile <path> <profile-name> [--plain]");

            return 1;
        }

        if (!TryParseOptions(args, out bool usePlainOutput))
        {
            ShowCommandInputError(
                code: "InvalidShowProfileArguments",
                message: "The show-profile command accepts only a path, a profile name, and the optional --plain switch.",
                path: "show-profile <path> <profile-name> [--plain]");

            return 1;
        }

        string inputPath = args[1];
        string profileName = args[2];

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

        ErrorProfileDefinition? profile = FindProfile(summary, profileName);

        if (profile is null)
        {
            ErrorCatalogValidationResult unknownProfileResult = new();

            unknownProfileResult.AddError(
                code: "UnknownProfile",
                message: $"The profile '{profileName}' does not exist.",
                path: profileName);

            new ConsoleValidationResultShow().Show(
                unknownProfileResult,
                new ConsoleShowOptions
                {
                    SourcePath = summary.DisplayPath
                });

            return 2;
        }

        if (usePlainOutput)
        {
            ProfileView.ShowPlain(summary, profile);
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
        out bool usePlainOutput)
    {
        usePlainOutput = false;

        for (int index = 3; index < args.Length; index++)
        {
            if (!string.Equals(
                    args[index],
                    "--plain",
                    StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (usePlainOutput)
            {
                return false;
            }

            usePlainOutput = true;
        }

        return true;
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

    private static void ShowCommandInputError(
        string code,
        string message,
        string path)
    {
        ErrorCatalogValidationResult validationResult = new();

        validationResult.AddError(
            code: code,
            message: message,
            path: path);

        new ConsoleValidationResultShow().Show(
            validationResult,
            new ConsoleShowOptions
            {
                SourcePath = "command line"
            });
    }
}
