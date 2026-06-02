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

   [Fact]
   public void HasCriticalOrHigherDiagnostics_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasDiagnostics? value = null;

      Assert.Throws<ArgumentNullException>(() =>
         value!.HasCriticalOrHigherDiagnostics());
   }

   [Fact]
   public void HasCriticalOrHigherDiagnostics_WhenDiagnosticsAreEmpty_ReturnsFalse()
   {
      var value = new TestDiagnosticsSource
      {
         Diagnostics = []
      };

      var actual = value.HasCriticalOrHigherDiagnostics();

      Assert.False(actual);
   }

   [Theory]
   [InlineData(IssueSeverity.None, false)]
   [InlineData(IssueSeverity.Trace, false)]
   [InlineData(IssueSeverity.Debug, false)]
   [InlineData(IssueSeverity.Information, false)]
   [InlineData(IssueSeverity.Warning, false)]
   [InlineData(IssueSeverity.Error, false)]
   [InlineData(IssueSeverity.Critical, true)]
   [InlineData(IssueSeverity.Fatal, true)]
   public void HasCriticalOrHigherDiagnostics_ReturnsExpectedResult(
      IssueSeverity severity,
      bool expected)
   {
      var value = new TestDiagnosticsSource
      {
         Diagnostics =
         [
            CreateDiagnostic(severity)
         ]
      };

      var actual = value.HasCriticalOrHigherDiagnostics();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasCriticalOrHigherDiagnostics_WhenAnyDiagnosticMatches_ReturnsTrue()
   {
      var value = new TestDiagnosticsSource
      {
         Diagnostics =
         [
            CreateDiagnostic(IssueSeverity.Error),
         CreateDiagnostic(IssueSeverity.Critical)
         ]
      };

      var actual = value.HasCriticalOrHigherDiagnostics();

      Assert.True(actual);
   }

   [Fact]
   public void HasCriticalOrHigherDiagnostics_WhenOnlyErrorDiagnosticsExist_ReturnsFalse()
   {
      var value = new TestDiagnosticsSource
      {
         Diagnostics =
         [
            CreateDiagnostic(IssueSeverity.Error),
         CreateDiagnostic(IssueSeverity.Warning)
         ]
      };

      var actual = value.HasCriticalOrHigherDiagnostics();

      Assert.False(actual);
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

   private sealed class TestDiagnosticsSource : IHasDiagnostics
   {
      public IReadOnlyList<DiagnosticInfo> Diagnostics { get; set; } = [];
   }

   [Fact]
   public void GetHighestDiagnosticSeverity_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasDiagnostics? value = null;

      Assert.Throws<ArgumentNullException>(() =>
         value!.GetHighestDiagnosticSeverity());
   }

   [Fact]
   public void GetHighestDiagnosticSeverity_WhenDiagnosticsAreEmpty_ReturnsNone()
   {
      var value = new TestDiagnosticsSource
      {
         Diagnostics = []
      };

      var actual = value.GetHighestDiagnosticSeverity();

      Assert.Equal(IssueSeverity.None, actual);
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
   public void GetHighestDiagnosticSeverity_WhenContainsSingleDiagnostic_ReturnsDiagnosticSeverity(
      IssueSeverity severity)
   {
      var value = new TestDiagnosticsSource
      {
         Diagnostics =
         [
            CreateDiagnosticInfo(severity)
         ]
      };

      var actual = value.GetHighestDiagnosticSeverity();

      Assert.Equal(severity, actual);
   }

   [Fact]
   public void GetHighestDiagnosticSeverity_WhenContainsMultipleDiagnostics_ReturnsHighestSeverity()
   {
      var value = new TestDiagnosticsSource
      {
         Diagnostics =
         [
            CreateDiagnosticInfo(IssueSeverity.Information),
         CreateDiagnosticInfo(IssueSeverity.Warning),
         CreateDiagnosticInfo(IssueSeverity.Error),
         CreateDiagnosticInfo(IssueSeverity.Debug)
         ]
      };

      var actual = value.GetHighestDiagnosticSeverity();

      Assert.Equal(IssueSeverity.Error, actual);
   }

   [Fact]
   public void GetHighestDiagnosticSeverity_WhenContainsFatal_ReturnsFatal()
   {
      var value = new TestDiagnosticsSource
      {
         Diagnostics =
         [
            CreateDiagnosticInfo(IssueSeverity.Information),
         CreateDiagnosticInfo(IssueSeverity.Warning),
         CreateDiagnosticInfo(IssueSeverity.Fatal),
         CreateDiagnosticInfo(IssueSeverity.Critical),
         CreateDiagnosticInfo(IssueSeverity.Error)
         ]
      };

      var actual = value.GetHighestDiagnosticSeverity();

      Assert.Equal(IssueSeverity.Fatal, actual);
   }

   [Fact]
   public void GetHighestDiagnosticSeverity_DoesNotDependOnDiagnosticOrder()
   {
      var firstValue = new TestDiagnosticsSource
      {
         Diagnostics =
         [
            CreateDiagnosticInfo(IssueSeverity.Fatal),
         CreateDiagnosticInfo(IssueSeverity.Information),
         CreateDiagnosticInfo(IssueSeverity.Warning)
         ]
      };

      var secondValue = new TestDiagnosticsSource
      {
         Diagnostics =
         [
            CreateDiagnosticInfo(IssueSeverity.Information),
         CreateDiagnosticInfo(IssueSeverity.Warning),
         CreateDiagnosticInfo(IssueSeverity.Fatal)
         ]
      };

      var firstActual = firstValue.GetHighestDiagnosticSeverity();
      var secondActual = secondValue.GetHighestDiagnosticSeverity();

      Assert.Equal(IssueSeverity.Fatal, firstActual);
      Assert.Equal(IssueSeverity.Fatal, secondActual);
   }
   private static DiagnosticInfo CreateDiagnosticInfo(IssueSeverity severity)
   {
      return new DiagnosticInfo
      {
         Code = $"AFW_{severity}",
         Message = $"Diagnostic with severity {severity}.",
         Severity = severity
      };
   }
   [Fact]
   public void HasOnlyInformationalOrLowerDiagnostics_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasDiagnostics? value = null;

      Assert.Throws<ArgumentNullException>(() =>
         value!.HasOnlyInformationalOrLowerDiagnostics());
   }

   [Fact]
   public void HasOnlyInformationalOrLowerDiagnostics_WhenDiagnosticsAreEmpty_ReturnsTrue()
   {
      var value = new TestDiagnosticsSource
      {
         Diagnostics = []
      };

      var actual = value.HasOnlyInformationalOrLowerDiagnostics();

      Assert.True(actual);
   }

   [Theory]
   [InlineData(IssueSeverity.None)]
   [InlineData(IssueSeverity.Trace)]
   [InlineData(IssueSeverity.Debug)]
   [InlineData(IssueSeverity.Information)]
   public void HasOnlyInformationalOrLowerDiagnostics_WhenSingleDiagnosticIsInformationalOrLower_ReturnsTrue(
      IssueSeverity severity)
   {
      var value = new TestDiagnosticsSource
      {
         Diagnostics =
         [
            CreateDiagnosticInfo(severity)
         ]
      };

      var actual = value.HasOnlyInformationalOrLowerDiagnostics();

      Assert.True(actual);
   }

   [Theory]
   [InlineData(IssueSeverity.Warning)]
   [InlineData(IssueSeverity.Error)]
   [InlineData(IssueSeverity.Critical)]
   [InlineData(IssueSeverity.Fatal)]
   public void HasOnlyInformationalOrLowerDiagnostics_WhenSingleDiagnosticIsWarningOrHigher_ReturnsFalse(
      IssueSeverity severity)
   {
      var value = new TestDiagnosticsSource
      {
         Diagnostics =
         [
            CreateDiagnosticInfo(severity)
         ]
      };

      var actual = value.HasOnlyInformationalOrLowerDiagnostics();

      Assert.False(actual);
   }

   [Fact]
   public void HasOnlyInformationalOrLowerDiagnostics_WhenAllDiagnosticsAreInformationalOrLower_ReturnsTrue()
   {
      var value = new TestDiagnosticsSource
      {
         Diagnostics =
         [
            CreateDiagnosticInfo(IssueSeverity.None),
         CreateDiagnosticInfo(IssueSeverity.Trace),
         CreateDiagnosticInfo(IssueSeverity.Debug),
         CreateDiagnosticInfo(IssueSeverity.Information)
         ]
      };

      var actual = value.HasOnlyInformationalOrLowerDiagnostics();

      Assert.True(actual);
   }

   [Fact]
   public void HasOnlyInformationalOrLowerDiagnostics_WhenAnyDiagnosticIsWarning_ReturnsFalse()
   {
      var value = new TestDiagnosticsSource
      {
         Diagnostics =
         [
            CreateDiagnosticInfo(IssueSeverity.None),
         CreateDiagnosticInfo(IssueSeverity.Information),
         CreateDiagnosticInfo(IssueSeverity.Warning)
         ]
      };

      var actual = value.HasOnlyInformationalOrLowerDiagnostics();

      Assert.False(actual);
   }

   [Fact]
   public void HasOnlyInformationalOrLowerDiagnostics_WhenAnyDiagnosticIsError_ReturnsFalse()
   {
      var value = new TestDiagnosticsSource
      {
         Diagnostics =
         [
            CreateDiagnosticInfo(IssueSeverity.Information),
         CreateDiagnosticInfo(IssueSeverity.Error)
         ]
      };

      var actual = value.HasOnlyInformationalOrLowerDiagnostics();

      Assert.False(actual);
   }

   [Fact]
   public void HasOnlyInformationalOrLowerDiagnostics_WhenAnyDiagnosticIsCritical_ReturnsFalse()
   {
      var value = new TestDiagnosticsSource
      {
         Diagnostics =
         [
            CreateDiagnosticInfo(IssueSeverity.Information),
         CreateDiagnosticInfo(IssueSeverity.Critical)
         ]
      };

      var actual = value.HasOnlyInformationalOrLowerDiagnostics();

      Assert.False(actual);
   }

   [Fact]
   public void HasAnyDiagnostics_WhenDiagnosticsPropertyIsNull_ReturnsFalse()
   {
      var value = new TestDiagnosticsSource
      {
         Diagnostics = null!
      };

      var actual = value.HasAnyDiagnostics();

      Assert.False(actual);
   }

   [Fact]
   public void HasDiagnosticErrors_WhenDiagnosticsPropertyIsNull_ReturnsFalse()
   {
      var value = new TestDiagnosticsSource
      {
         Diagnostics = null!
      };

      var actual = value.HasDiagnosticErrors();

      Assert.False(actual);
   }

   [Fact]
   public void HasDiagnosticWarningsOrErrors_WhenDiagnosticsPropertyIsNull_ReturnsFalse()
   {
      var value = new TestDiagnosticsSource
      {
         Diagnostics = null!
      };

      var actual = value.HasDiagnosticWarningsOrErrors();

      Assert.False(actual);
   }

   [Fact]
   public void HasCriticalOrHigherDiagnostics_WhenDiagnosticsPropertyIsNull_ReturnsFalse()
   {
      var value = new TestDiagnosticsSource
      {
         Diagnostics = null!
      };

      var actual = value.HasCriticalOrHigherDiagnostics();

      Assert.False(actual);
   }

   [Fact]
   public void HasOnlyInformationalOrLowerDiagnostics_WhenDiagnosticsPropertyIsNull_ReturnsTrue()
   {
      var value = new TestDiagnosticsSource
      {
         Diagnostics = null!
      };

      var actual = value.HasOnlyInformationalOrLowerDiagnostics();

      Assert.True(actual);
   }

   [Fact]
   public void GetHighestDiagnosticSeverity_WhenDiagnosticsPropertyIsNull_ReturnsNone()
   {
      var value = new TestDiagnosticsSource
      {
         Diagnostics = null!
      };

      var actual = value.GetHighestDiagnosticSeverity();

      Assert.Equal(IssueSeverity.None, actual);
   }

   [Fact]
   public void HasOnlyInformationalOrLowerDiagnostics_WhenAnyDiagnosticIsFatal_ReturnsFalse()
   {
      var value = new TestDiagnosticsSource
      {
         Diagnostics =
         [
            CreateDiagnosticInfo(IssueSeverity.Information),
         CreateDiagnosticInfo(IssueSeverity.Fatal)
         ]
      };

      var actual = value.HasOnlyInformationalOrLowerDiagnostics();

      Assert.False(actual);
   }
}