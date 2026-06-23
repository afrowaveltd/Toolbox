using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class BuiltInErrorCatalogContextProviderTests
{
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenTemplateProviderIsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => new BuiltInErrorCatalogContextProvider(
                null!,
                new FakeContextProvider()));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenContextProviderIsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => new BuiltInErrorCatalogContextProvider(
                new FakeTemplateProvider(),
                null!));
    }

    [Fact]
    public async Task LoadAsync_ShouldReturnInvalid_WhenTemplatesAreEmpty()
    {
        BuiltInErrorCatalogContextProvider provider = new(
            new FakeTemplateProvider([]),
            new FakeContextProvider());

        Response<ErrorCatalogContext> response =
            await provider.LoadAsync();

        Assert.False(response.IsSuccess);

        Assert.Equal(
            ResultStatus.Invalid,
            response.Status);

        Assert.Equal(
            "WIF_BUILT_IN_TEMPLATES_EMPTY",
            response.Issues[0].Code);
    }

    [Fact]
    public async Task LoadAsync_ShouldReturnInvalid_WhenTemplateFileNameIsMissing()
    {
        BuiltInErrorCatalogContextProvider provider = new(
            new FakeTemplateProvider(
            [
                new JsonsTemplateFile
                {
                    Content = "{}"
                }
            ]),
            new FakeContextProvider());

        Response<ErrorCatalogContext> response =
            await provider.LoadAsync();

        Assert.False(response.IsSuccess);

        Assert.Equal(
            "WIF_BUILT_IN_TEMPLATE_FILE_NAME_MISSING",
            response.Issues[0].Code);
    }

    [Fact]
    public async Task LoadAsync_ShouldReturnInvalid_WhenTemplateContentIsMissing()
    {
        BuiltInErrorCatalogContextProvider provider = new(
            new FakeTemplateProvider(
            [
                new JsonsTemplateFile
                {
                    TargetFileName = "errors.json"
                }
            ]),
            new FakeContextProvider());

        Response<ErrorCatalogContext> response =
            await provider.LoadAsync();

        Assert.False(response.IsSuccess);

        Assert.Equal(
            "WIF_BUILT_IN_TEMPLATE_CONTENT_MISSING",
            response.Issues[0].Code);
    }

    [Fact]
    public async Task LoadAsync_ShouldWriteTemplatesAndDelegateToContextProvider()
    {
        ErrorCatalogContext context = new();

        FakeContextProvider contextProvider = new(
            Response<ErrorCatalogContext>.Ok(
                context));

        BuiltInErrorCatalogContextProvider provider = new(
            new FakeTemplateProvider(
            [
                new JsonsTemplateFile
                {
                    TargetFileName = "errors.json",
                    Content = "{ \"errors\": [] }"
                },
                new JsonsTemplateFile
                {
                    TargetFileName = "profiles.json",
                    Content = "{ \"profiles\": [] }"
                }
            ]),
            contextProvider);

        Response<ErrorCatalogContext> response =
            await provider.LoadAsync();

        Assert.True(response.IsSuccess);

        Assert.Same(
            context,
            response.Data);

        Assert.NotNull(
            contextProvider.LastOptions);

        Assert.Equal(
            "BuiltInDefaults",
            contextProvider
                .LastOptions
                .PackageDirectoryName);

        Assert.True(
            contextProvider.FilesExistedDuringCall);

        Assert.False(
            Directory.Exists(
                contextProvider
                    .LastOptions
                    .RootDirectory));
    }

    [Fact]
    public async Task LoadAsync_ShouldForwardContextProviderFailure()
    {
        BuiltInErrorCatalogContextProvider provider = new(
            new FakeTemplateProvider(
            [
                new JsonsTemplateFile
                {
                    TargetFileName = "errors.json",
                    Content = "{}"
                }
            ]),
            new FakeContextProvider(
                Response<ErrorCatalogContext>.Invalid(
                    code: "BuiltInCatalogInvalid",
                    message: "Built-in catalog is invalid.")));

        Response<ErrorCatalogContext> response =
            await provider.LoadAsync();

        Assert.False(response.IsSuccess);

        Assert.Equal(
            ResultStatus.Invalid,
            response.Status);

        Assert.Equal(
            "BuiltInCatalogInvalid",
            response.Issues[0].Code);
    }

    [Fact]
    public async Task LoadAsync_ShouldThrowOperationCanceledException_WhenCancelled()
    {
        BuiltInErrorCatalogContextProvider provider = new(
            new FakeTemplateProvider(),
            new FakeContextProvider());

        using CancellationTokenSource cancellationTokenSource = new();

        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => provider.LoadAsync(
                cancellationTokenSource.Token));
    }

    [Fact]
    public async Task LoadAsync_ShouldCreateValidContextFromDefaultTemplates()
    {
        ServiceCollection services = new();

        services.AddWhenItFails();

        using ServiceProvider serviceProvider =
            services.BuildServiceProvider(
                new ServiceProviderOptions
                {
                    ValidateOnBuild = true,
                    ValidateScopes = true
                });

        IBuiltInErrorCatalogContextProvider provider =
            serviceProvider.GetRequiredService<
                IBuiltInErrorCatalogContextProvider>();

        Response<ErrorCatalogContext> response =
            await provider.LoadAsync();

        Assert.True(
            response.IsSuccess);

        Assert.NotNull(
            response.Data);

        Assert.NotNull(
            response.Data.ErrorCatalog);

        Assert.NotEmpty(
    response.Data.ErrorCatalog.GetAll());
        Assert.NotNull(
            response.Data.ProfileCatalog);

        Assert.True(
            response.Data.CrossValidationResult.IsValid);
    }

    private sealed class FakeTemplateProvider
        : IJsonsTemplateProvider
    {
        private readonly IReadOnlyList<JsonsTemplateFile>
            _templates;

        public FakeTemplateProvider(
            IReadOnlyList<JsonsTemplateFile>? templates = null)
        {
            _templates = templates
                ??
                [
                    new JsonsTemplateFile
                    {
                        TargetFileName = "errors.json",
                        Content = "{}"
                    }
                ];
        }

        public IReadOnlyList<JsonsTemplateFile> GetTemplateFiles(
            JsonsOptions options)
        {
            return _templates;
        }
    }

    private sealed class FakeContextProvider
        : IErrorCatalogContextProvider
    {
        private readonly Response<ErrorCatalogContext>
            _response;

        public FakeContextProvider(
            Response<ErrorCatalogContext>? response = null)
        {
            _response = response
                ?? Response<ErrorCatalogContext>.Ok(
                    new ErrorCatalogContext());
        }

        public JsonsOptions? LastOptions { get; private set; }

        public bool FilesExistedDuringCall { get; private set; }

        public Task<Response<ErrorCatalogContext>> LoadFromJsonsAsync(
            JsonsOptions options,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            LastOptions = options;

            FilesExistedDuringCall =
                Directory.Exists(
                    options.PackageDirectoryPath)
                && Directory
                    .EnumerateFiles(
                        options.PackageDirectoryPath)
                    .Any();

            return Task.FromResult(
                _response);
        }
    }
}