namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter;

/// <summary>
/// Represents one profile from the bundled WhenItFails reference catalog.
/// </summary>
internal sealed class WhenItFailsReferenceProfileSummary
{
   /// <summary>
   /// Gets or sets the profile name.
   /// </summary>
   public string Name { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the display name.
   /// </summary>
   public string DisplayName { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the description.
   /// </summary>
   public string Description { get; set; } = string.Empty;

   /// <summary>
   /// Gets included owner names.
   /// </summary>
   public List<string> IncludedOwnerNames { get; } = new();

   /// <summary>
   /// Gets included code group names.
   /// </summary>
   public List<string> IncludedCodeGroupNames { get; } = new();

   /// <summary>
   /// Gets included category names.
   /// </summary>
   public List<string> IncludedCategoryNames { get; } = new();

   /// <summary>
   /// Gets included subcategory names.
   /// </summary>
   public List<string> IncludedSubcategoryNames { get; } = new();

   /// <summary>
   /// Gets included tag names.
   /// </summary>
   public List<string> IncludedTagNames { get; } = new();

   /// <summary>
   /// Gets excluded tag names.
   /// </summary>
   public List<string> ExcludedTagNames { get; } = new();
}
