using Afrowave.Toolbox.Essentials.Diagnostics;

namespace Afrowave.Toolbox.Essentials.Tests.Diagnostics;

public sealed class DiagnosticSpanFactoryTests
{
   [Fact]
   public void Empty_ReturnsEmptyDiagnosticSpan()
   {
      var span = DiagnosticSpanFactory.Empty();

      Assert.NotNull(span);
      Assert.NotNull(span.Start);
      Assert.Null(span.End);
      Assert.Null(span.Label);
   }

   [Fact]
   public void Empty_ReturnsNewInstanceEachTime()
   {
      var first = DiagnosticSpanFactory.Empty();
      var second = DiagnosticSpanFactory.Empty();

      Assert.NotSame(first, second);
   }

   [Fact]
   public void FromStart_WhenStartIsNull_ThrowsArgumentNullException()
   {
      DiagnosticLocation? start = null;

      Assert.Throws<ArgumentNullException>(() =>
          DiagnosticSpanFactory.FromStart(start!));
   }

   [Fact]
   public void FromStart_CreatesSpanWithStart()
   {
      var start = DiagnosticLocationFactory.FromFullLocation("input.ajis", 1, 2, 3);

      var span = DiagnosticSpanFactory.FromStart(start);

      Assert.Same(start, span.Start);
      Assert.Null(span.End);
      Assert.Null(span.Label);
   }

   [Fact]
   public void FromStartEnd_WhenStartIsNull_ThrowsArgumentNullException()
   {
      DiagnosticLocation? start = null;
      var end = DiagnosticLocationFactory.FromLineColumn(3, 4);

      Assert.Throws<ArgumentNullException>(() =>
          DiagnosticSpanFactory.FromStartEnd(start!, end));
   }

   [Fact]
   public void FromStartEnd_WhenEndIsNull_ThrowsArgumentNullException()
   {
      var start = DiagnosticLocationFactory.FromLineColumn(1, 2);
      DiagnosticLocation? end = null;

      Assert.Throws<ArgumentNullException>(() =>
          DiagnosticSpanFactory.FromStartEnd(start, end!));
   }

   [Fact]
   public void FromStartEnd_CreatesSpanWithStartAndEnd()
   {
      var start = DiagnosticLocationFactory.FromLineColumn(1, 2);
      var end = DiagnosticLocationFactory.FromLineColumn(3, 4);

      var span = DiagnosticSpanFactory.FromStartEnd(start, end);

      Assert.Same(start, span.Start);
      Assert.Same(end, span.End);
      Assert.Null(span.Label);
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void FromLabel_WhenLabelIsInvalid_ThrowsArgumentException(string? label)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
          DiagnosticSpanFactory.FromLabel(label!));
   }

   [Fact]
   public void FromLabel_CreatesSpanWithLabel()
   {
      var span = DiagnosticSpanFactory.FromLabel("problem area");

      Assert.NotNull(span.Start);
      Assert.Null(span.End);
      Assert.Equal("problem area", span.Label);
   }

   [Fact]
   public void FromStartLabel_WhenStartIsNull_ThrowsArgumentNullException()
   {
      DiagnosticLocation? start = null;

      Assert.Throws<ArgumentNullException>(() =>
          DiagnosticSpanFactory.FromStartLabel(start!, "label"));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void FromStartLabel_WhenLabelIsInvalid_ThrowsArgumentException(string? label)
   {
      var start = DiagnosticLocationFactory.FromLineColumn(1, 2);

      Assert.ThrowsAny<ArgumentException>(() =>
          DiagnosticSpanFactory.FromStartLabel(start, label!));
   }

   [Fact]
   public void FromStartLabel_CreatesSpanWithStartAndLabel()
   {
      var start = DiagnosticLocationFactory.FromSourceLineColumn("input.ajis", 1, 2);

      var span = DiagnosticSpanFactory.FromStartLabel(start, "highlight");

      Assert.Same(start, span.Start);
      Assert.Null(span.End);
      Assert.Equal("highlight", span.Label);
   }

   [Fact]
   public void FromStartEndLabel_WhenStartIsNull_ThrowsArgumentNullException()
   {
      DiagnosticLocation? start = null;
      var end = DiagnosticLocationFactory.FromLineColumn(3, 4);

      Assert.Throws<ArgumentNullException>(() =>
          DiagnosticSpanFactory.FromStartEndLabel(start!, end, "label"));
   }

   [Fact]
   public void FromStartEndLabel_WhenEndIsNull_ThrowsArgumentNullException()
   {
      var start = DiagnosticLocationFactory.FromLineColumn(1, 2);
      DiagnosticLocation? end = null;

      Assert.Throws<ArgumentNullException>(() =>
          DiagnosticSpanFactory.FromStartEndLabel(start, end!, "label"));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void FromStartEndLabel_WhenLabelIsInvalid_ThrowsArgumentException(string? label)
   {
      var start = DiagnosticLocationFactory.FromLineColumn(1, 2);
      var end = DiagnosticLocationFactory.FromLineColumn(3, 4);

      Assert.ThrowsAny<ArgumentException>(() =>
          DiagnosticSpanFactory.FromStartEndLabel(start, end, label!));
   }

   [Fact]
   public void FromStartEndLabel_CreatesSpanWithStartEndAndLabel()
   {
      var start = DiagnosticLocationFactory.FromLineColumn(1, 2);
      var end = DiagnosticLocationFactory.FromLineColumn(3, 4);

      var span = DiagnosticSpanFactory.FromStartEndLabel(start, end, "highlight");

      Assert.Same(start, span.Start);
      Assert.Same(end, span.End);
      Assert.Equal("highlight", span.Label);
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void FromSourceRange_WhenSourceIsInvalid_ThrowsArgumentException(string? source)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
          DiagnosticSpanFactory.FromSourceRange(source!, 1, 2, 3, 4));
   }

   [Theory]
   [InlineData("input.ajis", 1, 2, 3, 4)]
   [InlineData("config.json", 0, 0, 0, 0)]
   [InlineData("source.md", -1, -2, -3, -4)]
   public void FromSourceRange_CreatesSpanWithSourceRange(
       string source,
       int startLine,
       int startColumn,
       int endLine,
       int endColumn)
   {
      var span = DiagnosticSpanFactory.FromSourceRange(
          source,
          startLine,
          startColumn,
          endLine,
          endColumn);

      Assert.Equal(source, span.Start.Source);
      Assert.Equal(startLine, span.Start.Line);
      Assert.Equal(startColumn, span.Start.Column);
      Assert.Null(span.Start.Offset);

      Assert.NotNull(span.End);
      Assert.Equal(source, span.End.Source);
      Assert.Equal(endLine, span.End.Line);
      Assert.Equal(endColumn, span.End.Column);
      Assert.Null(span.End.Offset);

      Assert.Null(span.Label);
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void FromSourceRangeLabel_WhenSourceIsInvalid_ThrowsArgumentException(string? source)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
          DiagnosticSpanFactory.FromSourceRangeLabel(source!, 1, 2, 3, 4, "label"));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void FromSourceRangeLabel_WhenLabelIsInvalid_ThrowsArgumentException(string? label)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
          DiagnosticSpanFactory.FromSourceRangeLabel("input.ajis", 1, 2, 3, 4, label!));
   }

   [Theory]
   [InlineData("input.ajis", 1, 2, 3, 4, "problem")]
   [InlineData("config.json", 0, 0, 0, 0, "zero range")]
   [InlineData("source.md", -1, -2, -3, -4, "relative range")]
   public void FromSourceRangeLabel_CreatesSpanWithSourceRangeAndLabel(
       string source,
       int startLine,
       int startColumn,
       int endLine,
       int endColumn,
       string label)
   {
      var span = DiagnosticSpanFactory.FromSourceRangeLabel(
          source,
          startLine,
          startColumn,
          endLine,
          endColumn,
          label);

      Assert.Equal(source, span.Start.Source);
      Assert.Equal(startLine, span.Start.Line);
      Assert.Equal(startColumn, span.Start.Column);

      Assert.NotNull(span.End);
      Assert.Equal(source, span.End.Source);
      Assert.Equal(endLine, span.End.Line);
      Assert.Equal(endColumn, span.End.Column);

      Assert.Equal(label, span.Label);
   }

   [Theory]
   [InlineData(0L, 10L)]
   [InlineData(128L, 256L)]
   [InlineData(-10L, 10L)]
   public void FromOffsetRange_CreatesSpanWithOffsetRange(long startOffset, long endOffset)
   {
      var span = DiagnosticSpanFactory.FromOffsetRange(startOffset, endOffset);

      Assert.Null(span.Start.Source);
      Assert.Null(span.Start.Line);
      Assert.Null(span.Start.Column);
      Assert.Equal(startOffset, span.Start.Offset);

      Assert.NotNull(span.End);
      Assert.Null(span.End.Source);
      Assert.Null(span.End.Line);
      Assert.Null(span.End.Column);
      Assert.Equal(endOffset, span.End.Offset);

      Assert.Null(span.Label);
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void FromOffsetRangeLabel_WhenLabelIsInvalid_ThrowsArgumentException(string? label)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
          DiagnosticSpanFactory.FromOffsetRangeLabel(0, 10, label!));
   }

   [Theory]
   [InlineData(0L, 10L, "offset range")]
   [InlineData(128L, 256L, "payload")]
   [InlineData(-10L, 10L, "relative range")]
   public void FromOffsetRangeLabel_CreatesSpanWithOffsetRangeAndLabel(
       long startOffset,
       long endOffset,
       string label)
   {
      var span = DiagnosticSpanFactory.FromOffsetRangeLabel(
          startOffset,
          endOffset,
          label);

      Assert.Equal(startOffset, span.Start.Offset);

      Assert.NotNull(span.End);
      Assert.Equal(endOffset, span.End.Offset);

      Assert.Equal(label, span.Label);
   }
}