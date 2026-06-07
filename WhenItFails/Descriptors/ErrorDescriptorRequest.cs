namespace Afrowave.Toolbox.WhenItFails.Descriptors;

/// <summary>
/// Describes a request for creating a runtime error descriptor.
/// </summary>
/// <remarks>
/// This request is intentionally small. It allows callers to resolve an error
/// by id, name or code and optionally override selected human-facing fields.
/// </remarks>
public sealed class ErrorDescriptorRequest
{
    /// <summary>
    /// Gets or sets the error identifier used for lookup.
    /// </summary>
    public string? ErrorId { get; set; }

    /// <summary>
    /// Gets or sets the error name used for lookup.
    /// </summary>
    public string? ErrorName { get; set; }

    /// <summary>
    /// Gets or sets the numeric error code used for lookup.
    /// </summary>
    public int? Code { get; set; }

    /// <summary>
    /// Gets or sets an optional title override.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets an optional message override.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets an optional severity override.
    /// </summary>
    public string? Severity { get; set; }

    /// <summary>
    /// Gets or sets an optional developer hint override.
    /// </summary>
    public string? DeveloperHint { get; set; }

    /// <summary>
    /// Gets or sets an optional documentation key override.
    /// </summary>
    public string? DocumentationKey { get; set; }
}