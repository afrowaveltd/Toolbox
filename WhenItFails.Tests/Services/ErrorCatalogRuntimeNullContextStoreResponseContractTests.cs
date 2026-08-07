using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Descriptors;
using Afrowave.Toolbox.WhenItFails.Initialization;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Services;

namespace Afrowave.Toolbox.WhenItFails.Tests.Services;

public sealed class ErrorCatalogRuntimeNullContextStoreResponseContractTests
{
    [Fact]
    public void GetCurrentContext_ShouldReturnInvalidResponse_WhenStoreReturnsNull()
    {
        Response<ErrorCatalogContext> response =
            CreateRuntime().GetCurrentContext();

        AssertInvalidContextResponse(response);
    }

    [Fact]
    public void FromId_ShouldReturnInvalidResponse_WhenStoreReturnsNull()
    {
        Response<ErrorDescriptor> response =
            CreateRuntime().FromId("AFW_CFG_0001");

        AssertInvalidRuntimeResponse(response);
    }

    [Fact]
    public void FromName_ShouldReturnInvalidResponse_WhenStoreReturnsNull()
    {
        Response<ErrorDescriptor> response =
            CreateRuntime().FromName("MISSING_CONFIGURATION_VALUE");

        AssertInvalidRuntimeResponse(response);
    }

    [Fact]
    public void FromCode_ShouldReturnInvalidResponse_WhenStoreReturnsNull()
    {
        Response<ErrorDescriptor> response =
            CreateRuntime().FromCode(200001);

        AssertInvalidRuntimeResponse(response);
    }

    [Fact]
    public void ResolveProfile_ShouldReturnInvalidResponse_WhenStoreReturnsNull()
    {
        Response<IReadOnlyList<ErrorDefinition>> response =
            CreateRuntime().ResolveProfile("DEFAULT");

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);

        var issue = Assert.Single(response.Issues);
        Assert.Equal("WIF_CONTEXT_STORE_RESPONSE_NULL", issue.Code);
        Assert.Equal(
            "The error catalog context store returned a null response.",
            response.Message);
    }

    private static ErrorCatalogRuntime CreateRuntime()
    {
        return new ErrorCatalogRuntime(
            new StubInitializer(),
            new WhenItFailsOptions(),
            new NullResponseContextStore(),
            new StubBuiltInContextProvider(),
            new StubDescriptorService(),
            new StubProfileSelectionService());
    }

    private static void AssertInvalidContextResponse(
        Response<ErrorCatalogContext> response)
    {
        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);

        var issue = Assert.Single(response.Issues);
        Assert.Equal("WIF_CONTEXT_STORE_RESPONSE_NULL", issue.Code);
        Assert.Equal(
            "The error catalog context store returned a null response.",
            response.Message);
    }

    private static void AssertInvalidRuntimeResponse(
        Response<ErrorDescriptor> response)
    {
        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);

        var issue = Assert.Single(response.Issues);
        Assert.Equal("WIF_CONTEXT_STORE_RESPONSE_NULL", issue.Code);
        Assert.Equal(
            "The error catalog context store returned a null response.",
            response.Message);
    }

    private sealed class StubInitializer : IErrorCatalogInitializer
    {
        public Task<Response<ErrorCatalogInitializationPayload>> InitializeAsync(
            JsonsOptions options,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                Response<ErrorCatalogInitializationPayload>.Ok(
                    new ErrorCatalogInitializationPayload()));
        }
    }

    private sealed class NullResponseContextStore : IErrorCatalogContextStore
    {
        public bool IsInitialized => true;

        public ErrorCatalogContext? Current => new ErrorCatalogContext();

        public Response<ErrorCatalogContext> GetCurrent()
        {
            return null!;
        }

        public void Set(ErrorCatalogContext context)
        {
        }
    }

    private sealed class StubBuiltInContextProvider : IBuiltInErrorCatalogContextProvider
    {
        public Task<Response<ErrorCatalogContext>> LoadAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                Response<ErrorCatalogContext>.Ok(
                    new ErrorCatalogContext()));
        }
    }

    private sealed class StubDescriptorService : IErrorDescriptorService
    {
        public Response<ErrorDescriptor> FromId(
            ErrorCatalogContext? context,
            string errorId)
        {
            return Response<ErrorDescriptor>.Ok(new ErrorDescriptor());
        }

        public Response<ErrorDescriptor> FromName(
            ErrorCatalogContext? context,
            string errorName)
        {
            return Response<ErrorDescriptor>.Ok(new ErrorDescriptor());
        }

        public Response<ErrorDescriptor> FromCode(
            ErrorCatalogContext? context,
            int code)
        {
            return Response<ErrorDescriptor>.Ok(new ErrorDescriptor());
        }
    }

    private sealed class StubProfileSelectionService : IErrorProfileSelectionService
    {
        public Response<IReadOnlyList<ErrorDefinition>> ResolveByProfileName(
            ErrorCatalogContext? context,
            string profileName)
        {
            return Response<IReadOnlyList<ErrorDefinition>>.Ok(
                Array.Empty<ErrorDefinition>());
        }
    }
}
