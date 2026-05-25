using Afrowave.Toolbox.Essentials.Issues;

namespace Afrowave.Toolbox.Essentials.Interfaces;

/// <summary>
/// Represents an object that carries one or more issues.
/// </summary>
public interface IHasIssues
{
   /// <summary>
   /// Gets issues associated with the object.
   /// </summary>
   IReadOnlyList<IssueInfo> Issues { get; }
}