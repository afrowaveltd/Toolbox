using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Descriptors;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Services;

namespace Afrowave.Toolbox.WhenItFails.Tests.Services;

public sealed class ErrorDescriptorServiceTests
{
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenDescriptorResolverIsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ErrorDescriptorService(null!));
    }

    [Fact]
    public void FromId_ShouldReturnResolverResponse()
    {
        ErrorDescriptor expectedDescriptor = CreateDescriptor();

        FakeErrorDescriptorResolver resolver = new(
            Response<ErrorDescriptor>.Ok(expectedDescriptor));

        ErrorDescriptorService service = new(resolver);

        ErrorCatalogContext context = new();

        Response<ErrorDescriptor> response =
            service.FromId(context, "AFW-CFG-0001");

        Assert.True(response.IsSuccess);
        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.Same(expectedDescriptor, response.Data);

        Assert.Equal("CreateById", resolver.LastCalledMethod);
        Assert.Same(context, resolver.LastContext);
        Assert.Equal("AFW-CFG-0001", resolver.LastTextValue);
    }

    [Fact]
    public void FromName_ShouldReturnResolverResponse()
    {
        ErrorDescriptor expectedDescriptor = CreateDescriptor();

        FakeErrorDescriptorResolver resolver = new(
            Response<ErrorDescriptor>.Ok(expectedDescriptor));

        ErrorDescriptorService service = new(resolver);

        ErrorCatalogContext context = new();

        Response<ErrorDescriptor> response =
            service.FromName(context, "MissingConfigurationValue");

        Assert.True(response.IsSuccess);
        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.Same(expectedDescriptor, response.Data);

        Assert.Equal("CreateByName", resolver.LastCalledMethod);
        Assert.Same(context, resolver.LastContext);
        Assert.Equal("MissingConfigurationValue", resolver.LastTextValue);
    }

    [Fact]
    public void FromCode_ShouldReturnResolverResponse()
    {
        ErrorDescriptor expectedDescriptor = CreateDescriptor();

        FakeErrorDescriptorResolver resolver = new(
            Response<ErrorDescriptor>.Ok(expectedDescriptor));

        ErrorDescriptorService service = new(resolver);

        ErrorCatalogContext context = new();

        Response<ErrorDescriptor> response =
            service.FromCode(context, 200001);

        Assert.True(response.IsSuccess);
        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.Same(expectedDescriptor, response.Data);

        Assert.Equal("CreateByCode", resolver.LastCalledMethod);
        Assert.Same(context, resolver.LastContext);
        Assert.Equal(200001, resolver.LastCode);
    }

    [Fact]
    public void FromId_ShouldReturnNotFoundResponse_WhenResolverReturnsNotFound()
    {
        FakeErrorDescriptorResolver resolver = new(
            Response<ErrorDescriptor>.NotFound(
                code: "ErrorDefinitionNotFoundById",
                message: "Error definition was not found."));

        ErrorDescriptorService service = new(resolver);

        Response<ErrorDescriptor> response =
            service.FromId(new ErrorCatalogContext(), "AFW-CFG-9999");

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.NotFound, response.Status);
        Assert.Null(response.Data);
        Assert.NotEmpty(response.Issues);
        Assert.Equal("ErrorDefinitionNotFoundById", response.Issues[0].Code);
    }

    [Fact]
    public void FromName_ShouldReturnInvalidResponse_WhenResolverReturnsInvalid()
    {
        FakeErrorDescriptorResolver resolver = new(
            Response<ErrorDescriptor>.Invalid(
                code: "ErrorNameIsEmpty",
                message: "Error name is empty."));

        ErrorDescriptorService service = new(resolver);

        Response<ErrorDescriptor> response =
            service.FromName(new ErrorCatalogContext(), string.Empty);

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);
        Assert.NotEmpty(response.Issues);
        Assert.Equal("ErrorNameIsEmpty", response.Issues[0].Code);
    }

    [Fact]
    public void FromCode_ShouldReturnInvalidResponse_WhenResolverReturnsInvalid()
    {
        FakeErrorDescriptorResolver resolver = new(
            Response<ErrorDescriptor>.Invalid(
                code: "ErrorCodeIsInvalid",
                message: "Error code must be greater than zero."));

        ErrorDescriptorService service = new(resolver);

        Response<ErrorDescriptor> response =
            service.FromCode(new ErrorCatalogContext(), 0);

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);
        Assert.NotEmpty(response.Issues);
        Assert.Equal("ErrorCodeIsInvalid", response.Issues[0].Code);
    }

    private static ErrorDescriptor CreateDescriptor()
    {
        return new ErrorDescriptor
        {
            Id = "AFW_CFG_0001",
            Code = 200001,
            Name = "MISSING_CONFIGURATION_VALUE",

            Owner = "AFW",
            CodePrefix = "CFG",
            CodeGroup = "CONFIGURATION",

            PrimaryCategory = "CONFIGURATION",
            Categories = ["CONFIGURATION", "STARTUP", "VALIDATION"],
            Subcategories = ["REQUIRED_VALUE", "APP_SETTINGS"],

            Title = "Missing configuration value",
            Message = "A required configuration value is missing.",
            Severity = "Error",

            DeveloperHint = "Check application configuration.",
            DocumentationKey = "errors.configuration.missing-value",

            Tags = ["CONFIGURATION", "STARTUP", "USER_VISIBLE"]
        };
    }

    private sealed class FakeErrorDescriptorResolver : IErrorDescriptorResolver
    {
        private readonly Response<ErrorDescriptor> _response;

        public FakeErrorDescriptorResolver(Response<ErrorDescriptor> response)
        {
            _response = response;
        }

        public string? LastCalledMethod { get; private set; }

        public ErrorCatalogContext? LastContext { get; private set; }

        public string? LastTextValue { get; private set; }

        public int? LastCode { get; private set; }

        public Response<ErrorDescriptor> CreateById(
            ErrorCatalogContext? context,
            string errorId)
        {
            LastCalledMethod = "CreateById";
            LastContext = context;
            LastTextValue = errorId;
            LastCode = null;

            return _response;
        }

        public Response<ErrorDescriptor> CreateByName(
            ErrorCatalogContext? context,
            string errorName)
        {
            LastCalledMethod = "CreateByName";
            LastContext = context;
            LastTextValue = errorName;
            LastCode = null;

            return _response;
        }

        public Response<ErrorDescriptor> CreateByCode(
            ErrorCatalogContext? context,
            int code)
        {
            LastCalledMethod = "CreateByCode";
            LastContext = context;
            LastTextValue = null;
            LastCode = code;

            return _response;
        }
    }
}