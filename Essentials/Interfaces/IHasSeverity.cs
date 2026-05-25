using Afrowave.Toolbox.Essentials.Enums;

namespace Afrowave.Toolbox.Essentials.Interfaces;

/// <summary>
/// Represents an object that has a severity level.
/// </summary>
public interface IHasSeverity
{
   /// <summary>
   /// Gets the severity level of the object.
   /// </summary>
   IssueSeverity Severity { get; }
}