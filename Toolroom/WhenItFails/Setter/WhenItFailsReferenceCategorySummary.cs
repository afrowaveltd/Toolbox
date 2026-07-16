namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter;

/// <summary>
/// Represents one category from the bundled WhenItFails reference catalog.
/// </summary>
internal sealed class WhenItFailsReferenceCategorySummary
{
   /// <summary>
   /// Gets or sets the category name.
   /// </summary>
   public string Name { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the display name.
   /// </summary>
   public string DisplayName { get; set; } = string.Empty;

   /// <summary>
   /// Gets the parent category names.
   /// </summary>
   public List<string> ParentCategoryNames { get; } = new();
}
