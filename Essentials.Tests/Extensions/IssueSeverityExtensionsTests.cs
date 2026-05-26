using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Extensions;

namespace Afrowave.Toolbox.Essentials.Tests.Extensions;

public sealed class IssueSeverityExtensionsTests
{
   [Theory]
   [InlineData(IssueSeverity.None, false)]
   [InlineData(IssueSeverity.Trace, false)]
   [InlineData(IssueSeverity.Debug, false)]
   [InlineData(IssueSeverity.Information, false)]
   [InlineData(IssueSeverity.Warning, false)]
   [InlineData(IssueSeverity.Error, true)]
   [InlineData(IssueSeverity.Critical, true)]
   [InlineData(IssueSeverity.Fatal, true)]
   public void IsErrorOrHigher_ReturnsExpectedResult(
       IssueSeverity severity,
       bool expected)
   {
      var actual = severity.IsErrorOrHigher();

      Assert.Equal(expected, actual);
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
   public void IsWarningOrHigher_ReturnsExpectedResult(
       IssueSeverity severity,
       bool expected)
   {
      var actual = severity.IsWarningOrHigher();

      Assert.Equal(expected, actual);
   }

   [Theory]
   [InlineData(IssueSeverity.None, true)]
   [InlineData(IssueSeverity.Trace, true)]
   [InlineData(IssueSeverity.Debug, true)]
   [InlineData(IssueSeverity.Information, true)]
   [InlineData(IssueSeverity.Warning, false)]
   [InlineData(IssueSeverity.Error, false)]
   [InlineData(IssueSeverity.Critical, false)]
   [InlineData(IssueSeverity.Fatal, false)]
   public void IsInformationOrLower_ReturnsExpectedResult(
       IssueSeverity severity,
       bool expected)
   {
      var actual = severity.IsInformationOrLower();

      Assert.Equal(expected, actual);
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
   public void IsCriticalOrHigher_ReturnsExpectedResult(
       IssueSeverity severity,
       bool expected)
   {
      var actual = severity.IsCriticalOrHigher();

      Assert.Equal(expected, actual);
   }
}