using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Interfaces;
using Afrowave.Toolbox.Essentials.Metadata;

namespace Afrowave.Toolbox.Essentials.Issues;

/// <summary>
/// Represents a lightweight issue, warning, error, or diagnostic message.
/// </summary>
public sealed class IssueInfo :
   IHasCode,
   IHasNumber,
   IHasDetails,
   IHasSeverity,
   IHasMessage,
   IHasMetadata
{
   /// <summary>
   /// Gets the stable issue code.
   /// </summary>
   public string Code { get; init; } = string.Empty;

   /// <summary>
   /// Gets the optional numeric issue number.
   /// </summary>
   public int? Number { get; init; }

   /// <summary>
   /// Gets the issue message.
   /// </summary>
   public string Message { get; init; } = string.Empty;

   /// <summary>
   /// Gets optional additional issue details.
   /// </summary>
   public string? Details { get; init; }

   /// <summary>
   /// Gets the issue severity.
   /// </summary>
   public IssueSeverity Severity { get; init; } = IssueSeverity.Error;

   /// <summary>
   /// Gets metadata associated with the issue.
   /// </summary>
   public MetadataBag Metadata { get; init; } = new();
}