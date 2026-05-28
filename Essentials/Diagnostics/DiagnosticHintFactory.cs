namespace Afrowave.Toolbox.Essentials.Diagnostics;

/// <summary>
/// Provides factory methods for creating <see cref="DiagnosticHint"/> instances.
/// </summary>
public static class DiagnosticHintFactory
{
   /// <summary>
   /// Creates a diagnostic hint with the specified kind.
   /// </summary>
   /// <param name="kind">The diagnostic hint kind.</param>
   /// <param name="message">The hint message.</param>
   /// <returns>The created diagnostic hint.</returns>
   public static DiagnosticHint Create(
       DiagnosticHintKind kind,
       string message)
   {
      ArgumentException.ThrowIfNullOrWhiteSpace(message);

      return new DiagnosticHint
      {
         Kind = kind,
         Message = message
      };
   }

   /// <summary>
   /// Creates a note diagnostic hint.
   /// </summary>
   /// <param name="message">The note message.</param>
   /// <returns>The created diagnostic hint.</returns>
   public static DiagnosticHint Note(string message)
   {
      return Create(DiagnosticHintKind.Note, message);
   }

   /// <summary>
   /// Creates a help diagnostic hint.
   /// </summary>
   /// <param name="message">The help message.</param>
   /// <returns>The created diagnostic hint.</returns>
   public static DiagnosticHint Help(string message)
   {
      return Create(DiagnosticHintKind.Help, message);
   }

   /// <summary>
   /// Creates a suggestion diagnostic hint.
   /// </summary>
   /// <param name="message">The suggestion message.</param>
   /// <returns>The created diagnostic hint.</returns>
   public static DiagnosticHint Suggestion(string message)
   {
      return Create(DiagnosticHintKind.Suggestion, message);
   }

   /// <summary>
   /// Creates an example diagnostic hint.
   /// </summary>
   /// <param name="message">The example message.</param>
   /// <returns>The created diagnostic hint.</returns>
   public static DiagnosticHint Example(string message)
   {
      return Create(DiagnosticHintKind.Example, message);
   }
}