using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.WhenItFails.Tests.Normalization;

public sealed class TextKeyNormalizerTests
{
   [Theory]
   [InlineData(null, "")]
   [InlineData("", "")]
   [InlineData("   ", "")]
   [InlineData("Network error", "NETWORK_ERROR")]
   [InlineData("Network Error", "NETWORK_ERROR")]
   [InlineData("network error", "NETWORK_ERROR")]
   [InlineData(" network   error ", "NETWORK_ERROR")]
   [InlineData("network-error", "NETWORK_ERROR")]
   [InlineData("network_error", "NETWORK_ERROR")]
   [InlineData("Network/Error", "NETWORK_ERROR")]
   [InlineData("HTTP 404", "HTTP_404")]
   [InlineData("__network__error__", "NETWORK_ERROR")]
   [InlineData("network.error", "NETWORK_ERROR")]
   public void NormalizeKey_ShouldReturnExpectedValue(
       string? input,
       string expected)
   {
      string actual = TextKeyNormalizer.NormalizeKey(input);

      Assert.Equal(expected, actual);
   }

   [Theory]
   [InlineData(null, "")]
   [InlineData("", "")]
   [InlineData("   ", "")]
   [InlineData("Network error", "Network error")]
   [InlineData("  Network error  ", "Network error")]
   [InlineData("network error", "network error")]
   public void NormalizeDisplayName_ShouldTrimOnly(
       string? input,
       string expected)
   {
      string actual = TextKeyNormalizer.NormalizeDisplayName(input);

      Assert.Equal(expected, actual);
   }
}