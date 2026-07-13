using Afrowave.Toolbox.WhenItFails.Definitions;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Views;

/// <summary>
/// Renders one owner in rich and plain text formats.
/// </summary>
internal static class OwnerView
{
    public static void Show(
        WhenItFailsWorkspaceSummary summary,
        ErrorOwnerDefinition owner)
    {
        AnsiConsole.Write(
            new Rule("[bold aqua]WhenItFails Owner[/]")
                .RuleStyle("aqua"));

        AnsiConsole.MarkupLine("[bold]Workspace:[/] {0}", Markup.Escape(summary.DisplayPath));
        AnsiConsole.MarkupLine("[bold]Name:[/] {0}", Markup.Escape(owner.Name));
        AnsiConsole.MarkupLine("[bold]Display name:[/] {0}", Markup.Escape(owner.DisplayName));
        AnsiConsole.MarkupLine("[bold]Code range:[/] {0} - {1}", owner.CodeFrom, owner.CodeTo);
        AnsiConsole.MarkupLine("[bold]Built in:[/] {0}", owner.IsBuiltIn ? "Yes" : "No");

        if (!string.IsNullOrWhiteSpace(owner.Description))
        {
            AnsiConsole.MarkupLine("[bold]Description:[/] {0}", Markup.Escape(owner.Description));
        }

        AnsiConsole.WriteLine();

        Table table = new();
        ConsoleTableViewHelper.ConfigureWideTable(table, Color.Aqua);
        table.AddColumn(new TableColumn("Property").NoWrap());
        table.AddColumn("Values");
        table.AddRow("Aliases", ConsoleTableViewHelper.Escape(FormatValues(owner.Aliases)));
        table.AddRow("Default mappings", ConsoleTableViewHelper.Escape(FormatMappings(owner.DefaultMappings)));

        AnsiConsole.Write(table);
    }

    public static void ShowPlain(
        WhenItFailsWorkspaceSummary summary,
        ErrorOwnerDefinition owner)
    {
        Console.WriteLine("WhenItFails Owner");
        Console.WriteLine($"Workspace: {summary.DisplayPath}");
        Console.WriteLine($"Name: {owner.Name}");
        Console.WriteLine($"Display name: {owner.DisplayName}");
        Console.WriteLine($"Code from: {owner.CodeFrom}");
        Console.WriteLine($"Code to: {owner.CodeTo}");
        Console.WriteLine($"Built in: {owner.IsBuiltIn}");
        Console.WriteLine($"Description: {owner.Description ?? string.Empty}");
        Console.WriteLine($"Aliases: {FormatValues(owner.Aliases)}");
        Console.WriteLine($"Default mappings: {FormatMappings(owner.DefaultMappings)}");
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
