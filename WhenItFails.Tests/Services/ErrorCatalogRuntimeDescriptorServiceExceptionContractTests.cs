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

public sealed class ErrorCatalogRuntimeDescriptorServiceExceptionContractTests
{
    [Fact]
    public void FromId_WhenDescriptorServiceThrows_ReturnsStableFailure()
    {
        ErrorCatalogRuntime runtime = CreateRuntime();

        Response<ErrorDescriptor> response =
            runtime.FromId("AFW-CFG-0001");

        AssertStableFailure(response);
    }

    [Fact]
    public void FromName_WhenDescriptorServiceThrows_ReturnsStableFailure()
    {
        ErrorCatalogRuntime runtime = CreateRuntime();

        Response<ErrorDescriptor> response =
            runtime.FromName("MissingConfigurationValue");

        AssertStableFailure(response);
    }

    [Fact]
    public void FromCode_WhenDescriptorServiceThrows_ReturnsStableFailure()
    {
        ErrorCatalogRuntime runtime = CreateRuntime();

        Response<ErrorDescriptor> response =
            runtime.FromCode(200001);

        AssertStableFailure(response);
    }

    [Fact]
    public void FromId_WhenDescriptorServiceCancels_RethrowsSameOperationCanceledException()
    {
        OperationCanceledException cancellation = new(
            "Runtime descriptor service cancellation must propagate.");

        ErrorCatalogRuntime runtime = CreateRuntime(
            new CancelingDescriptorService(cancellation));

        OperationCanceledException thrown =
            Assert.Throws<OperationCanceledException>(
                () => runtime.FromId("AFW-CFG-0001"));

        Assert.Same(cancellation, thrown);
    }

    private static ErrorCatalogRuntime CreateRuntime()
    {
        return CreateRuntime(new ThrowingDescriptorService());
    }

    private static ErrorCatalogRuntime CreateRuntime(
        IErrorDescriptorService descriptorService)
    {
        return new ErrorCatalogRuntime(
            new UnusedInitializer(),
            new WhenItFailsOptions(),
            new SuccessfulContextStore(),
            new UnusedBuiltInContextProvider(),
            descriptorService,
            new UnusedProfileSelectionService());
    }

    private static void AssertStableFailure(Response<ErrorDescriptor> response)
    {
        Assert.NotNull(response);
        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Failed, response.Status);
        Assert.Null(response.Data);
        Assert.Equal(
            "The error descriptor service failed.",
            response.Message);

        Assert.Collection(
            response.Issues,
            issue =>
            {
                Assert.Equal("WIF_DESCRIPTOR_SERVICE_FAILED", issue.Code);
                Assert.Equal(
                    "The error descriptor service failed.",
                    issue.Message);
            });
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

    private sealed class ThrowingDescriptorService : IErrorDescriptorService
    {
        public Response<ErrorDescriptor> FromId(
            ErrorCatalogContext? context,
            string errorId)
        {
            throw new InvalidOperationException(
                "Sensitive runtime descriptor service ID detail must not escape.");
        }

        public Response<ErrorDescriptor> FromName(
            ErrorCatalogContext? context,
            string errorName)
        {
            throw new InvalidOperationException(
                "Sensitive runtime descriptor service name detail must not escape.");
        }

        public Response<ErrorDescriptor> FromCode(
            ErrorCatalogContext? context,
            int code)
        {
            throw new InvalidOperationException(
                "Sensitive runtime descriptor service code detail must not escape.");
        }
    }

    private sealed class CancelingDescriptorService(
        OperationCanceledException cancellation) : IErrorDescriptorService
    {
        public Response<ErrorDescriptor> FromId(
            ErrorCatalogContext? context,
            string errorId)
        {
            throw cancellation;
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
