using Afrowave.Toolbox.WhenItFails.Definitions;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Views;

/// <summary>
/// Renders one code group in rich and plain text formats.
/// </summary>
internal static class CodeGroupView
{
    public static void Show(
        WhenItFailsWorkspaceSummary summary,
        ErrorCodeGroupDefinition codeGroup)
    {
        AnsiConsole.Write(
            new Rule("[bold aqua]WhenItFails Code Group[/]")
                .RuleStyle("aqua"));

        AnsiConsole.MarkupLine("[bold]Workspace:[/] {0}", Markup.Escape(summary.DisplayPath));
        AnsiConsole.MarkupLine("[bold]Name:[/] {0}", Markup.Escape(codeGroup.Name));
        AnsiConsole.MarkupLine("[bold]Display name:[/] {0}", Markup.Escape(codeGroup.DisplayName));
        AnsiConsole.MarkupLine("[bold]Code prefix:[/] {0}", Markup.Escape(codeGroup.CodePrefix));
        AnsiConsole.MarkupLine("[bold]Code range:[/] {0} - {1}", codeGroup.CodeFrom, codeGroup.CodeTo);

        if (!string.IsNullOrWhiteSpace(codeGroup.Description))
        {
            AnsiConsole.MarkupLine("[bold]Description:[/] {0}", Markup.Escape(codeGroup.Description));
        }

        AnsiConsole.WriteLine();

        Table table = new();
        ConsoleTableViewHelper.ConfigureWideTable(table, Color.Aqua);
        table.AddColumn(new TableColumn("Property").NoWrap());
        table.AddColumn("Values");
        table.AddRow("Default categories", ConsoleTableViewHelper.Escape(FormatValues(codeGroup.DefaultCategories)));
        table.AddRow("Default tags", ConsoleTableViewHelper.Escape(FormatValues(codeGroup.DefaultTags)));
        table.AddRow("Default mappings", ConsoleTableViewHelper.Escape(FormatMappings(codeGroup.DefaultMappings)));

        AnsiConsole.Write(table);
    }

    public static void ShowPlain(
        WhenItFailsWorkspaceSummary summary,
        ErrorCodeGroupDefinition codeGroup)
    {
        Console.WriteLine("WhenItFails Code Group");
        Console.WriteLine($"Workspace: {summary.DisplayPath}");
        Console.WriteLine($"Name: {codeGroup.Name}");
        Console.WriteLine($"Display name: {codeGroup.DisplayName}");
        Console.WriteLine($"Code prefix: {codeGroup.CodePrefix}");
        Console.WriteLine($"Code from: {codeGroup.CodeFrom}");
        Console.WriteLine($"Code to: {codeGroup.CodeTo}");
        Console.WriteLine($"Description: {codeGroup.Description ?? string.Empty}");
        Console.WriteLine($"Default categories: {FormatValues(codeGroup.DefaultCategories)}");
        Console.WriteLine($"Default tags: {FormatValues(codeGroup.DefaultTags)}");
        Console.WriteLine($"Default mappings: {FormatMappings(codeGroup.DefaultMappings)}");
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
            : string.Join(
                ", ",
                mappings
                    .OrderBy(mapping => mapping.Key)
                    .Select(mapping => $"{mapping.Key}={mapping.Value}"));
    }
}
