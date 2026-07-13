using Afrowave.Toolbox.WhenItFails.Definitions;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Views;

/// <summary>
/// Renders profile lists in both rich and plain text formats.
/// </summary>
internal static class ProfilesView
{
    /// <summary>
    /// Shows profiles using Spectre.Console rich formatting.
    /// </summary>
    /// <param name="summary">The validated workspace summary.</param>
    public static void Show(WhenItFailsWorkspaceSummary summary)
    {
        AnsiConsole.Write(
            new Rule("[bold aqua]WhenItFails Profiles[/]")
                .RuleStyle("aqua"));

        AnsiConsole.MarkupLine(
            "[bold]Workspace:[/] {0}",
            Markup.Escape(summary.DisplayPath));

        AnsiConsole.MarkupLine(
            "[bold]Profiles:[/] {0}",
            summary.ProfileCount.ToString());

        AnsiConsole.WriteLine();

        Table profileTable = new Table();

        ConsoleTableViewHelper.ConfigureWideTable(
            profileTable,
            Color.Aqua);

        profileTable.AddColumn(new TableColumn("Name").NoWrap());
        profileTable.AddColumn("Display name");
        profileTable.AddColumn(new TableColumn("Source").NoWrap());
        profileTable.AddColumn("Owners");
        profileTable.AddColumn("Code groups");
        profileTable.AddColumn("Categories");

        foreach (ErrorProfileDefinition profile in summary.ProfileCatalog.Profiles
            .OrderBy(profile => profile.Name, StringComparer.OrdinalIgnoreCase))
        {
            profileTable.AddRow(
                ConsoleTableViewHelper.Escape(profile.Name),
                ConsoleTableViewHelper.Escape(profile.DisplayName),
                ConsoleTableViewHelper.Escape(profile.Source),
                ConsoleTableViewHelper.Escape(CreateSelectionText(profile.IncludeOwners)),
                ConsoleTableViewHelper.Escape(CreateSelectionText(profile.IncludeCodeGroups)),
                ConsoleTableViewHelper.Escape(CreateSelectionText(profile.IncludeCategories)));
        }

        AnsiConsole.Write(profileTable);
    }

    /// <summary>
    /// Shows profiles in plain tab-separated text format.
    /// </summary>
    /// <param name="summary">The validated workspace summary.</param>
    public static void ShowPlain(WhenItFailsWorkspaceSummary summary)
    {
        Console.WriteLine("WhenItFails Profiles");
        Console.WriteLine($"Workspace: {summary.DisplayPath}");
        Console.WriteLine($"Profiles: {summary.ProfileCount}");
        Console.WriteLine();
        Console.WriteLine("Name\tDisplayName\tSource\tOwners\tCodeGroups\tCategories");

        foreach (ErrorProfileDefinition profile in summary.ProfileCatalog.Profiles
            .OrderBy(profile => profile.Name, StringComparer.OrdinalIgnoreCase))
        {
            Console.WriteLine(
                string.Join(
                    "\t",
                    [
                        profile.Name,
                        profile.DisplayName,
                        profile.Source,
                        CreateSelectionText(profile.IncludeOwners),
                        CreateSelectionText(profile.IncludeCodeGroups),
                        CreateSelectionText(profile.IncludeCategories)
                    ]));
        }
    }

    /// <summary>
    /// Creates a compact representation of profile selections.
    /// </summary>
    /// <param name="values">The selected catalog values.</param>
    /// <returns>A comma-separated list, or "All" when the selection is empty.</returns>
    public static string CreateSelectionText(IReadOnlyCollection<string> values)
    {
        return values.Count == 0
            ? "All"
            : string.Join(", ", values);
    }
}
