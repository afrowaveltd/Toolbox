using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Catalog;

namespace Afrowave.Toolbox.WhenItFails.Initialization;

/// <summary>
/// Contains the result of a completed WhenItFails catalog initialization.
/// </summary>
public sealed class ErrorCatalogInitializationPayload
{
   /// <summary>
   /// Gets or sets details about the prepared project-local JSON workspace.
   /// </summary>
   public JsonsBootstrapPayload Bootstrap { get; set; } = null!;

   /// <summary>
   /// Gets or sets the loaded and validated error catalog context.
   /// </summary>
   public ErrorCatalogContext Context { get; set; } = null!;
}