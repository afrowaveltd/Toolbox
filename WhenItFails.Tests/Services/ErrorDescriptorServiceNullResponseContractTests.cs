using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Descriptors;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Services;

namespace Afrowave.Toolbox.WhenItFails.Tests.Services;

public sealed class ErrorDescriptorServiceNullResponseContractTests
{
    [Fact]
    public void FromId_WhenResolverReturnsNull_ReturnsStableInvalidResponse()
    {
        ErrorDescriptorService service = new(new NullResponseResolver());

        Response<ErrorDescriptor> response =
            service.FromId(new ErrorCatalogContext(), "AFW-CFG-0001");

        AssertInvalidNullResolverResponse(response);
    }

    [Fact]
    public void FromName_WhenResolverReturnsNull_ReturnsStableInvalidResponse()
    {
        ErrorDescriptorService service = new(new NullResponseResolver());

        Response<ErrorDescriptor> response =
            service.FromName(new ErrorCatalogContext(), "MissingConfigurationValue");

        AssertInvalidNullResolverResponse(response);
    }

    [Fact]
    public void FromCode_WhenResolverReturnsNull_ReturnsStableInvalidResponse()
    {
        ErrorDescriptorService service = new(new NullResponseResolver());

        Response<ErrorDescriptor> response =
            service.FromCode(new ErrorCatalogContext(), 200001);

        AssertInvalidNullResolverResponse(response);
    }

    private static void AssertInvalidNullResolverResponse(
        Response<ErrorDescriptor> response)
    {
        Assert.NotNull(response);
        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);
        Assert.Equal("Error descriptor resolver returned a null response.", response.Message);

        Assert.Collection(
            response.Issues,
            issue => Assert.Equal(
                "ErrorDescriptorResolverReturnedNull",
                issue.Code));
    }

    private sealed class NullResponseResolver : IErrorDescriptorResolver
    {
        public Response<ErrorDescriptor> CreateById(
            ErrorCatalogContext? context,
            string errorId)
        {
            return null!;
        }

        public Response<ErrorDescriptor> CreateByName(
            ErrorCatalogContext? context,
            string errorName)
        {
            return null!;
        }

        public Response<ErrorDescriptor> CreateByCode(
            ErrorCatalogContext? context,
            int code)
        {
            return null!;
        }
    }
}