using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCatalogContextContractTests
{
    [Fact]
    public void Constructor_UsesNullDefaultsForRequiredReferences()
    {
        ErrorCatalogContext context = new();

        Assert.Null(context.ErrorCatalog);
        Assert.Null(context.ErrorCatalogDocument);
        Assert.Null(context.CrossValidationResult);
        Assert.Null(context.CategoryCatalog);
        Assert.Null(context.CodeGroupCatalog);
        Assert.Null(context.OwnerCatalog);
        Assert.Null(context.ProfileCatalog);
    }

    [Fact]
    public void Properties_PreserveAssignedInstances()
    {
        ErrorCatalog errorCatalog = new(Array.Empty<ErrorDefinition>());
        ErrorCatalogDocument errorCatalogDocument = new();
        ErrorCatalogValidationResult crossValidationResult = new();
        ErrorCategoryCatalogDocument categoryCatalog = new();
        ErrorCodeGroupCatalogDocument codeGroupCatalog = new();
        ErrorOwnerCatalogDocument ownerCatalog = new();
        ErrorProfileCatalogDocument profileCatalog = new();

        ErrorCatalogContext context = new()
        {
            ErrorCatalog = errorCatalog,
            ErrorCatalogDocument = errorCatalogDocument,
            CrossValidationResult = crossValidationResult,
            CategoryCatalog = categoryCatalog,
            CodeGroupCatalog = codeGroupCatalog,
            OwnerCatalog = ownerCatalog,
            ProfileCatalog = profileCatalog
        };

        Assert.Same(errorCatalog, context.ErrorCatalog);
        Assert.Same(errorCatalogDocument, context.ErrorCatalogDocument);
        Assert.Same(crossValidationResult, context.CrossValidationResult);
        Assert.Same(categoryCatalog, context.CategoryCatalog);
        Assert.Same(codeGroupCatalog, context.CodeGroupCatalog);
        Assert.Same(ownerCatalog, context.OwnerCatalog);
        Assert.Same(profileCatalog, context.ProfileCatalog);
    }
}
