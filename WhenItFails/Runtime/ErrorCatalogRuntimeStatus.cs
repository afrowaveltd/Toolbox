
using Afrowave.Toolbox.WhenItFails.Enums;

namespace Afrowave.Toolbox.WhenItFails.Runtime;

/// <summary>
/// Describes the currently active WhenItFails runtime state.
/// </summary>
public sealed class ErrorCatalogRuntimeStatus
{
    /// <summary>
    /// Gets the source of the currently active catalog context.
    /// </summary>
    public ErrorCatalogContextSource ContextSource { get; init; }

    /// <summary>
    /// Gets a value indicating whether the runtime is operating
    /// in a degraded recovery state.
    /// </summary>
    public bool IsDegraded { get; init; }

    /// <summary>
    /// Gets a value indicating whether a previously valid context
    /// was retained after a failed initialization attempt.
    /// </summary>
    public bool KeptPreviousContext { get; init; }

    /// <summary>
    /// Gets a value indicating whether an automatic built-in fallback
    /// was activated.
    /// </summary>
    public bool UsedFallback { get; init; }

    /// <summary>
    /// Gets the UTC timestamp at which this runtime state became active.
    /// </summary>
    public DateTimeOffset ActivatedAtUtc { get; init; }

    /// <summary>
    /// Gets the configured project-local package directory path.
    /// </summary>
    /// <remarks>
    /// When built-in defaults are active, this remains the configured
    /// project workspace path. Built-in templates themselves are loaded
    /// from an isolated temporary workspace.
    /// </remarks>
    public string PackageDirectoryPath { get; init; } = string.Empty;

}