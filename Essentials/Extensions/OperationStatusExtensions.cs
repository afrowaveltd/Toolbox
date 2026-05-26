using Afrowave.Toolbox.Essentials.Enums;

namespace Afrowave.Toolbox.Essentials.Extensions;

/// <summary>
/// Provides helper methods for working with <see cref="OperationStatus"/> values.
/// </summary>
public static class OperationStatusExtensions
{
   /// <summary>
   /// Determines whether the operation status represents a state before execution.
   /// </summary>
   /// <param name="status">The operation status.</param>
   /// <returns><c>true</c> if the status is <see cref="OperationStatus.Pending"/>; otherwise, <c>false</c>.</returns>
   public static bool IsPending(this OperationStatus status)
   {
      return status == OperationStatus.Pending;
   }

   /// <summary>
   /// Determines whether the operation status represents an active running state.
   /// </summary>
   /// <param name="status">The operation status.</param>
   /// <returns><c>true</c> if the status is <see cref="OperationStatus.Running"/>; otherwise, <c>false</c>.</returns>
   public static bool IsRunning(this OperationStatus status)
   {
      return status == OperationStatus.Running;
   }

   /// <summary>
   /// Determines whether the operation status represents a completed state.
   /// </summary>
   /// <param name="status">The operation status.</param>
   /// <returns><c>true</c> if the status represents completed, completed with warnings, or partially completed; otherwise, <c>false</c>.</returns>
   public static bool IsCompleted(this OperationStatus status)
   {
      return status is OperationStatus.Completed
          or OperationStatus.CompletedWithWarnings
          or OperationStatus.PartiallyCompleted;
   }

   /// <summary>
   /// Determines whether the operation status represents successful completion without warnings.
   /// </summary>
   /// <param name="status">The operation status.</param>
   /// <returns><c>true</c> if the status is <see cref="OperationStatus.Completed"/>; otherwise, <c>false</c>.</returns>
   public static bool IsSuccessfullyCompleted(this OperationStatus status)
   {
      return status == OperationStatus.Completed;
   }

   /// <summary>
   /// Determines whether the operation status represents completion with warnings.
   /// </summary>
   /// <param name="status">The operation status.</param>
   /// <returns><c>true</c> if the status is <see cref="OperationStatus.CompletedWithWarnings"/>; otherwise, <c>false</c>.</returns>
   public static bool HasWarnings(this OperationStatus status)
   {
      return status == OperationStatus.CompletedWithWarnings;
   }

   /// <summary>
   /// Determines whether the operation status represents partial completion.
   /// </summary>
   /// <param name="status">The operation status.</param>
   /// <returns><c>true</c> if the status is <see cref="OperationStatus.PartiallyCompleted"/>; otherwise, <c>false</c>.</returns>
   public static bool IsPartiallyCompleted(this OperationStatus status)
   {
      return status == OperationStatus.PartiallyCompleted;
   }

   /// <summary>
   /// Determines whether the operation status represents failure.
   /// </summary>
   /// <param name="status">The operation status.</param>
   /// <returns><c>true</c> if the status is <see cref="OperationStatus.Failed"/>; otherwise, <c>false</c>.</returns>
   public static bool IsFailed(this OperationStatus status)
   {
      return status == OperationStatus.Failed;
   }

   /// <summary>
   /// Determines whether the operation status represents cancellation.
   /// </summary>
   /// <param name="status">The operation status.</param>
   /// <returns><c>true</c> if the status is <see cref="OperationStatus.Cancelled"/>; otherwise, <c>false</c>.</returns>
   public static bool IsCancelled(this OperationStatus status)
   {
      return status == OperationStatus.Cancelled;
   }

   /// <summary>
   /// Determines whether the operation status represents a skipped operation.
   /// </summary>
   /// <param name="status">The operation status.</param>
   /// <returns><c>true</c> if the status is <see cref="OperationStatus.Skipped"/>; otherwise, <c>false</c>.</returns>
   public static bool IsSkipped(this OperationStatus status)
   {
      return status == OperationStatus.Skipped;
   }

   /// <summary>
   /// Determines whether the operation status represents a final state.
   /// </summary>
   /// <param name="status">The operation status.</param>
   /// <returns><c>true</c> if the status represents a final state; otherwise, <c>false</c>.</returns>
   public static bool IsFinal(this OperationStatus status)
   {
      return status is OperationStatus.Completed
          or OperationStatus.CompletedWithWarnings
          or OperationStatus.PartiallyCompleted
          or OperationStatus.Failed
          or OperationStatus.Cancelled
          or OperationStatus.Skipped
          or OperationStatus.NotSupported;
   }
}