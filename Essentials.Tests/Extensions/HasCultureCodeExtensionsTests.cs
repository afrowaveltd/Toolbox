using Afrowave.Toolbox.Essentials.Extensions;
using Afrowave.Toolbox.Essentials.Interfaces;
using Afrowave.Toolbox.Essentials.ValueObjects;

namespace Afrowave.Toolbox.Essentials.Tests.Extensions;

public sealed class HasCultureCodeExtensionsTests
{
   [Fact]
   public void HasCultureCode_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasCultureCode? value = null;

      Assert.Throws<ArgumentNullException>(() =>
          value!.HasCultureCode(new CultureCode("en")));
   }

   [Theory]
   [InlineData("en", "en", true)]
   [InlineData("en", "EN", true)]
   [InlineData("en-US", "en-us", true)]
   [InlineData("cs-CZ", "CS-cz", true)]
   [InlineData("en-US", "en-GB", false)]
   [InlineData("cs", "sk", false)]
   public void HasCultureCode_WithCultureCode_ReturnsExpectedResult(
       string currentCultureCode,
       string expectedCultureCode,
       bool expected)
   {
      var value = new TestHasCultureCode(new CultureCode(currentCultureCode));

      var actual = value.HasCultureCode(new CultureCode(expectedCultureCode));

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasCultureCodeWithString_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasCultureCode? value = null;

      Assert.Throws<ArgumentNullException>(() =>
          value!.HasCultureCode("en"));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void HasCultureCodeWithString_WhenCultureCodeIsInvalid_ThrowsArgumentException(
       string? cultureCode)
   {
      var value = new TestHasCultureCode(new CultureCode("en"));

      Assert.ThrowsAny<ArgumentException>(() =>
          value.HasCultureCode(cultureCode!));
   }

   [Theory]
   [InlineData("en", "en", true)]
   [InlineData("en", "EN", true)]
   [InlineData("en-US", "en-us", true)]
   [InlineData("cs-CZ", "CS-cz", true)]
   [InlineData("en-US", "en-GB", false)]
   [InlineData("cs", "sk", false)]
   public void HasCultureCode_WithString_ReturnsExpectedResult(
       string currentCultureCode,
       string expectedCultureCode,
       bool expected)
   {
      var value = new TestHasCultureCode(new CultureCode(currentCultureCode));

      var actual = value.HasCultureCode(expectedCultureCode);

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasSameNeutralCultureAs_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasCultureCode? value = null;

      Assert.Throws<ArgumentNullException>(() =>
          value!.HasSameNeutralCultureAs(new CultureCode("en")));
   }

   [Theory]
   [InlineData("en", "en", true)]
   [InlineData("en", "EN", true)]
   [InlineData("en-US", "en", true)]
   [InlineData("en-US", "en-GB", true)]
   [InlineData("EN-us", "en-gb", true)]
   [InlineData("cs-CZ", "cs", true)]
   [InlineData("cs-CZ", "sk-SK", false)]
   [InlineData("fr-FR", "en-US", false)]
   public void HasSameNeutralCultureAs_ReturnsExpectedResult(
       string currentCultureCode,
       string expectedCultureCode,
       bool expected)
   {
      var value = new TestHasCultureCode(new CultureCode(currentCultureCode));

      var actual = value.HasSameNeutralCultureAs(new CultureCode(expectedCultureCode));

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasNeutralCultureCode_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasCultureCode? value = null;

      Assert.Throws<ArgumentNullException>(() =>
          value!.HasNeutralCultureCode());
   }

   [Theory]
   [InlineData("en", true)]
   [InlineData("cs", true)]
   [InlineData("sw", true)]
   [InlineData("en-US", false)]
   [InlineData("cs-CZ", false)]
   [InlineData("sw-KE", false)]
   public void HasNeutralCultureCode_ReturnsExpectedResult(
       string cultureCode,
       bool expected)
   {
      var value = new TestHasCultureCode(new CultureCode(cultureCode));

      var actual = value.HasNeutralCultureCode();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasSpecificCultureCode_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasCultureCode? value = null;

      Assert.Throws<ArgumentNullException>(() =>
          value!.HasSpecificCultureCode());
   }

   [Theory]
   [InlineData("en", false)]
   [InlineData("cs", false)]
   [InlineData("sw", false)]
   [InlineData("en-US", true)]
   [InlineData("cs-CZ", true)]
   [InlineData("sw-KE", true)]
   public void HasSpecificCultureCode_ReturnsExpectedResult(
       string cultureCode,
       bool expected)
   {
      var value = new TestHasCultureCode(new CultureCode(cultureCode));

      var actual = value.HasSpecificCultureCode();

      Assert.Equal(expected, actual);
   }

   private sealed class TestHasCultureCode : IHasCultureCode
   {
      public TestHasCultureCode(CultureCode cultureCode)
      {
         CultureCode = cultureCode;
      }

      public CultureCode CultureCode { get; }
   }
}