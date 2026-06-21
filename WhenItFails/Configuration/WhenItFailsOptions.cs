using Afrowave.Toolbox.WhenItFails.Enums;

namespace Afrowave.Toolbox.WhenItFails.Configuration;

/// <summary>
/// Defines the main configuration of the WhenItFails runtime.
/// </summary>
/// <remarks>
/// Configuration can be supplied directly by code, through dependency
/// injection, from configuration files, or by any other mechanism chosen
/// by the consuming application.
/// </remarks>
public sealed class WhenItFailsOptions
{
    /// <summary>
    /// Gets or sets the project-local JSON workspace configuration.
    /// </summary>
    public JsonsOptions Jsons { get; set; } = new();

    /// <summary>
    /// Gets or sets the catalog initialization behavior.
    /// </summary>
    public ErrorCatalogInitializationMode InitializationMode { get; set; }
        = ErrorCatalogInitializationMode.Flexible;

    /// <summary>
    /// Gets or sets whether successfully recovered failures should be hidden
    /// from the normal public result flow.
    /// </summary>
    /// <remarks>
    /// A value of <see langword="null"/> means that no explicit override was
    /// supplied and the safe default behavior is used.
    ///
    /// A value of <see langword="true"/> may hide only failures from which
    /// the runtime recovered successfully by retaining a valid previous
    /// context or activating a valid fallback context.
    ///
    /// Unrecoverable failures, cancellation, invalid API usage and fatal
    /// runtime failures must never be hidden by this option.
    /// </remarks>
    public bool? HideRecoverableFailures { get; set; }
}