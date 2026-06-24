namespace Afrowave.Toolbox.WhenItFails.Enums;

/// <summary>
/// Describes the effective state of the active WhenItFails runtime.
/// </summary>
public enum ErrorCatalogRuntimeState
{
   /// <summary>
   /// The state cannot be determined from the available runtime data.
   /// </summary>
   Unknown = 0,

   /// <summary>
   /// A valid project-local catalog is active.
   /// </summary>
   ProjectCatalog = 1,

   /// <summary>
   /// A previously valid context remains active because
   /// project catalog reinitialization failed.
   /// </summary>
   PreviousContextRecovery = 2,

   /// <summary>
   /// Bundled defaults were activated automatically because
   /// the project catalog could not be loaded.
   /// </summary>
   BuiltInFallback = 3,

   /// <summary>
   /// Bundled defaults were activated explicitly and are not
   /// being used as an automatic recovery fallback.
   /// </summary>
   BuiltInDefaults = 4
}