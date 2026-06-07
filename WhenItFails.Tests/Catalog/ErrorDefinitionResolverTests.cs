using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorDefinitionResolverTests
{
    [Fact]
    public void FindById_ShouldReturnInvalidResponse_WhenContextIsNull()
    {
        ErrorDefinitionResolver resolver = new();

        Response<ErrorDefinition> response =
            resolver.FindById(null, "AFW-GEN-0001");

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Equal("ErrorCatalogContextIsNull", response.Issues[0].Code);
    }

    [Fact]
    public void FindById_ShouldReturnInvalidResponse_WhenErrorCatalogIsNull()
    {
        ErrorDefinitionResolver resolver = new();

        ErrorCatalogContext context = new();

        Response<ErrorDefinition> response =
            resolver.FindById(context, "AFW-GEN-0001");

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Equal("ErrorCatalogIsNull", response.Issues[0].Code);
    }

    [Fact]
    public void FindById_ShouldReturnInvalidResponse_WhenErrorIdIsEmpty()
    {
        ErrorDefinitionResolver resolver = new();

        ErrorCatalogContext context = CreateContext();

        Response<ErrorDefinition> response =
            resolver.FindById(context, string.Empty);

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Equal("ErrorIdIsEmpty", response.Issues[0].Code);
    }

    [Fact]
    public void FindById_ShouldReturnNotFoundResponse_WhenErrorDoesNotExist()
    {
        ErrorDefinitionResolver resolver = new();

        ErrorCatalogContext context = CreateContext();

        Response<ErrorDefinition> response =
            resolver.FindById(context, "AFW-GEN-9999");

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.NotFound, response.Status);
        Assert.Equal("ErrorDefinitionNotFoundById", response.Issues[0].Code);
    }

    [Fact]
    public void FindById_ShouldReturnErrorDefinition_WhenErrorExists()
    {
        ErrorDefinitionResolver resolver = new();

        ErrorCatalogContext context = CreateContext();

        Response<ErrorDefinition> response =
            resolver.FindById(context, "AFW-GEN-0001");

        Assert.True(response.IsSuccess);
        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.NotNull(response.Data);
        Assert.Equal("AFW_GEN_0001", response.Data.Id);
        Assert.Equal("UNKNOWNERROR", response.Data.Name);
    }

    [Fact]
    public void FindById_ShouldNormalizeInputBeforeSearch()
    {
        ErrorDefinitionResolver resolver = new();

        ErrorCatalogContext context = CreateContext();

        Response<ErrorDefinition> response =
            resolver.FindById(context, "afw gen 0001");

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.Equal("AFW_GEN_0001", response.Data.Id);
    }

    [Fact]
    public void FindByName_ShouldReturnInvalidResponse_WhenContextIsNull()
    {
        ErrorDefinitionResolver resolver = new();

        Response<ErrorDefinition> response =
            resolver.FindByName(null, "UnknownError");

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Equal("ErrorCatalogContextIsNull", response.Issues[0].Code);
    }

    [Fact]
    public void FindByName_ShouldReturnInvalidResponse_WhenErrorCatalogIsNull()
    {
        ErrorDefinitionResolver resolver = new();

        ErrorCatalogContext context = new();

        Response<ErrorDefinition> response =
            resolver.FindByName(context, "UnknownError");

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Equal("ErrorCatalogIsNull", response.Issues[0].Code);
    }

    [Fact]
    public void FindByName_ShouldReturnInvalidResponse_WhenErrorNameIsEmpty()
    {
        ErrorDefinitionResolver resolver = new();

        ErrorCatalogContext context = CreateContext();

        Response<ErrorDefinition> response =
            resolver.FindByName(context, string.Empty);

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Equal("ErrorNameIsEmpty", response.Issues[0].Code);
    }

    [Fact]
    public void FindByName_ShouldReturnNotFoundResponse_WhenErrorDoesNotExist()
    {
        ErrorDefinitionResolver resolver = new();

        ErrorCatalogContext context = CreateContext();

        Response<ErrorDefinition> response =
            resolver.FindByName(context, "DoesNotExist");

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.NotFound, response.Status);
        Assert.Equal("ErrorDefinitionNotFoundByName", response.Issues[0].Code);
    }

    [Fact]
    public void FindByName_ShouldReturnErrorDefinition_WhenErrorExists()
    {
        ErrorDefinitionResolver resolver = new();

        ErrorCatalogContext context = CreateContext();

        Response<ErrorDefinition> response =
            resolver.FindByName(context, "UnknownError");

        Assert.True(response.IsSuccess);
        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.NotNull(response.Data);
        Assert.Equal("UNKNOWNERROR", response.Data.Name);
        Assert.Equal(100001, response.Data.Code);
    }

    [Fact]
    public void FindByName_ShouldNormalizeInputBeforeSearch()
    {
        ErrorDefinitionResolver resolver = new();

        ErrorCatalogContext context = CreateContext();

        Response<ErrorDefinition> response =
            resolver.FindByName(context, "unknown error");

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.Equal("UNKNOWN_ERROR", response.Data.Name);
    }

    [Fact]
    public void FindByCode_ShouldReturnInvalidResponse_WhenContextIsNull()
    {
        ErrorDefinitionResolver resolver = new();

        Response<ErrorDefinition> response =
            resolver.FindByCode(null, 100001);

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Equal("ErrorCatalogContextIsNull", response.Issues[0].Code);
    }

    [Fact]
    public void FindByCode_ShouldReturnInvalidResponse_WhenErrorCatalogIsNull()
    {
        ErrorDefinitionResolver resolver = new();

        ErrorCatalogContext context = new();

        Response<ErrorDefinition> response =
            resolver.FindByCode(context, 100001);

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Equal("ErrorCatalogIsNull", response.Issues[0].Code);
    }

    [Fact]
    public void FindByCode_ShouldReturnInvalidResponse_WhenCodeIsInvalid()
    {
        ErrorDefinitionResolver resolver = new();

        ErrorCatalogContext context = CreateContext();

        Response<ErrorDefinition> response =
            resolver.FindByCode(context, 0);

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Equal("ErrorCodeIsInvalid", response.Issues[0].Code);
    }

    [Fact]
    public void FindByCode_ShouldReturnNotFoundResponse_WhenErrorDoesNotExist()
    {
        ErrorDefinitionResolver resolver = new();

        ErrorCatalogContext context = CreateContext();

        Response<ErrorDefinition> response =
            resolver.FindByCode(context, 999999);

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.NotFound, response.Status);
        Assert.Equal("ErrorDefinitionNotFoundByCode", response.Issues[0].Code);
    }

    [Fact]
    public void FindByCode_ShouldReturnErrorDefinition_WhenErrorExists()
    {
        ErrorDefinitionResolver resolver = new();

        ErrorCatalogContext context = CreateContext();

        Response<ErrorDefinition> response =
            resolver.FindByCode(context, 100001);

        Assert.True(response.IsSuccess);
        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.NotNull(response.Data);
        Assert.Equal("AFW_GEN_0001", response.Data.Id);
        Assert.Equal("UNKNOWNERROR", response.Data.Name);
    }

    private static ErrorCatalogContext CreateContext()
    {
        ErrorDefinition unknownError = new()
        {
            Id = "AFW_GEN_0001",
            Code = 100001,
            Name = "UNKNOWNERROR",
            Owner = "AFW",
            CodePrefix = "GEN",
            CodeGroup = "GENERAL",
            PrimaryCategory = "GENERAL",
            Categories = ["GENERAL"],
            Title = "Unknown error",
            Message = "An unknown error occurred.",
            DefaultSeverity = "Error"
        };

        ErrorDefinition unknownErrorWithSeparatedName = new()
        {
            Id = "AFW_GEN_0002",
            Code = 100002,
            Name = "UNKNOWN_ERROR",
            Owner = "AFW",
            CodePrefix = "GEN",
            CodeGroup = "GENERAL",
            PrimaryCategory = "GENERAL",
            Categories = ["GENERAL"],
            Title = "Unknown separated error",
            Message = "An unknown separated error occurred.",
            DefaultSeverity = "Error"
        };

        ErrorCatalog catalog = new(
        [
            unknownError,
            unknownErrorWithSeparatedName
        ]);

        return new ErrorCatalogContext
        {
            ErrorCatalog = catalog,
            CategoryCatalog = new ErrorCategoryCatalogDocument(),
            CodeGroupCatalog = new ErrorCodeGroupCatalogDocument(),
            OwnerCatalog = new ErrorOwnerCatalogDocument(),
            ProfileCatalog = new ErrorProfileCatalogDocument()
        };
    }
}