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
            "[green]details[/] [grey]<path>[/] [grey]<id|code|name>[/] [grey][[--plain]][/]",
            "Show one error definition in detail.");
        commandGrid.AddRow("[green]detail[/] [grey]<path>[/] [grey]<id|code|name>[/]", "Alias for details.");
        commandGrid.AddRow("[green]demo[/]", "Show a sample WhenItFails validation result.");
        commandGrid.AddRow("[green]help[/]", "Show this help screen.");

        Panel helpPanel = new Panel(commandGrid)
            .Header("[bold aqua]WhenItFails Setter[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Aqua);

        AnsiConsole.Write(helpPanel);
    }
}
