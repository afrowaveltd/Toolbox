using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCatalogContextProviderCrossValidationWarningTests
{
    [Fact]
    public async Task LoadFromJsonsAsync_ShouldSucceedAndPreserveCrossValidationWarnings()
    {
        ErrorCatalogContextProvider provider = new(
            new ErrorProvider(),
            new CategoryProvider(),
            new CodeGroupProvider(),
            new OwnerProvider(),
            new ProfileProvider());

        Response<ErrorCatalogContext> response = await provider.LoadFromJsonsAsync(
            new JsonsOptions
            {
                RootDirectory = "Jsons",
                PackageDirectoryName = "WhenItFails"
            });

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.True(response.Data.CrossValidationResult.IsValid);

        var issue = Assert.Single(response.Data.CrossValidationResult.Issues);
        Assert.Equal("UnknownProfileIncludeError", issue.Code);
        Assert.Equal(
            "Profile 'WEB' includes error 'MISSING_ERROR', but this error is not defined in the error catalog.",
            issue.Message);
    }

    private sealed class ErrorProvider : IErrorCatalogProvider
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

            return Task.FromResult(Response<ErrorCatalogProviderPayload>.Ok(
                new ErrorCatalogProviderPayload
                {
                    Catalog = new ErrorCatalog(document.Errors),
                    Document = document,
                    ValidationResult = new ErrorCatalogValidationResult()
                }));
        }
    }

    private sealed class CategoryProvider : IErrorCategoryCatalogProvider
    {
        public Task<Response<ErrorCategoryCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(Response<ErrorCategoryCatalogProviderPayload>.Ok(
                new ErrorCategoryCatalogProviderPayload
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
                }));
        }
    }

    private sealed class CodeGroupProvider : IErrorCodeGroupCatalogProvider
    {
        public Task<Response<ErrorCodeGroupCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(Response<ErrorCodeGroupCatalogProviderPayload>.Ok(
                new ErrorCodeGroupCatalogProviderPayload
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
                }));
        }
    }

    private sealed class OwnerProvider : IErrorOwnerCatalogProvider
    {
        public Task<Response<ErrorOwnerCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(Response<ErrorOwnerCatalogProviderPayload>.Ok(
                new ErrorOwnerCatalogProviderPayload
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
                }));
        }
    }

    private sealed class ProfileProvider : IErrorProfileCatalogProvider
    {
        public Task<Response<ErrorProfileCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(Response<ErrorProfileCatalogProviderPayload>.Ok(
                new ErrorProfileCatalogProviderPayload
                {
                    Document = new ErrorProfileCatalogDocument
                    {
                        Profiles =
                        [
                            new ErrorProfileDefinition
                            {
                                Name = "WEB",
                                DisplayName = "Web",
                                IncludeErrors = ["MISSING_ERROR"]
                            }
                        ]
                    },
                    ValidationResult = new ErrorCatalogValidationResult()
                }));
        }
    }
}
