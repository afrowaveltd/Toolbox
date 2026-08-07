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

public sealed class ErrorCatalogRuntimeNullDownstreamResponseContractTests
{
    [Fact]
    public void FromId_ShouldReturnInvalidResponse_WhenDescriptorServiceReturnsNull()
    {
        Response<ErrorDescriptor> response = CreateRuntime().FromId("AFW_CFG_0001");

        AssertInvalidDescriptorResponse(response);
    }

    [Fact]
    public void FromName_ShouldReturnInvalidResponse_WhenDescriptorServiceReturnsNull()
    {
        Response<ErrorDescriptor> response = CreateRuntime().FromName("MISSING_CONFIGURATION_VALUE");

        AssertInvalidDescriptorResponse(response);
    }

    [Fact]
    public void FromCode_ShouldReturnInvalidResponse_WhenDescriptorServiceReturnsNull()
    {
        Response<ErrorDescriptor> response = CreateRuntime().FromCode(200001);

        AssertInvalidDescriptorResponse(response);
    }

    [Fact]
    public void ResolveProfile_ShouldReturnInvalidResponse_WhenProfileSelectionServiceReturnsNull()
    {
        Response<IReadOnlyList<ErrorDefinition>> response =
            CreateRuntime().ResolveProfile("DEFAULT");

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);

        var issue = Assert.Single(response.Issues);
        Assert.Equal("WIF_PROFILE_SELECTION_RESPONSE_NULL", issue.Code);
        Assert.Equal(
            "The error profile selection service returned a null response.",
            response.Message);
    }

    private static ErrorCatalogRuntime CreateRuntime()
    {
        return new ErrorCatalogRuntime(
            new StubInitializer(),
            new WhenItFailsOptions(),
            new ContextStore(),
            new StubBuiltInContextProvider(),
            new NullDescriptorService(),
            new NullProfileSelectionService());
    }

    private static void AssertInvalidDescriptorResponse(
        Response<ErrorDescriptor> response)
    {
        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);

        var issue = Assert.Single(response.Issues);
        Assert.Equal("WIF_DESCRIPTOR_SERVICE_RESPONSE_NULL", issue.Code);
        Assert.Equal(
            "The error descriptor service returned a null response.",
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

    private sealed class ContextStore : IErrorCatalogContextStore
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

    private sealed class NullDescriptorService : IErrorDescriptorService
    {
        public Response<ErrorDescriptor> FromId(
            ErrorCatalogContext? context,
            string errorId)
        {
            return null!;
        }

        public Response<ErrorDescriptor> FromName(
            ErrorCatalogContext? context,
            string errorName)
        {
            return null!;
        }

        public Response<ErrorDescriptor> FromCode(
            ErrorCatalogContext? context,
            int code)
        {
            return null!;
        }
    }

    private sealed class NullProfileSelectionService : IErrorProfileSelectionService
    {
        public Response<IReadOnlyList<ErrorDefinition>> ResolveByProfileName(
            ErrorCatalogContext? context,
            string profileName)
        {
            return null!;
        }
    }
}