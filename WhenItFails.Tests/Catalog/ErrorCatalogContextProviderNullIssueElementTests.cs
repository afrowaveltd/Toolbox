using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Issues;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCatalogContextProviderNullIssueElementTests
{
    [Fact]
    public async Task LoadFromJsonsAsync_ShouldIgnoreNullIssueElementsAndPreserveValidWarnings()
    {
        IssueInfo warning = IssueInfoFactory.Warning(
            "ErrorCatalogProviderWarning",
            "The error catalog loaded with a recoverable warning.");

        Response<ErrorCatalogProviderPayload> sourceResponse = new()
        {
            Status = ResultStatus.SuccessWithWarnings,
            Data = CreateErrorCatalogPayload(),
            Issues = [null!, warning]
        };

        ErrorCatalogContextProvider provider = new(
            new FakeErrorCatalogProvider(sourceResponse),
            new FakeCategoryCatalogProvider(),
            new FakeCodeGroupCatalogProvider(),
            new FakeOwnerCatalogProvider(),
            new FakeProfileCatalogProvider());

        Response<ErrorCatalogContext> response =
            await provider.LoadFromJsonsAsync(CreateOptions());

        Assert.True(response.IsSuccess);
        Assert.Equal(ResultStatus.SuccessWithWarnings, response.Status);
        Assert.True(response.HasWarnings);
        Assert.NotNull(response.Data);
        Assert.True(response.Data.CrossValidationResult.IsValid);

        IssueInfo outputWarning = Assert.Single(response.Issues);
        Assert.Same(warning, outputWarning);
        Assert.Equal(IssueSeverity.Warning, outputWarning.Severity);
    }

    private static JsonsOptions CreateOptions()
    {
        return new JsonsOptions
        {
            RootDirectory = "Jsons",
            PackageDirectoryName = "WhenItFails"
        };
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

    private static ErrorCategoryCatalogProviderPayload CreateCategoryCatalogPayload()
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

    private static ErrorCodeGroupCatalogProviderPayload CreateCodeGroupCatalogPayload()
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

    private static ErrorOwnerCatalogProviderPayload CreateOwnerCatalogPayload()
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

    private static ErrorProfileCatalogProviderPayload CreateProfileCatalogPayload()
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

    private sealed class FakeErrorCatalogProvider(
        Response<ErrorCatalogProviderPayload> response)
        : IErrorCatalogProvider
    {
        public Task<Response<ErrorCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(response);
        }
    }

    private sealed class FakeCategoryCatalogProvider : IErrorCategoryCatalogProvider
    {
        public Task<Response<ErrorCategoryCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(
                Response<ErrorCategoryCatalogProviderPayload>.Ok(
                    CreateCategoryCatalogPayload()));
        }
    }

    private sealed class FakeCodeGroupCatalogProvider : IErrorCodeGroupCatalogProvider
    {
        public Task<Response<ErrorCodeGroupCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(
                Response<ErrorCodeGroupCatalogProviderPayload>.Ok(
                    CreateCodeGroupCatalogPayload()));
        }
    }

    private sealed class FakeOwnerCatalogProvider : IErrorOwnerCatalogProvider
    {
        public Task<Response<ErrorOwnerCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(
                Response<ErrorOwnerCatalogProviderPayload>.Ok(
                    CreateOwnerCatalogPayload()));
        }
    }

    private sealed class FakeProfileCatalogProvider : IErrorProfileCatalogProvider
    {
        public Task<Response<ErrorProfileCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(
                Response<ErrorProfileCatalogProviderPayload>.Ok(
                    CreateProfileCatalogPayload()));
        }
    }
}
