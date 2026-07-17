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
    private const string Usage =
        "show-code-group <path> <group-name|prefix> [--plain|--json]";

    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            CommandInputError.Show(
                "MissingShowCodeGroupPath",
                "The show-code-group command requires a project root or Jsons/WhenItFails directory path.",
                Usage);
            return 1;
        }

        if (args.Length < 3 || string.IsNullOrWhiteSpace(args[2]))
        {
            CommandInputError.Show(
                "MissingShowCodeGroupName",
                "The show-code-group command requires a code group name or prefix.",
                Usage);
            return 1;
        }

        if (!TryParseOptions(args, out bool usePlainOutput, out bool useJsonOutput))
        {
            CommandInputError.Show(
                "InvalidShowCodeGroupOutputArguments",
                "The --plain and --json switches are mutually exclusive and may be specified only once.",
                Usage);
            return 1;
        }

        string inputPath = args[1];
        string groupNameOrPrefix = args[2];
        WhenItFailsWorkspaceValidationOutcome validationOutcome =
            await new WhenItFailsWorkspaceValidator().ValidateAsync(inputPath);

        if (!validationOutcome.ValidationResult.IsValid)
        {
            if (useJsonOutput)
            {
                CommandJsonOutput.Write(
                    "show-code-group",
                    new ShowCodeGroupResult(
                        Found: false,
                        CodeGroup: null,
                        FailureCode: null,
                        FailureMessage: null,
                        Validation: validationOutcome.ValidationResult));
            }
            else
            {
                new ConsoleValidationResultShow().Show(
                    validationOutcome.ValidationResult,
                    new ConsoleShowOptions { SourcePath = validationOutcome.DisplayPath });
            }

            return 2;
        }

        WhenItFailsWorkspaceSummary summary =
            await new WhenItFailsWorkspaceSummarizer().LoadAsync(inputPath);
        ErrorCodeGroupDefinition? codeGroup = FindCodeGroup(summary, groupNameOrPrefix);

        if (codeGroup is null)
        {
            const string failureCode = "UnknownCodeGroup";
            string failureMessage = $"The code group '{groupNameOrPrefix}' does not exist.";

            if (useJsonOutput)
            {
                CommandJsonOutput.Write(
                    "show-code-group",
                    new ShowCodeGroupResult(
                        Found: false,
                        CodeGroup: null,
                        FailureCode: failureCode,
                        FailureMessage: failureMessage,
                        Validation: null));
            }
            else
            {
                ErrorCatalogValidationResult result = new();
                result.AddError(failureCode, failureMessage, groupNameOrPrefix);
                new ConsoleValidationResultShow().Show(
                    result,
                    new ConsoleShowOptions { SourcePath = summary.DisplayPath });
            }

            return 2;
        }

        if (usePlainOutput)
        {
            CodeGroupView.ShowPlain(summary, codeGroup);
        }
        else if (useJsonOutput)
        {
            CommandJsonOutput.Write(
                "show-code-group",
                new ShowCodeGroupResult(
                    Found: true,
                    CodeGroup: codeGroup,
                    FailureCode: null,
                    FailureMessage: null,
                    Validation: null));
        }
        else
        {
            CodeGroupView.Show(summary, codeGroup);
        }

        return 0;
    }

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

    public static bool TryParseOptions(string[] args, out bool usePlainOutput)
    {
        return TryParseOptions(args, out usePlainOutput, out _);
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

    private sealed record ShowCodeGroupResult(
        bool Found,
        ErrorCodeGroupDefinition? CodeGroup,
        string? FailureCode,
        string? FailureMessage,
        ErrorCatalogValidationResult? Validation);
}
