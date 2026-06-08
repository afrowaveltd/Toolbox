using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.WhenItFails.Tests.Normalization;

public sealed class DefinitionNormalizationHelperTests
{
   [Fact]
   public void NormalizeStringList_ShouldReturnEmptyList_WhenValuesAreNull()
   {
      List<string> result = DefinitionNormalizationHelper.NormalizeStringList(null);

      Assert.Empty(result);
   }

   [Fact]
   public void NormalizeDictionary_ShouldReturnEmptyDictionary_WhenValuesAreNull()
   {
      Dictionary<string, string> result = DefinitionNormalizationHelper.NormalizeDictionary(null);

      Assert.Empty(result);
   }

   [Fact]
   public void CreateDisplayName_ShouldUseFallbackName_WhenDisplayNameIsEmpty()
   {
      string result = DefinitionNormalizationHelper.CreateDisplayName(
          displayName: "   ",
          fallbackName: "fallback name");

      Assert.Equal("fallback name", result);
   }

   [Fact]
   public void NormalizeNullableDisplayText_ShouldReturnNull_WhenValueIsEmpty()
   {
      string? result = DefinitionNormalizationHelper.NormalizeNullableDisplayText("   ");

      Assert.Null(result);
   }
}
