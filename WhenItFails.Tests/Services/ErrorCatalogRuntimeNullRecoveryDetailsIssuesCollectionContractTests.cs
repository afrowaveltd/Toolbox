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

public sealed class ErrorCatalogRuntimeNullRecoveryDetailsIssuesCollectionContractTests
{
    [Fact]
    public async Task InitializeAsync_ShouldReturnWarningRecovery_WhenRecoveryReasonIssuesCollectionIsNull()
    {
        ErrorCatalogContext previousContext = new();

        ErrorCatalogRuntime runtime = new(
            new NullIssuesFailingInitializer(),
            new WhenItFailsOptions
            {
                InitializationMode =
                    ErrorCatalogInitializationMode.Flexible,

                HideRecoverableFailures = false
            },
            new PreviousContextStore(previousContext),
            new StubBuiltInContextProvider(),
            new StubDescriptorService(),
            new StubProfileSelectionService());

        Response<ErrorCatalogInitializationPayload> response =
            await runtime.InitializeAsync(new JsonsOptions());

        Assert.True(response.IsSuccess);
        Assert.Equal(ResultStatus.SuccessWithWarnings, response.Status);
        Assert.NotNull(response.Data);
        Assert.Same(previousContext, response.Data.Context);
        Assert.True(response.Data.KeptPreviousContext);

        var issue = Assert.Single(response.Issues);
        Assert.Equal("WIF_PREVIOUS_CONTEXT_RETAINED", issue.Code);
        Assert.Equal(
            "No additional initialization diagnostics were provided.",
            issue.Details);
    }

    private sealed class NullIssuesFailingInitializer : IErrorCatalogInitializer
    {
        public Task<Response<ErrorCatalogInitializationPayload>> InitializeAsync(
            JsonsOptions options,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                new Response<ErrorCatalogInitializationPayload>
                {
                    Status = ResultStatus.Invalid,
                    Issues = null!
                });
        }
    }

    private sealed class PreviousContextStore : IErrorCatalogContextStore
    {
        public PreviousContextStore(ErrorCatalogContext context)
        {
            Current = context;
        }

        public bool IsInitialized => true;

        public ErrorCatalogContext? Current { get; private set; }

        public Response<ErrorCatalogContext> GetCurrent()
        {
            return Response<ErrorCatalogContext>.Ok(Current);
        }

        public void Set(ErrorCatalogContext context)
        {
            Current = context;
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
