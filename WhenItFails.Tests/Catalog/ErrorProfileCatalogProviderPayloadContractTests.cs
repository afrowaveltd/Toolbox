using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorProfileCatalogProviderPayloadContractTests
{
    [Fact]
    public void Constructor_UsesNullDefaultsForRequiredReferences()
    {
        ErrorProfileCatalogProviderPayload payload = new();

        Assert.Null(payload.Document);
        Assert.Null(payload.ValidationResult);
    }

    [Fact]
    public void Properties_PreserveAssignedInstances()
    {
        ErrorProfileCatalogDocument document = new();
        ErrorCatalogValidationResult validationResult = new();

        ErrorProfileCatalogProviderPayload payload = new()
        {
            Document = document,
            ValidationResult = validationResult
        };

        Assert.Same(document, payload.Document);
        Assert.Same(validationResult, payload.ValidationResult);
    }
}
