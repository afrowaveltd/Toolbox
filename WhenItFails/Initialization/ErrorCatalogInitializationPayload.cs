using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Enums;
using Afrowave.Toolbox.WhenItFails.Runtime;

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

   /// <summary>
   /// Gets or sets the source of the context that remained active
   /// after initialization.
   /// </summary>
   public ErrorCatalogContextSource ContextSource { get; set; }
       = ErrorCatalogContextSource.ProjectCatalog;

   /// <summary>
   /// Gets or sets whether the previously active valid context
   /// was retained because a new context could not be activated.
   /// </summary>
   public bool KeptPreviousContext { get; set; }

   /// <summary>
   /// Gets or sets whether the bundled Afrowave default catalog
   /// was activated as a fallback.
   /// </summary>
   public bool UsedFallback { get; set; }

   /// <summary>
   /// Gets the exact publication created by a successful default
   /// initializer/runtime write, when the context store supports ownership.
   /// Internal infrastructure data: not exposed as a public API or serialized.
   /// Null for custom/legacy paths that cannot prove their write identity.
   /// </summary>
   internal ErrorCatalogContextPublication? OwnedPublication { get; set; }

   /// <summary>
   /// Gets whether initialization completed with a valid context
   /// but required recovery behavior.
   /// </summary>
   public bool IsDegraded =>
       KeptPreviousContext
       || UsedFallback;
}