using Afrowave.Toolbox.Essentials.Extensions;
using Afrowave.Toolbox.Essentials.ValueObjects;

namespace Afrowave.Toolbox.Essentials.Tests.Extensions;

public sealed class CultureCodeExtensionsTests
{
   [Theory]
   [InlineData("en", "en", true)]
   [InlineData("en", "EN", true)]
   [InlineData("en-US", "en", true)]
   [InlineData("en-US", "en-GB", true)]
   [InlineData("EN-us", "en-gb", true)]
   [InlineData("cs-CZ", "cs", true)]
   [InlineData("cs-CZ", "sk-SK", false)]
   [InlineData("fr-FR", "en-US", false)]
   public void HasSameNeutralPartAs_ReturnsExpectedResult(
       string first,
       string second,
       bool expected)
   {
      var firstCultureCode = new CultureCode(first);
      var secondCultureCode = new CultureCode(second);

      var actual = firstCultureCode.HasSameNeutralPartAs(secondCultureCode);

      Assert.Equal(expected, actual);
   }

   [Theory]
   [InlineData("en", "en", true)]
   [InlineData("en", "EN", true)]
   [InlineData("en-US", "en-us", true)]
   [InlineData("cs-CZ", "CS-cz", true)]
   [InlineData("en-US", "en-GB", false)]
   [InlineData("cs", "sk", false)]
   public void EqualsIgnoreCase_ReturnsExpectedResult(
       string first,
       string second,
       bool expected)
   {
      var firstCultureCode = new CultureCode(first);
      var secondCultureCode = new CultureCode(second);

      var actual = firstCultureCode.EqualsIgnoreCase(secondCultureCode);

      Assert.Equal(expected, actual);
   }

   [Theory]
   [InlineData("EN", "en")]
   [InlineData("EN-US", "en-us")]
   [InlineData("cs-CZ", "cs-cz")]
   [InlineData("SW", "sw")]
   public void ToLowerInvariantCode_ReturnsLowercaseCultureCode(
       string input,
       string expected)
   {
      var cultureCode = new CultureCode(input);

      var actual = cultureCode.ToLowerInvariantCode();

      Assert.Equal(expected, actual.Value);
   }

   [Theory]
   [InlineData("en", "en")]
   [InlineData("en-US", "en")]
   [InlineData("cs", "cs")]
   [InlineData("cs-CZ", "cs")]
   [InlineData("sw-KE", "sw")]
   public void GetParentOrSelf_ReturnsExpectedCultureCode(
       string input,
       string expected)
   {
      var cultureCode = new CultureCode(input);

      var actual = cultureCode.GetParentOrSelf();

      Assert.Equal(expected, actual.Value);
   }

   [Fact]
   public void GetParentOrSelf_WhenCultureCodeIsNeutral_ReturnsOriginalValue()
   {
      var cultureCode = new CultureCode("en");

      var actual = cultureCode.GetParentOrSelf();

      Assert.Equal(cultureCode, actual);
   }

   [Fact]
   public void GetParentOrSelf_WhenCultureCodeIsSpecific_ReturnsNeutralParent()
   {
      var cultureCode = new CultureCode("en-US");

      var actual = cultureCode.GetParentOrSelf();

      Assert.Equal("en", actual.Value);
   }
   [Fact]
   public void ToLowerInvariantCode_WhenCultureCodeIsDefault_ReturnsDefault()
   {
      var cultureCode = default(CultureCode);

      var actual = cultureCode.ToLowerInvariantCode();

      Assert.Equal(default, actual);
   }

   [Fact]
   public void GetParentOrSelf_WhenCultureCodeIsDefault_ReturnsDefault()
   {
      var cultureCode = default(CultureCode);

      var actual = cultureCode.GetParentOrSelf();

      Assert.Equal(default, actual);
   }

   [Fact]
   public void EqualsIgnoreCase_WhenBothCultureCodesAreDefault_ReturnsTrue()
   {
      var first = default(CultureCode);
      var second = default(CultureCode);

      var actual = first.EqualsIgnoreCase(second);

      Assert.True(actual);
   }

   [Fact]
   public void EqualsIgnoreCase_WhenOnlyFirstCultureCodeIsDefault_ReturnsFalse()
   {
      var first = default(CultureCode);
      var second = new CultureCode("en");

      var actual = first.EqualsIgnoreCase(second);

      Assert.False(actual);
   }
}