using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Models;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Views;

/// <summary>
/// Renders error definition lists in both rich and plain text formats.
/// </summary>
internal static class ErrorsView
{
    /// <summary>
    /// Shows error definitions using Spectre.Console rich formatting.
    /// </summary>
    /// <param name="summary">The workspace summary.</param>
    /// <param name="errors">The filtered error definitions to display.</param>
    /// <param name="errorListOptions">The active filter options.</param>
    public static void Show(
        WhenItFailsWorkspaceSummary summary,
        IReadOnlyCollection<ErrorDefinition> errors,
        ErrorListOptions errorListOptions)
    {
        AnsiConsole.Write(
            new Rule("[bold aqua]WhenItFails Error Definitions[/]")
                .RuleStyle("aqua"));

        AnsiConsole.MarkupLine(
            "[bold]Workspace:[/] {0}",
            Markup.Escape(summary.DisplayPath));

        AnsiConsole.MarkupLine(
            "[bold]Errors:[/] {0} shown from {1}",
            errors.Count.ToString(),
            summary.ErrorCount.ToString());

        ShowActiveErrorFilters(errorListOptions);

        AnsiConsole.WriteLine();

        Table errorTable = new Table();

        ConsoleTableViewHelper.ConfigureWideTable(
            errorTable,
            Color.Aqua);

        errorTable.AddColumn(new TableColumn("Code").NoWrap());
        errorTable.AddColumn(new TableColumn("Id").NoWrap());
        errorTable.AddColumn("Name");
        errorTable.AddColumn(new TableColumn("Area").NoWrap());
        errorTable.AddColumn(new TableColumn("Severity").NoWrap());
        errorTable.AddColumn("Title");

        foreach (ErrorDefinition errorDefinition in errors
            .OrderBy(errorDefinition => errorDefinition.Code)
            .ThenBy(errorDefinition => errorDefinition.Id))
        {
            string area = string.Join(
                " / ",
                [
                    errorDefinition.Owner,
                    errorDefinition.CodeGroup,
                    errorDefinition.PrimaryCategory
                ]);

            errorTable.AddRow(
                errorDefinition.Code.ToString(),
                ConsoleTableViewHelper.Escape(errorDefinition.Id),
                ConsoleTableViewHelper.Escape(errorDefinition.Name),
                ConsoleTableViewHelper.Escape(area),
                ConsoleTableViewHelper.Escape(errorDefinition.DefaultSeverity),
                ConsoleTableViewHelper.Escape(errorDefinition.Title));
        }

        AnsiConsole.Write(errorTable);
    }

    /// <summary>
    /// Shows error definitions in plain tab-separated text format.
    /// </summary>
    /// <param name="summary">The workspace summary.</param>
    /// <param name="errors">The filtered error definitions to display.</param>
    /// <param name="errorListOptions">The active filter options.</param>
    public static void ShowPlain(
        WhenItFailsWorkspaceSummary summary,
        IReadOnlyCollection<ErrorDefinition> errors,
        ErrorListOptions errorListOptions)
    {
        Console.WriteLine("WhenItFails Error Definitions");
        Console.WriteLine($"Workspace: {summary.DisplayPath}");
        Console.WriteLine($"Errors: {errors.Count} shown from {summary.ErrorCount}");

        string activeFilters = CreateActiveFiltersText(errorListOptions);

        if (!string.IsNullOrWhiteSpace(activeFilters))
        {
            Console.WriteLine($"Filters: {activeFilters}");
        }

        Console.WriteLine();

        Console.WriteLine(
            "Code\tId\tName\tOwner\tGroup\tCategory\tSeverity\tTitle");

        foreach (ErrorDefinition errorDefinition in errors
            .OrderBy(errorDefinition => errorDefinition.Code)
            .ThenBy(errorDefinition => errorDefinition.Id))
        {
            Console.WriteLine(
                string.Join(
                    "\t",
                    [
                        errorDefinition.Code.ToString(),
                        errorDefinition.Id,
                        errorDefinition.Name,
                        errorDefinition.Owner,
                        errorDefinition.CodeGroup,
                        errorDefinition.PrimaryCategory,
                        errorDefinition.DefaultSeverity,
                        errorDefinition.Title
                    ]));
        }
    }

    /// <summary>
    /// Shows the active filter labels on the console.
    /// </summary>
    /// <param name="errorListOptions">The active filter options.</param>
    public static void ShowActiveErrorFilters(ErrorListOptions errorListOptions)
    {
        string activeFilters = CreateActiveFiltersText(errorListOptions);

        if (string.IsNullOrWhiteSpace(activeFilters))
        {
            return;
        }

        AnsiConsole.MarkupLine(
            "[bold]Filters:[/] {0}",
            Markup.Escape(activeFilters));
    }

    /// <summary>
    /// Creates a human-readable text describing the active filters.
    /// </summary>
    /// <param name="errorListOptions">The active filter options.</param>
    /// <returns>A comma-separated filter description, or an empty string if no filters are active.</returns>
    public static string CreateActiveFiltersText(ErrorListOptions errorListOptions)
    {
        List<string> filters = [];

        AddFilterText(filters, "owner", errorListOptions.Owner);
        AddFilterText(filters, "group", errorListOptions.CodeGroup);
        AddFilterText(filters, "category", errorListOptions.Category);
        AddFilterText(filters, "severity", errorListOptions.Severity);
        AddFilterText(filters, "profile", errorListOptions.Profile);
        AddFilterText(filters, "search", errorListOptions.SearchText);

        return string.Join(
            ", ",
            filters);
    }

    private static void AddFilterText(
        List<string> filters,
        string name,
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        filters.Add($"{name}={value}");
    }
}
