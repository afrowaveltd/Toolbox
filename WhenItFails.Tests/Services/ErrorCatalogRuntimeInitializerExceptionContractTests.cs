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

public sealed class ErrorCatalogRuntimeInitializerExceptionContractTests
{
    [Fact]
    public async Task InitializeAsync_WhenInitializerThrows_ReturnsStableFailure()
    {
        ErrorCatalogRuntime runtime = CreateRuntime(
            new ThrowingInitializer());

        Response<ErrorCatalogInitializationPayload> response =
            await runtime.InitializeAsync();

        Assert.NotNull(response);
        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Failed, response.Status);
        Assert.Null(response.Data);
        Assert.Equal(
            "The error catalog initializer failed.",
            response.Message);

        Assert.Collection(
            response.Issues,
            issue =>
            {
                Assert.Equal("WIF_INITIALIZER_FAILED", issue.Code);
                Assert.Equal(
                    "The error catalog initializer failed.",
                    issue.Message);
            });
    }

    [Fact]
    public async Task InitializeAsync_WhenInitializerCancels_RethrowsSameOperationCanceledException()
    {
        OperationCanceledException cancellation = new(
            "Initializer cancellation must propagate.");

        ErrorCatalogRuntime runtime = CreateRuntime(
            new CancelingInitializer(cancellation));

        OperationCanceledException thrown =
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => runtime.InitializeAsync());

        Assert.Same(cancellation, thrown);
    }

    [Fact]
    public async Task InitializeAsync_WhenInitializerReturnsNullTask_ReturnsStableFailure()
    {
        ErrorCatalogRuntime runtime = CreateRuntime(
            new NullTaskInitializer());

        Response<ErrorCatalogInitializationPayload> response =
            await runtime.InitializeAsync();

        Assert.NotNull(response);
        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Failed, response.Status);
        Assert.Null(response.Data);
        Assert.Equal(
            "The error catalog initializer failed.",
            response.Message);

        Assert.Collection(
            response.Issues,
            issue =>
            {
                Assert.Equal("WIF_INITIALIZER_FAILED", issue.Code);
                Assert.Equal(
                    "The error catalog initializer failed.",
                    issue.Message);
            });
    }

    private static ErrorCatalogRuntime CreateRuntime(
        IErrorCatalogInitializer initializer)
    {
        return new ErrorCatalogRuntime(
            initializer,
            new WhenItFailsOptions(),
            new UnusedContextStore(),
            new UnusedBuiltInContextProvider(),
            new UnusedDescriptorService(),
            new UnusedProfileSelectionService());
    }

    private sealed class ThrowingInitializer : IErrorCatalogInitializer
    {
        public Task<Response<ErrorCatalogInitializationPayload>> InitializeAsync(
            JsonsOptions options,
            CancellationToken cancellationToken = default)
        {
            return Task.FromException<Response<ErrorCatalogInitializationPayload>>(
                new InvalidOperationException(
                    "Sensitive runtime initializer detail must not escape."));
        }
    }

    private sealed class CancelingInitializer : IErrorCatalogInitializer
    {
        private readonly OperationCanceledException _cancellation;

        public CancelingInitializer(
            OperationCanceledException cancellation)
        {
            _cancellation = cancellation;
        }

        public Task<Response<ErrorCatalogInitializationPayload>> InitializeAsync(
            JsonsOptions options,
            CancellationToken cancellationToken = default)
        {
            return Task.FromException<Response<ErrorCatalogInitializationPayload>>(
                _cancellation);
        }
    }

    private sealed class NullTaskInitializer : IErrorCatalogInitializer
    {
        public Task<Response<ErrorCatalogInitializationPayload>> InitializeAsync(
            JsonsOptions options,
            CancellationToken cancellationToken = default)
        {
            return null!;
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

    private sealed class UnusedBuiltInContextProvider
        : IBuiltInErrorCatalogContextProvider
    {
        public Task<Response<ErrorCatalogContext>> LoadAsync(
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "Unexpected built-in context provider call.");
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
