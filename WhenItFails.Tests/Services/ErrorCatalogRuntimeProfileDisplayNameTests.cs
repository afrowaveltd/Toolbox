using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Descriptors;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Resolution;
using Afrowave.Toolbox.WhenItFails.Services;

namespace Afrowave.Toolbox.WhenItFails.Tests.Services;

public sealed class ErrorCatalogRuntimeProfileDisplayNameTests
{
    [Fact]
    public void ResolveProfile_ShouldResolveByDisplayNameThroughRuntimeFacade()
    {
        ErrorDefinition error = new()
        {
            Id = "AFW-WEB-0001",
            Code = 100001,
            Name = "WebRequestFailed",
            Owner = "AFW",
            CodePrefix = "WEB",
            CodeGroup = "WEB",
            PrimaryCategory = "WEB",
            Categories = ["WEB"],
            Tags = ["USER_VISIBLE"],
            Title = "Web request failed",
            Message = "The web request failed.",
            DefaultSeverity = "Error"
        };

        ErrorCatalogContext context = new()
        {
            ErrorCatalog = new ErrorCatalog([error]),
            ErrorCatalogDocument = new ErrorCatalogDocument
            {
                Errors = [error]
            },
            CategoryCatalog = new ErrorCategoryCatalogDocument(),
            CodeGroupCatalog = new ErrorCodeGroupCatalogDocument(),
            OwnerCatalog = new ErrorOwnerCatalogDocument(),
            ProfileCatalog = new ErrorProfileCatalogDocument
            {
                Profiles =
                [
                    new ErrorProfileDefinition
                    {
                        Name = "WEB_API",
                        DisplayName = "Web API",
                        IncludeTags = ["USER_VISIBLE"]
                    }
                ]
            }
        };

        ErrorCatalogRuntime runtime = new(
            new UnusedInitializer(),
            new WhenItFailsOptions(),
            new FixedContextStore(context),
            new UnusedBuiltInContextProvider(),
            new UnusedDescriptorService(),
            new ErrorProfileSelectionService(new ErrorProfileResolver()));

        Response<IReadOnlyList<ErrorDefinition>> response =
            runtime.ResolveProfile("web api");

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        ErrorDefinition resolved = Assert.Single(response.Data);
        Assert.Equal(error.Id, resolved.Id);
    }

    private sealed class FixedContextStore(ErrorCatalogContext context)
        : IErrorCatalogContextStore
    {
        public Response<ErrorCatalogContext> GetCurrent() =>
            Response<ErrorCatalogContext>.Ok(context);

        public void Set(ErrorCatalogContext value)
        {
        }
    }

    private sealed class UnusedInitializer : IErrorCatalogInitializer
    {
        public Task<Response<ErrorCatalogInitializationPayload>> InitializeAsync(
            JsonsOptions options,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }

    private sealed class UnusedBuiltInContextProvider
        : IBuiltInErrorCatalogContextProvider
    {
        public Task<Response<ErrorCatalogContext>> LoadAsync(
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }

    private sealed class UnusedDescriptorService : IErrorDescriptorService
    {
        public Response<ErrorDescriptor> FromId(
            ErrorCatalogContext context,
            string errorId) =>
            throw new NotSupportedException();

        public Response<ErrorDescriptor> FromName(
            ErrorCatalogContext context,
            string errorName) =>
            throw new NotSupportedException();

        public Response<ErrorDescriptor> FromCode(
            ErrorCatalogContext context,
            int code) =>
            throw new NotSupportedException();
    }
}
