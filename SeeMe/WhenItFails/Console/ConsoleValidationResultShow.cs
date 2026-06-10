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

      foreach (ErrorCatalogValidationIssue issue in matchingIssues)
      {
         ShowIssue(issue, options, console);
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
            $"[{options.Theme.PathColor}]{Markup.Escape(options.SourcePath)}[/]");
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

   private static void ShowIssue(
      ErrorCatalogValidationIssue issue,
      ConsoleShowOptions options,
      IAnsiConsole console)
   {
      Color severityColor = GetSeverityColor(issue.Severity, options.Theme);
      string severityLabel = GetSeverityLabel(issue.Severity);

      Grid grid = new Grid();
      grid.AddColumn(new GridColumn().NoWrap());
      grid.AddColumn();

      grid.AddRow(
         $"[{severityColor}]code[/]",
         $"[bold {severityColor}]{Markup.Escape(issue.Code)}[/]");

      grid.AddRow(
         $"[{severityColor}]message[/]",
         Markup.Escape(issue.Message));

      if (options.ShowPath && !string.IsNullOrWhiteSpace(issue.Path))
      {
         grid.AddRow(
            $"[{options.Theme.PathColor}]path[/]",
            $"[{options.Theme.PathColor}]{Markup.Escape(issue.Path)}[/]");
      }

      if (options.ShowRelatedError && !string.IsNullOrWhiteSpace(issue.ErrorId))
      {
         grid.AddRow(
            $"[{options.Theme.NeutralColor}]error id[/]",
            Markup.Escape(issue.ErrorId));
      }

      if (options.ShowRelatedError && !string.IsNullOrWhiteSpace(issue.ErrorName))
      {
         grid.AddRow(
            $"[{options.Theme.NeutralColor}]error name[/]",
            Markup.Escape(issue.ErrorName));
      }

      console.Write(
         new Panel(grid)
            .Header($"[bold {severityColor}]{severityLabel}[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(severityColor));
   }

   private static Color GetSeverityColor(
      ErrorCatalogValidationSeverity severity,
      ConsoleShowTheme theme)
   {
      return severity switch
      {
         ErrorCatalogValidationSeverity.Error => theme.ErrorColor,
         ErrorCatalogValidationSeverity.Warning => theme.WarningColor,
         ErrorCatalogValidationSeverity.Information => theme.InformationColor,
         _ => theme.NeutralColor
      };
   }

   private static string GetSeverityLabel(ErrorCatalogValidationSeverity severity)
   {
      return severity switch
      {
         ErrorCatalogValidationSeverity.Error => "error",
         ErrorCatalogValidationSeverity.Warning => "warning",
         ErrorCatalogValidationSeverity.Information => "information",
         _ => "issue"
      };
   }

   private static string FormatCount(int count, Color color)
   {
      return $"[{color}]{count}[/]";
   }
}
