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
            "[green]add-profile[/] [grey]<path>[/] [grey]<name>[/] [grey]<display-name>[/] [grey][[description]][/]",
            "Safely add a project profile. Quote display names or descriptions containing spaces.");
        commandGrid.AddRow(
            "[green]remove-profile[/] [grey]<path>[/] [grey]<name>[/]",
            "Safely remove one project profile.");
        commandGrid.AddRow(
            "[green]set-profile-display-name[/] [grey]<path>[/] [grey]<profile-name>[/] [grey]<display-name>[/]",
            "Safely change the human-readable name of one profile.");
        commandGrid.AddRow(
            "[green]set-profile-description[/] [grey]<path>[/] [grey]<profile-name>[/] [grey]<description>[/]",
            "Safely set a profile description. Pass an empty quoted string to clear it.");
        commandGrid.AddRow(
            "[green]profile-add-owner[/] [grey]<path>[/] [grey]<profile-name>[/] [grey]<owner-name|alias>[/]",
            "Safely include an existing owner in one profile.");
        commandGrid.AddRow(
            "[green]profile-remove-owner[/] [grey]<path>[/] [grey]<profile-name>[/] [grey]<owner-name|alias>[/]",
            "Safely remove an included owner from one profile.");
        commandGrid.AddRow(
            "[green]profile-add-category[/] [grey]<path>[/] [grey]<profile-name>[/] [grey]<category-name|alias>[/]",
            "Safely include an existing category in one profile.");
        commandGrid.AddRow(
            "[green]profile-remove-category[/] [grey]<path>[/] [grey]<profile-name>[/] [grey]<category-name|alias>[/]",
            "Safely remove an included category from one profile.");
        commandGrid.AddRow(
            "[green]profile-add-code-group[/] [grey]<path>[/] [grey]<profile-name>[/] [grey]<group-name|prefix>[/]",
            "Safely include an existing code group in one profile.");
        commandGrid.AddRow(
            "[green]profile-remove-code-group[/] [grey]<path>[/] [grey]<profile-name>[/] [grey]<group-name|prefix>[/]",
            "Safely remove an included code group from one profile.");
        commandGrid.AddRow(
            "[green]profile-add-subcategory[/] [grey]<path>[/] [grey]<profile-name>[/] [grey]<subcategory>[/]",
            "Safely include an existing error subcategory in one profile.");
        commandGrid.AddRow(
            "[green]profile-remove-subcategory[/] [grey]<path>[/] [grey]<profile-name>[/] [grey]<subcategory>[/]",
            "Safely remove an included error subcategory from one profile.");
        commandGrid.AddRow(
            "[green]profile-add-tag[/] [grey]<path>[/] [grey]<profile-name>[/] [grey]<tag>[/]",
            "Safely include an existing error tag in one profile.");
        commandGrid.AddRow(
            "[green]profile-remove-tag[/] [grey]<path>[/] [grey]<profile-name>[/] [grey]<tag>[/]",
            "Safely remove an included error tag from one profile.");
        commandGrid.AddRow(
            "[green]profile-add-excluded-tag[/] [grey]<path>[/] [grey]<profile-name>[/] [grey]<tag>[/]",
            "Safely exclude an existing error tag from one profile.");
        commandGrid.AddRow(
            "[green]profile-remove-excluded-tag[/] [grey]<path>[/] [grey]<profile-name>[/] [grey]<tag>[/]",
            "Safely stop excluding an error tag from one profile.");
        commandGrid.AddRow(
            "[green]profile-add-error[/] [grey]<path>[/] [grey]<profile-name>[/] [grey]<id|code|name>[/]",
            "Safely include one existing error explicitly in a profile.");
        commandGrid.AddRow(
            "[green]profile-remove-error[/] [grey]<path>[/] [grey]<profile-name>[/] [grey]<id|code|name>[/]",
            "Safely remove one explicitly included error from a profile.");
        commandGrid.AddRow(
            "[green]profile-add-excluded-error[/] [grey]<path>[/] [grey]<profile-name>[/] [grey]<id|code|name>[/]",
            "Safely exclude one existing error explicitly from a profile.");
        commandGrid.AddRow(
            "[green]profile-remove-excluded-error[/] [grey]<path>[/] [grey]<profile-name>[/] [grey]<id|code|name>[/]",
            "Safely stop explicitly excluding one error from a profile.");
        commandGrid.AddRow(
            "[green]profile-set-default-mapping[/] [grey]<path>[/] [grey]<profile-name>[/] [grey]<mapping-key>[/] [grey]<mapping-value>[/]",
            "Safely add or update a profile default mapping. Quote values containing spaces.");
        commandGrid.AddRow(
            "[green]profile-remove-default-mapping[/] [grey]<path>[/] [grey]<profile-name>[/] [grey]<mapping-key>[/]",
            "Safely remove one default mapping from a profile.");
        commandGrid.AddRow(
            "[green]profile-set-metadata[/] [grey]<path>[/] [grey]<profile-name>[/] [grey]<metadata-key>[/] [grey]<metadata-value>[/]",
            "Safely add or update profile metadata. Quote values containing spaces.");
        commandGrid.AddRow(
            "[green]profile-remove-metadata[/] [grey]<path>[/] [grey]<profile-name>[/] [grey]<metadata-key>[/]",
            "Safely remove one metadata value from a profile.");
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
            "[green]error-add-tag[/] [grey]<path>[/] [grey]<id|code|name>[/] [grey]<tag>[/]",
            "Safely add one normalized tag to an error definition.");
        commandGrid.AddRow(
            "[green]error-remove-tag[/] [grey]<path>[/] [grey]<id|code|name>[/] [grey]<tag>[/]",
            "Safely remove one tag from an error definition.");
        commandGrid.AddRow(
            "[green]error-add-category[/] [grey]<path>[/] [grey]<id|code|name>[/] [grey]<category-name|alias>[/]",
            "Safely add one existing category to an error definition.");
        commandGrid.AddRow(
            "[green]error-remove-category[/] [grey]<path>[/] [grey]<id|code|name>[/] [grey]<category-name|alias>[/]",
            "Safely remove one category from an error definition.");
        commandGrid.AddRow(
            "[green]error-add-subcategory[/] [grey]<path>[/] [grey]<id|code|name>[/] [grey]<subcategory>[/]",
            "Safely add one existing subcategory to an error definition.");
        commandGrid.AddRow(
            "[green]error-remove-subcategory[/] [grey]<path>[/] [grey]<id|code|name>[/] [grey]<subcategory>[/]",
            "Safely remove one subcategory from an error definition.");
        commandGrid.AddRow(
            "[green]error-set-metadata[/] [grey]<path>[/] [grey]<id|code|name>[/] [grey]<metadata-key>[/] [grey]<metadata-value>[/]",
            "Safely add or update error metadata. Quote values containing spaces.");
        commandGrid.AddRow(
            "[green]error-remove-metadata[/] [grey]<path>[/] [grey]<id|code|name>[/] [grey]<metadata-key>[/]",
            "Safely remove one metadata value from an error definition.");
        commandGrid.AddRow(
            "[green]set-primary-category[/] [grey]<path>[/] [grey]<id|code|name>[/] [grey]<category-name|alias>[/]",
            "Safely change the primary category of one error definition.");
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
