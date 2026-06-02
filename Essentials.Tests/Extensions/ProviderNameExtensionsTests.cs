using Afrowave.Toolbox.Essentials.Extensions;
using Afrowave.Toolbox.Essentials.ValueObjects;

namespace Afrowave.Toolbox.Essentials.Tests.Extensions;

public sealed class ProviderNameExtensionsTests
{
   [Theory]
   [InlineData("ollama-local", "ollama-local", true)]
   [InlineData("ollama-local", "OLLAMA-LOCAL", true)]
   [InlineData("Ollama-Local", "ollama-local", true)]
   [InlineData("libretranslate-main", "LibreTranslate-Main", true)]
   [InlineData("ollama-local", "ollama-remote", false)]
   [InlineData("sqlite-default", "http-default", false)]
   public void EqualsIgnoreCase_ReturnsExpectedResult(
       string first,
       string second,
       bool expected)
   {
      var firstProviderName = new ProviderName(first);
      var secondProviderName = new ProviderName(second);

      var actual = firstProviderName.EqualsIgnoreCase(secondProviderName);

      Assert.Equal(expected, actual);
   }

   [Theory]
   [InlineData("OLLAMA-LOCAL", "ollama-local")]
   [InlineData("Ollama-Local", "ollama-local")]
   [InlineData("LibreTranslate-Main", "libretranslate-main")]
   [InlineData("SQLITE-DEFAULT", "sqlite-default")]
   public void ToLowerInvariantName_ReturnsLowercaseProviderName(
       string input,
       string expected)
   {
      var providerName = new ProviderName(input);

      var actual = providerName.ToLowerInvariantName();

      Assert.Equal(expected, actual.Value);
   }

   [Fact]
   public void ToLowerInvariantName_ReturnsNewProviderNameWithSameValueWhenAlreadyLowercase()
   {
      var providerName = new ProviderName("ollama-local");

      var actual = providerName.ToLowerInvariantName();

      Assert.Equal("ollama-local", actual.Value);
      Assert.Equal(providerName, actual);
   }
   [Fact]
   public void ToLowerInvariantName_WhenProviderNameIsDefault_ReturnsDefault()
   {
      var providerName = default(ProviderName);

      var actual = providerName.ToLowerInvariantName();

      Assert.Equal(default, actual);
   }
}