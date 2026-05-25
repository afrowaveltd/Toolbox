namespace Afrowave.Toolbox.Essentials.Diagnostics;

/// <summary>
/// Represents a location in a source document, file, stream, or input text.
/// </summary>
public sealed class DiagnosticLocation
{
   /// <summary>
   /// Gets the optional source name, file path, URI, or logical input name.
   /// </summary>
   public string? Source { get; init; }

   /// <summary>
   /// Gets the one-based line number, if available.
   /// </summary>
   public int? Line { get; init; }

   /// <summary>
   /// Gets the one-based column number, if available.
   /// </summary>
   public int? Column { get; init; }

   /// <summary>
   /// Gets the zero-based absolute offset, if available.
   /// </summary>
   public long? Offset { get; init; }
}