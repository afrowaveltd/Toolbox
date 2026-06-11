using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Views;

/// <summary>
/// Shared console table helpers for Spectre.Console rendering.
/// </summary>
internal static class ConsoleTableViewHelper
{
    /// <summary>
    /// Configures a table with wide layout, rounded border, and the specified border color.
    /// </summary>
    /// <param name="table">The table to configure.</param>
    /// <param name="borderColor">The border color to apply.</param>
    public static void ConfigureWideTable(
        Table table,
        Color borderColor)
    {
        int consoleWidth = GetConsoleWidth();
        int tableWidth = Math.Max(80, consoleWidth - 2);

        table.Border(TableBorder.Rounded);
        table.BorderColor(borderColor);
        table.Width(tableWidth);
    }

    /// <summary>
    /// Escapes a string value for safe Spectre.Console markup rendering.
    /// </summary>
    /// <param name="value">The value to escape.</param>
    /// <returns>The escaped string.</returns>
    public static string Escape(string? value)
    {
        return Markup.Escape(value ?? string.Empty);
    }

    /// <summary>
    /// Joins a collection of string values with commas, or returns "(all)" if empty.
    /// </summary>
    /// <param name="values">The values to join.</param>
    /// <returns>The joined and escaped string.</returns>
    public static string JoinValues(IReadOnlyCollection<string> values)
    {
        if (values.Count == 0)
        {
            return "[grey](all)[/]";
        }

        return Markup.Escape(string.Join(", ", values));
    }

    /// <summary>
    /// Joins a collection of string values with commas for plain text output.
    /// </summary>
    /// <param name="values">The values to join.</param>
    /// <returns>The joined plain string.</returns>
    public static string JoinPlainValues(IReadOnlyCollection<string> values)
    {
        if (values.Count == 0)
        {
            return string.Empty;
        }

        return string.Join(
            ", ",
            values);
    }

    private static int GetConsoleWidth()
    {
        try
        {
            if (Console.WindowWidth > 0)
            {
                return Console.WindowWidth;
            }
        }
        catch
        {
            // Some redirected/piped outputs do not expose a real console width.
        }

        return 120;
    }
}
