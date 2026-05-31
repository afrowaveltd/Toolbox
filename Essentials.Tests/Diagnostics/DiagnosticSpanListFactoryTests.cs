using Afrowave.Toolbox.Essentials.Diagnostics;

namespace Afrowave.Toolbox.Essentials.Tests.Diagnostics;

public sealed class DiagnosticSpanListFactoryTests
{
   [Fact]
   public void Empty_ReturnsEmptySpanList()
   {
      var spans = DiagnosticSpanListFactory.Empty();

      Assert.NotNull(spans);
      Assert.Empty(spans);
   }

   [Fact]
   public void Empty_ReturnsNewEmptyListEachTime()
   {
      var first = DiagnosticSpanListFactory.Empty();
      var second = DiagnosticSpanListFactory.Empty();

      Assert.NotSame(first, second);
      Assert.Empty(first);
      Assert.Empty(second);
   }

   [Fact]
   public void From_WithParams_WhenSpansIsNull_ThrowsArgumentNullException()
   {
      DiagnosticSpan[]? spans = null;

      Assert.Throws<ArgumentNullException>(() =>
          DiagnosticSpanListFactory.From(spans!));
   }

   [Fact]
   public void From_WithParams_WhenSpansAreEmpty_ReturnsEmptySpanList()
   {
      var spans = DiagnosticSpanListFactory.From([]);

      Assert.NotNull(spans);
      Assert.Empty(spans);
   }

   [Fact]
   public void From_WithParams_CreatesSpanList()
   {
      var first = DiagnosticSpanFactory.FromLabel("first");
      var second = DiagnosticSpanFactory.FromLabel("second");

      var spans = DiagnosticSpanListFactory.From(first, second);

      Assert.Equal(2, spans.Count);
      Assert.Same(first, spans[0]);
      Assert.Same(second, spans[1]);
   }

   [Fact]
   public void From_WithParams_ReturnsSnapshot()
   {
      var first = DiagnosticSpanFactory.FromLabel("first");
      var second = DiagnosticSpanFactory.FromLabel("second");

      var source = new[]
      {
            first,
            second
        };

      var spans = DiagnosticSpanListFactory.From(source);

      source[0] = DiagnosticSpanFactory.FromLabel("changed");

      Assert.Equal(2, spans.Count);
      Assert.Same(first, spans[0]);
      Assert.Same(second, spans[1]);
   }

   [Fact]
   public void From_WithEnumerable_WhenSpansIsNull_ThrowsArgumentNullException()
   {
      IEnumerable<DiagnosticSpan>? spans = null;

      Assert.Throws<ArgumentNullException>(() =>
          DiagnosticSpanListFactory.From(spans!));
   }

   [Fact]
   public void From_WithEnumerable_WhenSpansAreEmpty_ReturnsEmptySpanList()
   {
      IEnumerable<DiagnosticSpan> source = [];

      var spans = DiagnosticSpanListFactory.From(source);

      Assert.NotNull(spans);
      Assert.Empty(spans);
   }

   [Fact]
   public void From_WithEnumerable_CreatesSpanList()
   {
      var first = DiagnosticSpanFactory.FromLabel("first");
      var second = DiagnosticSpanFactory.FromLabel("second");

      IEnumerable<DiagnosticSpan> source =
      [
          first,
            second
      ];

      var spans = DiagnosticSpanListFactory.From(source);

      Assert.Equal(2, spans.Count);
      Assert.Same(first, spans[0]);
      Assert.Same(second, spans[1]);
   }

   [Fact]
   public void From_WithEnumerable_ReturnsSnapshot()
   {
      var first = DiagnosticSpanFactory.FromLabel("first");
      var second = DiagnosticSpanFactory.FromLabel("second");

      var source = new List<DiagnosticSpan>
        {
            first,
            second
        };

      var spans = DiagnosticSpanListFactory.From((IEnumerable<DiagnosticSpan>)source);

      source.Clear();

      Assert.Equal(2, spans.Count);
      Assert.Same(first, spans[0]);
      Assert.Same(second, spans[1]);
   }

   [Fact]
   public void FromStart_CreatesListWithOneSpan()
   {
      var start = DiagnosticLocationFactory.FromLineColumn(1, 2);

      var spans = DiagnosticSpanListFactory.FromStart(start);

      Assert.Single(spans);
      Assert.Same(start, spans[0].Start);
      Assert.Null(spans[0].End);
      Assert.Null(spans[0].Label);
   }

   [Fact]
   public void FromStartEnd_CreatesListWithOneSpan()
   {
      var start = DiagnosticLocationFactory.FromLineColumn(1, 2);
      var end = DiagnosticLocationFactory.FromLineColumn(3, 4);

      var spans = DiagnosticSpanListFactory.FromStartEnd(start, end);

      Assert.Single(spans);
      Assert.Same(start, spans[0].Start);
      Assert.Same(end, spans[0].End);
      Assert.Null(spans[0].Label);
   }

   [Fact]
   public void FromLabel_CreatesListWithOneLabeledSpan()
   {
      var spans = DiagnosticSpanListFactory.FromLabel("problem");

      Assert.Single(spans);
      Assert.Equal("problem", spans[0].Label);
   }

   [Fact]
   public void FromSourceRange_CreatesListWithOneSourceRangeSpan()
   {
      var spans = DiagnosticSpanListFactory.FromSourceRange(
          "input.ajis",
          1,
          2,
          3,
          4);

      Assert.Single(spans);

      var span = spans[0];

      Assert.Equal("input.ajis", span.Start.Source);
      Assert.Equal(1, span.Start.Line);
      Assert.Equal(2, span.Start.Column);

      Assert.NotNull(span.End);
      Assert.Equal("input.ajis", span.End.Source);
      Assert.Equal(3, span.End.Line);
      Assert.Equal(4, span.End.Column);

      Assert.Null(span.Label);
   }

   [Fact]
   public void FromSourceRangeLabel_CreatesListWithOneSourceRangeLabeledSpan()
   {
      var spans = DiagnosticSpanListFactory.FromSourceRangeLabel(
          "input.ajis",
          1,
          2,
          3,
          4,
          "unexpected token");

      Assert.Single(spans);

      var span = spans[0];

      Assert.Equal("input.ajis", span.Start.Source);
      Assert.Equal(1, span.Start.Line);
      Assert.Equal(2, span.Start.Column);

      Assert.NotNull(span.End);
      Assert.Equal("input.ajis", span.End.Source);
      Assert.Equal(3, span.End.Line);
      Assert.Equal(4, span.End.Column);

      Assert.Equal("unexpected token", span.Label);
   }

   [Fact]
   public void FromOffsetRange_CreatesListWithOneOffsetRangeSpan()
   {
      var spans = DiagnosticSpanListFactory.FromOffsetRange(10, 20);

      Assert.Single(spans);

      var span = spans[0];

      Assert.Equal(10, span.Start.Offset);
      Assert.NotNull(span.End);
      Assert.Equal(20, span.End.Offset);
      Assert.Null(span.Label);
   }

   [Fact]
   public void FromOffsetRangeLabel_CreatesListWithOneOffsetRangeLabeledSpan()
   {
      var spans = DiagnosticSpanListFactory.FromOffsetRangeLabel(
          10,
          20,
          "payload");

      Assert.Single(spans);

      var span = spans[0];

      Assert.Equal(10, span.Start.Offset);
      Assert.NotNull(span.End);
      Assert.Equal(20, span.End.Offset);
      Assert.Equal("payload", span.Label);
   }
}