using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Views;

/// <summary>
/// Renders the help screen showing all available commands.
/// </summary>
internal static class HelpView
{
    /// <summary>
    /// Shows the help screen with command descriptions.
    /// </summary>
    public static void Show()
    {
        Grid commandGrid = new Grid();
        commandGrid.AddColumn(new GridColumn().NoWrap());
        commandGrid.AddColumn();

        commandGrid.AddRow("[green]init[/] [grey]<path>[/]", "Create missing WhenItFails JSON files.");
        commandGrid.AddRow("[green]validate[/] [grey]<path>[/]", "Validate WhenItFails JSON files.");
        commandGrid.AddRow("[green]summary[/] [grey]<path>[/]", "Show a read-only summary of a WhenItFails JSON workspace.");
        commandGrid.AddRow("[green]inspect[/] [grey]<path>[/]", "Alias for summary.");
        commandGrid.AddRow(
            "[green]errors[/] [grey]<path>[/] [grey][[--plain]][/]",
            "List error definitions. Supports --owner, --group, --category, --severity, --profile, --search.");
        commandGrid.AddRow(
            "[green]list-profiles[/] [grey]<path>[/] [grey][[--plain]][/]",
            "List profiles from a validated WhenItFails workspace.");
        commandGrid.AddRow(
            "[green]show-profile[/] [grey]<path>[/] [grey]<profile-name>[/] [grey][[--plain]][/]",
            "Show one profile from a validated WhenItFails workspace.");
        commandGrid.AddRow(
            "[green]list-categories[/] [grey]<path>[/] [grey][[--plain]][/]",
            "List categories from a validated WhenItFails workspace.");
        commandGrid.AddRow(
            "[green]show-category[/] [grey]<path>[/] [grey]<category-name>[/] [grey][[--plain]][/]",
            "Show one category by name, display name, or alias.");
        commandGrid.AddRow(
            "[green]list-code-groups[/] [grey]<path>[/] [grey][[--plain]][/]",
            "List code groups from a validated WhenItFails workspace.");
        commandGrid.AddRow(
            "[green]show-code-group[/] [grey]<path>[/] [grey]<group-name|prefix>[/] [grey][[--plain]][/]",
            "Show one code group by name, display name, or prefix.");
        commandGrid.AddRow(
            "[green]list-owners[/] [grey]<path>[/] [grey][[--plain]][/]",
            "List owners from a validated WhenItFails workspace.");
        commandGrid.AddRow(
            "[green]show-owner[/] [grey]<path>[/] [grey]<owner-name|alias>[/] [grey][[--plain]][/]",
            "Show one owner by name, display name, or alias.");
        commandGrid.AddRow(
            "[green]details[/] [grey]<path>[/] [grey]<id|code|name>[/] [grey][[--plain]][/]",
            "Show one error definition in detail.");
        commandGrid.AddRow("[green]detail[/] [grey]<path>[/] [grey]<id|code|name>[/]", "Alias for details.");
        commandGrid.AddRow(
            "[green]set-title[/] [grey]<path>[/] [grey]<id|code|name>[/] [grey]<title>[/]",
            "Safely change the title of one error definition.");
        commandGrid.AddRow(
            "[green]set-message[/] [grey]<path>[/] [grey]<id|code|name>[/] [grey]<message>[/]",
            "Safely change the message of one error definition.");
        commandGrid.AddRow(
            "[green]set-developer-hint[/] [grey]<path>[/] [grey]<id|code|name>[/] [grey]<developer-hint>[/]",
            "Safely change the developer hint of one error definition.");
        commandGrid.AddRow(
            "[green]set-severity[/] [grey]<path>[/] [grey]<id|code|name>[/] [grey]<severity>[/]",
            "Safely change the severity of one error definition. Allowed values: Trace, Debug, Information, Warning, Error, Critical.");
        commandGrid.AddRow(
            "[green]set-documentation-key[/] [grey]<path>[/] [grey]<id|code|name>[/] [grey]<documentation-key>[/]",
            "Safely change the documentation key of one error definition.");
        commandGrid.AddRow("[green]demo[/]", "Show a sample WhenItFails validation result.");
        commandGrid.AddRow("[green]help[/]", "Show this help screen.");

        Panel helpPanel = new Panel(commandGrid)
            .Header("[bold aqua]WhenItFails Setter[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Aqua);

        AnsiConsole.Write(helpPanel);
    }
}
