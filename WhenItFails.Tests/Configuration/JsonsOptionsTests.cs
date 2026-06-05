using Afrowave.Toolbox.WhenItFails.Configuration;

namespace Afrowave.Toolbox.WhenItFails.Tests.Configuration;

public sealed class JsonsOptionsTests
{
    [Fact]
    public void PackageDirectoryPath_ShouldReturnDefaultPackageDirectory()
    {
        JsonsOptions options = new();

        string expectedPath = Path.Combine(
            "Jsons",
            "WhenItFails");

        Assert.Equal(expectedPath, options.PackageDirectoryPath);
    }

    [Fact]
    public void CatalogFilePaths_ShouldReturnDefaultPaths()
    {
        JsonsOptions options = new();

        Assert.Equal(
            Path.Combine("Jsons", "WhenItFails", "errors.en.json"),
            options.ErrorCatalogFilePath);

        Assert.Equal(
            Path.Combine("Jsons", "WhenItFails", "categories.en.json"),
            options.CategoryCatalogFilePath);

        Assert.Equal(
            Path.Combine("Jsons", "WhenItFails", "code-groups.en.json"),
            options.CodeGroupCatalogFilePath);

        Assert.Equal(
            Path.Combine("Jsons", "WhenItFails", "owners.en.json"),
            options.OwnerCatalogFilePath);

        Assert.Equal(
            Path.Combine("Jsons", "WhenItFails", "profiles.json"),
            options.ProfilesFilePath);
    }

    [Fact]
    public void CatalogFilePaths_ShouldUseCustomRootDirectory()
    {
        JsonsOptions options = new()
        {
            RootDirectory = "CustomJsons"
        };

        Assert.Equal(
            Path.Combine("CustomJsons", "WhenItFails", "errors.en.json"),
            options.ErrorCatalogFilePath);
    }

    [Fact]
    public void CatalogFilePaths_ShouldUseCustomPackageDirectoryName()
    {
        JsonsOptions options = new()
        {
            PackageDirectoryName = "Errors"
        };

        Assert.Equal(
            Path.Combine("Jsons", "Errors", "errors.en.json"),
            options.ErrorCatalogFilePath);
    }

    [Fact]
    public void CatalogFilePaths_ShouldUseCustomFileNames()
    {
        JsonsOptions options = new()
        {
            ErrorCatalogFileName = "custom-errors.json",
            CategoryCatalogFileName = "custom-categories.json",
            CodeGroupCatalogFileName = "custom-code-groups.json",
            OwnerCatalogFileName = "custom-owners.json",
            ProfilesFileName = "custom-profiles.json"
        };

        Assert.Equal(
            Path.Combine("Jsons", "WhenItFails", "custom-errors.json"),
            options.ErrorCatalogFilePath);

        Assert.Equal(
            Path.Combine("Jsons", "WhenItFails", "custom-categories.json"),
            options.CategoryCatalogFilePath);

        Assert.Equal(
            Path.Combine("Jsons", "WhenItFails", "custom-code-groups.json"),
            options.CodeGroupCatalogFilePath);

        Assert.Equal(
            Path.Combine("Jsons", "WhenItFails", "custom-owners.json"),
            options.OwnerCatalogFilePath);

        Assert.Equal(
            Path.Combine("Jsons", "WhenItFails", "custom-profiles.json"),
            options.ProfilesFilePath);
    }
}