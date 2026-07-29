using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCatalogContextProviderCrossValidationErrorSelectionTests
{
    [Fact]
    public async Task LoadFromJsonsAsync_ShouldSelectFirstErrorIssueAfterEarlierInformationIssue()
    {
        ErrorCatalogContextProvider provider = new(
            new MixedSeverityErrorCatalogProvider(),
            new CategoryCatalogProvider(),
            new CodeGroupCatalogProvider(),
            new OwnerCatalogProvider(),
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

    private sealed class MixedSeverityErrorCatalogProvider : IErrorCatalogProvider
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
                        Name = "INFORMATIONFIRST",
                        Owner = "AFW",
                        CodePrefix = "GEN",
                        CodeGroup = "GENERAL",
                        PrimaryCategory = "GENERAL",
                        Categories = ["SECONDARY"],
                        Title = "Information first",
                        Message = "Produces information before a later error.",
                        DefaultSeverity = "Error"
                    },
                    new ErrorDefinition
                    {
                        Id = "AFW_GEN_0002",
                        Code = 100002,
                        Name = "ERRORSECOND",
                        Owner = "MISSING_OWNER",
                        CodePrefix = "GEN",
                        CodeGroup = "GENERAL",
                        PrimaryCategory = "GENERAL",
                        Categories = ["GENERAL"],
                        Title = "Error second",
                        Message = "Produces the actual cross-validation error.",
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

    private sealed class CategoryCatalogProvider : IErrorCategoryCatalogProvider
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
                            },
                            new ErrorCategoryDefinition
                            {
                                Name = "SECONDARY",
                                DisplayName = "Secondary"
                            }
                        ]
                    },
                    ValidationResult = new ErrorCatalogValidationResult()
                }));
        }
    }

    private sealed class CodeGroupCatalogProvider : IErrorCodeGroupCatalogProvider
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

    private sealed class OwnerCatalogProvider : IErrorOwnerCatalogProvider
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
