using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Configuration;

namespace Afrowave.Toolbox.WhenItFails.Tests.Bootstrap;

public sealed class DefaultJsonsTemplateProviderTests
{
    [Fact]
    public void GetTemplateFiles_ShouldThrowArgumentNullException_WhenOptionsIsNull()
    {
        DefaultJsonsTemplateProvider provider = new();

        Assert.Throws<ArgumentNullException>(
            () => provider.GetTemplateFiles(null!));
    }

    [Fact]
    public void GetTemplateFiles_ShouldReturnDefaultTemplateFiles()
    {
        DefaultJsonsTemplateProvider provider = new();
        JsonsOptions options = new();

        IReadOnlyList<JsonsTemplateFile> templateFiles =
            provider.GetTemplateFiles(options);

        Assert.Equal(5, templateFiles.Count);

        Assert.Contains(templateFiles, file => file.Name == "Error catalog");
        Assert.Contains(templateFiles, file => file.Name == "Category catalog");
        Assert.Contains(templateFiles, file => file.Name == "Code group catalog");
        Assert.Contains(templateFiles, file => file.Name == "Owner catalog");
        Assert.Contains(templateFiles, file => file.Name == "Profiles");
    }

    [Fact]
    public void GetTemplateFiles_ShouldUseFileNamesFromOptions()
    {
        DefaultJsonsTemplateProvider provider = new();

        JsonsOptions options = new()
        {
            ErrorCatalogFileName = "custom-errors.json",
            CategoryCatalogFileName = "custom-categories.json",
            CodeGroupCatalogFileName = "custom-code-groups.json",
            OwnerCatalogFileName = "custom-owners.json",
            ProfilesFileName = "custom-profiles.json"
        };

        IReadOnlyList<JsonsTemplateFile> templateFiles =
            provider.GetTemplateFiles(options);

        Assert.Contains(
            templateFiles,
            file => file.Name == "Error catalog"
                && file.TargetFileName == "custom-errors.json");

        Assert.Contains(
            templateFiles,
            file => file.Name == "Category catalog"
                && file.TargetFileName == "custom-categories.json");

        Assert.Contains(
            templateFiles,
            file => file.Name == "Code group catalog"
                && file.TargetFileName == "custom-code-groups.json");

        Assert.Contains(
            templateFiles,
            file => file.Name == "Owner catalog"
                && file.TargetFileName == "custom-owners.json");

        Assert.Contains(
            templateFiles,
            file => file.Name == "Profiles"
                && file.TargetFileName == "custom-profiles.json");
    }

    [Fact]
    public void GetTemplateFiles_ShouldReturnTemplatesWithNonEmptyContent()
    {
        DefaultJsonsTemplateProvider provider = new();
        JsonsOptions options = new();

        IReadOnlyList<JsonsTemplateFile> templateFiles =
            provider.GetTemplateFiles(options);

        Assert.All(
            templateFiles,
            file =>
            {
                Assert.False(string.IsNullOrWhiteSpace(file.Name));
                Assert.False(string.IsNullOrWhiteSpace(file.TargetFileName));
                Assert.False(string.IsNullOrWhiteSpace(file.Content));
            });
    }

    [Fact]
    public void GetTemplateFiles_ShouldReturnErrorCatalogTemplateWithErrorsSection()
    {
        DefaultJsonsTemplateProvider provider = new();
        JsonsOptions options = new();

        JsonsTemplateFile templateFile =
            provider.GetTemplateFiles(options)
                .Single(file => file.Name == "Error catalog");

        Assert.Contains("\"errors\"", templateFile.Content);
        Assert.Contains("\"AFW-GEN-0001\"", templateFile.Content);
        Assert.Contains("\"AFW-CFG-0001\"", templateFile.Content);
    }

    [Fact]
    public void GetTemplateFiles_ShouldReturnCategoryCatalogTemplateWithCategoriesSection()
    {
        DefaultJsonsTemplateProvider provider = new();
        JsonsOptions options = new();

        JsonsTemplateFile templateFile =
            provider.GetTemplateFiles(options)
                .Single(file => file.Name == "Category catalog");

        Assert.Contains("\"categories\"", templateFile.Content);
        Assert.Contains("\"GENERAL\"", templateFile.Content);
        Assert.Contains("\"CONFIGURATION\"", templateFile.Content);
        Assert.Contains("\"VALIDATION\"", templateFile.Content);
    }

    [Fact]
    public void GetTemplateFiles_ShouldReturnCodeGroupCatalogTemplateWithCodeGroupsSection()
    {
        DefaultJsonsTemplateProvider provider = new();
        JsonsOptions options = new();

        JsonsTemplateFile templateFile =
            provider.GetTemplateFiles(options)
                .Single(file => file.Name == "Code group catalog");

        Assert.Contains("\"codeGroups\"", templateFile.Content);
        Assert.Contains("\"GEN\"", templateFile.Content);
        Assert.Contains("\"CFG\"", templateFile.Content);
        Assert.Contains("\"VAL\"", templateFile.Content);
    }

    [Fact]
    public void GetTemplateFiles_ShouldReturnOwnerCatalogTemplateWithOwnersSection()
    {
        DefaultJsonsTemplateProvider provider = new();
        JsonsOptions options = new();

        JsonsTemplateFile templateFile =
            provider.GetTemplateFiles(options)
                .Single(file => file.Name == "Owner catalog");

        Assert.Contains("\"owners\"", templateFile.Content);
        Assert.Contains("\"AFW\"", templateFile.Content);
        Assert.Contains("\"APP\"", templateFile.Content);
    }

    [Fact]
    public void GetTemplateFiles_ShouldReturnProfilesTemplateWithProfilesSection()
    {
        DefaultJsonsTemplateProvider provider = new();
        JsonsOptions options = new();

        JsonsTemplateFile templateFile =
            provider.GetTemplateFiles(options)
                .Single(file => file.Name == "Profiles");

        Assert.Contains("\"profiles\"", templateFile.Content);
    }
}