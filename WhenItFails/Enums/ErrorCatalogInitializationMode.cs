namespace Afrowave.Toolbox.WhenItFails.Enums;

/// <summary>
/// Defines how the error catalog runtime responds to catalog
/// initialization and reinitialization failures.
/// </summary>
public enum ErrorCatalogInitializationMode
{
    /// <summary>
    /// Requires the requested catalog initialization to succeed.
    /// No automatic fallback catalog is activated.
    /// </summary>
    /// <remarks>
    /// When a previously initialized valid context exists, that context
    /// remains active if reinitialization fails.
    /// </remarks>
    Strict = 0,

    /// <summary>
    /// Allows the runtime to recover from catalog initialization failures.
    /// </summary>
    /// <remarks>
    /// The runtime first retains an existing valid context. When no previous
    /// context exists, it may activate the bundled Afrowave default catalog.
    /// </remarks>
    Flexible = 1
}