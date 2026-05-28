using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Metadata;

namespace Afrowave.Toolbox.Essentials.Issues;

/// <summary>
/// Provides factory methods for creating <see cref="IssueInfo"/> instances.
/// </summary>
public static class IssueInfoFactory
{
   /// <summary>
   /// Creates an issue with the specified severity.
   /// </summary>
   /// <param name="code">The stable issue code.</param>
   /// <param name="message">The issue message.</param>
   /// <param name="severity">The issue severity.</param>
   /// <returns>The created issue.</returns>
   public static IssueInfo Create(
       string code,
       string message,
       IssueSeverity severity)
   {
      ArgumentException.ThrowIfNullOrWhiteSpace(code);
      ArgumentException.ThrowIfNullOrWhiteSpace(message);

      return new IssueInfo
      {
         Code = code,
         Message = message,
         Severity = severity
      };
   }

   /// <summary>
   /// Creates an issue with the specified severity and details.
   /// </summary>
   /// <param name="code">The stable issue code.</param>
   /// <param name="message">The issue message.</param>
   /// <param name="details">Optional detailed issue information.</param>
   /// <param name="severity">The issue severity.</param>
   /// <returns>The created issue.</returns>
   public static IssueInfo Create(
       string code,
       string message,
       string? details,
       IssueSeverity severity)
   {
      var issue = Create(code, message, severity);

      return new IssueInfo
      {
         Code = issue.Code,
         Message = issue.Message,
         Details = details,
         Severity = issue.Severity
      };
   }

   /// <summary>
   /// Creates an informational issue.
   /// </summary>
   /// <param name="code">The stable issue code.</param>
   /// <param name="message">The issue message.</param>
   /// <returns>The created issue.</returns>
   public static IssueInfo Information(
       string code,
       string message)
   {
      return Create(code, message, IssueSeverity.Information);
   }

   /// <summary>
   /// Creates a warning issue.
   /// </summary>
   /// <param name="code">The stable issue code.</param>
   /// <param name="message">The issue message.</param>
   /// <returns>The created issue.</returns>
   public static IssueInfo Warning(
       string code,
       string message)
   {
      return Create(code, message, IssueSeverity.Warning);
   }

   /// <summary>
   /// Creates an error issue.
   /// </summary>
   /// <param name="code">The stable issue code.</param>
   /// <param name="message">The issue message.</param>
   /// <returns>The created issue.</returns>
   public static IssueInfo Error(
       string code,
       string message)
   {
      return Create(code, message, IssueSeverity.Error);
   }

   /// <summary>
   /// Creates a critical issue.
   /// </summary>
   /// <param name="code">The stable issue code.</param>
   /// <param name="message">The issue message.</param>
   /// <returns>The created issue.</returns>
   public static IssueInfo Critical(
       string code,
       string message)
   {
      return Create(code, message, IssueSeverity.Critical);
   }

   /// <summary>
   /// Creates a fatal issue.
   /// </summary>
   /// <param name="code">The stable issue code.</param>
   /// <param name="message">The issue message.</param>
   /// <returns>The created issue.</returns>
   public static IssueInfo Fatal(
       string code,
       string message)
   {
      return Create(code, message, IssueSeverity.Fatal);
   }

   /// <summary>
   /// Creates a copy of an issue with metadata.
   /// </summary>
   /// <param name="issue">The source issue.</param>
   /// <param name="metadata">The metadata to attach.</param>
   /// <returns>A new issue with copied values and the specified metadata.</returns>
   public static IssueInfo WithMetadata(
       IssueInfo issue,
       MetadataBag metadata)
   {
      ArgumentNullException.ThrowIfNull(issue);
      ArgumentNullException.ThrowIfNull(metadata);

      return new IssueInfo
      {
         Code = issue.Code,
         Number = issue.Number,
         Message = issue.Message,
         Details = issue.Details,
         Severity = issue.Severity,
         Metadata = metadata
      };
   }

   /// <summary>
   /// Creates an issue with the specified severity and numeric identifier.
   /// </summary>
   /// <param name="code">The stable issue code.</param>
   /// <param name="number">The optional numeric issue identifier.</param>
   /// <param name="message">The issue message.</param>
   /// <param name="severity">The issue severity.</param>
   /// <returns>The created issue.</returns>
   public static IssueInfo Create(
       string code,
       int? number,
       string message,
       IssueSeverity severity)
   {
      ArgumentException.ThrowIfNullOrWhiteSpace(code);
      ArgumentException.ThrowIfNullOrWhiteSpace(message);

      return new IssueInfo
      {
         Code = code,
         Number = number,
         Message = message,
         Severity = severity
      };
   }

   /// <summary>
   /// Creates a copy of an issue with a numeric identifier.
   /// </summary>
   /// <param name="issue">The source issue.</param>
   /// <param name="number">The numeric identifier to attach.</param>
   /// <returns>A new issue with copied values and the specified numeric identifier.</returns>
   public static IssueInfo WithNumber(
       IssueInfo issue,
       int? number)
   {
      ArgumentNullException.ThrowIfNull(issue);

      return new IssueInfo
      {
         Code = issue.Code,
         Number = number,
         Message = issue.Message,
         Details = issue.Details,
         Severity = issue.Severity,
         Metadata = issue.Metadata
      };
   }

   /// <summary>
   /// Creates a copy of an issue with detailed information.
   /// </summary>
   /// <param name="issue">The source issue.</param>
   /// <param name="details">The detailed issue information.</param>
   /// <returns>A new issue with copied values and the specified details.</returns>
   public static IssueInfo WithDetails(
       IssueInfo issue,
       string? details)
   {
      ArgumentNullException.ThrowIfNull(issue);

      return new IssueInfo
      {
         Code = issue.Code,
         Number = issue.Number,
         Message = issue.Message,
         Details = details,
         Severity = issue.Severity,
         Metadata = issue.Metadata
      };
   }

   /// <summary>
   /// Creates a copy of an issue with a message.
   /// </summary>
   /// <param name="issue">The source issue.</param>
   /// <param name="message">The issue message.</param>
   /// <returns>A new issue with copied values and the specified message.</returns>
   public static IssueInfo WithMessage(
       IssueInfo issue,
       string message)
   {
      ArgumentNullException.ThrowIfNull(issue);
      ArgumentException.ThrowIfNullOrWhiteSpace(message);

      return new IssueInfo
      {
         Code = issue.Code,
         Number = issue.Number,
         Message = message,
         Details = issue.Details,
         Severity = issue.Severity,
         Metadata = issue.Metadata
      };
   }

   /// <summary>
   /// Creates a copy of an issue with a severity.
   /// </summary>
   /// <param name="issue">The source issue.</param>
   /// <param name="severity">The issue severity.</param>
   /// <returns>A new issue with copied values and the specified severity.</returns>
   public static IssueInfo WithSeverity(
       IssueInfo issue,
       IssueSeverity severity)
   {
      ArgumentNullException.ThrowIfNull(issue);

      return new IssueInfo
      {
         Code = issue.Code,
         Number = issue.Number,
         Message = issue.Message,
         Details = issue.Details,
         Severity = severity,
         Metadata = issue.Metadata
      };
   }

   /// <summary>
   /// Creates a copy of an issue with a stable issue code.
   /// </summary>
   /// <param name="issue">The source issue.</param>
   /// <param name="code">The stable issue code.</param>
   /// <returns>A new issue with copied values and the specified issue code.</returns>
   public static IssueInfo WithCode(
       IssueInfo issue,
       string code)
   {
      ArgumentNullException.ThrowIfNull(issue);
      ArgumentException.ThrowIfNullOrWhiteSpace(code);

      return new IssueInfo
      {
         Code = code,
         Number = issue.Number,
         Message = issue.Message,
         Details = issue.Details,
         Severity = issue.Severity,
         Metadata = issue.Metadata
      };
   }
}