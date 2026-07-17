using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Views;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Shows bundled reference catalog information.
/// </summary>
internal static class ReferenceCommand
{
   private const int DefaultErrorLimit = 20;

   /// <summary>
   /// Executes the reference command.
   /// </summary>
   /// <param name="args">Command line arguments.</param>
   /// <returns>Process exit code.</returns>
   public static async Task<int> ExecuteAsync(string[] args)
   {
      ArgumentNullException.ThrowIfNull(args);

      string subcommand = ResolveSubcommand(args);

      if (!TryParseArguments(
             args,
             subcommand,
             out WhenItFailsReferenceErrorListOptions errorListOptions,
             out bool useJsonOutput))
      {
         ShowInvalidUsage();

         return 1;
      }

      WhenItFailsReferenceCatalogSummarizer summarizer = new();

      WhenItFailsReferenceCatalogSummary summary =
         await summarizer.SummarizeAsync(Environment.CurrentDirectory);

      if (subcommand == "summary")
      {
         if (useJsonOutput)
         {
            CommandJsonOutput.Write("reference", new ReferenceResult("summary", summary));
         }
         else
         {
            ReferenceView.Show(summary);
         }

         return 0;
      }

      if (subcommand == "profiles")
      {
         if (useJsonOutput)
         {
            CommandJsonOutput.Write("reference", new ReferenceResult("profiles", summary.Profiles));
         }
         else
         {
            ReferenceView.ShowProfiles(summary);
         }

         return 0;
      }

      if (subcommand == "categories")
      {
         if (useJsonOutput)
         {
            CommandJsonOutput.Write("reference", new ReferenceResult("categories", summary.Categories));
         }
         else
         {
            ReferenceView.ShowCategories(summary);
         }

         return 0;
      }

      if (subcommand == "code-groups")
      {
         if (useJsonOutput)
         {
            CommandJsonOutput.Write("reference", new ReferenceResult("code-groups", summary.CodeGroups));
         }
         else
         {
            ReferenceView.ShowCodeGroups(summary);
         }

         return 0;
      }

      if (subcommand == "errors")
      {
         if (useJsonOutput)
         {
            IReadOnlyList<WhenItFailsReferenceErrorSummary> matchingErrors = summary.Errors
               .Where(error => MatchesErrorListOptions(error, errorListOptions))
               .ToList();

            IReadOnlyList<WhenItFailsReferenceErrorSummary> returnedErrors = errorListOptions.ShowAll
               ? matchingErrors
               : matchingErrors.Take(DefaultErrorLimit).ToList();

            CommandJsonOutput.Write(
               "reference",
               new ReferenceResult(
                  "errors",
                  new ReferenceErrorsResult(
                     errorListOptions,
                     matchingErrors.Count,
                     returnedErrors.Count,
                     returnedErrors)));
         }
         else
         {
            ReferenceView.ShowErrors(
               summary,
               errorListOptions);
         }

         return 0;
      }

      if (subcommand == "error")
      {
         return ShowError(
            summary,
            args[2],
            useJsonOutput);
      }

      if (subcommand == "profile")
      {
         return ShowProfile(
            summary,
            args[2],
            useJsonOutput);
      }

      ShowUnknownSubcommand(args[1]);

      return 1;
   }

   private static string ResolveSubcommand(string[] args)
   {
      return args.Length < 2
             || string.Equals(args[1], "--json", StringComparison.OrdinalIgnoreCase)
         ? "summary"
         : args[1].Trim().ToLowerInvariant();
   }

   private static bool TryParseArguments(
      string[] args,
      string subcommand,
      out WhenItFailsReferenceErrorListOptions errorListOptions,
      out bool useJsonOutput)
   {
      errorListOptions = new();
      useJsonOutput = false;

      if (subcommand is "error" or "profile")
      {
         if (args.Length < 3
             || args.Length > 4
             || string.IsNullOrWhiteSpace(args[2]))
         {
            return false;
         }

         if (args.Length == 4)
         {
            if (!string.Equals(args[3], "--json", StringComparison.OrdinalIgnoreCase))
            {
               return false;
            }

            useJsonOutput = true;
         }

         return true;
      }

      if (subcommand == "errors")
      {
         return TryParseErrorListArguments(
            args,
            errorListOptions,
            out useJsonOutput);
      }

      if (subcommand is not ("summary" or "profiles" or "categories" or "code-groups"))
      {
         return args.Length <= 2;
      }

      int optionStartIndex = args.Length >= 2
                             && string.Equals(args[1], "--json", StringComparison.OrdinalIgnoreCase)
         ? 1
         : 2;

      for (int index = optionStartIndex; index < args.Length; index++)
      {
         if (!string.Equals(args[index], "--json", StringComparison.OrdinalIgnoreCase)
             || useJsonOutput)
         {
            return false;
         }

         useJsonOutput = true;
      }

      return true;
   }

   private static bool TryParseErrorListArguments(
      string[] args,
      WhenItFailsReferenceErrorListOptions errorListOptions,
      out bool useJsonOutput)
   {
      useJsonOutput = false;
      int index = 2;

      while (index < args.Length)
      {
         string argument = args[index];

         if (string.Equals(argument, "--json", StringComparison.OrdinalIgnoreCase))
         {
            if (useJsonOutput)
            {
               return false;
            }

            useJsonOutput = true;
            index++;

            continue;
         }

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
                || string.IsNullOrWhiteSpace(args[index + 1])
                || args[index + 1].StartsWith("--", StringComparison.Ordinal))
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
                || string.IsNullOrWhiteSpace(args[index + 1])
                || args[index + 1].StartsWith("--", StringComparison.Ordinal))
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

   private static bool MatchesErrorListOptions(
      WhenItFailsReferenceErrorSummary error,
      WhenItFailsReferenceErrorListOptions options)
   {
      if (!string.IsNullOrWhiteSpace(options.CodeGroup)
          && !string.Equals(
             error.CodeGroup,
             options.CodeGroup,
             StringComparison.OrdinalIgnoreCase))
      {
         return false;
      }

      if (!string.IsNullOrWhiteSpace(options.Category)
          && !string.Equals(
             error.PrimaryCategory,
             options.Category,
             StringComparison.OrdinalIgnoreCase)
          && !error.CategoryNames.Any(categoryName =>
             string.Equals(
                categoryName,
                options.Category,
                StringComparison.OrdinalIgnoreCase)))
      {
         return false;
      }

      return true;
   }

   private static int ShowProfile(
      WhenItFailsReferenceCatalogSummary summary,
      string profileName,
      bool useJsonOutput)
   {
      WhenItFailsReferenceProfileSummary? profile =
         summary.Profiles.FirstOrDefault(candidate =>
            string.Equals(
               candidate.Name,
               profileName,
               StringComparison.OrdinalIgnoreCase));

      if (profile is null)
      {
         if (useJsonOutput)
         {
            CommandJsonOutput.Write(
               "reference",
               new ReferenceResult(
                  "profile",
                  new ReferenceLookupResult(
                     false,
                     profileName,
                     null,
                     "ReferenceProfileNotFound",
                     $"Reference profile '{profileName}' was not found.")));
         }
         else
         {
            AnsiConsole.MarkupLine(
               "[red]Reference profile was not found:[/] {0}",
               Markup.Escape(profileName));
         }

         return 1;
      }

      if (useJsonOutput)
      {
         CommandJsonOutput.Write(
            "reference",
            new ReferenceResult(
               "profile",
               new ReferenceLookupResult(
                  true,
                  profileName,
                  profile,
                  null,
                  null)));
      }
      else
      {
         ReferenceView.ShowProfile(profile);
      }

      return 0;
   }

   private static int ShowError(
      WhenItFailsReferenceCatalogSummary summary,
      string idOrName,
      bool useJsonOutput)
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
         if (useJsonOutput)
         {
            CommandJsonOutput.Write(
               "reference",
               new ReferenceResult(
                  "error",
                  new ReferenceLookupResult(
                     false,
                     idOrName,
                     null,
                     "ReferenceErrorNotFound",
                     $"Reference error '{idOrName}' was not found.")));
         }
         else
         {
            AnsiConsole.MarkupLine(
               "[red]Reference error was not found:[/] {0}",
               Markup.Escape(idOrName));
         }

         return 1;
      }

      if (useJsonOutput)
      {
         CommandJsonOutput.Write(
            "reference",
            new ReferenceResult(
               "error",
               new ReferenceLookupResult(
                  true,
                  idOrName,
                  error,
                  null,
                  null)));
      }
      else
      {
         ReferenceView.ShowError(error);
      }

      return 0;
   }

   private static void ShowInvalidUsage()
   {
      AnsiConsole.MarkupLine("[red]Invalid reference command usage.[/]");
      AnsiConsole.MarkupLine("Usage:");
      AnsiConsole.MarkupLine("  [grey]reference [[--json]][/]");
      AnsiConsole.MarkupLine("  [grey]reference summary [[--json]][/]");
      AnsiConsole.MarkupLine("  [grey]reference profiles [[--json]][/]");
      AnsiConsole.MarkupLine("  [grey]reference profile <name> [[--json]][/]");
      AnsiConsole.MarkupLine("  [grey]reference categories [[--json]][/]");
      AnsiConsole.MarkupLine("  [grey]reference code-groups [[--json]][/]");
      AnsiConsole.MarkupLine("  [grey]reference errors [[--all]] [[--group <code-group>]] [[--category <category>]] [[--json]][/]");
      AnsiConsole.MarkupLine("  [grey]reference error <id-or-name> [[--json]][/]");
   }

   private static void ShowUnknownSubcommand(string subcommand)
   {
      AnsiConsole.MarkupLine(
         "[red]Unknown reference subcommand:[/] {0}",
         Markup.Escape(subcommand));

      ShowInvalidUsage();
   }

   private sealed record ReferenceResult(
      string Subcommand,
      object Value);

   private sealed record ReferenceErrorsResult(
      WhenItFailsReferenceErrorListOptions Options,
      int MatchingCount,
      int ReturnedCount,
      IReadOnlyList<WhenItFailsReferenceErrorSummary> Errors);

   private sealed record ReferenceLookupResult(
      bool Found,
      string Lookup,
      object? Item,
      string? FailureCode,
      string? FailureMessage);
}
