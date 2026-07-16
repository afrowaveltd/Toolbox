namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter;

/// <summary>
/// Represents options used when listing reference catalog errors.
/// </summary>
internal sealed class WhenItFailsReferenceErrorListOptions
{
   /// <summary>
   /// Gets or sets a value indicating whether all matching errors should be shown.
   /// </summary>
   public bool ShowAll { get; set; }

   /// <summary>
   /// Gets or sets the optional code group filter.
   /// </summary>
   public string? CodeGroup { get; set; }

   /// <summary>
   /// Gets or sets the optional category filter.
   /// </summary>
   public string? Category { get; set; }
}
