using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCatalogProviderPayloadContractTests
{
    [Fact]
    public void NewInstance_ExposesNullDefaultsForRequiredReferences()
    {
        ErrorCatalogProviderPayload payload = new();

        Assert.Null(payload.Catalog);
        Assert.Null(payload.Document);
        Assert.Null(payload.ValidationResult);
    }

    [Fact]
    public void AssignedReferences_ArePreservedExactly()
    {
        ErrorCatalog catalog = new(Array.Empty<ErrorDefinition>());
        ErrorCatalogDocument document = new();
        ErrorCatalogValidationResult validationResult = new();

        ErrorCatalogProviderPayload payload = new()
        {
            Catalog = catalog,
            Document = document,
            ValidationResult = validationResult
        };

        Assert.Same(catalog, payload.Catalog);
        Assert.Same(document, payload.Document);
        Assert.Same(validationResult, payload.ValidationResult);
    }
}