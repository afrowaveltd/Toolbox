using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Descriptors;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Descriptors;

public sealed class ErrorDescriptorResolverFactoryExceptionContractTests
{
    [Fact]
    public void CreateById_ShouldReturnStableFailure_WhenDescriptorFactoryThrows()
    {
        ErrorDescriptorResolver resolver = new(
            new SuccessfulDefinitionResolver(),
            new ThrowingDescriptorFactory());

        Response<ErrorDescriptor> response = resolver.CreateById(
            new ErrorCatalogContext(),
            "AFW-CFG-0001");

        Assert.NotNull(response);
        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Failed, response.Status);
        Assert.Null(response.Data);
        Assert.Equal(
            "Error descriptor factory failed.",
            response.Message);

        Assert.Collection(
            response.Issues,
            issue =>
            {
                Assert.Equal("ErrorDescriptorFactoryFailed", issue.Code);
                Assert.Equal("Error descriptor factory failed.", issue.Message);
            });
    }

    private sealed class SuccessfulDefinitionResolver : IErrorDefinitionResolver
    {
        private static readonly Response<ErrorDefinition> SuccessfulResponse =
            Response<ErrorDefinition>.Ok(
                new ErrorDefinition
                {
                    Id = "AFW_CFG_0001",
                    Code = 200001,
                    Name = "MISSING_CONFIGURATION_VALUE"
                });

        public Response<ErrorDefinition> FindById(
            ErrorCatalogContext? context,
            string errorId) => SuccessfulResponse;

        public Response<ErrorDefinition> FindByName(
            ErrorCatalogContext? context,
            string errorName) => SuccessfulResponse;

        public Response<ErrorDefinition> FindByCode(
            ErrorCatalogContext? context,
            int code) => SuccessfulResponse;
    }

    private sealed class ThrowingDescriptorFactory : IErrorDescriptorFactory
    {
        public ErrorDescriptor Create(ErrorDefinition definition)
        {
            throw new InvalidOperationException(
                "Sensitive descriptor factory detail must not escape.");
        }
    }
}
