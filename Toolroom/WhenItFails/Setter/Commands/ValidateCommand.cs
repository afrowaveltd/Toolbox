using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'validate' command: validates WhenItFails JSON files.
/// </summary>
internal static class ValidateCommand
{
    private const string Usage = "validate <path> [--json]";

    /// <summary>
    /// Executes the validate command.
    /// </summary>
    /// <param name="args">The full command-line arguments (args[0] is "validate").</param>
    /// <returns>Exit code: 0 on valid, 1 on invalid arguments, 2 on validation errors.</returns>
    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            CommandInputError.Show(
                "MissingValidatePath",
                "The validate command requires a project root or Jsons/WhenItFails directory path.",
                Usage);
            return 1;
        }

        if (!TryParseOptions(args, out bool useJsonOutput))
        {
            CommandInputError.Show(
                "InvalidValidateArguments",
                "The validate command accepts only a path and the optional --json switch, which may be specified only once.",
                Usage);
            return 1;
        }

        string inputPath = args[1];
        WhenItFailsWorkspaceValidationOutcome outcome =
            await new WhenItFailsWorkspaceValidator().ValidateAsync(inputPath);

        if (useJsonOutput)
        {
            CommandJsonOutput.Write(
                "validate",
                new ValidateResult(
                    outcome.ValidationResult.IsValid,
                    outcome.DisplayPath,
                    outcome.ValidationResult));
        }
        else
        {
            new ConsoleValidationResultShow().Show(
                outcome.ValidationResult,
                new ConsoleShowOptions
                {
                    SourcePath = outcome.DisplayPath
                });
        }

        return outcome.ValidationResult.IsValid
            ? 0
            : 2;
    }

    public static bool TryParseOptions(string[] args, out bool useJsonOutput)
    {
        useJsonOutput = false;

        for (int index = 2; index < args.Length; index++)
        {
            if (!string.Equals(args[index], "--json", StringComparison.OrdinalIgnoreCase)
                || useJsonOutput)
            {
                return false;
            }

            useJsonOutput = true;
        }

        return true;
    }

    private sealed record ValidateResult(
        bool IsValid,
        string DisplayPath,
        ErrorCatalogValidationResult Validation);
}
