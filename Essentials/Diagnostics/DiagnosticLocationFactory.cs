namespace Afrowave.Toolbox.Essentials.Diagnostics;

/// <summary>
/// Provides factory methods for creating <see cref="DiagnosticLocation"/> instances.
/// </summary>
public static class DiagnosticLocationFactory
{
   /// <summary>
   /// Creates an empty diagnostic location.
   /// </summary>
   /// <returns>An empty diagnostic location.</returns>
   public static DiagnosticLocation Empty()
   {
      return new DiagnosticLocation();
   }

   /// <summary>
   /// Creates a diagnostic location with a source.
   /// </summary>
   /// <param name="source">The source name, file path, URI, or logical input name.</param>
   /// <returns>The created diagnostic location.</returns>
   public static DiagnosticLocation FromSource(string source)
   {
      ArgumentException.ThrowIfNullOrWhiteSpace(source);

      return new DiagnosticLocation
      {
         Source = source
      };
   }

   /// <summary>
   /// Creates a diagnostic location with line and column information.
   /// </summary>
   /// <param name="line">The line number.</param>
   /// <param name="column">The column number.</param>
   /// <returns>The created diagnostic location.</returns>
   public static DiagnosticLocation FromLineColumn(
       int line,
       int column)
   {
      return new DiagnosticLocation
      {
         Line = line,
         Column = column
      };
   }

   /// <summary>
   /// Creates a diagnostic location with source, line, and column information.
   /// </summary>
   /// <param name="source">The source name, file path, URI, or logical input name.</param>
   /// <param name="line">The line number.</param>
   /// <param name="column">The column number.</param>
   /// <returns>The created diagnostic location.</returns>
   public static DiagnosticLocation FromSourceLineColumn(
       string source,
       int line,
       int column)
   {
      ArgumentException.ThrowIfNullOrWhiteSpace(source);

      return new DiagnosticLocation
      {
         Source = source,
         Line = line,
         Column = column
      };
   }

   /// <summary>
   /// Creates a diagnostic location with an offset.
   /// </summary>
   /// <param name="offset">The offset value.</param>
   /// <returns>The created diagnostic location.</returns>
   public static DiagnosticLocation FromOffset(long offset)
   {
      return new DiagnosticLocation
      {
         Offset = offset
      };
   }

   /// <summary>
   /// Creates a diagnostic location with source and offset information.
   /// </summary>
   /// <param name="source">The source name, file path, URI, or logical input name.</param>
   /// <param name="offset">The offset value.</param>
   /// <returns>The created diagnostic location.</returns>
   public static DiagnosticLocation FromSourceOffset(
       string source,
       long offset)
   {
      ArgumentException.ThrowIfNullOrWhiteSpace(source);

      return new DiagnosticLocation
      {
         Source = source,
         Offset = offset
      };
   }

   /// <summary>
   /// Creates a diagnostic location with source, line, column, and offset information.
   /// </summary>
   /// <param name="source">The source name, file path, URI, or logical input name.</param>
   /// <param name="line">The line number.</param>
   /// <param name="column">The column number.</param>
   /// <param name="offset">The offset value.</param>
   /// <returns>The created diagnostic location.</returns>
   public static DiagnosticLocation FromFullLocation(
       string source,
       int line,
       int column,
       long offset)
   {
      ArgumentException.ThrowIfNullOrWhiteSpace(source);

      return new DiagnosticLocation
      {
         Source = source,
         Line = line,
         Column = column,
         Offset = offset
      };
   }
}