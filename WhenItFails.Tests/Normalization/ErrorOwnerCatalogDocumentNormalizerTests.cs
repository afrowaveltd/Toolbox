using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.WhenItFails.Tests.Normalization;

public sealed class ErrorOwnerCatalogDocumentNormalizerTests
{
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenOwnerDefinitionNormalizerIsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ErrorOwnerCatalogDocumentNormalizer(null!));
    }

    [Fact]
    public void Normalize_ShouldThrowArgumentNullException_WhenDocumentIsNull()
    {
        ErrorOwnerCatalogDocumentNormalizer normalizer = new();

        Assert.Throws<ArgumentNullException>(() => normalizer.Normalize(null!));
    }

    [Fact]
    public void Normalize_ShouldNormalizeCatalogHeader()
    {
        ErrorOwnerCatalogDocumentNormalizer normalizer = new();

        ErrorOwnerCatalogDocument document = new()
        {
            SchemaVersion = " 1.0 ",
            CatalogId = " owners.catalog ",
            CatalogName = " Owner Catalog ",
            Description = " Owner catalog description. ",
            Language = " en ",
            SourceCatalogId = " source.owners ",
            SourceCatalogVersion = " 1.0 ",
            IsShadowCopy = true,
            Tags = ["default catalog", "default-catalog", "owners"]
        };

        ErrorOwnerCatalogDocument normalizedDocument = normalizer.Normalize(document);

        Assert.Equal("1.0", normalizedDocument.SchemaVersion);
        Assert.Equal("owners.catalog", normalizedDocument.CatalogId);
        Assert.Equal("Owner Catalog", normalizedDocument.CatalogName);
        Assert.Equal("Owner catalog description.", normalizedDocument.Description);
        Assert.Equal("en", normalizedDocument.Language);
        Assert.Equal("source.owners", normalizedDocument.SourceCatalogId);
        Assert.Equal("1.0", normalizedDocument.SourceCatalogVersion);
        Assert.True(normalizedDocument.IsShadowCopy);

        Assert.Equal(
            ["DEFAULT_CATALOG", "OWNERS"],
            normalizedDocument.Tags);
    }

    [Fact]
    public void Normalize_ShouldNormalizeAllOwners()
    {
        ErrorOwnerCatalogDocumentNormalizer normalizer = new();

        ErrorOwnerCatalogDocument document = new()
        {
            Owners =
            [
                new ErrorOwnerDefinition
                {
                    Name = " afw ",
                    DisplayName = " Afrowave ",
                    Description = " Built-in Afrowave owner. ",
                    CodeFrom = 0,
                    CodeTo = 999999,
                    IsBuiltIn = true,
                    Aliases = ["afrowave", "Afro wave"],
                    DefaultMappings =
                    {
                        ["catalog role"] = " built-in "
                    }
                }
            ]
        };

        ErrorOwnerCatalogDocument normalizedDocument = normalizer.Normalize(document);

        ErrorOwnerDefinition owner = Assert.Single(normalizedDocument.Owners);

        Assert.Equal("AFW", owner.Name);
        Assert.Equal("Afrowave", owner.DisplayName);
        Assert.Equal("Built-in Afrowave owner.", owner.Description);
        Assert.Equal(0, owner.CodeFrom);
        Assert.Equal(999999, owner.CodeTo);
        Assert.True(owner.IsBuiltIn);
        Assert.Equal(["AFROWAVE", "AFRO_WAVE"], owner.Aliases);
        Assert.Equal("built-in", owner.DefaultMappings["CATALOG_ROLE"]);
    }

    [Fact]
    public void Normalize_ShouldNotModifyOriginalDocument()
    {
        ErrorOwnerCatalogDocumentNormalizer normalizer = new();

        ErrorOwnerCatalogDocument document = new()
        {
            CatalogName = " Owner Catalog ",
            Tags = ["default catalog"],
            Owners =
            [
                new ErrorOwnerDefinition
                {
                    Name = " afw ",
                    DisplayName = " Afrowave "
                }
            ]
        };

        ErrorOwnerCatalogDocument normalizedDocument = normalizer.Normalize(document);

        Assert.Equal(" Owner Catalog ", document.CatalogName);
        Assert.Equal(["default catalog"], document.Tags);
        Assert.Equal(" afw ", document.Owners[0].Name);

        Assert.Equal("Owner Catalog", normalizedDocument.CatalogName);
        Assert.Equal(["DEFAULT_CATALOG"], normalizedDocument.Tags);
        Assert.Equal("AFW", normalizedDocument.Owners[0].Name);
    }

    [Fact]
    public void Normalize_ShouldKeepMetadataReference()
    {
        ErrorOwnerCatalogDocumentNormalizer normalizer = new();

        ErrorOwnerCatalogDocument document = new();

        ErrorOwnerCatalogDocument normalizedDocument = normalizer.Normalize(document);

        Assert.Same(document.Metadata, normalizedDocument.Metadata);
    }
}