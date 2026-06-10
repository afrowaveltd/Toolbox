using Spectre.Console;

namespace Afrowave.Toolbox.SeeMe.WhenItFails.Console;

/// <summary>
/// Provides shared helpers for Spectre.Console markup used by console show components.
/// </summary>
internal static class ConsoleMarkupHelper
{
   /// <summary>
   /// Gets a Spectre.Console markup color name for a known color.
   /// </summary>
   /// <param name="color">Color to convert.</param>
   /// <returns>Markup color name.</returns>
   public static string GetColorMarkup(Color color)
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
}
