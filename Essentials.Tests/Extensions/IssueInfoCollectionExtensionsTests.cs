using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Extensions;
using Afrowave.Toolbox.Essentials.Issues;

namespace Afrowave.Toolbox.Essentials.Tests.Extensions;

public sealed class IssueInfoCollectionExtensionsTests
{
   [Fact]
   public void HasErrors_WhenIssuesIsNull_ThrowsArgumentNullException()
   {
      IEnumerable<IssueInfo>? issues = null;

      Assert.Throws<ArgumentNullException>(() => issues!.HasErrors());
   }

   [Fact]
   public void HasErrors_WhenCollectionIsEmpty_ReturnsFalse()
   {
      var issues = Array.Empty<IssueInfo>();

      var actual = issues.HasErrors();

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
      var issues = new[]
      {
            CreateIssue(severity)
        };

      var actual = issues.HasErrors();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasWarningsOrErrors_WhenIssuesIsNull_ThrowsArgumentNullException()
   {
      IEnumerable<IssueInfo>? issues = null;

      Assert.Throws<ArgumentNullException>(() => issues!.HasWarningsOrErrors());
   }

   [Fact]
   public void HasWarningsOrErrors_WhenCollectionIsEmpty_ReturnsFalse()
   {
      var issues = Array.Empty<IssueInfo>();

      var actual = issues.HasWarningsOrErrors();

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
      var issues = new[]
      {
            CreateIssue(severity)
        };

      var actual = issues.HasWarningsOrErrors();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void Errors_WhenIssuesIsNull_ThrowsArgumentNullException()
   {
      IEnumerable<IssueInfo>? issues = null;

      Assert.Throws<ArgumentNullException>(() => issues!.Errors().ToArray());
   }

   [Fact]
   public void Errors_ReturnsOnlyErrorOrHigherSeverityIssues()
   {
      var issues = new[]
      {
            CreateIssue(IssueSeverity.None, "none"),
            CreateIssue(IssueSeverity.Trace, "trace"),
            CreateIssue(IssueSeverity.Debug, "debug"),
            CreateIssue(IssueSeverity.Information, "information"),
            CreateIssue(IssueSeverity.Warning, "warning"),
            CreateIssue(IssueSeverity.Error, "error"),
            CreateIssue(IssueSeverity.Critical, "critical"),
            CreateIssue(IssueSeverity.Fatal, "fatal")
        };

      var actual = issues.Errors().Select(issue => issue.Code).ToArray();

      Assert.Equal(
          new[] { "error", "critical", "fatal" },
          actual);
   }

   [Fact]
   public void Warnings_WhenIssuesIsNull_ThrowsArgumentNullException()
   {
      IEnumerable<IssueInfo>? issues = null;

      Assert.Throws<ArgumentNullException>(() => issues!.Warnings().ToArray());
   }

   [Fact]
   public void Warnings_ReturnsOnlyWarningSeverityIssues()
   {
      var issues = new[]
      {
            CreateIssue(IssueSeverity.None, "none"),
            CreateIssue(IssueSeverity.Trace, "trace"),
            CreateIssue(IssueSeverity.Debug, "debug"),
            CreateIssue(IssueSeverity.Information, "information"),
            CreateIssue(IssueSeverity.Warning, "warning"),
            CreateIssue(IssueSeverity.Error, "error"),
            CreateIssue(IssueSeverity.Critical, "critical"),
            CreateIssue(IssueSeverity.Fatal, "fatal")
        };

      var actual = issues.Warnings().Select(issue => issue.Code).ToArray();

      Assert.Equal(
          new[] { "warning" },
          actual);
   }

   [Fact]
   public void Informational_WhenIssuesIsNull_ThrowsArgumentNullException()
   {
      IEnumerable<IssueInfo>? issues = null;

      Assert.Throws<ArgumentNullException>(() => issues!.Informational().ToArray());
   }

   [Fact]
   public void Informational_ReturnsOnlyInformationOrLowerSeverityIssues()
   {
      var issues = new[]
      {
            CreateIssue(IssueSeverity.None, "none"),
            CreateIssue(IssueSeverity.Trace, "trace"),
            CreateIssue(IssueSeverity.Debug, "debug"),
            CreateIssue(IssueSeverity.Information, "information"),
            CreateIssue(IssueSeverity.Warning, "warning"),
            CreateIssue(IssueSeverity.Error, "error"),
            CreateIssue(IssueSeverity.Critical, "critical"),
            CreateIssue(IssueSeverity.Fatal, "fatal")
        };

      var actual = issues.Informational().Select(issue => issue.Code).ToArray();

      Assert.Equal(
          new[] { "none", "trace", "debug", "information" },
          actual);
   }

   [Fact]
   public void WithSeverity_WhenIssuesIsNull_ThrowsArgumentNullException()
   {
      IEnumerable<IssueInfo>? issues = null;

      Assert.Throws<ArgumentNullException>(() => issues!.WithSeverity(IssueSeverity.Warning).ToArray());
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
   public void WithSeverity_ReturnsOnlyIssuesWithSpecifiedSeverity(
       IssueSeverity severity,
       string expectedCode)
   {
      var issues = new[]
      {
            CreateIssue(IssueSeverity.None, "none"),
            CreateIssue(IssueSeverity.Trace, "trace"),
            CreateIssue(IssueSeverity.Debug, "debug"),
            CreateIssue(IssueSeverity.Information, "information"),
            CreateIssue(IssueSeverity.Warning, "warning"),
            CreateIssue(IssueSeverity.Error, "error"),
            CreateIssue(IssueSeverity.Critical, "critical"),
            CreateIssue(IssueSeverity.Fatal, "fatal")
        };

      var actual = issues
          .WithSeverity(severity)
          .Select(issue => issue.Code)
          .ToArray();

      Assert.Equal(
          new[] { expectedCode },
          actual);
   }

   [Fact]
   public void HasAnyIssues_WhenIssuesIsNull_ThrowsArgumentNullException()
   {
      IReadOnlyList<IssueInfo>? issues = null;

      Assert.Throws<ArgumentNullException>(() =>
          issues!.HasAnyIssues());
   }

   [Fact]
   public void HasAnyIssues_WhenIssuesAreEmpty_ReturnsFalse()
   {
      IReadOnlyList<IssueInfo> issues = [];

      var actual = issues.HasAnyIssues();

      Assert.False(actual);
   }

   [Fact]
   public void HasAnyIssues_WhenIssuesContainItem_ReturnsTrue()
   {
      IReadOnlyList<IssueInfo> issues =
      [
         CreateIssue(IssueSeverity.Warning)
      ];

      var actual = issues.HasAnyIssues();

      Assert.True(actual);
   }

   [Fact]
   public void HasCriticalOrFatalIssues_WhenIssuesIsNull_ThrowsArgumentNullException()
   {
      IReadOnlyList<IssueInfo>? issues = null;

      Assert.Throws<ArgumentNullException>(() =>
          issues!.HasCriticalOrFatalIssues());
   }

   [Fact]
   public void HasCriticalOrFatalIssues_WhenIssuesAreEmpty_ReturnsFalse()
   {
      IReadOnlyList<IssueInfo> issues = [];

      var actual = issues.HasCriticalOrFatalIssues();

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
   public void HasCriticalOrFatalIssues_ReturnsExpectedResult(
       IssueSeverity severity,
       bool expected)
   {
      IReadOnlyList<IssueInfo> issues =
      [
         CreateIssue(severity)
      ];

      var actual = issues.HasCriticalOrFatalIssues();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasIssueWithSeverity_WhenIssuesIsNull_ThrowsArgumentNullException()
   {
      IReadOnlyList<IssueInfo>? issues = null;

      Assert.Throws<ArgumentNullException>(() =>
          issues!.HasIssueWithSeverity(IssueSeverity.Warning));
   }

   [Fact]
   public void HasIssueWithSeverity_WhenIssuesAreEmpty_ReturnsFalse()
   {
      IReadOnlyList<IssueInfo> issues = [];

      var actual = issues.HasIssueWithSeverity(IssueSeverity.Warning);

      Assert.False(actual);
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
   public void HasIssueWithSeverity_WhenMatchingSeverityExists_ReturnsTrue(
       IssueSeverity severity)
   {
      IReadOnlyList<IssueInfo> issues =
      [
         CreateIssue(severity)
      ];

      var actual = issues.HasIssueWithSeverity(severity);

      Assert.True(actual);
   }

   [Fact]
   public void HasIssueWithSeverity_WhenMatchingSeverityDoesNotExist_ReturnsFalse()
   {
      IReadOnlyList<IssueInfo> issues =
      [
         CreateIssue(IssueSeverity.Information),
         CreateIssue(IssueSeverity.Warning)
      ];

      var actual = issues.HasIssueWithSeverity(IssueSeverity.Error);

      Assert.False(actual);
   }

   private static IssueInfo CreateIssue(
       IssueSeverity severity,
       string? code = null)
   {
      return new IssueInfo
      {
         Code = code ?? severity.ToString().ToLowerInvariant(),
         Message = $"Test issue with severity {severity}.",
         Severity = severity
      };
   }
   [Fact]
public void AppendIssue_WhenIssuesIsNull_ThrowsArgumentNullException()
{
    IReadOnlyList<IssueInfo>? issues = null;

    var issue = IssueInfoFactory.Warning(
        "AFW_WARNING",
        "Warning message.");

    Assert.Throws<ArgumentNullException>(() =>
        issues!.AppendIssue(issue));
}

[Fact]
public void AppendIssue_WhenIssueIsNull_ThrowsArgumentNullException()
{
    IReadOnlyList<IssueInfo> issues = [];

    IssueInfo? issue = null;

    Assert.Throws<ArgumentNullException>(() =>
        issues.AppendIssue(issue!));
}

[Fact]
public void AppendIssue_WhenIssuesAreEmpty_ReturnsListWithAppendedIssue()
{
    IReadOnlyList<IssueInfo> issues = [];

    var issue = IssueInfoFactory.Warning(
        "AFW_WARNING",
        "Warning message.");

    var result = issues.AppendIssue(issue);

    Assert.Single(result);
    Assert.Same(issue, result[0]);
}

[Fact]
public void AppendIssue_WhenIssuesContainItems_ReturnsListWithOriginalItemsAndAppendedIssue()
{
    var first = IssueInfoFactory.Information(
        "AFW_INFO",
        "Information message.");

    var second = IssueInfoFactory.Warning(
        "AFW_WARNING",
        "Warning message.");

    IReadOnlyList<IssueInfo> issues =
    [
        first
    ];

    var result = issues.AppendIssue(second);

    Assert.Equal(2, result.Count);
    Assert.Same(first, result[0]);
    Assert.Same(second, result[1]);
}

[Fact]
public void AppendIssue_ReturnsNewList()
{
    var first = IssueInfoFactory.Information(
        "AFW_INFO",
        "Information message.");

    var second = IssueInfoFactory.Warning(
        "AFW_WARNING",
        "Warning message.");

    IReadOnlyList<IssueInfo> issues =
    [
        first
    ];

    var result = issues.AppendIssue(second);

    Assert.NotSame(issues, result);
}

[Fact]
public void AppendIssue_DoesNotModifyOriginalList()
{
    var first = IssueInfoFactory.Information(
        "AFW_INFO",
        "Information message.");

    var second = IssueInfoFactory.Warning(
        "AFW_WARNING",
        "Warning message.");

    IReadOnlyList<IssueInfo> issues =
    [
        first
    ];

    var result = issues.AppendIssue(second);

    Assert.Single(issues);
    Assert.Same(first, issues[0]);

    Assert.Equal(2, result.Count);
    Assert.Same(second, result[1]);
}

[Fact]
public void AppendIssues_WhenIssuesIsNull_ThrowsArgumentNullException()
{
    IReadOnlyList<IssueInfo>? issues = null;

    IReadOnlyList<IssueInfo> additionalIssues =
    [
        IssueInfoFactory.Warning(
            "AFW_WARNING",
            "Warning message.")
    ];

    Assert.Throws<ArgumentNullException>(() =>
        issues!.AppendIssues(additionalIssues));
}

[Fact]
public void AppendIssues_WhenAdditionalIssuesIsNull_ThrowsArgumentNullException()
{
    IReadOnlyList<IssueInfo> issues = [];

    IEnumerable<IssueInfo>? additionalIssues = null;

    Assert.Throws<ArgumentNullException>(() =>
        issues.AppendIssues(additionalIssues!));
}

[Fact]
public void AppendIssues_WhenBothCollectionsAreEmpty_ReturnsEmptyList()
{
    IReadOnlyList<IssueInfo> issues = [];
    IReadOnlyList<IssueInfo> additionalIssues = [];

    var result = issues.AppendIssues(additionalIssues);

    Assert.NotNull(result);
    Assert.Empty(result);
}

[Fact]
public void AppendIssues_WhenSourceIsEmpty_ReturnsAdditionalIssues()
{
    IReadOnlyList<IssueInfo> issues = [];

    var first = IssueInfoFactory.Warning(
        "AFW_WARNING",
        "Warning message.");

    var second = IssueInfoFactory.Error(
        "AFW_ERROR",
        "Error message.");

    IReadOnlyList<IssueInfo> additionalIssues =
    [
        first,
        second
    ];

    var result = issues.AppendIssues(additionalIssues);

    Assert.Equal(2, result.Count);
    Assert.Same(first, result[0]);
    Assert.Same(second, result[1]);
}

[Fact]
public void AppendIssues_WhenAdditionalIssuesAreEmpty_ReturnsOriginalIssues()
{
    var first = IssueInfoFactory.Information(
        "AFW_INFO",
        "Information message.");

    IReadOnlyList<IssueInfo> issues =
    [
        first
    ];

    IReadOnlyList<IssueInfo> additionalIssues = [];

    var result = issues.AppendIssues(additionalIssues);

    Assert.Single(result);
    Assert.Same(first, result[0]);
}

[Fact]
public void AppendIssues_ReturnsListWithOriginalAndAdditionalIssues()
{
    var first = IssueInfoFactory.Information(
        "AFW_INFO",
        "Information message.");

    var second = IssueInfoFactory.Warning(
        "AFW_WARNING",
        "Warning message.");

    var third = IssueInfoFactory.Error(
        "AFW_ERROR",
        "Error message.");

    IReadOnlyList<IssueInfo> issues =
    [
        first
    ];

    IReadOnlyList<IssueInfo> additionalIssues =
    [
        second,
        third
    ];

    var result = issues.AppendIssues(additionalIssues);

    Assert.Equal(3, result.Count);
    Assert.Same(first, result[0]);
    Assert.Same(second, result[1]);
    Assert.Same(third, result[2]);
}

[Fact]
public void AppendIssues_ReturnsNewList()
{
    var first = IssueInfoFactory.Information(
        "AFW_INFO",
        "Information message.");

    IReadOnlyList<IssueInfo> issues =
    [
        first
    ];

    IReadOnlyList<IssueInfo> additionalIssues = [];

    var result = issues.AppendIssues(additionalIssues);

    Assert.NotSame(issues, result);
}

    [Fact]
    public void AppendIssues_ReturnsSnapshotOfAdditionalIssues()
    {
        var first = IssueInfoFactory.Information(
            "AFW_INFO",
            "Information message.");

        var second = IssueInfoFactory.Warning(
            "AFW_WARNING",
            "Warning message.");

        IReadOnlyList<IssueInfo> issues =
        [
            first
        ];

        var additionalIssues = new List<IssueInfo>
    {
        second
    };

        var result = issues.AppendIssues(additionalIssues);

        additionalIssues.Clear();

        Assert.Equal(2, result.Count);
        Assert.Same(first, result[0]);
        Assert.Same(second, result[1]);
    }
    [Fact]
    public void HasIssueCode_WhenIssuesIsNull_ThrowsArgumentNullException()
    {
        IReadOnlyList<IssueInfo>? issues = null;

        Assert.Throws<ArgumentNullException>(() =>
            issues!.HasIssueCode("AFW_WARNING"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void HasIssueCode_WhenCodeIsInvalid_ThrowsArgumentException(
        string? code)
    {
        IReadOnlyList<IssueInfo> issues = [];

        Assert.ThrowsAny<ArgumentException>(() =>
            issues.HasIssueCode(code!));
    }

    [Fact]
    public void HasIssueCode_WhenIssuesAreEmpty_ReturnsFalse()
    {
        IReadOnlyList<IssueInfo> issues = [];

        var actual = issues.HasIssueCode("AFW_WARNING");

        Assert.False(actual);
    }

    [Fact]
    public void HasIssueCode_WhenCodeDoesNotExist_ReturnsFalse()
    {
        IReadOnlyList<IssueInfo> issues =
        [
            IssueInfoFactory.Warning(
            "AFW_WARNING",
            "Warning message.")
        ];

        var actual = issues.HasIssueCode("AFW_ERROR");

        Assert.False(actual);
    }

    [Fact]
    public void HasIssueCode_WhenCodeExists_ReturnsTrue()
    {
        IReadOnlyList<IssueInfo> issues =
        [
            IssueInfoFactory.Warning(
            "AFW_WARNING",
            "Warning message.")
        ];

        var actual = issues.HasIssueCode("AFW_WARNING");

        Assert.True(actual);
    }

    [Fact]
    public void HasIssueCode_UsesCaseInsensitiveComparison()
    {
        IReadOnlyList<IssueInfo> issues =
        [
            IssueInfoFactory.Warning(
            "AFW_WARNING",
            "Warning message.")
        ];

        var actual = issues.HasIssueCode("afw_warning");

        Assert.True(actual);
    }

    [Fact]
    public void TryGetIssueByCode_WhenIssuesIsNull_ThrowsArgumentNullException()
    {
        IReadOnlyList<IssueInfo>? issues = null;

        Assert.Throws<ArgumentNullException>(() =>
            issues!.TryGetIssueByCode("AFW_WARNING", out _));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryGetIssueByCode_WhenCodeIsInvalid_ThrowsArgumentException(
        string? code)
    {
        IReadOnlyList<IssueInfo> issues = [];

        Assert.ThrowsAny<ArgumentException>(() =>
            issues.TryGetIssueByCode(code!, out _));
    }

    [Fact]
    public void TryGetIssueByCode_WhenIssuesAreEmpty_ReturnsFalseAndNullIssue()
    {
        IReadOnlyList<IssueInfo> issues = [];

        var actual = issues.TryGetIssueByCode(
            "AFW_WARNING",
            out var issue);

        Assert.False(actual);
        Assert.Null(issue);
    }

    [Fact]
    public void TryGetIssueByCode_WhenCodeDoesNotExist_ReturnsFalseAndNullIssue()
    {
        IReadOnlyList<IssueInfo> issues =
        [
            IssueInfoFactory.Warning(
            "AFW_WARNING",
            "Warning message.")
        ];

        var actual = issues.TryGetIssueByCode(
            "AFW_ERROR",
            out var issue);

        Assert.False(actual);
        Assert.Null(issue);
    }

    [Fact]
    public void TryGetIssueByCode_WhenCodeExists_ReturnsTrueAndIssue()
    {
        var expectedIssue = IssueInfoFactory.Warning(
            "AFW_WARNING",
            "Warning message.");

        IReadOnlyList<IssueInfo> issues =
        [
            expectedIssue
        ];

        var actual = issues.TryGetIssueByCode(
            "AFW_WARNING",
            out var issue);

        Assert.True(actual);
        Assert.Same(expectedIssue, issue);
    }

    [Fact]
    public void TryGetIssueByCode_UsesCaseInsensitiveComparison()
    {
        var expectedIssue = IssueInfoFactory.Warning(
            "AFW_WARNING",
            "Warning message.");

        IReadOnlyList<IssueInfo> issues =
        [
            expectedIssue
        ];

        var actual = issues.TryGetIssueByCode(
            "afw_warning",
            out var issue);

        Assert.True(actual);
        Assert.Same(expectedIssue, issue);
    }

    [Fact]
    public void TryGetIssueByCode_WhenMultipleIssuesMatch_ReturnsFirstMatch()
    {
        var first = IssueInfoFactory.Warning(
            "AFW_DUPLICATE",
            "First warning.");

        var second = IssueInfoFactory.Error(
            "AFW_DUPLICATE",
            "Second error.");

        IReadOnlyList<IssueInfo> issues =
        [
            first,
        second
        ];

        var actual = issues.TryGetIssueByCode(
            "AFW_DUPLICATE",
            out var issue);

        Assert.True(actual);
        Assert.Same(first, issue);
    }
    [Fact]
    public void WhereSeverity_WhenIssuesIsNull_ThrowsArgumentNullException()
    {
        IReadOnlyList<IssueInfo>? issues = null;

        Assert.Throws<ArgumentNullException>(() =>
            issues!.WhereSeverity(IssueSeverity.Warning));
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
      var matching = CreateIssue(severity);

      IReadOnlyList<IssueInfo> issues = Enum
          .GetValues<IssueSeverity>()
          .Where(currentSeverity => currentSeverity != severity)
          .Select(currentSeverity => CreateIssue(currentSeverity))
          .Append(matching)
          .ToArray();

      var result = issues.WhereSeverity(severity);

      Assert.Single(result);
      Assert.Same(matching, result[0]);
   }

    [Fact]
    public void WhereSeverity_WhenNoMatchingSeverity_ReturnsEmptyList()
    {
        IReadOnlyList<IssueInfo> issues =
        [
            CreateIssue(IssueSeverity.Information),
        CreateIssue(IssueSeverity.Warning)
        ];

        var result = issues.WhereSeverity(IssueSeverity.Fatal);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void WhereSeverity_ReturnsNewList()
    {
        IReadOnlyList<IssueInfo> issues =
        [
            CreateIssue(IssueSeverity.Warning)
        ];

        var result = issues.WhereSeverity(IssueSeverity.Warning);

        Assert.NotSame(issues, result);
    }

    [Fact]
    public void WhereWarningOrHigher_WhenIssuesIsNull_ThrowsArgumentNullException()
    {
        IReadOnlyList<IssueInfo>? issues = null;

        Assert.Throws<ArgumentNullException>(() =>
            issues!.WhereWarningOrHigher());
    }

    [Fact]
    public void WhereWarningOrHigher_ReturnsWarningErrorCriticalAndFatalIssues()
    {
        var none = CreateIssue(IssueSeverity.None);
        var trace = CreateIssue(IssueSeverity.Trace);
        var debug = CreateIssue(IssueSeverity.Debug);
        var information = CreateIssue(IssueSeverity.Information);
        var warning = CreateIssue(IssueSeverity.Warning);
        var error = CreateIssue(IssueSeverity.Error);
        var critical = CreateIssue(IssueSeverity.Critical);
        var fatal = CreateIssue(IssueSeverity.Fatal);

        IReadOnlyList<IssueInfo> issues =
        [
            none,
        trace,
        debug,
        information,
        warning,
        error,
        critical,
        fatal
        ];

        var result = issues.WhereWarningOrHigher();

        Assert.Equal(4, result.Count);
        Assert.Same(warning, result[0]);
        Assert.Same(error, result[1]);
        Assert.Same(critical, result[2]);
        Assert.Same(fatal, result[3]);
    }

    [Fact]
    public void WhereWarningOrHigher_WhenNoMatchingIssues_ReturnsEmptyList()
    {
        IReadOnlyList<IssueInfo> issues =
        [
            CreateIssue(IssueSeverity.None),
        CreateIssue(IssueSeverity.Trace),
        CreateIssue(IssueSeverity.Debug),
        CreateIssue(IssueSeverity.Information)
        ];

        var result = issues.WhereWarningOrHigher();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void WhereErrorOrHigher_WhenIssuesIsNull_ThrowsArgumentNullException()
    {
        IReadOnlyList<IssueInfo>? issues = null;

        Assert.Throws<ArgumentNullException>(() =>
            issues!.WhereErrorOrHigher());
    }

    [Fact]
    public void WhereErrorOrHigher_ReturnsErrorCriticalAndFatalIssues()
    {
        var warning = CreateIssue(IssueSeverity.Warning);
        var error = CreateIssue(IssueSeverity.Error);
        var critical = CreateIssue(IssueSeverity.Critical);
        var fatal = CreateIssue(IssueSeverity.Fatal);

        IReadOnlyList<IssueInfo> issues =
        [
            CreateIssue(IssueSeverity.None),
        CreateIssue(IssueSeverity.Trace),
        CreateIssue(IssueSeverity.Debug),
        CreateIssue(IssueSeverity.Information),
        warning,
        error,
        critical,
        fatal
        ];

        var result = issues.WhereErrorOrHigher();

        Assert.Equal(3, result.Count);
        Assert.Same(error, result[0]);
        Assert.Same(critical, result[1]);
        Assert.Same(fatal, result[2]);
    }

    [Fact]
    public void WhereErrorOrHigher_WhenNoMatchingIssues_ReturnsEmptyList()
    {
        IReadOnlyList<IssueInfo> issues =
        [
            CreateIssue(IssueSeverity.None),
        CreateIssue(IssueSeverity.Trace),
        CreateIssue(IssueSeverity.Debug),
        CreateIssue(IssueSeverity.Information),
        CreateIssue(IssueSeverity.Warning)
        ];

        var result = issues.WhereErrorOrHigher();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void WhereCriticalOrHigher_WhenIssuesIsNull_ThrowsArgumentNullException()
    {
        IReadOnlyList<IssueInfo>? issues = null;

        Assert.Throws<ArgumentNullException>(() =>
            issues!.WhereCriticalOrHigher());
    }

    [Fact]
    public void WhereCriticalOrHigher_ReturnsCriticalAndFatalIssues()
    {
        var critical = CreateIssue(IssueSeverity.Critical);
        var fatal = CreateIssue(IssueSeverity.Fatal);

        IReadOnlyList<IssueInfo> issues =
        [
            CreateIssue(IssueSeverity.None),
        CreateIssue(IssueSeverity.Trace),
        CreateIssue(IssueSeverity.Debug),
        CreateIssue(IssueSeverity.Information),
        CreateIssue(IssueSeverity.Warning),
        CreateIssue(IssueSeverity.Error),
        critical,
        fatal
        ];

        var result = issues.WhereCriticalOrHigher();

        Assert.Equal(2, result.Count);
        Assert.Same(critical, result[0]);
        Assert.Same(fatal, result[1]);
    }

    [Fact]
    public void WhereCriticalOrHigher_WhenNoMatchingIssues_ReturnsEmptyList()
    {
        IReadOnlyList<IssueInfo> issues =
        [
            CreateIssue(IssueSeverity.None),
        CreateIssue(IssueSeverity.Trace),
        CreateIssue(IssueSeverity.Debug),
        CreateIssue(IssueSeverity.Information),
        CreateIssue(IssueSeverity.Warning),
        CreateIssue(IssueSeverity.Error)
        ];

        var result = issues.WhereCriticalOrHigher();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void FilteringMethods_PreserveOriginalOrder()
    {
        var first = IssueInfoFactory.Error(
            "AFW_FIRST",
            "First error.");

        var second = IssueInfoFactory.Fatal(
            "AFW_SECOND",
            "Second fatal.");

        var third = IssueInfoFactory.Critical(
            "AFW_THIRD",
            "Third critical.");

        IReadOnlyList<IssueInfo> issues =
        [
            first,
        CreateIssue(IssueSeverity.Information),
        second,
        CreateIssue(IssueSeverity.Warning),
        third
        ];

        var result = issues.WhereErrorOrHigher();

        Assert.Equal(3, result.Count);
        Assert.Same(first, result[0]);
        Assert.Same(second, result[1]);
        Assert.Same(third, result[2]);
    }
    [Fact]
    public void CountSeverity_WhenIssuesIsNull_ThrowsArgumentNullException()
    {
        IReadOnlyList<IssueInfo>? issues = null;

        Assert.Throws<ArgumentNullException>(() =>
            issues!.CountSeverity(IssueSeverity.Warning));
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
    public void CountSeverity_WhenIssuesAreEmpty_ReturnsZero(
        IssueSeverity severity)
    {
        IReadOnlyList<IssueInfo> issues = [];

        var result = issues.CountSeverity(severity);

        Assert.Equal(0, result);
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
        IReadOnlyList<IssueInfo> issues =
        [
            CreateIssue(IssueSeverity.None),
        CreateIssue(IssueSeverity.Trace),
        CreateIssue(IssueSeverity.Debug),
        CreateIssue(IssueSeverity.Information),
        CreateIssue(IssueSeverity.Information),
        CreateIssue(IssueSeverity.Warning),
        CreateIssue(IssueSeverity.Warning),
        CreateIssue(IssueSeverity.Warning),
        CreateIssue(IssueSeverity.Error),
        CreateIssue(IssueSeverity.Error),
        CreateIssue(IssueSeverity.Critical),
        CreateIssue(IssueSeverity.Fatal)
        ];

        var result = issues.CountSeverity(severity);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void CountWarningOrHigher_WhenIssuesIsNull_ThrowsArgumentNullException()
    {
        IReadOnlyList<IssueInfo>? issues = null;

        Assert.Throws<ArgumentNullException>(() =>
            issues!.CountWarningOrHigher());
    }

    [Fact]
    public void CountWarningOrHigher_WhenIssuesAreEmpty_ReturnsZero()
    {
        IReadOnlyList<IssueInfo> issues = [];

        var result = issues.CountWarningOrHigher();

        Assert.Equal(0, result);
    }

    [Fact]
    public void CountWarningOrHigher_ReturnsExpectedCount()
    {
        IReadOnlyList<IssueInfo> issues =
        [
            CreateIssue(IssueSeverity.None),
        CreateIssue(IssueSeverity.Trace),
        CreateIssue(IssueSeverity.Debug),
        CreateIssue(IssueSeverity.Information),
        CreateIssue(IssueSeverity.Warning),
        CreateIssue(IssueSeverity.Warning),
        CreateIssue(IssueSeverity.Error),
        CreateIssue(IssueSeverity.Critical),
        CreateIssue(IssueSeverity.Fatal)
        ];

        var result = issues.CountWarningOrHigher();

        Assert.Equal(5, result);
    }

    [Fact]
    public void CountWarningOrHigher_WhenNoMatchingIssues_ReturnsZero()
    {
        IReadOnlyList<IssueInfo> issues =
        [
            CreateIssue(IssueSeverity.None),
        CreateIssue(IssueSeverity.Trace),
        CreateIssue(IssueSeverity.Debug),
        CreateIssue(IssueSeverity.Information)
        ];

        var result = issues.CountWarningOrHigher();

        Assert.Equal(0, result);
    }

    [Fact]
    public void CountErrorOrHigher_WhenIssuesIsNull_ThrowsArgumentNullException()
    {
        IReadOnlyList<IssueInfo>? issues = null;

        Assert.Throws<ArgumentNullException>(() =>
            issues!.CountErrorOrHigher());
    }

    [Fact]
    public void CountErrorOrHigher_WhenIssuesAreEmpty_ReturnsZero()
    {
        IReadOnlyList<IssueInfo> issues = [];

        var result = issues.CountErrorOrHigher();

        Assert.Equal(0, result);
    }

    [Fact]
    public void CountErrorOrHigher_ReturnsExpectedCount()
    {
        IReadOnlyList<IssueInfo> issues =
        [
            CreateIssue(IssueSeverity.None),
        CreateIssue(IssueSeverity.Trace),
        CreateIssue(IssueSeverity.Debug),
        CreateIssue(IssueSeverity.Information),
        CreateIssue(IssueSeverity.Warning),
        CreateIssue(IssueSeverity.Error),
        CreateIssue(IssueSeverity.Error),
        CreateIssue(IssueSeverity.Critical),
        CreateIssue(IssueSeverity.Fatal)
        ];

        var result = issues.CountErrorOrHigher();

        Assert.Equal(4, result);
    }

    [Fact]
    public void CountErrorOrHigher_WhenNoMatchingIssues_ReturnsZero()
    {
        IReadOnlyList<IssueInfo> issues =
        [
            CreateIssue(IssueSeverity.None),
        CreateIssue(IssueSeverity.Trace),
        CreateIssue(IssueSeverity.Debug),
        CreateIssue(IssueSeverity.Information),
        CreateIssue(IssueSeverity.Warning)
        ];

        var result = issues.CountErrorOrHigher();

        Assert.Equal(0, result);
    }

    [Fact]
    public void CountCriticalOrHigher_WhenIssuesIsNull_ThrowsArgumentNullException()
    {
        IReadOnlyList<IssueInfo>? issues = null;

        Assert.Throws<ArgumentNullException>(() =>
            issues!.CountCriticalOrHigher());
    }

    [Fact]
    public void CountCriticalOrHigher_WhenIssuesAreEmpty_ReturnsZero()
    {
        IReadOnlyList<IssueInfo> issues = [];

        var result = issues.CountCriticalOrHigher();

        Assert.Equal(0, result);
    }

    [Fact]
    public void CountCriticalOrHigher_ReturnsExpectedCount()
    {
        IReadOnlyList<IssueInfo> issues =
        [
            CreateIssue(IssueSeverity.None),
        CreateIssue(IssueSeverity.Trace),
        CreateIssue(IssueSeverity.Debug),
        CreateIssue(IssueSeverity.Information),
        CreateIssue(IssueSeverity.Warning),
        CreateIssue(IssueSeverity.Error),
        CreateIssue(IssueSeverity.Critical),
        CreateIssue(IssueSeverity.Critical),
        CreateIssue(IssueSeverity.Fatal)
        ];

        var result = issues.CountCriticalOrHigher();

        Assert.Equal(3, result);
    }

    [Fact]
    public void CountCriticalOrHigher_WhenNoMatchingIssues_ReturnsZero()
    {
        IReadOnlyList<IssueInfo> issues =
        [
            CreateIssue(IssueSeverity.None),
        CreateIssue(IssueSeverity.Trace),
        CreateIssue(IssueSeverity.Debug),
        CreateIssue(IssueSeverity.Information),
        CreateIssue(IssueSeverity.Warning),
        CreateIssue(IssueSeverity.Error)
        ];

        var result = issues.CountCriticalOrHigher();

        Assert.Equal(0, result);
    }
    [Fact]
    public void GetHighestSeverity_WhenIssuesIsNull_ThrowsArgumentNullException()
    {
        IReadOnlyList<IssueInfo>? issues = null;

        Assert.Throws<ArgumentNullException>(() =>
            issues!.GetHighestSeverity());
    }

    [Fact]
    public void GetHighestSeverity_WhenIssuesAreEmpty_ReturnsNone()
    {
        IReadOnlyList<IssueInfo> issues = [];

        var result = issues.GetHighestSeverity();

        Assert.Equal(IssueSeverity.None, result);
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
    public void GetHighestSeverity_WhenCollectionContainsSingleIssue_ReturnsIssueSeverity(
        IssueSeverity severity)
    {
        IReadOnlyList<IssueInfo> issues =
        [
            CreateIssue(severity)
        ];

        var result = issues.GetHighestSeverity();

        Assert.Equal(severity, result);
    }

    [Fact]
    public void GetHighestSeverity_WhenCollectionContainsMultipleIssues_ReturnsHighestSeverity()
    {
        IReadOnlyList<IssueInfo> issues =
        [
            CreateIssue(IssueSeverity.Information),
        CreateIssue(IssueSeverity.Warning),
        CreateIssue(IssueSeverity.Error),
        CreateIssue(IssueSeverity.Debug)
        ];

        var result = issues.GetHighestSeverity();

        Assert.Equal(IssueSeverity.Error, result);
    }

    [Fact]
    public void GetHighestSeverity_WhenCollectionContainsFatal_ReturnsFatal()
    {
        IReadOnlyList<IssueInfo> issues =
        [
            CreateIssue(IssueSeverity.Information),
        CreateIssue(IssueSeverity.Warning),
        CreateIssue(IssueSeverity.Fatal),
        CreateIssue(IssueSeverity.Critical),
        CreateIssue(IssueSeverity.Error)
        ];

        var result = issues.GetHighestSeverity();

        Assert.Equal(IssueSeverity.Fatal, result);
    }

    [Fact]
    public void GetHighestSeverity_WhenCollectionContainsOnlyLowSeverityIssues_ReturnsHighestLowSeverity()
    {
        IReadOnlyList<IssueInfo> issues =
        [
            CreateIssue(IssueSeverity.None),
        CreateIssue(IssueSeverity.Trace),
        CreateIssue(IssueSeverity.Debug),
        CreateIssue(IssueSeverity.Information)
        ];

        var result = issues.GetHighestSeverity();

        Assert.Equal(IssueSeverity.Information, result);
    }

    [Fact]
    public void GetHighestSeverity_WhenHighestSeverityAppearsMultipleTimes_ReturnsThatSeverity()
    {
        IReadOnlyList<IssueInfo> issues =
        [
            CreateIssue(IssueSeverity.Warning),
        CreateIssue(IssueSeverity.Error),
        CreateIssue(IssueSeverity.Error),
        CreateIssue(IssueSeverity.Information)
        ];

        var result = issues.GetHighestSeverity();

        Assert.Equal(IssueSeverity.Error, result);
    }

    [Fact]
    public void GetHighestSeverity_DoesNotDependOnItemOrder()
    {
        IReadOnlyList<IssueInfo> firstOrder =
        [
            CreateIssue(IssueSeverity.Fatal),
        CreateIssue(IssueSeverity.Information),
        CreateIssue(IssueSeverity.Warning)
        ];

        IReadOnlyList<IssueInfo> secondOrder =
        [
            CreateIssue(IssueSeverity.Information),
        CreateIssue(IssueSeverity.Warning),
        CreateIssue(IssueSeverity.Fatal)
        ];

        var firstResult = firstOrder.GetHighestSeverity();
        var secondResult = secondOrder.GetHighestSeverity();

        Assert.Equal(IssueSeverity.Fatal, firstResult);
        Assert.Equal(IssueSeverity.Fatal, secondResult);
    }

    [Fact]
    public void ToResultStatus_WhenIssuesIsNull_ThrowsArgumentNullException()
    {
        IReadOnlyList<IssueInfo>? issues = null;

        Assert.Throws<ArgumentNullException>(() =>
            issues!.ToResultStatus());
    }

    [Fact]
    public void ToResultStatus_WhenIssuesAreEmpty_ReturnsSuccess()
    {
        IReadOnlyList<IssueInfo> issues = [];

        var result = issues.ToResultStatus();

        Assert.Equal(ResultStatus.Success, result);
    }

    [Theory]
    [InlineData(IssueSeverity.None)]
    [InlineData(IssueSeverity.Trace)]
    [InlineData(IssueSeverity.Debug)]
    [InlineData(IssueSeverity.Information)]
    public void ToResultStatus_WhenHighestSeverityIsBelowWarning_ReturnsSuccess(
        IssueSeverity severity)
    {
        IReadOnlyList<IssueInfo> issues =
        [
            CreateIssue(severity)
        ];

        var result = issues.ToResultStatus();

        Assert.Equal(ResultStatus.Success, result);
    }

    [Fact]
    public void ToResultStatus_WhenHighestSeverityIsWarning_ReturnsSuccessWithWarnings()
    {
        IReadOnlyList<IssueInfo> issues =
        [
            CreateIssue(IssueSeverity.Information),
        CreateIssue(IssueSeverity.Warning),
        CreateIssue(IssueSeverity.Debug)
        ];

        var result = issues.ToResultStatus();

        Assert.Equal(ResultStatus.SuccessWithWarnings, result);
    }

    [Theory]
    [InlineData(IssueSeverity.Error)]
    [InlineData(IssueSeverity.Critical)]
    [InlineData(IssueSeverity.Fatal)]
    public void ToResultStatus_WhenHighestSeverityIsErrorOrHigher_ReturnsFailed(
        IssueSeverity severity)
    {
        IReadOnlyList<IssueInfo> issues =
        [
            CreateIssue(IssueSeverity.Information),
        CreateIssue(IssueSeverity.Warning),
        CreateIssue(severity)
        ];

        var result = issues.ToResultStatus();

        Assert.Equal(ResultStatus.Failed, result);
    }

    [Fact]
    public void ToResultStatus_WhenCollectionContainsOnlyWarnings_ReturnsSuccessWithWarnings()
    {
        IReadOnlyList<IssueInfo> issues =
        [
            CreateIssue(IssueSeverity.Warning),
        CreateIssue(IssueSeverity.Warning)
        ];

        var result = issues.ToResultStatus();

        Assert.Equal(ResultStatus.SuccessWithWarnings, result);
    }

    [Fact]
    public void ToResultStatus_WhenCollectionContainsWarningAndError_ReturnsFailed()
    {
        IReadOnlyList<IssueInfo> issues =
        [
            CreateIssue(IssueSeverity.Warning),
        CreateIssue(IssueSeverity.Error)
        ];

        var result = issues.ToResultStatus();

        Assert.Equal(ResultStatus.Failed, result);
    }

    [Fact]
    public void ToResultStatus_WhenCollectionContainsFatal_ReturnsFailed()
    {
        IReadOnlyList<IssueInfo> issues =
        [
            CreateIssue(IssueSeverity.Information),
        CreateIssue(IssueSeverity.Fatal),
        CreateIssue(IssueSeverity.Warning)
        ];

        var result = issues.ToResultStatus();

        Assert.Equal(ResultStatus.Failed, result);
    }

    [Fact]
    public void ToResultStatus_DoesNotDependOnIssueOrder()
    {
        IReadOnlyList<IssueInfo> firstOrder =
        [
            CreateIssue(IssueSeverity.Error),
        CreateIssue(IssueSeverity.Warning),
        CreateIssue(IssueSeverity.Information)
        ];

        IReadOnlyList<IssueInfo> secondOrder =
        [
            CreateIssue(IssueSeverity.Information),
        CreateIssue(IssueSeverity.Warning),
        CreateIssue(IssueSeverity.Error)
        ];

        var firstResult = firstOrder.ToResultStatus();
        var secondResult = secondOrder.ToResultStatus();

        Assert.Equal(ResultStatus.Failed, firstResult);
        Assert.Equal(ResultStatus.Failed, secondResult);
    }
    [Fact]
    public void HasOnlyInformationalOrLowerIssues_WhenIssuesIsNull_ThrowsArgumentNullException()
    {
        IReadOnlyList<IssueInfo>? issues = null;

        Assert.Throws<ArgumentNullException>(() =>
            issues!.HasOnlyInformationalOrLowerIssues());
    }

    [Fact]
    public void HasOnlyInformationalOrLowerIssues_WhenIssuesAreEmpty_ReturnsTrue()
    {
        IReadOnlyList<IssueInfo> issues = [];

        var result = issues.HasOnlyInformationalOrLowerIssues();

        Assert.True(result);
    }

    [Theory]
    [InlineData(IssueSeverity.None)]
    [InlineData(IssueSeverity.Trace)]
    [InlineData(IssueSeverity.Debug)]
    [InlineData(IssueSeverity.Information)]
    public void HasOnlyInformationalOrLowerIssues_WhenSingleIssueIsInformationalOrLower_ReturnsTrue(
        IssueSeverity severity)
    {
        IReadOnlyList<IssueInfo> issues =
        [
            CreateIssue(severity)
        ];

        var result = issues.HasOnlyInformationalOrLowerIssues();

        Assert.True(result);
    }

    [Theory]
    [InlineData(IssueSeverity.Warning)]
    [InlineData(IssueSeverity.Error)]
    [InlineData(IssueSeverity.Critical)]
    [InlineData(IssueSeverity.Fatal)]
    public void HasOnlyInformationalOrLowerIssues_WhenSingleIssueIsWarningOrHigher_ReturnsFalse(
        IssueSeverity severity)
    {
        IReadOnlyList<IssueInfo> issues =
        [
            CreateIssue(severity)
        ];

        var result = issues.HasOnlyInformationalOrLowerIssues();

        Assert.False(result);
    }

    [Fact]
    public void HasOnlyInformationalOrLowerIssues_WhenAllIssuesAreInformationalOrLower_ReturnsTrue()
    {
        IReadOnlyList<IssueInfo> issues =
        [
            CreateIssue(IssueSeverity.None),
        CreateIssue(IssueSeverity.Trace),
        CreateIssue(IssueSeverity.Debug),
        CreateIssue(IssueSeverity.Information)
        ];

        var result = issues.HasOnlyInformationalOrLowerIssues();

        Assert.True(result);
    }

    [Fact]
    public void HasOnlyInformationalOrLowerIssues_WhenAnyIssueIsWarning_ReturnsFalse()
    {
        IReadOnlyList<IssueInfo> issues =
        [
            CreateIssue(IssueSeverity.None),
        CreateIssue(IssueSeverity.Information),
        CreateIssue(IssueSeverity.Warning)
        ];

        var result = issues.HasOnlyInformationalOrLowerIssues();

        Assert.False(result);
    }

    [Fact]
    public void HasOnlyInformationalOrLowerIssues_WhenAnyIssueIsError_ReturnsFalse()
    {
        IReadOnlyList<IssueInfo> issues =
        [
            CreateIssue(IssueSeverity.Information),
        CreateIssue(IssueSeverity.Error)
        ];

        var result = issues.HasOnlyInformationalOrLowerIssues();

        Assert.False(result);
    }

    [Fact]
    public void HasOnlyInformationalOrLowerIssues_WhenAnyIssueIsCritical_ReturnsFalse()
    {
        IReadOnlyList<IssueInfo> issues =
        [
            CreateIssue(IssueSeverity.Information),
        CreateIssue(IssueSeverity.Critical)
        ];

        var result = issues.HasOnlyInformationalOrLowerIssues();

        Assert.False(result);
    }

    [Fact]
    public void HasOnlyInformationalOrLowerIssues_WhenAnyIssueIsFatal_ReturnsFalse()
    {
        IReadOnlyList<IssueInfo> issues =
        [
            CreateIssue(IssueSeverity.Information),
        CreateIssue(IssueSeverity.Fatal)
        ];

        var result = issues.HasOnlyInformationalOrLowerIssues();

        Assert.False(result);
    }
    [Fact]
    public void HasWarningOrHigherIssues_WhenIssuesIsNull_ThrowsArgumentNullException()
    {
        IReadOnlyList<IssueInfo>? issues = null;

        Assert.Throws<ArgumentNullException>(() =>
            issues!.HasWarningOrHigherIssues());
    }

    [Fact]
    public void HasWarningOrHigherIssues_WhenIssuesAreEmpty_ReturnsFalse()
    {
        IReadOnlyList<IssueInfo> issues = [];

        var result = issues.HasWarningOrHigherIssues();

        Assert.False(result);
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
    public void HasWarningOrHigherIssues_ReturnsExpectedResult(
        IssueSeverity severity,
        bool expected)
    {
        IReadOnlyList<IssueInfo> issues =
        [
            CreateIssue(severity)
        ];

        var result = issues.HasWarningOrHigherIssues();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void HasErrorOrHigherIssues_WhenIssuesIsNull_ThrowsArgumentNullException()
    {
        IReadOnlyList<IssueInfo>? issues = null;

        Assert.Throws<ArgumentNullException>(() =>
            issues!.HasErrorOrHigherIssues());
    }

    [Fact]
    public void HasErrorOrHigherIssues_WhenIssuesAreEmpty_ReturnsFalse()
    {
        IReadOnlyList<IssueInfo> issues = [];

        var result = issues.HasErrorOrHigherIssues();

        Assert.False(result);
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
    public void HasErrorOrHigherIssues_ReturnsExpectedResult(
        IssueSeverity severity,
        bool expected)
    {
        IReadOnlyList<IssueInfo> issues =
        [
            CreateIssue(severity)
        ];

        var result = issues.HasErrorOrHigherIssues();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void HasCriticalOrHigherIssues_WhenIssuesIsNull_ThrowsArgumentNullException()
    {
        IReadOnlyList<IssueInfo>? issues = null;

        Assert.Throws<ArgumentNullException>(() =>
            issues!.HasCriticalOrHigherIssues());
    }

    [Fact]
    public void HasCriticalOrHigherIssues_WhenIssuesAreEmpty_ReturnsFalse()
    {
        IReadOnlyList<IssueInfo> issues = [];

        var result = issues.HasCriticalOrHigherIssues();

        Assert.False(result);
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
    public void HasCriticalOrHigherIssues_ReturnsExpectedResult(
        IssueSeverity severity,
        bool expected)
    {
        IReadOnlyList<IssueInfo> issues =
        [
            CreateIssue(severity)
        ];

        var result = issues.HasCriticalOrHigherIssues();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void HasWarningOrHigherIssues_WhenAnyIssueMatches_ReturnsTrue()
    {
        IReadOnlyList<IssueInfo> issues =
        [
            CreateIssue(IssueSeverity.Information),
        CreateIssue(IssueSeverity.Warning)
        ];

        var result = issues.HasWarningOrHigherIssues();

        Assert.True(result);
    }

    [Fact]
    public void HasErrorOrHigherIssues_WhenAnyIssueMatches_ReturnsTrue()
    {
        IReadOnlyList<IssueInfo> issues =
        [
            CreateIssue(IssueSeverity.Warning),
        CreateIssue(IssueSeverity.Error)
        ];

        var result = issues.HasErrorOrHigherIssues();

        Assert.True(result);
    }

    [Fact]
    public void HasCriticalOrHigherIssues_WhenAnyIssueMatches_ReturnsTrue()
    {
        IReadOnlyList<IssueInfo> issues =
        [
            CreateIssue(IssueSeverity.Error),
        CreateIssue(IssueSeverity.Critical)
        ];

        var result = issues.HasCriticalOrHigherIssues();

        Assert.True(result);
    }
}
