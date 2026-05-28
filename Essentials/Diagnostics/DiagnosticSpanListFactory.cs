namespace Afrowave.Toolbox.Essentials.Diagnostics;

/// <summary>
/// Provides factory methods for creating diagnostic span lists.
/// </summary>
public static class DiagnosticSpanListFactory
{
   /// <summary>
   /// Creates an empty diagnostic span list.
   /// </summary>
   /// <returns>An empty diagnostic span list.</returns>
   public static IReadOnlyList<DiagnosticSpan> Empty()
   {
      return new List<DiagnosticSpan>().AsReadOnly();
   }

   /// <summary>
   /// Creates a diagnostic span list from span instances.
   /// </summary>
   /// <param name="spans">The diagnostic spans.</param>
   /// <returns>A diagnostic span list.</returns>
   public static IReadOnlyList<DiagnosticSpan> From(
       params DiagnosticSpan[] spans)
   {
      ArgumentNullException.ThrowIfNull(spans);

      return [.. spans];
   }

   /// <summary>
   /// Creates a diagnostic span list from an enumerable collection.
   /// </summary>
   /// <param name="spans">The diagnostic spans.</param>
   /// <returns>A diagnostic span list.</returns>
   public static IReadOnlyList<DiagnosticSpan> From(
       IEnumerable<DiagnosticSpan> spans)
   {
      ArgumentNullException.ThrowIfNull(spans);

      return [.. spans];
   }

   /// <summary>
   /// Creates a diagnostic span list with one span from a start location.
   /// </summary>
   /// <param name="start">The start location.</param>
   /// <returns>A diagnostic span list containing one span.</returns>
   public static IReadOnlyList<DiagnosticSpan> FromStart(
       DiagnosticLocation start)
   {
      return
      [
          DiagnosticSpanFactory.FromStart(start)
      ];
   }

   /// <summary>
   /// Creates a diagnostic span list with one span from start and end locations.
   /// </summary>
   /// <param name="start">The start location.</param>
   /// <param name="end">The end location.</param>
   /// <returns>A diagnostic span list containing one span.</returns>
   public static IReadOnlyList<DiagnosticSpan> FromStartEnd(
       DiagnosticLocation start,
       DiagnosticLocation end)
   {
      return
      [
          DiagnosticSpanFactory.FromStartEnd(start, end)
      ];
   }

   /// <summary>
   /// Creates a diagnostic span list with one labeled span.
   /// </summary>
   /// <param name="label">The span label.</param>
   /// <returns>A diagnostic span list containing one span.</returns>
   public static IReadOnlyList<DiagnosticSpan> FromLabel(string label)
   {
      return
      [
          DiagnosticSpanFactory.FromLabel(label)
      ];
   }

   /// <summary>
   /// Creates a diagnostic span list with one source range span.
   /// </summary>
   /// <param name="source">The source name, file path, URI, or logical input name.</param>
   /// <param name="startLine">The start line.</param>
   /// <param name="startColumn">The start column.</param>
   /// <param name="endLine">The end line.</param>
   /// <param name="endColumn">The end column.</param>
   /// <returns>A diagnostic span list containing one span.</returns>
   public static IReadOnlyList<DiagnosticSpan> FromSourceRange(
       string source,
       int startLine,
       int startColumn,
       int endLine,
       int endColumn)
   {
      return
      [
          DiagnosticSpanFactory.FromSourceRange(
                source,
                startLine,
                startColumn,
                endLine,
                endColumn)
      ];
   }

   /// <summary>
   /// Creates a diagnostic span list with one source range span and label.
   /// </summary>
   /// <param name="source">The source name, file path, URI, or logical input name.</param>
   /// <param name="startLine">The start line.</param>
   /// <param name="startColumn">The start column.</param>
   /// <param name="endLine">The end line.</param>
   /// <param name="endColumn">The end column.</param>
   /// <param name="label">The span label.</param>
   /// <returns>A diagnostic span list containing one span.</returns>
   public static IReadOnlyList<DiagnosticSpan> FromSourceRangeLabel(
       string source,
       int startLine,
       int startColumn,
       int endLine,
       int endColumn,
       string label)
   {
      return
      [
          DiagnosticSpanFactory.FromSourceRangeLabel(
                source,
                startLine,
                startColumn,
                endLine,
                endColumn,
                label)
      ];
   }

   /// <summary>
   /// Creates a diagnostic span list with one offset range span.
   /// </summary>
   /// <param name="startOffset">The start offset.</param>
   /// <param name="endOffset">The end offset.</param>
   /// <returns>A diagnostic span list containing one span.</returns>
   public static IReadOnlyList<DiagnosticSpan> FromOffsetRange(
       long startOffset,
       long endOffset)
   {
      return
      [
          DiagnosticSpanFactory.FromOffsetRange(
                startOffset,
                endOffset)
      ];
   }

   /// <summary>
   /// Creates a diagnostic span list with one offset range span and label.
   /// </summary>
   /// <param name="startOffset">The start offset.</param>
   /// <param name="endOffset">The end offset.</param>
   /// <param name="label">The span label.</param>
   /// <returns>A diagnostic span list containing one span.</returns>
   public static IReadOnlyList<DiagnosticSpan> FromOffsetRangeLabel(
       long startOffset,
       long endOffset,
       string label)
   {
      return
      [
          DiagnosticSpanFactory.FromOffsetRangeLabel(
                startOffset,
                endOffset,
                label)
      ];
   }
}