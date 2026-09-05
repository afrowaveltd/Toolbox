using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Descriptors;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Descriptors;

public sealed class ErrorDescriptorResolverNullFactoryResultContractTests
{
    [Fact]
    public void CreateById_ShouldReturnStableInvalidResponse_WhenDescriptorFactoryReturnsNull()
    {
        ErrorDescriptorResolver resolver = new(
            new SuccessfulDefinitionResolver(),
            new NullResultDescriptorFactory());

        Response<ErrorDescriptor> response = resolver.CreateById(
            new ErrorCatalogContext(),
            "AFW-CFG-0001");

        Assert.NotNull(response);
        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);
        Assert.Equal(
            "Error descriptor factory returned a null descriptor.",
            response.Message);

        Assert.Collection(
            response.Issues,
            issue => Assert.Equal(
                "ErrorDescriptorFactoryReturnedNull",
                issue.Code));
    }

    private sealed class SuccessfulDefinitionResolver : IErrorDefinitionResolver
    {
        private static readonly Response<ErrorDefinition> Response =
            Essentials.Results.Response<ErrorDefinition>.Ok(
                new ErrorDefinition
                {
                    Id = "AFW_CFG_0001",
                    Code = 200001,
                    Name = "MISSING_CONFIGURATION_VALUE"
                });

        public Response<ErrorDefinition> FindById(
            ErrorCatalogContext? context,
            string errorId) => Response;

        public Response<ErrorDefinition> FindByName(
            ErrorCatalogContext? context,
            string errorName) => Response;

        public Response<ErrorDefinition> FindByCode(
            ErrorCatalogContext? context,
            int code) => Response;
    }

    private sealed class NullResultDescriptorFactory : IErrorDescriptorFactory
    {
        public ErrorDescriptor Create(ErrorDefinition definition)
        {
            return null!;
        }
    }
}
