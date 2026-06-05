namespace Afrowave.Toolbox.WhenItFails.Configuration;

/// <summary>
/// Defines project JSON workspace paths used by this package.
/// </summary>
/// <remarks>
/// The package uses bundled read-only JSON catalogs as source templates,
/// but runtime code should work with project-local copies.
///
/// Default project layout:
///
/// <code>
/// Jsons/
///   WhenItFails/
///     errors.en.json
///     categories.en.json
///     code-groups.en.json
///     owners.en.json
///     profiles.json
/// </code>
///
/// Project files should be created only when missing.
/// Existing project files must not be overwritten automatically.
/// </remarks>
public sealed class JsonsOptions
{
    /// <summary>
    /// Gets or sets the root JSON directory.
    /// </summary>
    public string RootDirectory { get; set; } = "Jsons";

    /// <summary>
    /// Gets or sets the package-specific directory name under <see cref="RootDirectory"/>.
    /// </summary>
    public string PackageDirectoryName { get; set; } = "WhenItFails";

    /// <summary>
    /// Gets or sets the default error catalog file name.
    /// </summary>
    public string ErrorCatalogFileName { get; set; } = "errors.en.json";

    /// <summary>
    /// Gets or sets the default category catalog file name.
    /// </summary>
    public string CategoryCatalogFileName { get; set; } = "categories.en.json";

    /// <summary>
    /// Gets or sets the default code group catalog file name.
    /// </summary>
    public string CodeGroupCatalogFileName { get; set; } = "code-groups.en.json";

    /// <summary>
    /// Gets or sets the default owner catalog file name.
    /// </summary>
    public string OwnerCatalogFileName { get; set; } = "owners.en.json";

    /// <summary>
    /// Gets or sets the default profile catalog file name.
    /// </summary>
    public string ProfilesFileName { get; set; } = "profiles.json";

    /// <summary>
    /// Gets the package JSON workspace directory path.
    /// </summary>
    public string PackageDirectoryPath =>
        Path.Combine(
            RootDirectory,
            PackageDirectoryName);

    /// <summary>
    /// Gets the project-local error catalog file path.
    /// </summary>
    public string ErrorCatalogFilePath =>
        Path.Combine(
            PackageDirectoryPath,
            ErrorCatalogFileName);

    /// <summary>
    /// Gets the project-local category catalog file path.
    /// </summary>
    public string CategoryCatalogFilePath =>
        Path.Combine(
            PackageDirectoryPath,
            CategoryCatalogFileName);

    /// <summary>
    /// Gets the project-local code group catalog file path.
    /// </summary>
    public string CodeGroupCatalogFilePath =>
        Path.Combine(
            PackageDirectoryPath,
            CodeGroupCatalogFileName);

    /// <summary>
    /// Gets the project-local owner catalog file path.
    /// </summary>
    public string OwnerCatalogFilePath =>
        Path.Combine(
            PackageDirectoryPath,
            OwnerCatalogFileName);

    /// <summary>
    /// Gets the project-local profiles file path.
    /// </summary>
    public string ProfilesFilePath =>
        Path.Combine(
            PackageDirectoryPath,
            ProfilesFileName);
}