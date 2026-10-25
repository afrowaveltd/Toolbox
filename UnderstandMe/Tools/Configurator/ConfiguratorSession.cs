using Afrowave.Toolbox.UnderstandMe.Settings;
using System.Text.Json;

namespace Afrowave.Toolbox.UnderstandMe.Configurator;

/// <summary>Offline settings draft with revision-checked persistence and no implicit writes.</summary>
public sealed partial class ConfiguratorSession
{
    private string _baselineJson;

    private ConfiguratorSession(string path, UnderstandMeSettings settings, string? revision)
    {
        Path = System.IO.Path.GetFullPath(path);
        Draft = settings;
        _baselineJson = JsonSerializer.Serialize(settings);
        Revision = revision;
    }

    /// <summary>Selected absolute configuration file path.</summary>
    public string Path { get; }
    /// <summary>Candidate settings held only in RAM until explicitly saved.</summary>
    public UnderstandMeSettings Draft { get; private set; }
    /// <summary>Revision loaded from disk or null for an unsaved new file.</summary>
    public string? Revision { get; private set; }
    /// <summary>Whether settings differ from the baseline or have never been saved.</summary>
    public bool IsDirty => Revision is null ||
        !string.Equals(JsonSerializer.Serialize(Draft), _baselineJson, StringComparison.Ordinal);

    /// <summary>Load an existing settings document using core validation.</summary>
    public static ConfiguratorSession Open(string path)
    {
        var loaded = UnderstandMeSettingsJsonStore.Load(path);
        return new ConfiguratorSession(path, loaded.Settings, loaded.Revision);
    }

    /// <summary>Create an in-memory default draft without overwriting an existing file.</summary>
    public static ConfiguratorSession CreateNew(string path)
    {
        var fullPath = System.IO.Path.GetFullPath(path);
        if (File.Exists(fullPath))
            throw new IOException("A file already exists at this path.");
        return new ConfiguratorSession(fullPath, new UnderstandMeSettings(), null);
    }

    /// <summary>Edit the source language while preserving all storage registrations.</summary>
    public void SetSourceLanguage(string language)
    {
        ArgumentNullException.ThrowIfNull(language);
        var old = Draft;
        Draft = new UnderstandMeSettings
        {
            SchemaVersion = old.SchemaVersion,
            Localization = new LocalizationSettings
            {
                DefaultSourceLanguage = language.Trim(),
                Storages = old.Localization.Storages
            },
            Translation = old.Translation
        };
    }

    /// <summary>Edit FIFO buffer capacity without losing configured providers.</summary>
    public void SetWorkerBufferCapacity(int capacity)
    {
        var old = Draft;
        Draft = new UnderstandMeSettings
        {
            SchemaVersion = old.SchemaVersion,
            Localization = old.Localization,
            Translation = new TranslationSettings
            {
                Providers = old.Translation.Providers,
                Queues = new TranslationQueueSettings { DefaultWorkerBufferCapacity = capacity }
            }
        };
    }

    /// <summary>Validate the complete RAM draft without saving or contacting providers.</summary>
    public IReadOnlyList<SettingsValidationIssue> Validate() => UnderstandMeSettingsValidator.Validate(Draft);

    /// <summary>Save after UI confirmation; the core store validates and checks on-disk revision.</summary>
    public void Save()
    {
        var revision = UnderstandMeSettingsJsonStore.Save(Path, Draft, Revision);
        Revision = revision;
        _baselineJson = JsonSerializer.Serialize(Draft);
    }
}
