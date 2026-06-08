using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Validation;

/// <summary>
/// Default validator for error code group catalog documents.
/// </summary>
public sealed class ErrorCodeGroupCatalogValidator : IErrorCodeGroupCatalogValidator
{
   /// <inheritdoc />
   public ErrorCatalogValidationResult Validate(ErrorCodeGroupCatalogDocument? document)
   {
      ErrorCatalogValidationResult result = new();

      if(document is null)
      {
         result.AddError(
             code: "CodeGroupCatalogDocumentIsNull",
             message: "Error code group catalog document is null.",
             path: "$");

         return result;
      }

      ValidateDocumentHeader(document, result);
      ValidateCodeGroups(document, result);

      return result;
   }

   private static void ValidateDocumentHeader(
       ErrorCodeGroupCatalogDocument document,
       ErrorCatalogValidationResult result)
   {
      CatalogValidationHelper.ValidateDocumentHeader(
          catalogKind: "Code group",
          schemaVersion: document.SchemaVersion,
          catalogId: document.CatalogId,
          catalogName: document.CatalogName,
          language: document.Language,
          result: result);
   }

   private static void ValidateCodeGroups(
       ErrorCodeGroupCatalogDocument document,
       ErrorCatalogValidationResult result)
   {
      if(document.CodeGroups.Count == 0)
      {
         result.AddWarning(
             code: "CodeGroupCatalogContainsNoCodeGroups",
             message: "Code group catalog does not contain any code group definitions.",
             path: "codeGroups");

         return;
      }

      HashSet<string> usedNames = new(StringComparer.OrdinalIgnoreCase);
      HashSet<string> usedCodePrefixes = new(StringComparer.OrdinalIgnoreCase);

      for(int codeGroupIndex = 0; codeGroupIndex < document.CodeGroups.Count; codeGroupIndex++)
      {
         ErrorCodeGroupDefinition codeGroup = document.CodeGroups[codeGroupIndex];
         string codeGroupPath = $"codeGroups[{codeGroupIndex}]";

         ValidateSingleCodeGroup(codeGroup, codeGroupPath, result);

         CatalogValidationHelper.AddDuplicateKeyCheck(
             usedNames,
             codeGroup.Name,
             codeGroup.Name,
             "DuplicateCodeGroupName",
             $"Duplicate code group name '{codeGroup.Name}'.",
             codeGroupPath + ".name",
             result);

         CatalogValidationHelper.AddDuplicateKeyCheck(
             usedCodePrefixes,
             codeGroup.CodePrefix,
             codeGroup.Name,
             "DuplicateCodeGroupPrefix",
             $"Duplicate code group prefix '{codeGroup.CodePrefix}'.",
             codeGroupPath + ".codePrefix",
             result);

         CatalogValidationHelper.ValidateStringCollection(
             codeGroup.DefaultCategories,
             codeGroup.Name,
             codeGroupPath + ".defaultCategories",
             "DefaultCategory",
             result);

         CatalogValidationHelper.ValidateStringCollection(
             codeGroup.DefaultTags,
             codeGroup.Name,
             codeGroupPath + ".defaultTags",
             "DefaultTag",
             result);
      }

      ValidateRangeOverlaps(document, result);
   }

   private static void ValidateSingleCodeGroup(
       ErrorCodeGroupDefinition codeGroup,
       string codeGroupPath,
       ErrorCatalogValidationResult result)
   {
      if(string.IsNullOrWhiteSpace(codeGroup.Name))
      {
         result.AddError(
             code: "MissingCodeGroupName",
             message: "Code group name is missing.",
             path: codeGroupPath + ".name");
      }

      if(string.IsNullOrWhiteSpace(codeGroup.DisplayName))
      {
         result.AddWarning(
             code: "MissingCodeGroupDisplayName",
             message: "Code group display name is missing.",
             errorId: codeGroup.Name,
             path: codeGroupPath + ".displayName");
      }

      if(string.IsNullOrWhiteSpace(codeGroup.CodePrefix))
      {
         result.AddError(
             code: "MissingCodeGroupPrefix",
             message: "Code group prefix is missing.",
             errorId: codeGroup.Name,
             path: codeGroupPath + ".codePrefix");
      }

      if(codeGroup.CodeFrom <= 0)
      {
         result.AddError(
             code: "InvalidCodeGroupCodeFrom",
             message: "Code group codeFrom must be greater than zero.",
             errorId: codeGroup.Name,
             path: codeGroupPath + ".codeFrom");
      }

      if(codeGroup.CodeTo <= 0)
      {
         result.AddError(
             code: "InvalidCodeGroupCodeTo",
             message: "Code group codeTo must be greater than zero.",
             errorId: codeGroup.Name,
             path: codeGroupPath + ".codeTo");
      }

      if(codeGroup.CodeFrom > 0
          && codeGroup.CodeTo > 0
          && codeGroup.CodeFrom > codeGroup.CodeTo)
      {
         result.AddError(
             code: "InvalidCodeGroupRange",
             message: $"Code group range is invalid. codeFrom '{codeGroup.CodeFrom}' is greater than codeTo '{codeGroup.CodeTo}'.",
             errorId: codeGroup.Name,
             path: codeGroupPath);
      }
   }

   private static void ValidateRangeOverlaps(
       ErrorCodeGroupCatalogDocument document,
       ErrorCatalogValidationResult result)
   {
      for(int firstIndex = 0; firstIndex < document.CodeGroups.Count; firstIndex++)
      {
         ErrorCodeGroupDefinition first = document.CodeGroups[firstIndex];

         if(!IsValidRange(first))
         {
            continue;
         }

         for(int secondIndex = firstIndex + 1; secondIndex < document.CodeGroups.Count; secondIndex++)
         {
            ErrorCodeGroupDefinition second = document.CodeGroups[secondIndex];

            if(!IsValidRange(second))
            {
               continue;
            }

            bool overlaps =
                first.CodeFrom <= second.CodeTo
                && second.CodeFrom <= first.CodeTo;

            if(overlaps)
            {
               result.AddError(
                   code: "CodeGroupRangeOverlap",
                   message: $"Code group '{first.Name}' range {first.CodeFrom}-{first.CodeTo} overlaps with code group '{second.Name}' range {second.CodeFrom}-{second.CodeTo}.",
                   errorId: first.Name,
                   errorName: second.Name,
                   path: $"codeGroups[{firstIndex}]");
            }
         }
      }
   }

   private static bool IsValidRange(ErrorCodeGroupDefinition codeGroup)
   {
      return codeGroup.CodeFrom > 0
          && codeGroup.CodeTo > 0
          && codeGroup.CodeFrom <= codeGroup.CodeTo;
   }
}
