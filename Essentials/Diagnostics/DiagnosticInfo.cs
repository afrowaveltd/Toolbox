using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Interfaces;
using Afrowave.Toolbox.Essentials.Metadata;

namespace Afrowave.Toolbox.Essentials.Diagnostics;

/// <summary>
/// Represents a diagnostic message with optional location, span, hints, and metadata.
/// </summary>
public sealed class DiagnosticInfo : IHasCode, IHasSeverity, IHasMetadata
{
   /// <summary>
   /// Gets the stable diagnostic code.
   /// </summary>
   public string Code { get; init; } = string.Empty;

   /// <summary>
   /// Gets the diagnostic message.
   /// </summary>
   public string Message { get; init; } = string.Empty;

   /// <summary>
   /// Gets optional additional diagnostic details.
   /// </summary>
   public string? Details { get; init; }

   /// <summary>
   /// Gets the diagnostic severity.
   /// </summary>
   public IssueSeverity Severity { get; init; } = IssueSeverity.Information;

   /// <summary>
   /// Gets the primary diagnostic location, if available.
   /// </summary>
   public DiagnosticLocation? Location { get; init; }

   /// <summary>
   /// Gets diagnostic spans related to this diagnostic message.
   /// </summary>
   public IReadOnlyList<DiagnosticSpan> Spans { get; init; } = [];

   /// <summary>
   /// Gets diagnostic hints related to this diagnostic message.
   /// </summary>
   public IReadOnlyList<DiagnosticHint> Hints { get; init; } = [];

   /// <summary>
   /// Gets metadata associated with this diagnostic message.
   /// </summary>
   public MetadataBag Metadata { get; init; } = new();
}