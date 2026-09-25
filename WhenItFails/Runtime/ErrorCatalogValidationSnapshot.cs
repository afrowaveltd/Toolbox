using Afrowave.Toolbox.WhenItFails.Enums;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Runtime;

/// <summary>
/// Detached, read-only record of the active context's cross-validation findings.
/// </summary>
/// <remarks>
/// This captures the issue list and severities at construction. It does not
/// rerun cross-validation or guarantee that a concurrently mutated source
/// can be captured transactionally.
/// </remarks>
public sealed class ErrorCatalogValidationSnapshot
{
    internal ErrorCatalogValidationSnapshot(ErrorCatalogValidationResult source)
    {
        ArgumentNullException.ThrowIfNull(source);

        IReadOnlyList<ErrorCatalogValidationIssueSnapshot> issues =
            Array.AsReadOnly(
                source.Issues
                    .Select(issue => new ErrorCatalogValidationIssueSnapshot(issue))
                    .ToArray());

        Issues = issues;
        IsValid = !issues.Any(issue =>
            issue.Severity == ErrorCatalogValidationSeverity.Error);
    }

    /// <summary>Gets detached validation issues captured in source order.</summary>
    public IReadOnlyList<ErrorCatalogValidationIssueSnapshot> Issues { get; }

    /// <summary>
    /// Gets validity computed from the captured issue severities, not live issues.
    /// </summary>
    public bool IsValid { get; }
}
