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

public sealed class ErrorCatalogRuntimeProfileSelectionServiceExceptionContractTests
{
    [Fact]
    public void ResolveProfile_WhenProfileSelectionServiceThrows_ReturnsStableFailure()
    {
        ErrorCatalogRuntime runtime =
            CreateRuntime(new ThrowingProfileSelectionService());

        Response<IReadOnlyList<ErrorDefinition>> response =
            runtime.ResolveProfile("DEFAULT");

        Assert.NotNull(response);
        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Failed, response.Status);
        Assert.Null(response.Data);
        Assert.Equal(
            "The error profile selection service failed.",
            response.Message);

        Assert.Collection(
            response.Issues,
            issue =>
            {
                Assert.Equal("WIF_PROFILE_SELECTION_FAILED", issue.Code);
                Assert.Equal(
                    "The error profile selection service failed.",
                    issue.Message);
            });
    }

    [Fact]
    public void ResolveProfile_WhenProfileSelectionServiceCancels_RethrowsSameOperationCanceledException()
    {
        OperationCanceledException cancellation = new(
            "Profile selection cancellation must propagate unchanged.");

        ErrorCatalogRuntime runtime =
            CreateRuntime(
                new CancellingProfileSelectionService(cancellation));

        OperationCanceledException thrown =
            Assert.Throws<OperationCanceledException>(
                () => runtime.ResolveProfile("DEFAULT"));

        Assert.Same(cancellation, thrown);
    }

    private static ErrorCatalogRuntime CreateRuntime(
        IErrorProfileSelectionService profileSelectionService)
    {
        return new ErrorCatalogRuntime(
            new UnusedInitializer(),
            new WhenItFailsOptions(),
            new SuccessfulContextStore(),
            new UnusedBuiltInContextProvider(),
            new UnusedDescriptorService(),
            profileSelectionService);
    }

    private sealed class SuccessfulContextStore : IErrorCatalogContextStore
    {
        private readonly ErrorCatalogContext _context = new();

        public bool IsInitialized => true;

        public ErrorCatalogContext? Current => _context;

        public Response<ErrorCatalogContext> GetCurrent()
        {
            return Response<ErrorCatalogContext>.Ok(_context);
        }

        public void Set(ErrorCatalogContext context)
        {
            throw new InvalidOperationException("Unexpected Set call.");
        }
    }

    private sealed class ThrowingProfileSelectionService
        : IErrorProfileSelectionService
    {
        public Response<IReadOnlyList<ErrorDefinition>> ResolveByProfileName(
            ErrorCatalogContext? context,
            string profileName)
        {
            throw new InvalidOperationException(
                "Sensitive runtime profile selection detail must not escape.");
        }
    }

    private sealed class CancellingProfileSelectionService
        : IErrorProfileSelectionService
    {
        private readonly OperationCanceledException _cancellation;

        public CancellingProfileSelectionService(
            OperationCanceledException cancellation)
        {
            _cancellation = cancellation;
        }

        public Response<IReadOnlyList<ErrorDefinition>> ResolveByProfileName(
            ErrorCatalogContext? context,
            string profileName)
        {
            throw _cancellation;
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
}
