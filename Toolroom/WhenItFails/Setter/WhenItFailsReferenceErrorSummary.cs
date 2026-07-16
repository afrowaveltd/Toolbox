namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter;

/// <summary>
/// Represents one error from the bundled WhenItFails reference catalog.
/// </summary>
internal sealed class WhenItFailsReferenceErrorSummary
{
   /// <summary>
   /// Gets or sets the error id.
   /// </summary>
   public string Id { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the numeric error code.
   /// </summary>
   public int Code { get; set; }

   /// <summary>
   /// Gets or sets the error name.
   /// </summary>
   public string Name { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the code group name.
   /// </summary>
   public string CodeGroup { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the primary category name.
   /// </summary>
   public string PrimaryCategory { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the human-readable title.
   /// </summary>
   public string Title { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the user-facing message.
   /// </summary>
   public string Message { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the default severity.
   /// </summary>
   public string DefaultSeverity { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the developer hint.
   /// </summary>
   public string DeveloperHint { get; set; } = string.Empty;

   /// <summary>
   /// Gets the category names.
   /// </summary>
   public List<string> CategoryNames { get; } = new();

   /// <summary>
   /// Gets the tag names.
   /// </summary>
   public List<string> TagNames { get; } = new();
}
