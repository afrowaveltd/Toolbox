using Afrowave.Toolbox.Essentials.Diagnostics;

namespace Afrowave.Toolbox.Essentials.Tests.Diagnostics;

public sealed class DiagnosticLocationFactoryTests
{
   [Fact]
   public void Empty_ReturnsEmptyDiagnosticLocation()
   {
      var location = DiagnosticLocationFactory.Empty();

      Assert.NotNull(location);
      Assert.Null(location.Source);
      Assert.Null(location.Line);
      Assert.Null(location.Column);
      Assert.Null(location.Offset);
   }

   [Fact]
   public void Empty_ReturnsNewInstanceEachTime()
   {
      var first = DiagnosticLocationFactory.Empty();
      var second = DiagnosticLocationFactory.Empty();

      Assert.NotSame(first, second);
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void FromSource_WhenSourceIsInvalid_ThrowsArgumentException(string? source)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
          DiagnosticLocationFactory.FromSource(source!));
   }

   [Fact]
   public void FromSource_CreatesLocationWithSource()
   {
      var location = DiagnosticLocationFactory.FromSource("input.ajis");

      Assert.Equal("input.ajis", location.Source);
      Assert.Null(location.Line);
      Assert.Null(location.Column);
      Assert.Null(location.Offset);
   }

   [Theory]
   [InlineData(1, 1)]
   [InlineData(10, 5)]
   [InlineData(0, 0)]
   [InlineData(-1, -2)]
   public void FromLineColumn_CreatesLocationWithLineAndColumn(int line, int column)
   {
      var location = DiagnosticLocationFactory.FromLineColumn(line, column);

      Assert.Null(location.Source);
      Assert.Equal(line, location.Line);
      Assert.Equal(column, location.Column);
      Assert.Null(location.Offset);
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void FromSourceLineColumn_WhenSourceIsInvalid_ThrowsArgumentException(string? source)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
          DiagnosticLocationFactory.FromSourceLineColumn(source!, 1, 1));
   }

   [Theory]
   [InlineData("input.ajis", 1, 1)]
   [InlineData("config.json", 10, 5)]
   [InlineData("source.md", 0, 0)]
   [InlineData("source.md", -1, -2)]
   public void FromSourceLineColumn_CreatesLocationWithSourceLineAndColumn(
       string source,
       int line,
       int column)
   {
      var location = DiagnosticLocationFactory.FromSourceLineColumn(
          source,
          line,
          column);

      Assert.Equal(source, location.Source);
      Assert.Equal(line, location.Line);
      Assert.Equal(column, location.Column);
      Assert.Null(location.Offset);
   }

   [Theory]
   [InlineData(0L)]
   [InlineData(1L)]
   [InlineData(128L)]
   [InlineData(-1L)]
   public void FromOffset_CreatesLocationWithOffset(long offset)
   {
      var location = DiagnosticLocationFactory.FromOffset(offset);

      Assert.Null(location.Source);
      Assert.Null(location.Line);
      Assert.Null(location.Column);
      Assert.Equal(offset, location.Offset);
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void FromSourceOffset_WhenSourceIsInvalid_ThrowsArgumentException(string? source)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
          DiagnosticLocationFactory.FromSourceOffset(source!, 0));
   }

   [Theory]
   [InlineData("input.ajis", 0L)]
   [InlineData("config.json", 128L)]
   [InlineData("source.md", -1L)]
   public void FromSourceOffset_CreatesLocationWithSourceAndOffset(
       string source,
       long offset)
   {
      var location = DiagnosticLocationFactory.FromSourceOffset(source, offset);

      Assert.Equal(source, location.Source);
      Assert.Null(location.Line);
      Assert.Null(location.Column);
      Assert.Equal(offset, location.Offset);
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void FromFullLocation_WhenSourceIsInvalid_ThrowsArgumentException(string? source)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
          DiagnosticLocationFactory.FromFullLocation(source!, 1, 1, 0));
   }

   [Theory]
   [InlineData("input.ajis", 1, 1, 0L)]
   [InlineData("config.json", 10, 5, 128L)]
   [InlineData("source.md", 0, 0, 0L)]
   [InlineData("source.md", -1, -2, -3L)]
   public void FromFullLocation_CreatesLocationWithAllValues(
       string source,
       int line,
       int column,
       long offset)
   {
      var location = DiagnosticLocationFactory.FromFullLocation(
          source,
          line,
          column,
          offset);

      Assert.Equal(source, location.Source);
      Assert.Equal(line, location.Line);
      Assert.Equal(column, location.Column);
      Assert.Equal(offset, location.Offset);
   }
}