using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorOwnerCatalogProviderPayloadContractTests
{
    [Fact]
    public void Constructor_UsesNullDefaultsForRequiredReferences()
    {
        ErrorOwnerCatalogProviderPayload payload = new();

        Assert.Null(payload.Document);
        Assert.Null(payload.ValidationResult);
    }

    [Fact]
    public void Properties_PreserveAssignedInstances()
    {
        ErrorOwnerCatalogDocument document = new();
        ErrorCatalogValidationResult validationResult = new();

        ErrorOwnerCatalogProviderPayload payload = new()
        {
            Document = document,
            ValidationResult = validationResult
        };

        Assert.Same(document, payload.Document);
        Assert.Same(validationResult, payload.ValidationResult);
    }
}
