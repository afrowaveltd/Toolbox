using Afrowave.Toolbox.WhenItFails.Configuration;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter;

/// <summary>
/// Resolves project root paths and direct Jsons/WhenItFails paths for Setter commands.
/// </summary>
internal static class WhenItFailsWorkspacePathResolver
{
    private const string JsonsDirectoryName = "Jsons";
    private const string PackageDirectoryName = "WhenItFails";

    private static readonly string[] KnownCatalogFileNames =
    [
       "errors.en.json",
      "categories.en.json",
      "code-groups.en.json",
      "owners.en.json",
      "profiles.json"
    ];

    /// <summary>
    /// Resolves JSON options from either a project root path or a direct Jsons/WhenItFails path.
    /// </summary>
    /// <param name="inputPath">Project root or Jsons/WhenItFails directory.</param>
    /// <returns>Resolved JSON options.</returns>
    public static JsonsOptions ResolveJsonsOptions(string inputPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);

        string fullInputPath = Path.GetFullPath(inputPath);
        DirectoryInfo inputDirectory = new(fullInputPath);

        if (LooksLikePackageDirectory(inputDirectory))
        {
            return CreateOptionsFromPackageDirectory(inputDirectory);
        }

        if (LooksLikePackageDirectoryByName(inputDirectory))
        {
            return CreateOptionsFromPackageDirectory(inputDirectory);
        }

        return new JsonsOptions
        {
            RootDirectory = Path.Combine(fullInputPath, JsonsDirectoryName),
            PackageDirectoryName = PackageDirectoryName
        };
    }

    /// <summary>
    /// Creates a display path for console output.
    /// </summary>
    /// <param name="inputPath">Original input path.</param>
    /// <param name="packageDirectoryPath">Resolved package directory path.</param>
    /// <returns>Display path.</returns>
    public static string CreateDisplayPath(
       string inputPath,
       string packageDirectoryPath)
    {
        string fullInputPath = Path.GetFullPath(inputPath);
        DirectoryInfo inputDirectory = new(fullInputPath);

        if (LooksLikePackageDirectory(inputDirectory)
            || LooksLikePackageDirectoryByName(inputDirectory))
        {
            return inputDirectory.Name;
        }

        return Path.GetRelativePath(
           fullInputPath,
           packageDirectoryPath);
    }

    private static JsonsOptions CreateOptionsFromPackageDirectory(
       DirectoryInfo packageDirectory)
    {
        return new JsonsOptions
        {
            RootDirectory = packageDirectory.Parent?.FullName ?? packageDirectory.FullName,
            PackageDirectoryName = packageDirectory.Name
        };
    }

    private static bool LooksLikePackageDirectory(DirectoryInfo directory)
    {
        if (!directory.Exists)
        {
            return false;
        }

        foreach (string knownCatalogFileName in KnownCatalogFileNames)
        {
            string catalogFilePath = Path.Combine(
               directory.FullName,
               knownCatalogFileName);

            if (File.Exists(catalogFilePath))
            {
                return true;
            }
        }

        return false;
    }

    private static bool LooksLikePackageDirectoryByName(DirectoryInfo directory)
    {
        if (!string.Equals(
               directory.Name,
               PackageDirectoryName,
               StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        DirectoryInfo? parentDirectory = directory.Parent;

        if (parentDirectory is null)
        {
            return false;
        }

        return string.Equals(
           parentDirectory.Name,
           JsonsDirectoryName,
           StringComparison.OrdinalIgnoreCase);
    }
}