using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.WhenItFails.Tests.Normalization;

public sealed class ErrorDefinitionNormalizerTests
{
   [Fact]
   public void Normalize_ShouldThrowArgumentNullException_WhenDefinitionIsNull()
   {
      ErrorDefinitionNormalizer normalizer = new();

      Assert.Throws<ArgumentNullException>(() => normalizer.Normalize(null!));
   }

   [Fact]
   public void Normalize_ShouldNormalizeIdentityAndClassificationFields()
   {
      ErrorDefinitionNormalizer normalizer = new();

      ErrorDefinition definition = new()
      {
         Id = "afw cfg 0001",
         Code = 200001,
         Name = "Missing Configuration Value",
         Owner = "afw",
         CodePrefix = "cfg",
         CodeGroup = "Configuration",
         PrimaryCategory = "Network error",
         Categories = ["Network error", "network-error", "Startup"],
         Subcategories = ["Required Value", "required-value"],
         Title = " Missing configuration value ",
         Message = " A required configuration value is missing. ",
         DefaultSeverity = " Error ",
         DeveloperHint = " Check appsettings.json. ",
         DocumentationKey = " configuration.missing-value ",
         Tags = ["user visible", "user-visible", "startup"]
      };

      ErrorDefinition normalizedDefinition = normalizer.Normalize(definition);

      Assert.Equal("AFW_CFG_0001", normalizedDefinition.Id);
      Assert.Equal(200001, normalizedDefinition.Code);
      Assert.Equal("MISSING_CONFIGURATION_VALUE", normalizedDefinition.Name);
      Assert.Equal("AFW", normalizedDefinition.Owner);
      Assert.Equal("CFG", normalizedDefinition.CodePrefix);
      Assert.Equal("CONFIGURATION", normalizedDefinition.CodeGroup);
      Assert.Equal("NETWORK_ERROR", normalizedDefinition.PrimaryCategory);

      Assert.Equal(
          ["NETWORK_ERROR", "STARTUP"],
          normalizedDefinition.Categories);

      Assert.Equal(
          ["REQUIRED_VALUE"],
          normalizedDefinition.Subcategories);

      Assert.Equal("Missing configuration value", normalizedDefinition.Title);
      Assert.Equal("A required configuration value is missing.", normalizedDefinition.Message);
      Assert.Equal("Error", normalizedDefinition.DefaultSeverity);
      Assert.Equal("Check appsettings.json.", normalizedDefinition.DeveloperHint);
      Assert.Equal("configuration.missing-value", normalizedDefinition.DocumentationKey);

      Assert.Equal(
          ["USER_VISIBLE", "STARTUP"],
          normalizedDefinition.Tags);
   }

   [Fact]
   public void Normalize_ShouldRemoveEmptyValuesFromCollections()
   {
      ErrorDefinitionNormalizer normalizer = new();

      ErrorDefinition definition = new()
      {
         Categories = ["", " ", "Network"],
         Subcategories = ["", "Timeout"],
         Tags = ["", "Retryable"]
      };

      ErrorDefinition normalizedDefinition = normalizer.Normalize(definition);

      Assert.Equal(["NETWORK"], normalizedDefinition.Categories);
      Assert.Equal(["TIMEOUT"], normalizedDefinition.Subcategories);
      Assert.Equal(["RETRYABLE"], normalizedDefinition.Tags);
   }

   [Fact]
   public void Normalize_ShouldNotChangeMetadataReference()
   {
      ErrorDefinitionNormalizer normalizer = new();

      ErrorDefinition definition = new();

      ErrorDefinition normalizedDefinition = normalizer.Normalize(definition);

      Assert.Same(definition.Metadata, normalizedDefinition.Metadata);
   }
}