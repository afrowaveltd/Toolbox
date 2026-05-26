using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Interfaces;

namespace Afrowave.Toolbox.Essentials.Extensions;

/// <summary>
/// Provides helper methods for objects carrying an operation risk level.
/// </summary>
public static class HasRiskLevelExtensions
{
   /// <summary>
   /// Determines whether the object's risk level matches the specified risk level.
   /// </summary>
   /// <param name="value">The object carrying a risk level.</param>
   /// <param name="riskLevel">The risk level to compare with.</param>
   /// <returns><c>true</c> if the object has the specified risk level; otherwise, <c>false</c>.</returns>
   public static bool HasRiskLevel(
       this IHasRiskLevel value,
       OperationRiskLevel riskLevel)
   {
      ArgumentNullException.ThrowIfNull(value);

      return value.RiskLevel == riskLevel;
   }

   /// <summary>
   /// Determines whether the object's risk level represents a read-only operation.
   /// </summary>
   /// <param name="value">The object carrying a risk level.</param>
   /// <returns><c>true</c> if the risk level is <see cref="OperationRiskLevel.ReadOnly"/>; otherwise, <c>false</c>.</returns>
   public static bool IsReadOnlyRisk(this IHasRiskLevel value)
   {
      ArgumentNullException.ThrowIfNull(value);

      return value.RiskLevel.IsReadOnly();
   }

   /// <summary>
   /// Determines whether the object's risk level represents a medium or higher risk.
   /// </summary>
   /// <param name="value">The object carrying a risk level.</param>
   /// <returns><c>true</c> if the risk level is <see cref="OperationRiskLevel.Medium"/> or higher; otherwise, <c>false</c>.</returns>
   public static bool HasMediumOrHigherRisk(this IHasRiskLevel value)
   {
      ArgumentNullException.ThrowIfNull(value);

      return value.RiskLevel.IsMediumOrHigher();
   }

   /// <summary>
   /// Determines whether the object's risk level represents a high or higher risk.
   /// </summary>
   /// <param name="value">The object carrying a risk level.</param>
   /// <returns><c>true</c> if the risk level is <see cref="OperationRiskLevel.High"/> or higher; otherwise, <c>false</c>.</returns>
   public static bool HasHighOrHigherRisk(this IHasRiskLevel value)
   {
      ArgumentNullException.ThrowIfNull(value);

      return value.RiskLevel.IsHighOrHigher();
   }

   /// <summary>
   /// Determines whether the object's risk level represents a dangerous operation.
   /// </summary>
   /// <param name="value">The object carrying a risk level.</param>
   /// <returns><c>true</c> if the risk level is <see cref="OperationRiskLevel.Dangerous"/>; otherwise, <c>false</c>.</returns>
   public static bool IsDangerousRisk(this IHasRiskLevel value)
   {
      ArgumentNullException.ThrowIfNull(value);

      return value.RiskLevel.IsDangerous();
   }

   /// <summary>
   /// Determines whether the object's risk level should usually require explicit approval before execution.
   /// </summary>
   /// <param name="value">The object carrying a risk level.</param>
   /// <returns><c>true</c> if the risk level should usually require approval; otherwise, <c>false</c>.</returns>
   public static bool UsuallyRequiresApproval(this IHasRiskLevel value)
   {
      ArgumentNullException.ThrowIfNull(value);

      return value.RiskLevel.UsuallyRequiresApproval();
   }

   /// <summary>
   /// Determines whether the object's risk level is usually safe for automatic execution.
   /// </summary>
   /// <param name="value">The object carrying a risk level.</param>
   /// <returns><c>true</c> if the risk level is usually safe for automatic execution; otherwise, <c>false</c>.</returns>
   public static bool IsUsuallySafeForAutomaticExecution(this IHasRiskLevel value)
   {
      ArgumentNullException.ThrowIfNull(value);

      return value.RiskLevel.IsUsuallySafeForAutomaticExecution();
   }
}