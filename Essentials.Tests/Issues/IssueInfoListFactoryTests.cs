using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Issues;

namespace Afrowave.Toolbox.Essentials.Tests.Issues;

public sealed class IssueInfoListFactoryTests
{
   [Fact]
   public void Empty_ReturnsEmptyIssueList()
   {
      var issues = IssueInfoListFactory.Empty();

      Assert.NotNull(issues);
      Assert.Empty(issues);
   }

   [Fact]
   public void Empty_ReturnsNewEmptyListEachTime()
   {
      var first = IssueInfoListFactory.Empty();
      var second = IssueInfoListFactory.Empty();

      Assert.NotSame(first, second);
      Assert.Empty(first);
      Assert.Empty(second);
   }

   [Fact]
   public void From_WithParams_WhenIssuesIsNull_ThrowsArgumentNullException()
   {
      IssueInfo[]? issues = null;

      Assert.Throws<ArgumentNullException>(() =>
          IssueInfoListFactory.From(issues!));
   }

   [Fact]
   public void From_WithParams_WhenIssuesAreEmpty_ReturnsEmptyIssueList()
   {
      var issues = IssueInfoListFactory.From([]);

      Assert.NotNull(issues);
      Assert.Empty(issues);
   }

   [Fact]
   public void From_WithParams_CreatesIssueList()
   {
      var first = IssueInfoFactory.Warning(
          "AFW_WARNING",
          "Warning message.");

      var second = IssueInfoFactory.Error(
          "AFW_ERROR",
          "Error message.");

      var issues = IssueInfoListFactory.From(first, second);

      Assert.Equal(2, issues.Count);
      Assert.Same(first, issues[0]);
      Assert.Same(second, issues[1]);
   }

   [Fact]
   public void From_WithParams_ReturnsSnapshot()
   {
      var first = IssueInfoFactory.Warning(
          "AFW_WARNING",
          "Warning message.");

      var second = IssueInfoFactory.Error(
          "AFW_ERROR",
          "Error message.");

      var source = new[]
      {
            first,
            second
        };

      var issues = IssueInfoListFactory.From(source);

      source[0] = IssueInfoFactory.Information(
          "AFW_INFO",
          "Changed message.");

      Assert.Equal(2, issues.Count);
      Assert.Same(first, issues[0]);
      Assert.Same(second, issues[1]);
   }

   [Fact]
   public void From_WithEnumerable_WhenIssuesIsNull_ThrowsArgumentNullException()
   {
      IEnumerable<IssueInfo>? issues = null;

      Assert.Throws<ArgumentNullException>(() =>
          IssueInfoListFactory.From(issues!));
   }

   [Fact]
   public void From_WithEnumerable_WhenIssuesAreEmpty_ReturnsEmptyIssueList()
   {
      IEnumerable<IssueInfo> source = [];

      var issues = IssueInfoListFactory.From(source);

      Assert.NotNull(issues);
      Assert.Empty(issues);
   }

   [Fact]
   public void From_WithEnumerable_CreatesIssueList()
   {
      var first = IssueInfoFactory.Warning(
          "AFW_WARNING",
          "Warning message.");

      var second = IssueInfoFactory.Error(
          "AFW_ERROR",
          "Error message.");

      IEnumerable<IssueInfo> source =
      [
          first,
            second
      ];

      var issues = IssueInfoListFactory.From(source);

      Assert.Equal(2, issues.Count);
      Assert.Same(first, issues[0]);
      Assert.Same(second, issues[1]);
   }

   [Fact]
   public void From_WithEnumerable_ReturnsSnapshot()
   {
      var first = IssueInfoFactory.Warning(
          "AFW_WARNING",
          "Warning message.");

      var second = IssueInfoFactory.Error(
          "AFW_ERROR",
          "Error message.");

      var source = new List<IssueInfo>
        {
            first,
            second
        };

      var issues = IssueInfoListFactory.From((IEnumerable<IssueInfo>)source);

      source.Clear();

      Assert.Equal(2, issues.Count);
      Assert.Same(first, issues[0]);
      Assert.Same(second, issues[1]);
   }

   [Fact]
   public void Information_CreatesListWithOneInformationIssue()
   {
      var issues = IssueInfoListFactory.Information(
          "AFW_INFO",
          "Information message.");

      Assert.Single(issues);

      var issue = issues[0];

      Assert.Equal("AFW_INFO", issue.Code);
      Assert.Equal("Information message.", issue.Message);
      Assert.Equal(IssueSeverity.Information, issue.Severity);
   }

   [Fact]
   public void Warning_CreatesListWithOneWarningIssue()
   {
      var issues = IssueInfoListFactory.Warning(
          "AFW_WARNING",
          "Warning message.");

      Assert.Single(issues);

      var issue = issues[0];

      Assert.Equal("AFW_WARNING", issue.Code);
      Assert.Equal("Warning message.", issue.Message);
      Assert.Equal(IssueSeverity.Warning, issue.Severity);
   }

   [Fact]
   public void Error_CreatesListWithOneErrorIssue()
   {
      var issues = IssueInfoListFactory.Error(
          "AFW_ERROR",
          "Error message.");

      Assert.Single(issues);

      var issue = issues[0];

      Assert.Equal("AFW_ERROR", issue.Code);
      Assert.Equal("Error message.", issue.Message);
      Assert.Equal(IssueSeverity.Error, issue.Severity);
   }

   [Fact]
   public void Critical_CreatesListWithOneCriticalIssue()
   {
      var issues = IssueInfoListFactory.Critical(
          "AFW_CRITICAL",
          "Critical message.");

      Assert.Single(issues);

      var issue = issues[0];

      Assert.Equal("AFW_CRITICAL", issue.Code);
      Assert.Equal("Critical message.", issue.Message);
      Assert.Equal(IssueSeverity.Critical, issue.Severity);
   }

   [Fact]
   public void Fatal_CreatesListWithOneFatalIssue()
   {
      var issues = IssueInfoListFactory.Fatal(
          "AFW_FATAL",
          "Fatal message.");

      Assert.Single(issues);

      var issue = issues[0];

      Assert.Equal("AFW_FATAL", issue.Code);
      Assert.Equal("Fatal message.", issue.Message);
      Assert.Equal(IssueSeverity.Fatal, issue.Severity);
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Information_WhenCodeIsInvalid_ThrowsArgumentException(string? code)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
          IssueInfoListFactory.Information(code!, "Information message."));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Information_WhenMessageIsInvalid_ThrowsArgumentException(string? message)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
          IssueInfoListFactory.Information("AFW_INFO", message!));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Warning_WhenCodeIsInvalid_ThrowsArgumentException(string? code)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
          IssueInfoListFactory.Warning(code!, "Warning message."));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Warning_WhenMessageIsInvalid_ThrowsArgumentException(string? message)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
          IssueInfoListFactory.Warning("AFW_WARNING", message!));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Error_WhenCodeIsInvalid_ThrowsArgumentException(string? code)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
          IssueInfoListFactory.Error(code!, "Error message."));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Error_WhenMessageIsInvalid_ThrowsArgumentException(string? message)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
          IssueInfoListFactory.Error("AFW_ERROR", message!));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Critical_WhenCodeIsInvalid_ThrowsArgumentException(string? code)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
          IssueInfoListFactory.Critical(code!, "Critical message."));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Critical_WhenMessageIsInvalid_ThrowsArgumentException(string? message)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
          IssueInfoListFactory.Critical("AFW_CRITICAL", message!));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Fatal_WhenCodeIsInvalid_ThrowsArgumentException(string? code)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
          IssueInfoListFactory.Fatal(code!, "Fatal message."));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Fatal_WhenMessageIsInvalid_ThrowsArgumentException(string? message)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
          IssueInfoListFactory.Fatal("AFW_FATAL", message!));
   }
}