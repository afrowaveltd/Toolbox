using System.Text.Json;
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

    [Fact]
    public void GetTemplateFiles_ShouldIncludeThermalCatalogAndRevisedOwnerRanges()
    {
        DefaultJsonsTemplateProvider provider = new();
        IReadOnlyList<JsonsTemplateFile> templateFiles =
            provider.GetTemplateFiles(new JsonsOptions());

        using JsonDocument ownerDocument = ParseTemplate(templateFiles, "Owner catalog");
        JsonElement afwOwner = FindByName(
            ownerDocument.RootElement.GetProperty("owners"),
            "AFW");
        JsonElement appOwner = FindByName(
            ownerDocument.RootElement.GetProperty("owners"),
            "APP");

        Assert.Equal(1_099_999, afwOwner.GetProperty("codeTo").GetInt32());
        Assert.Equal(1_100_000, appOwner.GetProperty("codeFrom").GetInt32());

        using JsonDocument categoryDocument = ParseTemplate(templateFiles, "Category catalog");
        JsonElement thermalCategory = FindByName(
            categoryDocument.RootElement.GetProperty("categories"),
            "THERMAL");
        Assert.Equal("Thermal", thermalCategory.GetProperty("displayName").GetString());

        using JsonDocument codeGroupDocument = ParseTemplate(templateFiles, "Code group catalog");
        JsonElement thermalCodeGroup = FindByName(
            codeGroupDocument.RootElement.GetProperty("codeGroups"),
            "THERMAL");
        Assert.Equal("THM", thermalCodeGroup.GetProperty("codePrefix").GetString());
        Assert.Equal(1_000_000, thermalCodeGroup.GetProperty("codeFrom").GetInt32());
        Assert.Equal(1_099_999, thermalCodeGroup.GetProperty("codeTo").GetInt32());

        using JsonDocument errorDocument = ParseTemplate(templateFiles, "Error catalog");
        JsonElement thermalError = FindByName(
            errorDocument.RootElement.GetProperty("errors"),
            "TemperatureLimitExceeded");
        Assert.Equal("AFW-THM-0001", thermalError.GetProperty("id").GetString());
        Assert.Equal(1_000_001, thermalError.GetProperty("code").GetInt32());
    }

    private static JsonDocument ParseTemplate(
        IReadOnlyList<JsonsTemplateFile> templateFiles,
        string name)
    {
        string content = templateFiles.Single(file => file.Name == name).Content;
        return JsonDocument.Parse(content);
    }

    private static JsonElement FindByName(JsonElement items, string name)
    {
        foreach (JsonElement item in items.EnumerateArray())
        {
            if (string.Equals(
                item.GetProperty("name").GetString(),
                name,
                StringComparison.Ordinal))
            {
                return item;
            }
        }

        throw new Xunit.Sdk.XunitException($"Template item '{name}' was not found.");
    }
}