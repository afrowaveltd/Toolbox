using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.WhenItFails.Tests.Normalization;

public sealed class ErrorCatalogDocumentNormalizerTests
{
   [Fact]
   public void Constructor_ShouldThrowArgumentNullException_WhenErrorDefinitionNormalizerIsNull()
   {
      Assert.Throws<ArgumentNullException>(
          () => new ErrorCatalogDocumentNormalizer(null!));
   }

   [Fact]
   public void Normalize_ShouldThrowArgumentNullException_WhenDocumentIsNull()
   {
      ErrorCatalogDocumentNormalizer normalizer = new();

      Assert.Throws<ArgumentNullException>(() => normalizer.Normalize(null!));
   }

   [Fact]
   public void Normalize_ShouldNormalizeCatalogHeader()
   {
      ErrorCatalogDocumentNormalizer normalizer = new();

      ErrorCatalogDocument document = new()
      {
         SchemaVersion = " 1.0 ",
         CatalogId = " test.catalog ",
         CatalogName = " Test Catalog ",
         Description = " Test description. ",
         Language = " en ",
         SourceCatalogId = " source.catalog ",
         SourceCatalogVersion = " 1.0 ",
         IsShadowCopy = true,
         Tags = ["default catalog", "default-catalog", "web"]
      };

      ErrorCatalogDocument normalizedDocument = normalizer.Normalize(document);

      Assert.Equal("1.0", normalizedDocument.SchemaVersion);
      Assert.Equal("test.catalog", normalizedDocument.CatalogId);
      Assert.Equal("Test Catalog", normalizedDocument.CatalogName);
      Assert.Equal("Test description.", normalizedDocument.Description);
      Assert.Equal("en", normalizedDocument.Language);
      Assert.Equal("source.catalog", normalizedDocument.SourceCatalogId);
      Assert.Equal("1.0", normalizedDocument.SourceCatalogVersion);
      Assert.True(normalizedDocument.IsShadowCopy);

      Assert.Equal(
          ["DEFAULT_CATALOG", "WEB"],
          normalizedDocument.Tags);
   }

   [Fact]
   public void Normalize_ShouldNormalizeAllErrors()
   {
      ErrorCatalogDocumentNormalizer normalizer = new();

      ErrorCatalogDocument document = new()
      {
         Errors =
          [
              new ErrorDefinition
                {
                    Id = "afw cfg 0001",
                    Code = 200001,
                    Name = "Missing Configuration Value",
                    Owner = "afw",
                    CodePrefix = "cfg",
                    CodeGroup = "Configuration",
                    PrimaryCategory = "Configuration",
                    Categories = ["Configuration", "startup"],
                    Subcategories = ["Required Value"],
                    Title = " Missing configuration value ",
                    Message = " A required configuration value is missing. ",
                    DefaultSeverity = " Error ",
                    Tags = ["user visible", "startup"]
                }
          ]
      };

      ErrorCatalogDocument normalizedDocument = normalizer.Normalize(document);

      ErrorDefinition error = Assert.Single(normalizedDocument.Errors);

      Assert.Equal("AFW_CFG_0001", error.Id);
      Assert.Equal("MISSING_CONFIGURATION_VALUE", error.Name);
      Assert.Equal("AFW", error.Owner);
      Assert.Equal("CFG", error.CodePrefix);
      Assert.Equal("CONFIGURATION", error.CodeGroup);
      Assert.Equal("CONFIGURATION", error.PrimaryCategory);
      Assert.Equal(["CONFIGURATION", "STARTUP"], error.Categories);
      Assert.Equal(["REQUIRED_VALUE"], error.Subcategories);
      Assert.Equal("Missing configuration value", error.Title);
      Assert.Equal("A required configuration value is missing.", error.Message);
      Assert.Equal("Error", error.DefaultSeverity);
      Assert.Equal(["USER_VISIBLE", "STARTUP"], error.Tags);
   }

   [Fact]
   public void Normalize_ShouldNotModifyOriginalDocument()
   {
      ErrorCatalogDocumentNormalizer normalizer = new();

      ErrorCatalogDocument document = new()
      {
         CatalogName = " Test Catalog ",
         Tags = ["default catalog"]
      };

      ErrorCatalogDocument normalizedDocument = normalizer.Normalize(document);

      Assert.Equal(" Test Catalog ", document.CatalogName);
      Assert.Equal(["default catalog"], document.Tags);

      Assert.Equal("Test Catalog", normalizedDocument.CatalogName);
      Assert.Equal(["DEFAULT_CATALOG"], normalizedDocument.Tags);
   }

   [Fact]
   public void Normalize_ShouldKeepMetadataReference()
   {
      ErrorCatalogDocumentNormalizer normalizer = new();

      ErrorCatalogDocument document = new();

      ErrorCatalogDocument normalizedDocument = normalizer.Normalize(document);

      Assert.Same(document.Metadata, normalizedDocument.Metadata);
   }
}