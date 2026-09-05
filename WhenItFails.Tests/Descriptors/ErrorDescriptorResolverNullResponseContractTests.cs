using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Descriptors;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Descriptors;

public sealed class ErrorDescriptorResolverNullResponseContractTests
{
    [Fact]
    public void CreateById_ShouldReturnStableInvalidResponse_WhenDefinitionResolverReturnsNull()
    {
        ErrorDescriptorResolver resolver = CreateResolver();

        Response<ErrorDescriptor> response = resolver.CreateById(
            new ErrorCatalogContext(),
            "AFW-CFG-0001");

        AssertStableInvalidNullResolverResponse(response);
    }

    [Fact]
    public void CreateByName_ShouldReturnStableInvalidResponse_WhenDefinitionResolverReturnsNull()
    {
        ErrorDescriptorResolver resolver = CreateResolver();

        Response<ErrorDescriptor> response = resolver.CreateByName(
            new ErrorCatalogContext(),
            "MissingConfigurationValue");

        AssertStableInvalidNullResolverResponse(response);
    }

    [Fact]
    public void CreateByCode_ShouldReturnStableInvalidResponse_WhenDefinitionResolverReturnsNull()
    {
        ErrorDescriptorResolver resolver = CreateResolver();

        Response<ErrorDescriptor> response = resolver.CreateByCode(
            new ErrorCatalogContext(),
            200001);

        AssertStableInvalidNullResolverResponse(response);
    }

    private static ErrorDescriptorResolver CreateResolver()
    {
        return new ErrorDescriptorResolver(
            new NullResponseDefinitionResolver(),
            new ThrowingErrorDescriptorFactory());
    }

    private static void AssertStableInvalidNullResolverResponse(
        Response<ErrorDescriptor> response)
    {
        Assert.NotNull(response);
        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);
        Assert.Equal(
            "Error definition resolver returned a null response.",
            response.Message);

        Assert.Collection(
            response.Issues,
            issue => Assert.Equal(
                "ErrorDefinitionResolverReturnedNull",
                issue.Code));
    }

    private sealed class NullResponseDefinitionResolver : IErrorDefinitionResolver
    {
        public Response<ErrorDefinition> FindById(
            ErrorCatalogContext? context,
            string errorId)
        {
            return null!;
        }

        public Response<ErrorDefinition> FindByName(
            ErrorCatalogContext? context,
            string errorName)
        {
            return null!;
        }

        public Response<ErrorDefinition> FindByCode(
            ErrorCatalogContext? context,
            int code)
        {
            return null!;
        }
    }

    private sealed class ThrowingErrorDescriptorFactory : IErrorDescriptorFactory
    {
        public ErrorDescriptor Create(ErrorDefinition definition)
        {
            throw new InvalidOperationException(
                "Descriptor factory must not run when the definition resolver returns null.");
        }
    }
}
