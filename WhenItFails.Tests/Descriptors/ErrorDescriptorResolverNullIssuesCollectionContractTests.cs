using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Descriptors;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Descriptors;

public sealed class ErrorDescriptorResolverNullIssuesCollectionContractTests
{
    [Fact]
    public void CreateById_ShouldUseFallbackCode_WhenFailureIssuesCollectionIsNull()
    {
        Response<ErrorDefinition> sourceResponse = new()
        {
            Status = ResultStatus.Failed,
            Data = null,
            Message = string.Empty,
            Issues = null!
        };

        ErrorDescriptorResolver resolver = new(
            new StubErrorDefinitionResolver(sourceResponse),
            new ThrowingErrorDescriptorFactory());

        Response<ErrorDescriptor> response = resolver.CreateById(
            new ErrorCatalogContext(),
            "AFW-CFG-0001");

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Failed, response.Status);
        Assert.Null(response.Data);
        Assert.Equal("Error definition resolving failed.", response.Message);

        var issue = Assert.Single(response.Issues);
        Assert.Equal("ErrorDefinitionResolveFailed", issue.Code);
        Assert.Equal("Error definition resolving failed.", issue.Message);
    }

    private sealed class StubErrorDefinitionResolver(
        Response<ErrorDefinition> response) : IErrorDefinitionResolver
    {
        public Response<ErrorDefinition> FindById(
            ErrorCatalogContext? context,
            string errorId) => response;

        public Response<ErrorDefinition> FindByName(
            ErrorCatalogContext? context,
            string errorName) => response;

        public Response<ErrorDefinition> FindByCode(
            ErrorCatalogContext? context,
            int code) => response;
    }

    private sealed class ThrowingErrorDescriptorFactory : IErrorDescriptorFactory
    {
        public ErrorDescriptor Create(ErrorDefinition definition)
        {
            throw new InvalidOperationException(
                "Descriptor factory must not run for a failed definition response.");
        }
    }
}
