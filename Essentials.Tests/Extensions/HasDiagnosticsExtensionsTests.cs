using Afrowave.Toolbox.Essentials.Diagnostics;
using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Extensions;
using Afrowave.Toolbox.Essentials.Interfaces;

namespace Afrowave.Toolbox.Essentials.Tests.Extensions;

public sealed class HasDiagnosticsExtensionsTests
{
   [Fact]
   public void HasAnyDiagnostics_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasDiagnostics? value = null;

      Assert.Throws<ArgumentNullException>(() => value!.HasAnyDiagnostics());
   }

   [Fact]
   public void HasAnyDiagnostics_WhenDiagnosticsAreEmpty_ReturnsFalse()
   {
      var value = new TestHasDiagnostics([]);

      var actual = value.HasAnyDiagnostics();

      Assert.False(actual);
   }

   [Fact]
   public void HasAnyDiagnostics_WhenDiagnosticsContainOneItem_ReturnsTrue()
   {
      var value = new TestHasDiagnostics(
      [
          CreateDiagnostic(IssueSeverity.Information)
      ]);

      var actual = value.HasAnyDiagnostics();

      Assert.True(actual);
   }

   [Fact]
   public void HasDiagnosticErrors_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasDiagnostics? value = null;

      Assert.Throws<ArgumentNullException>(() => value!.HasDiagnosticErrors());
   }

   [Fact]
   public void HasDiagnosticErrors_WhenDiagnosticsAreEmpty_ReturnsFalse()
   {
      var value = new TestHasDiagnostics([]);

      var actual = value.HasDiagnosticErrors();

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
   public void HasDiagnosticErrors_ReturnsExpectedResult(
       IssueSeverity severity,
       bool expected)
   {
      var value = new TestHasDiagnostics(
      [
          CreateDiagnostic(severity)
      ]);

      var actual = value.HasDiagnosticErrors();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasDiagnosticWarningsOrErrors_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasDiagnostics? value = null;

      Assert.Throws<ArgumentNullException>(() => value!.HasDiagnosticWarningsOrErrors());
   }

   [Fact]
   public void HasDiagnosticWarningsOrErrors_WhenDiagnosticsAreEmpty_ReturnsFalse()
   {
      var value = new TestHasDiagnostics([]);

      var actual = value.HasDiagnosticWarningsOrErrors();

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
   public void HasDiagnosticWarningsOrErrors_ReturnsExpectedResult(
       IssueSeverity severity,
       bool expected)
   {
      var value = new TestHasDiagnostics(
      [
          CreateDiagnostic(severity)
      ]);

      var actual = value.HasDiagnosticWarningsOrErrors();

      Assert.Equal(expected, actual);
   }

   private static DiagnosticInfo CreateDiagnostic(IssueSeverity severity)
   {
      return new DiagnosticInfo
      {
         Code = severity.ToString(),
         Message = $"Test diagnostic with severity {severity}.",
         Severity = severity
      };
   }

   private sealed class TestHasDiagnostics : IHasDiagnostics
   {
      public TestHasDiagnostics(IReadOnlyList<DiagnosticInfo> diagnostics)
      {
         Diagnostics = diagnostics;
      }

      public IReadOnlyList<DiagnosticInfo> Diagnostics { get; }
   }
}