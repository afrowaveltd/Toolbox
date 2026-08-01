using System.Text.Json;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class TemperatureReadingStaleCatalogTests
{
    [Fact]
    public async Task DefaultCatalogs_ShouldDefineTemperatureReadingStaleContract()
    {
        string repositoryRoot = FindRepositoryRoot();
        string jsonsDirectory = Path.Combine(repositoryRoot, "Jsons", "WhenItFails");

        using JsonDocument errorsDocument = await LoadJsonAsync(
            Path.Combine(jsonsDirectory, "errors.en.json"));

        JsonElement error = FindByName(
            errorsDocument.RootElement.GetProperty("errors"),
            "TEMPERATUREREADINGSTALE");

        Assert.Equal("AFW_THM_0004", error.GetProperty("id").GetString());
        Assert.Equal(1_000_004, error.GetProperty("code").GetInt32());
        Assert.Equal("AFW", error.GetProperty("owner").GetString());
        Assert.Equal("THM", error.GetProperty("codePrefix").GetString());
        Assert.Equal("THERMAL", error.GetProperty("codeGroup").GetString());
        Assert.Equal("THERMAL", error.GetProperty("primaryCategory").GetString());
        Assert.Equal(
            "Temperature reading stale",
            error.GetProperty("title").GetString());
        Assert.Equal(
            "Temperature reading from sensor {sensor} is stale; its age of {age} exceeds the configured maximum age of {maxAge}.",
            error.GetProperty("message").GetString());
        Assert.Equal("Error", error.GetProperty("defaultSeverity").GetString());
        Assert.Equal(
            "Verify sensor polling, timestamps, clock synchronization, transport delays, buffering, cache invalidation, and the configured stale-data fail-safe policy.",
            error.GetProperty("developerHint").GetString());
        Assert.Equal(
            "when-it-fails/errors/thermal/temperature-reading-stale",
            error.GetProperty("documentationKey").GetString());

        string[] categories = error.GetProperty("categories")
            .EnumerateArray()
            .Select(item => item.GetString()!)
            .ToArray();
        Assert.Contains("THERMAL", categories);
        Assert.Contains("VALIDATION", categories);

        string[] subcategories = error.GetProperty("subcategories")
            .EnumerateArray()
            .Select(item => item.GetString()!)
            .ToArray();
        Assert.Contains("SENSOR", subcategories);
        Assert.Contains("STALE_READING", subcategories);

        string[] tags = error.GetProperty("tags")
            .EnumerateArray()
            .Select(item => item.GetString()!)
            .ToArray();
        Assert.Contains("THERMAL", tags);
        Assert.Contains("TEMPERATURE", tags);
        Assert.Contains("SENSOR", tags);
        Assert.Contains("STALE_DATA", tags);
        Assert.Contains("FAIL_SAFE", tags);
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
