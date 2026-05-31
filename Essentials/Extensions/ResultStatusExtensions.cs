using Afrowave.Toolbox.Essentials.Enums;

namespace Afrowave.Toolbox.Essentials.Extensions;

/// <summary>
/// Provides helper methods for working with <see cref="ResultStatus"/> values.
/// </summary>
public static class ResultStatusExtensions
{
   /// <summary>
   /// Determines whether the result status represents a successful result.
   /// </summary>
   /// <param name="status">The result status.</param>
   /// <returns><c>true</c> if the status represents success; otherwise, <c>false</c>.</returns>
   public static bool IsSuccess(this ResultStatus status)
   {
      return status is ResultStatus.Success
          or ResultStatus.SuccessWithWarnings;
   }

   /// <summary>
   /// Determines whether the result status represents a successful result with warnings.
   /// </summary>
   /// <param name="status">The result status.</param>
   /// <returns><c>true</c> if the status is <see cref="ResultStatus.SuccessWithWarnings"/>; otherwise, <c>false</c>.</returns>
   public static bool HasWarnings(this ResultStatus status)
   {
      return status == ResultStatus.SuccessWithWarnings;
   }

   /// <summary>
   /// Determines whether the result status represents a partial result.
   /// </summary>
   /// <param name="status">The result status.</param>
   /// <returns><c>true</c> if the status is <see cref="ResultStatus.Partial"/>; otherwise, <c>false</c>.</returns>
   public static bool IsPartial(this ResultStatus status)
   {
      return status == ResultStatus.Partial;
   }

   /// <summary>
   /// Determines whether the result status represents a failed or invalid result.
   /// </summary>
   /// <param name="status">The result status.</param>
   /// <returns><c>true</c> if the status represents failure; otherwise, <c>false</c>.</returns>
   public static bool IsFailure(this ResultStatus status)
   {
      return status is ResultStatus.Failed
          or ResultStatus.Invalid
          or ResultStatus.NotSupported
          or ResultStatus.Cancelled;
   }

   /// <summary>
   /// Determines whether the result status represents a missing result.
   /// </summary>
   /// <param name="status">The result status.</param>
   /// <returns><c>true</c> if the status is <see cref="ResultStatus.NotFound"/>; otherwise, <c>false</c>.</returns>
   public static bool IsNotFound(this ResultStatus status)
   {
      return status == ResultStatus.NotFound;
   }

   /// <summary>
   /// Determines whether the result status represents a final state.
   /// </summary>
   /// <param name="status">The result status.</param>
   /// <returns><c>true</c> if the status represents a final state; otherwise, <c>false</c>.</returns>
   public static bool IsFinal(this ResultStatus status)
   {
      return !status.IsUnknown();
   }
   /// <summary>
   /// Determines whether the result status is unknown.
   /// </summary>
   /// <param name="status">The result status.</param>
   /// <returns><c>true</c> if the status is <see cref="ResultStatus.Unknown"/>; otherwise, <c>false</c>.</returns>
   public static bool IsUnknown(this ResultStatus status)
   {
      return status == ResultStatus.Unknown;
   }

   /// <summary>
   /// Determines whether the result status does not represent a successful result.
   /// </summary>
   /// <param name="status">The result status.</param>
   /// <returns><c>true</c> if the status is not success or success with warnings; otherwise, <c>false</c>.</returns>
   public static bool IsNonSuccess(this ResultStatus status)
   {
      return !status.IsSuccess();
   }

   /// <summary>
   /// Determines whether the result status represents an invalid result.
   /// </summary>
   /// <param name="status">The result status.</param>
   /// <returns><c>true</c> if the status is <see cref="ResultStatus.Invalid"/>; otherwise, <c>false</c>.</returns>
   public static bool IsInvalid(this ResultStatus status)
   {
      return status == ResultStatus.Invalid;
   }

   /// <summary>
   /// Determines whether the result status represents an unsupported operation.
   /// </summary>
   /// <param name="status">The result status.</param>
   /// <returns><c>true</c> if the status is <see cref="ResultStatus.NotSupported"/>; otherwise, <c>false</c>.</returns>
   public static bool IsNotSupported(this ResultStatus status)
   {
      return status == ResultStatus.NotSupported;
   }

   /// <summary>
   /// Determines whether the result status represents a cancelled operation.
   /// </summary>
   /// <param name="status">The result status.</param>
   /// <returns><c>true</c> if the status is <see cref="ResultStatus.Cancelled"/>; otherwise, <c>false</c>.</returns>
   public static bool IsCancelled(this ResultStatus status)
   {
      return status == ResultStatus.Cancelled;
   }

   /// <summary>
   /// Determines whether the result status represents a failed result.
   /// </summary>
   /// <param name="status">The result status.</param>
   /// <returns><c>true</c> if the status is <see cref="ResultStatus.Failed"/>; otherwise, <c>false</c>.</returns>
   public static bool IsFailed(this ResultStatus status)
   {
      return status == ResultStatus.Failed;
   }
}