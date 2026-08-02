using System.Text.Json;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ThermalFallbackProtectionActionUnverifiedCatalogTests
{
    [Fact]
    public async Task DefaultCatalogs_ShouldDefineThermalFallbackProtectionActionUnverifiedContract()
    {
        string repositoryRoot = FindRepositoryRoot();
        string jsonsDirectory = Path.Combine(repositoryRoot, "Jsons", "WhenItFails");

        using JsonDocument errorsDocument = await LoadJsonAsync(
            Path.Combine(jsonsDirectory, "errors.en.json"));

        JsonElement error = FindByName(
            errorsDocument.RootElement.GetProperty("errors"),
            "THERMALFALLBACKPROTECTIONACTIONUNVERIFIED");

        Assert.Equal("AFW_THM_0012", error.GetProperty("id").GetString());
        Assert.Equal(1_000_012, error.GetProperty("code").GetInt32());
        Assert.Equal("AFW", error.GetProperty("owner").GetString());
        Assert.Equal("THM", error.GetProperty("codePrefix").GetString());
        Assert.Equal("THERMAL", error.GetProperty("codeGroup").GetString());
        Assert.Equal("THERMAL", error.GetProperty("primaryCategory").GetString());
        Assert.Equal(
            "Thermal fallback protection action unverified",
            error.GetProperty("title").GetString());
        Assert.Equal(
            "Thermal fallback protection action {fallbackAction} for {component} could not be verified after {primaryAction} while handling {condition}.",
            error.GetProperty("message").GetString());
        Assert.Equal("Critical", error.GetProperty("defaultSeverity").GetString());
        Assert.Equal(
            "Verify the primary action result, fallback command delivery, acknowledgement, actuator feedback, telemetry freshness, correlation identifiers, timeout policy, remaining safe options, operator escalation, and evidence required before restart.",
            error.GetProperty("developerHint").GetString());
        Assert.Equal(
            "when-it-fails/errors/thermal/thermal-fallback-protection-action-unverified",
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
        Assert.Contains("FALLBACK_ACTION", subcategories);
        Assert.Contains("VERIFICATION", subcategories);

        string[] tags = error.GetProperty("tags")
            .EnumerateArray()
            .Select(item => item.GetString()!)
            .ToArray();
        Assert.Contains("THERMAL", tags);
        Assert.Contains("FAIL_SAFE", tags);
        Assert.Contains("VERIFICATION_REQUIRED", tags);
        Assert.Contains("OPERATOR_ACTION_REQUIRED", tags);
        Assert.Contains("USER_VISIBLE", tags);
        Assert.DoesNotContain("FALLBACK_FAILED", tags);
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
