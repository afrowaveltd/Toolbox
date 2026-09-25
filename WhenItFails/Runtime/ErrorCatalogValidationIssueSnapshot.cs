using Afrowave.Toolbox.WhenItFails.Enums;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Runtime;

/// <summary>
/// Detached, read-only record of one validation issue at capture time.
/// </summary>
public sealed class ErrorCatalogValidationIssueSnapshot
{
    internal ErrorCatalogValidationIssueSnapshot(ErrorCatalogValidationIssue source)
    {
        ArgumentNullException.ThrowIfNull(source);

        Severity = source.Severity;
        Code = source.Code;
        Message = source.Message;
        ErrorId = source.ErrorId;
        ErrorName = source.ErrorName;
        Path = source.Path;
    }

    /// <summary>Gets the captured issue severity.</summary>
    public ErrorCatalogValidationSeverity Severity { get; }

    /// <summary>Gets the captured issue code.</summary>
    public string Code { get; }

    /// <summary>Gets the captured issue message.</summary>
    public string Message { get; }

    /// <summary>Gets the captured related error ID, if present.</summary>
    public string? ErrorId { get; }

    /// <summary>Gets the captured related error name, if present.</summary>
    public string? ErrorName { get; }

    /// <summary>Gets the captured issue path, if present.</summary>
    public string? Path { get; }
}
