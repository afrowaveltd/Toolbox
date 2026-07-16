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

      summary.ProfileNames.AddRange(profileNames);

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

   private static async Task<JsonDocument> LoadJsonDocumentAsync(
      string directoryPath,
      string fileName)
   {
      string filePath = Path.Combine(directoryPath, fileName);

      await using FileStream stream = File.OpenRead(filePath);

      return await JsonDocument.ParseAsync(stream);
   }
}
