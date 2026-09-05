using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Issues;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Descriptors;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Descriptors;

public sealed class ErrorDescriptorResolverFallbackContractTests
{
    [Fact]
    public void CreateById_ShouldUseFallbackCodeAndMessage_WhenFailureHasNeither()
    {
        Response<ErrorDefinition> sourceResponse = new()
        {
            Status = ResultStatus.Failed,
            Message = string.Empty,
            Issues = []
        };

        ErrorDescriptorResolver resolver = CreateResolver(sourceResponse);

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

    [Fact]
    public void CreateById_ShouldPreserveFirstIssueCodeAndUseFallbackMessage_WhenMessageIsEmpty()
    {
        Response<ErrorDefinition> failureWithIssue =
            Response<ErrorDefinition>.Fail(
                code: "CustomResolverFailure",
                message: "Original resolver failure.");

        Response<ErrorDefinition> sourceResponse = new()
        {
            Status = ResultStatus.Invalid,
            Message = "   ",
            Issues = failureWithIssue.Issues
        };

        ErrorDescriptorResolver resolver = CreateResolver(sourceResponse);

        Response<ErrorDescriptor> response = resolver.CreateById(
            new ErrorCatalogContext(),
            "AFW-CFG-0001");

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);
        Assert.Equal("Error definition resolving failed.", response.Message);

        var issue = Assert.Single(response.Issues);
        Assert.Equal("CustomResolverFailure", issue.Code);
        Assert.Equal("Error definition resolving failed.", issue.Message);
    }

    [Fact]
    public void CreateById_ShouldUseFallbackCode_WhenFirstIssueCodeIsWhitespace()
    {
        Response<ErrorDefinition> sourceResponse = new()
        {
            Status = ResultStatus.Failed,
            Message = "Resolver failed.",
            Issues = new List<IssueInfo>
            {
                new()
                {
                    Code = "   ",
                    Message = "Malformed resolver issue."
                }
            }
        };

        ErrorDescriptorResolver resolver = CreateResolver(sourceResponse);

        Response<ErrorDescriptor> response = resolver.CreateById(
            new ErrorCatalogContext(),
            "AFW-CFG-0001");

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Failed, response.Status);
        Assert.Null(response.Data);
        Assert.Equal("Resolver failed.", response.Message);

        var issue = Assert.Single(response.Issues);
        Assert.Equal("ErrorDefinitionResolveFailed", issue.Code);
        Assert.Equal("Resolver failed.", issue.Message);
    }

    [Fact]
    public void CreateById_ShouldUseFirstLaterValidIssueCode_WhenEarlierIssueCodeIsWhitespace()
    {
        Response<ErrorDefinition> sourceResponse = new()
        {
            Status = ResultStatus.Failed,
            Message = "Resolver failed.",
            Issues = new List<IssueInfo>
            {
                new()
                {
                    Code = "   ",
                    Message = "Malformed resolver issue."
                },
                new()
                {
                    Code = "LaterResolverFailure",
                    Message = "Usable resolver issue."
                }
            }
        };

        ErrorDescriptorResolver resolver = CreateResolver(sourceResponse);

        Response<ErrorDescriptor> response = resolver.CreateById(
            new ErrorCatalogContext(),
            "AFW-CFG-0001");

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Failed, response.Status);
        Assert.Null(response.Data);
        Assert.Equal("Resolver failed.", response.Message);

        var issue = Assert.Single(response.Issues);
        Assert.Equal("LaterResolverFailure", issue.Code);
        Assert.Equal("Resolver failed.", issue.Message);
    }

    private static ErrorDescriptorResolver CreateResolver(
        Response<ErrorDefinition> response)
    {
        return new ErrorDescriptorResolver(
            new StubErrorDefinitionResolver(response),
            new ThrowingErrorDescriptorFactory());
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
