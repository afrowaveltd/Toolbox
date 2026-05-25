namespace Afrowave.Toolbox.Essentials.Diagnostics;

/// <summary>
/// Represents a diagnostic hint, such as a note, help text, suggestion, or example.
/// </summary>
public sealed class DiagnosticHint
{
   /// <summary>
   /// Gets the kind of diagnostic hint.
   /// </summary>
   public DiagnosticHintKind Kind { get; init; } = DiagnosticHintKind.Note;

   /// <summary>
   /// Gets the hint message.
   /// </summary>
   public string Message { get; init; } = string.Empty;
}