using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCatalogContextProviderCrossValidationFailureContractTests
{
    [Fact]
    public async Task LoadFromJsonsAsync_ShouldReturnFirstCrossValidationIssueWithoutPartialContext()
    {
        ErrorCatalogContextProvider provider = new(
            new InvalidReferenceErrorCatalogProvider(),
            new EmptyCategoryCatalogProvider(),
            new EmptyCodeGroupCatalogProvider(),
            new EmptyOwnerCatalogProvider(),
            new EmptyProfileCatalogProvider());

        Response<ErrorCatalogContext> response = await provider.LoadFromJsonsAsync(
            new JsonsOptions
            {
                RootDirectory = "Jsons",
                PackageDirectoryName = "WhenItFails"
            });

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);

        var issue = Assert.Single(response.Issues);
        Assert.Equal("UnknownErrorOwner", issue.Code);
        Assert.Equal(
            "Error owner 'MISSING_OWNER' is not defined in the owner catalog.",
            issue.Message);
        Assert.Equal(issue.Message, response.Message);
    }

    private sealed class InvalidReferenceErrorCatalogProvider : IErrorCatalogProvider
    {
        public Task<Response<ErrorCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ErrorCatalogDocument document = new()
            {
                Errors =
                [
                    new ErrorDefinition
                    {
                        Id = "AFW_GEN_0001",
                        Code = 100001,
                        Name = "UNKNOWNERROR",
                        Owner = "MISSING_OWNER",
                        CodePrefix = "GEN",
                        CodeGroup = "MISSING_GROUP",
                        Title = "Unknown error",
                        Message = "An unknown error occurred.",
                        DefaultSeverity = "Error"
                    }
                ]
            };

            return Task.FromResult(Response<ErrorCatalogProviderPayload>.Ok(
                new ErrorCatalogProviderPayload
                {
                    Catalog = new ErrorCatalog(document.Errors),
                    Document = document,
                    ValidationResult = new ErrorCatalogValidationResult()
                }));
        }
    }

    private sealed class EmptyCategoryCatalogProvider : IErrorCategoryCatalogProvider
    {
        public Task<Response<ErrorCategoryCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(Response<ErrorCategoryCatalogProviderPayload>.Ok(
                new ErrorCategoryCatalogProviderPayload
                {
                    Document = new ErrorCategoryCatalogDocument(),
                    ValidationResult = new ErrorCatalogValidationResult()
                }));
        }
    }

    private sealed class EmptyCodeGroupCatalogProvider : IErrorCodeGroupCatalogProvider
    {
        public Task<Response<ErrorCodeGroupCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(Response<ErrorCodeGroupCatalogProviderPayload>.Ok(
                new ErrorCodeGroupCatalogProviderPayload
                {
                    Document = new ErrorCodeGroupCatalogDocument(),
                    ValidationResult = new ErrorCatalogValidationResult()
                }));
        }
    }

    private sealed class EmptyOwnerCatalogProvider : IErrorOwnerCatalogProvider
    {
        public Task<Response<ErrorOwnerCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(Response<ErrorOwnerCatalogProviderPayload>.Ok(
                new ErrorOwnerCatalogProviderPayload
                {
                    Document = new ErrorOwnerCatalogDocument(),
                    ValidationResult = new ErrorCatalogValidationResult()
                }));
        }
    }

    private sealed class EmptyProfileCatalogProvider : IErrorProfileCatalogProvider
    {
        public Task<Response<ErrorProfileCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(Response<ErrorProfileCatalogProviderPayload>.Ok(
                new ErrorProfileCatalogProviderPayload
                {
                    Document = new ErrorProfileCatalogDocument(),
                    ValidationResult = new ErrorCatalogValidationResult()
                }));
        }
    }
}
