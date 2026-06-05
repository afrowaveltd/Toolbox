namespace Afrowave.Toolbox.WhenItFails.Bootstrap;

/// <summary>
/// Represents one bundled JSON template file that can be copied into the project workspace.
/// </summary>
public sealed class JsonsTemplateFile
{
    /// <summary>
    /// Gets or sets the logical template name.
    /// </summary>
    /// <example>Error catalog</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target file name inside the package JSON workspace.
    /// </summary>
    /// <example>errors.en.json</example>
    public string TargetFileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the JSON template content.
    /// </summary>
    public string Content { get; set; } = string.Empty;
}