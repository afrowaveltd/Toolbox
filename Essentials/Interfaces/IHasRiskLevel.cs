using Afrowave.Toolbox.Essentials.Enums;

namespace Afrowave.Toolbox.Essentials.Interfaces;

/// <summary>
/// Represents an object that has an operation risk level.
/// </summary>
public interface IHasRiskLevel
{
   /// <summary>
   /// Gets the risk level of the operation or object.
   /// </summary>
   OperationRiskLevel RiskLevel { get; }
}