using Afrowave.Toolbox.WhenItFails.Definitions;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Views;

/// <summary>
/// Renders one category in rich and plain text formats.
/// </summary>
internal static class CategoryView
{
    public static void Show(
        WhenItFailsWorkspaceSummary summary,
        ErrorCategoryDefinition category)
    {
        AnsiConsole.Write(
            new Rule("[bold aqua]WhenItFails Category[/]")
                .RuleStyle("aqua"));

        AnsiConsole.MarkupLine("[bold]Workspace:[/] {0}", Markup.Escape(summary.DisplayPath));
        AnsiConsole.MarkupLine("[bold]Name:[/] {0}", Markup.Escape(category.Name));
        AnsiConsole.MarkupLine("[bold]Display name:[/] {0}", Markup.Escape(category.DisplayName));

        if (!string.IsNullOrWhiteSpace(category.Description))
        {
            AnsiConsole.MarkupLine("[bold]Description:[/] {0}", Markup.Escape(category.Description));
        }

        AnsiConsole.WriteLine();

        Table table = new();
        ConsoleTableViewHelper.ConfigureWideTable(table, Color.Aqua);
        table.AddColumn(new TableColumn("Property").NoWrap());
        table.AddColumn("Values");
        AddListRow(table, "Parents", category.ParentCategories);
        AddListRow(table, "Aliases", category.Aliases);
        AddListRow(table, "Default tags", category.DefaultTags);
        AddMappingsRow(table, category.DefaultMappings);
        AnsiConsole.Write(table);
    }

    public static void ShowPlain(
        WhenItFailsWorkspaceSummary summary,
        ErrorCategoryDefinition category)
    {
        Console.WriteLine("WhenItFails Category");
        Console.WriteLine($"Workspace: {summary.DisplayPath}");
        Console.WriteLine($"Name: {category.Name}");
        Console.WriteLine($"Display name: {category.DisplayName}");
        Console.WriteLine($"Description: {category.Description ?? string.Empty}");
        Console.WriteLine($"Parents: {FormatValues(category.ParentCategories)}");
        Console.WriteLine($"Aliases: {FormatValues(category.Aliases)}");
        Console.WriteLine($"Default tags: {FormatValues(category.DefaultTags)}");
        Console.WriteLine($"Default mappings: {FormatMappings(category.DefaultMappings)}");
    }

    private static void AddListRow(Table table, string label, IReadOnlyCollection<string> values)
    {
        table.AddRow(
            ConsoleTableViewHelper.Escape(label),
            ConsoleTableViewHelper.Escape(FormatValues(values)));
    }

    private static void AddMappingsRow(Table table, IReadOnlyDictionary<string, string> mappings)
    {
        table.AddRow(
            "Default mappings",
            ConsoleTableViewHelper.Escape(FormatMappings(mappings)));
    }

    private static string FormatValues(IReadOnlyCollection<string> values)
    {
        return values.Count == 0
            ? "None"
            : string.Join(", ", values.OrderBy(value => value));
    }

    private static string FormatMappings(IReadOnlyDictionary<string, string> mappings)
    {
        return mappings.Count == 0
            ? "None"
            : string.Join(", ", mappings.OrderBy(mapping => mapping.Key).Select(mapping => $"{mapping.Key}={mapping.Value}"));
    }
}
