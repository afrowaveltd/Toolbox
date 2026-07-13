using Afrowave.Toolbox.WhenItFails.Definitions;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Views;

/// <summary>
/// Renders category lists in rich and plain text formats.
/// </summary>
internal static class CategoriesView
{
    public static void Show(WhenItFailsWorkspaceSummary summary)
    {
        AnsiConsole.Write(
            new Rule("[bold aqua]WhenItFails Categories[/]")
                .RuleStyle("aqua"));

        AnsiConsole.MarkupLine(
            "[bold]Workspace:[/] {0}",
            Markup.Escape(summary.DisplayPath));
        AnsiConsole.MarkupLine(
            "[bold]Categories:[/] {0}",
            summary.CategoryCount.ToString());
        AnsiConsole.WriteLine();

        Table table = new();
        ConsoleTableViewHelper.ConfigureWideTable(table, Color.Aqua);
        table.AddColumn(new TableColumn("Name").NoWrap());
        table.AddColumn("Display name");
        table.AddColumn("Parents");
        table.AddColumn("Aliases");
        table.AddColumn("Default tags");
        table.AddColumn("Description");

        foreach (ErrorCategoryDefinition category in summary.CategoryCatalog.Categories
            .OrderBy(category => category.Name))
        {
            table.AddRow(
                ConsoleTableViewHelper.Escape(category.Name),
                ConsoleTableViewHelper.Escape(category.DisplayName),
                ConsoleTableViewHelper.Escape(FormatValues(category.ParentCategories)),
                ConsoleTableViewHelper.Escape(FormatValues(category.Aliases)),
                ConsoleTableViewHelper.Escape(FormatValues(category.DefaultTags)),
                ConsoleTableViewHelper.Escape(category.Description ?? string.Empty));
        }

        AnsiConsole.Write(table);
    }

    public static void ShowPlain(WhenItFailsWorkspaceSummary summary)
    {
        Console.WriteLine("WhenItFails Categories");
        Console.WriteLine($"Workspace: {summary.DisplayPath}");
        Console.WriteLine($"Categories: {summary.CategoryCount}");
        Console.WriteLine();
        Console.WriteLine("Name\tDisplayName\tParents\tAliases\tDefaultTags\tDescription");

        foreach (ErrorCategoryDefinition category in summary.CategoryCatalog.Categories
            .OrderBy(category => category.Name))
        {
            Console.WriteLine(
                string.Join(
                    "\t",
                    [
                        category.Name,
                        category.DisplayName,
                        FormatValues(category.ParentCategories),
                        FormatValues(category.Aliases),
                        FormatValues(category.DefaultTags),
                        category.Description ?? string.Empty
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
