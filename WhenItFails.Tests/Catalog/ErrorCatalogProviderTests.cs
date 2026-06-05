using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCatalogProviderTests
{
   [Fact]
   public void Constructor_ShouldThrowArgumentNullException_WhenLoaderIsNull()
   {
      Assert.Throws<ArgumentNullException>(
          () => new ErrorCatalogProvider(
              null!,
              new ErrorCatalogDocumentNormalizer(),
              new ErrorCatalogValidator(),
              new ErrorCatalogFactory()));
   }

   [Fact]
   public void Constructor_ShouldThrowArgumentNullException_WhenNormalizerIsNull()
   {
      Assert.Throws<ArgumentNullException>(
          () => new ErrorCatalogProvider(
              new FakeSuccessfulLoader(CreateValidDocument()),
              null!,
              new ErrorCatalogValidator(),
              new ErrorCatalogFactory()));
   }

   [Fact]
   public void Constructor_ShouldThrowArgumentNullException_WhenValidatorIsNull()
   {
      Assert.Throws<ArgumentNullException>(
          () => new ErrorCatalogProvider(
              new FakeSuccessfulLoader(CreateValidDocument()),
              new ErrorCatalogDocumentNormalizer(),
              null!,
              new ErrorCatalogFactory()));
   }

   [Fact]
   public void Constructor_ShouldThrowArgumentNullException_WhenFactoryIsNull()
   {
      Assert.Throws<ArgumentNullException>(
          () => new ErrorCatalogProvider(
              new FakeSuccessfulLoader(CreateValidDocument()),
              new ErrorCatalogDocumentNormalizer(),
              new ErrorCatalogValidator(),
              null!));
   }

   [Fact]
   public async Task LoadFromFileAsync_ShouldReturnNotFoundResponse_WhenLoadingReturnsNotFound()
   {
      ErrorCatalogProvider provider = new(
          new FakeFailedLoader(),
          new ErrorCatalogDocumentNormalizer(),
          new ErrorCatalogValidator(),
          new ErrorCatalogFactory());

      Response<ErrorCatalogProviderPayload> response =
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
      ErrorCatalogProvider provider = new(
          new FakeSuccessfulLoader(null),
          new ErrorCatalogDocumentNormalizer(),
          new ErrorCatalogValidator(),
          new ErrorCatalogFactory());

      Response<ErrorCatalogProviderPayload> response =
          await provider.LoadFromFileAsync("catalog.json");

      Assert.False(response.IsSuccess);
      Assert.Equal(ResultStatus.Invalid, response.Status);
      Assert.Null(response.Data);
      Assert.NotEmpty(response.Issues);
      Assert.Equal("LoadedCatalogDocumentIsNull", response.Issues[0].Code);
   }

   [Fact]
   public async Task LoadFromFileAsync_ShouldReturnInvalidResponse_WhenValidationFails()
   {
      ErrorCatalogDocument document = CreateValidDocument();
      document.Errors[0].Id = string.Empty;

      ErrorCatalogProvider provider = new(
          new FakeSuccessfulLoader(document),
          new ErrorCatalogDocumentNormalizer(),
          new ErrorCatalogValidator(),
          new ErrorCatalogFactory());

      Response<ErrorCatalogProviderPayload> response =
          await provider.LoadFromFileAsync("catalog.json");

      Assert.False(response.IsSuccess);
      Assert.Equal(ResultStatus.Invalid, response.Status);
      Assert.Null(response.Data);
      Assert.NotEmpty(response.Issues);
      Assert.Equal("CatalogValidationFailed", response.Issues[0].Code);
   }

   [Fact]
   public async Task LoadFromFileAsync_ShouldReturnCatalog_WhenLoadingAndValidationSucceed()
   {
      ErrorCatalogProvider provider = new(
          new FakeSuccessfulLoader(CreateValidDocument()),
          new ErrorCatalogDocumentNormalizer(),
          new ErrorCatalogValidator(),
          new ErrorCatalogFactory());

      Response<ErrorCatalogProviderPayload> response =
          await provider.LoadFromFileAsync("catalog.json");

      Assert.True(response.IsSuccess);
      Assert.Equal(ResultStatus.Success, response.Status);
      Assert.NotNull(response.Data);
      Assert.NotNull(response.Data.Catalog);
      Assert.NotNull(response.Data.ValidationResult);
      Assert.True(response.Data.ValidationResult.IsValid);

      ErrorDefinition? error = response.Data.Catalog.FindById("AFW-CFG-0001");

      Assert.NotNull(error);
      Assert.Equal("AFW_CFG_0001", error.Id);
      Assert.Equal("MISSINGCONFIGURATIONVALUE", error.Name);
   }

   [Fact]
   public async Task LoadFromFileAsync_ShouldNormalizeDocumentBeforeCreatingCatalog()
   {
      ErrorCatalogDocument document = CreateValidDocument();

      document.Errors[0].Id = "afw cfg 0001";
      document.Errors[0].Name = "Missing Configuration Value";
      document.Errors[0].Owner = "afw";
      document.Errors[0].CodePrefix = "cfg";
      document.Errors[0].CodeGroup = "Configuration";
      document.Errors[0].PrimaryCategory = "Configuration";
      document.Errors[0].Categories = ["Configuration", "startup", "startup"];
      document.Errors[0].Subcategories = ["Required Value", "required-value"];
      document.Errors[0].Tags = ["user visible", "user-visible"];

      ErrorCatalogProvider provider = new(
          new FakeSuccessfulLoader(document),
          new ErrorCatalogDocumentNormalizer(),
          new ErrorCatalogValidator(),
          new ErrorCatalogFactory());

      Response<ErrorCatalogProviderPayload> response =
          await provider.LoadFromFileAsync("catalog.json");

      Assert.True(response.IsSuccess);
      Assert.Equal(ResultStatus.Success, response.Status);
      Assert.NotNull(response.Data);
      Assert.NotNull(response.Data.Catalog);

      ErrorDefinition? error = response.Data.Catalog.FindById("AFW-CFG-0001");

      Assert.NotNull(error);
      Assert.Equal("AFW_CFG_0001", error.Id);
      Assert.Equal("MISSING_CONFIGURATION_VALUE", error.Name);
      Assert.Equal("AFW", error.Owner);
      Assert.Equal("CFG", error.CodePrefix);
      Assert.Equal("CONFIGURATION", error.CodeGroup);
      Assert.Equal("CONFIGURATION", error.PrimaryCategory);
      Assert.Equal(["CONFIGURATION", "STARTUP"], error.Categories);
      Assert.Equal(["REQUIRED_VALUE"], error.Subcategories);
      Assert.Equal(["USER_VISIBLE"], error.Tags);
   }

   [Fact]
   public async Task LoadFromFileAsync_ShouldThrowOperationCanceledException_WhenCancelled()
   {
      using CancellationTokenSource cancellationTokenSource = new();
      await cancellationTokenSource.CancelAsync();

      ErrorCatalogProvider provider = new(
          new FakeSuccessfulLoader(CreateValidDocument()),
          new ErrorCatalogDocumentNormalizer(),
          new ErrorCatalogValidator(),
          new ErrorCatalogFactory());

      await Assert.ThrowsAnyAsync<OperationCanceledException>(
          () => provider.LoadFromFileAsync(
              "catalog.json",
              cancellationTokenSource.Token));
   }

   private static ErrorCatalogDocument CreateValidDocument()
   {
      return new ErrorCatalogDocument
      {
         SchemaVersion = "1.0",
         CatalogId = "test.catalog",
         CatalogName = "Test Catalog",
         Language = "en",
         Errors =
          [
              new ErrorDefinition
                {
                    Id = "AFW-CFG-0001",
                    Code = 200001,
                    Name = "MissingConfigurationValue",
                    Owner = "AFW",
                    CodePrefix = "CFG",
                    CodeGroup = "Configuration",
                    PrimaryCategory = "Configuration",
                    Categories = ["Configuration", "Startup", "Validation"],
                    Subcategories = ["RequiredValue", "AppSettings"],
                    Title = "Missing configuration value",
                    Message = "A required configuration value is missing.",
                    DefaultSeverity = "Error",
                    Tags = ["configuration", "startup"]
                }
          ]
      };
   }

   private sealed class FakeSuccessfulLoader : IErrorCatalogLoader
   {
      private readonly ErrorCatalogDocument? _document;

      public FakeSuccessfulLoader(ErrorCatalogDocument? document)
      {
         _document = document;
      }

      public Task<Response<ErrorCatalogDocument>> LoadFromFileAsync(
          string filePath,
          CancellationToken cancellationToken = default)
      {
         cancellationToken.ThrowIfCancellationRequested();

         return Task.FromResult(Response<ErrorCatalogDocument>.Ok(_document));
      }
   }

   private sealed class FakeFailedLoader : IErrorCatalogLoader
   {
      public Task<Response<ErrorCatalogDocument>> LoadFromFileAsync(
          string filePath,
          CancellationToken cancellationToken = default)
      {
         cancellationToken.ThrowIfCancellationRequested();

         return Task.FromResult(Response<ErrorCatalogDocument>.NotFound(
             code: "FileNotFound",
             message: "File was not found."));
      }
   }
}