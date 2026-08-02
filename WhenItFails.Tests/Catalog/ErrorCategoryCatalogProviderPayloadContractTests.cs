using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCategoryCatalogProviderPayloadContractTests
{
    [Fact]
    public void NewPayload_UsesNullReferenceDefaults()
    {
        ErrorCategoryCatalogProviderPayload payload = new();

        Assert.Null(payload.Document);
        Assert.Null(payload.ValidationResult);
    }

    [Fact]
    public void AssignedReferences_ArePreservedExactly()
    {
        ErrorCategoryCatalogDocument document = new();
        ErrorCatalogValidationResult validationResult = new();

        ErrorCategoryCatalogProviderPayload payload = new()
        {
            Document = document,
            ValidationResult = validationResult
        };

        Assert.Same(document, payload.Document);
        Assert.Same(validationResult, payload.ValidationResult);
    }
}
