using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Views;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Shows bundled reference catalog information.
/// </summary>
internal static class ReferenceCommand
{
   /// <summary>
   /// Executes the reference command.
   /// </summary>
   /// <returns>Process exit code.</returns>
   public static async Task<int> ExecuteAsync()
   {
      WhenItFailsReferenceCatalogSummarizer summarizer = new();

      WhenItFailsReferenceCatalogSummary summary =
         await summarizer.SummarizeAsync(Environment.CurrentDirectory);

      ReferenceView.Show(summary);

      return 0;
   }
}
