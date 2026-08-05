using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorDefinitionResolverResponseShapeContractTests
{
    [Fact]
    public void FindById_ShouldReturnNullDataAndSingleStableIssue_WhenInputIsInvalid()
    {
        ErrorDefinitionResolver resolver = new();
        ErrorCatalogContext context = CreateContext(out _);

        Response<ErrorDefinition> response = resolver.FindById(context, "   ");

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);

        Assert.Single(response.Issues);
        Assert.Equal("ErrorIdIsEmpty", response.Issues[0].Code);
        Assert.Equal("Error id is empty.", response.Issues[0].Message);
    }

    [Fact]
    public void FindByName_ShouldReturnNullDataAndSingleStableIssue_WhenDefinitionIsMissing()
    {
        ErrorDefinitionResolver resolver = new();
        ErrorCatalogContext context = CreateContext(out _);

        Response<ErrorDefinition> response = resolver.FindByName(context, "MissingError");

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.NotFound, response.Status);
        Assert.Null(response.Data);

        Assert.Single(response.Issues);
        Assert.Equal("ErrorDefinitionNotFoundByName", response.Issues[0].Code);
        Assert.Equal(
            "Error definition with name 'MissingError' was not found.",
            response.Issues[0].Message);
    }

    [Fact]
    public void FindByCode_ShouldReturnExactDefinitionWithoutIssues_WhenDefinitionExists()
    {
        ErrorDefinitionResolver resolver = new();
        ErrorCatalogContext context = CreateContext(out ErrorDefinition expectedDefinition);

        Response<ErrorDefinition> response = resolver.FindByCode(context, expectedDefinition.Code);

        Assert.True(response.IsSuccess);
        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.Same(expectedDefinition, response.Data);
        Assert.Empty(response.Issues);
    }

    private static ErrorCatalogContext CreateContext(out ErrorDefinition definition)
    {
        definition = new ErrorDefinition
        {
            Id = "AFW_TST_0001",
            Code = 900001,
            Name = "TEST_ERROR",
            Owner = "AFW",
            CodePrefix = "TST",
            CodeGroup = "TEST",
            PrimaryCategory = "TEST",
            Categories = ["TEST"],
            Title = "Test error",
            Message = "A test error occurred.",
            DefaultSeverity = "Error"
        };

        return new ErrorCatalogContext
        {
            ErrorCatalog = new ErrorCatalog([definition]),
            CategoryCatalog = new ErrorCategoryCatalogDocument(),
            CodeGroupCatalog = new ErrorCodeGroupCatalogDocument(),
            OwnerCatalog = new ErrorOwnerCatalogDocument(),
            ProfileCatalog = new ErrorProfileCatalogDocument()
        };
    }
}
