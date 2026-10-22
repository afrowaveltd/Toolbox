namespace Afrowave.Toolbox.UnderstandMe.Settings;

/// <summary>Root configuration for the localization and translation host.</summary>
/// <remarks>Instances are configuration data, not a running service. Validate them before use.</remarks>
public sealed class UnderstandMeSettings
{
    /// <summary>Version of the JSON settings contract.</summary>
    public int SchemaVersion { get; init; } = 1;

    /// <summary>Localization and source-dictionary configuration.</summary>
    public LocalizationSettings Localization { get; init; } = new();

    /// <summary>Translation providers and queue defaults.</summary>
    public TranslationSettings Translation { get; init; } = new();
}

/// <summary>Localization options and registered dictionaries.</summary>
public sealed class LocalizationSettings
{
    /// <summary>Default source language for new dictionaries.</summary>
    public string DefaultSourceLanguage { get; init; } = "en";

    /// <summary>Explicitly registered dictionary stores.</summary>
    public List<LocalizationStorageSettings> Storages { get; init; } = [];
}

/// <summary>Configuration of a single dictionary store.</summary>
public sealed class LocalizationStorageSettings
{
    /// <summary>Unique identifier within this host.</summary>
    public string Id { get; init; } = "";

    /// <summary>Source language authoritative for this store.</summary>
    public string SourceLanguage { get; init; } = "en";

    /// <summary>When true, a source entry must have an exactly equal key and value.</summary>
    /// <remarks>Validation of existing source entries is a separate operation.</remarks>
    public bool SetValueFromKey { get; init; } = true;
}

/// <summary>Translation provider registration and scheduler defaults.</summary>
public sealed class TranslationSettings
{
    /// <summary>Configured automatic or manual translation providers.</summary>
    public List<TranslationBackendSettings> Providers { get; init; } = [];

    /// <summary>Shared defaults for translation queues and workers.</summary>
    public TranslationQueueSettings Queues { get; init; } = new();
}

/// <summary>A provider registration; provider-specific options are introduced by adapters.</summary>
public sealed class TranslationBackendSettings
{
    /// <summary>Unique registration ID, allowing multiple instances of one type.</summary>
    public string Id { get; init; } = "";

    /// <summary>Backend adapter type, for example LibreTranslate.</summary>
    public string Type { get; init; } = "";

    /// <summary>Whether this registration may be used.</summary>
    public bool Enabled { get; init; } = true;
}

/// <summary>Queue-wide resource defaults.</summary>
public sealed class TranslationQueueSettings
{
    /// <summary>Maximum FIFO items assigned ahead to each worker by default.</summary>
    public int DefaultWorkerBufferCapacity { get; init; } = 5;
}

/// <summary>One configuration validation issue with an addressable field path.</summary>
/// <param name="Path">JSON-like path to the field.</param>
/// <param name="Message">Human-readable problem description.</param>
public sealed record SettingsValidationIssue(string Path, string Message);

/// <summary>Validates the shared, provider-independent settings contract without I/O.</summary>
public static class UnderstandMeSettingsValidator
{
    /// <summary>Validates a complete settings object without modifying it or contacting backends.</summary>
    /// <param name="settings">Configuration to validate.</param>
    /// <returns>All detected issues; an empty collection means structurally valid settings.</returns>
    public static IReadOnlyList<SettingsValidationIssue> Validate(UnderstandMeSettings? settings)
    {
        var errors = new List<SettingsValidationIssue>();
        if (settings is null)
        {
            errors.Add(new("$", "Settings cannot be null."));
            return errors;
        }

        if (settings.SchemaVersion != 1)
            errors.Add(new("$.SchemaVersion", "Only settings schema version 1 is supported."));
        if (settings.Localization is null)
            errors.Add(new("$.Localization", "Localization settings are required."));
        else
        {
            if (string.IsNullOrWhiteSpace(settings.Localization.DefaultSourceLanguage))
                errors.Add(new("$.Localization.DefaultSourceLanguage", "Default source language is required."));
            ValidateStorages(settings.Localization.Storages, errors);
        }

        if (settings.Translation is null)
            errors.Add(new("$.Translation", "Translation settings are required."));
        else
        {
            ValidateProviders(settings.Translation.Providers, errors);
            if (settings.Translation.Queues is null)
                errors.Add(new("$.Translation.Queues", "Queue settings are required."));
            else if (settings.Translation.Queues.DefaultWorkerBufferCapacity < 1)
                errors.Add(new("$.Translation.Queues.DefaultWorkerBufferCapacity", "Buffer capacity must be positive."));
        }
        return errors;
    }

    private static void ValidateStorages(List<LocalizationStorageSettings>? stores, List<SettingsValidationIssue> errors)
    {
        if (stores is null)
        {
            errors.Add(new("$.Localization.Storages", "Storage list cannot be null."));
            return;
        }
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < stores.Count; i++)
        {
            var path = "$.Localization.Storages[" + i + "]";
            var item = stores[i];
            if (item is null)
            {
                errors.Add(new(path, "Storage cannot be null."));
                continue;
            }
            if (string.IsNullOrWhiteSpace(item.Id))
                errors.Add(new(path + ".Id", "Storage ID is required."));
            else if (!ids.Add(item.Id.Trim()))
                errors.Add(new(path + ".Id", "Storage ID must be unique."));
            if (string.IsNullOrWhiteSpace(item.SourceLanguage))
                errors.Add(new(path + ".SourceLanguage", "Source language is required."));
        }
    }

    private static void ValidateProviders(List<TranslationBackendSettings>? providers, List<SettingsValidationIssue> errors)
    {
        if (providers is null)
        {
            errors.Add(new("$.Translation.Providers", "Provider list cannot be null."));
            return;
        }
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < providers.Count; i++)
        {
            var path = "$.Translation.Providers[" + i + "]";
            var item = providers[i];
            if (item is null)
            {
                errors.Add(new(path, "Provider cannot be null."));
                continue;
            }
            if (string.IsNullOrWhiteSpace(item.Id))
                errors.Add(new(path + ".Id", "Provider ID is required."));
            else if (!ids.Add(item.Id.Trim()))
                errors.Add(new(path + ".Id", "Provider ID must be unique."));
            if (string.IsNullOrWhiteSpace(item.Type))
                errors.Add(new(path + ".Type", "Provider type is required."));
        }
    }
}
