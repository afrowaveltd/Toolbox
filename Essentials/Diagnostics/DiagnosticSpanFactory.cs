namespace Afrowave.Toolbox.Essentials.Diagnostics;

/// <summary>
/// Provides factory methods for creating <see cref="DiagnosticSpan"/> instances.
/// </summary>
public static class DiagnosticSpanFactory
{
   /// <summary>
   /// Creates an empty diagnostic span.
   /// </summary>
   /// <returns>An empty diagnostic span.</returns>
   public static DiagnosticSpan Empty()
   {
      return new DiagnosticSpan();
   }

   /// <summary>
   /// Creates a diagnostic span with a start location.
   /// </summary>
   /// <param name="start">The start location.</param>
   /// <returns>The created diagnostic span.</returns>
   public static DiagnosticSpan FromStart(DiagnosticLocation start)
   {
      ArgumentNullException.ThrowIfNull(start);

      return new DiagnosticSpan
      {
         Start = start
      };
   }

   /// <summary>
   /// Creates a diagnostic span with a start location and an end location.
   /// </summary>
   /// <param name="start">The start location.</param>
   /// <param name="end">The end location.</param>
   /// <returns>The created diagnostic span.</returns>
   public static DiagnosticSpan FromStartEnd(
       DiagnosticLocation start,
       DiagnosticLocation end)
   {
      ArgumentNullException.ThrowIfNull(start);
      ArgumentNullException.ThrowIfNull(end);

      return new DiagnosticSpan
      {
         Start = start,
         End = end
      };
   }

   /// <summary>
   /// Creates a diagnostic span with a label.
   /// </summary>
   /// <param name="label">The span label.</param>
   /// <returns>The created diagnostic span.</returns>
   public static DiagnosticSpan FromLabel(string label)
   {
      ArgumentException.ThrowIfNullOrWhiteSpace(label);

      return new DiagnosticSpan
      {
         Label = label
      };
   }

   /// <summary>
   /// Creates a diagnostic span with a start location and a label.
   /// </summary>
   public static DiagnosticSpan FromStartLabel(
       DiagnosticLocation start,
       string label)
   {
      ArgumentNullException.ThrowIfNull(start);
      ArgumentException.ThrowIfNullOrWhiteSpace(label);

      return new DiagnosticSpan
      {
         Start = start,
         Label = label
      };
   }

   /// <summary>
   /// Creates a diagnostic span with a start location, an end location, and a label.
   /// </summary>
   public static DiagnosticSpan FromStartEndLabel(
       DiagnosticLocation start,
       DiagnosticLocation end,
       string label)
   {
      ArgumentNullException.ThrowIfNull(start);
      ArgumentNullException.ThrowIfNull(end);
      ArgumentException.ThrowIfNullOrWhiteSpace(label);

      return new DiagnosticSpan
      {
         Start = start,
         End = end,
         Label = label
      };
   }

   /// <summary>
   /// Creates a diagnostic span from source line and column values.
   /// </summary>
   public static DiagnosticSpan FromSourceRange(
       string source,
       int startLine,
       int startColumn,
       int endLine,
       int endColumn)
   {
      ArgumentException.ThrowIfNullOrWhiteSpace(source);

      return new DiagnosticSpan
      {
         Start = DiagnosticLocationFactory.FromSourceLineColumn(
              source,
              startLine,
              startColumn),
         End = DiagnosticLocationFactory.FromSourceLineColumn(
              source,
              endLine,
              endColumn)
      };
   }

   /// <summary>
   /// Creates a diagnostic span from source line and column values with a label.
   /// </summary>
   public static DiagnosticSpan FromSourceRangeLabel(
       string source,
       int startLine,
       int startColumn,
       int endLine,
       int endColumn,
       string label)
   {
      ArgumentException.ThrowIfNullOrWhiteSpace(source);
      ArgumentException.ThrowIfNullOrWhiteSpace(label);

      return new DiagnosticSpan
      {
         Start = DiagnosticLocationFactory.FromSourceLineColumn(
              source,
              startLine,
              startColumn),
         End = DiagnosticLocationFactory.FromSourceLineColumn(
              source,
              endLine,
              endColumn),
         Label = label
      };
   }

   /// <summary>
   /// Creates a diagnostic span from offsets.
   /// </summary>
   public static DiagnosticSpan FromOffsetRange(
       long startOffset,
       long endOffset)
   {
      return new DiagnosticSpan
      {
         Start = DiagnosticLocationFactory.FromOffset(startOffset),
         End = DiagnosticLocationFactory.FromOffset(endOffset)
      };
   }

   /// <summary>
   /// Creates a diagnostic span from offsets with a label.
   /// </summary>
   public static DiagnosticSpan FromOffsetRangeLabel(
       long startOffset,
       long endOffset,
       string label)
   {
      ArgumentException.ThrowIfNullOrWhiteSpace(label);

      return new DiagnosticSpan
      {
         Start = DiagnosticLocationFactory.FromOffset(startOffset),
         End = DiagnosticLocationFactory.FromOffset(endOffset),
         Label = label
      };
   }
}