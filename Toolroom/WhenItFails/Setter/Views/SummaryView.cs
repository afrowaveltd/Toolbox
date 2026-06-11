using Afrowave.Toolbox.WhenItFails.Definitions;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Views;

/// <summary>
/// Renders the workspace summary view with catalog overviews and tables.
/// </summary>
internal static class SummaryView
{
    /// <summary>
    /// Shows the full workspace summary.
    /// </summary>
    /// <param name="summary">The workspace summary to display.</param>
    public static void Show(WhenItFailsWorkspaceSummary summary)
    {
        AnsiConsole.Write(
            new Rule("[bold aqua]WhenItFails Workspace Summary[/]")
                .RuleStyle("aqua"));

        AnsiConsole.MarkupLine(
            "[bold]Workspace:[/] {0}",
            Markup.Escape(summary.DisplayPath));

        AnsiConsole.MarkupLine(
            "[bold]Directory:[/] {0}",
            Markup.Escape(summary.PackageDirectoryPath));

        AnsiConsole.WriteLine();

        ShowCatalogOverview(summary);
        ShowOwnerTable(summary.OwnerCatalog.Owners);
        ShowCodeGroupTable(summary.CodeGroupCatalog.CodeGroups);
        ShowProfileTable(summary.ProfileCatalog.Profiles);
        ShowTopCategoryTable(summary.ErrorCatalog.Errors);
    }

    private static void ShowCatalogOverview(WhenItFailsWorkspaceSummary summary)
    {
        Table overviewTable = new Table();

        ConsoleTableViewHelper.ConfigureWideTable(
            overviewTable,
            Color.Aqua);

        overviewTable.AddColumn("Catalog");
        overviewTable.AddColumn("Name");
        overviewTable.AddColumn("Items");
        overviewTable.AddColumn("Language");

        overviewTable.AddRow(
            "Errors",
            ConsoleTableViewHelper.Escape(summary.ErrorCatalog.CatalogName),
            summary.ErrorCount.ToString(),
            ConsoleTableViewHelper.Escape(summary.ErrorCatalog.Language));

        overviewTable.AddRow(
            "Categories",
            ConsoleTableViewHelper.Escape(summary.CategoryCatalog.CatalogName),
            summary.CategoryCount.ToString(),
            ConsoleTableViewHelper.Escape(summary.CategoryCatalog.Language));

        overviewTable.AddRow(
            "Code groups",
            ConsoleTableViewHelper.Escape(summary.CodeGroupCatalog.CatalogName),
            summary.CodeGroupCount.ToString(),
            ConsoleTableViewHelper.Escape(summary.CodeGroupCatalog.Language));

        overviewTable.AddRow(
            "Owners",
            ConsoleTableViewHelper.Escape(summary.OwnerCatalog.CatalogName),
            summary.OwnerCount.ToString(),
            ConsoleTableViewHelper.Escape(summary.OwnerCatalog.Language));

        overviewTable.AddRow(
            "Profiles",
            ConsoleTableViewHelper.Escape(summary.ProfileCatalog.CatalogName),
            summary.ProfileCount.ToString(),
            ConsoleTableViewHelper.Escape(summary.ProfileCatalog.Language));

        AnsiConsole.Write(overviewTable);
    }

    private static void ShowOwnerTable(IReadOnlyCollection<ErrorOwnerDefinition> owners)
    {
        Table ownerTable = new Table();

        ConsoleTableViewHelper.ConfigureWideTable(
            ownerTable,
            Color.Blue);

        ownerTable.AddColumn("Owner");
        ownerTable.AddColumn("Display name");
        ownerTable.AddColumn("Range");
        ownerTable.AddColumn("Built-in");

        foreach (ErrorOwnerDefinition owner in owners.OrderBy(owner => owner.CodeFrom))
        {
            ownerTable.AddRow(
                ConsoleTableViewHelper.Escape(owner.Name),
                ConsoleTableViewHelper.Escape(owner.DisplayName),
                $"{owner.CodeFrom} - {owner.CodeTo}",
                owner.IsBuiltIn ? "[green]yes[/]" : "[grey]no[/]");
        }

        AnsiConsole.Write(ownerTable);
    }

    private static void ShowCodeGroupTable(IReadOnlyCollection<ErrorCodeGroupDefinition> codeGroups)
    {
        Table codeGroupTable = new Table();

        ConsoleTableViewHelper.ConfigureWideTable(
            codeGroupTable,
            Color.Green);

        codeGroupTable.AddColumn("Code group");
        codeGroupTable.AddColumn("Prefix");
        codeGroupTable.AddColumn("Display name");
        codeGroupTable.AddColumn("Range");

        foreach (ErrorCodeGroupDefinition codeGroup in codeGroups.OrderBy(codeGroup => codeGroup.CodeFrom))
        {
            codeGroupTable.AddRow(
                ConsoleTableViewHelper.Escape(codeGroup.Name),
                ConsoleTableViewHelper.Escape(codeGroup.CodePrefix),
                ConsoleTableViewHelper.Escape(codeGroup.DisplayName),
                $"{codeGroup.CodeFrom} - {codeGroup.CodeTo}");
        }

        AnsiConsole.Write(codeGroupTable);
    }

    private static void ShowProfileTable(IReadOnlyCollection<ErrorProfileDefinition> profiles)
    {
        Table profileTable = new Table();

        ConsoleTableViewHelper.ConfigureWideTable(
            profileTable,
            Color.Yellow);

        profileTable.AddColumn("Profile");
        profileTable.AddColumn("Display name");
        profileTable.AddColumn("Owners");
        profileTable.AddColumn("Code groups");
        profileTable.AddColumn("Categories");

        foreach (ErrorProfileDefinition profile in profiles.OrderBy(profile => profile.Name))
        {
            profileTable.AddRow(
                ConsoleTableViewHelper.Escape(profile.Name),
                ConsoleTableViewHelper.Escape(profile.DisplayName),
                ConsoleTableViewHelper.JoinValues(profile.IncludeOwners),
                ConsoleTableViewHelper.JoinValues(profile.IncludeCodeGroups),
                ConsoleTableViewHelper.JoinValues(profile.IncludeCategories));
        }

        AnsiConsole.Write(profileTable);
    }

    private static void ShowTopCategoryTable(IReadOnlyCollection<ErrorDefinition> errors)
    {
        Table categoryTable = new Table();

        ConsoleTableViewHelper.ConfigureWideTable(
            categoryTable,
            Color.Purple);

        categoryTable.AddColumn("Primary category");
        categoryTable.AddColumn("Errors");

        IEnumerable<IGrouping<string, ErrorDefinition>> groupedErrors =
            errors
                .GroupBy(error => string.IsNullOrWhiteSpace(error.PrimaryCategory)
                    ? "(empty)"
                    : error.PrimaryCategory)
                .OrderByDescending(group => group.Count())
                .ThenBy(group => group.Key);

        foreach (IGrouping<string, ErrorDefinition> group in groupedErrors)
        {
            categoryTable.AddRow(
                ConsoleTableViewHelper.Escape(group.Key),
                group.Count().ToString());
        }

        AnsiConsole.Write(categoryTable);
    }
}
