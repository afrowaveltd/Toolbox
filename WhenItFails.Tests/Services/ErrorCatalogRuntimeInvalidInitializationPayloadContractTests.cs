using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Descriptors;
using Afrowave.Toolbox.WhenItFails.Initialization;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Services;

namespace Afrowave.Toolbox.WhenItFails.Tests.Services;

public sealed class ErrorCatalogRuntimeInvalidInitializationPayloadContractTests
{
    [Fact]
    public async Task InitializeAsync_ShouldReturnInvalidResponse_WhenSuccessfulPayloadHasNullBootstrap()
    {
        ErrorCatalogInitializationPayload payload = new()
        {
            Bootstrap = null!,
            Context = new ErrorCatalogContext()
        };

        Response<ErrorCatalogInitializationPayload> response =
            await CreateRuntime(payload).InitializeAsync();

        AssertInvalidResponse(
            response,
            "WIF_INITIALIZATION_BOOTSTRAP_NULL",
            "The successful error catalog initialization payload has a null bootstrap value.");
    }

    [Fact]
    public async Task InitializeAsync_ShouldReturnInvalidResponse_WhenSuccessfulPayloadHasNullContext()
    {
        ErrorCatalogInitializationPayload payload = new()
        {
            Bootstrap = new JsonsBootstrapPayload(),
            Context = null!
        };

        Response<ErrorCatalogInitializationPayload> response =
            await CreateRuntime(payload).InitializeAsync();

        AssertInvalidResponse(
            response,
            "WIF_INITIALIZATION_CONTEXT_NULL",
            "The successful error catalog initialization payload has a null context value.");
    }

    private static ErrorCatalogRuntime CreateRuntime(
        ErrorCatalogInitializationPayload payload)
    {
        return new ErrorCatalogRuntime(
            new PayloadInitializer(payload),
            new WhenItFailsOptions(),
            new StubContextStore(),
            new StubBuiltInContextProvider(),
            new StubDescriptorService(),
            new StubProfileSelectionService());
    }

    private static void AssertInvalidResponse(
        Response<ErrorCatalogInitializationPayload> response,
        string code,
        string message)
    {
        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);

        var issue = Assert.Single(response.Issues);
        Assert.Equal(code, issue.Code);
        Assert.Equal(message, response.Message);
    }

    private sealed class PayloadInitializer(
        ErrorCatalogInitializationPayload payload)
        : IErrorCatalogInitializer
    {
        public Task<Response<ErrorCatalogInitializationPayload>> InitializeAsync(
            JsonsOptions options,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                Response<ErrorCatalogInitializationPayload>.Ok(payload));
        }
    }

    private sealed class StubContextStore : IErrorCatalogContextStore
    {
        public bool IsInitialized => false;

        public ErrorCatalogContext? Current => null;

        public Response<ErrorCatalogContext> GetCurrent()
        {
            return Response<ErrorCatalogContext>.Invalid(
                code: "ErrorCatalogContextNotInitialized",
                message: "The error catalog context has not been initialized.");
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
