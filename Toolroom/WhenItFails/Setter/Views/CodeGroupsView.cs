using Afrowave.Toolbox.WhenItFails.Definitions;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Views;

/// <summary>
/// Renders code group lists in rich and plain text formats.
/// </summary>
internal static class CodeGroupsView
{
    public static void Show(WhenItFailsWorkspaceSummary summary)
    {
        AnsiConsole.Write(
            new Rule("[bold aqua]WhenItFails Code Groups[/]")
                .RuleStyle("aqua"));

        AnsiConsole.MarkupLine(
            "[bold]Workspace:[/] {0}",
            Markup.Escape(summary.DisplayPath));
        AnsiConsole.MarkupLine(
            "[bold]Code groups:[/] {0}",
            summary.CodeGroupCount.ToString());
        AnsiConsole.WriteLine();

        Table table = new();
        ConsoleTableViewHelper.ConfigureWideTable(table, Color.Aqua);
        table.AddColumn(new TableColumn("Name").NoWrap());
        table.AddColumn("Display name");
        table.AddColumn(new TableColumn("Prefix").NoWrap());
        table.AddColumn(new TableColumn("Code range").NoWrap());
        table.AddColumn("Default categories");
        table.AddColumn("Default tags");
        table.AddColumn("Description");

        foreach (ErrorCodeGroupDefinition codeGroup in summary.CodeGroupCatalog.CodeGroups
            .OrderBy(codeGroup => codeGroup.CodeFrom)
            .ThenBy(codeGroup => codeGroup.Name))
        {
            table.AddRow(
                ConsoleTableViewHelper.Escape(codeGroup.Name),
                ConsoleTableViewHelper.Escape(codeGroup.DisplayName),
                ConsoleTableViewHelper.Escape(codeGroup.CodePrefix),
                $"{codeGroup.CodeFrom}–{codeGroup.CodeTo}",
                ConsoleTableViewHelper.Escape(FormatValues(codeGroup.DefaultCategories)),
                ConsoleTableViewHelper.Escape(FormatValues(codeGroup.DefaultTags)),
                ConsoleTableViewHelper.Escape(codeGroup.Description ?? string.Empty));
        }

        AnsiConsole.Write(table);
    }

    public static void ShowPlain(WhenItFailsWorkspaceSummary summary)
    {
        Console.WriteLine("WhenItFails Code Groups");
        Console.WriteLine($"Workspace: {summary.DisplayPath}");
        Console.WriteLine($"Code groups: {summary.CodeGroupCount}");
        Console.WriteLine();
        Console.WriteLine(
            "Name\tDisplayName\tCodePrefix\tCodeFrom\tCodeTo\tDefaultCategories\tDefaultTags\tDescription");

        foreach (ErrorCodeGroupDefinition codeGroup in summary.CodeGroupCatalog.CodeGroups
            .OrderBy(codeGroup => codeGroup.CodeFrom)
            .ThenBy(codeGroup => codeGroup.Name))
        {
            Console.WriteLine(
                string.Join(
                    "\t",
                    [
                        codeGroup.Name,
                        codeGroup.DisplayName,
                        codeGroup.CodePrefix,
                        codeGroup.CodeFrom.ToString(),
                        codeGroup.CodeTo.ToString(),
                        FormatValues(codeGroup.DefaultCategories),
                        FormatValues(codeGroup.DefaultTags),
                        codeGroup.Description ?? string.Empty
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
