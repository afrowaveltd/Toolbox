namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter;

/// <summary>
/// Represents a compact summary of the bundled WhenItFails reference catalog.
/// </summary>
internal sealed class WhenItFailsReferenceCatalogSummary
{
   /// <summary>
   /// Gets or sets the resolved reference catalog directory path.
   /// </summary>
   public string DirectoryPath { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the reference catalog display path.
   /// </summary>
   public string DisplayPath { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the number of owners.
   /// </summary>
   public int OwnerCount { get; set; }

   /// <summary>
   /// Gets or sets the number of categories.
   /// </summary>
   public int CategoryCount { get; set; }

   /// <summary>
   /// Gets or sets the number of code groups.
   /// </summary>
   public int CodeGroupCount { get; set; }

   /// <summary>
   /// Gets or sets the number of profiles.
   /// </summary>
   public int ProfileCount { get; set; }

   /// <summary>
   /// Gets or sets the number of errors.
   /// </summary>
   public int ErrorCount { get; set; }

   /// <summary>
   /// Gets the profile names.
   /// </summary>
   public List<string> ProfileNames { get; } = new();

   /// <summary>
   /// Gets the category summaries.
   /// </summary>
   public List<WhenItFailsReferenceCategorySummary> Categories { get; } = new();
}
