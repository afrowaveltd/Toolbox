using Spectre.Console;

namespace Afrowave.Toolbox.UnderstandMe.Configurator;

/// <summary>
/// Small, safe, dependency-free Markdown help presenter. This is NOT the future full Markdown
/// AST/parser; it formats headings, lists and fenced code and prints other syntax literally.
/// All document text is escaped before reaching Spectre markup.
/// </summary>
public static class ConfiguratorHelpRenderer
{
    /// <summary>Render the bundled basic Markdown safely in an injected terminal.</summary>
    public static void Render(IAnsiConsole console, string markdown)
    {
        ArgumentNullException.ThrowIfNull(console);
        ArgumentNullException.ThrowIfNull(markdown);

        using var reader = new StringReader(markdown);
        bool inCode = false;
        while (reader.ReadLine() is { } line)
        {
            var trimmed = line.TrimStart();
            if (trimmed.StartsWith("```", StringComparison.Ordinal))
            {
                inCode = !inCode;
                continue;
            }

            if (inCode)
            {
                console.Write(new Text("    " + line + "\n"));
                continue;
            }

            var level = 0;
            while (level < line.Length && line[level] == '#' && level < 6)
                level++;
            if (level > 0 && level < line.Length && line[level] == ' ')
            {
                console.Write(new Markup("[bold]" + Markup.Escape(line[(level + 1)..]) + "[/]\n"));
            }
            else if (trimmed.StartsWith("- ", StringComparison.Ordinal) ||
                     trimmed.StartsWith("* ", StringComparison.Ordinal))
            {
                console.Write(new Text("  • " + trimmed[2..] + "\n"));
            }
            else
            {
                console.Write(new Text(line + "\n"));
            }
        }
    }
}
