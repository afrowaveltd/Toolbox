using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.WhenItFails.Validation;

/// <summary>
/// Provides shared helper methods for catalog validators.
/// </summary>
internal static class CatalogValidationHelper
{
   public static void ValidateDocumentHeader(
       string catalogKind,
       string? schemaVersion,
       string? catalogId,
       string? catalogName,
       string? language,
       ErrorCatalogValidationResult result)
   {
      if(string.IsNullOrWhiteSpace(schemaVersion))
      {
         result.AddError(
             code: "MissingSchemaVersion",
             message: $"{catalogKind} catalog schema version is missing.",
             path: "schemaVersion");
      }

      if(string.IsNullOrWhiteSpace(catalogId))
      {
         result.AddError(
             code: "MissingCatalogId",
             message: $"{catalogKind} catalog id is missing.",
             path: "catalogId");
      }

      if(string.IsNullOrWhiteSpace(catalogName))
      {
         result.AddWarning(
             code: "MissingCatalogName",
             message: $"{catalogKind} catalog name is missing.",
             path: "catalogName");
      }

      if(string.IsNullOrWhiteSpace(language))
      {
         result.AddWarning(
             code: "MissingCatalogLanguage",
             message: $"{catalogKind} catalog language is missing. Default language should normally be specified.",
             path: "language");
      }
   }

   public static void ValidateStringCollection(
       IReadOnlyList<string> values,
       string ownerId,
       string path,
       string valueName,
       ErrorCatalogValidationResult result)
   {
      HashSet<string> usedValues = new(StringComparer.OrdinalIgnoreCase);

      for(int valueIndex = 0; valueIndex < values.Count; valueIndex++)
      {
         string value = values[valueIndex];

         if(string.IsNullOrWhiteSpace(value))
         {
            result.AddWarning(
                code: $"Empty{valueName}",
                message: $"{valueName} value is empty.",
                errorId: ownerId,
                path: $"{path}[{valueIndex}]");

            continue;
         }

         string normalizedValue = TextKeyNormalizer.NormalizeKey(value);

         if(!usedValues.Add(normalizedValue))
         {
            result.AddWarning(
                code: $"Duplicate{valueName}",
                message: $"{valueName} '{value}' is duplicated using normalized comparison.",
                errorId: ownerId,
                path: $"{path}[{valueIndex}]");
         }
      }
   }

   public static void AddDuplicateKeyCheck(
       HashSet<string> usedValues,
       string value,
       string ownerId,
       string issueCode,
       string issueMessage,
       string path,
       ErrorCatalogValidationResult result)
   {
      if(string.IsNullOrWhiteSpace(value))
      {
         return;
      }

      string normalizedValue = TextKeyNormalizer.NormalizeKey(value);

      if(!usedValues.Add(normalizedValue))
      {
         result.AddError(
             code: issueCode,
             message: issueMessage,
             errorId: ownerId,
             path: path);
      }
   }
}
