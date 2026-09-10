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

public sealed class ErrorCatalogRuntimeBuiltInContextProviderFlexibleFallbackExceptionContractTests
{
    [Fact]
    public async Task InitializeAsync_WhenFlexibleFallbackProviderThrows_ReturnsStableFallbackFailure()
    {
        ErrorCatalogRuntime runtime = CreateRuntime(
            new ThrowingBuiltInContextProvider());

        Response<ErrorCatalogInitializationPayload> response =
            await runtime.InitializeAsync(new JsonsOptions());

        Assert.NotNull(response);
        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Failed, response.Status);
        Assert.Null(response.Data);
        Assert.Equal(
            "The configured error catalog failed and the bundled default catalog could not be activated.",
            response.Message);

        Assert.Collection(
            response.Issues,
            issue =>
            {
                Assert.Equal("WIF_DEFAULT_FALLBACK_FAILED", issue.Code);
                Assert.Equal(
                    "The configured error catalog failed and the bundled default catalog could not be activated.",
                    issue.Message);
            });

        Assert.Equal(
            "CatalogDocumentsInvalid",
            response.Metadata["WhenItFails.ProjectFailure.Code"]);

        Assert.Equal(
            "WIF_BUILT_IN_CONTEXT_PROVIDER_FAILED",
            response.Metadata["WhenItFails.FallbackFailure.Code"]);

        Assert.Equal(
            ResultStatus.Failed.ToString(),
            response.Metadata["WhenItFails.FallbackFailure.Status"]);

        Assert.Equal(
            "The bundled default catalog provider failed.",
            response.Metadata["WhenItFails.FallbackFailure.Message"]);
    }

    [Fact]
    public async Task InitializeAsync_WhenFlexibleFallbackProviderCancels_RethrowsSameOperationCanceledException()
    {
        OperationCanceledException cancellation = new(
            "Flexible fallback provider cancellation must propagate unchanged.");

        ErrorCatalogRuntime runtime = CreateRuntime(
            new CancelingBuiltInContextProvider(cancellation));

        OperationCanceledException thrown =
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => runtime.InitializeAsync(new JsonsOptions()));

        Assert.Same(cancellation, thrown);
    }

    [Fact]
    public async Task InitializeAsync_WhenFlexibleFallbackProviderReturnsNullTask_ReturnsStableFallbackFailure()
    {
        ErrorCatalogRuntime runtime = CreateRuntime(
            new NullTaskBuiltInContextProvider());

        Response<ErrorCatalogInitializationPayload> response =
            await runtime.InitializeAsync(new JsonsOptions());

        Assert.NotNull(response);
        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Failed, response.Status);
        Assert.Null(response.Data);
        Assert.Equal(
            "The configured error catalog failed and the bundled default catalog could not be activated.",
            response.Message);

        Assert.Collection(
            response.Issues,
            issue =>
            {
                Assert.Equal("WIF_DEFAULT_FALLBACK_FAILED", issue.Code);
                Assert.Equal(
                    "The configured error catalog failed and the bundled default catalog could not be activated.",
                    issue.Message);
            });

        Assert.Equal(
            "CatalogDocumentsInvalid",
            response.Metadata["WhenItFails.ProjectFailure.Code"]);

        Assert.Equal(
            "WIF_BUILT_IN_CONTEXT_PROVIDER_FAILED",
            response.Metadata["WhenItFails.FallbackFailure.Code"]);

        Assert.Equal(
            ResultStatus.Failed.ToString(),
            response.Metadata["WhenItFails.FallbackFailure.Status"]);

        Assert.Equal(
            "The bundled default catalog provider failed.",
            response.Metadata["WhenItFails.FallbackFailure.Message"]);
    }

    private static ErrorCatalogRuntime CreateRuntime(
        IBuiltInErrorCatalogContextProvider builtInContextProvider)
    {
        return new ErrorCatalogRuntime(
            new FailingInitializer(),
            new WhenItFailsOptions
            {
                InitializationMode = ErrorCatalogInitializationMode.Flexible
            },
            new EmptyContextStore(),
            builtInContextProvider,
            new UnusedDescriptorService(),
            new UnusedProfileSelectionService());
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
            throw new InvalidOperationException("Unexpected Set call.");
        }
    }

    private sealed class ThrowingBuiltInContextProvider
        : IBuiltInErrorCatalogContextProvider
    {
        public Task<Response<ErrorCatalogContext>> LoadAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromException<Response<ErrorCatalogContext>>(
                new InvalidOperationException(
                    "Sensitive flexible fallback provider detail must not escape."));
        }
    }

    private sealed class CancelingBuiltInContextProvider
        : IBuiltInErrorCatalogContextProvider
    {
        private readonly OperationCanceledException _cancellation;

        public CancelingBuiltInContextProvider(
            OperationCanceledException cancellation)
        {
            _cancellation = cancellation;
        }

        public Task<Response<ErrorCatalogContext>> LoadAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromException<Response<ErrorCatalogContext>>(
                _cancellation);
        }
    }

    private sealed class NullTaskBuiltInContextProvider
        : IBuiltInErrorCatalogContextProvider
    {
        public Task<Response<ErrorCatalogContext>> LoadAsync(
            CancellationToken cancellationToken = default)
        {
            return null!;
        }
    }

    private sealed class UnusedDescriptorService : IErrorDescriptorService
    {
        public Response<ErrorDescriptor> FromId(
            ErrorCatalogContext? context,
            string errorId)
        {
            throw new InvalidOperationException("Unexpected FromId call.");
        }

        public Response<ErrorDescriptor> FromName(
            ErrorCatalogContext? context,
            string errorName)
        {
            throw new InvalidOperationException("Unexpected FromName call.");
        }

        public Response<ErrorDescriptor> FromCode(
            ErrorCatalogContext? context,
            int code)
        {
            throw new InvalidOperationException("Unexpected FromCode call.");
        }
    }

    private sealed class UnusedProfileSelectionService
        : IErrorProfileSelectionService
    {
        public Response<IReadOnlyList<ErrorDefinition>> ResolveByProfileName(
            ErrorCatalogContext? context,
            string profileName)
        {
            throw new InvalidOperationException(
                "Unexpected profile selection service call.");
        }
    }
}
