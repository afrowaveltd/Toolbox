using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Catalog;

/// <summary>
/// Represents the result of loading, validating and creating a runtime error catalog.
/// </summary>
public sealed class ErrorCatalogProviderResult
{
   /// <summary>
   /// Gets or sets a value indicating whether the catalog was successfully created.
   /// </summary>
   public bool Success { get; set; }

   /// <summary>
   /// Gets or sets the created runtime error catalog.
   /// </summary>
   public IErrorCatalog? Catalog { get; set; }

   /// <summary>
   /// Gets or sets the original loading result.
   /// </summary>
   public ErrorCatalogLoadResult? LoadResult { get; set; }

   /// <summary>
   /// Gets or sets the validation result.
   /// </summary>
   public ErrorCatalogValidationResult? ValidationResult { get; set; }

   /// <summary>
   /// Gets or sets a machine-friendly error code when the provider operation failed.
   /// </summary>
   public string? ErrorCode { get; set; }

   /// <summary>
   /// Gets or sets a human-readable error message when the provider operation failed.
   /// </summary>
   public string? ErrorMessage { get; set; }

   /// <summary>
   /// Creates a successful provider result.
   /// </summary>
   public static ErrorCatalogProviderResult Ok(
       IErrorCatalog catalog,
       ErrorCatalogLoadResult loadResult,
       ErrorCatalogValidationResult validationResult)
   {
      ArgumentNullException.ThrowIfNull(catalog);
      ArgumentNullException.ThrowIfNull(loadResult);
      ArgumentNullException.ThrowIfNull(validationResult);

      return new ErrorCatalogProviderResult
      {
         Success = true,
         Catalog = catalog,
         LoadResult = loadResult,
         ValidationResult = validationResult
      };
   }

   /// <summary>
   /// Creates a failed provider result.
   /// </summary>
   public static ErrorCatalogProviderResult Fail(
       string errorCode,
       string errorMessage,
       ErrorCatalogLoadResult? loadResult = null,
       ErrorCatalogValidationResult? validationResult = null)
   {
      return new ErrorCatalogProviderResult
      {
         Success = false,
         ErrorCode = errorCode,
         ErrorMessage = errorMessage,
         LoadResult = loadResult,
         ValidationResult = validationResult
      };
   }
}