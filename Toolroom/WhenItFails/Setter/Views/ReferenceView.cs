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

}
