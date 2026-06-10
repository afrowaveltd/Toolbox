namespace Afrowave.Toolbox.SeeMe.WhenItFails.Console;

/// <summary>
/// Defines options for showing WhenItFails validation results in the console.
/// </summary>
public sealed class ConsoleShowOptions
{
   /// <summary>
   /// Gets or sets an optional file path displayed as the source of the validation result.
   /// </summary>
   public string? SourcePath { get; set; }

   /// <summary>
   /// Gets or sets a value indicating whether error issues should be shown.
   /// </summary>
   public bool ShowErrors { get; set; } = true;

   /// <summary>
   /// Gets or sets a value indicating whether warning issues should be shown.
   /// </summary>
   public bool ShowWarnings { get; set; } = true;

   /// <summary>
   /// Gets or sets a value indicating whether informational issues should be shown.
   /// </summary>
   public bool ShowInformation { get; set; } = true;

   /// <summary>
   /// Gets or sets a value indicating whether a summary panel should be shown.
   /// </summary>
   public bool ShowSummary { get; set; } = true;

   /// <summary>
   /// Gets or sets a value indicating whether issue path should be shown when available.
   /// </summary>
   public bool ShowPath { get; set; } = true;

   /// <summary>
   /// Gets or sets a value indicating whether related error id and name should be shown when available.
   /// </summary>
   public bool ShowRelatedError { get; set; } = true;

   /// <summary>
   /// Gets or sets maximum number of issues to show.
   /// Null means all matching issues are shown.
   /// </summary>
   public int? MaxIssues { get; set; }

   /// <summary>
   /// Gets or sets text shown under the validation path marker.
   /// </summary>
   public string LocationMarkerText { get; set; } = "here";

   /// <summary>
   /// Gets or sets the theme used for console output.
   /// </summary>
   public ConsoleShowTheme Theme { get; set; } = ConsoleShowTheme.Default;
}
