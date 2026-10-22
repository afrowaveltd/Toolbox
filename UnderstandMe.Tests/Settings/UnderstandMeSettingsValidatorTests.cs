using System.Text.Json;
using Afrowave.Toolbox.UnderstandMe.Settings;
using Xunit;

namespace Afrowave.Toolbox.UnderstandMe.Tests.Settings;

public sealed class UnderstandMeSettingsValidatorTests
{
    [Fact]
    public void Default_configuration_is_valid_and_has_small_fifo_buffer()
    {
        var settings = new UnderstandMeSettings();
        Assert.Empty(UnderstandMeSettingsValidator.Validate(settings));
        Assert.Equal(5, settings.Translation.Queues.DefaultWorkerBufferCapacity);
        Assert.Empty(settings.Translation.Providers);
    }

    [Fact]
    public void Multiple_instances_of_same_backend_type_are_allowed()
    {
        var settings = new UnderstandMeSettings
        {
            Translation = new TranslationSettings
            {
                Providers = [
                    new() { Id = "libre-local", Type = "LibreTranslate" },
                    new() { Id = "libre-remote", Type = "LibreTranslate", Enabled = false }
                ]
            }
        };
        Assert.Empty(UnderstandMeSettingsValidator.Validate(settings));
    }

    [Fact]
    public void Detects_duplicates_case_insensitively_and_invalid_capacity()
    {
        var settings = new UnderstandMeSettings
        {
            Localization = new LocalizationSettings
            {
                Storages = [
                    new() { Id = "Main" },
                    new() { Id = " main " }
                ]
            },
            Translation = new TranslationSettings
            {
                Providers = [
                    new() { Id = "Local", Type = "LibreTranslate" },
                    new() { Id = "local", Type = "LibreTranslate" }
                ],
                Queues = new() { DefaultWorkerBufferCapacity = 0 }
            }
        };
        var issues = UnderstandMeSettingsValidator.Validate(settings);
        Assert.Contains(issues, issue => issue.Path.EndsWith("Storages[1].Id"));
        Assert.Contains(issues, issue => issue.Path.EndsWith("Providers[1].Id"));
        Assert.Contains(issues, issue => issue.Path.EndsWith("DefaultWorkerBufferCapacity"));
    }

    [Fact]
    public void Missing_fields_and_unsupported_schema_report_paths()
    {
        var settings = new UnderstandMeSettings
        {
            SchemaVersion = 200,
            Localization = new LocalizationSettings
            {
                DefaultSourceLanguage = " ",
                Storages = [new() { Id = "", SourceLanguage = "" }]
            },
            Translation = new TranslationSettings
            {
                Providers = [new() { Id = "", Type = "" }]
            }
        };
        var issues = UnderstandMeSettingsValidator.Validate(settings);
        Assert.True(issues.Count >= 6);
        Assert.Contains(issues, issue => issue.Path == "$.SchemaVersion");
        Assert.Single(UnderstandMeSettingsValidator.Validate(null));
    }

    [Fact]
    public void Standard_json_round_trip_preserves_component_configuration()
    {
        var original = new UnderstandMeSettings
        {
            Localization = new LocalizationSettings
            {
                Storages = [new() { Id = "source", SourceLanguage = "cs", SetValueFromKey = false }]
            }
        };
        var json = JsonSerializer.Serialize(original);
        var result = JsonSerializer.Deserialize<UnderstandMeSettings>(json);
        Assert.NotNull(result);
        Assert.Empty(UnderstandMeSettingsValidator.Validate(result));
        Assert.Equal("cs", result.Localization.Storages[0].SourceLanguage);
        Assert.False(result.Localization.Storages[0].SetValueFromKey);
    }
}
