using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.WhenItFails.Validation;

/// <summary>
/// Default validator for error category catalog documents.
/// </summary>
public sealed class ErrorCategoryCatalogValidator : IErrorCategoryCatalogValidator
{
   /// <inheritdoc />
   public ErrorCatalogValidationResult Validate(ErrorCategoryCatalogDocument? document)
   {
      ErrorCatalogValidationResult result = new();

      if(document is null)
      {
         result.AddError(
             code: "CategoryCatalogDocumentIsNull",
             message: "Error category catalog document is null.",
             path: "$");

         return result;
      }

      ValidateDocumentHeader(document, result);
      ValidateCategories(document, result);

      return result;
   }

   private static void ValidateDocumentHeader(
       ErrorCategoryCatalogDocument document,
       ErrorCatalogValidationResult result)
   {
      CatalogValidationHelper.ValidateDocumentHeader(
          catalogKind: "Category",
          schemaVersion: document.SchemaVersion,
          catalogId: document.CatalogId,
          catalogName: document.CatalogName,
          language: document.Language,
          result: result);
   }

   private static void ValidateCategories(
       ErrorCategoryCatalogDocument document,
       ErrorCatalogValidationResult result)
   {
      if(document.Categories.Count == 0)
      {
         result.AddWarning(
             code: "CategoryCatalogContainsNoCategories",
             message: "Category catalog does not contain any category definitions.",
             path: "categories");

         return;
      }

      HashSet<string> usedNames = new(StringComparer.OrdinalIgnoreCase);
      HashSet<string> allCategoryNames = CreateNormalizedCategoryNameSet(document.Categories);
      HashSet<string> usedAliases = new(StringComparer.OrdinalIgnoreCase);

      for(int categoryIndex = 0; categoryIndex < document.Categories.Count; categoryIndex++)
      {
         ErrorCategoryDefinition category = document.Categories[categoryIndex];
         string categoryPath = $"categories[{categoryIndex}]";

         ValidateSingleCategory(
             category,
             categoryPath,
             result);

         CatalogValidationHelper.AddDuplicateKeyCheck(
             usedNames,
             category.Name,
             category.Name,
             "DuplicateCategoryName",
             $"Duplicate category name '{category.Name}'.",
             categoryPath + ".name",
             result);

         CatalogValidationHelper.ValidateStringCollection(
             category.Aliases,
             category.Name,
             categoryPath + ".aliases",
             "CategoryAlias",
             result);

         ValidateCategoryAliases(
             category,
             categoryPath,
             allCategoryNames,
             usedAliases,
             result);

         CatalogValidationHelper.ValidateStringCollection(
             category.ParentCategories,
             category.Name,
             categoryPath + ".parentCategories",
             "ParentCategory",
             result);

         CatalogValidationHelper.ValidateStringCollection(
             category.DefaultTags,
             category.Name,
             categoryPath + ".defaultTags",
             "DefaultTag",
             result);
      }
   }

   private static HashSet<string> CreateNormalizedCategoryNameSet(
       IEnumerable<ErrorCategoryDefinition> categories)
   {
      HashSet<string> categoryNames = new(StringComparer.OrdinalIgnoreCase);

      foreach(ErrorCategoryDefinition category in categories)
      {
         string normalizedName = TextKeyNormalizer.NormalizeKey(category.Name);

         if(!string.IsNullOrWhiteSpace(normalizedName))
         {
            categoryNames.Add(normalizedName);
         }
      }

      return categoryNames;
   }

   private static void ValidateCategoryAliases(
       ErrorCategoryDefinition category,
       string categoryPath,
       HashSet<string> allCategoryNames,
       HashSet<string> usedAliases,
       ErrorCatalogValidationResult result)
   {
      foreach(string alias in category.Aliases)
      {
         string normalizedAlias = TextKeyNormalizer.NormalizeKey(alias);

         if(string.IsNullOrWhiteSpace(normalizedAlias))
         {
            continue;
         }

         if(!usedAliases.Add(normalizedAlias))
         {
            result.AddWarning(
                code: "DuplicateCategoryAliasAcrossCatalog",
                message: $"Category alias '{alias}' is used more than once in the category catalog.",
                errorId: category.Name,
                errorName: category.DisplayName,
                path: categoryPath + ".aliases");
         }

         if(allCategoryNames.Contains(normalizedAlias))
         {
            result.AddWarning(
                code: "CategoryAliasMatchesExistingCategoryName",
                message: $"Category alias '{alias}' matches an existing category name.",
                errorId: category.Name,
                errorName: category.DisplayName,
                path: categoryPath + ".aliases");
         }
      }
   }

   private static void ValidateSingleCategory(
       ErrorCategoryDefinition category,
       string categoryPath,
       ErrorCatalogValidationResult result)
   {
      if(string.IsNullOrWhiteSpace(category.Name))
      {
         result.AddError(
             code: "MissingCategoryName",
             message: "Category name is missing.",
             path: categoryPath + ".name");
      }

      if(string.IsNullOrWhiteSpace(category.DisplayName))
      {
         result.AddWarning(
             code: "MissingCategoryDisplayName",
             message: "Category display name is missing.",
             errorId: category.Name,
             path: categoryPath + ".displayName");
      }
   }
}
