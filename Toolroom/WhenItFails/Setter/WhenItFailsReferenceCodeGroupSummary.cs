namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter;

/// <summary>
/// Represents one code group from the bundled WhenItFails reference catalog.
/// </summary>
internal sealed class WhenItFailsReferenceCodeGroupSummary
{
   /// <summary>
   /// Gets or sets the code group name.
   /// </summary>
   public string Name { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the display name.
   /// </summary>
   public string DisplayName { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the code prefix.
   /// </summary>
   public string CodePrefix { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the first reserved code.
   /// </summary>
   public int CodeFrom { get; set; }

   /// <summary>
   /// Gets or sets the last reserved code.
   /// </summary>
   public int CodeTo { get; set; }
}
