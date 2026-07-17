namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Models;

/// <summary>
/// Options for filtering and displaying error lists.
/// </summary>
internal sealed class ErrorListOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to use plain text output instead of rich console output.
    /// </summary>
    public bool UsePlainOutput { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use structured JSON output.
    /// </summary>
    public bool UseJsonOutput { get; set; }

    /// <summary>
    /// Gets or sets the owner filter value.
    /// </summary>
    public string? Owner { get; set; }

    /// <summary>
    /// Gets or sets the code group filter value.
    /// </summary>
    public string? CodeGroup { get; set; }

    /// <summary>
    /// Gets or sets the category filter value.
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Gets or sets the severity filter value.
    /// </summary>
    public string? Severity { get; set; }

    /// <summary>
    /// Gets or sets the profile filter value.
    /// </summary>
    public string? Profile { get; set; }

    /// <summary>
    /// Gets or sets the search text filter value.
    /// </summary>
    public string? SearchText { get; set; }
}
