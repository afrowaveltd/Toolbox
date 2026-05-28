namespace Afrowave.Toolbox.Essentials.Diagnostics;

/// <summary>
/// Provides factory methods for creating diagnostic information lists.
/// </summary>
public static class DiagnosticInfoListFactory
{
   /// <summary>
   /// Creates an empty diagnostic list.
   /// </summary>
   /// <returns>An empty diagnostic list.</returns>
   public static IReadOnlyList<DiagnosticInfo> Empty()
   {
      return new List<DiagnosticInfo>().AsReadOnly();
   }

   /// <summary>
   /// Creates a diagnostic list from diagnostic instances.
   /// </summary>
   /// <param name="diagnostics">The diagnostic messages.</param>
   /// <returns>A diagnostic list.</returns>
   public static IReadOnlyList<DiagnosticInfo> From(
       params DiagnosticInfo[] diagnostics)
   {
      ArgumentNullException.ThrowIfNull(diagnostics);

      return [.. diagnostics];
   }

   /// <summary>
   /// Creates a diagnostic list from an enumerable collection.
   /// </summary>
   /// <param name="diagnostics">The diagnostic messages.</param>
   /// <returns>A diagnostic list.</returns>
   public static IReadOnlyList<DiagnosticInfo> From(
       IEnumerable<DiagnosticInfo> diagnostics)
   {
      ArgumentNullException.ThrowIfNull(diagnostics);

      return [.. diagnostics];
   }

   /// <summary>
   /// Creates a diagnostic list with one informational diagnostic message.
   /// </summary>
   /// <param name="code">The stable diagnostic code.</param>
   /// <param name="message">The diagnostic message.</param>
   /// <returns>A diagnostic list containing one informational diagnostic message.</returns>
   public static IReadOnlyList<DiagnosticInfo> Information(
       string code,
       string message)
   {
      return
      [
          DiagnosticInfoFactory.Information(code, message)
      ];
   }

   /// <summary>
   /// Creates a diagnostic list with one warning diagnostic message.
   /// </summary>
   /// <param name="code">The stable diagnostic code.</param>
   /// <param name="message">The diagnostic message.</param>
   /// <returns>A diagnostic list containing one warning diagnostic message.</returns>
   public static IReadOnlyList<DiagnosticInfo> Warning(
       string code,
       string message)
   {
      return
      [
          DiagnosticInfoFactory.Warning(code, message)
      ];
   }

   /// <summary>
   /// Creates a diagnostic list with one error diagnostic message.
   /// </summary>
   /// <param name="code">The stable diagnostic code.</param>
   /// <param name="message">The diagnostic message.</param>
   /// <returns>A diagnostic list containing one error diagnostic message.</returns>
   public static IReadOnlyList<DiagnosticInfo> Error(
       string code,
       string message)
   {
      return
      [
          DiagnosticInfoFactory.Error(code, message)
      ];
   }

   /// <summary>
   /// Creates a diagnostic list with one critical diagnostic message.
   /// </summary>
   /// <param name="code">The stable diagnostic code.</param>
   /// <param name="message">The diagnostic message.</param>
   /// <returns>A diagnostic list containing one critical diagnostic message.</returns>
   public static IReadOnlyList<DiagnosticInfo> Critical(
       string code,
       string message)
   {
      return
      [
          DiagnosticInfoFactory.Critical(code, message)
      ];
   }

   /// <summary>
   /// Creates a diagnostic list with one fatal diagnostic message.
   /// </summary>
   /// <param name="code">The stable diagnostic code.</param>
   /// <param name="message">The diagnostic message.</param>
   /// <returns>A diagnostic list containing one fatal diagnostic message.</returns>
   public static IReadOnlyList<DiagnosticInfo> Fatal(
       string code,
       string message)
   {
      return
      [
          DiagnosticInfoFactory.Fatal(code, message)
      ];
   }
}