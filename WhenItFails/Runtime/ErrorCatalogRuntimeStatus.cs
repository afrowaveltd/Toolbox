using Afrowave.Toolbox.Essentials.Enums;
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
    /// Gets the effective semantic runtime state.
    /// </summary>
    public ErrorCatalogRuntimeState State =>
        ContextSource switch
        {
            ErrorCatalogContextSource.ProjectCatalog
                when !IsDegraded
                     && !KeptPreviousContext
                     && !UsedFallback =>
                ErrorCatalogRuntimeState.ProjectCatalog,

            ErrorCatalogContextSource.PreviousContext
                when IsDegraded
                     && KeptPreviousContext
                     && !UsedFallback =>
                ErrorCatalogRuntimeState.PreviousContextRecovery,

            ErrorCatalogContextSource.BuiltInDefaults
                when IsDegraded
                     && !KeptPreviousContext
                     && UsedFallback =>
                ErrorCatalogRuntimeState.BuiltInFallback,

            ErrorCatalogContextSource.BuiltInDefaults
                when !IsDegraded
                     && !KeptPreviousContext
                     && !UsedFallback =>
                ErrorCatalogRuntimeState.BuiltInDefaults,

            _ =>
                ErrorCatalogRuntimeState.Unknown
        };

    /// <summary>
    /// Gets a value indicating whether the runtime status contains
    /// a valid and internally consistent combination of state values.
    /// </summary>
    public bool IsConsistent =>
        State != ErrorCatalogRuntimeState.Unknown;
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
    /// Gets the code describing why recovery mode was activated.
    /// </summary>
    /// <remarks>
    /// This value is null when the active context was loaded normally
    /// or activated through an explicit reset.
    /// </remarks>
    public string? RecoveryReasonCode { get; init; }

    /// <summary>
    /// Gets the result status of the failed initialization attempt
    /// that caused recovery mode to be activated.
    /// </summary>
    public ResultStatus? RecoveryStatus { get; init; }

    /// <summary>
    /// Gets the message associated with the failed initialization attempt
    /// that caused recovery mode to be activated.
    /// </summary>
    public string? RecoveryMessage { get; init; }

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