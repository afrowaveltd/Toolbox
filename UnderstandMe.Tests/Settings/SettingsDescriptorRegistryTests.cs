using System.Text.Json;
using Afrowave.Toolbox.UnderstandMe.Settings;
using Xunit;

namespace Afrowave.Toolbox.UnderstandMe.Tests.Settings;

public sealed class SettingsDescriptorRegistryTests
{
    [Fact]
    public void Built_in_descriptors_expose_complete_generic_field_metadata()
    {
        var registry = SettingsDescriptorRegistry.CreateDefault();
        Assert.Equal(2, registry.GetAll().Count);
        var backend = registry.Find(SettingsComponentKind.TranslationBackend, "libretranslate");
        Assert.NotNull(backend);
        Assert.Contains(backend.Fields, f => f.Key == "Endpoint" && f.Kind == SettingsFieldKind.HttpEndpoint);
        Assert.Contains(backend.Fields, f => f.Key == "ApiKeyReference" &&
            f.Kind == SettingsFieldKind.SecretReference && f.RequiredWhenKey == "AuthenticationMode");
        Assert.NotNull(registry.Find(SettingsComponentKind.LocalizationStorage, "FileJson"));
        Assert.Null(registry.Find(SettingsComponentKind.TranslationBackend, "FileJson"));
    }

    [Fact]
    public void Valid_libre_and_storage_options_round_trip_without_specialized_form()
    {
        var settings = new UnderstandMeSettings
        {
            Translation = new TranslationSettings
            {
                Providers =
                [
                    new()
                    {
                        Id = "libre-local",
                        Type = "LibreTranslate",
                        Options = new()
                        {
                            ["Endpoint"] = JsonSerializer.SerializeToElement("http://localhost:5000"),
                            ["AuthenticationMode"] = JsonSerializer.SerializeToElement("ApiKey"),
                            ["ApiKeyReference"] = JsonSerializer.SerializeToElement("env:LIBRE_API_KEY"),
                            ["RequestTimeoutSeconds"] = JsonSerializer.SerializeToElement(15),
                            ["RequestThrottleMs"] = JsonSerializer.SerializeToElement(0)
                        }
                    }
                ]
            },
            Localization = new LocalizationSettings
            {
                Storages =
                [
                    new()
                    {
                        Id = "main", Type = "FileJson",
                        Options = new()
                        {
                            ["Directory"] = JsonSerializer.SerializeToElement("Languages"),
                            ["ReadOnly"] = JsonSerializer.SerializeToElement(true)
                        }
                    }
                ]
            }
        };
        Assert.Empty(UnderstandMeSettingsValidator.Validate(settings));

        var copy = JsonSerializer.Deserialize<UnderstandMeSettings>(JsonSerializer.Serialize(settings))!;
        Assert.Empty(UnderstandMeSettingsValidator.Validate(copy));
        Assert.Equal("env:LIBRE_API_KEY", copy.Translation.Providers[0].Options["ApiKeyReference"].GetString());
        Assert.True(copy.Localization.Storages[0].Options["ReadOnly"].GetBoolean());
    }

    [Fact]
    public void Reports_nonsecret_field_errors_and_missing_conditional_credentials()
    {
        var settings = new UnderstandMeSettings
        {
            Translation = new TranslationSettings
            {
                Providers =
                [
                    new()
                    {
                        Id = "broken", Type = "LibreTranslate",
                        Options = new()
                        {
                            ["Endpoint"] = JsonSerializer.SerializeToElement("https://alice:password@host.example"),
                            ["AuthenticationMode"] = JsonSerializer.SerializeToElement("ApiKey"),
                            ["RequestTimeoutSeconds"] = JsonSerializer.SerializeToElement(0),
                            ["RequestThrottleMs"] = JsonSerializer.SerializeToElement("five"),
                            ["Typo"] = JsonSerializer.SerializeToElement("ignored")
                        }
                    }
                ]
            }
        };
        var errors = UnderstandMeSettingsValidator.Validate(settings);
        Assert.Contains(errors, x => x.Path.EndsWith("Options.Endpoint"));
        Assert.Contains(errors, x => x.Path.EndsWith("Options.ApiKeyReference"));
        Assert.Contains(errors, x => x.Path.EndsWith("Options.RequestTimeoutSeconds"));
        Assert.Contains(errors, x => x.Path.EndsWith("Options.RequestThrottleMs"));
        Assert.Contains(errors, x => x.Path.EndsWith("Options.Typo"));
        Assert.DoesNotContain(errors, x => x.Message.Contains("password", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Rejects_raw_api_key_and_noncanonical_option_case()
    {
        var registry = SettingsDescriptorRegistry.CreateDefault();
        var options = new Dictionary<string, JsonElement>
        {
            ["ApiKeyReference"] = JsonSerializer.SerializeToElement("this-is-a-real-key"),
            ["endpoint"] = JsonSerializer.SerializeToElement("http://localhost:5000")
        };
        var errors = registry.ValidateOptions(
            SettingsComponentKind.TranslationBackend, "LibreTranslate", options, "$.Translation.Providers[0]");
        Assert.Equal(2, errors.Count);
        Assert.All(errors, x => Assert.StartsWith("$.Translation.Providers[0].Options.", x.Path));
    }

    [Fact]
    public void Unknown_types_fail_closed_until_plugin_schema_is_registered()
    {
        var registry = SettingsDescriptorRegistry.CreateDefault();
        var plugin = new TranslationBackendSettings { Id = "custom", Type = "MyTranslator" };
        var settings = new UnderstandMeSettings
        {
            Translation = new TranslationSettings { Providers = [plugin] }
        };
        Assert.Contains(UnderstandMeSettingsValidator.Validate(settings), x => x.Path.EndsWith(".Type"));
        registry.Register(new(SettingsComponentKind.TranslationBackend, "MyTranslator", "My translator",
        [
            new("Model", "Model name", "Model to use.", SettingsFieldKind.Text,
                JsonSerializer.SerializeToElement("basic"))
        ]));
        Assert.Empty(UnderstandMeSettingsValidator.Validate(settings, registry));
        Assert.Throws<ArgumentException>(() =>
            registry.Register(new(SettingsComponentKind.TranslationBackend, "mytranslator", "Duplicate",
            [new("Model", "Model", "Model name.", SettingsFieldKind.Text)])));
    }

    [Fact]
    public void Malformed_field_schemas_are_rejected()
    {
        var registry = new SettingsDescriptorRegistry();
        Assert.Throws<ArgumentException>(() => registry.Register(new(
            SettingsComponentKind.TranslationBackend, "Bad", "Bad",
            [
                new("Timeout", "Timeout", "Seconds.", SettingsFieldKind.Integer,
                    JsonSerializer.SerializeToElement(-1), Minimum: 1)
            ])));
        Assert.Throws<ArgumentException>(() => registry.Register(new(
            SettingsComponentKind.LocalizationStorage, "Bad", "Bad",
            [
                new("X", "First", "Help.", SettingsFieldKind.Text),
                new("x", "Second", "Help.", SettingsFieldKind.Text)
            ])));
    }

    [Fact]
    public void Custom_plugin_schema_is_enforced_by_file_storage()
    {
        var directory = Path.Combine(Path.GetTempPath(), "um-metadata-" + Guid.NewGuid().ToString("N"));
        var file = Path.Combine(directory, "UnderstandMeSettings.json");
        try
        {
            var registry = SettingsDescriptorRegistry.CreateDefault();
            registry.Register(new(SettingsComponentKind.TranslationBackend, "CustomEngine", "Custom engine",
            [
                new("Model", "Model", "Model identifier.", SettingsFieldKind.Text, Required: true)
            ]));
            var settings = new UnderstandMeSettings
            {
                Translation = new TranslationSettings
                {
                    Providers =
                    [
                        new()
                        {
                            Id = "custom", Type = "CustomEngine",
                            Options = new() { ["Model"] = JsonSerializer.SerializeToElement("demo") }
                        }
                    ]
                }
            };
            var revision = UnderstandMeSettingsJsonStore.Save(file, settings, descriptors: registry);
            Assert.Equal(revision, UnderstandMeSettingsJsonStore.Load(file, registry).Revision);
            Assert.Throws<UnderstandMeSettingsValidationException>(() => UnderstandMeSettingsJsonStore.Load(file));
        }
        finally
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, true);
        }
    }
}
