using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter;

/// <summary>
/// Contains a read-only summary of a WhenItFails JSON workspace.
/// </summary>
internal sealed class WhenItFailsWorkspaceSummary
{
    /// <summary>
    /// Gets or sets the resolved WhenItFails package directory path.
    /// </summary>
    public string PackageDirectoryPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display path used in console output.
    /// </summary>
    public string DisplayPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the loaded error catalog document.
    /// </summary>
    public ErrorCatalogDocument ErrorCatalog { get; set; } = new();

    /// <summary>
    /// Gets or sets the loaded category catalog document.
    /// </summary>
    public ErrorCategoryCatalogDocument CategoryCatalog { get; set; } = new();

    /// <summary>
    /// Gets or sets the loaded code group catalog document.
    /// </summary>
    public ErrorCodeGroupCatalogDocument CodeGroupCatalog { get; set; } = new();

    /// <summary>
    /// Gets or sets the loaded owner catalog document.
    /// </summary>
    public ErrorOwnerCatalogDocument OwnerCatalog { get; set; } = new();

    /// <summary>
    /// Gets or sets the loaded profile catalog document.
    /// </summary>
    public ErrorProfileCatalogDocument ProfileCatalog { get; set; } = new();

    /// <summary>
    /// Gets the number of error definitions.
    /// </summary>
    public int ErrorCount => ErrorCatalog.Errors.Count;

    /// <summary>
    /// Gets the number of category definitions.
    /// </summary>
    public int CategoryCount => CategoryCatalog.Categories.Count;

    /// <summary>
    /// Gets the number of code group definitions.
    /// </summary>
    public int CodeGroupCount => CodeGroupCatalog.CodeGroups.Count;

    /// <summary>
    /// Gets the number of owner definitions.
    /// </summary>
    public int OwnerCount => OwnerCatalog.Owners.Count;

    /// <summary>
    /// Gets the number of profile definitions.
    /// </summary>
    public int ProfileCount => ProfileCatalog.Profiles.Count;
}