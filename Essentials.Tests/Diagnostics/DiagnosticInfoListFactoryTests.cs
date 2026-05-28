using Afrowave.Toolbox.Essentials.Diagnostics;
using Afrowave.Toolbox.Essentials.Enums;

namespace Afrowave.Toolbox.Essentials.Tests.Diagnostics;

public sealed class DiagnosticInfoListFactoryTests
{
   [Fact]
   public void Empty_ReturnsEmptyDiagnosticList()
   {
      var diagnostics = DiagnosticInfoListFactory.Empty();

      Assert.NotNull(diagnostics);
      Assert.Empty(diagnostics);
   }

   [Fact]
   public void Empty_ReturnsNewEmptyListEachTime()
   {
      var first = DiagnosticInfoListFactory.Empty();
      var second = DiagnosticInfoListFactory.Empty();

      Assert.NotSame(first, second);
      Assert.Empty(first);
      Assert.Empty(second);
   }

   [Fact]
   public void From_WithParams_WhenDiagnosticsIsNull_ThrowsArgumentNullException()
   {
      DiagnosticInfo[]? diagnostics = null;

      Assert.Throws<ArgumentNullException>(() =>
          DiagnosticInfoListFactory.From(diagnostics!));
   }

   [Fact]
   public void From_WithParams_WhenDiagnosticsAreEmpty_ReturnsEmptyDiagnosticList()
   {
      var diagnostics = DiagnosticInfoListFactory.From([]);

      Assert.NotNull(diagnostics);
      Assert.Empty(diagnostics);
   }

   [Fact]
   public void From_WithParams_CreatesDiagnosticList()
   {
      var first = DiagnosticInfoFactory.Warning(
          "AFW_WARNING",
          "Warning message.");

      var second = DiagnosticInfoFactory.Error(
          "AFW_ERROR",
          "Error message.");

      var diagnostics = DiagnosticInfoListFactory.From(first, second);

      Assert.Equal(2, diagnostics.Count);
      Assert.Same(first, diagnostics[0]);
      Assert.Same(second, diagnostics[1]);
   }

   [Fact]
   public void From_WithParams_ReturnsSnapshot()
   {
      var first = DiagnosticInfoFactory.Warning(
          "AFW_WARNING",
          "Warning message.");

      var second = DiagnosticInfoFactory.Error(
          "AFW_ERROR",
          "Error message.");

      var source = new[]
      {
            first,
            second
        };

      var diagnostics = DiagnosticInfoListFactory.From(source);

      source[0] = DiagnosticInfoFactory.Information(
          "AFW_INFO",
          "Changed message.");

      Assert.Equal(2, diagnostics.Count);
      Assert.Same(first, diagnostics[0]);
      Assert.Same(second, diagnostics[1]);
   }

   [Fact]
   public void From_WithEnumerable_WhenDiagnosticsIsNull_ThrowsArgumentNullException()
   {
      IEnumerable<DiagnosticInfo>? diagnostics = null;

      Assert.Throws<ArgumentNullException>(() =>
          DiagnosticInfoListFactory.From(diagnostics!));
   }

   [Fact]
   public void From_WithEnumerable_WhenDiagnosticsAreEmpty_ReturnsEmptyDiagnosticList()
   {
      IEnumerable<DiagnosticInfo> source = [];

      var diagnostics = DiagnosticInfoListFactory.From(source);

      Assert.NotNull(diagnostics);
      Assert.Empty(diagnostics);
   }

   [Fact]
   public void From_WithEnumerable_CreatesDiagnosticList()
   {
      var first = DiagnosticInfoFactory.Warning(
          "AFW_WARNING",
          "Warning message.");

      var second = DiagnosticInfoFactory.Error(
          "AFW_ERROR",
          "Error message.");

      IEnumerable<DiagnosticInfo> source =
      [
          first,
            second
      ];

      var diagnostics = DiagnosticInfoListFactory.From(source);

      Assert.Equal(2, diagnostics.Count);
      Assert.Same(first, diagnostics[0]);
      Assert.Same(second, diagnostics[1]);
   }

   [Fact]
   public void From_WithEnumerable_ReturnsSnapshot()
   {
      var first = DiagnosticInfoFactory.Warning(
          "AFW_WARNING",
          "Warning message.");

      var second = DiagnosticInfoFactory.Error(
          "AFW_ERROR",
          "Error message.");

      var source = new List<DiagnosticInfo>
        {
            first,
            second
        };

      var diagnostics = DiagnosticInfoListFactory.From((IEnumerable<DiagnosticInfo>)source);

      source.Clear();

      Assert.Equal(2, diagnostics.Count);
      Assert.Same(first, diagnostics[0]);
      Assert.Same(second, diagnostics[1]);
   }

   [Fact]
   public void Information_CreatesListWithOneInformationDiagnostic()
   {
      var diagnostics = DiagnosticInfoListFactory.Information(
          "AFW_INFO",
          "Information message.");

      Assert.Single(diagnostics);

      var diagnostic = diagnostics[0];

      Assert.Equal("AFW_INFO", diagnostic.Code);
      Assert.Equal("Information message.", diagnostic.Message);
      Assert.Equal(IssueSeverity.Information, diagnostic.Severity);
   }

   [Fact]
   public void Warning_CreatesListWithOneWarningDiagnostic()
   {
      var diagnostics = DiagnosticInfoListFactory.Warning(
          "AFW_WARNING",
          "Warning message.");

      Assert.Single(diagnostics);

      var diagnostic = diagnostics[0];

      Assert.Equal("AFW_WARNING", diagnostic.Code);
      Assert.Equal("Warning message.", diagnostic.Message);
      Assert.Equal(IssueSeverity.Warning, diagnostic.Severity);
   }

   [Fact]
   public void Error_CreatesListWithOneErrorDiagnostic()
   {
      var diagnostics = DiagnosticInfoListFactory.Error(
          "AFW_ERROR",
          "Error message.");

      Assert.Single(diagnostics);

      var diagnostic = diagnostics[0];

      Assert.Equal("AFW_ERROR", diagnostic.Code);
      Assert.Equal("Error message.", diagnostic.Message);
      Assert.Equal(IssueSeverity.Error, diagnostic.Severity);
   }

   [Fact]
   public void Critical_CreatesListWithOneCriticalDiagnostic()
   {
      var diagnostics = DiagnosticInfoListFactory.Critical(
          "AFW_CRITICAL",
          "Critical message.");

      Assert.Single(diagnostics);

      var diagnostic = diagnostics[0];

      Assert.Equal("AFW_CRITICAL", diagnostic.Code);
      Assert.Equal("Critical message.", diagnostic.Message);
      Assert.Equal(IssueSeverity.Critical, diagnostic.Severity);
   }

   [Fact]
   public void Fatal_CreatesListWithOneFatalDiagnostic()
   {
      var diagnostics = DiagnosticInfoListFactory.Fatal(
          "AFW_FATAL",
          "Fatal message.");

      Assert.Single(diagnostics);

      var diagnostic = diagnostics[0];

      Assert.Equal("AFW_FATAL", diagnostic.Code);
      Assert.Equal("Fatal message.", diagnostic.Message);
      Assert.Equal(IssueSeverity.Fatal, diagnostic.Severity);
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Information_WhenCodeIsInvalid_ThrowsArgumentException(string? code)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
          DiagnosticInfoListFactory.Information(code!, "Information message."));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Information_WhenMessageIsInvalid_ThrowsArgumentException(string? message)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
          DiagnosticInfoListFactory.Information("AFW_INFO", message!));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Warning_WhenCodeIsInvalid_ThrowsArgumentException(string? code)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
          DiagnosticInfoListFactory.Warning(code!, "Warning message."));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Warning_WhenMessageIsInvalid_ThrowsArgumentException(string? message)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
          DiagnosticInfoListFactory.Warning("AFW_WARNING", message!));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Error_WhenCodeIsInvalid_ThrowsArgumentException(string? code)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
          DiagnosticInfoListFactory.Error(code!, "Error message."));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Error_WhenMessageIsInvalid_ThrowsArgumentException(string? message)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
          DiagnosticInfoListFactory.Error("AFW_ERROR", message!));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Critical_WhenCodeIsInvalid_ThrowsArgumentException(string? code)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
          DiagnosticInfoListFactory.Critical(code!, "Critical message."));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Critical_WhenMessageIsInvalid_ThrowsArgumentException(string? message)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
          DiagnosticInfoListFactory.Critical("AFW_CRITICAL", message!));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Fatal_WhenCodeIsInvalid_ThrowsArgumentException(string? code)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
          DiagnosticInfoListFactory.Fatal(code!, "Fatal message."));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Fatal_WhenMessageIsInvalid_ThrowsArgumentException(string? message)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
          DiagnosticInfoListFactory.Fatal("AFW_FATAL", message!));
   }
}