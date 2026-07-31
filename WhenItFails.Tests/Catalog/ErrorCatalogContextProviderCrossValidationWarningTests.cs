using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Issues;
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
            new WarningProfileProvider());

        Response<ErrorCatalogContext> response = await provider.LoadFromJsonsAsync(
            CreateOptions());

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.True(response.Data.CrossValidationResult.IsValid);

        var issue = Assert.Single(response.Data.CrossValidationResult.Issues);
        Assert.Equal("UnknownProfileIncludeError", issue.Code);
        Assert.Equal(
            "Profile 'WEB' includes error 'MISSING_ERROR', but this error is not defined in the error catalog.",
            issue.Message);
    }

    [Fact]
    public async Task LoadFromJsonsAsync_ShouldSucceedAndPreserveCrossValidationInformation()
    {
        ErrorCatalogContextProvider provider = new(
            new InformationErrorProvider(),
            new InformationCategoryProvider(),
            new CodeGroupProvider(),
            new OwnerProvider(),
            new EmptyProfileProvider());

        Response<ErrorCatalogContext> response = await provider.LoadFromJsonsAsync(
            CreateOptions());

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.True(response.Data.CrossValidationResult.IsValid);

        var issue = Assert.Single(response.Data.CrossValidationResult.Issues);
        Assert.Equal("PrimaryCategoryNotListedInCategories", issue.Code);
        Assert.Equal(
            "Primary category 'GENERAL' is not listed in the additional categories collection.",
            issue.Message);
    }

    [Fact]
    public async Task LoadFromJsonsAsync_ShouldKeepMixedNonErrorIssuesInsideCrossValidationResult()
    {
        ErrorCatalogContextProvider provider = new(
            new InformationErrorProvider(),
            new InformationCategoryProvider(),
            new CodeGroupProvider(),
            new OwnerProvider(),
            new WarningProfileProvider());

        Response<ErrorCatalogContext> response = await provider.LoadFromJsonsAsync(
            CreateOptions());

        Assert.True(response.IsSuccess);
        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.NotNull(response.Data);
        Assert.Empty(response.Issues);
        Assert.True(string.IsNullOrEmpty(response.Message));
        Assert.True(response.Data.CrossValidationResult.IsValid);
        Assert.Collection(
            response.Data.CrossValidationResult.Issues,
            issue => Assert.Equal("PrimaryCategoryNotListedInCategories", issue.Code),
            issue => Assert.Equal("UnknownProfileIncludeError", issue.Code));
    }

    [Fact]
    public async Task LoadFromJsonsAsync_ShouldKeepProviderAndCrossValidationWarningsInSeparateLayers()
    {
        ErrorCatalogContextProvider provider = new(
            new WarningErrorProvider(),
            new CategoryProvider(),
            new CodeGroupProvider(),
            new OwnerProvider(),
            new WarningProfileProvider());

        Response<ErrorCatalogContext> response = await provider.LoadFromJsonsAsync(
            CreateOptions());

        Assert.True(response.IsSuccess);
        Assert.Equal(ResultStatus.SuccessWithWarnings, response.Status);
        Assert.True(response.HasWarnings);
        Assert.NotNull(response.Data);
        Assert.True(response.Data.CrossValidationResult.IsValid);

        IssueInfo providerWarning = Assert.Single(response.Issues);
        Assert.Equal("ErrorCatalogProviderWarning", providerWarning.Code);
        Assert.Equal(IssueSeverity.Warning, providerWarning.Severity);

        var crossValidationWarning = Assert.Single(
            response.Data.CrossValidationResult.Issues);
        Assert.Equal("UnknownProfileIncludeError", crossValidationWarning.Code);
    }

    private static JsonsOptions CreateOptions()
    {
        return new JsonsOptions
        {
            RootDirectory = "Jsons",
            PackageDirectoryName = "WhenItFails"
        };
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
                    CreateErrorDefinition(["GENERAL"])
                ]
            };

            return Task.FromResult(Response<ErrorCatalogProviderPayload>.Ok(
                CreateErrorPayload(document)));
        }
    }

    private sealed class WarningErrorProvider : IErrorCatalogProvider
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
                    CreateErrorDefinition(["GENERAL"])
                ]
            };

            IssueInfo warning = IssueInfoFactory.Warning(
                "ErrorCatalogProviderWarning",
                "The error catalog loaded with a recoverable warning.");

            return Task.FromResult(
                Response<ErrorCatalogProviderPayload>.OkWithWarnings(
                    CreateErrorPayload(document),
                    [warning]));
        }
    }

    private sealed class InformationErrorProvider : IErrorCatalogProvider
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
                    CreateErrorDefinition(["SECONDARY"])
                ]
            };

            return Task.FromResult(Response<ErrorCatalogProviderPayload>.Ok(
                CreateErrorPayload(document)));
        }
    }

    private static ErrorDefinition CreateErrorDefinition(List<string> categories)
    {
        return new ErrorDefinition
        {
            Id = "AFW_GEN_0001",
            Code = 100001,
            Name = "UNKNOWNERROR",
            Owner = "AFW",
            CodePrefix = "GEN",
            CodeGroup = "GENERAL",
            PrimaryCategory = "GENERAL",
            Categories = categories,
            Title = "Unknown error",
            Message = "An unknown error occurred.",
            DefaultSeverity = "Error"
        };
    }

    private static ErrorCatalogProviderPayload CreateErrorPayload(
        ErrorCatalogDocument document)
    {
        return new ErrorCatalogProviderPayload
        {
            Catalog = new ErrorCatalog(document.Errors),
            Document = document,
            ValidationResult = new ErrorCatalogValidationResult()
        };
    }

    private sealed class CategoryProvider : IErrorCategoryCatalogProvider
    {
        public Task<Response<ErrorCategoryCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(Response<ErrorCategoryCatalogProviderPayload>.Ok(
                CreateCategoryPayload(
                    new ErrorCategoryDefinition
                    {
                        Name = "GENERAL",
                        DisplayName = "General"
                    })));
        }
    }

    private sealed class InformationCategoryProvider : IErrorCategoryCatalogProvider
    {
        public Task<Response<ErrorCategoryCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(Response<ErrorCategoryCatalogProviderPayload>.Ok(
                CreateCategoryPayload(
                    new ErrorCategoryDefinition
                    {
                        Name = "GENERAL",
                        DisplayName = "General"
                    },
                    new ErrorCategoryDefinition
                    {
                        Name = "SECONDARY",
                        DisplayName = "Secondary"
                    })));
        }
    }

    private static ErrorCategoryCatalogProviderPayload CreateCategoryPayload(
        params ErrorCategoryDefinition[] categories)
    {
        return new ErrorCategoryCatalogProviderPayload
        {
            Document = new ErrorCategoryCatalogDocument
            {
                Categories = [.. categories]
            },
            ValidationResult = new ErrorCatalogValidationResult()
        };
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

    private sealed class WarningProfileProvider : IErrorProfileCatalogProvider
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

    private sealed class EmptyProfileProvider : IErrorProfileCatalogProvider
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
