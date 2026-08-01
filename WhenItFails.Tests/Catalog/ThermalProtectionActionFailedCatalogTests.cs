using System.Text.Json;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ThermalProtectionActionFailedCatalogTests
{
    [Fact]
    public async Task DefaultCatalogs_ShouldDefineThermalProtectionActionFailedContract()
    {
        string repositoryRoot = FindRepositoryRoot();
        string jsonsDirectory = Path.Combine(repositoryRoot, "Jsons", "WhenItFails");

        using JsonDocument errorsDocument = await LoadJsonAsync(
            Path.Combine(jsonsDirectory, "errors.en.json"));

        JsonElement error = FindByName(
            errorsDocument.RootElement.GetProperty("errors"),
            "THERMALPROTECTIONACTIONFAILED");

        Assert.Equal("AFW_THM_0009", error.GetProperty("id").GetString());
        Assert.Equal(1_000_009, error.GetProperty("code").GetInt32());
        Assert.Equal("AFW", error.GetProperty("owner").GetString());
        Assert.Equal("THM", error.GetProperty("codePrefix").GetString());
        Assert.Equal("THERMAL", error.GetProperty("codeGroup").GetString());
        Assert.Equal("THERMAL", error.GetProperty("primaryCategory").GetString());
        Assert.Equal(
            "Thermal protection action failed",
            error.GetProperty("title").GetString());
        Assert.Equal(
            "Thermal protection action {action} failed for {component} while handling {condition}.",
            error.GetProperty("message").GetString());
        Assert.Equal("Critical", error.GetProperty("defaultSeverity").GetString());
        Assert.Equal(
            "Verify the selected protection policy, actuator or control path, permissions, command result, hardware state, fallback action, operator escalation, and evidence required before restart.",
            error.GetProperty("developerHint").GetString());
        Assert.Equal(
            "when-it-fails/errors/thermal/thermal-protection-action-failed",
            error.GetProperty("documentationKey").GetString());

        string[] categories = error.GetProperty("categories")
            .EnumerateArray()
            .Select(item => item.GetString()!)
            .ToArray();
        Assert.Contains("THERMAL", categories);
        Assert.Contains("GENERAL", categories);

        string[] subcategories = error.GetProperty("subcategories")
            .EnumerateArray()
            .Select(item => item.GetString()!)
            .ToArray();
        Assert.Contains("PROTECTION_ACTION", subcategories);
        Assert.Contains("FAIL_SAFE", subcategories);

        string[] tags = error.GetProperty("tags")
            .EnumerateArray()
            .Select(item => item.GetString()!)
            .ToArray();
        Assert.Contains("THERMAL", tags);
        Assert.Contains("FAIL_SAFE", tags);
        Assert.Contains("SHUTDOWN", tags);
        Assert.Contains("OPERATOR_ACTION_REQUIRED", tags);
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
