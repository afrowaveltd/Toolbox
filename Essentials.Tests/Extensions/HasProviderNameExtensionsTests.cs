using Afrowave.Toolbox.Essentials.Extensions;
using Afrowave.Toolbox.Essentials.Interfaces;
using Afrowave.Toolbox.Essentials.ValueObjects;

namespace Afrowave.Toolbox.Essentials.Tests.Extensions;

public sealed class HasProviderNameExtensionsTests
{
   [Fact]
   public void HasProviderName_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasProviderName? value = null;

      Assert.Throws<ArgumentNullException>(() =>
          value!.HasProviderName(new ProviderName("ollama-local")));
   }

   [Theory]
   [InlineData("ollama-local", "ollama-local", true)]
   [InlineData("ollama-local", "OLLAMA-LOCAL", true)]
   [InlineData("Ollama-Local", "ollama-local", true)]
   [InlineData("libretranslate-main", "LibreTranslate-Main", true)]
   [InlineData("ollama-local", "ollama-remote", false)]
   [InlineData("sqlite-default", "http-default", false)]
   public void HasProviderName_WithProviderName_ReturnsExpectedResult(
       string currentProviderName,
       string expectedProviderName,
       bool expected)
   {
      var value = new TestHasProviderName(new ProviderName(currentProviderName));

      var actual = value.HasProviderName(new ProviderName(expectedProviderName));

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasProviderNameWithString_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasProviderName? value = null;

      Assert.Throws<ArgumentNullException>(() =>
          value!.HasProviderName("ollama-local"));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void HasProviderNameWithString_WhenProviderNameIsInvalid_ThrowsArgumentException(
       string? providerName)
   {
      var value = new TestHasProviderName(new ProviderName("ollama-local"));

      Assert.ThrowsAny<ArgumentException>(() =>
          value.HasProviderName(providerName!));
   }

   [Theory]
   [InlineData("ollama-local", "ollama-local", true)]
   [InlineData("ollama-local", "OLLAMA-LOCAL", true)]
   [InlineData("Ollama-Local", "ollama-local", true)]
   [InlineData("libretranslate-main", "LibreTranslate-Main", true)]
   [InlineData("ollama-local", "ollama-remote", false)]
   [InlineData("sqlite-default", "http-default", false)]
   public void HasProviderName_WithString_ReturnsExpectedResult(
       string currentProviderName,
       string expectedProviderName,
       bool expected)
   {
      var value = new TestHasProviderName(new ProviderName(currentProviderName));

      var actual = value.HasProviderName(expectedProviderName);

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void GetNormalizedProviderName_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasProviderName? value = null;

      Assert.Throws<ArgumentNullException>(() =>
          value!.GetNormalizedProviderName());
   }

   [Theory]
   [InlineData("OLLAMA-LOCAL", "ollama-local")]
   [InlineData("Ollama-Local", "ollama-local")]
   [InlineData("LibreTranslate-Main", "libretranslate-main")]
   [InlineData("SQLITE-DEFAULT", "sqlite-default")]
   public void GetNormalizedProviderName_ReturnsLowercaseProviderName(
       string input,
       string expected)
   {
      var value = new TestHasProviderName(new ProviderName(input));

      var actual = value.GetNormalizedProviderName();

      Assert.Equal(expected, actual.Value);
   }

   [Fact]
   public void GetNormalizedProviderName_WhenAlreadyLowercase_ReturnsEquivalentProviderName()
   {
      var value = new TestHasProviderName(new ProviderName("ollama-local"));

      var actual = value.GetNormalizedProviderName();

      Assert.Equal("ollama-local", actual.Value);
      Assert.Equal(value.ProviderName, actual);
   }

   private sealed class TestHasProviderName : IHasProviderName
   {
      public TestHasProviderName(ProviderName providerName)
      {
         ProviderName = providerName;
      }

      public ProviderName ProviderName { get; }
   }
}