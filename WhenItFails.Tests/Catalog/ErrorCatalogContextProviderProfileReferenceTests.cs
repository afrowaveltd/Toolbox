using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCatalogContextProviderProfileReferenceTests
{
    [Fact]
    public async Task LoadFromJsonsAsync_ShouldReuseValidatedDocumentsFromProviderPayloads()
    {
        ErrorCatalogProviderPayload errorPayload = CreateErrorCatalogPayload();
        ErrorCategoryCatalogProviderPayload categoryPayload = CreateCategoryPayload();
        ErrorCodeGroupCatalogProviderPayload codeGroupPayload = CreateCodeGroupPayload();
        ErrorOwnerCatalogProviderPayload ownerPayload = CreateOwnerPayload();
        ErrorProfileCatalogProviderPayload profilePayload = CreateProfilePayload();

        ErrorCatalogContextProvider provider = new(
            new FakeErrorCatalogProvider(errorPayload),
            new FakeCategoryCatalogProvider(categoryPayload),
            new FakeCodeGroupCatalogProvider(codeGroupPayload),
            new FakeOwnerCatalogProvider(ownerPayload),
            new FakeProfileCatalogProvider(profilePayload));

        Response<ErrorCatalogContext> response = await provider.LoadFromJsonsAsync(
            CreateOptions());

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.Same(errorPayload.Catalog, response.Data.ErrorCatalog);
        Assert.Same(errorPayload.Document, response.Data.ErrorCatalogDocument);
        Assert.Same(categoryPayload.Document, response.Data.CategoryCatalog);
        Assert.Same(codeGroupPayload.Document, response.Data.CodeGroupCatalog);
        Assert.Same(ownerPayload.Document, response.Data.OwnerCatalog);
        Assert.Same(profilePayload.Document, response.Data.ProfileCatalog);

        Assert.NotSame(errorPayload.ValidationResult, response.Data.CrossValidationResult);
        Assert.NotSame(categoryPayload.ValidationResult, response.Data.CrossValidationResult);
        Assert.NotSame(codeGroupPayload.ValidationResult, response.Data.CrossValidationResult);
        Assert.NotSame(ownerPayload.ValidationResult, response.Data.CrossValidationResult);
        Assert.NotSame(profilePayload.ValidationResult, response.Data.CrossValidationResult);
        Assert.True(response.Data.CrossValidationResult.IsValid);
    }

    [Fact]
    public async Task LoadFromJsonsAsync_ShouldUseFreshCrossValidation_WhenSuccessfulProviderPayloadsContainLocalIssues()
    {
        ErrorCatalogProviderPayload errorPayload = CreateErrorCatalogPayload();
        ErrorCategoryCatalogProviderPayload categoryPayload = CreateCategoryPayload();
        ErrorCodeGroupCatalogProviderPayload codeGroupPayload = CreateCodeGroupPayload();
        ErrorOwnerCatalogProviderPayload ownerPayload = CreateOwnerPayload();
        ErrorProfileCatalogProviderPayload profilePayload = CreateProfilePayload();

        AddProviderLocalError(errorPayload.ValidationResult, "ErrorProviderLocalIssue");
        AddProviderLocalError(categoryPayload.ValidationResult, "CategoryProviderLocalIssue");
        AddProviderLocalError(codeGroupPayload.ValidationResult, "CodeGroupProviderLocalIssue");
        AddProviderLocalError(ownerPayload.ValidationResult, "OwnerProviderLocalIssue");
        AddProviderLocalError(profilePayload.ValidationResult, "ProfileProviderLocalIssue");

        ErrorCatalogContextProvider provider = new(
            new FakeErrorCatalogProvider(errorPayload),
            new FakeCategoryCatalogProvider(categoryPayload),
            new FakeCodeGroupCatalogProvider(codeGroupPayload),
            new FakeOwnerCatalogProvider(ownerPayload),
            new FakeProfileCatalogProvider(profilePayload));

        Response<ErrorCatalogContext> response = await provider.LoadFromJsonsAsync(
            CreateOptions());

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.True(response.Data.CrossValidationResult.IsValid);
        Assert.Empty(response.Data.CrossValidationResult.Issues);
        Assert.NotSame(errorPayload.ValidationResult, response.Data.CrossValidationResult);
        Assert.NotSame(categoryPayload.ValidationResult, response.Data.CrossValidationResult);
        Assert.NotSame(codeGroupPayload.ValidationResult, response.Data.CrossValidationResult);
        Assert.NotSame(ownerPayload.ValidationResult, response.Data.CrossValidationResult);
        Assert.NotSame(profilePayload.ValidationResult, response.Data.CrossValidationResult);
    }

    private static JsonsOptions CreateOptions()
    {
        return new JsonsOptions
        {
            RootDirectory = "Jsons",
            PackageDirectoryName = "WhenItFails"
        };
    }

    private static void AddProviderLocalError(
        ErrorCatalogValidationResult validationResult,
        string code)
    {
        validationResult.AddError(
            code: code,
            message: "Provider-local validation issue.",
            path: "$");
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

    private sealed class FakeErrorCatalogProvider : IErrorCatalogProvider
    {
        private readonly ErrorCatalogProviderPayload _payload;

        public FakeErrorCatalogProvider(ErrorCatalogProviderPayload payload)
        {
            _payload = payload;
        }

        public Task<Response<ErrorCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(Response<ErrorCatalogProviderPayload>.Ok(_payload));
        }
    }

    private sealed class FakeCategoryCatalogProvider : IErrorCategoryCatalogProvider
    {
        private readonly ErrorCategoryCatalogProviderPayload _payload;

        public FakeCategoryCatalogProvider(ErrorCategoryCatalogProviderPayload payload)
        {
            _payload = payload;
        }

        public Task<Response<ErrorCategoryCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(Response<ErrorCategoryCatalogProviderPayload>.Ok(_payload));
        }
    }

    private sealed class FakeCodeGroupCatalogProvider : IErrorCodeGroupCatalogProvider
    {
        private readonly ErrorCodeGroupCatalogProviderPayload _payload;

        public FakeCodeGroupCatalogProvider(ErrorCodeGroupCatalogProviderPayload payload)
        {
            _payload = payload;
        }

        public Task<Response<ErrorCodeGroupCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(Response<ErrorCodeGroupCatalogProviderPayload>.Ok(_payload));
        }
    }

    private sealed class FakeOwnerCatalogProvider : IErrorOwnerCatalogProvider
    {
        private readonly ErrorOwnerCatalogProviderPayload _payload;

        public FakeOwnerCatalogProvider(ErrorOwnerCatalogProviderPayload payload)
        {
            _payload = payload;
        }

        public Task<Response<ErrorOwnerCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(Response<ErrorOwnerCatalogProviderPayload>.Ok(_payload));
        }
    }

    private sealed class FakeProfileCatalogProvider : IErrorProfileCatalogProvider
    {
        private readonly ErrorProfileCatalogProviderPayload _payload;

        public FakeProfileCatalogProvider(ErrorProfileCatalogProviderPayload payload)
        {
            _payload = payload;
        }

        public Task<Response<ErrorProfileCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(Response<ErrorProfileCatalogProviderPayload>.Ok(_payload));
        }
    }
}
