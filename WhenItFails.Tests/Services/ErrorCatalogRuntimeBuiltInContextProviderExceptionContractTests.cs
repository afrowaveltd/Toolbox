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

public sealed class ErrorCatalogRuntimeBuiltInContextProviderExceptionContractTests
{
    [Fact]
    public async Task ResetToDefaultsAsync_WhenBuiltInProviderThrows_ReturnsStableFailure()
    {
        ErrorCatalogRuntime runtime = CreateRuntime(
            new ThrowingBuiltInContextProvider());

        Response<ErrorCatalogInitializationPayload> response =
            await runtime.ResetToDefaultsAsync();

        Assert.NotNull(response);
        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Failed, response.Status);
        Assert.Null(response.Data);
        Assert.Equal(
            "The bundled default catalog provider failed.",
            response.Message);

        Assert.Collection(
            response.Issues,
            issue =>
            {
                Assert.Equal(
                    "WIF_BUILT_IN_CONTEXT_PROVIDER_FAILED",
                    issue.Code);
                Assert.Equal(
                    "The bundled default catalog provider failed.",
                    issue.Message);
            });
    }

    [Fact]
    public async Task ResetToDefaultsAsync_WhenBuiltInProviderCancels_RethrowsSameOperationCanceledException()
    {
        OperationCanceledException cancellation = new(
            "Runtime built-in provider cancellation must propagate.");

        ErrorCatalogRuntime runtime = CreateRuntime(
            new CancelingBuiltInContextProvider(cancellation));

        OperationCanceledException thrown =
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => runtime.ResetToDefaultsAsync());

        Assert.Same(cancellation, thrown);
    }

    [Fact]
    public async Task ResetToDefaultsAsync_WhenBuiltInProviderReturnsNullTask_ReturnsStableFailure()
    {
        ErrorCatalogRuntime runtime = CreateRuntime(
            new NullTaskBuiltInContextProvider());

        Response<ErrorCatalogInitializationPayload> response =
            await runtime.ResetToDefaultsAsync();

        Assert.NotNull(response);
        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Failed, response.Status);
        Assert.Null(response.Data);
        Assert.Equal(
            "The bundled default catalog provider failed.",
            response.Message);

        Assert.Collection(
            response.Issues,
            issue =>
            {
                Assert.Equal(
                    "WIF_BUILT_IN_CONTEXT_PROVIDER_FAILED",
                    issue.Code);
                Assert.Equal(
                    "The bundled default catalog provider failed.",
                    issue.Message);
            });
    }

    private static ErrorCatalogRuntime CreateRuntime(
        IBuiltInErrorCatalogContextProvider builtInContextProvider)
    {
        return new ErrorCatalogRuntime(
            new UnusedInitializer(),
            new WhenItFailsOptions(),
            new UnusedContextStore(),
            builtInContextProvider,
            new UnusedDescriptorService(),
            new UnusedProfileSelectionService());
    }

    private sealed class ThrowingBuiltInContextProvider
        : IBuiltInErrorCatalogContextProvider
    {
        public Task<Response<ErrorCatalogContext>> LoadAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromException<Response<ErrorCatalogContext>>(
                new InvalidOperationException(
                    "Sensitive runtime built-in provider detail must not escape."));
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

    private sealed class UnusedInitializer : IErrorCatalogInitializer
    {
        public Task<Response<ErrorCatalogInitializationPayload>> InitializeAsync(
            JsonsOptions options,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Unexpected initializer call.");
        }
    }

    private sealed class UnusedContextStore : IErrorCatalogContextStore
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
            throw new InvalidOperationException("Unexpected Set call.");
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
