using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'demo' command: shows a sample WhenItFails validation result.
/// </summary>
internal static class DemoCommand
{
    /// <summary>
    /// Executes the demo command.
    /// </summary>
    /// <returns>Exit code: always 0.</returns>
    public static int Execute()
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

        ConsoleValidationResultShow validationResultShow = new();

        validationResultShow.Show(
            validationResult,
            new ConsoleShowOptions
            {
                SourcePath = "Jsons/WhenItFails/errors.json"
            });

        return 0;
    }
}
