using Afrowave.Toolbox.Essentials.Enums;

namespace Afrowave.Toolbox.Essentials.Interfaces;

/// <summary>
/// Represents a common operation result contract without a data payload.
/// </summary>
public interface IResult :
    IHasStatus<ResultStatus>,
    IHasMessage,
    IHasIssues,
    IHasMetadata
{
   /// <summary>
   /// Gets a value indicating whether the operation completed successfully.
   /// </summary>
   bool IsSuccess { get; }

   /// <summary>
   /// Gets a value indicating whether the operation failed.
   /// </summary>
   bool IsFailure { get; }

   /// <summary>
   /// Gets a value indicating whether the operation completed with warnings.
   /// </summary>
   bool HasWarnings { get; }
}