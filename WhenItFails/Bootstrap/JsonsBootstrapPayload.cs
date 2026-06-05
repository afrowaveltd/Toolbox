namespace Afrowave.Toolbox.WhenItFails.Bootstrap;

/// <summary>
/// Contains information produced by the JSON workspace bootstrap process.
/// </summary>
public sealed class JsonsBootstrapPayload
{
    /// <summary>
    /// Gets or sets the root JSON directory.
    /// </summary>
    public string RootDirectory { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the package-specific JSON workspace directory.
    /// </summary>
    public string PackageDirectoryPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the package directory already existed.
    /// </summary>
    public bool PackageDirectoryAlreadyExisted { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the package directory was created.
    /// </summary>
    public bool PackageDirectoryCreated { get; set; }

    /// <summary>
    /// Gets file-level bootstrap results.
    /// </summary>
    public List<JsonsBootstrapFileResult> Files { get; } = new();
}