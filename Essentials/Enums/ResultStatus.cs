namespace Afrowave.Toolbox.Essentials.Enums;

/// <summary>
/// Describes the status of a produced result.
/// </summary>
public enum ResultStatus
{
   /// <summary>
   /// The result status is unknown or was not specified.
   /// </summary>
   Unknown = 0,

   /// <summary>
   /// The result was produced successfully.
   /// </summary>
   Success = 1,

   /// <summary>
   /// The result was produced successfully, but contains warnings.
   /// </summary>
   SuccessWithWarnings = 2,

   /// <summary>
   /// The result was produced only partially.
   /// </summary>
   Partial = 3,

   /// <summary>
   /// No result was found or produced.
   /// </summary>
   NotFound = 4,

   /// <summary>
   /// The input or request was invalid.
   /// </summary>
   Invalid = 5,

   /// <summary>
   /// The result could not be produced because the requested operation is not supported.
   /// </summary>
   NotSupported = 6,

   /// <summary>
   /// The result could not be produced because the operation was cancelled.
   /// </summary>
   Cancelled = 7,

   /// <summary>
   /// The result could not be produced because the operation failed.
   /// </summary>
   Failed = 8
}