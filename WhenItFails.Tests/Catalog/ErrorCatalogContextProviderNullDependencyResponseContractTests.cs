using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCatalogContextProviderNullDependencyResponseContractTests
{
    [Theory]
    [InlineData(
        ProviderKind.ErrorCatalog,
        "WIF_ERROR_CATALOG_PROVIDER_RESPONSE_NULL",
        "The error catalog provider returned a null response.")]
    [InlineData(
        ProviderKind.CategoryCatalog,
        "WIF_CATEGORY_CATALOG_PROVIDER_RESPONSE_NULL",
        "The error category catalog provider returned a null response.")]
    [InlineData(
        ProviderKind.CodeGroupCatalog,
        "WIF_CODE_GROUP_CATALOG_PROVIDER_RESPONSE_NULL",
        "The error code group catalog provider returned a null response.")]
    [InlineData(
        ProviderKind.OwnerCatalog,
        "WIF_OWNER_CATALOG_PROVIDER_RESPONSE_NULL",
        "The error owner catalog provider returned a null response.")]
    [InlineData(
        ProviderKind.ProfileCatalog,
        "WIF_PROFILE_CATALOG_PROVIDER_RESPONSE_NULL",
        "The error profile catalog provider returned a null response.")]
    public async Task LoadFromJsonsAsync_ShouldReturnInvalidResponse_WhenDependencyReturnsNull(
        ProviderKind providerKind,
        string expectedCode,
        string expectedMessage)
    {
        ErrorCatalogContextProvider provider = CreateProvider(providerKind);

        Response<ErrorCatalogContext> response =
            await provider.LoadFromJsonsAsync(new JsonsOptions
            {
                RootDirectory = "Jsons",
                PackageDirectoryName = "WhenItFails"
            });

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);

        var issue = Assert.Single(response.Issues);
        Assert.Equal(expectedCode, issue.Code);
        Assert.Equal(expectedMessage, response.Message);
    }

    private static ErrorCatalogContextProvider CreateProvider(
        ProviderKind providerKind)
    {
        return new ErrorCatalogContextProvider(
            new TestErrorCatalogProvider(
                providerKind == ProviderKind.ErrorCatalog),
            new TestCategoryCatalogProvider(
                providerKind == ProviderKind.CategoryCatalog),
            new TestCodeGroupCatalogProvider(
                providerKind == ProviderKind.CodeGroupCatalog),
            new TestOwnerCatalogProvider(
                providerKind == ProviderKind.OwnerCatalog),
            new TestProfileCatalogProvider(
                providerKind == ProviderKind.ProfileCatalog));
    }

    private static ErrorCatalogProviderPayload CreateErrorCatalogPayload()
    {
        ErrorCatalogDocument document = new()
        {
            SchemaVersion = "1.0",
            CatalogId = "test.errors",
            CatalogName = "Test Errors",
            Language = "en",
            Errors =
            [
                new ErrorDefinition
                {
                    Id = "AFW_GEN_0001",
                    Code = 100001,
                    Name = "UNKNOWNERROR",
                    Owner = "AFW",
                    CodePrefix = "GEN",
                    CodeGroup = "GENERAL",
                    PrimaryCategory = "GENERAL",
                    Categories = ["GENERAL"],
                    Title = "Unknown error",
                    Message = "An unknown error occurred.",
                    DefaultSeverity = "Error"
                }
            ]
        };

        return new ErrorCatalogProviderPayload
        {
            Catalog = new ErrorCatalog(document.Errors),
            Document = document,
            ValidationResult = new ErrorCatalogValidationResult()
        };
    }

    private static ErrorCategoryCatalogProviderPayload CreateCategoryPayload()
    {
        return new ErrorCategoryCatalogProviderPayload
        {
            Document = new ErrorCategoryCatalogDocument
            {
                Categories =
                [
                    new ErrorCategoryDefinition
                    {
                        Name = "GENERAL",
                        DisplayName = "General"
                    }
                ]
            },
            ValidationResult = new ErrorCatalogValidationResult()
        };
    }

    private static ErrorCodeGroupCatalogProviderPayload CreateCodeGroupPayload()
    {
        return new ErrorCodeGroupCatalogProviderPayload
        {
            Document = new ErrorCodeGroupCatalogDocument
            {
                CodeGroups =
                [
                    new ErrorCodeGroupDefinition
                    {
                        Name = "GENERAL",
                        DisplayName = "General",
                        CodePrefix = "GEN",
                        CodeFrom = 100000,
                        CodeTo = 199999
                    }
                ]
            },
            ValidationResult = new ErrorCatalogValidationResult()
        };
    }

    private static ErrorOwnerCatalogProviderPayload CreateOwnerPayload()
    {
        return new ErrorOwnerCatalogProviderPayload
        {
            Document = new ErrorOwnerCatalogDocument
            {
                Owners =
                [
                    new ErrorOwnerDefinition
                    {
                        Name = "AFW",
                        DisplayName = "Afrowave",
                        CodeFrom = 0,
                        CodeTo = 999999,
                        IsBuiltIn = true
                    }
                ]
            },
            ValidationResult = new ErrorCatalogValidationResult()
        };
    }

    private static ErrorProfileCatalogProviderPayload CreateProfilePayload()
    {
        return new ErrorProfileCatalogProviderPayload
        {
            Document = new ErrorProfileCatalogDocument
            {
                Profiles =
                [
                    new ErrorProfileDefinition
                    {
                        Name = "WEB",
                        DisplayName = "Web"
                    }
                ]
            },
            ValidationResult = new ErrorCatalogValidationResult()
        };
    }

    public enum ProviderKind
    {
        ErrorCatalog,
        CategoryCatalog,
        CodeGroupCatalog,
        OwnerCatalog,
        ProfileCatalog
    }

    private sealed class TestErrorCatalogProvider(bool returnNull)
        : IErrorCatalogProvider
    {
        public Task<Response<ErrorCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                returnNull
                    ? null!
                    : Response<ErrorCatalogProviderPayload>.Ok(
                        CreateErrorCatalogPayload()));
        }
    }

    private sealed class TestCategoryCatalogProvider(bool returnNull)
        : IErrorCategoryCatalogProvider
    {
        public Task<Response<ErrorCategoryCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                returnNull
                    ? null!
                    : Response<ErrorCategoryCatalogProviderPayload>.Ok(
                        CreateCategoryPayload()));
        }
    }

    private sealed class TestCodeGroupCatalogProvider(bool returnNull)
        : IErrorCodeGroupCatalogProvider
    {
        public Task<Response<ErrorCodeGroupCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                returnNull
                    ? null!
                    : Response<ErrorCodeGroupCatalogProviderPayload>.Ok(
                        CreateCodeGroupPayload()));
        }
    }

    private sealed class TestOwnerCatalogProvider(bool returnNull)
        : IErrorOwnerCatalogProvider
    {
        public Task<Response<ErrorOwnerCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                returnNull
                    ? null!
                    : Response<ErrorOwnerCatalogProviderPayload>.Ok(
                        CreateOwnerPayload()));
        }
    }

    private sealed class TestProfileCatalogProvider(bool returnNull)
        : IErrorProfileCatalogProvider
    {
        public Task<Response<ErrorProfileCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                returnNull
                    ? null!
                    : Response<ErrorProfileCatalogProviderPayload>.Ok(
                        CreateProfilePayload()));
        }
    }
}
