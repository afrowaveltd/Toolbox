using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Views;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'show-code-group' command: shows one code group from a validated WhenItFails workspace.
/// </summary>
internal static class ShowCodeGroupCommand
{
    public static async Task<int> ExecuteAsync(string[] args)
    {
        const string usage = "show-code-group <path> <group-name|prefix> [--plain]";

        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            ShowCommandInputError(
                "MissingShowCodeGroupPath",
                "The show-code-group command requires a project root or Jsons/WhenItFails directory path.",
                usage);
            return 1;
        }

        if (args.Length < 3 || string.IsNullOrWhiteSpace(args[2]))
        {
            ShowCommandInputError(
                "MissingShowCodeGroupName",
                "The show-code-group command requires a code group name or prefix.",
                usage);
            return 1;
        }

        if (!TryParseOptions(args, out bool usePlainOutput))
        {
            ShowCommandInputError(
                "InvalidShowCodeGroupArguments",
                "The show-code-group command accepts only a path, a code group name or prefix, and the optional --plain switch.",
                usage);
            return 1;
        }

        string inputPath = args[1];
        string groupNameOrPrefix = args[2];
        WhenItFailsWorkspaceValidator validator = new();
        WhenItFailsWorkspaceValidationOutcome validationOutcome = await validator.ValidateAsync(inputPath);

        if (!validationOutcome.ValidationResult.IsValid)
        {
            new ConsoleValidationResultShow().Show(
                validationOutcome.ValidationResult,
                new ConsoleShowOptions { SourcePath = validationOutcome.DisplayPath });
            return 2;
        }

        WhenItFailsWorkspaceSummary summary = await new WhenItFailsWorkspaceSummarizer().LoadAsync(inputPath);
        ErrorCodeGroupDefinition? codeGroup = FindCodeGroup(summary, groupNameOrPrefix);

        if (codeGroup is null)
        {
            ErrorCatalogValidationResult result = new();
            result.AddError(
                "UnknownCodeGroup",
                $"The code group '{groupNameOrPrefix}' does not exist.",
                groupNameOrPrefix);
            new ConsoleValidationResultShow().Show(
                result,
                new ConsoleShowOptions { SourcePath = summary.DisplayPath });
            return 2;
        }

        if (usePlainOutput)
        {
            CodeGroupView.ShowPlain(summary, codeGroup);
        }
        else
        {
            CodeGroupView.Show(summary, codeGroup);
        }

        return 0;
    }

    public static bool TryParseOptions(string[] args, out bool usePlainOutput)
    {
        return PlainOutputOptionParser.TryParse(
            args,
            3,
            out usePlainOutput);
    }

    public static ErrorCodeGroupDefinition? FindCodeGroup(
        WhenItFailsWorkspaceSummary summary,
        string groupNameOrPrefix)
    {
        return summary.CodeGroupCatalog.CodeGroups.FirstOrDefault(codeGroup =>
            string.Equals(codeGroup.Name, groupNameOrPrefix, StringComparison.OrdinalIgnoreCase)
            || string.Equals(codeGroup.DisplayName, groupNameOrPrefix, StringComparison.OrdinalIgnoreCase)
            || string.Equals(codeGroup.CodePrefix, groupNameOrPrefix, StringComparison.OrdinalIgnoreCase));
    }

    private static void ShowCommandInputError(string code, string message, string path)
    {
        ErrorCatalogValidationResult result = new();
        result.AddError(code, message, path);
        new ConsoleValidationResultShow().Show(
            result,
            new ConsoleShowOptions { SourcePath = "command line" });
    }
}
