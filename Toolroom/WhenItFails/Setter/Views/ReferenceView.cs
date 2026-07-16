using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Views;

/// <summary>
/// Shows bundled reference catalog information.
/// </summary>
internal static class ReferenceView
{
   /// <summary>
   /// Shows compact reference catalog summary.
   /// </summary>
   /// <param name="summary">Reference catalog summary.</param>
   public static void Show(WhenItFailsReferenceCatalogSummary summary)
   {
      ArgumentNullException.ThrowIfNull(summary);

      Grid grid = new();

      grid.AddColumn();
      grid.AddColumn();

      grid.AddRow("[grey]Path[/]", Markup.Escape(summary.DisplayPath));
      grid.AddRow("[grey]Owners[/]", summary.OwnerCount.ToString());
      grid.AddRow("[grey]Categories[/]", summary.CategoryCount.ToString());
      grid.AddRow("[grey]Code groups[/]", summary.CodeGroupCount.ToString());
      grid.AddRow("[grey]Profiles[/]", summary.ProfileCount.ToString());
      grid.AddRow("[grey]Errors[/]", summary.ErrorCount.ToString());

      AnsiConsole.Write(
         new Panel(grid)
            .Header("[green]WhenItFails reference catalog[/]")
            .Border(BoxBorder.Rounded));

      if (summary.ProfileNames.Count == 0)
      {
         return;
      }

      Table table = new();

      table.Border(TableBorder.Rounded);
      table.AddColumn("Profile");

      foreach (string profileName in summary.ProfileNames)
      {
         table.AddRow(Markup.Escape(profileName));
      }

      AnsiConsole.Write(table);
   }


   /// <summary>
   /// Shows reference catalog profiles.
   /// </summary>
   /// <param name="summary">Reference catalog summary.</param>
   public static void ShowProfiles(WhenItFailsReferenceCatalogSummary summary)
   {
      ArgumentNullException.ThrowIfNull(summary);

      Table table = new();

      table.Border(TableBorder.Rounded);
      table.AddColumn("Profile");

      AnsiConsole.MarkupLine("[green]WhenItFails reference profiles[/]");

      foreach (string profileName in summary.ProfileNames)
      {
         table.AddRow(Markup.Escape(profileName));
      }

      AnsiConsole.Write(table);
   }

   /// <summary>
   /// Shows reference catalog categories.
   /// </summary>
   /// <param name="summary">Reference catalog summary.</param>
   public static void ShowCategories(WhenItFailsReferenceCatalogSummary summary)
   {
      ArgumentNullException.ThrowIfNull(summary);

      Table table = new();

      table.Border(TableBorder.Rounded);
      table.AddColumn("Name");
      table.AddColumn("Display name");
      table.AddColumn("Parents");

      AnsiConsole.MarkupLine("[green]WhenItFails reference categories[/]");

      foreach (WhenItFailsReferenceCategorySummary category in summary.Categories)
      {
         string parents = category.ParentCategoryNames.Count == 0
            ? string.Empty
            : string.Join(", ", category.ParentCategoryNames);

         table.AddRow(
            Markup.Escape(category.Name),
            Markup.Escape(category.DisplayName),
            Markup.Escape(parents));
      }

      AnsiConsole.Write(table);
   }


   /// <summary>
   /// Shows reference catalog code groups.
   /// </summary>
   /// <param name="summary">Reference catalog summary.</param>
   public static void ShowCodeGroups(WhenItFailsReferenceCatalogSummary summary)
   {
      ArgumentNullException.ThrowIfNull(summary);

      Table table = new();

      table.Border(TableBorder.Rounded);
      table.AddColumn("Name");
      table.AddColumn("Display name");
      table.AddColumn("Prefix");
      table.AddColumn("Range");

      AnsiConsole.MarkupLine("[green]WhenItFails reference code groups[/]");

      foreach (WhenItFailsReferenceCodeGroupSummary codeGroup in summary.CodeGroups)
      {
         string range = codeGroup.CodeFrom.ToString() + "–" + codeGroup.CodeTo.ToString();

         table.AddRow(
            Markup.Escape(codeGroup.Name),
            Markup.Escape(codeGroup.DisplayName),
            Markup.Escape(codeGroup.CodePrefix),
            Markup.Escape(range));
      }

      AnsiConsole.Write(table);
   }


   /// <summary>
   /// Shows reference catalog errors.
   /// </summary>
   /// <param name="summary">Reference catalog summary.</param>
   /// <param name="options">Reference error list options.</param>
   public static void ShowErrors(
      WhenItFailsReferenceCatalogSummary summary,
      WhenItFailsReferenceErrorListOptions options)
   {
      ArgumentNullException.ThrowIfNull(summary);
      ArgumentNullException.ThrowIfNull(options);

      const int DefaultLimit = 20;

      List<WhenItFailsReferenceErrorSummary> matchingErrors = summary.Errors
         .Where(error => MatchesErrorListOptions(error, options))
         .ToList();

      IReadOnlyList<WhenItFailsReferenceErrorSummary> errors = options.ShowAll
         ? matchingErrors
         : matchingErrors.Take(DefaultLimit).ToList();

      Table table = new();

      table.Border(TableBorder.Rounded);
      table.AddColumn("Id");
      table.AddColumn("Code");
      table.AddColumn("Group");
      table.AddColumn("Name");

      AnsiConsole.MarkupLine("[green]WhenItFails reference errors[/]");

      foreach (WhenItFailsReferenceErrorSummary error in errors)
      {
         table.AddRow(
            Markup.Escape(error.Id),
            error.Code.ToString(),
            Markup.Escape(error.CodeGroup),
            Markup.Escape(error.Name));
      }

      AnsiConsole.Write(table);

      if (!options.ShowAll && matchingErrors.Count > DefaultLimit)
      {
         int hiddenCount = matchingErrors.Count - DefaultLimit;

         AnsiConsole.MarkupLine(
            "[grey]{0} more error(s) hidden. Use [white]reference errors --all[/] to show all matching errors.[/]",
            hiddenCount);
      }
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


   /// <summary>
   /// Shows one reference catalog error in detail.
   /// </summary>
   /// <param name="error">Reference error summary.</param>
   public static void ShowError(WhenItFailsReferenceErrorSummary error)
   {
      ArgumentNullException.ThrowIfNull(error);

      Grid grid = new();

      grid.AddColumn();
      grid.AddColumn();

      grid.AddRow("[grey]Id[/]", Markup.Escape(error.Id));
      grid.AddRow("[grey]Code[/]", error.Code.ToString());
      grid.AddRow("[grey]Name[/]", Markup.Escape(error.Name));
      grid.AddRow("[grey]Group[/]", Markup.Escape(error.CodeGroup));
      grid.AddRow("[grey]Primary category[/]", Markup.Escape(error.PrimaryCategory));
      grid.AddRow("[grey]Categories[/]", Markup.Escape(string.Join(", ", error.CategoryNames)));
      grid.AddRow("[grey]Severity[/]", Markup.Escape(error.DefaultSeverity));
      grid.AddRow("[grey]Title[/]", Markup.Escape(error.Title));
      grid.AddRow("[grey]Message[/]", Markup.Escape(error.Message));
      grid.AddRow("[grey]Developer hint[/]", Markup.Escape(error.DeveloperHint));
      grid.AddRow("[grey]Tags[/]", Markup.Escape(string.Join(", ", error.TagNames)));

      AnsiConsole.Write(
         new Panel(grid)
            .Header("[green]WhenItFails reference error[/]")
            .Border(BoxBorder.Rounded));
   }


   /// <summary>
   /// Shows one reference catalog profile in detail.
   /// </summary>
   /// <param name="profile">Reference profile summary.</param>
   public static void ShowProfile(WhenItFailsReferenceProfileSummary profile)
   {
      ArgumentNullException.ThrowIfNull(profile);

      Grid grid = new();

      grid.AddColumn();
      grid.AddColumn();

      grid.AddRow("[grey]Name[/]", Markup.Escape(profile.Name));
      grid.AddRow("[grey]Display name[/]", Markup.Escape(profile.DisplayName));
      grid.AddRow("[grey]Description[/]", Markup.Escape(profile.Description));
      grid.AddRow("[grey]Owners[/]", Markup.Escape(string.Join(", ", profile.IncludedOwnerNames)));
      grid.AddRow("[grey]Code groups[/]", Markup.Escape(string.Join(", ", profile.IncludedCodeGroupNames)));
      grid.AddRow("[grey]Categories[/]", Markup.Escape(string.Join(", ", profile.IncludedCategoryNames)));
      grid.AddRow("[grey]Subcategories[/]", Markup.Escape(string.Join(", ", profile.IncludedSubcategoryNames)));
      grid.AddRow("[grey]Tags[/]", Markup.Escape(string.Join(", ", profile.IncludedTagNames)));
      grid.AddRow("[grey]Excluded tags[/]", Markup.Escape(string.Join(", ", profile.ExcludedTagNames)));

      AnsiConsole.Write(
         new Panel(grid)
            .Header("[green]WhenItFails reference profile[/]")
            .Border(BoxBorder.Rounded));
   }

}
