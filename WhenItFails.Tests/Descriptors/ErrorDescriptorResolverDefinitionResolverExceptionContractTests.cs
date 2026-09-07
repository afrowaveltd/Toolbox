using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Descriptors;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Descriptors;

public sealed class ErrorDescriptorResolverDefinitionResolverExceptionContractTests
{
    [Fact]
    public void CreateById_ShouldReturnStableFailure_WhenDefinitionResolverThrows()
    {
        ErrorDescriptorResolver resolver = new(
            new ThrowingDefinitionResolver(),
            new ThrowingDescriptorFactory());

        Response<ErrorDescriptor> response = resolver.CreateById(
            new ErrorCatalogContext(),
            "AFW-CFG-0001");

        Assert.NotNull(response);
        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Failed, response.Status);
        Assert.Null(response.Data);
        Assert.Equal(
            "Error definition resolver failed.",
            response.Message);

        Assert.Collection(
            response.Issues,
            issue =>
            {
                Assert.Equal("ErrorDefinitionResolverFailed", issue.Code);
                Assert.Equal("Error definition resolver failed.", issue.Message);
            });
    }

    private sealed class ThrowingDefinitionResolver : IErrorDefinitionResolver
    {
        public Response<ErrorDefinition> FindById(
            ErrorCatalogContext? context,
            string errorId)
        {
            throw new InvalidOperationException(
                "Sensitive definition resolver detail must not escape.");
        }

        public Response<ErrorDefinition> FindByName(
            ErrorCatalogContext? context,
            string errorName)
        {
            throw new InvalidOperationException("Unexpected FindByName call.");
        }

        public Response<ErrorDefinition> FindByCode(
            ErrorCatalogContext? context,
            int code)
        {
            throw new InvalidOperationException("Unexpected FindByCode call.");
        }
    }

    private sealed class ThrowingDescriptorFactory : IErrorDescriptorFactory
    {
        public ErrorDescriptor Create(ErrorDefinition definition)
        {
            throw new InvalidOperationException(
                "Descriptor factory must not run when definition resolution throws.");
        }
    }
}
