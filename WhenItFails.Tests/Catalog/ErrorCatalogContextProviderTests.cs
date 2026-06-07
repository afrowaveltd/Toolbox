using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCatalogContextProviderTests
{
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenErrorCatalogProviderIsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ErrorCatalogContextProvider(
                null!,
                new FakeCategoryCatalogProvider(),
                new FakeCodeGroupCatalogProvider(),
                new FakeOwnerCatalogProvider(),
                new FakeProfileCatalogProvider()));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenCategoryCatalogProviderIsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ErrorCatalogContextProvider(
                new FakeErrorCatalogProvider(),
                null!,
                new FakeCodeGroupCatalogProvider(),
                new FakeOwnerCatalogProvider(),
                new FakeProfileCatalogProvider()));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenCodeGroupCatalogProviderIsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ErrorCatalogContextProvider(
                new FakeErrorCatalogProvider(),
                new FakeCategoryCatalogProvider(),
                null!,
                new FakeOwnerCatalogProvider(),
                new FakeProfileCatalogProvider()));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenOwnerCatalogProviderIsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ErrorCatalogContextProvider(
                new FakeErrorCatalogProvider(),
                new FakeCategoryCatalogProvider(),
                new FakeCodeGroupCatalogProvider(),
                null!,
                new FakeProfileCatalogProvider()));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenProfileCatalogProviderIsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ErrorCatalogContextProvider(
                new FakeErrorCatalogProvider(),
                new FakeCategoryCatalogProvider(),
                new FakeCodeGroupCatalogProvider(),
                new FakeOwnerCatalogProvider(),
                null!));
    }

    [Fact]
    public async Task LoadFromJsonsAsync_ShouldThrowArgumentNullException_WhenOptionsIsNull()
    {
        ErrorCatalogContextProvider provider = CreateSuccessfulProvider();

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => provider.LoadFromJsonsAsync(null!));
    }

    [Fact]
    public async Task LoadFromJsonsAsync_ShouldReturnContext_WhenAllProvidersSucceed()
    {
        ErrorCatalogContextProvider provider = CreateSuccessfulProvider();

        JsonsOptions options = CreateOptions();

        Response<ErrorCatalogContext> response =
            await provider.LoadFromJsonsAsync(options);

        Assert.True(response.IsSuccess);
        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.NotNull(response.Data);

        Assert.NotNull(response.Data.CrossValidationResult);
        Assert.True(response.Data.CrossValidationResult.IsValid);

        Assert.NotNull(response.Data.ErrorCatalog);
        Assert.NotNull(response.Data.CategoryCatalog);
        Assert.NotNull(response.Data.CodeGroupCatalog);
        Assert.NotNull(response.Data.OwnerCatalog);
        Assert.NotNull(response.Data.ProfileCatalog);

        Assert.NotEmpty(response.Data.ErrorCatalog.GetAll());
        Assert.NotEmpty(response.Data.CategoryCatalog.Categories);
        Assert.NotEmpty(response.Data.CodeGroupCatalog.CodeGroups);
        Assert.NotEmpty(response.Data.OwnerCatalog.Owners);
        Assert.NotEmpty(response.Data.ProfileCatalog.Profiles);
    }

    [Fact]
    public async Task LoadFromJsonsAsync_ShouldReturnSourceStatus_WhenErrorCatalogProviderFails()
    {
        ErrorCatalogContextProvider provider = new(
            new FakeErrorCatalogProvider(
                Response<ErrorCatalogProviderPayload>.NotFound(
                    code: "ErrorCatalogMissing",
                    message: "Error catalog is missing.")),
            new FakeCategoryCatalogProvider(),
            new FakeCodeGroupCatalogProvider(),
            new FakeOwnerCatalogProvider(),
            new FakeProfileCatalogProvider());

        Response<ErrorCatalogContext> response =
            await provider.LoadFromJsonsAsync(CreateOptions());

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.NotFound, response.Status);
        Assert.NotEmpty(response.Issues);
        Assert.Equal("ErrorCatalogMissing", response.Issues[0].Code);
    }

    [Fact]
    public async Task LoadFromJsonsAsync_ShouldReturnSourceStatus_WhenCategoryCatalogProviderFails()
    {
        ErrorCatalogContextProvider provider = new(
            new FakeErrorCatalogProvider(),
            new FakeCategoryCatalogProvider(
                Response<ErrorCategoryCatalogProviderPayload>.Invalid(
                    code: "CategoryCatalogInvalid",
                    message: "Category catalog is invalid.")),
            new FakeCodeGroupCatalogProvider(),
            new FakeOwnerCatalogProvider(),
            new FakeProfileCatalogProvider());

        Response<ErrorCatalogContext> response =
            await provider.LoadFromJsonsAsync(CreateOptions());

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.NotEmpty(response.Issues);
        Assert.Equal("CategoryCatalogInvalid", response.Issues[0].Code);
    }

    [Fact]
    public async Task LoadFromJsonsAsync_ShouldReturnSourceStatus_WhenCodeGroupCatalogProviderFails()
    {
        ErrorCatalogContextProvider provider = new(
            new FakeErrorCatalogProvider(),
            new FakeCategoryCatalogProvider(),
            new FakeCodeGroupCatalogProvider(
                Response<ErrorCodeGroupCatalogProviderPayload>.Invalid(
                    code: "CodeGroupCatalogInvalid",
                    message: "Code group catalog is invalid.")),
            new FakeOwnerCatalogProvider(),
            new FakeProfileCatalogProvider());

        Response<ErrorCatalogContext> response =
            await provider.LoadFromJsonsAsync(CreateOptions());

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.NotEmpty(response.Issues);
        Assert.Equal("CodeGroupCatalogInvalid", response.Issues[0].Code);
    }

    [Fact]
    public async Task LoadFromJsonsAsync_ShouldReturnSourceStatus_WhenOwnerCatalogProviderFails()
    {
        ErrorCatalogContextProvider provider = new(
            new FakeErrorCatalogProvider(),
            new FakeCategoryCatalogProvider(),
            new FakeCodeGroupCatalogProvider(),
            new FakeOwnerCatalogProvider(
                Response<ErrorOwnerCatalogProviderPayload>.Invalid(
                    code: "OwnerCatalogInvalid",
                    message: "Owner catalog is invalid.")),
            new FakeProfileCatalogProvider());

        Response<ErrorCatalogContext> response =
            await provider.LoadFromJsonsAsync(CreateOptions());

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.NotEmpty(response.Issues);
        Assert.Equal("OwnerCatalogInvalid", response.Issues[0].Code);
    }

    [Fact]
    public async Task LoadFromJsonsAsync_ShouldReturnSourceStatus_WhenProfileCatalogProviderFails()
    {
        ErrorCatalogContextProvider provider = new(
            new FakeErrorCatalogProvider(),
            new FakeCategoryCatalogProvider(),
            new FakeCodeGroupCatalogProvider(),
            new FakeOwnerCatalogProvider(),
            new FakeProfileCatalogProvider(
                Response<ErrorProfileCatalogProviderPayload>.Invalid(
                    code: "ProfileCatalogInvalid",
                    message: "Profile catalog is invalid.")));

        Response<ErrorCatalogContext> response =
            await provider.LoadFromJsonsAsync(CreateOptions());

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.NotEmpty(response.Issues);
        Assert.Equal("ProfileCatalogInvalid", response.Issues[0].Code);
    }

    [Fact]
    public async Task LoadFromJsonsAsync_ShouldReturnInvalidResponse_WhenAnySuccessfulProviderHasNullPayload()
    {
        ErrorCatalogContextProvider provider = new(
            new FakeErrorCatalogProvider(
                Response<ErrorCatalogProviderPayload>.Ok(null)),
            new FakeCategoryCatalogProvider(),
            new FakeCodeGroupCatalogProvider(),
            new FakeOwnerCatalogProvider(),
            new FakeProfileCatalogProvider());

        Response<ErrorCatalogContext> response =
            await provider.LoadFromJsonsAsync(CreateOptions());

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.NotEmpty(response.Issues);
        Assert.Equal("ErrorCatalogContextPayloadIsNull", response.Issues[0].Code);
    }

    [Fact]
    public async Task LoadFromJsonsAsync_ShouldThrowOperationCanceledException_WhenCancelled()
    {
        ErrorCatalogContextProvider provider = CreateSuccessfulProvider();

        using CancellationTokenSource cancellationTokenSource = new();
        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => provider.LoadFromJsonsAsync(
                CreateOptions(),
                cancellationTokenSource.Token));
    }

    [Fact]
    public async Task LoadFromJsonsAsync_ShouldReturnInvalidResponse_WhenCrossValidationFails()
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
                Id = "UNKNOWN_GEN_0001",
                Code = 100001,
                Name = "UNKNOWNERROR",
                Owner = "UNKNOWN",
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

        ErrorCatalog catalog = new(document.Errors);

        ErrorCatalogContextProvider provider = new(
            new FakeErrorCatalogProvider(
                Response<ErrorCatalogProviderPayload>.Ok(
                    new ErrorCatalogProviderPayload
                    {
                        Catalog = catalog,
                        Document = document,
                        ValidationResult = new ErrorCatalogValidationResult()
                    })),
            new FakeCategoryCatalogProvider(),
            new FakeCodeGroupCatalogProvider(),
            new FakeOwnerCatalogProvider(),
            new FakeProfileCatalogProvider());

        Response<ErrorCatalogContext> response =
            await provider.LoadFromJsonsAsync(CreateOptions());

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.NotEmpty(response.Issues);
        Assert.Equal("UnknownErrorOwner", response.Issues[0].Code);
    }

    private static ErrorCatalogContextProvider CreateSuccessfulProvider()
    {
        return new ErrorCatalogContextProvider(
            new FakeErrorCatalogProvider(),
            new FakeCategoryCatalogProvider(),
            new FakeCodeGroupCatalogProvider(),
            new FakeOwnerCatalogProvider(),
            new FakeProfileCatalogProvider());
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

        ErrorCatalog catalog = new(document.Errors);

        return new ErrorCatalogProviderPayload
        {
            Catalog = catalog,
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

    private sealed class FakeErrorCatalogProvider : IErrorCatalogProvider
    {
        private readonly Response<ErrorCatalogProviderPayload> _response;

        public FakeErrorCatalogProvider(
            Response<ErrorCatalogProviderPayload>? response = null)
        {
            _response = response ?? Response<ErrorCatalogProviderPayload>.Ok(
                CreateErrorCatalogPayload());
        }

        public Task<Response<ErrorCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(_response);
        }
    }

    private sealed class FakeCategoryCatalogProvider : IErrorCategoryCatalogProvider
    {
        private readonly Response<ErrorCategoryCatalogProviderPayload> _response;

        public FakeCategoryCatalogProvider(
            Response<ErrorCategoryCatalogProviderPayload>? response = null)
        {
            _response = response ?? Response<ErrorCategoryCatalogProviderPayload>.Ok(
                CreateCategoryCatalogPayload());
        }

        public Task<Response<ErrorCategoryCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(_response);
        }
    }

    private sealed class FakeCodeGroupCatalogProvider : IErrorCodeGroupCatalogProvider
    {
        private readonly Response<ErrorCodeGroupCatalogProviderPayload> _response;

        public FakeCodeGroupCatalogProvider(
            Response<ErrorCodeGroupCatalogProviderPayload>? response = null)
        {
            _response = response ?? Response<ErrorCodeGroupCatalogProviderPayload>.Ok(
                CreateCodeGroupCatalogPayload());
        }

        public Task<Response<ErrorCodeGroupCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(_response);
        }
    }

    private sealed class FakeOwnerCatalogProvider : IErrorOwnerCatalogProvider
    {
        private readonly Response<ErrorOwnerCatalogProviderPayload> _response;

        public FakeOwnerCatalogProvider(
            Response<ErrorOwnerCatalogProviderPayload>? response = null)
        {
            _response = response ?? Response<ErrorOwnerCatalogProviderPayload>.Ok(
                CreateOwnerCatalogPayload());
        }

        public Task<Response<ErrorOwnerCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(_response);
        }
    }

    private sealed class FakeProfileCatalogProvider : IErrorProfileCatalogProvider
    {
        private readonly Response<ErrorProfileCatalogProviderPayload> _response;

        public FakeProfileCatalogProvider(
            Response<ErrorProfileCatalogProviderPayload>? response = null)
        {
            _response = response ?? Response<ErrorProfileCatalogProviderPayload>.Ok(
                CreateProfileCatalogPayload());
        }

        public Task<Response<ErrorProfileCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(_response);
        }
    }
}