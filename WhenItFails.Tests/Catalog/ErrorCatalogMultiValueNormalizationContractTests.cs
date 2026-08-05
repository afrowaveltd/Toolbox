using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCatalogMultiValueNormalizationContractTests
{
    [Theory]
    [InlineData("owner", "afro wave")]
    [InlineData("code-prefix", "cfg core")]
    [InlineData("code-group", "configuration group")]
    [InlineData("category", "runtime configuration")]
    [InlineData("subcategory", "required value")]
    [InlineData("tag", "user visible")]
    public void MultiValueLookup_ShouldUseConsistentNormalizedKeys(
        string lookup,
        string key)
    {
        ErrorDefinition definition = new()
        {
            Id = "AFW-TST-0001",
            Name = "NormalizedLookupError",
            Owner = "Afro-Wave",
            CodePrefix = "CFG-CORE",
            CodeGroup = "Configuration-Group",
            PrimaryCategory = "Runtime-Configuration",
            Categories = ["Startup-Flow"],
            Subcategories = ["Required-Value"],
            Tags = ["User-Visible"]
        };

        ErrorCatalog catalog = new([definition]);

        IReadOnlyList<ErrorDefinition> results = lookup switch
        {
            "owner" => catalog.FindByOwner(key),
            "code-prefix" => catalog.FindByCodePrefix(key),
            "code-group" => catalog.FindByCodeGroup(key),
            "category" => catalog.FindByCategory(key),
            "subcategory" => catalog.FindBySubcategory(key),
            "tag" => catalog.FindByTag(key),
            _ => throw new ArgumentOutOfRangeException(nameof(lookup), lookup, null)
        };

        ErrorDefinition result = Assert.Single(results);
        Assert.Same(definition, result);
    }
}
