using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'validate' command: validates WhenItFails JSON files.
/// </summary>
internal static class ValidateCommand
{
    /// <summary>
    /// Executes the validate command.
    /// </summary>
    /// <param name="args">The full command-line arguments (args[0] is "validate").</param>
    /// <returns>Exit code: 0 on valid, 1 on missing path, 2 on validation errors.</returns>
    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            ErrorCatalogValidationResult missingPathResult = new();

            missingPathResult.AddError(
                code: "MissingValidatePath",
                message: "The validate command requires a project root or Jsons/WhenItFails directory path.",
                path: "validate <path>");

            new ConsoleValidationResultShow().Show(
                missingPathResult,
                new ConsoleShowOptions
                {
                    SourcePath = "command line"
                });

            return 1;
        }

        string inputPath = args[1];

        WhenItFailsWorkspaceValidator validator = new();

        WhenItFailsWorkspaceValidationOutcome outcome =
            await validator.ValidateAsync(inputPath);

        new ConsoleValidationResultShow().Show(
            outcome.ValidationResult,
            new ConsoleShowOptions
            {
                SourcePath = outcome.DisplayPath
            });

        return outcome.ValidationResult.IsValid
            ? 0
            : 2;
    }
}
