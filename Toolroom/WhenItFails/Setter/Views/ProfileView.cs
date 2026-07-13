using Afrowave.Toolbox.WhenItFails.Definitions;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Views;

/// <summary>
/// Renders one profile in rich and plain text formats.
/// </summary>
internal static class ProfileView
{
    /// <summary>
    /// Shows one profile using Spectre.Console rich formatting.
    /// </summary>
    public static void Show(
        WhenItFailsWorkspaceSummary summary,
        ErrorProfileDefinition profile)
    {
        AnsiConsole.Write(
            new Rule("[bold aqua]WhenItFails Profile[/]")
                .RuleStyle("aqua"));

        AnsiConsole.MarkupLine(
            "[bold]Workspace:[/] {0}",
            Markup.Escape(summary.DisplayPath));
        AnsiConsole.MarkupLine(
            "[bold]Name:[/] {0}",
            Markup.Escape(profile.Name));
        AnsiConsole.MarkupLine(
            "[bold]Display name:[/] {0}",
            Markup.Escape(profile.DisplayName));
        AnsiConsole.MarkupLine(
            "[bold]Source:[/] {0}",
            Markup.Escape(profile.Source));

        if (!string.IsNullOrWhiteSpace(profile.Description))
        {
            AnsiConsole.MarkupLine(
                "[bold]Description:[/] {0}",
                Markup.Escape(profile.Description));
        }

        AnsiConsole.WriteLine();

        Table table = new Table();
        ConsoleTableViewHelper.ConfigureWideTable(table, Color.Aqua);
        table.AddColumn(new TableColumn("Selection").NoWrap());
        table.AddColumn("Values");

        AddListRow(table, "Owners", profile.IncludeOwners);
        AddListRow(table, "Code groups", profile.IncludeCodeGroups);
        AddListRow(table, "Categories", profile.IncludeCategories);
        AddListRow(table, "Subcategories", profile.IncludeSubcategories);
        AddListRow(table, "Include tags", profile.IncludeTags);
        AddListRow(table, "Include errors", profile.IncludeErrors);
        AddListRow(table, "Exclude tags", profile.ExcludeTags);
        AddListRow(table, "Exclude errors", profile.ExcludeErrors);
        AddMappingsRow(table, profile.DefaultMappings);

        AnsiConsole.Write(table);
    }

    /// <summary>
    /// Shows one profile as line-oriented plain text.
    /// </summary>
    public static void ShowPlain(
        WhenItFailsWorkspaceSummary summary,
        ErrorProfileDefinition profile)
    {
        Console.WriteLine("WhenItFails Profile");
        Console.WriteLine($"Workspace: {summary.DisplayPath}");
        Console.WriteLine($"Name: {profile.Name}");
        Console.WriteLine($"Display name: {profile.DisplayName}");
        Console.WriteLine($"Source: {profile.Source}");
        Console.WriteLine($"Description: {profile.Description ?? string.Empty}");
        Console.WriteLine($"Owners: {FormatValues(profile.IncludeOwners)}");
        Console.WriteLine($"Code groups: {FormatValues(profile.IncludeCodeGroups)}");
        Console.WriteLine($"Categories: {FormatValues(profile.IncludeCategories)}");
        Console.WriteLine($"Subcategories: {FormatValues(profile.IncludeSubcategories)}");
        Console.WriteLine($"Include tags: {FormatValues(profile.IncludeTags)}");
        Console.WriteLine($"Include errors: {FormatValues(profile.IncludeErrors)}");
        Console.WriteLine($"Exclude tags: {FormatValues(profile.ExcludeTags)}");
        Console.WriteLine($"Exclude errors: {FormatValues(profile.ExcludeErrors)}");
        Console.WriteLine($"Default mappings: {FormatMappings(profile.DefaultMappings)}");
    }

    private static void AddListRow(
        Table table,
        string label,
        IReadOnlyCollection<string> values)
    {
        table.AddRow(
            ConsoleTableViewHelper.Escape(label),
            ConsoleTableViewHelper.Escape(FormatValues(values)));
    }

    private static void AddMappingsRow(
        Table table,
        IReadOnlyDictionary<string, string> mappings)
    {
        table.AddRow(
            "Default mappings",
            ConsoleTableViewHelper.Escape(FormatMappings(mappings)));
    }

    private static string FormatValues(IReadOnlyCollection<string> values)
    {
        return values.Count == 0
            ? "All"
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
