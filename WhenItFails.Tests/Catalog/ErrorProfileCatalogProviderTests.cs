using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Metadata;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorProfileCatalogProviderTests
{
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenLoaderIsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ErrorProfileCatalogProvider(
                null!,
                new ErrorProfileCatalogDocumentNormalizer(),
                new ErrorProfileCatalogValidator()));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenNormalizerIsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ErrorProfileCatalogProvider(
                new FakeSuccessfulLoader(CreateValidDocument()),
                null!,
                new ErrorProfileCatalogValidator()));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenValidatorIsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ErrorProfileCatalogProvider(
                new FakeSuccessfulLoader(CreateValidDocument()),
                new ErrorProfileCatalogDocumentNormalizer(),
                null!));
    }

    [Fact]
    public async Task LoadFromFileAsync_ShouldReturnNotFoundResponse_WhenLoadingReturnsNotFound()
    {
        ErrorProfileCatalogProvider provider = new(
            new FakeFailedLoader(),
            new ErrorProfileCatalogDocumentNormalizer(),
            new ErrorProfileCatalogValidator());

        Response<ErrorProfileCatalogProviderPayload> response =
            await provider.LoadFromFileAsync("missing.json");

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.NotFound, response.Status);
        Assert.Null(response.Data);
        Assert.NotEmpty(response.Issues);
        Assert.Equal("FileNotFound", response.Issues[0].Code);
    }

    [Fact]
    public async Task LoadFromFileAsync_ShouldReturnInvalidResponse_WhenLoadedDocumentIsNull()
    {
        ErrorProfileCatalogProvider provider = new(
            new FakeSuccessfulLoader(null),
            new ErrorProfileCatalogDocumentNormalizer(),
            new ErrorProfileCatalogValidator());

        Response<ErrorProfileCatalogProviderPayload> response =
            await provider.LoadFromFileAsync("profiles.json");

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);
        Assert.NotEmpty(response.Issues);
        Assert.Equal("LoadedProfileCatalogDocumentIsNull", response.Issues[0].Code);
    }

    [Fact]
    public async Task LoadFromFileAsync_ShouldReturnInvalidResponse_WhenValidationFails()
    {
        ErrorProfileCatalogDocument document = CreateValidDocument();
        document.Profiles[0].Name = string.Empty;

        ErrorProfileCatalogProvider provider = new(
            new FakeSuccessfulLoader(document),
            new ErrorProfileCatalogDocumentNormalizer(),
            new ErrorProfileCatalogValidator());

        Response<ErrorProfileCatalogProviderPayload> response =
            await provider.LoadFromFileAsync("profiles.json");

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);
        Assert.NotEmpty(response.Issues);
        Assert.Equal("ProfileCatalogValidationFailed", response.Issues[0].Code);
    }

    [Fact]
    public async Task LoadFromFileAsync_ShouldReturnDocument_WhenLoadingAndValidationSucceed()
    {
        ErrorProfileCatalogProvider provider = new(
            new FakeSuccessfulLoader(CreateValidDocument()),
            new ErrorProfileCatalogDocumentNormalizer(),
            new ErrorProfileCatalogValidator());

        Response<ErrorProfileCatalogProviderPayload> response =
            await provider.LoadFromFileAsync("profiles.json");

        Assert.True(response.IsSuccess);
        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.NotNull(response.Data);
        Assert.NotNull(response.Data.Document);
        Assert.NotNull(response.Data.ValidationResult);
        Assert.True(response.Data.ValidationResult.IsValid);

        ErrorProfileDefinition profile =
            Assert.Single(response.Data.Document.Profiles);

        Assert.Equal("WEB_API", profile.Name);
        Assert.Equal("Web API", profile.DisplayName);
        Assert.Equal(["AFW", "APP"], profile.IncludeOwners);
        Assert.Equal(["CONFIGURATION", "VALIDATION"], profile.IncludeCodeGroups);
        Assert.Equal(["WEB", "SERVER", "VALIDATION"], profile.IncludeCategories);
        Assert.Equal(["REQUIRED_VALUE"], profile.IncludeSubcategories);
        Assert.Equal(["USER_VISIBLE"], profile.IncludeTags);
        Assert.Equal(["INTERNAL_ONLY"], profile.ExcludeTags);
    }

    [Fact]
    public async Task LoadFromFileAsync_ShouldNormalizeDocumentBeforeReturningPayload()
    {
        ErrorProfileCatalogDocument document = CreateValidDocument();

        document.Profiles[0].Name = " web api ";
        document.Profiles[0].DisplayName = " Web API ";
        document.Profiles[0].IncludeOwners = ["afw", "AFW"];
        document.Profiles[0].IncludeCodeGroups = ["configuration", "validation"];
        document.Profiles[0].IncludeCategories = ["web", "server", "web"];
        document.Profiles[0].IncludeSubcategories = ["required value", "required-value"];
        document.Profiles[0].IncludeTags = ["user visible", "user-visible"];
        document.Profiles[0].ExcludeTags = ["internal only", "internal-only"];
        document.Profiles[0].DefaultMappings =
            new Dictionary<string, string>
            {
                ["web.problemDetails"] = " true "
            };

        ErrorProfileCatalogProvider provider = new(
            new FakeSuccessfulLoader(document),
            new ErrorProfileCatalogDocumentNormalizer(),
            new ErrorProfileCatalogValidator());

        Response<ErrorProfileCatalogProviderPayload> response =
            await provider.LoadFromFileAsync("profiles.json");

        Assert.True(response.IsSuccess);
        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.NotNull(response.Data);

        ErrorProfileDefinition profile =
            Assert.Single(response.Data.Document.Profiles);

        Assert.Equal("WEB_API", profile.Name);
        Assert.Equal("Web API", profile.DisplayName);
        Assert.Equal(["AFW"], profile.IncludeOwners);
        Assert.Equal(["CONFIGURATION", "VALIDATION"], profile.IncludeCodeGroups);
        Assert.Equal(["WEB", "SERVER"], profile.IncludeCategories);
        Assert.Equal(["REQUIRED_VALUE"], profile.IncludeSubcategories);
        Assert.Equal(["USER_VISIBLE"], profile.IncludeTags);
        Assert.Equal(["INTERNAL_ONLY"], profile.ExcludeTags);
        Assert.Equal("true", profile.DefaultMappings["WEB_PROBLEMDETAILS"]);
    }

    [Fact]
    public async Task LoadFromFileAsync_ShouldReturnPayloadIndependentFromLoaderDocument()
    {
        ErrorProfileCatalogDocument loaderDocument = CreateValidDocument();
        loaderDocument.Tags = ["source tag"];
        loaderDocument.Metadata = new MetadataBag(
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["consumer"] = "Setter"
            });

        ErrorProfileCatalogProvider provider = new(
            new FakeSuccessfulLoader(loaderDocument),
            new ErrorProfileCatalogDocumentNormalizer(),
            new ErrorProfileCatalogValidator());

        Response<ErrorProfileCatalogProviderPayload> response =
            await provider.LoadFromFileAsync("profiles.json");

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.NotSame(loaderDocument, response.Data.Document);
        Assert.NotSame(loaderDocument.Tags, response.Data.Document.Tags);
        Assert.NotSame(loaderDocument.Metadata, response.Data.Document.Metadata);
        Assert.NotSame(loaderDocument.Profiles, response.Data.Document.Profiles);
        Assert.NotSame(loaderDocument.Profiles[0], response.Data.Document.Profiles[0]);

        response.Data.Document.Tags.Add("RUNTIME_ONLY");
        response.Data.Document.Metadata.Set("consumer", "Runtime");
        response.Data.Document.Profiles[0].Name = "CHANGED";
        response.Data.Document.Profiles[0].IncludeOwners.Add("RUNTIME");

        Assert.Equal(["source tag"], loaderDocument.Tags);
        Assert.Equal("Setter", loaderDocument.Metadata["consumer"]);
        Assert.Equal("WEB_API", loaderDocument.Profiles[0].Name);
        Assert.Equal(["AFW", "APP"], loaderDocument.Profiles[0].IncludeOwners);
    }

    [Fact]
    public async Task LoadFromFileAsync_ShouldThrowOperationCanceledException_WhenCancelled()
    {
        using CancellationTokenSource cancellationTokenSource = new();
        await cancellationTokenSource.CancelAsync();

        ErrorProfileCatalogProvider provider = new(
            new FakeSuccessfulLoader(CreateValidDocument()),
            new ErrorProfileCatalogDocumentNormalizer(),
            new ErrorProfileCatalogValidator());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => provider.LoadFromFileAsync(
                "profiles.json",
                cancellationTokenSource.Token));
    }

    private static ErrorProfileCatalogDocument CreateValidDocument()
    {
        return new ErrorProfileCatalogDocument
        {
            SchemaVersion = "1.0",
            CatalogId = "test.profiles",
            CatalogName = "Test Profiles",
            Language = "en",
            Profiles =
            [
                new ErrorProfileDefinition
                {
                    Name = "WEB_API",
                    DisplayName = "Web API",
                    Description = "Profile for web APIs.",
                    IncludeOwners = ["AFW", "APP"],
                    IncludeCodeGroups = ["CONFIGURATION", "VALIDATION"],
                    IncludeCategories = ["WEB", "SERVER", "VALIDATION"],
                    IncludeSubcategories = ["REQUIRED_VALUE"],
                    IncludeTags = ["USER_VISIBLE"],
                    ExcludeTags = ["INTERNAL_ONLY"],
                    DefaultMappings =
                    {
                        ["WEB_PROBLEMDETAILS"] = "true",
                        ["PRODUCTION_INCLUDEEXCEPTIONDETAILS"] = "false"
                    }
                }
            ]
        };
    }

    private sealed class FakeSuccessfulLoader : IErrorProfileCatalogLoader
    {
        private readonly ErrorProfileCatalogDocument? _document;

        public FakeSuccessfulLoader(ErrorProfileCatalogDocument? document)
        {
            _document = document;
        }

        public Task<Response<ErrorProfileCatalogDocument>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(Response<ErrorProfileCatalogDocument>.Ok(_document));
        }
    }

    private sealed class FakeFailedLoader : IErrorProfileCatalogLoader
    {
        public Task<Response<ErrorProfileCatalogDocument>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(Response<ErrorProfileCatalogDocument>.NotFound(
                code: "FileNotFound",
                message: "File was not found."));
        }
    }
}
