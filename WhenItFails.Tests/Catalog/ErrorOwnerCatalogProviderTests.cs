using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorOwnerCatalogProviderTests
{
   [Fact]
   public void Constructor_ShouldThrowArgumentNullException_WhenLoaderIsNull()
   {
      Assert.Throws<ArgumentNullException>(
          () => new ErrorOwnerCatalogProvider(
              null!,
              new ErrorOwnerCatalogDocumentNormalizer(),
              new ErrorOwnerCatalogValidator()));
   }

   [Fact]
   public void Constructor_ShouldThrowArgumentNullException_WhenNormalizerIsNull()
   {
      Assert.Throws<ArgumentNullException>(
          () => new ErrorOwnerCatalogProvider(
              new FakeSuccessfulLoader(CreateValidDocument()),
              null!,
              new ErrorOwnerCatalogValidator()));
   }

   [Fact]
   public void Constructor_ShouldThrowArgumentNullException_WhenValidatorIsNull()
   {
      Assert.Throws<ArgumentNullException>(
          () => new ErrorOwnerCatalogProvider(
              new FakeSuccessfulLoader(CreateValidDocument()),
              new ErrorOwnerCatalogDocumentNormalizer(),
              null!));
   }

   [Fact]
   public async Task LoadFromFileAsync_ShouldReturnNotFoundResponse_WhenLoadingReturnsNotFound()
   {
      ErrorOwnerCatalogProvider provider = new(
          new FakeFailedLoader(),
          new ErrorOwnerCatalogDocumentNormalizer(),
          new ErrorOwnerCatalogValidator());

      Response<ErrorOwnerCatalogProviderPayload> response =
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
      ErrorOwnerCatalogProvider provider = new(
          new FakeSuccessfulLoader(null),
          new ErrorOwnerCatalogDocumentNormalizer(),
          new ErrorOwnerCatalogValidator());

      Response<ErrorOwnerCatalogProviderPayload> response =
          await provider.LoadFromFileAsync("owners.json");

      Assert.False(response.IsSuccess);
      Assert.Equal(ResultStatus.Invalid, response.Status);
      Assert.Null(response.Data);
      Assert.NotEmpty(response.Issues);
      Assert.Equal("LoadedOwnerCatalogDocumentIsNull", response.Issues[0].Code);
   }

   [Fact]
   public async Task LoadFromFileAsync_ShouldReturnInvalidResponse_WhenValidationFails()
   {
      ErrorOwnerCatalogDocument document = CreateValidDocument();
      document.Owners[0].Name = string.Empty;

      ErrorOwnerCatalogProvider provider = new(
          new FakeSuccessfulLoader(document),
          new ErrorOwnerCatalogDocumentNormalizer(),
          new ErrorOwnerCatalogValidator());

      Response<ErrorOwnerCatalogProviderPayload> response =
          await provider.LoadFromFileAsync("owners.json");

      Assert.False(response.IsSuccess);
      Assert.Equal(ResultStatus.Invalid, response.Status);
      Assert.Null(response.Data);
      Assert.NotEmpty(response.Issues);
      Assert.Equal("OwnerCatalogValidationFailed", response.Issues[0].Code);
   }

   [Fact]
   public async Task LoadFromFileAsync_ShouldReturnDocument_WhenLoadingAndValidationSucceed()
   {
      ErrorOwnerCatalogProvider provider = new(
          new FakeSuccessfulLoader(CreateValidDocument()),
          new ErrorOwnerCatalogDocumentNormalizer(),
          new ErrorOwnerCatalogValidator());

      Response<ErrorOwnerCatalogProviderPayload> response =
          await provider.LoadFromFileAsync("owners.json");

      Assert.True(response.IsSuccess);
      Assert.Equal(ResultStatus.Success, response.Status);
      Assert.NotNull(response.Data);
      Assert.NotNull(response.Data.Document);
      Assert.NotNull(response.Data.ValidationResult);
      Assert.True(response.Data.ValidationResult.IsValid);

      ErrorOwnerDefinition owner = Assert.Single(response.Data.Document.Owners);

      Assert.Equal("AFW", owner.Name);
      Assert.Equal("Afrowave", owner.DisplayName);
      Assert.Equal(0, owner.CodeFrom);
      Assert.Equal(999999, owner.CodeTo);
      Assert.True(owner.IsBuiltIn);
   }

   [Fact]
   public async Task LoadFromFileAsync_ShouldNormalizeDocumentBeforeReturningPayload()
   {
      ErrorOwnerCatalogDocument document = CreateValidDocument();

      document.Owners[0].Name = " afw ";
      document.Owners[0].DisplayName = " Afrowave ";
      document.Owners[0].Aliases = ["Afro wave", "afro-wave"];

      ErrorOwnerCatalogProvider provider = new(
          new FakeSuccessfulLoader(document),
          new ErrorOwnerCatalogDocumentNormalizer(),
          new ErrorOwnerCatalogValidator());

      Response<ErrorOwnerCatalogProviderPayload> response =
          await provider.LoadFromFileAsync("owners.json");

      Assert.True(response.IsSuccess);
      Assert.Equal(ResultStatus.Success, response.Status);
      Assert.NotNull(response.Data);

      ErrorOwnerDefinition owner = Assert.Single(response.Data.Document.Owners);

      Assert.Equal("AFW", owner.Name);
      Assert.Equal("Afrowave", owner.DisplayName);
      Assert.Equal(["AFRO_WAVE"], owner.Aliases);
   }

   [Fact]
   public async Task LoadFromFileAsync_ShouldThrowOperationCanceledException_WhenCancelled()
   {
      using CancellationTokenSource cancellationTokenSource = new();
      await cancellationTokenSource.CancelAsync();

      ErrorOwnerCatalogProvider provider = new(
          new FakeSuccessfulLoader(CreateValidDocument()),
          new ErrorOwnerCatalogDocumentNormalizer(),
          new ErrorOwnerCatalogValidator());

      await Assert.ThrowsAnyAsync<OperationCanceledException>(
          () => provider.LoadFromFileAsync(
              "owners.json",
              cancellationTokenSource.Token));
   }

   private static ErrorOwnerCatalogDocument CreateValidDocument()
   {
      return new ErrorOwnerCatalogDocument
      {
         SchemaVersion = "1.0",
         CatalogId = "test.owners",
         CatalogName = "Test Owners",
         Language = "en",
         Owners =
          [
              new ErrorOwnerDefinition
                {
                    Name = "AFW",
                    DisplayName = "Afrowave",
                    Description = "Built-in Afrowave owner.",
                    CodeFrom = 0,
                    CodeTo = 999999,
                    IsBuiltIn = true,
                    Aliases = ["AFROWAVE"]
                }
          ]
      };
   }

   private sealed class FakeSuccessfulLoader : IErrorOwnerCatalogLoader
   {
      private readonly ErrorOwnerCatalogDocument? _document;

      public FakeSuccessfulLoader(ErrorOwnerCatalogDocument? document)
      {
         _document = document;
      }

      public Task<Response<ErrorOwnerCatalogDocument>> LoadFromFileAsync(
          string filePath,
          CancellationToken cancellationToken = default)
      {
         cancellationToken.ThrowIfCancellationRequested();

         return Task.FromResult(Response<ErrorOwnerCatalogDocument>.Ok(_document));
      }
   }

   private sealed class FakeFailedLoader : IErrorOwnerCatalogLoader
   {
      public Task<Response<ErrorOwnerCatalogDocument>> LoadFromFileAsync(
          string filePath,
          CancellationToken cancellationToken = default)
      {
         cancellationToken.ThrowIfCancellationRequested();

         return Task.FromResult(Response<ErrorOwnerCatalogDocument>.NotFound(
             code: "FileNotFound",
             message: "File was not found."));
      }
   }
}