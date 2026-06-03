namespace Afrowave.Toolbox.Essentials.Issues;

/// <summary>
/// Provides factory methods for creating issue information lists.
/// </summary>
public static class IssueInfoListFactory
{
   /// <summary>
   /// Creates an empty issue list.
   /// </summary>
   /// <returns>An empty issue list.</returns>
   public static IReadOnlyList<IssueInfo> Empty()
   {
      return new List<IssueInfo>().AsReadOnly();
   }


   /// <summary>
   /// Creates an issue list from issue instances.
   /// </summary>
   /// <param name="issues">The issues.</param>
   /// <returns>An issue list.</returns>
   public static IReadOnlyList<IssueInfo> From(
      params IssueInfo[] issues)
   {
      ArgumentNullException.ThrowIfNull(issues);

      return Array.AsReadOnly(issues.ToArray());
   }

   /// <summary>
   /// Creates an issue list from an enumerable collection.
   /// </summary>
   /// <param name="issues">The issues.</param>
   /// <returns>An issue list.</returns>
   public static IReadOnlyList<IssueInfo> From(
      IEnumerable<IssueInfo> issues)
   {
      ArgumentNullException.ThrowIfNull(issues);

      return Array.AsReadOnly(issues.ToArray());
   }

   /// <summary>
   /// Creates an issue list with one informational issue.
   /// </summary>
   /// <param name="code">The stable issue code.</param>
   /// <param name="message">The issue message.</param>
   /// <returns>An issue list containing one informational issue.</returns>
   public static IReadOnlyList<IssueInfo> Information(
      string code,
      string message)
   {
      return Array.AsReadOnly(
         new[]
         {
            IssueInfoFactory.Information(code, message)
         });
   }

   /// <summary>
   /// Creates an issue list with one warning issue.
   /// </summary>
   /// <param name="code">The stable issue code.</param>
   /// <param name="message">The issue message.</param>
   /// <returns>An issue list containing one warning issue.</returns>
   public static IReadOnlyList<IssueInfo> Warning(
      string code,
      string message)
   {
      return Array.AsReadOnly(
         new[]
         {
            IssueInfoFactory.Warning(code, message)
         });
   }

   /// <summary>
   /// Creates an issue list with one error issue.
   /// </summary>
   /// <param name="code">The stable issue code.</param>
   /// <param name="message">The issue message.</param>
   /// <returns>An issue list containing one error issue.</returns>
   public static IReadOnlyList<IssueInfo> Error(
      string code,
      string message)
   {
      return Array.AsReadOnly(
         new[]
         {
            IssueInfoFactory.Error(code, message)
         });
   }

   /// <summary>
   /// Creates an issue list with one critical issue.
   /// </summary>
   /// <param name="code">The stable issue code.</param>
   /// <param name="message">The issue message.</param>
   /// <returns>An issue list containing one critical issue.</returns>
   public static IReadOnlyList<IssueInfo> Critical(
      string code,
      string message)
   {
      return Array.AsReadOnly(
         new[]
         {
            IssueInfoFactory.Critical(code, message)
         });
   }

   /// <summary>
   /// Creates an issue list with one fatal issue.
   /// </summary>
   /// <param name="code">The stable issue code.</param>
   /// <param name="message">The issue message.</param>
   /// <returns>An issue list containing one fatal issue.</returns>
   public static IReadOnlyList<IssueInfo> Fatal(
      string code,
      string message)
   {
      return Array.AsReadOnly(
         new[]
         {
            IssueInfoFactory.Fatal(code, message)
         });
   }
}