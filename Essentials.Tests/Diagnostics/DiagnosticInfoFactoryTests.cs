using Afrowave.Toolbox.Essentials.Diagnostics;
using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Metadata;

namespace Afrowave.Toolbox.Essentials.Tests.Diagnostics;

public sealed class DiagnosticInfoFactoryTests
{
   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Create_WhenCodeIsInvalid_ThrowsArgumentException(string? code)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
          DiagnosticInfoFactory.Create(code!, "Message.", IssueSeverity.Warning));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Create_WhenMessageIsInvalid_ThrowsArgumentException(string? message)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
          DiagnosticInfoFactory.Create("AFW001", message!, IssueSeverity.Warning));
   }

   [Theory]
   [InlineData(IssueSeverity.None)]
   [InlineData(IssueSeverity.Trace)]
   [InlineData(IssueSeverity.Debug)]
   [InlineData(IssueSeverity.Information)]
   [InlineData(IssueSeverity.Warning)]
   [InlineData(IssueSeverity.Error)]
   [InlineData(IssueSeverity.Critical)]
   [InlineData(IssueSeverity.Fatal)]
   public void Create_WithSeverity_CreatesDiagnostic(IssueSeverity severity)
   {
      var diagnostic = DiagnosticInfoFactory.Create(
          "AFW001",
          "Test diagnostic.",
          severity);

      Assert.Equal("AFW001", diagnostic.Code);
      Assert.Equal("Test diagnostic.", diagnostic.Message);
      Assert.Equal(severity, diagnostic.Severity);
      Assert.Null(diagnostic.Details);
      Assert.Null(diagnostic.Location);
      Assert.Empty(diagnostic.Spans);
      Assert.Empty(diagnostic.Hints);
      Assert.NotNull(diagnostic.Metadata);
      Assert.True(diagnostic.Metadata.IsEmpty);
   }

   [Fact]
   public void Create_WithDetails_CreatesDiagnosticWithDetails()
   {
      var diagnostic = DiagnosticInfoFactory.Create(
          "AFW001",
          "Test diagnostic.",
          "Detailed diagnostic information.",
          IssueSeverity.Warning);

      Assert.Equal("AFW001", diagnostic.Code);
      Assert.Equal("Test diagnostic.", diagnostic.Message);
      Assert.Equal("Detailed diagnostic information.", diagnostic.Details);
      Assert.Equal(IssueSeverity.Warning, diagnostic.Severity);
      Assert.Null(diagnostic.Location);
      Assert.Empty(diagnostic.Spans);
      Assert.Empty(diagnostic.Hints);
      Assert.NotNull(diagnostic.Metadata);
      Assert.True(diagnostic.Metadata.IsEmpty);
   }

   [Fact]
   public void Create_WithDetails_AllowsNullDetails()
   {
      var diagnostic = DiagnosticInfoFactory.Create(
          "AFW001",
          "Test diagnostic.",
          null,
          IssueSeverity.Warning);

      Assert.Equal("AFW001", diagnostic.Code);
      Assert.Equal("Test diagnostic.", diagnostic.Message);
      Assert.Null(diagnostic.Details);
      Assert.Equal(IssueSeverity.Warning, diagnostic.Severity);
   }

   [Fact]
   public void Information_CreatesInformationDiagnostic()
   {
      var diagnostic = DiagnosticInfoFactory.Information(
          "AFW_INFO",
          "Information message.");

      Assert.Equal("AFW_INFO", diagnostic.Code);
      Assert.Equal("Information message.", diagnostic.Message);
      Assert.Equal(IssueSeverity.Information, diagnostic.Severity);
   }

   [Fact]
   public void Warning_CreatesWarningDiagnostic()
   {
      var diagnostic = DiagnosticInfoFactory.Warning(
          "AFW_WARNING",
          "Warning message.");

      Assert.Equal("AFW_WARNING", diagnostic.Code);
      Assert.Equal("Warning message.", diagnostic.Message);
      Assert.Equal(IssueSeverity.Warning, diagnostic.Severity);
   }

   [Fact]
   public void Error_CreatesErrorDiagnostic()
   {
      var diagnostic = DiagnosticInfoFactory.Error(
          "AFW_ERROR",
          "Error message.");

      Assert.Equal("AFW_ERROR", diagnostic.Code);
      Assert.Equal("Error message.", diagnostic.Message);
      Assert.Equal(IssueSeverity.Error, diagnostic.Severity);
   }

   [Fact]
   public void Critical_CreatesCriticalDiagnostic()
   {
      var diagnostic = DiagnosticInfoFactory.Critical(
          "AFW_CRITICAL",
          "Critical message.");

      Assert.Equal("AFW_CRITICAL", diagnostic.Code);
      Assert.Equal("Critical message.", diagnostic.Message);
      Assert.Equal(IssueSeverity.Critical, diagnostic.Severity);
   }

   [Fact]
   public void Fatal_CreatesFatalDiagnostic()
   {
      var diagnostic = DiagnosticInfoFactory.Fatal(
          "AFW_FATAL",
          "Fatal message.");

      Assert.Equal("AFW_FATAL", diagnostic.Code);
      Assert.Equal("Fatal message.", diagnostic.Message);
      Assert.Equal(IssueSeverity.Fatal, diagnostic.Severity);
   }

   [Fact]
   public void WithLocation_WhenDiagnosticIsNull_ThrowsArgumentNullException()
   {
      DiagnosticInfo? diagnostic = null;
      var location = new DiagnosticLocation();

      Assert.Throws<ArgumentNullException>(() =>
          DiagnosticInfoFactory.WithLocation(diagnostic!, location));
   }

   [Fact]
   public void WithLocation_WhenLocationIsNull_ThrowsArgumentNullException()
   {
      var diagnostic = DiagnosticInfoFactory.Warning(
          "AFW_WARNING",
          "Warning message.");

      DiagnosticLocation? location = null;

      Assert.Throws<ArgumentNullException>(() =>
          DiagnosticInfoFactory.WithLocation(diagnostic, location!));
   }

   [Fact]
   public void WithLocation_CreatesCopyWithSpecifiedLocation()
   {
      var diagnostic = CreateFullDiagnostic();

      var location = DiagnosticLocationFactory.FromFullLocation(
          "input.ajis",
          10,
          5,
          128);

      var copy = DiagnosticInfoFactory.WithLocation(diagnostic, location);

      Assert.NotSame(diagnostic, copy);
      AssertCommonValues(diagnostic, copy);

      Assert.Same(location, copy.Location);
      Assert.Same(diagnostic.Spans, copy.Spans);
      Assert.Same(diagnostic.Hints, copy.Hints);
      Assert.Same(diagnostic.Metadata, copy.Metadata);
   }

   [Fact]
   public void WithSpans_WhenDiagnosticIsNull_ThrowsArgumentNullException()
   {
      DiagnosticInfo? diagnostic = null;
      IReadOnlyList<DiagnosticSpan> spans = [];

      Assert.Throws<ArgumentNullException>(() =>
          DiagnosticInfoFactory.WithSpans(diagnostic!, spans));
   }

   [Fact]
   public void WithSpans_WhenSpansIsNull_ThrowsArgumentNullException()
   {
      var diagnostic = DiagnosticInfoFactory.Warning(
          "AFW_WARNING",
          "Warning message.");

      IReadOnlyList<DiagnosticSpan>? spans = null;

      Assert.Throws<ArgumentNullException>(() =>
          DiagnosticInfoFactory.WithSpans(diagnostic, spans!));
   }

   [Fact]
   public void WithSpans_CreatesCopyWithSpecifiedSpans()
   {
      var diagnostic = CreateFullDiagnostic();

      IReadOnlyList<DiagnosticSpan> spans =
      [
          DiagnosticSpanFactory.FromLabel("first"),
            DiagnosticSpanFactory.FromLabel("second")
      ];

      var copy = DiagnosticInfoFactory.WithSpans(diagnostic, spans);

      Assert.NotSame(diagnostic, copy);
      AssertCommonValues(diagnostic, copy);

      Assert.Same(diagnostic.Location, copy.Location);
      Assert.Same(spans, copy.Spans);
      Assert.Same(diagnostic.Hints, copy.Hints);
      Assert.Same(diagnostic.Metadata, copy.Metadata);
   }

   [Fact]
   public void WithHints_WhenDiagnosticIsNull_ThrowsArgumentNullException()
   {
      DiagnosticInfo? diagnostic = null;
      IReadOnlyList<DiagnosticHint> hints = [];

      Assert.Throws<ArgumentNullException>(() =>
          DiagnosticInfoFactory.WithHints(diagnostic!, hints));
   }

   [Fact]
   public void WithHints_WhenHintsIsNull_ThrowsArgumentNullException()
   {
      var diagnostic = DiagnosticInfoFactory.Warning(
          "AFW_WARNING",
          "Warning message.");

      IReadOnlyList<DiagnosticHint>? hints = null;

      Assert.Throws<ArgumentNullException>(() =>
          DiagnosticInfoFactory.WithHints(diagnostic, hints!));
   }

   [Fact]
   public void WithHints_CreatesCopyWithSpecifiedHints()
   {
      var diagnostic = CreateFullDiagnostic();

      IReadOnlyList<DiagnosticHint> hints =
      [
          DiagnosticHintFactory.Help("Use a valid value."),
            DiagnosticHintFactory.Example("Example: true")
      ];

      var copy = DiagnosticInfoFactory.WithHints(diagnostic, hints);

      Assert.NotSame(diagnostic, copy);
      AssertCommonValues(diagnostic, copy);

      Assert.Same(diagnostic.Location, copy.Location);
      Assert.Same(diagnostic.Spans, copy.Spans);
      Assert.Same(hints, copy.Hints);
      Assert.Same(diagnostic.Metadata, copy.Metadata);
   }

   [Fact]
   public void WithMetadata_WhenDiagnosticIsNull_ThrowsArgumentNullException()
   {
      DiagnosticInfo? diagnostic = null;
      var metadata = new MetadataBag();

      Assert.Throws<ArgumentNullException>(() =>
          DiagnosticInfoFactory.WithMetadata(diagnostic!, metadata));
   }

   [Fact]
   public void WithMetadata_WhenMetadataIsNull_ThrowsArgumentNullException()
   {
      var diagnostic = DiagnosticInfoFactory.Warning(
          "AFW_WARNING",
          "Warning message.");

      MetadataBag? metadata = null;

      Assert.Throws<ArgumentNullException>(() =>
          DiagnosticInfoFactory.WithMetadata(diagnostic, metadata!));
   }

   [Fact]
   public void WithMetadata_CreatesCopyWithSpecifiedMetadata()
   {
      var diagnostic = CreateFullDiagnostic();

      var metadata = new MetadataBag();
      metadata.Set("provider", "ollama-local");
      metadata.Set("profile", "markdown-refine");

      var copy = DiagnosticInfoFactory.WithMetadata(diagnostic, metadata);

      Assert.NotSame(diagnostic, copy);
      AssertCommonValues(diagnostic, copy);

      Assert.Same(diagnostic.Location, copy.Location);
      Assert.Same(diagnostic.Spans, copy.Spans);
      Assert.Same(diagnostic.Hints, copy.Hints);
      Assert.Same(metadata, copy.Metadata);

      Assert.True(copy.Metadata.TryGet("provider", out var provider));
      Assert.True(copy.Metadata.TryGet("profile", out var profile));

      Assert.Equal("ollama-local", provider);
      Assert.Equal("markdown-refine", profile);
   }

   [Fact]
   public void WithMetadata_DoesNotModifyOriginalDiagnostic()
   {
      var originalMetadata = new MetadataBag();
      originalMetadata.Set("original", "yes");

      var diagnostic = new DiagnosticInfo
      {
         Code = "AFW001",
         Message = "Test diagnostic.",
         Severity = IssueSeverity.Warning,
         Metadata = originalMetadata
      };

      var newMetadata = new MetadataBag();
      newMetadata.Set("new", "yes");

      var copy = DiagnosticInfoFactory.WithMetadata(diagnostic, newMetadata);

      Assert.Same(originalMetadata, diagnostic.Metadata);
      Assert.Same(newMetadata, copy.Metadata);

      Assert.True(diagnostic.Metadata.TryGet("original", out var originalValue));
      Assert.False(diagnostic.Metadata.TryGet("new", out _));
      Assert.Equal("yes", originalValue);

      Assert.True(copy.Metadata.TryGet("new", out var newValue));
      Assert.False(copy.Metadata.TryGet("original", out _));
      Assert.Equal("yes", newValue);
   }

   [Fact]
   public void WithMessage_WhenDiagnosticIsNull_ThrowsArgumentNullException()
   {
      DiagnosticInfo? diagnostic = null;

      Assert.Throws<ArgumentNullException>(() =>
          DiagnosticInfoFactory.WithMessage(diagnostic!, "New message."));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void WithMessage_WhenMessageIsInvalid_ThrowsArgumentException(
       string? message)
   {
      var diagnostic = DiagnosticInfoFactory.Warning(
          "AFW_WARNING",
          "Warning message.");

      Assert.ThrowsAny<ArgumentException>(() =>
          DiagnosticInfoFactory.WithMessage(diagnostic, message!));
   }

   [Fact]
   public void WithMessage_CreatesCopyWithSpecifiedMessage()
   {
      var metadata = new MetadataBag();
      metadata.Set("source", "unit-test");

      var diagnostic = new DiagnosticInfo
      {
         Code = "AFW001",
         Message = "Original message.",
         Details = "Detailed information.",
         Severity = IssueSeverity.Warning,
         Location = DiagnosticLocationFactory.FromFullLocation(
              "input.ajis",
              1,
              2,
              3),
         Spans =
          [
              DiagnosticSpanFactory.FromLabel("span")
          ],
         Hints =
          [
              DiagnosticHintFactory.Help("Use a valid value.")
          ],
         Metadata = metadata
      };

      var copy = DiagnosticInfoFactory.WithMessage(
          diagnostic,
          "New message.");

      Assert.NotSame(diagnostic, copy);

      Assert.Equal(diagnostic.Code, copy.Code);
      Assert.Equal("New message.", copy.Message);
      Assert.Equal(diagnostic.Details, copy.Details);
      Assert.Equal(diagnostic.Severity, copy.Severity);
      Assert.Same(diagnostic.Location, copy.Location);
      Assert.Same(diagnostic.Spans, copy.Spans);
      Assert.Same(diagnostic.Hints, copy.Hints);
      Assert.Same(diagnostic.Metadata, copy.Metadata);
   }

   [Fact]
   public void WithMessage_DoesNotModifyOriginalDiagnostic()
   {
      var diagnostic = new DiagnosticInfo
      {
         Code = "AFW001",
         Message = "Original message.",
         Details = "Detailed information.",
         Severity = IssueSeverity.Warning
      };

      var copy = DiagnosticInfoFactory.WithMessage(
          diagnostic,
          "New message.");

      Assert.Equal("Original message.", diagnostic.Message);
      Assert.Equal("New message.", copy.Message);
   }

   [Fact]
   public void WithDetails_WhenDiagnosticIsNull_ThrowsArgumentNullException()
   {
      DiagnosticInfo? diagnostic = null;

      Assert.Throws<ArgumentNullException>(() =>
          DiagnosticInfoFactory.WithDetails(diagnostic!, "Detailed information."));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   [InlineData("Detailed information.")]
   [InlineData(" details ")]
   public void WithDetails_CreatesCopyWithSpecifiedDetails(
       string? details)
   {
      var metadata = new MetadataBag();
      metadata.Set("source", "unit-test");

      var diagnostic = new DiagnosticInfo
      {
         Code = "AFW001",
         Message = "Test diagnostic.",
         Details = "Original details.",
         Severity = IssueSeverity.Warning,
         Location = DiagnosticLocationFactory.FromFullLocation(
              "input.ajis",
              1,
              2,
              3),
         Spans =
          [
              DiagnosticSpanFactory.FromLabel("span")
          ],
         Hints =
          [
              DiagnosticHintFactory.Help("Use a valid value.")
          ],
         Metadata = metadata
      };

      var copy = DiagnosticInfoFactory.WithDetails(diagnostic, details);

      Assert.NotSame(diagnostic, copy);

      Assert.Equal(diagnostic.Code, copy.Code);
      Assert.Equal(diagnostic.Message, copy.Message);
      Assert.Equal(details, copy.Details);
      Assert.Equal(diagnostic.Severity, copy.Severity);
      Assert.Same(diagnostic.Location, copy.Location);
      Assert.Same(diagnostic.Spans, copy.Spans);
      Assert.Same(diagnostic.Hints, copy.Hints);
      Assert.Same(diagnostic.Metadata, copy.Metadata);
   }

   [Fact]
   public void WithDetails_DoesNotModifyOriginalDiagnostic()
   {
      var diagnostic = new DiagnosticInfo
      {
         Code = "AFW001",
         Message = "Test diagnostic.",
         Details = "Original details.",
         Severity = IssueSeverity.Warning
      };

      var copy = DiagnosticInfoFactory.WithDetails(
          diagnostic,
          "New details.");

      Assert.Equal("Original details.", diagnostic.Details);
      Assert.Equal("New details.", copy.Details);
   }

   [Fact]
   public void WithSeverity_WhenDiagnosticIsNull_ThrowsArgumentNullException()
   {
      DiagnosticInfo? diagnostic = null;

      Assert.Throws<ArgumentNullException>(() =>
          DiagnosticInfoFactory.WithSeverity(diagnostic!, IssueSeverity.Error));
   }

   [Theory]
   [InlineData(IssueSeverity.None)]
   [InlineData(IssueSeverity.Trace)]
   [InlineData(IssueSeverity.Debug)]
   [InlineData(IssueSeverity.Information)]
   [InlineData(IssueSeverity.Warning)]
   [InlineData(IssueSeverity.Error)]
   [InlineData(IssueSeverity.Critical)]
   [InlineData(IssueSeverity.Fatal)]
   public void WithSeverity_CreatesCopyWithSpecifiedSeverity(
       IssueSeverity severity)
   {
      var metadata = new MetadataBag();
      metadata.Set("source", "unit-test");

      var diagnostic = new DiagnosticInfo
      {
         Code = "AFW001",
         Message = "Test diagnostic.",
         Details = "Detailed information.",
         Severity = IssueSeverity.Warning,
         Location = DiagnosticLocationFactory.FromFullLocation(
              "input.ajis",
              1,
              2,
              3),
         Spans =
          [
              DiagnosticSpanFactory.FromLabel("span")
          ],
         Hints =
          [
              DiagnosticHintFactory.Help("Use a valid value.")
          ],
         Metadata = metadata
      };

      var copy = DiagnosticInfoFactory.WithSeverity(diagnostic, severity);

      Assert.NotSame(diagnostic, copy);

      Assert.Equal(diagnostic.Code, copy.Code);
      Assert.Equal(diagnostic.Message, copy.Message);
      Assert.Equal(diagnostic.Details, copy.Details);
      Assert.Equal(severity, copy.Severity);
      Assert.Same(diagnostic.Location, copy.Location);
      Assert.Same(diagnostic.Spans, copy.Spans);
      Assert.Same(diagnostic.Hints, copy.Hints);
      Assert.Same(diagnostic.Metadata, copy.Metadata);
   }

   [Fact]
   public void WithSeverity_DoesNotModifyOriginalDiagnostic()
   {
      var diagnostic = new DiagnosticInfo
      {
         Code = "AFW001",
         Message = "Test diagnostic.",
         Details = "Detailed information.",
         Severity = IssueSeverity.Warning
      };

      var copy = DiagnosticInfoFactory.WithSeverity(
          diagnostic,
          IssueSeverity.Error);

      Assert.Equal(IssueSeverity.Warning, diagnostic.Severity);
      Assert.Equal(IssueSeverity.Error, copy.Severity);
   }

   [Fact]
   public void WithCode_WhenDiagnosticIsNull_ThrowsArgumentNullException()
   {
      DiagnosticInfo? diagnostic = null;

      Assert.Throws<ArgumentNullException>(() =>
          DiagnosticInfoFactory.WithCode(diagnostic!, "AFW_NEW_CODE"));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void WithCode_WhenCodeIsInvalid_ThrowsArgumentException(
       string? code)
   {
      var diagnostic = DiagnosticInfoFactory.Warning(
          "AFW_WARNING",
          "Warning message.");

      Assert.ThrowsAny<ArgumentException>(() =>
          DiagnosticInfoFactory.WithCode(diagnostic, code!));
   }

   [Fact]
   public void WithCode_CreatesCopyWithSpecifiedCode()
   {
      var metadata = new MetadataBag();
      metadata.Set("source", "unit-test");

      var diagnostic = new DiagnosticInfo
      {
         Code = "AFW_OLD_CODE",
         Message = "Test diagnostic.",
         Details = "Detailed information.",
         Severity = IssueSeverity.Warning,
         Location = DiagnosticLocationFactory.FromFullLocation(
              "input.ajis",
              1,
              2,
              3),
         Spans =
          [
              DiagnosticSpanFactory.FromLabel("span")
          ],
         Hints =
          [
              DiagnosticHintFactory.Help("Use a valid value.")
          ],
         Metadata = metadata
      };

      var copy = DiagnosticInfoFactory.WithCode(
          diagnostic,
          "AFW_NEW_CODE");

      Assert.NotSame(diagnostic, copy);

      Assert.Equal("AFW_NEW_CODE", copy.Code);
      Assert.Equal(diagnostic.Message, copy.Message);
      Assert.Equal(diagnostic.Details, copy.Details);
      Assert.Equal(diagnostic.Severity, copy.Severity);
      Assert.Same(diagnostic.Location, copy.Location);
      Assert.Same(diagnostic.Spans, copy.Spans);
      Assert.Same(diagnostic.Hints, copy.Hints);
      Assert.Same(diagnostic.Metadata, copy.Metadata);
   }

   [Fact]
   public void WithCode_DoesNotModifyOriginalDiagnostic()
   {
      var diagnostic = new DiagnosticInfo
      {
         Code = "AFW_OLD_CODE",
         Message = "Test diagnostic.",
         Details = "Detailed information.",
         Severity = IssueSeverity.Warning
      };

      var copy = DiagnosticInfoFactory.WithCode(
          diagnostic,
          "AFW_NEW_CODE");

      Assert.Equal("AFW_OLD_CODE", diagnostic.Code);
      Assert.Equal("AFW_NEW_CODE", copy.Code);
   }

   private static DiagnosticInfo CreateFullDiagnostic()
   {
      var metadata = new MetadataBag();
      metadata.Set("source", "unit-test");

      return new DiagnosticInfo
      {
         Code = "AFW001",
         Message = "Test diagnostic.",
         Details = "Detailed diagnostic information.",
         Severity = IssueSeverity.Warning,
         Location = DiagnosticLocationFactory.FromFullLocation(
              "original.ajis",
              1,
              2,
              3),
         Spans =
          [
              DiagnosticSpanFactory.FromLabel("original span")
          ],
         Hints =
          [
              DiagnosticHintFactory.Note("Original hint.")
          ],
         Metadata = metadata
      };
   }

   private static void AssertCommonValues(
       DiagnosticInfo expected,
       DiagnosticInfo actual)
   {
      Assert.Equal(expected.Code, actual.Code);
      Assert.Equal(expected.Message, actual.Message);
      Assert.Equal(expected.Details, actual.Details);
      Assert.Equal(expected.Severity, actual.Severity);
   }
}