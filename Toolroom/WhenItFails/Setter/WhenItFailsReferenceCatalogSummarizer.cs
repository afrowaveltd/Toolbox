using System.Text.Json;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter;

/// <summary>
/// Reads the bundled WhenItFails reference catalog and creates a compact summary.
/// </summary>
internal sealed class WhenItFailsReferenceCatalogSummarizer
{
   private const string ReferenceCatalogRelativePath = "WhenItFails/ReferenceCatalog/Core";

   /// <summary>
   /// Creates a summary of the bundled reference catalog.
   /// </summary>
   /// <param name="startDirectoryPath">Directory where the lookup should start.</param>
   /// <returns>Reference catalog summary.</returns>
   public async Task<WhenItFailsReferenceCatalogSummary> SummarizeAsync(string startDirectoryPath)
   {
      ArgumentException.ThrowIfNullOrWhiteSpace(startDirectoryPath);

      string referenceCatalogDirectoryPath = FindReferenceCatalogDirectory(startDirectoryPath);

      WhenItFailsReferenceCatalogSummary summary = new()
      {
         DirectoryPath = referenceCatalogDirectoryPath,
         DisplayPath = CreateDisplayPath(startDirectoryPath, referenceCatalogDirectoryPath),
         OwnerCount = await CountArrayItemsAsync(referenceCatalogDirectoryPath, "owners.en.json", "owners"),
         CategoryCount = await CountArrayItemsAsync(referenceCatalogDirectoryPath, "categories.en.json", "categories"),
         CodeGroupCount = await CountArrayItemsAsync(referenceCatalogDirectoryPath, "code-groups.en.json", "codeGroups"),
         ProfileCount = await CountArrayItemsAsync(referenceCatalogDirectoryPath, "profiles.json", "profiles"),
         ErrorCount = await CountArrayItemsAsync(referenceCatalogDirectoryPath, "errors.en.json", "errors")
      };

      IReadOnlyList<string> profileNames = await ReadProfileNamesAsync(referenceCatalogDirectoryPath);
      IReadOnlyList<WhenItFailsReferenceCategorySummary> categories =
         await ReadCategoriesAsync(referenceCatalogDirectoryPath);

      summary.ProfileNames.AddRange(profileNames);
      summary.Categories.AddRange(categories);

      return summary;
   }

   private static string FindReferenceCatalogDirectory(string startDirectoryPath)
   {
      DirectoryInfo? directory = new(Path.GetFullPath(startDirectoryPath));

      while (directory is not null)
      {
         string candidatePath = Path.Combine(
            directory.FullName,
            ReferenceCatalogRelativePath);

         if (Directory.Exists(candidatePath)
             && File.Exists(Path.Combine(candidatePath, "errors.en.json"))
             && File.Exists(Path.Combine(candidatePath, "profiles.json")))
         {
            return candidatePath;
         }

         directory = directory.Parent;
      }

      throw new DirectoryNotFoundException(
         $"WhenItFails reference catalog was not found. Expected relative path: {ReferenceCatalogRelativePath}");
   }

   private static string CreateDisplayPath(
      string startDirectoryPath,
      string referenceCatalogDirectoryPath)
   {
      string fullStartDirectoryPath = Path.GetFullPath(startDirectoryPath);
      string fullReferenceCatalogDirectoryPath = Path.GetFullPath(referenceCatalogDirectoryPath);

      string relativePath = Path.GetRelativePath(
         fullStartDirectoryPath,
         fullReferenceCatalogDirectoryPath);

      return relativePath == "."
         ? Path.GetFileName(fullReferenceCatalogDirectoryPath)
         : relativePath;
   }

   private static async Task<int> CountArrayItemsAsync(
      string directoryPath,
      string fileName,
      string propertyName)
   {
      using JsonDocument document = await LoadJsonDocumentAsync(directoryPath, fileName);

      JsonElement rootElement = document.RootElement;

      if (!rootElement.TryGetProperty(propertyName, out JsonElement arrayElement)
          || arrayElement.ValueKind != JsonValueKind.Array)
      {
         return 0;
      }

      return arrayElement.GetArrayLength();
   }

   private static async Task<IReadOnlyList<string>> ReadProfileNamesAsync(string directoryPath)
   {
      using JsonDocument document = await LoadJsonDocumentAsync(directoryPath, "profiles.json");

      JsonElement rootElement = document.RootElement;

      if (!rootElement.TryGetProperty("profiles", out JsonElement profilesElement)
          || profilesElement.ValueKind != JsonValueKind.Array)
      {
         return Array.Empty<string>();
      }

      List<string> profileNames = new();

      foreach (JsonElement profileElement in profilesElement.EnumerateArray())
      {
         if (!profileElement.TryGetProperty("name", out JsonElement nameElement)
             || nameElement.ValueKind != JsonValueKind.String)
         {
            continue;
         }

         string? name = nameElement.GetString();

         if (!string.IsNullOrWhiteSpace(name))
         {
            profileNames.Add(name);
         }
      }

      return profileNames;
   }

   private static async Task<IReadOnlyList<WhenItFailsReferenceCategorySummary>> ReadCategoriesAsync(
      string directoryPath)
   {
      using JsonDocument document = await LoadJsonDocumentAsync(directoryPath, "categories.en.json");

      JsonElement rootElement = document.RootElement;

      if (!rootElement.TryGetProperty("categories", out JsonElement categoriesElement)
          || categoriesElement.ValueKind != JsonValueKind.Array)
      {
         return Array.Empty<WhenItFailsReferenceCategorySummary>();
      }

      List<WhenItFailsReferenceCategorySummary> categories = new();

      foreach (JsonElement categoryElement in categoriesElement.EnumerateArray())
      {
         string name = ReadStringProperty(categoryElement, "name");
         string displayName = ReadStringProperty(categoryElement, "displayName");

         if (string.IsNullOrWhiteSpace(name))
         {
            continue;
         }

         WhenItFailsReferenceCategorySummary category = new()
         {
            Name = name,
            DisplayName = string.IsNullOrWhiteSpace(displayName)
               ? name
               : displayName
         };

         if (categoryElement.TryGetProperty("parentCategories", out JsonElement parentCategoriesElement)
             && parentCategoriesElement.ValueKind == JsonValueKind.Array)
         {
            foreach (JsonElement parentCategoryElement in parentCategoriesElement.EnumerateArray())
            {
               if (parentCategoryElement.ValueKind != JsonValueKind.String)
               {
                  continue;
               }

               string? parentCategoryName = parentCategoryElement.GetString();

               if (!string.IsNullOrWhiteSpace(parentCategoryName))
               {
                  category.ParentCategoryNames.Add(parentCategoryName);
               }
            }
         }

         categories.Add(category);
      }

      return categories;
   }

   private static string ReadStringProperty(
      JsonElement element,
      string propertyName)
   {
      if (!element.TryGetProperty(propertyName, out JsonElement propertyElement)
          || propertyElement.ValueKind != JsonValueKind.String)
      {
         return string.Empty;
      }

      return propertyElement.GetString() ?? string.Empty;
   }

   private static async Task<JsonDocument> LoadJsonDocumentAsync(
      string directoryPath,
      string fileName)
   {
      string filePath = Path.Combine(directoryPath, fileName);

      await using FileStream stream = File.OpenRead(filePath);

      return await JsonDocument.ParseAsync(stream);
   }
}
