using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Descriptors;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Descriptors;

public sealed class ErrorDescriptorResolverTests
{
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenDefinitionResolverIsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ErrorDescriptorResolver(
                null!,
                new ErrorDescriptorFactory()));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenDescriptorFactoryIsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ErrorDescriptorResolver(
                new FakeErrorDefinitionResolver(),
                null!));
    }

    [Fact]
    public void CreateById_ShouldReturnDescriptor_WhenDefinitionIsResolved()
    {
        ErrorDescriptorResolver resolver = new(
            new FakeErrorDefinitionResolver(),
            new ErrorDescriptorFactory());

        Response<ErrorDescriptor> response =
            resolver.CreateById(
                new ErrorCatalogContext(),
                "AFW-CFG-0001");

        Assert.True(response.IsSuccess);
        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.NotNull(response.Data);

        Assert.Equal("AFW_CFG_0001", response.Data.Id);
        Assert.Equal(200001, response.Data.Code);
        Assert.Equal("MISSING_CONFIGURATION_VALUE", response.Data.Name);
        Assert.Equal("Error", response.Data.Severity);
    }

    [Fact]
    public void CreateByName_ShouldReturnDescriptor_WhenDefinitionIsResolved()
    {
        ErrorDescriptorResolver resolver = new(
            new FakeErrorDefinitionResolver(),
            new ErrorDescriptorFactory());

        Response<ErrorDescriptor> response =
            resolver.CreateByName(
                new ErrorCatalogContext(),
                "Missing configuration value");

        Assert.True(response.IsSuccess);
        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.NotNull(response.Data);

        Assert.Equal("AFW_CFG_0001", response.Data.Id);
        Assert.Equal("MISSING_CONFIGURATION_VALUE", response.Data.Name);
    }

    [Fact]
    public void CreateByCode_ShouldReturnDescriptor_WhenDefinitionIsResolved()
    {
        ErrorDescriptorResolver resolver = new(
            new FakeErrorDefinitionResolver(),
            new ErrorDescriptorFactory());

        Response<ErrorDescriptor> response =
            resolver.CreateByCode(
                new ErrorCatalogContext(),
                200001);

        Assert.True(response.IsSuccess);
        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.NotNull(response.Data);

        Assert.Equal("AFW_CFG_0001", response.Data.Id);
        Assert.Equal(200001, response.Data.Code);
    }

    [Fact]
    public void CreateById_ShouldReturnSourceStatus_WhenDefinitionResolverReturnsNotFound()
    {
        ErrorDescriptorResolver resolver = new(
            new FakeErrorDefinitionResolver(
                Response<ErrorDefinition>.NotFound(
                    code: "ErrorDefinitionNotFoundById",
                    message: "Error definition was not found.")),
            new ErrorDescriptorFactory());

        Response<ErrorDescriptor> response =
            resolver.CreateById(
                new ErrorCatalogContext(),
                "AFW-CFG-9999");

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.NotFound, response.Status);
        Assert.Null(response.Data);
        Assert.NotEmpty(response.Issues);
        Assert.Equal("ErrorDefinitionNotFoundById", response.Issues[0].Code);
    }

    [Fact]
    public void CreateByName_ShouldReturnSourceStatus_WhenDefinitionResolverReturnsInvalid()
    {
        ErrorDescriptorResolver resolver = new(
            new FakeErrorDefinitionResolver(
                Response<ErrorDefinition>.Invalid(
                    code: "ErrorNameIsEmpty",
                    message: "Error name is empty.")),
            new ErrorDescriptorFactory());

        Response<ErrorDescriptor> response =
            resolver.CreateByName(
                new ErrorCatalogContext(),
                string.Empty);

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);
        Assert.NotEmpty(response.Issues);
        Assert.Equal("ErrorNameIsEmpty", response.Issues[0].Code);
    }

    [Fact]
    public void CreateByCode_ShouldReturnInvalidResponse_WhenResolvedDefinitionIsNull()
    {
        ErrorDescriptorResolver resolver = new(
            new FakeErrorDefinitionResolver(
                Response<ErrorDefinition>.Ok(null)),
            new ErrorDescriptorFactory());

        Response<ErrorDescriptor> response =
            resolver.CreateByCode(
                new ErrorCatalogContext(),
                200001);

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);
        Assert.NotEmpty(response.Issues);
        Assert.Equal("ResolvedErrorDefinitionIsNull", response.Issues[0].Code);
    }

    [Fact]
    public void CreateById_ShouldUseDescriptorFactory()
    {
        FakeErrorDescriptorFactory descriptorFactory = new();

        ErrorDescriptorResolver resolver = new(
            new FakeErrorDefinitionResolver(),
            descriptorFactory);

        Response<ErrorDescriptor> response =
            resolver.CreateById(
                new ErrorCatalogContext(),
                "AFW-CFG-0001");

        Assert.True(response.IsSuccess);
        Assert.True(descriptorFactory.WasCalled);
        Assert.NotNull(descriptorFactory.LastDefinition);
        Assert.Equal("AFW_CFG_0001", descriptorFactory.LastDefinition.Id);
    }

    private static ErrorDefinition CreateDefinition()
    {
        return new ErrorDefinition
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
            DefaultSeverity = "Error",

            DeveloperHint = "Check application configuration.",
            DocumentationKey = "errors.configuration.missing-value",

            Tags = ["CONFIGURATION", "STARTUP", "USER_VISIBLE"]
        };
    }

    private sealed class FakeErrorDefinitionResolver : IErrorDefinitionResolver
    {
        private readonly Response<ErrorDefinition> _response;

        public FakeErrorDefinitionResolver(
            Response<ErrorDefinition>? response = null)
        {
            _response = response ?? Response<ErrorDefinition>.Ok(
                CreateDefinition());
        }

        public Response<ErrorDefinition> FindById(
            ErrorCatalogContext? context,
            string errorId)
        {
            return _response;
        }

        public Response<ErrorDefinition> FindByName(
            ErrorCatalogContext? context,
            string errorName)
        {
            return _response;
        }

        public Response<ErrorDefinition> FindByCode(
            ErrorCatalogContext? context,
            int code)
        {
            return _response;
        }
    }

    private sealed class FakeErrorDescriptorFactory : IErrorDescriptorFactory
    {
        public bool WasCalled { get; private set; }

        public ErrorDefinition? LastDefinition { get; private set; }

        public ErrorDescriptor Create(ErrorDefinition definition)
        {
            WasCalled = true;
            LastDefinition = definition;

            return new ErrorDescriptor
            {
                Id = definition.Id,
                Code = definition.Code,
                Name = definition.Name,
                Owner = definition.Owner,
                CodePrefix = definition.CodePrefix,
                CodeGroup = definition.CodeGroup,
                PrimaryCategory = definition.PrimaryCategory,
                Categories = [.. definition.Categories],
                Subcategories = [.. definition.Subcategories],
                Title = definition.Title,
                Message = definition.Message,
                Severity = definition.DefaultSeverity,
                DeveloperHint = definition.DeveloperHint,
                DocumentationKey = definition.DocumentationKey,
                Tags = [.. definition.Tags],
                Metadata = definition.Metadata
            };
        }
    }
}