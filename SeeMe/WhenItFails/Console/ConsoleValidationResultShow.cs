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

      foreach (ErrorCatalogValidationIssue issue in matchingIssues)
      {
         ShowIssue(issue, options, console);
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
            $"[{GetColorMarkup(options.Theme.PathColor)}]{Markup.Escape(options.SourcePath)}[/]");
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
      string severityColorMarkup = GetColorMarkup(severityColor);
      string pathColorMarkup = GetColorMarkup(options.Theme.PathColor);
      string severityLabel = Markup.Escape(GetSeverityLabel(issue.Severity));
      string issueCode = Markup.Escape($"[{issue.Code}]");
      string issueMessage = Markup.Escape(issue.Message);

      console.MarkupLine(
         $"[bold {severityColorMarkup}]{severityLabel}[/][grey]{issueCode}[/]: {issueMessage}");

      if (!string.IsNullOrWhiteSpace(options.SourcePath))
      {
         string sourcePath = Markup.Escape(options.SourcePath);

         console.MarkupLine(
            $"  [grey]-->[/] [{pathColorMarkup}]{sourcePath}[/]");
      }

      if (options.ShowPath && !string.IsNullOrWhiteSpace(issue.Path))
      {
         string issuePath = Markup.Escape(issue.Path);

         console.MarkupLine("   [grey]|[/]");
         console.MarkupLine(
            $"   [grey]|[/] [{pathColorMarkup}]{issuePath}[/]");
         console.MarkupLine(
            $"   [grey]|[/] [{severityColorMarkup}]^ validation path[/]");
      }

      if (options.ShowRelatedError && HasRelatedError(issue))
      {
         console.MarkupLine("   [grey]|[/]");

         if (!string.IsNullOrWhiteSpace(issue.ErrorId))
         {
            string errorId = Markup.Escape(issue.ErrorId);

            console.MarkupLine(
               $"   [grey]= related error id:[/] {errorId}");
         }

         if (!string.IsNullOrWhiteSpace(issue.ErrorName))
         {
            string errorName = Markup.Escape(issue.ErrorName);

            console.MarkupLine(
               $"   [grey]= related error name:[/] {errorName}");
         }
      }
   }

   private static bool HasRelatedError(ErrorCatalogValidationIssue issue)
   {
      return !string.IsNullOrWhiteSpace(issue.ErrorId)
         || !string.IsNullOrWhiteSpace(issue.ErrorName);
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

   private static string GetColorMarkup(Color color)
   {
      if (color == Color.Red)
      {
         return "red";
      }

      if (color == Color.Yellow)
      {
         return "yellow";
      }

      if (color == Color.Blue)
      {
         return "blue";
      }

      if (color == Color.Green)
      {
         return "green";
      }

      if (color == Color.Aqua)
      {
         return "aqua";
      }

      if (color == Color.Grey)
      {
         return "grey";
      }

      if (color == Color.Silver)
      {
         return "silver";
      }

      return "white";
   }

   private static string FormatCount(int count, Color color)
   {
      return $"[{GetColorMarkup(color)}]{count}[/]";
   }
}
