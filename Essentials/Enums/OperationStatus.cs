namespace Afrowave.Toolbox.Essentials.Enums;

/// <summary>
/// Describes the current or final status of an operation.
/// </summary>
public enum OperationStatus
{
   /// <summary>
   /// The operation status is unknown or was not specified.
   /// </summary>
   Unknown = 0,

   /// <summary>
   /// The operation was created but has not started yet.
   /// </summary>
   Pending = 1,

   /// <summary>
   /// The operation is currently running.
   /// </summary>
   Running = 2,

   /// <summary>
   /// The operation completed successfully.
   /// </summary>
   Completed = 3,

   /// <summary>
   /// The operation completed, but produced warnings or partial issues.
   /// </summary>
   CompletedWithWarnings = 4,

   /// <summary>
   /// The operation completed only partially.
   /// </summary>
   PartiallyCompleted = 5,

   /// <summary>
   /// The operation failed.
   /// </summary>
   Failed = 6,

   /// <summary>
   /// The operation was cancelled before completion.
   /// </summary>
   Cancelled = 7,

   /// <summary>
   /// The operation was skipped intentionally.
   /// </summary>
   Skipped = 8,

   /// <summary>
   /// The operation is not supported in the current context.
   /// </summary>
   NotSupported = 9
}
