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
    public static async Task<int> ExecuteAsync(string[] args)
    {
        const string usage = "show-owner <path> <owner-name|alias> [--plain]";

        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            CommandInputError.Show(
                "MissingShowOwnerPath",
                "The show-owner command requires a project root or Jsons/WhenItFails directory path.",
                usage);
            return 1;
        }

        if (args.Length < 3 || string.IsNullOrWhiteSpace(args[2]))
        {
            CommandInputError.Show(
                "MissingShowOwnerName",
                "The show-owner command requires an owner name or alias.",
                usage);
            return 1;
        }

        if (!TryParseOptions(args, out bool usePlainOutput))
        {
            CommandInputError.Show(
                "InvalidShowOwnerArguments",
                "The show-owner command accepts only a path, an owner name or alias, and the optional --plain switch.",
                usage);
            return 1;
        }

        string inputPath = args[1];
        string ownerNameOrAlias = args[2];
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
        ErrorOwnerDefinition? owner = FindOwner(summary, ownerNameOrAlias);

        if (owner is null)
        {
            ErrorCatalogValidationResult result = new();
            result.AddError(
                "UnknownOwner",
                $"The owner '{ownerNameOrAlias}' does not exist.",
                ownerNameOrAlias);
            new ConsoleValidationResultShow().Show(
                result,
                new ConsoleShowOptions { SourcePath = summary.DisplayPath });
            return 2;
        }

        if (usePlainOutput)
        {
            OwnerView.ShowPlain(summary, owner);
        }
        else
        {
            OwnerView.Show(summary, owner);
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
}
