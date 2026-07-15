using System.Globalization;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'explain-profile' command.
/// </summary>
internal static class ExplainProfileCommand
{
    private const string Usage =
        "explain-profile <path> <profile-name|display-name> [--plain]";

    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            CommandInputError.Show(
                "MissingExplainProfilePath",
                "The explain-profile command requires a project root or Jsons/WhenItFails directory path.",
                Usage);
            return 1;
        }

        if (args.Length < 3 || string.IsNullOrWhiteSpace(args[2]))
        {
            CommandInputError.Show(
                "MissingExplainProfileName",
                "The explain-profile command requires a profile name or display name.",
                Usage);
            return 1;
        }

        bool usePlainOutput = false;
        for (int index = 3; index < args.Length; index++)
        {
            if (string.Equals(args[index], "--plain", StringComparison.OrdinalIgnoreCase))
            {
                if (usePlainOutput)
                {
                    CommandInputError.Show(
                        "DuplicateExplainProfilePlainSwitch",
                        "The --plain switch may be specified only once.",
                        Usage);
                    return 1;
                }

                usePlainOutput = true;
                continue;
            }

            CommandInputError.Show(
                "InvalidExplainProfileArguments",
                $"Unknown explain-profile argument '{args[index]}'.",
                Usage);
            return 1;
        }

        string inputPath = args[1];
        string profileName = args[2];
        Response<ProfileExplanation> response =
            await new WhenItFailsProfileExplainer().ExplainAsync(inputPath, profileName);

        if (!response.IsSuccess || response.Data is null)
        {
            ShowFailure(response, inputPath, profileName);
            return 2;
        }

        if (usePlainOutput)
        {
            ShowPlain(response.Data);
        }
        else
        {
            Show(response.Data);
        }

        return 0;
    }

    private static void Show(ProfileExplanation explanation)
    {
        AnsiConsole.MarkupLine(
            "[bold]Profile:[/] {0} [grey]({1})[/]",
            Markup.Escape(explanation.ProfileName),
            Markup.Escape(explanation.DisplayName));
        AnsiConsole.MarkupLine(
            "[bold]Included:[/] {0}  [bold]Excluded:[/] {1}  [bold]Total:[/] {2}",
            explanation.IncludedErrors,
            explanation.ExcludedErrors,
            explanation.TotalErrors);

        Table table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("State")
            .AddColumn("Code")
            .AddColumn("Id")
            .AddColumn("Name")
            .AddColumn("Include reasons")
            .AddColumn("Exclusion reasons");

        foreach (ProfileErrorExplanation error in explanation.Errors)
        {
            table.AddRow(
                error.IsIncluded ? "[green]Included[/]" : "[red]Excluded[/]",
                error.Code.ToString(CultureInfo.InvariantCulture),
                Markup.Escape(error.Id),
                Markup.Escape(error.Name),
                Markup.Escape(string.Join(", ", error.IncludeReasons)),
                Markup.Escape(string.Join(", ", error.ExclusionReasons)));
        }

        AnsiConsole.Write(table);
    }

    private static void ShowPlain(ProfileExplanation explanation)
    {
        foreach (ProfileErrorExplanation error in explanation.Errors)
        {
            string[] fields =
            [
                explanation.ProfileName,
                error.IsIncluded ? "INCLUDED" : "EXCLUDED",
                error.Code.ToString(CultureInfo.InvariantCulture),
                error.Id,
                error.Name,
                string.Join('|', error.IncludeReasons),
                string.Join('|', error.ExclusionReasons)
            ];

            Console.WriteLine(string.Join('\t', fields));
        }
    }

    private static void ShowFailure(
        Response<ProfileExplanation> response,
        string inputPath,
        string profileName)
    {
        ErrorCatalogValidationResult result = new();
        string failureCode = response.Issues.Count > 0
            ? response.Issues[0].Code
            : "ExplainProfileFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The profile could not be explained."
            : response.Message;

        result.AddError(failureCode, failureMessage, profileName);
        new ConsoleValidationResultShow().Show(
            result,
            new ConsoleShowOptions { SourcePath = inputPath });
    }
}
