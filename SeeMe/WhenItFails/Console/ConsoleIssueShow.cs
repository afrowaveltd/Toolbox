using Afrowave.Toolbox.WhenItFails.Enums;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.SeeMe.WhenItFails.Console;

/// <summary>
/// Shows a single <see cref="ErrorCatalogValidationIssue"/> in a human-readable console form.
/// </summary>
public sealed class ConsoleIssueShow
{
   /// <summary>
   /// Shows a validation issue in the console.
   /// </summary>
   /// <param name="issue">Validation issue to show.</param>
   /// <param name="options">Optional show options.</param>
   /// <param name="console">Optional Spectre console. If null, <see cref="AnsiConsole.Console"/> is used.</param>
   public void Show(
      ErrorCatalogValidationIssue issue,
      ConsoleShowOptions? options = null,
      IAnsiConsole? console = null)
   {
      ArgumentNullException.ThrowIfNull(issue);

      options ??= new ConsoleShowOptions();
      console ??= AnsiConsole.Console;

      ShowIssue(issue, options, console);
   }

   private static void ShowIssue(
      ErrorCatalogValidationIssue issue,
      ConsoleShowOptions options,
      IAnsiConsole console)
   {
      Color severityColor = GetSeverityColor(issue.Severity, options.Theme);
      string severityColorMarkup = ConsoleMarkupHelper.GetColorMarkup(severityColor);
      string pathColorMarkup = ConsoleMarkupHelper.GetColorMarkup(options.Theme.PathColor);
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

}
