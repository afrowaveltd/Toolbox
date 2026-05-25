using Afrowave.Toolbox.Essentials.Diagnostics;

namespace Afrowave.Toolbox.Essentials.Interfaces;

/// <summary>
/// Represents an object that carries diagnostic information.
/// </summary>
public interface IHasDiagnostics
{
   /// <summary>
   /// Gets diagnostics associated with the object.
   /// </summary>
   IReadOnlyList<DiagnosticInfo> Diagnostics { get; }
}