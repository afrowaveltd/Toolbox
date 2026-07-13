using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Creates and renders validation errors caused by invalid command-line input.
/// </summary>
internal static class CommandInputError
{
    /// <summary>
    /// Creates one validation result describing invalid command-line input.
    /// </summary>
    public static ErrorCatalogValidationResult CreateResult(
        string code,
        string message,
        string path)
    {
        ErrorCatalogValidationResult result = new();
        result.AddError(
            code: code,
            message: message,
            path: path);

        return result;
    }

    /// <summary>
    /// Renders one command-line validation error.
    /// </summary>
    public static void Show(
        string code,
        string message,
        string path)
    {
        ErrorCatalogValidationResult result = CreateResult(code, message, path);

        new ConsoleValidationResultShow().Show(
            result,
            new ConsoleShowOptions
            {
                SourcePath = "command line"
            });
    }
}
