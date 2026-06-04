using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.WhenItFails.Validation;

/// <summary>
/// Default validator for error catalog documents.
/// </summary>
public sealed class ErrorCatalogValidator : IErrorCatalogValidator
{
   private static readonly HashSet<string> AllowedSeverities = new(StringComparer.OrdinalIgnoreCase)
    {
        "Trace",
        "Debug",
        "Information",
        "Warning",
        "Error",
        "Critical"
    };

   /// <inheritdoc />
   public ErrorCatalogValidationResult Validate(ErrorCatalogDocument? document)
   {
      ErrorCatalogValidationResult result = new();

      if(document is null)
      {
         result.AddError(
             code: "CatalogDocumentIsNull",
             message: "Error catalog document is null.",
             path: "$");

         return result;
      }

      ValidateDocumentHeader(document, result);
      ValidateErrors(document, result);

      return result;
   }

   private static void ValidateDocumentHeader(
       ErrorCatalogDocument document,
       ErrorCatalogValidationResult result)
   {
      if(string.IsNullOrWhiteSpace(document.SchemaVersion))
      {
         result.AddError(
             code: "MissingSchemaVersion",
             message: "Catalog schema version is missing.",
             path: "schemaVersion");
      }

      if(string.IsNullOrWhiteSpace(document.CatalogId))
      {
         result.AddError(
             code: "MissingCatalogId",
             message: "Catalog id is missing.",
             path: "catalogId");
      }

      if(string.IsNullOrWhiteSpace(document.CatalogName))
      {
         result.AddWarning(
             code: "MissingCatalogName",
             message: "Catalog name is missing.",
             path: "catalogName");
      }

      if(string.IsNullOrWhiteSpace(document.Language))
      {
         result.AddWarning(
             code: "MissingCatalogLanguage",
             message: "Catalog language is missing. Default language should normally be specified.",
             path: "language");
      }
   }

   private static void ValidateErrors(
       ErrorCatalogDocument document,
       ErrorCatalogValidationResult result)
   {
      if(document.Errors.Count == 0)
      {
         result.AddWarning(
             code: "CatalogContainsNoErrors",
             message: "Catalog does not contain any error definitions.",
             path: "errors");

         return;
      }

      HashSet<string> usedIds = new(StringComparer.OrdinalIgnoreCase);
      HashSet<int> usedCodes = new();
      HashSet<string> usedNames = new(StringComparer.OrdinalIgnoreCase);

      for(int errorIndex = 0; errorIndex < document.Errors.Count; errorIndex++)
      {
         ErrorDefinition error = document.Errors[errorIndex];

         ValidateSingleError(error, errorIndex, result);

         AddDuplicateCheck(
             usedIds,
             error.Id,
             "DuplicateErrorId",
             $"Duplicate error id '{error.Id}'.",
             error,
             $"errors[{errorIndex}].id",
             result);

         if(error.Code > 0 && !usedCodes.Add(error.Code))
         {
            result.AddError(
                code: "DuplicateErrorCode",
                message: $"Duplicate error code '{error.Code}'.",
                errorId: error.Id,
                errorName: error.Name,
                path: $"errors[{errorIndex}].code");
         }

         AddDuplicateCheck(
             usedNames,
             error.Name,
             "DuplicateErrorName",
             $"Duplicate error name '{error.Name}'.",
             error,
             $"errors[{errorIndex}].name",
             result);
      }
   }

   private static void ValidateSingleError(
       ErrorDefinition error,
       int errorIndex,
       ErrorCatalogValidationResult result)
   {
      string errorPath = $"errors[{errorIndex}]";

      if(string.IsNullOrWhiteSpace(error.Id))
      {
         result.AddError(
             code: "MissingErrorId",
             message: "Error id is missing.",
             errorName: error.Name,
             path: $"{errorPath}.id");
      }

      if(error.Code <= 0)
      {
         result.AddError(
             code: "InvalidErrorCode",
             message: "Error code must be greater than zero.",
             errorId: error.Id,
             errorName: error.Name,
             path: $"{errorPath}.code");
      }

      if(string.IsNullOrWhiteSpace(error.Name))
      {
         result.AddError(
             code: "MissingErrorName",
             message: "Error name is missing.",
             errorId: error.Id,
             path: $"{errorPath}.name");
      }

      if(string.IsNullOrWhiteSpace(error.Owner))
      {
         result.AddError(
             code: "MissingErrorOwner",
             message: "Error owner is missing.",
             errorId: error.Id,
             errorName: error.Name,
             path: $"{errorPath}.owner");
      }

      if(string.IsNullOrWhiteSpace(error.CodePrefix))
      {
         result.AddError(
             code: "MissingErrorCodePrefix",
             message: "Error code prefix is missing.",
             errorId: error.Id,
             errorName: error.Name,
             path: $"{errorPath}.codePrefix");
      }

      if(string.IsNullOrWhiteSpace(error.CodeGroup))
      {
         result.AddError(
             code: "MissingErrorCodeGroup",
             message: "Error code group is missing.",
             errorId: error.Id,
             errorName: error.Name,
             path: $"{errorPath}.codeGroup");
      }

      if(string.IsNullOrWhiteSpace(error.PrimaryCategory))
      {
         result.AddError(
             code: "MissingErrorPrimaryCategory",
             message: "Error primary category is missing.",
             errorId: error.Id,
             errorName: error.Name,
             path: $"{errorPath}.primaryCategory");
      }

      ValidateErrorIdStructure(error, errorPath, result);
      ValidateStringCollection(error.Categories, error, $"{errorPath}.categories", "Category", result);
      ValidateStringCollection(error.Subcategories, error, $"{errorPath}.subcategories", "Subcategory", result);
      ValidateStringCollection(error.Tags, error, $"{errorPath}.tags", "Tag", result);

      if(string.IsNullOrWhiteSpace(error.Title))
      {
         result.AddWarning(
             code: "MissingErrorTitle",
             message: "Error title is missing.",
             errorId: error.Id,
             errorName: error.Name,
             path: $"{errorPath}.title");
      }

      if(string.IsNullOrWhiteSpace(error.Message))
      {
         result.AddError(
             code: "MissingErrorMessage",
             message: "Error message is missing.",
             errorId: error.Id,
             errorName: error.Name,
             path: $"{errorPath}.message");
      }

      if(string.IsNullOrWhiteSpace(error.DefaultSeverity))
      {
         result.AddError(
             code: "MissingDefaultSeverity",
             message: "Default severity is missing.",
             errorId: error.Id,
             errorName: error.Name,
             path: $"{errorPath}.defaultSeverity");
      }
      else if(!AllowedSeverities.Contains(error.DefaultSeverity))
      {
         result.AddError(
             code: "UnknownDefaultSeverity",
             message: $"Default severity '{error.DefaultSeverity}' is not supported.",
             errorId: error.Id,
             errorName: error.Name,
             path: $"{errorPath}.defaultSeverity");
      }
   }

   private static void ValidateErrorIdStructure(
       ErrorDefinition error,
       string errorPath,
       ErrorCatalogValidationResult result)
   {
      if(string.IsNullOrWhiteSpace(error.Id)
          || string.IsNullOrWhiteSpace(error.Owner)
          || string.IsNullOrWhiteSpace(error.CodePrefix))
      {
         return;
      }

      string normalizedId = TextKeyNormalizer.NormalizeKey(error.Id);
      string normalizedOwner = TextKeyNormalizer.NormalizeKey(error.Owner);
      string normalizedCodePrefix = TextKeyNormalizer.NormalizeKey(error.CodePrefix);

      string expectedStart = $"{normalizedOwner}_{normalizedCodePrefix}_";

      if(!normalizedId.StartsWith(expectedStart, StringComparison.OrdinalIgnoreCase))
      {
         result.AddError(
             code: "ErrorIdDoesNotMatchOwnerAndCodePrefix",
             message: $"Error id '{error.Id}' does not match owner '{error.Owner}' and code prefix '{error.CodePrefix}'. Expected normalized id to start with '{expectedStart}'.",
             errorId: error.Id,
             errorName: error.Name,
             path: $"{errorPath}.id");
      }
   }

   private static void ValidateStringCollection(
       List<string> values,
       ErrorDefinition error,
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
                errorId: error.Id,
                errorName: error.Name,
                path: $"{path}[{valueIndex}]");

            continue;
         }

         string normalizedValue = TextKeyNormalizer.NormalizeKey(value);

         if(!usedValues.Add(normalizedValue))
         {
            result.AddWarning(
                code: $"Duplicate{valueName}",
                message: $"{valueName} '{value}' is duplicated in error '{error.Id}' using normalized comparison.",
                errorId: error.Id,
                errorName: error.Name,
                path: $"{path}[{valueIndex}]");
         }
      }
   }

   private static void AddDuplicateCheck(
       HashSet<string> usedValues,
       string value,
       string issueCode,
       string issueMessage,
       ErrorDefinition error,
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
             errorId: error.Id,
             errorName: error.Name,
             path: path);
      }
   }
}