using Afrowave.Toolbox.WhenItFails.Definitions;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Views;

/// <summary>
/// Renders owner lists in rich and plain text formats.
/// </summary>
internal static class OwnersView
{
    public static void Show(WhenItFailsWorkspaceSummary summary)
    {
        AnsiConsole.Write(
            new Rule("[bold aqua]WhenItFails Owners[/]")
                .RuleStyle("aqua"));

        AnsiConsole.MarkupLine(
            "[bold]Workspace:[/] {0}",
            Markup.Escape(summary.DisplayPath));
        AnsiConsole.MarkupLine(
            "[bold]Owners:[/] {0}",
            summary.OwnerCount.ToString());
        AnsiConsole.WriteLine();

        Table table = new();
        ConsoleTableViewHelper.ConfigureWideTable(table, Color.Aqua);
        table.AddColumn(new TableColumn("Name").NoWrap());
        table.AddColumn("Display name");
        table.AddColumn(new TableColumn("Code range").NoWrap());
        table.AddColumn(new TableColumn("Built in").NoWrap());
        table.AddColumn("Aliases");
        table.AddColumn("Description");

        foreach (ErrorOwnerDefinition owner in summary.OwnerCatalog.Owners
            .OrderBy(owner => owner.CodeFrom)
            .ThenBy(owner => owner.Name))
        {
            table.AddRow(
                ConsoleTableViewHelper.Escape(owner.Name),
                ConsoleTableViewHelper.Escape(owner.DisplayName),
                $"{owner.CodeFrom}–{owner.CodeTo}",
                owner.IsBuiltIn ? "Yes" : "No",
                ConsoleTableViewHelper.Escape(FormatValues(owner.Aliases)),
                ConsoleTableViewHelper.Escape(owner.Description ?? string.Empty));
        }

        AnsiConsole.Write(table);
    }

    public static void ShowPlain(WhenItFailsWorkspaceSummary summary)
    {
        Console.WriteLine("WhenItFails Owners");
        Console.WriteLine($"Workspace: {summary.DisplayPath}");
        Console.WriteLine($"Owners: {summary.OwnerCount}");
        Console.WriteLine();
        Console.WriteLine("Name\tDisplayName\tCodeFrom\tCodeTo\tIsBuiltIn\tAliases\tDescription");

        foreach (ErrorOwnerDefinition owner in summary.OwnerCatalog.Owners
            .OrderBy(owner => owner.CodeFrom)
            .ThenBy(owner => owner.Name))
        {
            Console.WriteLine(
                string.Join(
                    "\t",
                    [
                        owner.Name,
                        owner.DisplayName,
                        owner.CodeFrom.ToString(),
                        owner.CodeTo.ToString(),
                        owner.IsBuiltIn.ToString(),
                        FormatValues(owner.Aliases),
                        owner.Description ?? string.Empty
                    ]));
        }
    }

    private static string FormatValues(IReadOnlyCollection<string> values)
    {
        return values.Count == 0
            ? "None"
            : string.Join(", ", values);
    }
}
