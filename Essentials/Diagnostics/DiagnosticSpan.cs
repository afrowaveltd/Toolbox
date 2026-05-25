namespace Afrowave.Toolbox.Essentials.Diagnostics;

/// <summary>
/// Represents a diagnostic span in a source document, file, stream, or input text.
/// </summary>
public sealed class DiagnosticSpan
{
   /// <summary>
   /// Gets the start location of the diagnostic span.
   /// </summary>
   public DiagnosticLocation Start { get; init; } = new();

   /// <summary>
   /// Gets the optional end location of the diagnostic span.
   /// </summary>
   public DiagnosticLocation? End { get; init; }

   /// <summary>
   /// Gets an optional label describing this span.
   /// </summary>
   public string? Label { get; init; }
}