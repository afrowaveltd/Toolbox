using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.WhenItFails.Tests.Normalization;

public sealed class ErrorCodeGroupCatalogDocumentNormalizerTests
{
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenCodeGroupDefinitionNormalizerIsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ErrorCodeGroupCatalogDocumentNormalizer(null!));
    }

    [Fact]
    public void Normalize_ShouldThrowArgumentNullException_WhenDocumentIsNull()
    {
        ErrorCodeGroupCatalogDocumentNormalizer normalizer = new();

        Assert.Throws<ArgumentNullException>(() => normalizer.Normalize(null!));
    }

    [Fact]
    public void Normalize_ShouldNormalizeCatalogHeader()
    {
        ErrorCodeGroupCatalogDocumentNormalizer normalizer = new();

        ErrorCodeGroupCatalogDocument document = new()
        {
            SchemaVersion = " 1.0 ",
            CatalogId = " code-groups.catalog ",
            CatalogName = " Code Group Catalog ",
            Description = " Code group catalog description. ",
            Language = " en ",
            SourceCatalogId = " source.code-groups ",
            SourceCatalogVersion = " 1.0 ",
            IsShadowCopy = true,
            Tags = ["default catalog", "default-catalog", "code groups"]
        };

        ErrorCodeGroupCatalogDocument normalizedDocument = normalizer.Normalize(document);

        Assert.Equal("1.0", normalizedDocument.SchemaVersion);
        Assert.Equal("code-groups.catalog", normalizedDocument.CatalogId);
        Assert.Equal("Code Group Catalog", normalizedDocument.CatalogName);
        Assert.Equal("Code group catalog description.", normalizedDocument.Description);
        Assert.Equal("en", normalizedDocument.Language);
        Assert.Equal("source.code-groups", normalizedDocument.SourceCatalogId);
        Assert.Equal("1.0", normalizedDocument.SourceCatalogVersion);
        Assert.True(normalizedDocument.IsShadowCopy);

        Assert.Equal(
            ["DEFAULT_CATALOG", "CODE_GROUPS"],
            normalizedDocument.Tags);
    }

    [Fact]
    public void Normalize_ShouldNormalizeAllCodeGroups()
    {
        ErrorCodeGroupCatalogDocumentNormalizer normalizer = new();

        ErrorCodeGroupCatalogDocument document = new()
        {
            CodeGroups =
            [
                new ErrorCodeGroupDefinition
                {
                    Name = "configuration errors",
                    DisplayName = " Configuration errors ",
                    CodePrefix = " cfg ",
                    CodeFrom = 200000,
                    CodeTo = 299999,
                    Description = " Configuration related errors. ",
                    DefaultCategories = ["configuration", "startup"],
                    DefaultTags = ["settings", "user visible"],
                    DefaultMappings =
                    {
                        ["default severity"] = " Error "
                    }
                }
            ]
        };

        ErrorCodeGroupCatalogDocument normalizedDocument = normalizer.Normalize(document);

        ErrorCodeGroupDefinition codeGroup = Assert.Single(normalizedDocument.CodeGroups);

        Assert.Equal("CONFIGURATION_ERRORS", codeGroup.Name);
        Assert.Equal("Configuration errors", codeGroup.DisplayName);
        Assert.Equal("CFG", codeGroup.CodePrefix);
        Assert.Equal(200000, codeGroup.CodeFrom);
        Assert.Equal(299999, codeGroup.CodeTo);
        Assert.Equal("Configuration related errors.", codeGroup.Description);
        Assert.Equal(["CONFIGURATION", "STARTUP"], codeGroup.DefaultCategories);
        Assert.Equal(["SETTINGS", "USER_VISIBLE"], codeGroup.DefaultTags);
        Assert.Equal("Error", codeGroup.DefaultMappings["DEFAULT_SEVERITY"]);
    }

    [Fact]
    public void Normalize_ShouldNotModifyOriginalDocument()
    {
        ErrorCodeGroupCatalogDocumentNormalizer normalizer = new();

        ErrorCodeGroupCatalogDocument document = new()
        {
            CatalogName = " Code Group Catalog ",
            Tags = ["default catalog"],
            CodeGroups =
            [
                new ErrorCodeGroupDefinition
                {
                    Name = "configuration errors",
                    CodePrefix = " cfg "
                }
            ]
        };

        ErrorCodeGroupCatalogDocument normalizedDocument = normalizer.Normalize(document);

        Assert.Equal(" Code Group Catalog ", document.CatalogName);
        Assert.Equal(["default catalog"], document.Tags);
        Assert.Equal("configuration errors", document.CodeGroups[0].Name);
        Assert.Equal(" cfg ", document.CodeGroups[0].CodePrefix);

        Assert.Equal("Code Group Catalog", normalizedDocument.CatalogName);
        Assert.Equal(["DEFAULT_CATALOG"], normalizedDocument.Tags);
        Assert.Equal("CONFIGURATION_ERRORS", normalizedDocument.CodeGroups[0].Name);
        Assert.Equal("CFG", normalizedDocument.CodeGroups[0].CodePrefix);
    }

    [Fact]
    public void Normalize_ShouldKeepMetadataReference()
    {
        ErrorCodeGroupCatalogDocumentNormalizer normalizer = new();

        ErrorCodeGroupCatalogDocument document = new();

        ErrorCodeGroupCatalogDocument normalizedDocument = normalizer.Normalize(document);

        Assert.Same(document.Metadata, normalizedDocument.Metadata);
    }
}