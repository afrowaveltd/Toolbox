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

        commandGrid.AddRow("[green]init[/] [grey]<path>[/] [grey][[--json]][/]", "Create missing WhenItFails JSON files.");
        commandGrid.AddRow("[green]validate[/] [grey]<path>[/] [grey][[--json]][/]", "Validate WhenItFails JSON files.");
        commandGrid.AddRow("[green]summary[/] [grey]<path>[/] [grey][[--json]][/]", "Show a read-only summary of a WhenItFails JSON workspace.");
        commandGrid.AddRow("[green]inspect[/] [grey]<path>[/] [grey][[--json]][/]", "Alias for summary.");
        commandGrid.AddRow(
            "[green]errors[/] [grey]<path>[/] [grey][[--plain|--json]][/]",
            "List error definitions. Supports --owner, --group, --category, --severity, --profile, --search.");
        commandGrid.AddRow(
            "[green]list-profiles[/] [grey]<path>[/] [grey][[--plain|--json]][/]",
            "List profiles from a validated WhenItFails workspace.");
        commandGrid.AddRow(
            "[green]show-profile[/] [grey]<path>[/] [grey]<profile-name>[/] [grey][[--plain|--json]][/]",
            "Show one profile from a validated WhenItFails workspace.");
        commandGrid.AddRow(
            "[green]explain-profile[/] [grey]<path>[/] [grey]<profile-name|display-name>[/] [grey][[--plain|--json]][/]",
            "Explain why each error is included in or excluded from one profile.");
        commandGrid.AddRow(
            "[green]add-profile[/] [grey]<path>[/] [grey]<name>[/] [grey]<display-name>[/] [grey][[description]][/] [grey][[--json]][/]",
            "Safely add a project profile. Quote display names or descriptions containing spaces.");
        commandGrid.AddRow(
            "[green]remove-profile[/] [grey]<path>[/] [grey]<name>[/] [grey][[--json]][/]",
            "Safely remove one project profile.");
        commandGrid.AddRow(
            "[green]set-profile-display-name[/] [grey]<path>[/] [grey]<profile-name>[/] [grey]<display-name>[/] [grey][[--json]][/]",
            "Safely change the human-readable name of one profile.");
        commandGrid.AddRow(
            "[green]set-profile-description[/] [grey]<path>[/] [grey]<profile-name>[/] [grey]<description>[/] [grey][[--json]][/]",
            "Safely set a profile description. Pass an empty quoted string to clear it.");
        commandGrid.AddRow(
            "[green]profile-add-owner[/] [grey]<path>[/] [grey]<profile-name>[/] [grey]<owner-name|alias>[/] [grey][[--json]][/]",
            "Safely include an existing owner in one profile.");
        commandGrid.AddRow(
            "[green]profile-remove-owner[/] [grey]<path>[/] [grey]<profile-name>[/] [grey]<owner-name|alias>[/] [grey][[--json]][/]",
            "Safely remove an included owner from one profile.");
        commandGrid.AddRow(
            "[green]profile-add-category[/] [grey]<path>[/] [grey]<profile-name>[/] [grey]<category-name|alias>[/] [grey][[--json]][/]",
            "Safely include an existing category in one profile.");
        commandGrid.AddRow(
            "[green]profile-remove-category[/] [grey]<path>[/] [grey]<profile-name>[/] [grey]<category-name|alias>[/] [grey][[--json]][/]",
            "Safely remove an included category from one profile.");
        commandGrid.AddRow(
            "[green]profile-add-code-group[/] [grey]<path>[/] [grey]<profile-name>[/] [grey]<group-name|prefix>[/] [grey][[--json]][/]",
            "Safely include an existing code group in one profile.");
        commandGrid.AddRow(
            "[green]profile-remove-code-group[/] [grey]<path>[/] [grey]<profile-name>[/] [grey]<group-name|prefix>[/] [grey][[--json]][/]",
            "Safely remove an included code group from one profile.");
        commandGrid.AddRow(
            "[green]profile-add-subcategory[/] [grey]<path>[/] [grey]<profile-name>[/] [grey]<subcategory>[/] [grey][[--json]][/]",
            "Safely include an existing error subcategory in one profile.");
        commandGrid.AddRow(
            "[green]profile-remove-subcategory[/] [grey]<path>[/] [grey]<profile-name>[/] [grey]<subcategory>[/] [grey][[--json]][/]",
            "Safely remove an included error subcategory from one profile.");
        commandGrid.AddRow(
            "[green]profile-add-tag[/] [grey]<path>[/] [grey]<profile-name>[/] [grey]<tag>[/] [grey][[--json]][/]",
            "Safely include an existing error tag in one profile.");
        commandGrid.AddRow(
            "[green]profile-remove-tag[/] [grey]<path>[/] [grey]<profile-name>[/] [grey]<tag>[/] [grey][[--json]][/]",
            "Safely remove an included error tag from one profile.");
        commandGrid.AddRow(
            "[green]profile-add-excluded-tag[/] [grey]<path>[/] [grey]<profile-name>[/] [grey]<tag>[/] [grey][[--json]][/]",
            "Safely exclude an existing error tag from one profile.");
        commandGrid.AddRow(
            "[green]profile-remove-excluded-tag[/] [grey]<path>[/] [grey]<profile-name>[/] [grey]<tag>[/] [grey][[--json]][/]",
            "Safely stop excluding an error tag from one profile.");
        commandGrid.AddRow(
            "[green]profile-add-error[/] [grey]<path>[/] [grey]<profile-name>[/] [grey]<id|code|name>[/] [grey][[--json]][/]",
            "Safely include one existing error explicitly in a profile.");
        commandGrid.AddRow(
            "[green]profile-remove-error[/] [grey]<path>[/] [grey]<profile-name>[/] [grey]<id|code|name>[/] [grey][[--json]][/]",
            "Safely remove one explicitly included error from a profile.");
        commandGrid.AddRow(
            "[green]profile-add-excluded-error[/] [grey]<path>[/] [grey]<profile-name>[/] [grey]<id|code|name>[/] [grey][[--json]][/]",
            "Safely exclude one existing error explicitly from a profile.");
        commandGrid.AddRow(
            "[green]profile-remove-excluded-error[/] [grey]<path>[/] [grey]<profile-name>[/] [grey]<id|code|name>[/] [grey][[--json]][/]",
            "Safely stop explicitly excluding one error from a profile.");
        commandGrid.AddRow(
            "[green]profile-set-default-mapping[/] [grey]<path>[/] [grey]<profile-name>[/] [grey]<mapping-key>[/] [grey]<mapping-value>[/] [grey][[--json]][/]",
            "Safely add or update a profile default mapping. Quote values containing spaces.");
        commandGrid.AddRow(
            "[green]profile-remove-default-mapping[/] [grey]<path>[/] [grey]<profile-name>[/] [grey]<mapping-key>[/] [grey][[--json]][/]",
            "Safely remove one default mapping from a profile.");
        commandGrid.AddRow(
            "[green]profile-set-metadata[/] [grey]<path>[/] [grey]<profile-name>[/] [grey]<metadata-key>[/] [grey]<metadata-value>[/] [grey][[--json]][/]",
            "Safely add or update profile metadata. Quote values containing spaces.");
        commandGrid.AddRow(
            "[green]profile-remove-metadata[/] [grey]<path>[/] [grey]<profile-name>[/] [grey]<metadata-key>[/] [grey][[--json]][/]",
            "Safely remove one metadata value from a profile.");
        commandGrid.AddRow(
            "[green]list-categories[/] [grey]<path>[/] [grey][[--plain|--json]][/]",
            "List categories from a validated WhenItFails workspace.");
        commandGrid.AddRow(
            "[green]show-category[/] [grey]<path>[/] [grey]<category-name>[/] [grey][[--plain|--json]][/]",
            "Show one category by name, display name, or alias.");
        commandGrid.AddRow(
            "[green]list-code-groups[/] [grey]<path>[/] [grey][[--plain|--json]][/]",
            "List code groups from a validated WhenItFails workspace.");
        commandGrid.AddRow(
            "[green]show-code-group[/] [grey]<path>[/] [grey]<group-name|prefix>[/] [grey][[--plain|--json]][/]",
            "Show one code group by name, display name, or prefix.");
        commandGrid.AddRow(
            "[green]list-owners[/] [grey]<path>[/] [grey][[--plain|--json]][/]",
            "List owners from a validated WhenItFails workspace.");
        commandGrid.AddRow(
            "[green]show-owner[/] [grey]<path>[/] [grey]<owner-name|alias>[/] [grey][[--plain|--json]][/]",
            "Show one owner by name, display name, or alias.");
        commandGrid.AddRow(
            "[green]details[/] [grey]<path>[/] [grey]<id|code|name>[/] [grey][[--plain|--json]][/]",
            "Show one error definition in detail.");
        commandGrid.AddRow(
            "[green]detail[/] [grey]<path>[/] [grey]<id|code|name>[/] [grey][[--plain|--json]][/]",
            "Alias for details.");
        commandGrid.AddRow(
            "[green]next-code[/] [grey]<path>[/] [grey]<owner-name|alias>[/] [grey]<group-name|prefix>[/] [grey][[--plain|--json]][/]",
            "Read-only suggestion of the first free numeric code and structured id.");
        commandGrid.AddRow(
            "[green]list-backups[/] [grey]<path>[/] [grey][[--plain|--json]][/]",
            "Read-only list of catalog backups, newest first.");
        commandGrid.AddRow(
            "[green]restore-backup[/] [grey]<path>[/] [grey]<backup-file-name>[/] [grey][[--json]][/]",
            "Safely restore one catalog backup, validate the workspace, and roll back on failure.");
        commandGrid.AddRow(
            "[green]add-error[/] [grey]<path>[/] [grey]<owner>[/] [grey]<group>[/] [grey]<category>[/] [grey]<name>[/] [grey]<title>[/] [grey]<message>[/] [grey][[severity]][/] [grey][[--json]][/]",
            "Safely add one complete error definition; JSON output includes structured success or failure details.");
        commandGrid.AddRow(
            "[green]remove-error[/] [grey]<path>[/] [grey]<id|code|name>[/] [grey][[--json]][/]",
            "Safely remove one error definition; JSON failures include blocking profile references.");
        commandGrid.AddRow(
            "[green]error-references[/] [grey]<path>[/] [grey]<id|code|name>[/] [grey][[--plain|--json]][/]",
            "Read-only list of profiles that explicitly include or exclude one error.");
        commandGrid.AddRow(
            "[green]error-add-tag[/] [grey]<path>[/] [grey]<id|code|name>[/] [grey]<tag>[/] [grey][[--json]][/]",
            "Safely add one normalized tag; JSON output includes structured success or failure details.");
        commandGrid.AddRow(
            "[green]error-remove-tag[/] [grey]<path>[/] [grey]<id|code|name>[/] [grey]<tag>[/] [grey][[--json]][/]",
            "Safely remove one tag from an error definition.");
        commandGrid.AddRow(
            "[green]error-add-category[/] [grey]<path>[/] [grey]<id|code|name>[/] [grey]<category-name|alias>[/] [grey][[--json]][/]",
            "Safely add one existing category to an error definition.");
        commandGrid.AddRow(
            "[green]error-remove-category[/] [grey]<path>[/] [grey]<id|code|name>[/] [grey]<category-name|alias>[/] [grey][[--json]][/]",
            "Safely remove one category from an error definition.");
        commandGrid.AddRow(
            "[green]error-add-subcategory[/] [grey]<path>[/] [grey]<id|code|name>[/] [grey]<subcategory>[/] [grey][[--json]][/]",
            "Safely add one existing subcategory to an error definition.");
        commandGrid.AddRow(
            "[green]error-remove-subcategory[/] [grey]<path>[/] [grey]<id|code|name>[/] [grey]<subcategory>[/] [grey][[--json]][/]",
            "Safely remove one subcategory from an error definition.");
        commandGrid.AddRow(
            "[green]error-set-metadata[/] [grey]<path>[/] [grey]<id|code|name>[/] [grey]<metadata-key>[/] [grey]<metadata-value>[/] [grey][[--json]][/]",
            "Safely add or update error metadata. Quote values containing spaces.");
        commandGrid.AddRow(
            "[green]error-remove-metadata[/] [grey]<path>[/] [grey]<id|code|name>[/] [grey]<metadata-key>[/] [grey][[--json]][/]",
            "Safely remove one metadata value from an error definition.");
        commandGrid.AddRow(
            "[green]set-primary-category[/] [grey]<path>[/] [grey]<id|code|name>[/] [grey]<category-name|alias>[/] [grey][[--json]][/]",
            "Safely change the primary category; JSON output includes structured success or failure details.");
        commandGrid.AddRow(
            "[green]set-owner[/] [grey]<path>[/] [grey]<id|code|name>[/] [grey]<owner-name|alias>[/] [grey][[--json]][/]",
            "Atomically change an error owner and structured id; JSON output includes the complete result.");
        commandGrid.AddRow(
            "[green]set-code-group[/] [grey]<path>[/] [grey]<id|code|name>[/] [grey]<group-name|prefix>[/] [grey][[--json]][/]",
            "Atomically change code group, prefix, numeric code, and structured id; JSON output includes the complete result.");
        commandGrid.AddRow(
            "[green]set-name[/] [grey]<path>[/] [grey]<id|code|name>[/] [grey]<new-name>[/] [grey][[--json]][/]",
            "Safely change the normalized machine-friendly name; JSON output includes structured success or failure details.");
        commandGrid.AddRow(
            "[green]set-title[/] [grey]<path>[/] [grey]<id|code|name>[/] [grey]<title>[/] [grey][[--json]][/]",
            "Safely change the title; JSON output includes structured success or failure details.");
        commandGrid.AddRow(
            "[green]set-message[/] [grey]<path>[/] [grey]<id|code|name>[/] [grey]<message>[/] [grey][[--json]][/]",
            "Safely change the message; JSON output includes structured success or failure details.");
        commandGrid.AddRow(
            "[green]set-developer-hint[/] [grey]<path>[/] [grey]<id|code|name>[/] [grey]<developer-hint>[/] [grey][[--json]][/]",
            "Safely change the developer hint; JSON output includes structured success or failure details.");
        commandGrid.AddRow(
            "[green]set-severity[/] [grey]<path>[/] [grey]<id|code|name>[/] [grey]<severity>[/] [grey][[--json]][/]",
            "Safely change the severity; JSON output includes structured success or failure details. Allowed values: Trace, Debug, Information, Warning, Error, Critical.");
        commandGrid.AddRow(
            "[green]set-documentation-key[/] [grey]<path>[/] [grey]<id|code|name>[/] [grey]<documentation-key>[/] [grey][[--json]][/]",
            "Safely change the documentation key; JSON output includes structured success or failure details.");
        commandGrid.AddRow("[green]demo[/]", "Show a sample WhenItFails validation result.");
        commandGrid.AddRow("[green]help[/]", "Show this help screen.");

        Panel helpPanel = new Panel(commandGrid)
            .Header("[bold aqua]WhenItFails Setter[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Aqua);

        AnsiConsole.Write(helpPanel);
    }
}
