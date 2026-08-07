using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Descriptors;
using Afrowave.Toolbox.WhenItFails.Enums;
using Afrowave.Toolbox.WhenItFails.Initialization;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Services;

namespace Afrowave.Toolbox.WhenItFails.Tests.Services;

public sealed class ErrorCatalogRuntimeNullFlexibleFallbackResponseContractTests
{
    [Fact]
    public async Task InitializeAsync_ShouldReturnStableFallbackFailure_WhenBuiltInProviderReturnsNull()
    {
        ErrorCatalogRuntime runtime = new(
            new FailingInitializer(),
            new WhenItFailsOptions
            {
                InitializationMode = ErrorCatalogInitializationMode.Flexible
            },
            new EmptyContextStore(),
            new NullBuiltInContextProvider(),
            new StubDescriptorService(),
            new StubProfileSelectionService());

        Response<ErrorCatalogInitializationPayload> response =
            await runtime.InitializeAsync(new JsonsOptions());

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);

        var issue = Assert.Single(response.Issues);
        Assert.Equal("WIF_DEFAULT_FALLBACK_FAILED", issue.Code);

        Assert.Equal(
            "CatalogDocumentsInvalid",
            response.Metadata["WhenItFails.ProjectFailure.Code"]);

        Assert.Equal(
            "WIF_BUILT_IN_CONTEXT_RESPONSE_NULL",
            response.Metadata["WhenItFails.FallbackFailure.Code"]);

        Assert.Equal(
            ResultStatus.Invalid.ToString(),
            response.Metadata["WhenItFails.FallbackFailure.Status"]);

        Assert.Equal(
            "The bundled default catalog provider returned a null response.",
            response.Metadata["WhenItFails.FallbackFailure.Message"]);
    }

    private sealed class FailingInitializer : IErrorCatalogInitializer
    {
        public Task<Response<ErrorCatalogInitializationPayload>> InitializeAsync(
            JsonsOptions options,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                Response<ErrorCatalogInitializationPayload>.Invalid(
                    code: "CatalogDocumentsInvalid",
                    message: "Catalog documents are invalid."));
        }
    }

    private sealed class EmptyContextStore : IErrorCatalogContextStore
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

    private sealed class NullBuiltInContextProvider : IBuiltInErrorCatalogContextProvider
    {
        public Task<Response<ErrorCatalogContext>> LoadAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Response<ErrorCatalogContext>>(null!);
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
