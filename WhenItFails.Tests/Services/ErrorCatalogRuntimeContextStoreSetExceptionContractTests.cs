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

public sealed class ErrorCatalogRuntimeContextStoreSetExceptionContractTests
{
    [Fact]
    public async Task ResetToDefaultsAsync_WhenContextStoreSetThrows_ReturnsStableFailure()
    {
        ErrorCatalogRuntime runtime = CreateRuntime(
            new ThrowingSetContextStore());

        Response<ErrorCatalogInitializationPayload> response =
            await runtime.ResetToDefaultsAsync();

        Assert.NotNull(response);
        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Failed, response.Status);
        Assert.Null(response.Data);
        Assert.Equal(
            "The error catalog context store failed.",
            response.Message);

        Assert.Collection(
            response.Issues,
            issue =>
            {
                Assert.Equal("WIF_CONTEXT_STORE_FAILED", issue.Code);
                Assert.Equal(
                    "The error catalog context store failed.",
                    issue.Message);
            });
    }

    [Fact]
    public async Task ResetToDefaultsAsync_WhenContextStoreSetCancels_RethrowsSameOperationCanceledException()
    {
        OperationCanceledException cancellation = new(
            "Runtime context store Set cancellation must propagate.");

        ErrorCatalogRuntime runtime = CreateRuntime(
            new CancelingSetContextStore(cancellation));

        OperationCanceledException thrown =
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => runtime.ResetToDefaultsAsync());

        Assert.Same(cancellation, thrown);
    }

    [Fact]
    public async Task InitializeAsync_WhenFlexibleFallbackContextStoreSetThrows_ReturnsStableFallbackFailure()
    {
        ErrorCatalogRuntime runtime = CreateFlexibleRuntime(
            new EmptyThrowingSetContextStore());

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
            "WIF_CONTEXT_STORE_FAILED",
            response.Metadata["WhenItFails.FallbackFailure.Code"]);

        Assert.Equal(
            ResultStatus.Failed.ToString(),
            response.Metadata["WhenItFails.FallbackFailure.Status"]);

        Assert.Equal(
            "The error catalog context store failed.",
            response.Metadata["WhenItFails.FallbackFailure.Message"]);
    }

    private static ErrorCatalogRuntime CreateRuntime(
        IErrorCatalogContextStore contextStore)
    {
        return new ErrorCatalogRuntime(
            new UnusedInitializer(),
            new WhenItFailsOptions(),
            contextStore,
            new SuccessfulBuiltInContextProvider(),
            new UnusedDescriptorService(),
            new UnusedProfileSelectionService());
    }

    private static ErrorCatalogRuntime CreateFlexibleRuntime(
        IErrorCatalogContextStore contextStore)
    {
        return new ErrorCatalogRuntime(
            new FailingInitializer(),
            new WhenItFailsOptions
            {
                InitializationMode = ErrorCatalogInitializationMode.Flexible
            },
            contextStore,
            new SuccessfulBuiltInContextProvider(),
            new UnusedDescriptorService(),
            new UnusedProfileSelectionService());
    }

    private sealed class SuccessfulBuiltInContextProvider
        : IBuiltInErrorCatalogContextProvider
    {
        public Task<Response<ErrorCatalogContext>> LoadAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                Response<ErrorCatalogContext>.Ok(
                    new ErrorCatalogContext()));
        }
    }

    private sealed class ThrowingSetContextStore : IErrorCatalogContextStore
    {
        public bool IsInitialized =>
            throw new InvalidOperationException("Unexpected IsInitialized access.");

        public ErrorCatalogContext? Current =>
            throw new InvalidOperationException("Unexpected Current access.");

        public Response<ErrorCatalogContext> GetCurrent()
        {
            throw new InvalidOperationException("Unexpected GetCurrent call.");
        }

        public void Set(ErrorCatalogContext context)
        {
            throw new InvalidOperationException(
                "Sensitive runtime context store Set detail must not escape.");
        }
    }

    private sealed class EmptyThrowingSetContextStore : IErrorCatalogContextStore
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
            throw new InvalidOperationException(
                "Sensitive flexible fallback context store Set detail must not escape.");
        }
    }

    private sealed class CancelingSetContextStore : IErrorCatalogContextStore
    {
        private readonly OperationCanceledException _cancellation;

        public CancelingSetContextStore(
            OperationCanceledException cancellation)
        {
            _cancellation = cancellation;
        }

        public bool IsInitialized =>
            throw new InvalidOperationException("Unexpected IsInitialized access.");

        public ErrorCatalogContext? Current =>
            throw new InvalidOperationException("Unexpected Current access.");

        public Response<ErrorCatalogContext> GetCurrent()
        {
            throw new InvalidOperationException("Unexpected GetCurrent call.");
        }

        public void Set(ErrorCatalogContext context)
        {
            throw _cancellation;
        }
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

    private sealed class UnusedInitializer : IErrorCatalogInitializer
    {
        public Task<Response<ErrorCatalogInitializationPayload>> InitializeAsync(
            JsonsOptions options,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Unexpected initializer call.");
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
