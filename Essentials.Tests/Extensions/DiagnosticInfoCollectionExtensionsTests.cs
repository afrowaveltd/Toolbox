using Afrowave.Toolbox.Essentials.Diagnostics;
using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Extensions;

namespace Afrowave.Toolbox.Essentials.Tests.Extensions;

public sealed class DiagnosticInfoCollectionExtensionsTests
{
   [Fact]
   public void HasErrors_WhenDiagnosticsIsNull_ThrowsArgumentNullException()
   {
      IEnumerable<DiagnosticInfo>? diagnostics = null;

      Assert.Throws<ArgumentNullException>(() => diagnostics!.HasErrors());
   }

   [Fact]
   public void HasErrors_WhenCollectionIsEmpty_ReturnsFalse()
   {
      var diagnostics = Array.Empty<DiagnosticInfo>();

      var actual = diagnostics.HasErrors();

      Assert.False(actual);
   }

   [Theory]
   [InlineData(IssueSeverity.None, false)]
   [InlineData(IssueSeverity.Trace, false)]
   [InlineData(IssueSeverity.Debug, false)]
   [InlineData(IssueSeverity.Information, false)]
   [InlineData(IssueSeverity.Warning, false)]
   [InlineData(IssueSeverity.Error, true)]
   [InlineData(IssueSeverity.Critical, true)]
   [InlineData(IssueSeverity.Fatal, true)]
   public void HasErrors_ReturnsExpectedResult(
       IssueSeverity severity,
       bool expected)
   {
      var diagnostics = new[]
      {
            CreateDiagnostic(severity)
        };

      var actual = diagnostics.HasErrors();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasWarningsOrErrors_WhenDiagnosticsIsNull_ThrowsArgumentNullException()
   {
      IEnumerable<DiagnosticInfo>? diagnostics = null;

      Assert.Throws<ArgumentNullException>(() => diagnostics!.HasWarningsOrErrors());
   }

   [Fact]
   public void HasWarningsOrErrors_WhenCollectionIsEmpty_ReturnsFalse()
   {
      var diagnostics = Array.Empty<DiagnosticInfo>();

      var actual = diagnostics.HasWarningsOrErrors();

      Assert.False(actual);
   }

   [Theory]
   [InlineData(IssueSeverity.None, false)]
   [InlineData(IssueSeverity.Trace, false)]
   [InlineData(IssueSeverity.Debug, false)]
   [InlineData(IssueSeverity.Information, false)]
   [InlineData(IssueSeverity.Warning, true)]
   [InlineData(IssueSeverity.Error, true)]
   [InlineData(IssueSeverity.Critical, true)]
   [InlineData(IssueSeverity.Fatal, true)]
   public void HasWarningsOrErrors_ReturnsExpectedResult(
       IssueSeverity severity,
       bool expected)
   {
      var diagnostics = new[]
      {
            CreateDiagnostic(severity)
        };

      var actual = diagnostics.HasWarningsOrErrors();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void Errors_WhenDiagnosticsIsNull_ThrowsArgumentNullException()
   {
      IEnumerable<DiagnosticInfo>? diagnostics = null;

      Assert.Throws<ArgumentNullException>(() => diagnostics!.Errors().ToArray());
   }

   [Fact]
   public void Errors_ReturnsOnlyErrorOrHigherSeverityDiagnostics()
   {
      var diagnostics = new[]
      {
            CreateDiagnostic(IssueSeverity.None, "none"),
            CreateDiagnostic(IssueSeverity.Trace, "trace"),
            CreateDiagnostic(IssueSeverity.Debug, "debug"),
            CreateDiagnostic(IssueSeverity.Information, "information"),
            CreateDiagnostic(IssueSeverity.Warning, "warning"),
            CreateDiagnostic(IssueSeverity.Error, "error"),
            CreateDiagnostic(IssueSeverity.Critical, "critical"),
            CreateDiagnostic(IssueSeverity.Fatal, "fatal")
        };

      var actual = diagnostics.Errors().Select(diagnostic => diagnostic.Code).ToArray();

      Assert.Equal(
          new[] { "error", "critical", "fatal" },
          actual);
   }

   [Fact]
   public void Warnings_WhenDiagnosticsIsNull_ThrowsArgumentNullException()
   {
      IEnumerable<DiagnosticInfo>? diagnostics = null;

      Assert.Throws<ArgumentNullException>(() => diagnostics!.Warnings().ToArray());
   }

   [Fact]
   public void Warnings_ReturnsOnlyWarningSeverityDiagnostics()
   {
      var diagnostics = new[]
      {
            CreateDiagnostic(IssueSeverity.None, "none"),
            CreateDiagnostic(IssueSeverity.Trace, "trace"),
            CreateDiagnostic(IssueSeverity.Debug, "debug"),
            CreateDiagnostic(IssueSeverity.Information, "information"),
            CreateDiagnostic(IssueSeverity.Warning, "warning"),
            CreateDiagnostic(IssueSeverity.Error, "error"),
            CreateDiagnostic(IssueSeverity.Critical, "critical"),
            CreateDiagnostic(IssueSeverity.Fatal, "fatal")
        };

      var actual = diagnostics.Warnings().Select(diagnostic => diagnostic.Code).ToArray();

      Assert.Equal(
          new[] { "warning" },
          actual);
   }

   [Fact]
   public void Informational_WhenDiagnosticsIsNull_ThrowsArgumentNullException()
   {
      IEnumerable<DiagnosticInfo>? diagnostics = null;

      Assert.Throws<ArgumentNullException>(() => diagnostics!.Informational().ToArray());
   }

   [Fact]
   public void Informational_ReturnsOnlyInformationOrLowerSeverityDiagnostics()
   {
      var diagnostics = new[]
      {
            CreateDiagnostic(IssueSeverity.None, "none"),
            CreateDiagnostic(IssueSeverity.Trace, "trace"),
            CreateDiagnostic(IssueSeverity.Debug, "debug"),
            CreateDiagnostic(IssueSeverity.Information, "information"),
            CreateDiagnostic(IssueSeverity.Warning, "warning"),
            CreateDiagnostic(IssueSeverity.Error, "error"),
            CreateDiagnostic(IssueSeverity.Critical, "critical"),
            CreateDiagnostic(IssueSeverity.Fatal, "fatal")
        };

      var actual = diagnostics.Informational().Select(diagnostic => diagnostic.Code).ToArray();

      Assert.Equal(
          new[] { "none", "trace", "debug", "information" },
          actual);
   }

   [Fact]
   public void WithSeverity_WhenDiagnosticsIsNull_ThrowsArgumentNullException()
   {
      IEnumerable<DiagnosticInfo>? diagnostics = null;

      Assert.Throws<ArgumentNullException>(() =>
          diagnostics!.WithSeverity(IssueSeverity.Warning).ToArray());
   }

   [Theory]
   [InlineData(IssueSeverity.None, "none")]
   [InlineData(IssueSeverity.Trace, "trace")]
   [InlineData(IssueSeverity.Debug, "debug")]
   [InlineData(IssueSeverity.Information, "information")]
   [InlineData(IssueSeverity.Warning, "warning")]
   [InlineData(IssueSeverity.Error, "error")]
   [InlineData(IssueSeverity.Critical, "critical")]
   [InlineData(IssueSeverity.Fatal, "fatal")]
   public void WithSeverity_ReturnsOnlyDiagnosticsWithSpecifiedSeverity(
       IssueSeverity severity,
       string expectedCode)
   {
      var diagnostics = new[]
      {
            CreateDiagnostic(IssueSeverity.None, "none"),
            CreateDiagnostic(IssueSeverity.Trace, "trace"),
            CreateDiagnostic(IssueSeverity.Debug, "debug"),
            CreateDiagnostic(IssueSeverity.Information, "information"),
            CreateDiagnostic(IssueSeverity.Warning, "warning"),
            CreateDiagnostic(IssueSeverity.Error, "error"),
            CreateDiagnostic(IssueSeverity.Critical, "critical"),
            CreateDiagnostic(IssueSeverity.Fatal, "fatal")
        };

      var actual = diagnostics
          .WithSeverity(severity)
          .Select(diagnostic => diagnostic.Code)
          .ToArray();

      Assert.Equal(
          new[] { expectedCode },
          actual);
   }

   private static DiagnosticInfo CreateDiagnostic(
       IssueSeverity severity,
       string? code = null)
   {
      return new DiagnosticInfo
      {
         Code = code ?? severity.ToString().ToLowerInvariant(),
         Message = $"Test diagnostic with severity {severity}.",
         Severity = severity
      };
   }
}