using Afrowave.Toolbox.WhenItFails.Definitions;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Views;

/// <summary>
/// Renders a single error definition detail in both rich and plain text formats.
/// </summary>
internal static class DetailsView
{
    /// <summary>
    /// Shows error definition details using Spectre.Console rich formatting.
    /// </summary>
    /// <param name="summary">The workspace summary.</param>
    /// <param name="errorDefinition">The error definition to display.</param>
    public static void Show(
        WhenItFailsWorkspaceSummary summary,
        ErrorDefinition errorDefinition)
    {
        AnsiConsole.Write(
            new Rule("[bold aqua]WhenItFails Error Detail[/]")
                .RuleStyle("aqua"));

        AnsiConsole.MarkupLine(
            "[bold]Workspace:[/] {0}",
            Markup.Escape(summary.DisplayPath));

        AnsiConsole.WriteLine();

        Table detailTable = new Table();

        ConsoleTableViewHelper.ConfigureWideTable(
            detailTable,
            Color.Aqua);

        detailTable.AddColumn(new TableColumn("Field").NoWrap());
        detailTable.AddColumn("Value");

        detailTable.AddRow("Code", errorDefinition.Code.ToString());
        detailTable.AddRow("Id", ConsoleTableViewHelper.Escape(errorDefinition.Id));
        detailTable.AddRow("Name", ConsoleTableViewHelper.Escape(errorDefinition.Name));
        detailTable.AddRow("Title", ConsoleTableViewHelper.Escape(errorDefinition.Title));
        detailTable.AddRow("Message", ConsoleTableViewHelper.Escape(errorDefinition.Message));
        detailTable.AddRow("Severity", ConsoleTableViewHelper.Escape(errorDefinition.DefaultSeverity));
        detailTable.AddRow("Owner", ConsoleTableViewHelper.Escape(errorDefinition.Owner));
        detailTable.AddRow("Code prefix", ConsoleTableViewHelper.Escape(errorDefinition.CodePrefix));
        detailTable.AddRow("Code group", ConsoleTableViewHelper.Escape(errorDefinition.CodeGroup));
        detailTable.AddRow("Primary category", ConsoleTableViewHelper.Escape(errorDefinition.PrimaryCategory));
        detailTable.AddRow("Categories", Markup.Escape(ConsoleTableViewHelper.JoinPlainValues(errorDefinition.Categories)));
        detailTable.AddRow("Subcategories", Markup.Escape(ConsoleTableViewHelper.JoinPlainValues(errorDefinition.Subcategories)));
        detailTable.AddRow("Tags", Markup.Escape(ConsoleTableViewHelper.JoinPlainValues(errorDefinition.Tags)));
        detailTable.AddRow("Developer hint", ConsoleTableViewHelper.Escape(errorDefinition.DeveloperHint));
        detailTable.AddRow("Documentation key", ConsoleTableViewHelper.Escape(errorDefinition.DocumentationKey));

        AnsiConsole.Write(detailTable);
    }

    /// <summary>
    /// Shows error definition details in plain text format.
    /// </summary>
    /// <param name="summary">The workspace summary.</param>
    /// <param name="errorDefinition">The error definition to display.</param>
    public static void ShowPlain(
        WhenItFailsWorkspaceSummary summary,
        ErrorDefinition errorDefinition)
    {
        Console.WriteLine("WhenItFails Error Detail");
        Console.WriteLine($"Workspace: {summary.DisplayPath}");
        Console.WriteLine();

        Console.WriteLine($"Code: {errorDefinition.Code}");
        Console.WriteLine($"Id: {errorDefinition.Id}");
        Console.WriteLine($"Name: {errorDefinition.Name}");
        Console.WriteLine($"Title: {errorDefinition.Title}");
        Console.WriteLine($"Message: {errorDefinition.Message}");
        Console.WriteLine($"Severity: {errorDefinition.DefaultSeverity}");
        Console.WriteLine($"Owner: {errorDefinition.Owner}");
        Console.WriteLine($"Code prefix: {errorDefinition.CodePrefix}");
        Console.WriteLine($"Code group: {errorDefinition.CodeGroup}");
        Console.WriteLine($"Primary category: {errorDefinition.PrimaryCategory}");
        Console.WriteLine($"Categories: {ConsoleTableViewHelper.JoinPlainValues(errorDefinition.Categories)}");
        Console.WriteLine($"Subcategories: {ConsoleTableViewHelper.JoinPlainValues(errorDefinition.Subcategories)}");
        Console.WriteLine($"Tags: {ConsoleTableViewHelper.JoinPlainValues(errorDefinition.Tags)}");
        Console.WriteLine($"Developer hint: {errorDefinition.DeveloperHint ?? string.Empty}");
        Console.WriteLine($"Documentation key: {errorDefinition.DocumentationKey ?? string.Empty}");
    }
}
