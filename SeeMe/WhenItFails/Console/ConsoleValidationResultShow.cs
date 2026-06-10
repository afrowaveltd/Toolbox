using Afrowave.Toolbox.WhenItFails.Enums;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.SeeMe.WhenItFails.Console;

/// <summary>
/// Shows <see cref="ErrorCatalogValidationResult"/> instances in a human-readable console form.
/// </summary>
public sealed class ConsoleValidationResultShow
{
   /// <summary>
   /// Shows a validation result in the console.
   /// </summary>
   /// <param name="validationResult">Validation result to show.</param>
   /// <param name="options">Optional show options.</param>
   /// <param name="console">Optional Spectre console. If null, <see cref="AnsiConsole.Console"/> is used.</param>
   public void Show(
      ErrorCatalogValidationResult validationResult,
      ConsoleShowOptions? options = null,
      IAnsiConsole? console = null)
   {
      ArgumentNullException.ThrowIfNull(validationResult);

      options ??= new ConsoleShowOptions();
      console ??= AnsiConsole.Console;

      IReadOnlyList<ErrorCatalogValidationIssue> matchingIssues = GetMatchingIssues(validationResult, options);

      if (options.ShowSummary)
      {
         ShowSummary(validationResult, matchingIssues, options, console);
      }

      if (matchingIssues.Count == 0)
      {
         return;
      }

      console.WriteLine();

      ConsoleIssueShow issueShow = new();

      foreach (ErrorCatalogValidationIssue issue in matchingIssues)
      {
         issueShow.Show(issue, options, console);
         console.WriteLine();
      }
   }

   private static IReadOnlyList<ErrorCatalogValidationIssue> GetMatchingIssues(
      ErrorCatalogValidationResult validationResult,
      ConsoleShowOptions options)
   {
      IEnumerable<ErrorCatalogValidationIssue> matchingIssues = validationResult.Issues
         .Where(issue => ShouldShowIssue(issue, options));

      if (options.MaxIssues is not null)
      {
         matchingIssues = matchingIssues.Take(Math.Max(0, options.MaxIssues.Value));
      }

      return matchingIssues.ToList();
   }

   private static bool ShouldShowIssue(
      ErrorCatalogValidationIssue issue,
      ConsoleShowOptions options)
   {
      return issue.Severity switch
      {
         ErrorCatalogValidationSeverity.Error => options.ShowErrors,
         ErrorCatalogValidationSeverity.Warning => options.ShowWarnings,
         ErrorCatalogValidationSeverity.Information => options.ShowInformation,
         _ => true
      };
   }

   private static void ShowSummary(
      ErrorCatalogValidationResult validationResult,
      IReadOnlyList<ErrorCatalogValidationIssue> matchingIssues,
      ConsoleShowOptions options,
      IAnsiConsole console)
   {
      int errorCount = validationResult.Issues.Count(issue => issue.Severity == ErrorCatalogValidationSeverity.Error);
      int warningCount = validationResult.Issues.Count(issue => issue.Severity == ErrorCatalogValidationSeverity.Warning);
      int informationCount = validationResult.Issues.Count(issue => issue.Severity == ErrorCatalogValidationSeverity.Information);

      string title = validationResult.IsValid
         ? "[bold green]Validation passed[/]"
         : "[bold red]Validation failed[/]";

      Table summaryTable = new Table()
         .NoBorder()
         .AddColumn("Name")
         .AddColumn("Value");

      if (!string.IsNullOrWhiteSpace(options.SourcePath))
      {
         summaryTable.AddRow(
            "[grey]Source[/]",
            $"[{ConsoleMarkupHelper.GetColorMarkup(options.Theme.PathColor)}]{Markup.Escape(options.SourcePath)}[/]");
      }

      summaryTable.AddRow("[grey]Errors[/]", FormatCount(errorCount, options.Theme.ErrorColor));
      summaryTable.AddRow("[grey]Warnings[/]", FormatCount(warningCount, options.Theme.WarningColor));
      summaryTable.AddRow("[grey]Information[/]", FormatCount(informationCount, options.Theme.InformationColor));
      summaryTable.AddRow("[grey]Shown[/]", FormatCount(matchingIssues.Count, options.Theme.NeutralColor));

      Color borderColor = validationResult.IsValid
         ? options.Theme.SuccessColor
         : options.Theme.ErrorColor;

      console.Write(
         new Panel(summaryTable)
            .Header(title)
            .Border(BoxBorder.Rounded)
            .BorderColor(borderColor));
   }

   private static string FormatCount(int count, Color color)
   {
      return $"[{ConsoleMarkupHelper.GetColorMarkup(color)}]{count}[/]";
   }
}
