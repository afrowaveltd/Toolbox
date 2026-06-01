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
   [Fact]
   public void HasAnyDiagnostics_WhenDiagnosticsIsNull_ThrowsArgumentNullException()
   {
      IEnumerable<DiagnosticInfo>? diagnostics = null;

      Assert.Throws<ArgumentNullException>(() =>
         diagnostics!.HasAnyDiagnostics());
   }

   [Fact]
   public void HasAnyDiagnostics_WhenDiagnosticsAreEmpty_ReturnsFalse()
   {
      IEnumerable<DiagnosticInfo> diagnostics = [];

      var actual = diagnostics.HasAnyDiagnostics();

      Assert.False(actual);
   }

   [Fact]
   public void HasAnyDiagnostics_WhenDiagnosticsContainOneItem_ReturnsTrue()
   {
      IEnumerable<DiagnosticInfo> diagnostics =
      [
         CreateDiagnosticInfo(IssueSeverity.Information)
      ];

      var actual = diagnostics.HasAnyDiagnostics();

      Assert.True(actual);
   }

   [Fact]
   public void HasAnyDiagnostics_WhenDiagnosticsContainMultipleItems_ReturnsTrue()
   {
      IEnumerable<DiagnosticInfo> diagnostics =
      [
         CreateDiagnosticInfo(IssueSeverity.Information),
      CreateDiagnosticInfo(IssueSeverity.Warning),
      CreateDiagnosticInfo(IssueSeverity.Error)
      ];

      var actual = diagnostics.HasAnyDiagnostics();

      Assert.True(actual);
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
   public void HasWarningOrHigherDiagnostics_WhenDiagnosticsIsNull_ThrowsArgumentNullException()
   {
      IEnumerable<DiagnosticInfo>? diagnostics = null;

      Assert.Throws<ArgumentNullException>(() =>
         diagnostics!.HasWarningOrHigherDiagnostics());
   }

   [Fact]
   public void HasWarningOrHigherDiagnostics_WhenDiagnosticsAreEmpty_ReturnsFalse()
   {
      IEnumerable<DiagnosticInfo> diagnostics = [];

      var actual = diagnostics.HasWarningOrHigherDiagnostics();

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
   public void HasWarningOrHigherDiagnostics_ReturnsExpectedResult(
      IssueSeverity severity,
      bool expected)
   {
      IEnumerable<DiagnosticInfo> diagnostics =
      [
         CreateDiagnosticInfo(severity)
      ];

      var actual = diagnostics.HasWarningOrHigherDiagnostics();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasWarningOrHigherDiagnostics_WhenAnyDiagnosticMatches_ReturnsTrue()
   {
      IEnumerable<DiagnosticInfo> diagnostics =
      [
         CreateDiagnosticInfo(IssueSeverity.Information),
      CreateDiagnosticInfo(IssueSeverity.Warning)
      ];

      var actual = diagnostics.HasWarningOrHigherDiagnostics();

      Assert.True(actual);
   }

   [Fact]
   public void HasErrorOrHigherDiagnostics_WhenDiagnosticsIsNull_ThrowsArgumentNullException()
   {
      IEnumerable<DiagnosticInfo>? diagnostics = null;

      Assert.Throws<ArgumentNullException>(() =>
         diagnostics!.HasErrorOrHigherDiagnostics());
   }

   [Fact]
   public void HasErrorOrHigherDiagnostics_WhenDiagnosticsAreEmpty_ReturnsFalse()
   {
      IEnumerable<DiagnosticInfo> diagnostics = [];

      var actual = diagnostics.HasErrorOrHigherDiagnostics();

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
   public void HasErrorOrHigherDiagnostics_ReturnsExpectedResult(
      IssueSeverity severity,
      bool expected)
   {
      IEnumerable<DiagnosticInfo> diagnostics =
      [
         CreateDiagnosticInfo(severity)
      ];

      var actual = diagnostics.HasErrorOrHigherDiagnostics();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasErrorOrHigherDiagnostics_WhenAnyDiagnosticMatches_ReturnsTrue()
   {
      IEnumerable<DiagnosticInfo> diagnostics =
      [
         CreateDiagnosticInfo(IssueSeverity.Warning),
      CreateDiagnosticInfo(IssueSeverity.Error)
      ];

      var actual = diagnostics.HasErrorOrHigherDiagnostics();

      Assert.True(actual);
   }
   [Fact]
   public void HasCriticalOrHigherDiagnostics_WhenDiagnosticsIsNull_ThrowsArgumentNullException()
   {
      IEnumerable<DiagnosticInfo>? diagnostics = null;

      Assert.Throws<ArgumentNullException>(() =>
         diagnostics!.HasCriticalOrHigherDiagnostics());
   }

   [Fact]
   public void HasCriticalOrHigherDiagnostics_WhenDiagnosticsAreEmpty_ReturnsFalse()
   {
      IEnumerable<DiagnosticInfo> diagnostics = [];

      var actual = diagnostics.HasCriticalOrHigherDiagnostics();

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
      IEnumerable<DiagnosticInfo> diagnostics =
      [
         CreateDiagnosticInfo(severity)
      ];

      var actual = diagnostics.HasCriticalOrHigherDiagnostics();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasCriticalOrHigherDiagnostics_WhenAnyDiagnosticMatches_ReturnsTrue()
   {
      IEnumerable<DiagnosticInfo> diagnostics =
      [
         CreateDiagnosticInfo(IssueSeverity.Error),
      CreateDiagnosticInfo(IssueSeverity.Critical)
      ];

      var actual = diagnostics.HasCriticalOrHigherDiagnostics();

      Assert.True(actual);
   }

   [Fact]
   public void HasCriticalOrHigherDiagnostics_WhenOnlyErrorDiagnosticsExist_ReturnsFalse()
   {
      IEnumerable<DiagnosticInfo> diagnostics =
      [
         CreateDiagnosticInfo(IssueSeverity.Error),
      CreateDiagnosticInfo(IssueSeverity.Warning)
      ];

      var actual = diagnostics.HasCriticalOrHigherDiagnostics();

      Assert.False(actual);
   }
   [Fact]
   public void WhereSeverity_WhenDiagnosticsIsNull_ThrowsArgumentNullException()
   {
      IEnumerable<DiagnosticInfo>? diagnostics = null;

      Assert.Throws<ArgumentNullException>(() =>
         diagnostics!.WhereSeverity(IssueSeverity.Warning).ToList());
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
   public void WhereSeverity_ReturnsOnlyMatchingSeverity(
   IssueSeverity severity)
   {
      IEnumerable<DiagnosticInfo> diagnostics =
      [
         CreateDiagnosticInfo(IssueSeverity.None),
      CreateDiagnosticInfo(IssueSeverity.Trace),
      CreateDiagnosticInfo(IssueSeverity.Debug),
      CreateDiagnosticInfo(IssueSeverity.Information),
      CreateDiagnosticInfo(IssueSeverity.Warning),
      CreateDiagnosticInfo(IssueSeverity.Error),
      CreateDiagnosticInfo(IssueSeverity.Critical),
      CreateDiagnosticInfo(IssueSeverity.Fatal)
      ];

      var result = diagnostics
         .WhereSeverity(severity)
         .ToList();

      Assert.NotEmpty(result);
      Assert.All(
         result,
         diagnostic => Assert.Equal(severity, diagnostic.Severity));
   }

   [Fact]
   public void WhereSeverity_WhenNoMatchingSeverity_ReturnsEmptyCollection()
   {
      IEnumerable<DiagnosticInfo> diagnostics =
      [
         CreateDiagnosticInfo(IssueSeverity.Information),
      CreateDiagnosticInfo(IssueSeverity.Warning)
      ];

      var result = diagnostics
         .WhereSeverity(IssueSeverity.Fatal)
         .ToList();

      Assert.Empty(result);
   }

   [Fact]
   public void WhereWarningOrHigher_WhenDiagnosticsIsNull_ThrowsArgumentNullException()
   {
      IEnumerable<DiagnosticInfo>? diagnostics = null;

      Assert.Throws<ArgumentNullException>(() =>
         diagnostics!.WhereWarningOrHigher().ToList());
   }

   [Fact]
   public void WhereWarningOrHigher_ReturnsWarningErrorCriticalAndFatalDiagnostics()
   {
      var warning = CreateDiagnosticInfo(IssueSeverity.Warning);
      var error = CreateDiagnosticInfo(IssueSeverity.Error);
      var critical = CreateDiagnosticInfo(IssueSeverity.Critical);
      var fatal = CreateDiagnosticInfo(IssueSeverity.Fatal);

      IEnumerable<DiagnosticInfo> diagnostics =
      [
         CreateDiagnosticInfo(IssueSeverity.None),
      CreateDiagnosticInfo(IssueSeverity.Trace),
      CreateDiagnosticInfo(IssueSeverity.Debug),
      CreateDiagnosticInfo(IssueSeverity.Information),
      warning,
      error,
      critical,
      fatal
      ];

      var result = diagnostics
         .WhereWarningOrHigher()
         .ToList();

      Assert.Equal(4, result.Count);
      Assert.Same(warning, result[0]);
      Assert.Same(error, result[1]);
      Assert.Same(critical, result[2]);
      Assert.Same(fatal, result[3]);
   }

   [Fact]
   public void WhereErrorOrHigher_WhenDiagnosticsIsNull_ThrowsArgumentNullException()
   {
      IEnumerable<DiagnosticInfo>? diagnostics = null;

      Assert.Throws<ArgumentNullException>(() =>
         diagnostics!.WhereErrorOrHigher().ToList());
   }

   [Fact]
   public void WhereErrorOrHigher_ReturnsErrorCriticalAndFatalDiagnostics()
   {
      var error = CreateDiagnosticInfo(IssueSeverity.Error);
      var critical = CreateDiagnosticInfo(IssueSeverity.Critical);
      var fatal = CreateDiagnosticInfo(IssueSeverity.Fatal);

      IEnumerable<DiagnosticInfo> diagnostics =
      [
         CreateDiagnosticInfo(IssueSeverity.None),
      CreateDiagnosticInfo(IssueSeverity.Trace),
      CreateDiagnosticInfo(IssueSeverity.Debug),
      CreateDiagnosticInfo(IssueSeverity.Information),
      CreateDiagnosticInfo(IssueSeverity.Warning),
      error,
      critical,
      fatal
      ];

      var result = diagnostics
         .WhereErrorOrHigher()
         .ToList();

      Assert.Equal(3, result.Count);
      Assert.Same(error, result[0]);
      Assert.Same(critical, result[1]);
      Assert.Same(fatal, result[2]);
   }

   [Fact]
   public void WhereCriticalOrHigher_WhenDiagnosticsIsNull_ThrowsArgumentNullException()
   {
      IEnumerable<DiagnosticInfo>? diagnostics = null;

      Assert.Throws<ArgumentNullException>(() =>
         diagnostics!.WhereCriticalOrHigher().ToList());
   }

   [Fact]
   public void WhereCriticalOrHigher_ReturnsCriticalAndFatalDiagnostics()
   {
      var critical = CreateDiagnosticInfo(IssueSeverity.Critical);
      var fatal = CreateDiagnosticInfo(IssueSeverity.Fatal);

      IEnumerable<DiagnosticInfo> diagnostics =
      [
         CreateDiagnosticInfo(IssueSeverity.None),
      CreateDiagnosticInfo(IssueSeverity.Trace),
      CreateDiagnosticInfo(IssueSeverity.Debug),
      CreateDiagnosticInfo(IssueSeverity.Information),
      CreateDiagnosticInfo(IssueSeverity.Warning),
      CreateDiagnosticInfo(IssueSeverity.Error),
      critical,
      fatal
      ];

      var result = diagnostics
         .WhereCriticalOrHigher()
         .ToList();

      Assert.Equal(2, result.Count);
      Assert.Same(critical, result[0]);
      Assert.Same(fatal, result[1]);
   }

   [Fact]
   public void WhereCriticalOrHigher_WhenOnlyErrorDiagnosticsExist_ReturnsEmptyCollection()
   {
      IEnumerable<DiagnosticInfo> diagnostics =
      [
         CreateDiagnosticInfo(IssueSeverity.Warning),
      CreateDiagnosticInfo(IssueSeverity.Error)
      ];

      var result = diagnostics
         .WhereCriticalOrHigher()
         .ToList();

      Assert.Empty(result);
   }

   [Fact]
   public void WhereMethods_PreserveOriginalOrder()
   {
      var first = CreateDiagnosticInfo(IssueSeverity.Error);
      var second = CreateDiagnosticInfo(IssueSeverity.Fatal);
      var third = CreateDiagnosticInfo(IssueSeverity.Critical);

      IEnumerable<DiagnosticInfo> diagnostics =
      [
         first,
      CreateDiagnosticInfo(IssueSeverity.Information),
      second,
      CreateDiagnosticInfo(IssueSeverity.Warning),
      third
      ];

      var result = diagnostics
         .WhereErrorOrHigher()
         .ToList();

      Assert.Equal(3, result.Count);
      Assert.Same(first, result[0]);
      Assert.Same(second, result[1]);
      Assert.Same(third, result[2]);
   }
   [Fact]
   public void CountSeverity_WhenDiagnosticsIsNull_ThrowsArgumentNullException()
   {
      IEnumerable<DiagnosticInfo>? diagnostics = null;

      Assert.Throws<ArgumentNullException>(() =>
         diagnostics!.CountSeverity(IssueSeverity.Warning));
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
   public void CountSeverity_WhenDiagnosticsAreEmpty_ReturnsZero(
      IssueSeverity severity)
   {
      IEnumerable<DiagnosticInfo> diagnostics = [];

      var actual = diagnostics.CountSeverity(severity);

      Assert.Equal(0, actual);
   }

   [Theory]
   [InlineData(IssueSeverity.None, 1)]
   [InlineData(IssueSeverity.Trace, 1)]
   [InlineData(IssueSeverity.Debug, 1)]
   [InlineData(IssueSeverity.Information, 2)]
   [InlineData(IssueSeverity.Warning, 3)]
   [InlineData(IssueSeverity.Error, 2)]
   [InlineData(IssueSeverity.Critical, 1)]
   [InlineData(IssueSeverity.Fatal, 1)]
   public void CountSeverity_ReturnsExpectedCount(
      IssueSeverity severity,
      int expected)
   {
      IEnumerable<DiagnosticInfo> diagnostics =
      [
         CreateDiagnosticInfo(IssueSeverity.None),
      CreateDiagnosticInfo(IssueSeverity.Trace),
      CreateDiagnosticInfo(IssueSeverity.Debug),
      CreateDiagnosticInfo(IssueSeverity.Information),
      CreateDiagnosticInfo(IssueSeverity.Information),
      CreateDiagnosticInfo(IssueSeverity.Warning),
      CreateDiagnosticInfo(IssueSeverity.Warning),
      CreateDiagnosticInfo(IssueSeverity.Warning),
      CreateDiagnosticInfo(IssueSeverity.Error),
      CreateDiagnosticInfo(IssueSeverity.Error),
      CreateDiagnosticInfo(IssueSeverity.Critical),
      CreateDiagnosticInfo(IssueSeverity.Fatal)
      ];

      var actual = diagnostics.CountSeverity(severity);

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void CountWarningOrHigher_WhenDiagnosticsIsNull_ThrowsArgumentNullException()
   {
      IEnumerable<DiagnosticInfo>? diagnostics = null;

      Assert.Throws<ArgumentNullException>(() =>
         diagnostics!.CountWarningOrHigher());
   }

   [Fact]
   public void CountWarningOrHigher_WhenDiagnosticsAreEmpty_ReturnsZero()
   {
      IEnumerable<DiagnosticInfo> diagnostics = [];

      var actual = diagnostics.CountWarningOrHigher();

      Assert.Equal(0, actual);
   }

   [Fact]
   public void CountWarningOrHigher_ReturnsExpectedCount()
   {
      IEnumerable<DiagnosticInfo> diagnostics =
      [
         CreateDiagnosticInfo(IssueSeverity.None),
      CreateDiagnosticInfo(IssueSeverity.Trace),
      CreateDiagnosticInfo(IssueSeverity.Debug),
      CreateDiagnosticInfo(IssueSeverity.Information),
      CreateDiagnosticInfo(IssueSeverity.Warning),
      CreateDiagnosticInfo(IssueSeverity.Warning),
      CreateDiagnosticInfo(IssueSeverity.Error),
      CreateDiagnosticInfo(IssueSeverity.Critical),
      CreateDiagnosticInfo(IssueSeverity.Fatal)
      ];

      var actual = diagnostics.CountWarningOrHigher();

      Assert.Equal(5, actual);
   }

   [Fact]
   public void CountWarningOrHigher_WhenNoMatchingDiagnostics_ReturnsZero()
   {
      IEnumerable<DiagnosticInfo> diagnostics =
      [
         CreateDiagnosticInfo(IssueSeverity.None),
      CreateDiagnosticInfo(IssueSeverity.Trace),
      CreateDiagnosticInfo(IssueSeverity.Debug),
      CreateDiagnosticInfo(IssueSeverity.Information)
      ];

      var actual = diagnostics.CountWarningOrHigher();

      Assert.Equal(0, actual);
   }

   [Fact]
   public void CountErrorOrHigher_WhenDiagnosticsIsNull_ThrowsArgumentNullException()
   {
      IEnumerable<DiagnosticInfo>? diagnostics = null;

      Assert.Throws<ArgumentNullException>(() =>
         diagnostics!.CountErrorOrHigher());
   }

   [Fact]
   public void CountErrorOrHigher_WhenDiagnosticsAreEmpty_ReturnsZero()
   {
      IEnumerable<DiagnosticInfo> diagnostics = [];

      var actual = diagnostics.CountErrorOrHigher();

      Assert.Equal(0, actual);
   }

   [Fact]
   public void CountErrorOrHigher_ReturnsExpectedCount()
   {
      IEnumerable<DiagnosticInfo> diagnostics =
      [
         CreateDiagnosticInfo(IssueSeverity.None),
      CreateDiagnosticInfo(IssueSeverity.Trace),
      CreateDiagnosticInfo(IssueSeverity.Debug),
      CreateDiagnosticInfo(IssueSeverity.Information),
      CreateDiagnosticInfo(IssueSeverity.Warning),
      CreateDiagnosticInfo(IssueSeverity.Error),
      CreateDiagnosticInfo(IssueSeverity.Error),
      CreateDiagnosticInfo(IssueSeverity.Critical),
      CreateDiagnosticInfo(IssueSeverity.Fatal)
      ];

      var actual = diagnostics.CountErrorOrHigher();

      Assert.Equal(4, actual);
   }

   [Fact]
   public void CountErrorOrHigher_WhenNoMatchingDiagnostics_ReturnsZero()
   {
      IEnumerable<DiagnosticInfo> diagnostics =
      [
         CreateDiagnosticInfo(IssueSeverity.None),
      CreateDiagnosticInfo(IssueSeverity.Trace),
      CreateDiagnosticInfo(IssueSeverity.Debug),
      CreateDiagnosticInfo(IssueSeverity.Information),
      CreateDiagnosticInfo(IssueSeverity.Warning)
      ];

      var actual = diagnostics.CountErrorOrHigher();

      Assert.Equal(0, actual);
   }

   [Fact]
   public void CountCriticalOrHigher_WhenDiagnosticsIsNull_ThrowsArgumentNullException()
   {
      IEnumerable<DiagnosticInfo>? diagnostics = null;

      Assert.Throws<ArgumentNullException>(() =>
         diagnostics!.CountCriticalOrHigher());
   }

   [Fact]
   public void CountCriticalOrHigher_WhenDiagnosticsAreEmpty_ReturnsZero()
   {
      IEnumerable<DiagnosticInfo> diagnostics = [];

      var actual = diagnostics.CountCriticalOrHigher();

      Assert.Equal(0, actual);
   }

   [Fact]
   public void CountCriticalOrHigher_ReturnsExpectedCount()
   {
      IEnumerable<DiagnosticInfo> diagnostics =
      [
         CreateDiagnosticInfo(IssueSeverity.None),
      CreateDiagnosticInfo(IssueSeverity.Trace),
      CreateDiagnosticInfo(IssueSeverity.Debug),
      CreateDiagnosticInfo(IssueSeverity.Information),
      CreateDiagnosticInfo(IssueSeverity.Warning),
      CreateDiagnosticInfo(IssueSeverity.Error),
      CreateDiagnosticInfo(IssueSeverity.Critical),
      CreateDiagnosticInfo(IssueSeverity.Critical),
      CreateDiagnosticInfo(IssueSeverity.Fatal)
      ];

      var actual = diagnostics.CountCriticalOrHigher();

      Assert.Equal(3, actual);
   }

   [Fact]
   public void CountCriticalOrHigher_WhenNoMatchingDiagnostics_ReturnsZero()
   {
      IEnumerable<DiagnosticInfo> diagnostics =
      [
         CreateDiagnosticInfo(IssueSeverity.None),
      CreateDiagnosticInfo(IssueSeverity.Trace),
      CreateDiagnosticInfo(IssueSeverity.Debug),
      CreateDiagnosticInfo(IssueSeverity.Information),
      CreateDiagnosticInfo(IssueSeverity.Warning),
      CreateDiagnosticInfo(IssueSeverity.Error)
      ];

      var actual = diagnostics.CountCriticalOrHigher();

      Assert.Equal(0, actual);
   }
   [Fact]
   public void GetHighestSeverity_WhenDiagnosticsIsNull_ThrowsArgumentNullException()
   {
      IEnumerable<DiagnosticInfo>? diagnostics = null;

      Assert.Throws<ArgumentNullException>(() =>
         diagnostics!.GetHighestSeverity());
   }

   [Fact]
   public void GetHighestSeverity_WhenDiagnosticsAreEmpty_ReturnsNone()
   {
      IEnumerable<DiagnosticInfo> diagnostics = [];

      var actual = diagnostics.GetHighestSeverity();

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
   public void GetHighestSeverity_WhenCollectionContainsSingleDiagnostic_ReturnsDiagnosticSeverity(
      IssueSeverity severity)
   {
      IEnumerable<DiagnosticInfo> diagnostics =
      [
         CreateDiagnosticInfo(severity)
      ];

      var actual = diagnostics.GetHighestSeverity();

      Assert.Equal(severity, actual);
   }

   [Fact]
   public void GetHighestSeverity_WhenCollectionContainsMultipleDiagnostics_ReturnsHighestSeverity()
   {
      IEnumerable<DiagnosticInfo> diagnostics =
      [
         CreateDiagnosticInfo(IssueSeverity.Information),
      CreateDiagnosticInfo(IssueSeverity.Warning),
      CreateDiagnosticInfo(IssueSeverity.Error),
      CreateDiagnosticInfo(IssueSeverity.Debug)
      ];

      var actual = diagnostics.GetHighestSeverity();

      Assert.Equal(IssueSeverity.Error, actual);
   }

   [Fact]
   public void GetHighestSeverity_WhenCollectionContainsFatal_ReturnsFatal()
   {
      IEnumerable<DiagnosticInfo> diagnostics =
      [
         CreateDiagnosticInfo(IssueSeverity.Information),
      CreateDiagnosticInfo(IssueSeverity.Warning),
      CreateDiagnosticInfo(IssueSeverity.Fatal),
      CreateDiagnosticInfo(IssueSeverity.Critical),
      CreateDiagnosticInfo(IssueSeverity.Error)
      ];

      var actual = diagnostics.GetHighestSeverity();

      Assert.Equal(IssueSeverity.Fatal, actual);
   }

   [Fact]
   public void GetHighestSeverity_WhenCollectionContainsOnlyLowSeverityDiagnostics_ReturnsHighestLowSeverity()
   {
      IEnumerable<DiagnosticInfo> diagnostics =
      [
         CreateDiagnosticInfo(IssueSeverity.None),
      CreateDiagnosticInfo(IssueSeverity.Trace),
      CreateDiagnosticInfo(IssueSeverity.Debug),
      CreateDiagnosticInfo(IssueSeverity.Information)
      ];

      var actual = diagnostics.GetHighestSeverity();

      Assert.Equal(IssueSeverity.Information, actual);
   }

   [Fact]
   public void GetHighestSeverity_WhenHighestSeverityAppearsMultipleTimes_ReturnsThatSeverity()
   {
      IEnumerable<DiagnosticInfo> diagnostics =
      [
         CreateDiagnosticInfo(IssueSeverity.Warning),
      CreateDiagnosticInfo(IssueSeverity.Error),
      CreateDiagnosticInfo(IssueSeverity.Error),
      CreateDiagnosticInfo(IssueSeverity.Information)
      ];

      var actual = diagnostics.GetHighestSeverity();

      Assert.Equal(IssueSeverity.Error, actual);
   }

   [Fact]
   public void GetHighestSeverity_DoesNotDependOnItemOrder()
   {
      IEnumerable<DiagnosticInfo> firstOrder =
      [
         CreateDiagnosticInfo(IssueSeverity.Fatal),
      CreateDiagnosticInfo(IssueSeverity.Information),
      CreateDiagnosticInfo(IssueSeverity.Warning)
      ];

      IEnumerable<DiagnosticInfo> secondOrder =
      [
         CreateDiagnosticInfo(IssueSeverity.Information),
      CreateDiagnosticInfo(IssueSeverity.Warning),
      CreateDiagnosticInfo(IssueSeverity.Fatal)
      ];

      var firstActual = firstOrder.GetHighestSeverity();
      var secondActual = secondOrder.GetHighestSeverity();

      Assert.Equal(IssueSeverity.Fatal, firstActual);
      Assert.Equal(IssueSeverity.Fatal, secondActual);
   }

   [Fact]
   public void HasOnlyInformationalOrLowerDiagnostics_WhenDiagnosticsIsNull_ThrowsArgumentNullException()
   {
      IEnumerable<DiagnosticInfo>? diagnostics = null;

      Assert.Throws<ArgumentNullException>(() =>
         diagnostics!.HasOnlyInformationalOrLowerDiagnostics());
   }

   [Fact]
   public void HasOnlyInformationalOrLowerDiagnostics_WhenDiagnosticsAreEmpty_ReturnsTrue()
   {
      IEnumerable<DiagnosticInfo> diagnostics = [];

      var actual = diagnostics.HasOnlyInformationalOrLowerDiagnostics();

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
      IEnumerable<DiagnosticInfo> diagnostics =
      [
         CreateDiagnosticInfo(severity)
      ];

      var actual = diagnostics.HasOnlyInformationalOrLowerDiagnostics();

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
      IEnumerable<DiagnosticInfo> diagnostics =
      [
         CreateDiagnosticInfo(severity)
      ];

      var actual = diagnostics.HasOnlyInformationalOrLowerDiagnostics();

      Assert.False(actual);
   }

   [Fact]
   public void HasOnlyInformationalOrLowerDiagnostics_WhenAllDiagnosticsAreInformationalOrLower_ReturnsTrue()
   {
      IEnumerable<DiagnosticInfo> diagnostics =
      [
         CreateDiagnosticInfo(IssueSeverity.None),
      CreateDiagnosticInfo(IssueSeverity.Trace),
      CreateDiagnosticInfo(IssueSeverity.Debug),
      CreateDiagnosticInfo(IssueSeverity.Information)
      ];

      var actual = diagnostics.HasOnlyInformationalOrLowerDiagnostics();

      Assert.True(actual);
   }

   [Fact]
   public void HasOnlyInformationalOrLowerDiagnostics_WhenAnyDiagnosticIsWarning_ReturnsFalse()
   {
      IEnumerable<DiagnosticInfo> diagnostics =
      [
         CreateDiagnosticInfo(IssueSeverity.None),
      CreateDiagnosticInfo(IssueSeverity.Information),
      CreateDiagnosticInfo(IssueSeverity.Warning)
      ];

      var actual = diagnostics.HasOnlyInformationalOrLowerDiagnostics();

      Assert.False(actual);
   }

   [Fact]
   public void HasOnlyInformationalOrLowerDiagnostics_WhenAnyDiagnosticIsError_ReturnsFalse()
   {
      IEnumerable<DiagnosticInfo> diagnostics =
      [
         CreateDiagnosticInfo(IssueSeverity.Information),
      CreateDiagnosticInfo(IssueSeverity.Error)
      ];

      var actual = diagnostics.HasOnlyInformationalOrLowerDiagnostics();

      Assert.False(actual);
   }

   [Fact]
   public void HasOnlyInformationalOrLowerDiagnostics_WhenAnyDiagnosticIsCritical_ReturnsFalse()
   {
      IEnumerable<DiagnosticInfo> diagnostics =
      [
         CreateDiagnosticInfo(IssueSeverity.Information),
      CreateDiagnosticInfo(IssueSeverity.Critical)
      ];

      var actual = diagnostics.HasOnlyInformationalOrLowerDiagnostics();

      Assert.False(actual);
   }

   [Fact]
   public void HasOnlyInformationalOrLowerDiagnostics_WhenAnyDiagnosticIsFatal_ReturnsFalse()
   {
      IEnumerable<DiagnosticInfo> diagnostics =
      [
         CreateDiagnosticInfo(IssueSeverity.Information),
      CreateDiagnosticInfo(IssueSeverity.Fatal)
      ];

      var actual = diagnostics.HasOnlyInformationalOrLowerDiagnostics();

      Assert.False(actual);
   }
}