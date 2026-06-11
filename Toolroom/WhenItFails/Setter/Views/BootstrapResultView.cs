using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Views;

/// <summary>
/// Renders the result of a workspace initialization (bootstrap) operation.
/// </summary>
internal static class BootstrapResultView
{
    /// <summary>
    /// Shows the bootstrap result with file creation statuses.
    /// </summary>
    /// <param name="payload">The bootstrap payload to display.</param>
    public static void Show(JsonsBootstrapPayload payload)
    {
        string directoryStatus = payload.PackageDirectoryCreated
            ? "[green]created[/]"
            : "[grey]already existed[/]";

        AnsiConsole.MarkupLine(
            "[bold aqua]WhenItFails JSON workspace:[/] {0} ({1})",
            Markup.Escape(payload.PackageDirectoryPath),
            directoryStatus);

        Table table = new Table();

        ConsoleTableViewHelper.ConfigureWideTable(
            table,
            Color.Aqua);

        table.AddColumn("File");
        table.AddColumn("Status");
        table.AddColumn("Path");
        table.AddColumn("Message");

        foreach (JsonsBootstrapFileResult fileResult in payload.Files)
        {
            string status = fileResult.Created
                ? "[green]Created[/]"
                : "[yellow]Skipped[/]";

            string displayPath = Path.GetRelativePath(
                payload.PackageDirectoryPath,
                fileResult.TargetFilePath);

            table.AddRow(
                Markup.Escape(fileResult.Name),
                status,
                Markup.Escape(displayPath),
                Markup.Escape(fileResult.Message ?? string.Empty));
        }

        AnsiConsole.Write(table);
    }
}
