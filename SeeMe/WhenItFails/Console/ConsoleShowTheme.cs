using Spectre.Console;

namespace Afrowave.Toolbox.SeeMe.WhenItFails.Console;

/// <summary>
/// Defines colors used by console show components.
/// </summary>
public sealed class ConsoleShowTheme
{
   /// <summary>
   /// Gets the default console show theme.
   /// </summary>
   public static ConsoleShowTheme Default { get; } = new();

   /// <summary>
   /// Gets or sets color used for successful validation result.
   /// </summary>
   public Color SuccessColor { get; set; } = Color.Green;

   /// <summary>
   /// Gets or sets color used for errors.
   /// </summary>
   public Color ErrorColor { get; set; } = Color.Red;

   /// <summary>
   /// Gets or sets color used for warnings.
   /// </summary>
   public Color WarningColor { get; set; } = Color.Yellow;

   /// <summary>
   /// Gets or sets color used for informational messages.
   /// </summary>
   public Color InformationColor { get; set; } = Color.Blue;

   /// <summary>
   /// Gets or sets color used for neutral labels and borders.
   /// </summary>
   public Color NeutralColor { get; set; } = Color.Grey;

   /// <summary>
   /// Gets or sets color used for source paths.
   /// </summary>
   public Color PathColor { get; set; } = Color.Aqua;

   /// <summary>
   /// Gets or sets color used for help-like secondary text.
   /// </summary>
   public Color HintColor { get; set; } = Color.Silver;
}
