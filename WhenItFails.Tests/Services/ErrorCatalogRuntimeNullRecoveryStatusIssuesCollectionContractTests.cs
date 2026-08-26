using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Descriptors;
using Afrowave.Toolbox.WhenItFails.Enums;
using Afrowave.Toolbox.WhenItFails.Initialization;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Services;

namespace Afrowave.Toolbox.WhenItFails.Tests.Services;

public sealed class ErrorCatalogRuntimeNullRecoveryStatusIssuesCollectionContractTests
{
    [Fact]
    public async Task InitializeAsync_ShouldRetainPreviousContext_WhenRecoveryReasonIssuesCollectionIsNull()
    {
        ErrorCatalogContext previousContext = new();

        Response<ErrorCatalogInitializationPayload> failedInitialization =
            Response<ErrorCatalogInitializationPayload>.Invalid(
                code: "CatalogDocumentsInvalid",
                message: "Catalog documents are invalid.");

        failedInitialization.Issues = null!;

        ErrorCatalogRuntime runtime = new(
            new StubInitializer(failedInitialization),
            new WhenItFailsOptions
            {
                InitializationMode = ErrorCatalogInitializationMode.Flexible,
                HideRecoverableFailures = true
            },
            new StubContextStore(previousContext),
            new StubBuiltInContextProvider(),
            new StubDescriptorService(),
            new StubProfileSelectionService());

        Response<ErrorCatalogInitializationPayload> response =
            await runtime.InitializeAsync(new JsonsOptions());

        Assert.True(response.IsSuccess);
        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.NotNull(response.Data);
        Assert.Same(previousContext, response.Data.Context);
        Assert.True(response.Data.KeptPreviousContext);
        Assert.False(response.Data.UsedFallback);
    }

    private sealed class StubInitializer(
        Response<ErrorCatalogInitializationPayload> response)
        : IErrorCatalogInitializer
    {
        public Task<Response<ErrorCatalogInitializationPayload>> InitializeAsync(
            JsonsOptions options,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(response);
        }
    }

    private sealed class StubContextStore(ErrorCatalogContext context)
        : IErrorCatalogContextStore
    {
        public bool IsInitialized => true;

        public ErrorCatalogContext? Current => context;

        public Response<ErrorCatalogContext> GetCurrent()
        {
            return Response<ErrorCatalogContext>.Ok(context);
        }

        public void Set(ErrorCatalogContext context)
        {
        }
    }

    private sealed class StubBuiltInContextProvider
        : IBuiltInErrorCatalogContextProvider
    {
        public Task<Response<ErrorCatalogContext>> LoadAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                Response<ErrorCatalogContext>.Ok(new ErrorCatalogContext()));
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

    private sealed class StubProfileSelectionService
        : IErrorProfileSelectionService
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
