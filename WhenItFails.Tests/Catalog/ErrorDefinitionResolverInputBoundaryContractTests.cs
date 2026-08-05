using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorDefinitionResolverInputBoundaryContractTests
{
    [Fact]
    public void FindById_ShouldReturnInvalidResponse_WhenErrorIdIsNullAtRuntime()
    {
        ErrorDefinitionResolver resolver = new();
        ErrorCatalogContext context = CreateContext();

        Response<ErrorDefinition> response = resolver.FindById(context, null!);

        AssertInvalid(response, "ErrorIdIsEmpty");
    }

    [Fact]
    public void FindById_ShouldReturnInvalidResponse_WhenErrorIdIsWhitespace()
    {
        ErrorDefinitionResolver resolver = new();
        ErrorCatalogContext context = CreateContext();

        Response<ErrorDefinition> response = resolver.FindById(context, " \t\r\n ");

        AssertInvalid(response, "ErrorIdIsEmpty");
    }

    [Fact]
    public void FindByName_ShouldReturnInvalidResponse_WhenErrorNameIsNullAtRuntime()
    {
        ErrorDefinitionResolver resolver = new();
        ErrorCatalogContext context = CreateContext();

        Response<ErrorDefinition> response = resolver.FindByName(context, null!);

        AssertInvalid(response, "ErrorNameIsEmpty");
    }

    [Fact]
    public void FindByName_ShouldReturnInvalidResponse_WhenErrorNameIsWhitespace()
    {
        ErrorDefinitionResolver resolver = new();
        ErrorCatalogContext context = CreateContext();

        Response<ErrorDefinition> response = resolver.FindByName(context, " \t\r\n ");

        AssertInvalid(response, "ErrorNameIsEmpty");
    }

    [Fact]
    public void FindByCode_ShouldReturnInvalidResponse_WhenCodeIsNegative()
    {
        ErrorDefinitionResolver resolver = new();
        ErrorCatalogContext context = CreateContext();

        Response<ErrorDefinition> response = resolver.FindByCode(context, -1);

        AssertInvalid(response, "ErrorCodeIsInvalid");
    }

    private static void AssertInvalid(
        Response<ErrorDefinition> response,
        string expectedIssueCode)
    {
        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);

        Assert.NotEmpty(response.Issues);
        Assert.Equal(expectedIssueCode, response.Issues[0].Code);
    }

    private static ErrorCatalogContext CreateContext()
    {
        return new ErrorCatalogContext
        {
            ErrorCatalog = new ErrorCatalog([]),
            CategoryCatalog = new ErrorCategoryCatalogDocument(),
            CodeGroupCatalog = new ErrorCodeGroupCatalogDocument(),
            OwnerCatalog = new ErrorOwnerCatalogDocument(),
            ProfileCatalog = new ErrorProfileCatalogDocument()
        };
    }
}
