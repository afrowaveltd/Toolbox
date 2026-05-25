namespace Afrowave.Toolbox.Essentials.Enums;

/// <summary>
/// Describes the expected risk level of an operation.
/// </summary>
public enum OperationRiskLevel
{
   /// <summary>
   /// No known risk or the risk is not applicable.
   /// </summary>
   None = 0,

   /// <summary>
   /// Read-only operation. It should not modify state.
   /// </summary>
   ReadOnly = 1,

   /// <summary>
   /// Low-risk operation with limited and easily reversible impact.
   /// </summary>
   Low = 2,

   /// <summary>
   /// Operation that may modify state but should usually be recoverable.
   /// </summary>
   Medium = 3,

   /// <summary>
   /// Operation that can significantly affect system behavior, data, security, or availability.
   /// </summary>
   High = 4,

   /// <summary>
   /// Operation that can cause destructive, security-sensitive, or hard-to-recover changes.
   /// </summary>
   Dangerous = 5
}
