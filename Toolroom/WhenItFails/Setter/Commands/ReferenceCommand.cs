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

      if (!TryParseArguments(
             args,
             subcommand,
             out WhenItFailsReferenceErrorListOptions errorListOptions))
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

      if (subcommand == "categories")
      {
         ReferenceView.ShowCategories(summary);

         return 0;
      }

      if (subcommand == "code-groups")
      {
         ReferenceView.ShowCodeGroups(summary);

         return 0;
      }

      if (subcommand == "errors")
      {
         ReferenceView.ShowErrors(
            summary,
            errorListOptions);

         return 0;
      }

      if (subcommand == "error")
      {
         return ShowError(
            summary,
            args[2]);
      }

      ShowUnknownSubcommand(args[1]);

      return 1;
   }

   private static bool TryParseArguments(
      string[] args,
      string subcommand,
      out WhenItFailsReferenceErrorListOptions errorListOptions)
   {
      errorListOptions = new();

      if (subcommand == "error")
      {
         return args.Length == 3
                && !string.IsNullOrWhiteSpace(args[2]);
      }

      if (subcommand != "errors")
      {
         return args.Length <= 2;
      }

      int index = 2;

      while (index < args.Length)
      {
         string argument = args[index];

         if (string.Equals(argument, "--all", StringComparison.OrdinalIgnoreCase))
         {
            if (errorListOptions.ShowAll)
            {
               return false;
            }

            errorListOptions.ShowAll = true;
            index++;

            continue;
         }

         if (string.Equals(argument, "--group", StringComparison.OrdinalIgnoreCase))
         {
            if (!string.IsNullOrWhiteSpace(errorListOptions.CodeGroup)
                || index + 1 >= args.Length
                || string.IsNullOrWhiteSpace(args[index + 1]))
            {
               return false;
            }

            errorListOptions.CodeGroup = args[index + 1];
            index += 2;

            continue;
         }

         if (string.Equals(argument, "--category", StringComparison.OrdinalIgnoreCase))
         {
            if (!string.IsNullOrWhiteSpace(errorListOptions.Category)
                || index + 1 >= args.Length
                || string.IsNullOrWhiteSpace(args[index + 1]))
            {
               return false;
            }

            errorListOptions.Category = args[index + 1];
            index += 2;

            continue;
         }

         return false;
      }

      return true;
   }

   private static int ShowError(
      WhenItFailsReferenceCatalogSummary summary,
      string idOrName)
   {
      WhenItFailsReferenceErrorSummary? error =
         summary.Errors.FirstOrDefault(candidate =>
            string.Equals(
               candidate.Id,
               idOrName,
               StringComparison.OrdinalIgnoreCase)
            || string.Equals(
               candidate.Name,
               idOrName,
               StringComparison.OrdinalIgnoreCase));

      if (error is null)
      {
         AnsiConsole.MarkupLine(
            "[red]Reference error was not found:[/] {0}",
            Markup.Escape(idOrName));

         return 1;
      }

      ReferenceView.ShowError(error);

      return 0;
   }

   private static void ShowInvalidUsage()
   {
      AnsiConsole.MarkupLine("[red]Invalid reference command usage.[/]");
      AnsiConsole.MarkupLine("Usage:");
      AnsiConsole.MarkupLine("  [grey]reference[/]");
      AnsiConsole.MarkupLine("  [grey]reference summary[/]");
      AnsiConsole.MarkupLine("  [grey]reference profiles[/]");
      AnsiConsole.MarkupLine("  [grey]reference categories[/]");
      AnsiConsole.MarkupLine("  [grey]reference code-groups[/]");
      AnsiConsole.MarkupLine("  [grey]reference errors[/]");
      AnsiConsole.MarkupLine("  [grey]reference errors --all[/]");
      AnsiConsole.MarkupLine("  [grey]reference errors --group <code-group>[/]");
      AnsiConsole.MarkupLine("  [grey]reference errors --category <category>[/]");
      AnsiConsole.MarkupLine("  [grey]reference error <id-or-name>[/]");
   }

   private static void ShowUnknownSubcommand(string subcommand)
   {
      AnsiConsole.MarkupLine(
         "[red]Unknown reference subcommand:[/] {0}",
         Markup.Escape(subcommand));

      ShowInvalidUsage();
   }
}
