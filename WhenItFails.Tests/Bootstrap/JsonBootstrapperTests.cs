using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Bootstrap;

public sealed class JsonsBootstrapperTests
{
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenTemplateProviderIsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => new JsonsBootstrapper(null!));
    }

    [Fact]
    public async Task EnsureWorkspaceAsync_ShouldThrowArgumentNullException_WhenOptionsIsNull()
    {
        JsonsBootstrapper bootstrapper = new(
            new FakeJsonsTemplateProvider());

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => bootstrapper.EnsureWorkspaceAsync(null!));
    }

    [Fact]
    public async Task EnsureWorkspaceAsync_ShouldCreatePackageDirectory_WhenItDoesNotExist()
    {
        JsonsBootstrapper bootstrapper = new(
            new FakeJsonsTemplateProvider());

        string rootDirectory = CreateTemporaryRootDirectory();
        string packageDirectoryName = "WhenItFails";

        try
        {
            JsonsOptions options = new()
            {
                RootDirectory = rootDirectory,
                PackageDirectoryName = packageDirectoryName
            };

            string expectedPackageDirectoryPath =
                Path.Combine(rootDirectory, packageDirectoryName);

            Response<JsonsBootstrapPayload> response =
                await bootstrapper.EnsureWorkspaceAsync(options);

            Assert.True(response.IsSuccess);
            Assert.Equal(ResultStatus.Success, response.Status);
            Assert.NotNull(response.Data);

            Assert.True(Directory.Exists(expectedPackageDirectoryPath));
            Assert.Equal(rootDirectory, response.Data.RootDirectory);
            Assert.Equal(expectedPackageDirectoryPath, response.Data.PackageDirectoryPath);
            Assert.False(response.Data.PackageDirectoryAlreadyExisted);
            Assert.True(response.Data.PackageDirectoryCreated);
            Assert.Empty(response.Data.Files);
        }
        finally
        {
            DeleteDirectoryIfExists(rootDirectory);
        }
    }

    [Fact]
    public async Task EnsureWorkspaceAsync_ShouldNotRecreatePackageDirectory_WhenItAlreadyExists()
    {
        JsonsBootstrapper bootstrapper = new(
            new FakeJsonsTemplateProvider());

        string rootDirectory = CreateTemporaryRootDirectory();
        string packageDirectoryName = "WhenItFails";
        string packageDirectoryPath = Path.Combine(rootDirectory, packageDirectoryName);

        try
        {
            Directory.CreateDirectory(packageDirectoryPath);

            JsonsOptions options = new()
            {
                RootDirectory = rootDirectory,
                PackageDirectoryName = packageDirectoryName
            };

            Response<JsonsBootstrapPayload> response =
                await bootstrapper.EnsureWorkspaceAsync(options);

            Assert.True(response.IsSuccess);
            Assert.Equal(ResultStatus.Success, response.Status);
            Assert.NotNull(response.Data);

            Assert.True(Directory.Exists(packageDirectoryPath));
            Assert.True(response.Data.PackageDirectoryAlreadyExisted);
            Assert.False(response.Data.PackageDirectoryCreated);
            Assert.Equal(packageDirectoryPath, response.Data.PackageDirectoryPath);
            Assert.Empty(response.Data.Files);
        }
        finally
        {
            DeleteDirectoryIfExists(rootDirectory);
        }
    }

    [Fact]
    public async Task EnsureWorkspaceAsync_ShouldTrimConfiguredPaths()
    {
        JsonsBootstrapper bootstrapper = new(
            new FakeJsonsTemplateProvider());

        string rootDirectory = CreateTemporaryRootDirectory();
        string packageDirectoryName = "WhenItFails";

        try
        {
            JsonsOptions options = new()
            {
                RootDirectory = $" {rootDirectory} ",
                PackageDirectoryName = $" {packageDirectoryName} "
            };

            string expectedPackageDirectoryPath =
                Path.Combine(rootDirectory, packageDirectoryName);

            Response<JsonsBootstrapPayload> response =
                await bootstrapper.EnsureWorkspaceAsync(options);

            Assert.True(response.IsSuccess);
            Assert.NotNull(response.Data);

            Assert.Equal(rootDirectory, response.Data.RootDirectory);
            Assert.Equal(expectedPackageDirectoryPath, response.Data.PackageDirectoryPath);
            Assert.True(Directory.Exists(expectedPackageDirectoryPath));
        }
        finally
        {
            DeleteDirectoryIfExists(rootDirectory);
        }
    }

    [Fact]
    public async Task EnsureWorkspaceAsync_ShouldCreateTemplateFiles_WhenTheyDoNotExist()
    {
        IReadOnlyList<JsonsTemplateFile> templateFiles =
        [
            new JsonsTemplateFile
            {
                Name = "Error catalog",
                TargetFileName = "errors.en.json",
                Content = "{ \"catalogId\": \"errors\" }"
            },
            new JsonsTemplateFile
            {
                Name = "Category catalog",
                TargetFileName = "categories.en.json",
                Content = "{ \"catalogId\": \"categories\" }"
            }
        ];

        JsonsBootstrapper bootstrapper = new(
            new FakeJsonsTemplateProvider(templateFiles));

        string rootDirectory = CreateTemporaryRootDirectory();

        try
        {
            JsonsOptions options = new()
            {
                RootDirectory = rootDirectory,
                PackageDirectoryName = "WhenItFails"
            };

            string packageDirectoryPath =
                Path.Combine(rootDirectory, "WhenItFails");

            string errorCatalogPath =
                Path.Combine(packageDirectoryPath, "errors.en.json");

            string categoryCatalogPath =
                Path.Combine(packageDirectoryPath, "categories.en.json");

            Response<JsonsBootstrapPayload> response =
                await bootstrapper.EnsureWorkspaceAsync(options);

            Assert.True(response.IsSuccess);
            Assert.NotNull(response.Data);

            Assert.True(File.Exists(errorCatalogPath));
            Assert.True(File.Exists(categoryCatalogPath));

            Assert.Equal("{ \"catalogId\": \"errors\" }", File.ReadAllText(errorCatalogPath));
            Assert.Equal("{ \"catalogId\": \"categories\" }", File.ReadAllText(categoryCatalogPath));

            Assert.Equal(2, response.Data.Files.Count);

            JsonsBootstrapFileResult errorFileResult =
                Assert.Single(response.Data.Files, file => file.Name == "Error catalog");

            Assert.Equal(errorCatalogPath, errorFileResult.TargetFilePath);
            Assert.False(errorFileResult.AlreadyExisted);
            Assert.True(errorFileResult.Created);
            Assert.False(errorFileResult.Skipped);

            JsonsBootstrapFileResult categoryFileResult =
                Assert.Single(response.Data.Files, file => file.Name == "Category catalog");

            Assert.Equal(categoryCatalogPath, categoryFileResult.TargetFilePath);
            Assert.False(categoryFileResult.AlreadyExisted);
            Assert.True(categoryFileResult.Created);
            Assert.False(categoryFileResult.Skipped);
        }
        finally
        {
            DeleteDirectoryIfExists(rootDirectory);
        }
    }

    [Fact]
    public async Task EnsureWorkspaceAsync_ShouldSkipTemplateFile_WhenItAlreadyExists()
    {
        IReadOnlyList<JsonsTemplateFile> templateFiles =
        [
            new JsonsTemplateFile
            {
                Name = "Error catalog",
                TargetFileName = "errors.en.json",
                Content = "{ \"catalogId\": \"new-template\" }"
            }
        ];

        JsonsBootstrapper bootstrapper = new(
            new FakeJsonsTemplateProvider(templateFiles));

        string rootDirectory = CreateTemporaryRootDirectory();
        string packageDirectoryPath = Path.Combine(rootDirectory, "WhenItFails");
        string errorCatalogPath = Path.Combine(packageDirectoryPath, "errors.en.json");

        try
        {
            Directory.CreateDirectory(packageDirectoryPath);
            File.WriteAllText(errorCatalogPath, "{ \"catalogId\": \"existing\" }");

            JsonsOptions options = new()
            {
                RootDirectory = rootDirectory,
                PackageDirectoryName = "WhenItFails"
            };

            Response<JsonsBootstrapPayload> response =
                await bootstrapper.EnsureWorkspaceAsync(options);

            Assert.True(response.IsSuccess);
            Assert.NotNull(response.Data);

            Assert.True(File.Exists(errorCatalogPath));
            Assert.Equal("{ \"catalogId\": \"existing\" }", File.ReadAllText(errorCatalogPath));

            JsonsBootstrapFileResult fileResult =
                Assert.Single(response.Data.Files);

            Assert.Equal("Error catalog", fileResult.Name);
            Assert.Equal(errorCatalogPath, fileResult.TargetFilePath);
            Assert.True(fileResult.AlreadyExisted);
            Assert.False(fileResult.Created);
            Assert.True(fileResult.Skipped);
        }
        finally
        {
            DeleteDirectoryIfExists(rootDirectory);
        }
    }

    [Fact]
    public async Task EnsureWorkspaceAsync_ShouldTrimTemplateTargetFileName()
    {
        IReadOnlyList<JsonsTemplateFile> templateFiles =
        [
            new JsonsTemplateFile
            {
                Name = "Error catalog",
                TargetFileName = " errors.en.json ",
                Content = "{ \"catalogId\": \"errors\" }"
            }
        ];

        JsonsBootstrapper bootstrapper = new(
            new FakeJsonsTemplateProvider(templateFiles));

        string rootDirectory = CreateTemporaryRootDirectory();

        try
        {
            JsonsOptions options = new()
            {
                RootDirectory = rootDirectory,
                PackageDirectoryName = "WhenItFails"
            };

            string expectedFilePath =
                Path.Combine(rootDirectory, "WhenItFails", "errors.en.json");

            Response<JsonsBootstrapPayload> response =
                await bootstrapper.EnsureWorkspaceAsync(options);

            Assert.True(response.IsSuccess);
            Assert.NotNull(response.Data);

            Assert.True(File.Exists(expectedFilePath));

            JsonsBootstrapFileResult fileResult =
                Assert.Single(response.Data.Files);

            Assert.Equal(expectedFilePath, fileResult.TargetFilePath);
        }
        finally
        {
            DeleteDirectoryIfExists(rootDirectory);
        }
    }

    [Fact]
    public async Task EnsureWorkspaceAsync_ShouldThrowOperationCanceledException_WhenCancelled()
    {
        JsonsBootstrapper bootstrapper = new(
            new FakeJsonsTemplateProvider());

        using CancellationTokenSource cancellationTokenSource = new();
        await cancellationTokenSource.CancelAsync();

        JsonsOptions options = new();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => bootstrapper.EnsureWorkspaceAsync(
                options,
                cancellationTokenSource.Token));
    }

    private static string CreateTemporaryRootDirectory()
    {
        string rootDirectory = Path.Combine(
            Path.GetTempPath(),
            $"when-it-fails-bootstrap-test-{Guid.NewGuid():N}");

        return rootDirectory;
    }

    private static void DeleteDirectoryIfExists(string directoryPath)
    {
        if (Directory.Exists(directoryPath))
        {
            Directory.Delete(directoryPath, recursive: true);
        }
    }

    private sealed class FakeJsonsTemplateProvider : IJsonsTemplateProvider
    {
        private readonly IReadOnlyList<JsonsTemplateFile> _templateFiles;

        public FakeJsonsTemplateProvider(
            IReadOnlyList<JsonsTemplateFile>? templateFiles = null)
        {
            _templateFiles = templateFiles ?? [];
        }

        public IReadOnlyList<JsonsTemplateFile> GetTemplateFiles(JsonsOptions options)
        {
            return _templateFiles;
        }
    }
}