using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Initialization;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Initialization;

public sealed class ErrorCatalogInitializerTests
{
   [Fact]
   public void Constructor_ShouldThrowArgumentNullException_WhenBootstrapperIsNull()
   {
      Assert.Throws<ArgumentNullException>(
          () => new ErrorCatalogInitializer(
              null!,
              new FakeContextProvider(),
              new FakeContextStore()));
   }

   [Fact]
   public void Constructor_ShouldThrowArgumentNullException_WhenContextProviderIsNull()
   {
      Assert.Throws<ArgumentNullException>(
          () => new ErrorCatalogInitializer(
              new FakeBootstrapper(),
              null!,
              new FakeContextStore()));
   }

   [Fact]
   public void Constructor_ShouldThrowArgumentNullException_WhenContextStoreIsNull()
   {
      Assert.Throws<ArgumentNullException>(
          () => new ErrorCatalogInitializer(
              new FakeBootstrapper(),
              new FakeContextProvider(),
              null!));
   }

   [Fact]
   public async Task InitializeAsync_ShouldThrowArgumentNullException_WhenOptionsIsNull()
   {
      ErrorCatalogInitializer initializer = CreateInitializer();

      await Assert.ThrowsAsync<ArgumentNullException>(
          () => initializer.InitializeAsync(null!));
   }

   [Fact]
   public async Task InitializeAsync_ShouldReturnBootstrapFailure_WithoutLoadingContext()
   {
      FakeBootstrapper bootstrapper = new(
          Response<JsonsBootstrapPayload>.NotFound(
              code: "BootstrapSourceMissing",
              message: "Bootstrap source is missing."));

      FakeContextProvider contextProvider = new();
      FakeContextStore contextStore = new();

      ErrorCatalogInitializer initializer = new(
          bootstrapper,
          contextProvider,
          contextStore);

      Response<ErrorCatalogInitializationPayload> response =
          await initializer.InitializeAsync(new JsonsOptions());

      Assert.False(response.IsSuccess);
      Assert.Equal(ResultStatus.NotFound, response.Status);
      Assert.Equal("BootstrapSourceMissing", response.Issues[0].Code);

      Assert.False(contextProvider.WasCalled);
      Assert.Null(contextStore.StoredContext);
   }

   [Fact]
   public async Task InitializeAsync_ShouldReturnInvalid_WhenBootstrapPayloadIsNull()
   {
      FakeBootstrapper bootstrapper = new(
          Response<JsonsBootstrapPayload>.Ok(null));

      FakeContextProvider contextProvider = new();
      FakeContextStore contextStore = new();

      ErrorCatalogInitializer initializer = new(
          bootstrapper,
          contextProvider,
          contextStore);

      Response<ErrorCatalogInitializationPayload> response =
          await initializer.InitializeAsync(new JsonsOptions());

      Assert.False(response.IsSuccess);
      Assert.Equal(ResultStatus.Invalid, response.Status);
      Assert.Equal(
          "ErrorCatalogBootstrapPayloadIsNull",
          response.Issues[0].Code);

      Assert.False(contextProvider.WasCalled);
      Assert.Null(contextStore.StoredContext);
   }

   [Fact]
   public async Task InitializeAsync_ShouldReturnContextFailure_WithoutUpdatingStore()
   {
      FakeContextProvider contextProvider = new(
          Response<ErrorCatalogContext>.Invalid(
              code: "CatalogDocumentsInvalid",
              message: "Catalog documents are invalid."));

      FakeContextStore contextStore = new();

      ErrorCatalogInitializer initializer = new(
          new FakeBootstrapper(),
          contextProvider,
          contextStore);

      Response<ErrorCatalogInitializationPayload> response =
          await initializer.InitializeAsync(new JsonsOptions());

      Assert.False(response.IsSuccess);
      Assert.Equal(ResultStatus.Invalid, response.Status);
      Assert.Equal("CatalogDocumentsInvalid", response.Issues[0].Code);

      Assert.True(contextProvider.WasCalled);
      Assert.Null(contextStore.StoredContext);
   }

   [Fact]
   public async Task InitializeAsync_ShouldReturnInvalid_WhenContextPayloadIsNull()
   {
      FakeContextProvider contextProvider = new(
          Response<ErrorCatalogContext>.Ok(null));

      FakeContextStore contextStore = new();

      ErrorCatalogInitializer initializer = new(
          new FakeBootstrapper(),
          contextProvider,
          contextStore);

      Response<ErrorCatalogInitializationPayload> response =
          await initializer.InitializeAsync(new JsonsOptions());

      Assert.False(response.IsSuccess);
      Assert.Equal(ResultStatus.Invalid, response.Status);
      Assert.Equal(
          "ErrorCatalogContextPayloadIsNull",
          response.Issues[0].Code);

      Assert.Null(contextStore.StoredContext);
   }

   [Fact]
   public async Task InitializeAsync_ShouldStoreAndReturnContext_WhenInitializationSucceeds()
   {
      JsonsBootstrapPayload bootstrapPayload = new();
      ErrorCatalogContext context = new();

      FakeBootstrapper bootstrapper = new(
          Response<JsonsBootstrapPayload>.Ok(bootstrapPayload));

      FakeContextProvider contextProvider = new(
          Response<ErrorCatalogContext>.Ok(context));

      FakeContextStore contextStore = new();

      ErrorCatalogInitializer initializer = new(
          bootstrapper,
          contextProvider,
          contextStore);

      Response<ErrorCatalogInitializationPayload> response =
          await initializer.InitializeAsync(new JsonsOptions());

      Assert.True(response.IsSuccess);
      Assert.Equal(ResultStatus.Success, response.Status);
      Assert.NotNull(response.Data);

      Assert.Same(bootstrapPayload, response.Data.Bootstrap);
      Assert.Same(context, response.Data.Context);
      Assert.Same(context, contextStore.StoredContext);
   }

   [Fact]
   public async Task InitializeAsync_ShouldPassOptionsToBothDependencies()
   {
      JsonsOptions options = new()
      {
         RootDirectory = "CustomJsons",
         PackageDirectoryName = "CustomWhenItFails"
      };

      FakeBootstrapper bootstrapper = new();
      FakeContextProvider contextProvider = new();

      ErrorCatalogInitializer initializer = new(
          bootstrapper,
          contextProvider,
          new FakeContextStore());

      await initializer.InitializeAsync(options);

      Assert.Same(options, bootstrapper.LastOptions);
      Assert.Same(options, contextProvider.LastOptions);
   }

   [Fact]
   public async Task InitializeAsync_ShouldThrowOperationCanceledException_WhenCancelled()
   {
      ErrorCatalogInitializer initializer = CreateInitializer();

      using CancellationTokenSource cancellationTokenSource = new();
      await cancellationTokenSource.CancelAsync();

      await Assert.ThrowsAnyAsync<OperationCanceledException>(
          () => initializer.InitializeAsync(
              new JsonsOptions(),
              cancellationTokenSource.Token));
   }

   private static ErrorCatalogInitializer CreateInitializer()
   {
      return new ErrorCatalogInitializer(
          new FakeBootstrapper(),
          new FakeContextProvider(),
          new FakeContextStore());
   }

   private sealed class FakeBootstrapper : IJsonsBootstrapper
   {
      private readonly Response<JsonsBootstrapPayload> _response;

      public FakeBootstrapper(
          Response<JsonsBootstrapPayload>? response = null)
      {
         _response = response
             ?? Response<JsonsBootstrapPayload>.Ok(
                 new JsonsBootstrapPayload());
      }

      public JsonsOptions? LastOptions { get; private set; }

      public Task<Response<JsonsBootstrapPayload>> EnsureWorkspaceAsync(
          JsonsOptions options,
          CancellationToken cancellationToken = default)
      {
         cancellationToken.ThrowIfCancellationRequested();

         LastOptions = options;

         return Task.FromResult(_response);
      }
   }

   private sealed class FakeContextProvider
       : IErrorCatalogContextProvider
   {
      private readonly Response<ErrorCatalogContext> _response;

      public FakeContextProvider(
          Response<ErrorCatalogContext>? response = null)
      {
         _response = response
             ?? Response<ErrorCatalogContext>.Ok(
                 new ErrorCatalogContext());
      }

      public bool WasCalled { get; private set; }

      public JsonsOptions? LastOptions { get; private set; }

      public Task<Response<ErrorCatalogContext>> LoadFromJsonsAsync(
          JsonsOptions options,
          CancellationToken cancellationToken = default)
      {
         cancellationToken.ThrowIfCancellationRequested();

         WasCalled = true;
         LastOptions = options;

         return Task.FromResult(_response);
      }
   }

   private sealed class FakeContextStore : IErrorCatalogContextStore
   {
      public bool IsInitialized =>
          StoredContext is not null;

      public ErrorCatalogContext? Current =>
          StoredContext;

      public ErrorCatalogContext? StoredContext { get; private set; }

      public Response<ErrorCatalogContext> GetCurrent()
      {
         return StoredContext is null
             ? Response<ErrorCatalogContext>.Invalid(
                 code: "ErrorCatalogContextNotInitialized",
                 message: "Error catalog context has not been initialized.")
             : Response<ErrorCatalogContext>.Ok(StoredContext);
      }

      public void Set(ErrorCatalogContext context)
      {
         ArgumentNullException.ThrowIfNull(context);

         StoredContext = context;
      }
   }
}