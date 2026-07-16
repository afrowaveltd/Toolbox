using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Views;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Shows bundled reference catalog information.
/// </summary>
internal static class ReferenceCommand
{
   /// <summary>
   /// Executes the reference command.
   /// </summary>
   /// <param name="args">Command line arguments.</param>
   /// <returns>Process exit code.</returns>
   public static async Task<int> ExecuteAsync(string[] args)
   {
      ArgumentNullException.ThrowIfNull(args);

      string subcommand = args.Length < 2
         ? "summary"
         : args[1].Trim().ToLowerInvariant();

      if (args.Length > 2)
      {
         ShowInvalidUsage();

         return 1;
      }

      WhenItFailsReferenceCatalogSummarizer summarizer = new();

      WhenItFailsReferenceCatalogSummary summary =
         await summarizer.SummarizeAsync(Environment.CurrentDirectory);

      if (subcommand == "summary")
      {
         ReferenceView.Show(summary);

         return 0;
      }

      if (subcommand == "profiles")
      {
         ReferenceView.ShowProfiles(summary);

         return 0;
      }

      ShowUnknownSubcommand(args[1]);

      return 1;
   }

   private static void ShowInvalidUsage()
   {
      AnsiConsole.MarkupLine("[red]Invalid reference command usage.[/]");
      AnsiConsole.MarkupLine("Usage:");
      AnsiConsole.MarkupLine("  [grey]reference[/]");
      AnsiConsole.MarkupLine("  [grey]reference summary[/]");
      AnsiConsole.MarkupLine("  [grey]reference profiles[/]");
   }

   private static void ShowUnknownSubcommand(string subcommand)
   {
      AnsiConsole.MarkupLine(
         "[red]Unknown reference subcommand:[/] {0}",
         Markup.Escape(subcommand));

      ShowInvalidUsage();
   }
}
