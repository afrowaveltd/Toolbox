using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Loading;

/// <summary>
/// Represents the result of loading an error catalog document.
/// </summary>
public sealed class ErrorCatalogLoadResult
{
   /// <summary>
   /// Gets or sets a value indicating whether the catalog document was loaded successfully.
   /// </summary>
   public bool Success { get; set; }

   /// <summary>
   /// Gets or sets the loaded error catalog document.
   /// </summary>
   public ErrorCatalogDocument? Document { get; set; }

   /// <summary>
   /// Gets or sets a machine-friendly error code describing why loading failed.
   /// </summary>
   /// <example>FileNotFound</example>
   public string? ErrorCode { get; set; }

   /// <summary>
   /// Gets or sets a human-readable loading error message.
   /// </summary>
   public string? ErrorMessage { get; set; }

   /// <summary>
   /// Gets or sets the original exception when loading failed because of an exception.
   /// </summary>
   public Exception? Exception { get; set; }

   /// <summary>
   /// Creates a successful load result.
   /// </summary>
   /// <param name="document">Loaded catalog document.</param>
   /// <returns>Successful load result.</returns>
   public static ErrorCatalogLoadResult Ok(ErrorCatalogDocument document)
   {
      ArgumentNullException.ThrowIfNull(document);

      return new ErrorCatalogLoadResult
      {
         Success = true,
         Document = document
      };
   }

   /// <summary>
   /// Creates a failed load result.
   /// </summary>
   /// <param name="errorCode">Machine-friendly loading error code.</param>
   /// <param name="errorMessage">Human-readable loading error message.</param>
   /// <param name="exception">Optional original exception.</param>
   /// <returns>Failed load result.</returns>
   public static ErrorCatalogLoadResult Fail(
       string errorCode,
       string errorMessage,
       Exception? exception = null)
   {
      return new ErrorCatalogLoadResult
      {
         Success = false,
         ErrorCode = errorCode,
         ErrorMessage = errorMessage,
         Exception = exception
      };
   }
}