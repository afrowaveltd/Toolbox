using System.Text.Json;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class TemperatureLimitExceededCatalogTests
{
    [Fact]
    public async Task DefaultCatalogs_ShouldDefineTemperatureLimitExceededContract()
    {
        string repositoryRoot = FindRepositoryRoot();
        string jsonsDirectory = Path.Combine(repositoryRoot, "Jsons", "WhenItFails");

        using JsonDocument errorsDocument = await LoadJsonAsync(
            Path.Combine(jsonsDirectory, "errors.en.json"));
        using JsonDocument categoriesDocument = await LoadJsonAsync(
            Path.Combine(jsonsDirectory, "categories.en.json"));
        using JsonDocument codeGroupsDocument = await LoadJsonAsync(
            Path.Combine(jsonsDirectory, "code-groups.en.json"));

        JsonElement thermalCategory = FindByName(
            categoriesDocument.RootElement.GetProperty("categories"),
            "THERMAL");
        Assert.Equal("Thermal", thermalCategory.GetProperty("displayName").GetString());

        JsonElement thermalCodeGroup = FindByName(
            codeGroupsDocument.RootElement.GetProperty("codeGroups"),
            "THERMAL");
        Assert.Equal("THM", thermalCodeGroup.GetProperty("codePrefix").GetString());
        Assert.Equal(1_000_000, thermalCodeGroup.GetProperty("codeFrom").GetInt32());
        Assert.Equal(1_099_999, thermalCodeGroup.GetProperty("codeTo").GetInt32());

        JsonElement error = FindByName(
            errorsDocument.RootElement.GetProperty("errors"),
            "TEMPERATURELIMITEXCEEDED");

        Assert.Equal("AFW_THM_0001", error.GetProperty("id").GetString());
        Assert.Equal(1_000_001, error.GetProperty("code").GetInt32());
        Assert.Equal("AFW", error.GetProperty("owner").GetString());
        Assert.Equal("THM", error.GetProperty("codePrefix").GetString());
        Assert.Equal("THERMAL", error.GetProperty("codeGroup").GetString());
        Assert.Equal("THERMAL", error.GetProperty("primaryCategory").GetString());
        Assert.Equal("Temperature limit exceeded", error.GetProperty("title").GetString());
        Assert.Equal(
            "The reported temperature {temperature}{unit} exceeds the configured safe limit of {limit}{unit}.",
            error.GetProperty("message").GetString());
        Assert.Equal("Warning", error.GetProperty("defaultSeverity").GetString());
        Assert.Equal(
            "Verify the sensor reading, unit conversion, cooling path, workload, configured limits, and shutdown policy.",
            error.GetProperty("developerHint").GetString());
        Assert.Equal(
            "when-it-fails/errors/thermal/temperature-limit-exceeded",
            error.GetProperty("documentationKey").GetString());

        string[] categories = error.GetProperty("categories")
            .EnumerateArray()
            .Select(item => item.GetString()!)
            .ToArray();
        Assert.Contains("THERMAL", categories);
        Assert.Contains("VALIDATION", categories);

        string[] tags = error.GetProperty("tags")
            .EnumerateArray()
            .Select(item => item.GetString()!)
            .ToArray();
        Assert.Contains("THERMAL", tags);
        Assert.Contains("TEMPERATURE", tags);
        Assert.Contains("USER_VISIBLE", tags);
    }

    private static async Task<JsonDocument> LoadJsonAsync(string path)
    {
        await using FileStream stream = File.OpenRead(path);
        return await JsonDocument.ParseAsync(stream);
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

        throw new Xunit.Sdk.XunitException($"Catalog item '{name}' was not found.");
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "Jsons", "WhenItFails")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not locate the repository root containing Jsons/WhenItFails.");
    }
}
