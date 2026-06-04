using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.WhenItFails.Tests.Normalization;

public sealed class ErrorCategoryCatalogDocumentNormalizerTests
{
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenCategoryDefinitionNormalizerIsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ErrorCategoryCatalogDocumentNormalizer(null!));
    }

    [Fact]
    public void Normalize_ShouldThrowArgumentNullException_WhenDocumentIsNull()
    {
        ErrorCategoryCatalogDocumentNormalizer normalizer = new();

        Assert.Throws<ArgumentNullException>(() => normalizer.Normalize(null!));
    }

    [Fact]
    public void Normalize_ShouldNormalizeCatalogHeader()
    {
        ErrorCategoryCatalogDocumentNormalizer normalizer = new();

        ErrorCategoryCatalogDocument document = new()
        {
            SchemaVersion = " 1.0 ",
            CatalogId = " categories.catalog ",
            CatalogName = " Category Catalog ",
            Description = " Category catalog description. ",
            Language = " en ",
            SourceCatalogId = " source.categories ",
            SourceCatalogVersion = " 1.0 ",
            IsShadowCopy = true,
            Tags = ["default catalog", "default-catalog", "categories"]
        };

        ErrorCategoryCatalogDocument normalizedDocument = normalizer.Normalize(document);

        Assert.Equal("1.0", normalizedDocument.SchemaVersion);
        Assert.Equal("categories.catalog", normalizedDocument.CatalogId);
        Assert.Equal("Category Catalog", normalizedDocument.CatalogName);
        Assert.Equal("Category catalog description.", normalizedDocument.Description);
        Assert.Equal("en", normalizedDocument.Language);
        Assert.Equal("source.categories", normalizedDocument.SourceCatalogId);
        Assert.Equal("1.0", normalizedDocument.SourceCatalogVersion);
        Assert.True(normalizedDocument.IsShadowCopy);

        Assert.Equal(
            ["DEFAULT_CATALOG", "CATEGORIES"],
            normalizedDocument.Tags);
    }

    [Fact]
    public void Normalize_ShouldNormalizeAllCategories()
    {
        ErrorCategoryCatalogDocumentNormalizer normalizer = new();

        ErrorCategoryCatalogDocument document = new()
        {
            Categories =
            [
                new ErrorCategoryDefinition
                {
                    Name = "network error",
                    DisplayName = " Network error ",
                    Description = " Network related errors. ",
                    Aliases = ["networking", "Network Error"],
                    ParentCategories = ["external communication"],
                    DefaultTags = ["retryable candidate", "server"],
                    DefaultMappings =
                    {
                        ["web.httpStatusCode"] = " 503 "
                    }
                }
            ]
        };

        ErrorCategoryCatalogDocument normalizedDocument = normalizer.Normalize(document);

        ErrorCategoryDefinition category = Assert.Single(normalizedDocument.Categories);

        Assert.Equal("NETWORK_ERROR", category.Name);
        Assert.Equal("Network error", category.DisplayName);
        Assert.Equal("Network related errors.", category.Description);
        Assert.Equal(["NETWORKING", "NETWORK_ERROR"], category.Aliases);
        Assert.Equal(["EXTERNAL_COMMUNICATION"], category.ParentCategories);
        Assert.Equal(["RETRYABLE_CANDIDATE", "SERVER"], category.DefaultTags);
        Assert.Equal("503", category.DefaultMappings["WEB_HTTPSTATUSCODE"]);
    }

    [Fact]
    public void Normalize_ShouldNotModifyOriginalDocument()
    {
        ErrorCategoryCatalogDocumentNormalizer normalizer = new();

        ErrorCategoryCatalogDocument document = new()
        {
            CatalogName = " Category Catalog ",
            Tags = ["default catalog"],
            Categories =
            [
                new ErrorCategoryDefinition
                {
                    Name = "network error",
                    DisplayName = " Network error "
                }
            ]
        };

        ErrorCategoryCatalogDocument normalizedDocument = normalizer.Normalize(document);

        Assert.Equal(" Category Catalog ", document.CatalogName);
        Assert.Equal(["default catalog"], document.Tags);
        Assert.Equal("network error", document.Categories[0].Name);

        Assert.Equal("Category Catalog", normalizedDocument.CatalogName);
        Assert.Equal(["DEFAULT_CATALOG"], normalizedDocument.Tags);
        Assert.Equal("NETWORK_ERROR", normalizedDocument.Categories[0].Name);
    }

    [Fact]
    public void Normalize_ShouldKeepMetadataReference()
    {
        ErrorCategoryCatalogDocumentNormalizer normalizer = new();

        ErrorCategoryCatalogDocument document = new();

        ErrorCategoryCatalogDocument normalizedDocument = normalizer.Normalize(document);

        Assert.Same(document.Metadata, normalizedDocument.Metadata);
    }
}