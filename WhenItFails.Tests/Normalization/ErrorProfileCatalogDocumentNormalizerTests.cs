using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.WhenItFails.Tests.Normalization;

public sealed class ErrorProfileCatalogDocumentNormalizerTests
{
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenProfileDefinitionNormalizerIsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ErrorProfileCatalogDocumentNormalizer(null!));
    }

    [Fact]
    public void Normalize_ShouldThrowArgumentNullException_WhenDocumentIsNull()
    {
        ErrorProfileCatalogDocumentNormalizer normalizer = new();

        Assert.Throws<ArgumentNullException>(() => normalizer.Normalize(null!));
    }

    [Fact]
    public void Normalize_ShouldNormalizeCatalogHeader()
    {
        ErrorProfileCatalogDocumentNormalizer normalizer = new();

        ErrorProfileCatalogDocument document = new()
        {
            SchemaVersion = " 1.0 ",
            CatalogId = " profiles.catalog ",
            CatalogName = " Profile Catalog ",
            Description = " Profile catalog description. ",
            Language = " en ",
            SourceCatalogId = " source.profiles ",
            SourceCatalogVersion = " 1.0 ",
            IsShadowCopy = true,
            Tags = ["default catalog", "default-catalog", "profiles"]
        };

        ErrorProfileCatalogDocument normalizedDocument = normalizer.Normalize(document);

        Assert.Equal("1.0", normalizedDocument.SchemaVersion);
        Assert.Equal("profiles.catalog", normalizedDocument.CatalogId);
        Assert.Equal("Profile Catalog", normalizedDocument.CatalogName);
        Assert.Equal("Profile catalog description.", normalizedDocument.Description);
        Assert.Equal("en", normalizedDocument.Language);
        Assert.Equal("source.profiles", normalizedDocument.SourceCatalogId);
        Assert.Equal("1.0", normalizedDocument.SourceCatalogVersion);
        Assert.True(normalizedDocument.IsShadowCopy);

        Assert.Equal(
            ["DEFAULT_CATALOG", "PROFILES"],
            normalizedDocument.Tags);
    }

    [Fact]
    public void Normalize_ShouldNormalizeAllProfiles()
    {
        ErrorProfileCatalogDocumentNormalizer normalizer = new();

        ErrorProfileCatalogDocument document = new()
        {
            Profiles =
            [
                new ErrorProfileDefinition
                {
                    Name = " web api ",
                    DisplayName = " Web API ",
                    Description = " Profile for web APIs. ",
                    IncludeOwners = ["afw"],
                    IncludeCodeGroups = ["configuration", "validation"],
                    IncludeCategories = ["web", "server"],
                    IncludeSubcategories = ["required value"],
                    IncludeTags = ["user visible"],
                    ExcludeTags = ["internal only"],
                    DefaultMappings =
                    {
                        ["web.problemDetails"] = " true "
                    }
                }
            ]
        };

        ErrorProfileCatalogDocument normalizedDocument = normalizer.Normalize(document);

        ErrorProfileDefinition profile = Assert.Single(normalizedDocument.Profiles);

        Assert.Equal("WEB_API", profile.Name);
        Assert.Equal("Web API", profile.DisplayName);
        Assert.Equal("Profile for web APIs.", profile.Description);
        Assert.Equal(["AFW"], profile.IncludeOwners);
        Assert.Equal(["CONFIGURATION", "VALIDATION"], profile.IncludeCodeGroups);
        Assert.Equal(["WEB", "SERVER"], profile.IncludeCategories);
        Assert.Equal(["REQUIRED_VALUE"], profile.IncludeSubcategories);
        Assert.Equal(["USER_VISIBLE"], profile.IncludeTags);
        Assert.Equal(["INTERNAL_ONLY"], profile.ExcludeTags);
        Assert.Equal("true", profile.DefaultMappings["WEB_PROBLEMDETAILS"]);
    }

    [Fact]
    public void Normalize_ShouldNotModifyOriginalDocument()
    {
        ErrorProfileCatalogDocumentNormalizer normalizer = new();

        ErrorProfileCatalogDocument document = new()
        {
            CatalogName = " Profile Catalog ",
            Tags = ["default catalog"],
            Profiles =
            [
                new ErrorProfileDefinition
                {
                    Name = " web api ",
                    DisplayName = " Web API ",
                    IncludeTags = ["user visible"]
                }
            ]
        };

        ErrorProfileCatalogDocument normalizedDocument = normalizer.Normalize(document);

        Assert.Equal(" Profile Catalog ", document.CatalogName);
        Assert.Equal(["default catalog"], document.Tags);
        Assert.Equal(" web api ", document.Profiles[0].Name);
        Assert.Equal(["user visible"], document.Profiles[0].IncludeTags);

        Assert.Equal("Profile Catalog", normalizedDocument.CatalogName);
        Assert.Equal(["DEFAULT_CATALOG"], normalizedDocument.Tags);
        Assert.Equal("WEB_API", normalizedDocument.Profiles[0].Name);
        Assert.Equal(["USER_VISIBLE"], normalizedDocument.Profiles[0].IncludeTags);
    }

    [Fact]
    public void Normalize_ShouldKeepMetadataReference()
    {
        ErrorProfileCatalogDocumentNormalizer normalizer = new();

        ErrorProfileCatalogDocument document = new();

        ErrorProfileCatalogDocument normalizedDocument = normalizer.Normalize(document);

        Assert.Same(document.Metadata, normalizedDocument.Metadata);
    }
}