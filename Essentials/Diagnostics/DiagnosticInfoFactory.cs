using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Metadata;

namespace Afrowave.Toolbox.Essentials.Diagnostics;

/// <summary>
/// Provides factory methods for creating <see cref="DiagnosticInfo"/> instances.
/// </summary>
public static class DiagnosticInfoFactory
{
   /// <summary>
   /// Creates a diagnostic message with the specified severity.
   /// </summary>
   /// <param name="code">The stable diagnostic code.</param>
   /// <param name="message">The diagnostic message.</param>
   /// <param name="severity">The diagnostic severity.</param>
   /// <returns>The created diagnostic message.</returns>
   public static DiagnosticInfo Create(
       string code,
       string message,
       IssueSeverity severity)
   {
      ArgumentException.ThrowIfNullOrWhiteSpace(code);
      ArgumentException.ThrowIfNullOrWhiteSpace(message);

      return new DiagnosticInfo
      {
         Code = code,
         Message = message,
         Severity = severity
      };
   }

   /// <summary>
   /// Creates a diagnostic message with optional details.
   /// </summary>
   /// <param name="code">The stable diagnostic code.</param>
   /// <param name="message">The diagnostic message.</param>
   /// <param name="details">Optional detailed diagnostic information.</param>
   /// <param name="severity">The diagnostic severity.</param>
   /// <returns>The created diagnostic message.</returns>
   public static DiagnosticInfo Create(
       string code,
       string message,
       string? details,
       IssueSeverity severity)
   {
      ArgumentException.ThrowIfNullOrWhiteSpace(code);
      ArgumentException.ThrowIfNullOrWhiteSpace(message);

      return new DiagnosticInfo
      {
         Code = code,
         Message = message,
         Details = details,
         Severity = severity
      };
   }

   /// <summary>
   /// Creates an informational diagnostic message.
   /// </summary>
   /// <param name="code">The stable diagnostic code.</param>
   /// <param name="message">The diagnostic message.</param>
   /// <returns>The created diagnostic message.</returns>
   public static DiagnosticInfo Information(
       string code,
       string message)
   {
      return Create(code, message, IssueSeverity.Information);
   }

   /// <summary>
   /// Creates a warning diagnostic message.
   /// </summary>
   /// <param name="code">The stable diagnostic code.</param>
   /// <param name="message">The diagnostic message.</param>
   /// <returns>The created diagnostic message.</returns>
   public static DiagnosticInfo Warning(
       string code,
       string message)
   {
      return Create(code, message, IssueSeverity.Warning);
   }

   /// <summary>
   /// Creates an error diagnostic message.
   /// </summary>
   /// <param name="code">The stable diagnostic code.</param>
   /// <param name="message">The diagnostic message.</param>
   /// <returns>The created diagnostic message.</returns>
   public static DiagnosticInfo Error(
       string code,
       string message)
   {
      return Create(code, message, IssueSeverity.Error);
   }

   /// <summary>
   /// Creates a critical diagnostic message.
   /// </summary>
   /// <param name="code">The stable diagnostic code.</param>
   /// <param name="message">The diagnostic message.</param>
   /// <returns>The created diagnostic message.</returns>
   public static DiagnosticInfo Critical(
       string code,
       string message)
   {
      return Create(code, message, IssueSeverity.Critical);
   }

   /// <summary>
   /// Creates a fatal diagnostic message.
   /// </summary>
   /// <param name="code">The stable diagnostic code.</param>
   /// <param name="message">The diagnostic message.</param>
   /// <returns>The created diagnostic message.</returns>
   public static DiagnosticInfo Fatal(
       string code,
       string message)
   {
      return Create(code, message, IssueSeverity.Fatal);
   }

   /// <summary>
   /// Creates a copy of a diagnostic message with a location.
   /// </summary>
   /// <param name="diagnostic">The source diagnostic message.</param>
   /// <param name="location">The diagnostic location.</param>
   /// <returns>A new diagnostic message with copied values and the specified location.</returns>
   public static DiagnosticInfo WithLocation(
       DiagnosticInfo diagnostic,
       DiagnosticLocation location)
   {
      ArgumentNullException.ThrowIfNull(diagnostic);
      ArgumentNullException.ThrowIfNull(location);

      return new DiagnosticInfo
      {
         Code = diagnostic.Code,
         Message = diagnostic.Message,
         Details = diagnostic.Details,
         Severity = diagnostic.Severity,
         Location = location,
         Spans = diagnostic.Spans,
         Hints = diagnostic.Hints,
         Metadata = diagnostic.Metadata
      };
   }

   /// <summary>
   /// Creates a copy of a diagnostic message with spans.
   /// </summary>
   /// <param name="diagnostic">The source diagnostic message.</param>
   /// <param name="spans">The diagnostic spans.</param>
   /// <returns>A new diagnostic message with copied values and the specified spans.</returns>
   public static DiagnosticInfo WithSpans(
       DiagnosticInfo diagnostic,
       IReadOnlyList<DiagnosticSpan> spans)
   {
      ArgumentNullException.ThrowIfNull(diagnostic);
      ArgumentNullException.ThrowIfNull(spans);

      return new DiagnosticInfo
      {
         Code = diagnostic.Code,
         Message = diagnostic.Message,
         Details = diagnostic.Details,
         Severity = diagnostic.Severity,
         Location = diagnostic.Location,
         Spans = spans,
         Hints = diagnostic.Hints,
         Metadata = diagnostic.Metadata
      };
   }

   /// <summary>
   /// Creates a copy of a diagnostic message with hints.
   /// </summary>
   /// <param name="diagnostic">The source diagnostic message.</param>
   /// <param name="hints">The diagnostic hints.</param>
   /// <returns>A new diagnostic message with copied values and the specified hints.</returns>
   public static DiagnosticInfo WithHints(
       DiagnosticInfo diagnostic,
       IReadOnlyList<DiagnosticHint> hints)
   {
      ArgumentNullException.ThrowIfNull(diagnostic);
      ArgumentNullException.ThrowIfNull(hints);

      return new DiagnosticInfo
      {
         Code = diagnostic.Code,
         Message = diagnostic.Message,
         Details = diagnostic.Details,
         Severity = diagnostic.Severity,
         Location = diagnostic.Location,
         Spans = diagnostic.Spans,
         Hints = hints,
         Metadata = diagnostic.Metadata
      };
   }

   /// <summary>
   /// Creates a copy of a diagnostic message with metadata.
   /// </summary>
   /// <param name="diagnostic">The source diagnostic message.</param>
   /// <param name="metadata">The metadata to attach.</param>
   /// <returns>A new diagnostic message with copied values and the specified metadata.</returns>
   public static DiagnosticInfo WithMetadata(
       DiagnosticInfo diagnostic,
       MetadataBag metadata)
   {
      ArgumentNullException.ThrowIfNull(diagnostic);
      ArgumentNullException.ThrowIfNull(metadata);

      return new DiagnosticInfo
      {
         Code = diagnostic.Code,
         Message = diagnostic.Message,
         Details = diagnostic.Details,
         Severity = diagnostic.Severity,
         Location = diagnostic.Location,
         Spans = diagnostic.Spans,
         Hints = diagnostic.Hints,
         Metadata = metadata
      };
   }

   /// <summary>
   /// Creates a copy of a diagnostic message with a new message.
   /// </summary>
   /// <param name="diagnostic">The source diagnostic message.</param>
   /// <param name="message">The diagnostic message.</param>
   /// <returns>A new diagnostic message with copied values and the specified message.</returns>
   public static DiagnosticInfo WithMessage(
       DiagnosticInfo diagnostic,
       string message)
   {
      ArgumentNullException.ThrowIfNull(diagnostic);
      ArgumentException.ThrowIfNullOrWhiteSpace(message);

      return new DiagnosticInfo
      {
         Code = diagnostic.Code,
         Message = message,
         Details = diagnostic.Details,
         Severity = diagnostic.Severity,
         Location = diagnostic.Location,
         Spans = diagnostic.Spans,
         Hints = diagnostic.Hints,
         Metadata = diagnostic.Metadata
      };
   }

   /// <summary>
   /// Creates a copy of a diagnostic message with detailed information.
   /// </summary>
   /// <param name="diagnostic">The source diagnostic message.</param>
   /// <param name="details">The detailed diagnostic information.</param>
   /// <returns>A new diagnostic message with copied values and the specified details.</returns>
   public static DiagnosticInfo WithDetails(
       DiagnosticInfo diagnostic,
       string? details)
   {
      ArgumentNullException.ThrowIfNull(diagnostic);

      return new DiagnosticInfo
      {
         Code = diagnostic.Code,
         Message = diagnostic.Message,
         Details = details,
         Severity = diagnostic.Severity,
         Location = diagnostic.Location,
         Spans = diagnostic.Spans,
         Hints = diagnostic.Hints,
         Metadata = diagnostic.Metadata
      };
   }

   /// <summary>
   /// Creates a copy of a diagnostic message with a severity.
   /// </summary>
   /// <param name="diagnostic">The source diagnostic message.</param>
   /// <param name="severity">The diagnostic severity.</param>
   /// <returns>A new diagnostic message with copied values and the specified severity.</returns>
   public static DiagnosticInfo WithSeverity(
       DiagnosticInfo diagnostic,
       IssueSeverity severity)
   {
      ArgumentNullException.ThrowIfNull(diagnostic);

      return new DiagnosticInfo
      {
         Code = diagnostic.Code,
         Message = diagnostic.Message,
         Details = diagnostic.Details,
         Severity = severity,
         Location = diagnostic.Location,
         Spans = diagnostic.Spans,
         Hints = diagnostic.Hints,
         Metadata = diagnostic.Metadata
      };
   }

   /// <summary>
   /// Creates a copy of a diagnostic message with a stable diagnostic code.
   /// </summary>
   /// <param name="diagnostic">The source diagnostic message.</param>
   /// <param name="code">The stable diagnostic code.</param>
   /// <returns>A new diagnostic message with copied values and the specified diagnostic code.</returns>
   public static DiagnosticInfo WithCode(
       DiagnosticInfo diagnostic,
       string code)
   {
      ArgumentNullException.ThrowIfNull(diagnostic);
      ArgumentException.ThrowIfNullOrWhiteSpace(code);

      return new DiagnosticInfo
      {
         Code = code,
         Message = diagnostic.Message,
         Details = diagnostic.Details,
         Severity = diagnostic.Severity,
         Location = diagnostic.Location,
         Spans = diagnostic.Spans,
         Hints = diagnostic.Hints,
         Metadata = diagnostic.Metadata
      };
   }
}