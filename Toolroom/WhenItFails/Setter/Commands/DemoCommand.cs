using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'demo' command: shows a sample WhenItFails validation result.
/// </summary>
internal static class DemoCommand
{
    private const string SourcePath = "Jsons/WhenItFails/errors.json";

    /// <summary>
    /// Executes the demo command without optional output switches.
    /// </summary>
    /// <returns>Exit code: 0 on success.</returns>
    public static int Execute()
    {
        return Execute(["demo"]);
    }

    /// <summary>
    /// Executes the demo command.
    /// </summary>
    /// <param name="args">The full command-line arguments.</param>
    /// <returns>Exit code: 0 on success, 1 on invalid arguments.</returns>
    public static int Execute(string[] args)
    {
        if (!TryParseOptions(args, out bool useJsonOutput))
        {
            CommandInputError.Show(
                "InvalidDemoArguments",
                "The demo command accepts only the optional --json switch, which may be specified once.",
                "demo [--json]");
            return 1;
        }

        ErrorCatalogValidationResult validationResult = CreateValidationResult();

        if (useJsonOutput)
        {
            CommandJsonOutput.Write(
                "demo",
                new DemoResult(SourcePath, validationResult));
        }
        else
        {
            new ConsoleValidationResultShow().Show(
                validationResult,
                new ConsoleShowOptions
                {
                    SourcePath = SourcePath
                });
        }

        return 0;
    }

    public static bool TryParseOptions(string[] args, out bool useJsonOutput)
    {
        useJsonOutput = false;

        for (int index = 1; index < args.Length; index++)
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

    private static ErrorCatalogValidationResult CreateValidationResult()
    {
        ErrorCatalogValidationResult validationResult = new();

        validationResult.AddError(
            code: "MissingCatalogId",
            message: "Catalog id is missing.",
            path: "catalogId");

        validationResult.AddWarning(
            code: "UnknownProfileIncludeOwner",
            message: "Profile references an unknown owner.",
            path: "profiles[0].includeOwners[0]");

        validationResult.AddInformation(
            code: "PrimaryCategoryNotListed",
            message: "Primary category is not listed in additional categories. This is allowed, but listing it can make filtering easier.",
            errorId: "CFG-0001",
            errorName: "MissingConfigurationValue",
            path: "errors[0].primaryCategory");

        return validationResult;
    }

    private sealed record DemoResult(
        string SourcePath,
        ErrorCatalogValidationResult Validation);
}
