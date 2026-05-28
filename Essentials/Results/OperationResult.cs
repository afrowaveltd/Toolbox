using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Extensions;
using Afrowave.Toolbox.Essentials.Interfaces;
using Afrowave.Toolbox.Essentials.Issues;
using Afrowave.Toolbox.Essentials.Metadata;

namespace Afrowave.Toolbox.Essentials.Results;

/// <summary>
/// Represents the result of an operation without a data payload.
/// </summary>
public sealed class OperationResult :
    IHasStatus<ResultStatus>,
    IHasMessage,
    IHasIssues,
    IHasMetadata
{
   /// <summary>
   /// Gets or sets the result status.
   /// </summary>
   public ResultStatus Status { get; set; } = ResultStatus.Unknown;

   /// <summary>
   /// Gets or sets the result message.
   /// </summary>
   public string Message { get; set; } = string.Empty;

   /// <summary>
   /// Gets or sets the issues attached to the result.
   /// </summary>
   public IReadOnlyList<IssueInfo> Issues { get; set; } = IssueInfoListFactory.Empty();

   /// <summary>
   /// Gets or sets the metadata attached to the result.
   /// </summary>
   public MetadataBag Metadata { get; set; } = MetadataBagFactory.Empty();

   /// <summary>
   /// Gets a value indicating whether the operation completed successfully.
   /// </summary>
   public bool IsSuccess =>
       Status is ResultStatus.Success or ResultStatus.SuccessWithWarnings;

   /// <summary>
   /// Gets a value indicating whether the operation failed.
   /// </summary>
   public bool IsFailure =>
       Status is ResultStatus.Failed or ResultStatus.Invalid or ResultStatus.NotFound;

   /// <summary>
   /// Gets a value indicating whether the operation completed with warnings.
   /// </summary>
   public bool HasWarnings =>
       Status == ResultStatus.SuccessWithWarnings
       || Issues.HasWarningsOrErrors();
}