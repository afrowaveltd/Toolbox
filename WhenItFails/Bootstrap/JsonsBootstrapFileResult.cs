namespace Afrowave.Toolbox.WhenItFails.Bootstrap;

/// <summary>
/// Represents bootstrap status of one project-local JSON file.
/// </summary>
public sealed class JsonsBootstrapFileResult
{
    /// <summary>
    /// Gets or sets the logical file name.
    /// </summary>
    /// <example>Error catalog</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target project-local file path.
    /// </summary>
    public string TargetFilePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the file already existed before bootstrap.
    /// </summary>
    public bool AlreadyExisted { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the file was created by bootstrap.
    /// </summary>
    public bool Created { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the file was skipped.
    /// </summary>
    public bool Skipped { get; set; }

    /// <summary>
    /// Gets or sets an optional message describing what happened.
    /// </summary>
    public string? Message { get; set; }
}