namespace Afrowave.Toolbox.WhenItFails.Enums;

/// <summary>
/// Identifies the source of the error catalog context
/// that remains active after initialization.
/// </summary>
public enum ErrorCatalogContextSource
{
    /// <summary>
    /// The active context was loaded from the configured
    /// project-local catalog workspace.
    /// </summary>
    ProjectCatalog = 0,

    /// <summary>
    /// Initialization of a new context failed and the previously
    /// active valid context was retained.
    /// </summary>
    PreviousContext = 1,

    /// <summary>
    /// The project-local catalog could not be activated and the
    /// bundled Afrowave default catalog was used instead.
    /// </summary>
    BuiltInDefaults = 2
}