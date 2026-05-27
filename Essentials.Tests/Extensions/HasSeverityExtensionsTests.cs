using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Extensions;
using Afrowave.Toolbox.Essentials.Interfaces;

namespace Afrowave.Toolbox.Essentials.Tests.Extensions;

public sealed class HasSeverityExtensionsTests
{
   [Fact]
   public void HasSeverity_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasSeverity? value = null;

      Assert.Throws<ArgumentNullException>(() =>
          value!.HasSeverity(IssueSeverity.Warning));
   }

   [Theory]
   [InlineData(IssueSeverity.None, IssueSeverity.None, true)]
   [InlineData(IssueSeverity.Trace, IssueSeverity.Trace, true)]
   [InlineData(IssueSeverity.Debug, IssueSeverity.Debug, true)]
   [InlineData(IssueSeverity.Information, IssueSeverity.Information, true)]
   [InlineData(IssueSeverity.Warning, IssueSeverity.Warning, true)]
   [InlineData(IssueSeverity.Error, IssueSeverity.Error, true)]
   [InlineData(IssueSeverity.Critical, IssueSeverity.Critical, true)]
   [InlineData(IssueSeverity.Fatal, IssueSeverity.Fatal, true)]
   [InlineData(IssueSeverity.Warning, IssueSeverity.Error, false)]
   [InlineData(IssueSeverity.Error, IssueSeverity.Warning, false)]
   public void HasSeverity_ReturnsExpectedResult(
       IssueSeverity currentSeverity,
       IssueSeverity expectedSeverity,
       bool expected)
   {
      var value = new TestHasSeverity(currentSeverity);

      var actual = value.HasSeverity(expectedSeverity);

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasErrorOrHigherSeverity_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasSeverity? value = null;

      Assert.Throws<ArgumentNullException>(() =>
          value!.HasErrorOrHigherSeverity());
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
   public void HasErrorOrHigherSeverity_ReturnsExpectedResult(
       IssueSeverity severity,
       bool expected)
   {
      var value = new TestHasSeverity(severity);

      var actual = value.HasErrorOrHigherSeverity();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasWarningOrHigherSeverity_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasSeverity? value = null;

      Assert.Throws<ArgumentNullException>(() =>
          value!.HasWarningOrHigherSeverity());
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
   public void HasWarningOrHigherSeverity_ReturnsExpectedResult(
       IssueSeverity severity,
       bool expected)
   {
      var value = new TestHasSeverity(severity);

      var actual = value.HasWarningOrHigherSeverity();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasInformationOrLowerSeverity_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasSeverity? value = null;

      Assert.Throws<ArgumentNullException>(() =>
          value!.HasInformationOrLowerSeverity());
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
   public void HasInformationOrLowerSeverity_ReturnsExpectedResult(
       IssueSeverity severity,
       bool expected)
   {
      var value = new TestHasSeverity(severity);

      var actual = value.HasInformationOrLowerSeverity();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasCriticalOrHigherSeverity_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasSeverity? value = null;

      Assert.Throws<ArgumentNullException>(() =>
          value!.HasCriticalOrHigherSeverity());
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
   public void HasCriticalOrHigherSeverity_ReturnsExpectedResult(
       IssueSeverity severity,
       bool expected)
   {
      var value = new TestHasSeverity(severity);

      var actual = value.HasCriticalOrHigherSeverity();

      Assert.Equal(expected, actual);
   }

   private sealed class TestHasSeverity : IHasSeverity
   {
      public TestHasSeverity(IssueSeverity severity)
      {
         Severity = severity;
      }

      public IssueSeverity Severity { get; }
   }
}