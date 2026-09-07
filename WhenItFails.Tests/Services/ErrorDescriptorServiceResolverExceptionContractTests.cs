using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Descriptors;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Services;

namespace Afrowave.Toolbox.WhenItFails.Tests.Services;

public sealed class ErrorDescriptorServiceResolverExceptionContractTests
{
    [Fact]
    public void FromId_WhenResolverThrows_ReturnsStableFailure()
    {
        ErrorDescriptorService service = new(new ThrowingResolver());

        Response<ErrorDescriptor> response =
            service.FromId(new ErrorCatalogContext(), "AFW-CFG-0001");

        Assert.NotNull(response);
        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Failed, response.Status);
        Assert.Null(response.Data);
        Assert.Equal(
            "Error descriptor resolver failed.",
            response.Message);

        Assert.Collection(
            response.Issues,
            issue =>
            {
                Assert.Equal("ErrorDescriptorResolverFailed", issue.Code);
                Assert.Equal("Error descriptor resolver failed.", issue.Message);
            });
    }

    private sealed class ThrowingResolver : IErrorDescriptorResolver
    {
        public Response<ErrorDescriptor> CreateById(
            ErrorCatalogContext? context,
            string errorId)
        {
            throw new InvalidOperationException(
                "Sensitive descriptor resolver detail must not escape.");
        }

        public Response<ErrorDescriptor> CreateByName(
            ErrorCatalogContext? context,
            string errorName)
        {
            throw new InvalidOperationException("Unexpected CreateByName call.");
        }

        public Response<ErrorDescriptor> CreateByCode(
            ErrorCatalogContext? context,
            int code)
        {
            throw new InvalidOperationException("Unexpected CreateByCode call.");
        }
    }
}
