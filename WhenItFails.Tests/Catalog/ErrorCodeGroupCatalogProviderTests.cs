using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCodeGroupCatalogProviderTests
{
   [Fact]
   public void Constructor_ShouldThrowArgumentNullException_WhenLoaderIsNull()
   {
      Assert.Throws<ArgumentNullException>(
          () => new ErrorCodeGroupCatalogProvider(
              null!,
              new ErrorCodeGroupCatalogDocumentNormalizer(),
              new ErrorCodeGroupCatalogValidator()));
   }

   [Fact]
   public void Constructor_ShouldThrowArgumentNullException_WhenNormalizerIsNull()
   {
      Assert.Throws<ArgumentNullException>(
          () => new ErrorCodeGroupCatalogProvider(
              new FakeSuccessfulLoader(CreateValidDocument()),
              null!,
              new ErrorCodeGroupCatalogValidator()));
   }

   [Fact]
   public void Constructor_ShouldThrowArgumentNullException_WhenValidatorIsNull()
   {
      Assert.Throws<ArgumentNullException>(
          () => new ErrorCodeGroupCatalogProvider(
              new FakeSuccessfulLoader(CreateValidDocument()),
              new ErrorCodeGroupCatalogDocumentNormalizer(),
              null!));
   }

   [Fact]
   public async Task LoadFromFileAsync_ShouldReturnNotFoundResponse_WhenLoadingReturnsNotFound()
   {
      ErrorCodeGroupCatalogProvider provider = new(
          new FakeFailedLoader(),
          new ErrorCodeGroupCatalogDocumentNormalizer(),
          new ErrorCodeGroupCatalogValidator());

      Response<ErrorCodeGroupCatalogProviderPayload> response =
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
      ErrorCodeGroupCatalogProvider provider = new(
          new FakeSuccessfulLoader(null),
          new ErrorCodeGroupCatalogDocumentNormalizer(),
          new ErrorCodeGroupCatalogValidator());

      Response<ErrorCodeGroupCatalogProviderPayload> response =
          await provider.LoadFromFileAsync("code-groups.json");

      Assert.False(response.IsSuccess);
      Assert.Equal(ResultStatus.Invalid, response.Status);
      Assert.Null(response.Data);
      Assert.NotEmpty(response.Issues);
      Assert.Equal("LoadedCodeGroupCatalogDocumentIsNull", response.Issues[0].Code);
   }

   [Fact]
   public async Task LoadFromFileAsync_ShouldReturnInvalidResponse_WhenValidationFails()
   {
      ErrorCodeGroupCatalogDocument document = CreateValidDocument();
      document.CodeGroups[0].Name = string.Empty;

      ErrorCodeGroupCatalogProvider provider = new(
          new FakeSuccessfulLoader(document),
          new ErrorCodeGroupCatalogDocumentNormalizer(),
          new ErrorCodeGroupCatalogValidator());

      Response<ErrorCodeGroupCatalogProviderPayload> response =
          await provider.LoadFromFileAsync("code-groups.json");

      Assert.False(response.IsSuccess);
      Assert.Equal(ResultStatus.Invalid, response.Status);
      Assert.Null(response.Data);
      Assert.NotEmpty(response.Issues);
      Assert.Equal("CodeGroupCatalogValidationFailed", response.Issues[0].Code);
   }

   [Fact]
   public async Task LoadFromFileAsync_ShouldReturnDocument_WhenLoadingAndValidationSucceed()
   {
      ErrorCodeGroupCatalogProvider provider = new(
          new FakeSuccessfulLoader(CreateValidDocument()),
          new ErrorCodeGroupCatalogDocumentNormalizer(),
          new ErrorCodeGroupCatalogValidator());

      Response<ErrorCodeGroupCatalogProviderPayload> response =
          await provider.LoadFromFileAsync("code-groups.json");

      Assert.True(response.IsSuccess);
      Assert.Equal(ResultStatus.Success, response.Status);
      Assert.NotNull(response.Data);
      Assert.NotNull(response.Data.Document);
      Assert.NotNull(response.Data.ValidationResult);
      Assert.True(response.Data.ValidationResult.IsValid);

      ErrorCodeGroupDefinition codeGroup = Assert.Single(response.Data.Document.CodeGroups);

      Assert.Equal("CONFIGURATION", codeGroup.Name);
      Assert.Equal("Configuration", codeGroup.DisplayName);
      Assert.Equal("CFG", codeGroup.CodePrefix);
      Assert.Equal(200000, codeGroup.CodeFrom);
      Assert.Equal(299999, codeGroup.CodeTo);
   }

   [Fact]
   public async Task LoadFromFileAsync_ShouldNormalizeDocumentBeforeReturningPayload()
   {
      ErrorCodeGroupCatalogDocument document = CreateValidDocument();

      document.CodeGroups[0].Name = "configuration errors";
      document.CodeGroups[0].DisplayName = " Configuration errors ";
      document.CodeGroups[0].CodePrefix = " cfg ";
      document.CodeGroups[0].DefaultCategories = ["configuration", "configuration"];
      document.CodeGroups[0].DefaultTags = ["user visible", "user-visible"];

      ErrorCodeGroupCatalogProvider provider = new(
          new FakeSuccessfulLoader(document),
          new ErrorCodeGroupCatalogDocumentNormalizer(),
          new ErrorCodeGroupCatalogValidator());

      Response<ErrorCodeGroupCatalogProviderPayload> response =
          await provider.LoadFromFileAsync("code-groups.json");

      Assert.True(response.IsSuccess);
      Assert.Equal(ResultStatus.Success, response.Status);
      Assert.NotNull(response.Data);

      ErrorCodeGroupDefinition codeGroup = Assert.Single(response.Data.Document.CodeGroups);

      Assert.Equal("CONFIGURATION_ERRORS", codeGroup.Name);
      Assert.Equal("Configuration errors", codeGroup.DisplayName);
      Assert.Equal("CFG", codeGroup.CodePrefix);
      Assert.Equal(["CONFIGURATION"], codeGroup.DefaultCategories);
      Assert.Equal(["USER_VISIBLE"], codeGroup.DefaultTags);
   }

   [Fact]
   public async Task LoadFromFileAsync_ShouldThrowOperationCanceledException_WhenCancelled()
   {
      using CancellationTokenSource cancellationTokenSource = new();
      await cancellationTokenSource.CancelAsync();

      ErrorCodeGroupCatalogProvider provider = new(
          new FakeSuccessfulLoader(CreateValidDocument()),
          new ErrorCodeGroupCatalogDocumentNormalizer(),
          new ErrorCodeGroupCatalogValidator());

      await Assert.ThrowsAnyAsync<OperationCanceledException>(
          () => provider.LoadFromFileAsync(
              "code-groups.json",
              cancellationTokenSource.Token));
   }

   private static ErrorCodeGroupCatalogDocument CreateValidDocument()
   {
      return new ErrorCodeGroupCatalogDocument
      {
         SchemaVersion = "1.0",
         CatalogId = "test.code-groups",
         CatalogName = "Test Code Groups",
         Language = "en",
         CodeGroups =
          [
              new ErrorCodeGroupDefinition
                {
                    Name = "CONFIGURATION",
                    DisplayName = "Configuration",
                    CodePrefix = "CFG",
                    CodeFrom = 200000,
                    CodeTo = 299999,
                    Description = "Errors related to configuration.",
                    DefaultCategories = ["CONFIGURATION"],
                    DefaultTags = ["SETTINGS"]
                }
          ]
      };
   }

   private sealed class FakeSuccessfulLoader : IErrorCodeGroupCatalogLoader
   {
      private readonly ErrorCodeGroupCatalogDocument? _document;

      public FakeSuccessfulLoader(ErrorCodeGroupCatalogDocument? document)
      {
         _document = document;
      }

      public Task<Response<ErrorCodeGroupCatalogDocument>> LoadFromFileAsync(
          string filePath,
          CancellationToken cancellationToken = default)
      {
         cancellationToken.ThrowIfCancellationRequested();

         return Task.FromResult(Response<ErrorCodeGroupCatalogDocument>.Ok(_document));
      }
   }

   private sealed class FakeFailedLoader : IErrorCodeGroupCatalogLoader
   {
      public Task<Response<ErrorCodeGroupCatalogDocument>> LoadFromFileAsync(
          string filePath,
          CancellationToken cancellationToken = default)
      {
         cancellationToken.ThrowIfCancellationRequested();

         return Task.FromResult(Response<ErrorCodeGroupCatalogDocument>.NotFound(
             code: "FileNotFound",
             message: "File was not found."));
      }
   }
}