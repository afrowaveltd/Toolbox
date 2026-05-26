using Afrowave.Toolbox.Essentials.Enums;

namespace Afrowave.Toolbox.Essentials.Extensions;

/// <summary>
/// Provides helper methods for working with <see cref="OperationRiskLevel"/> values.
/// </summary>
public static class OperationRiskLevelExtensions
{
   /// <summary>
   /// Determines whether the risk level represents a read-only operation.
   /// </summary>
   /// <param name="riskLevel">The operation risk level.</param>
   /// <returns><c>true</c> if the risk level is <see cref="OperationRiskLevel.ReadOnly"/>; otherwise, <c>false</c>.</returns>
   public static bool IsReadOnly(this OperationRiskLevel riskLevel)
   {
      return riskLevel == OperationRiskLevel.ReadOnly;
   }

   /// <summary>
   /// Determines whether the risk level represents a medium or higher risk.
   /// </summary>
   /// <param name="riskLevel">The operation risk level.</param>
   /// <returns><c>true</c> if the risk level is <see cref="OperationRiskLevel.Medium"/> or higher; otherwise, <c>false</c>.</returns>
   public static bool IsMediumOrHigher(this OperationRiskLevel riskLevel)
   {
      return riskLevel >= OperationRiskLevel.Medium;
   }

   /// <summary>
   /// Determines whether the risk level represents a high or higher risk.
   /// </summary>
   /// <param name="riskLevel">The operation risk level.</param>
   /// <returns><c>true</c> if the risk level is <see cref="OperationRiskLevel.High"/> or higher; otherwise, <c>false</c>.</returns>
   public static bool IsHighOrHigher(this OperationRiskLevel riskLevel)
   {
      return riskLevel >= OperationRiskLevel.High;
   }

   /// <summary>
   /// Determines whether the risk level represents a dangerous operation.
   /// </summary>
   /// <param name="riskLevel">The operation risk level.</param>
   /// <returns><c>true</c> if the risk level is <see cref="OperationRiskLevel.Dangerous"/>; otherwise, <c>false</c>.</returns>
   public static bool IsDangerous(this OperationRiskLevel riskLevel)
   {
      return riskLevel == OperationRiskLevel.Dangerous;
   }

   /// <summary>
   /// Determines whether the risk level should usually require explicit approval before execution.
   /// </summary>
   /// <param name="riskLevel">The operation risk level.</param>
   /// <returns><c>true</c> if the risk level should usually require approval; otherwise, <c>false</c>.</returns>
   public static bool UsuallyRequiresApproval(this OperationRiskLevel riskLevel)
   {
      return riskLevel >= OperationRiskLevel.Medium;
   }

   /// <summary>
   /// Determines whether the risk level is usually safe for automatic execution.
   /// </summary>
   /// <param name="riskLevel">The operation risk level.</param>
   /// <returns><c>true</c> if the risk level is usually safe for automatic execution; otherwise, <c>false</c>.</returns>
   public static bool IsUsuallySafeForAutomaticExecution(this OperationRiskLevel riskLevel)
   {
      return riskLevel is OperationRiskLevel.None
          or OperationRiskLevel.ReadOnly
          or OperationRiskLevel.Low;
   }
}