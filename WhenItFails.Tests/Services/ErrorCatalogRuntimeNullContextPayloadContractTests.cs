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

public sealed class ErrorCatalogRuntimeNullContextPayloadContractTests
{
    [Fact]
    public void FromId_ShouldReturnInvalidResponse_WhenStoreReturnsSuccessWithNullContext()
    {
        Response<ErrorDescriptor> response = CreateRuntime().FromId("AFW_CFG_0001");

        AssertInvalidNullContextResponse(response);
    }

    [Fact]
    public void FromName_ShouldReturnInvalidResponse_WhenStoreReturnsSuccessWithNullContext()
    {
        Response<ErrorDescriptor> response = CreateRuntime().FromName("MISSING_CONFIGURATION_VALUE");

        AssertInvalidNullContextResponse(response);
    }

    [Fact]
    public void FromCode_ShouldReturnInvalidResponse_WhenStoreReturnsSuccessWithNullContext()
    {
        Response<ErrorDescriptor> response = CreateRuntime().FromCode(200001);

        AssertInvalidNullContextResponse(response);
    }

    [Fact]
    public void ResolveProfile_ShouldReturnInvalidResponse_WhenStoreReturnsSuccessWithNullContext()
    {
        Response<IReadOnlyList<ErrorDefinition>> response =
            CreateRuntime().ResolveProfile("DEFAULT");

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);

        var issue = Assert.Single(response.Issues);
        Assert.Equal("WIF_CURRENT_CONTEXT_PAYLOAD_NULL", issue.Code);
        Assert.Equal("The current error catalog context payload is null.", response.Message);
    }

    private static ErrorCatalogRuntime CreateRuntime()
    {
        return new ErrorCatalogRuntime(
            new StubInitializer(),
            new WhenItFailsOptions(),
            new NullPayloadContextStore(),
            new StubBuiltInContextProvider(),
            new StubDescriptorService(),
            new StubProfileSelectionService());
    }

    private static void AssertInvalidNullContextResponse(
        Response<ErrorDescriptor> response)
    {
        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);

        var issue = Assert.Single(response.Issues);
        Assert.Equal("WIF_CURRENT_CONTEXT_PAYLOAD_NULL", issue.Code);
        Assert.Equal("The current error catalog context payload is null.", response.Message);
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

    private sealed class NullPayloadContextStore : IErrorCatalogContextStore
    {
        public bool IsInitialized => true;

        public ErrorCatalogContext? Current => null;

        public Response<ErrorCatalogContext> GetCurrent()
        {
            return Response<ErrorCatalogContext>.Ok(null);
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