using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCategoryCatalogProviderTests
{
   [Fact]
   public void Constructor_ShouldThrowArgumentNullException_WhenLoaderIsNull()
   {
      Assert.Throws<ArgumentNullException>(
          () => new ErrorCategoryCatalogProvider(
              null!,
              new ErrorCategoryCatalogDocumentNormalizer(),
              new ErrorCategoryCatalogValidator()));
   }

   [Fact]
   public void Constructor_ShouldThrowArgumentNullException_WhenNormalizerIsNull()
   {
      Assert.Throws<ArgumentNullException>(
          () => new ErrorCategoryCatalogProvider(
              new FakeSuccessfulLoader(CreateValidDocument()),
              null!,
              new ErrorCategoryCatalogValidator()));
   }

   [Fact]
   public void Constructor_ShouldThrowArgumentNullException_WhenValidatorIsNull()
   {
      Assert.Throws<ArgumentNullException>(
          () => new ErrorCategoryCatalogProvider(
              new FakeSuccessfulLoader(CreateValidDocument()),
              new ErrorCategoryCatalogDocumentNormalizer(),
              null!));
   }

   [Fact]
   public async Task LoadFromFileAsync_ShouldReturnNotFoundResponse_WhenLoadingReturnsNotFound()
   {
      ErrorCategoryCatalogProvider provider = new(
          new FakeFailedLoader(),
          new ErrorCategoryCatalogDocumentNormalizer(),
          new ErrorCategoryCatalogValidator());

      Response<ErrorCategoryCatalogProviderPayload> response =
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
      ErrorCategoryCatalogProvider provider = new(
          new FakeSuccessfulLoader(null),
          new ErrorCategoryCatalogDocumentNormalizer(),
          new ErrorCategoryCatalogValidator());

      Response<ErrorCategoryCatalogProviderPayload> response =
          await provider.LoadFromFileAsync("categories.json");

      Assert.False(response.IsSuccess);
      Assert.Equal(ResultStatus.Invalid, response.Status);
      Assert.Null(response.Data);
      Assert.NotEmpty(response.Issues);
      Assert.Equal("LoadedCategoryCatalogDocumentIsNull", response.Issues[0].Code);
   }

   [Fact]
   public async Task LoadFromFileAsync_ShouldReturnInvalidResponse_WhenValidationFails()
   {
      ErrorCategoryCatalogDocument document = CreateValidDocument();
      document.Categories[0].Name = string.Empty;

      ErrorCategoryCatalogProvider provider = new(
          new FakeSuccessfulLoader(document),
          new ErrorCategoryCatalogDocumentNormalizer(),
          new ErrorCategoryCatalogValidator());

      Response<ErrorCategoryCatalogProviderPayload> response =
          await provider.LoadFromFileAsync("categories.json");

      Assert.False(response.IsSuccess);
      Assert.Equal(ResultStatus.Invalid, response.Status);
      Assert.Null(response.Data);
      Assert.NotEmpty(response.Issues);
      Assert.Equal("CategoryCatalogValidationFailed", response.Issues[0].Code);
   }

   [Fact]
   public async Task LoadFromFileAsync_ShouldReturnDocument_WhenLoadingAndValidationSucceed()
   {
      ErrorCategoryCatalogProvider provider = new(
          new FakeSuccessfulLoader(CreateValidDocument()),
          new ErrorCategoryCatalogDocumentNormalizer(),
          new ErrorCategoryCatalogValidator());

      Response<ErrorCategoryCatalogProviderPayload> response =
          await provider.LoadFromFileAsync("categories.json");

      Assert.True(response.IsSuccess);
      Assert.Equal(ResultStatus.Success, response.Status);
      Assert.NotNull(response.Data);
      Assert.NotNull(response.Data.Document);
      Assert.NotNull(response.Data.ValidationResult);
      Assert.True(response.Data.ValidationResult.IsValid);

      ErrorCategoryDefinition category = Assert.Single(response.Data.Document.Categories);

      Assert.Equal("NETWORK", category.Name);
      Assert.Equal("Network", category.DisplayName);
   }

   [Fact]
   public async Task LoadFromFileAsync_ShouldNormalizeDocumentBeforeReturningPayload()
   {
      ErrorCategoryCatalogDocument document = CreateValidDocument();

      document.Categories[0].Name = "network error";
      document.Categories[0].DisplayName = " Network error ";
      document.Categories[0].Aliases = ["networking", "networking"];
      document.Categories[0].DefaultTags = ["retryable candidate", "retryable-candidate"];

      ErrorCategoryCatalogProvider provider = new(
          new FakeSuccessfulLoader(document),
          new ErrorCategoryCatalogDocumentNormalizer(),
          new ErrorCategoryCatalogValidator());

      Response<ErrorCategoryCatalogProviderPayload> response =
          await provider.LoadFromFileAsync("categories.json");

      Assert.True(response.IsSuccess);
      Assert.NotNull(response.Data);

      ErrorCategoryDefinition category = Assert.Single(response.Data.Document.Categories);

      Assert.Equal("NETWORK_ERROR", category.Name);
      Assert.Equal("Network error", category.DisplayName);
      Assert.Equal(["NETWORKING"], category.Aliases);
      Assert.Equal(["RETRYABLE_CANDIDATE"], category.DefaultTags);
   }

   [Fact]
   public async Task LoadFromFileAsync_ShouldThrowOperationCanceledException_WhenCancelled()
   {
      using CancellationTokenSource cancellationTokenSource = new();
      await cancellationTokenSource.CancelAsync();

      ErrorCategoryCatalogProvider provider = new(
          new FakeSuccessfulLoader(CreateValidDocument()),
          new ErrorCategoryCatalogDocumentNormalizer(),
          new ErrorCategoryCatalogValidator());

      await Assert.ThrowsAnyAsync<OperationCanceledException>(
          () => provider.LoadFromFileAsync(
              "categories.json",
              cancellationTokenSource.Token));
   }

   private static ErrorCategoryCatalogDocument CreateValidDocument()
   {
      return new ErrorCategoryCatalogDocument
      {
         SchemaVersion = "1.0",
         CatalogId = "test.categories",
         CatalogName = "Test Categories",
         Language = "en",
         Categories =
          [
              new ErrorCategoryDefinition
                {
                    Name = "NETWORK",
                    DisplayName = "Network",
                    Description = "Errors related to network communication.",
                    Aliases = ["NETWORKING"],
                    ParentCategories = ["EXTERNAL_COMMUNICATION"],
                    DefaultTags = ["RETRYABLE_CANDIDATE"]
                }
          ]
      };
   }

   private sealed class FakeSuccessfulLoader : IErrorCategoryCatalogLoader
   {
      private readonly ErrorCategoryCatalogDocument? _document;

      public FakeSuccessfulLoader(ErrorCategoryCatalogDocument? document)
      {
         _document = document;
      }

      public Task<Response<ErrorCategoryCatalogDocument>> LoadFromFileAsync(
          string filePath,
          CancellationToken cancellationToken = default)
      {
         cancellationToken.ThrowIfCancellationRequested();

         return Task.FromResult(Response<ErrorCategoryCatalogDocument>.Ok(_document));
      }
   }

   private sealed class FakeFailedLoader : IErrorCategoryCatalogLoader
   {
      public Task<Response<ErrorCategoryCatalogDocument>> LoadFromFileAsync(
          string filePath,
          CancellationToken cancellationToken = default)
      {
         cancellationToken.ThrowIfCancellationRequested();

         return Task.FromResult(Response<ErrorCategoryCatalogDocument>.NotFound(
             code: "FileNotFound",
             message: "File was not found."));
      }
   }
}
