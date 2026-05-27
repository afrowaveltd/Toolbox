using Afrowave.Toolbox.Essentials.Diagnostics;

namespace Afrowave.Toolbox.Essentials.Extensions;

/// <summary>
/// Provides helper methods for working with <see cref="DiagnosticHint"/>.
/// </summary>
public static class DiagnosticHintExtensions
{
   /// <summary>
   /// Determines whether the diagnostic hint is a note.
   /// </summary>
   /// <param name="hint">The diagnostic hint.</param>
   /// <returns><c>true</c> if the hint is a note; otherwise, <c>false</c>.</returns>
   public static bool IsNote(this DiagnosticHint hint)
   {
      ArgumentNullException.ThrowIfNull(hint);

      return hint.Kind == DiagnosticHintKind.Note;
   }

   /// <summary>
   /// Determines whether the diagnostic hint is help text.
   /// </summary>
   /// <param name="hint">The diagnostic hint.</param>
   /// <returns><c>true</c> if the hint is help text; otherwise, <c>false</c>.</returns>
   public static bool IsHelp(this DiagnosticHint hint)
   {
      ArgumentNullException.ThrowIfNull(hint);

      return hint.Kind == DiagnosticHintKind.Help;
   }

   /// <summary>
   /// Determines whether the diagnostic hint is a suggestion.
   /// </summary>
   /// <param name="hint">The diagnostic hint.</param>
   /// <returns><c>true</c> if the hint is a suggestion; otherwise, <c>false</c>.</returns>
   public static bool IsSuggestion(this DiagnosticHint hint)
   {
      ArgumentNullException.ThrowIfNull(hint);

      return hint.Kind == DiagnosticHintKind.Suggestion;
   }

   /// <summary>
   /// Determines whether the diagnostic hint is an example.
   /// </summary>
   /// <param name="hint">The diagnostic hint.</param>
   /// <returns><c>true</c> if the hint is an example; otherwise, <c>false</c>.</returns>
   public static bool IsExample(this DiagnosticHint hint)
   {
      ArgumentNullException.ThrowIfNull(hint);

      return hint.Kind == DiagnosticHintKind.Example;
   }

   /// <summary>
   /// Determines whether the diagnostic hint has a non-empty message.
   /// </summary>
   /// <param name="hint">The diagnostic hint.</param>
   /// <returns><c>true</c> if the hint message is not null, empty, or whitespace; otherwise, <c>false</c>.</returns>
   public static bool HasMessage(this DiagnosticHint hint)
   {
      ArgumentNullException.ThrowIfNull(hint);

      return !string.IsNullOrWhiteSpace(hint.Message);
   }

   /// <summary>
   /// Determines whether the diagnostic hint message contains the specified text, ignoring case.
   /// </summary>
   /// <param name="hint">The diagnostic hint.</param>
   /// <param name="text">The text to search for.</param>
   /// <returns><c>true</c> if the hint message contains the specified text ignoring case; otherwise, <c>false</c>.</returns>
   public static bool MessageContains(
       this DiagnosticHint hint,
       string text)
   {
      ArgumentNullException.ThrowIfNull(hint);
      ArgumentException.ThrowIfNullOrWhiteSpace(text);

      return hint.Message.Contains(
          text,
          StringComparison.OrdinalIgnoreCase);
   }
}