using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Views;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'show-owner' command: shows one owner from a validated WhenItFails workspace.
/// </summary>
internal static class ShowOwnerCommand
{
    private const string Usage =
        "show-owner <path> <owner-name|alias> [--plain|--json]";

    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            CommandInputError.Show(
                "MissingShowOwnerPath",
                "The show-owner command requires a project root or Jsons/WhenItFails directory path.",
                Usage);
            return 1;
        }

        if (args.Length < 3 || string.IsNullOrWhiteSpace(args[2]))
        {
            CommandInputError.Show(
                "MissingShowOwnerName",
                "The show-owner command requires an owner name or alias.",
                Usage);
            return 1;
        }

        if (!TryParseOptions(args, out bool usePlainOutput, out bool useJsonOutput))
        {
            CommandInputError.Show(
                "InvalidShowOwnerOutputArguments",
                "The --plain and --json switches are mutually exclusive and may be specified only once.",
                Usage);
            return 1;
        }

        string inputPath = args[1];
        string ownerNameOrAlias = args[2];
        WhenItFailsWorkspaceValidationOutcome validationOutcome =
            await new WhenItFailsWorkspaceValidator().ValidateAsync(inputPath);

        if (!validationOutcome.ValidationResult.IsValid)
        {
            if (useJsonOutput)
            {
                CommandJsonOutput.Write(
                    "show-owner",
                    new ShowOwnerResult(
                        Found: false,
                        Owner: null,
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
        ErrorOwnerDefinition? owner = FindOwner(summary, ownerNameOrAlias);

        if (owner is null)
        {
            const string failureCode = "UnknownOwner";
            string failureMessage = $"The owner '{ownerNameOrAlias}' does not exist.";

            if (useJsonOutput)
            {
                CommandJsonOutput.Write(
                    "show-owner",
                    new ShowOwnerResult(
                        Found: false,
                        Owner: null,
                        FailureCode: failureCode,
                        FailureMessage: failureMessage,
                        Validation: null));
            }
            else
            {
                ErrorCatalogValidationResult result = new();
                result.AddError(failureCode, failureMessage, ownerNameOrAlias);
                new ConsoleValidationResultShow().Show(
                    result,
                    new ConsoleShowOptions { SourcePath = summary.DisplayPath });
            }

            return 2;
        }

        if (usePlainOutput)
        {
            OwnerView.ShowPlain(summary, owner);
        }
        else if (useJsonOutput)
        {
            CommandJsonOutput.Write(
                "show-owner",
                new ShowOwnerResult(
                    Found: true,
                    Owner: owner,
                    FailureCode: null,
                    FailureMessage: null,
                    Validation: null));
        }
        else
        {
            OwnerView.Show(summary, owner);
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

    public static ErrorOwnerDefinition? FindOwner(
        WhenItFailsWorkspaceSummary summary,
        string ownerNameOrAlias)
    {
        return summary.OwnerCatalog.Owners.FirstOrDefault(owner =>
            string.Equals(owner.Name, ownerNameOrAlias, StringComparison.OrdinalIgnoreCase)
            || string.Equals(owner.DisplayName, ownerNameOrAlias, StringComparison.OrdinalIgnoreCase)
            || owner.Aliases.Any(alias =>
                string.Equals(alias, ownerNameOrAlias, StringComparison.OrdinalIgnoreCase)));
    }

    private sealed record ShowOwnerResult(
        bool Found,
        ErrorOwnerDefinition? Owner,
        string? FailureCode,
        string? FailureMessage,
        ErrorCatalogValidationResult? Validation);
}
