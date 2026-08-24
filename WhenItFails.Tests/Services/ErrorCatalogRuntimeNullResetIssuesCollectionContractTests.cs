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

public sealed class ErrorCatalogRuntimeNullResetIssuesCollectionContractTests
{
    [Fact]
    public async Task ResetToDefaultsAsync_ShouldReturnStableFailure_WhenBuiltInResponseIssuesCollectionIsNull()
    {
        ErrorCatalogRuntime runtime = new(
            new StubInitializer(),
            new WhenItFailsOptions(),
            new EmptyContextStore(),
            new NullIssuesBuiltInContextProvider(),
            new StubDescriptorService(),
            new StubProfileSelectionService());

        Response<ErrorCatalogInitializationPayload> response =
            await runtime.ResetToDefaultsAsync();

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);

        var issue = Assert.Single(response.Issues);
        Assert.Equal("WIF_RESET_TO_DEFAULTS_FAILED", issue.Code);

        Assert.Equal(
            "WIF_BUILT_IN_CONTEXT_PAYLOAD_NULL",
            response.Metadata["WhenItFails.ResetFailure.Code"]);
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

    private sealed class NullIssuesBuiltInContextProvider : IBuiltInErrorCatalogContextProvider
    {
        public Task<Response<ErrorCatalogContext>> LoadAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                new Response<ErrorCatalogContext>
                {
                    Status = ResultStatus.Invalid,
                    Message = "Built-in catalog is invalid.",
                    Issues = null!
                });
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
