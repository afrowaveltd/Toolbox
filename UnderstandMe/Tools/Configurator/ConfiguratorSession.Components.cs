using System.Text.Json;
using Afrowave.Toolbox.UnderstandMe.Settings;

namespace Afrowave.Toolbox.UnderstandMe.Configurator;

/// <summary>Descriptor-driven provider/storage draft edits. No UI or disk I/O.</summary>
public sealed partial class ConfiguratorSession
{
    private static readonly SettingsDescriptorRegistry Descriptors = SettingsDescriptorRegistry.CreateDefault();

    /// <summary>Registered built-in component schemas. The editor does not invent provider-specific fields.</summary>
    public static IReadOnlyList<SettingsComponentDescriptor> GetAvailableTypes(SettingsComponentKind kind) =>
        Descriptors.GetAll().Where(x => x.Kind == kind).OrderBy(x => x.Type, StringComparer.Ordinal).ToArray();

    /// <summary>Find a registered component schema, or return null if the type has no schema.</summary>
    public static SettingsComponentDescriptor? FindDescriptor(SettingsComponentKind kind, string type) =>
        Descriptors.Find(kind, type);

    /// <summary>Get the current provider/storage ID list without exposing stored secret option values.</summary>
    public IReadOnlyList<(string Id, string Type)> ListComponents(SettingsComponentKind kind) =>
        kind switch
        {
            SettingsComponentKind.TranslationBackend =>
                Draft.Translation.Providers.Select(x => (x.Id, x.Type)).ToArray(),
            SettingsComponentKind.LocalizationStorage =>
                Draft.Localization.Storages.Select(x => (x.Id, x.Type)).ToArray(),
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };

    /// <summary>Read the currently registered metadata and a copy of its options for a UI form.</summary>
    public (string Type, IReadOnlyDictionary<string, JsonElement> Options) GetComponent(
        SettingsComponentKind kind, string id)
    {
        return kind switch
        {
            SettingsComponentKind.TranslationBackend =>
                FindProvider(id) is { } p
                    ? (p.Type, new Dictionary<string, JsonElement>(p.Options))
                    : throw new ArgumentException("Unknown provider ID.", nameof(id)),
            SettingsComponentKind.LocalizationStorage =>
                FindStorage(id) is { } s
                    ? (s.Type, new Dictionary<string, JsonElement>(s.Options))
                    : throw new ArgumentException("Unknown storage ID.", nameof(id)),
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };
    }

    /// <summary>Add a registration with descriptor defaults. No file is written before Save.</summary>
    public void AddComponent(SettingsComponentKind kind, string id, string type)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(type);
        id = id.Trim();
        if (ListComponents(kind).Any(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException("Component ID must be unique.", nameof(id));
        var schema = Descriptors.Find(kind, type)
            ?? throw new ArgumentException("Component type is not registered.", nameof(type));
        var values = schema.Fields
            .Where(x => x.DefaultValue.HasValue)
            .ToDictionary(x => x.Key, x => x.DefaultValue!.Value.Clone(), StringComparer.Ordinal);
        if (kind == SettingsComponentKind.TranslationBackend)
        {
            var providers = Draft.Translation.Providers.ToList();
            providers.Add(new TranslationBackendSettings { Id = id, Type = schema.Type, Options = values });
            ReplaceProviders(providers);
        }
        else if (kind == SettingsComponentKind.LocalizationStorage)
        {
            var storages = Draft.Localization.Storages.ToList();
            storages.Add(new LocalizationStorageSettings
            {
                Id = id, Type = schema.Type, Options = values,
                SourceLanguage = Draft.Localization.DefaultSourceLanguage
            });
            ReplaceStorages(storages);
        }
        else
            throw new ArgumentOutOfRangeException(nameof(kind));
    }

    /// <summary>Remove an existing registration from the draft; caller must confirm destructive actions.</summary>
    public void RemoveComponent(SettingsComponentKind kind, string id)
    {
        if (kind == SettingsComponentKind.TranslationBackend)
        {
            var list = Draft.Translation.Providers.Where(x => x.Id != id).ToList();
            if (list.Count == Draft.Translation.Providers.Count)
                throw new ArgumentException("Unknown provider ID.", nameof(id));
            ReplaceProviders(list);
        }
        else if (kind == SettingsComponentKind.LocalizationStorage)
        {
            var list = Draft.Localization.Storages.Where(x => x.Id != id).ToList();
            if (list.Count == Draft.Localization.Storages.Count)
                throw new ArgumentException("Unknown storage ID.", nameof(id));
            ReplaceStorages(list);
        }
        else
            throw new ArgumentOutOfRangeException(nameof(kind));
    }

    /// <summary>Set a typed option, enforcing the existing registered field schema before modifying the draft.</summary>
    public void SetOption(SettingsComponentKind kind, string id, string key, JsonElement value)
    {
        var component = GetComponent(kind, id);
        var descriptor = Descriptors.Find(kind, component.Type)
            ?? throw new ArgumentException("No registered descriptor for component type.", nameof(kind));
        if (!descriptor.Fields.Any(x => x.Key == key))
            throw new ArgumentException("Unknown option key.", nameof(key));
        var newOptions = new Dictionary<string, JsonElement>(component.Options, StringComparer.Ordinal)
        {
            [key] = value.Clone()
        };
        var fieldError = Descriptors.ValidateOptions(kind, component.Type, newOptions, "$")
            .FirstOrDefault(x => x.Path == "$.Options." + key && x.Message != "Required option is missing.");
        if (fieldError is not null)
            throw new ArgumentException(fieldError.Message, nameof(value));
        ReplaceOptions(kind, id, newOptions);
    }

    /// <summary>Remove an optional option; required/conditionally required values are caught by core validation.</summary>
    public void RemoveOption(SettingsComponentKind kind, string id, string key)
    {
        var component = GetComponent(kind, id);
        if (Descriptors.Find(kind, component.Type)?.Fields.All(x => x.Key != key) != false)
            throw new ArgumentException("Unknown option key.", nameof(key));
        var options = new Dictionary<string, JsonElement>(component.Options, StringComparer.Ordinal);
        options.Remove(key);
        ReplaceOptions(kind, id, options);
    }

    /// <summary>Enable/disable a translation provider without mutating the loaded baseline.</summary>
    public void SetProviderEnabled(string id, bool enabled)
    {
        if (FindProvider(id) is null) throw new ArgumentException("Unknown provider ID.", nameof(id));
        ReplaceProviders(Draft.Translation.Providers.Select(x => x.Id == id ? new TranslationBackendSettings
        {
            Id = x.Id, Type = x.Type, Options = x.Options, Enabled = enabled
        } : x).ToList());
    }

    /// <summary>Edit the source locale and dictionaryless setting of a storage registration.</summary>
    public void SetStorageProperties(string id, string sourceLanguage, bool setValueFromKey)
    {
        if (FindStorage(id) is null) throw new ArgumentException("Unknown storage ID.", nameof(id));
        ReplaceStorages(Draft.Localization.Storages.Select(x => x.Id == id ? new LocalizationStorageSettings
        {
            Id = x.Id, Type = x.Type, Options = x.Options,
            SourceLanguage = sourceLanguage.Trim(), SetValueFromKey = setValueFromKey
        } : x).ToList());
    }

    private TranslationBackendSettings? FindProvider(string id) =>
        Draft.Translation.Providers.FirstOrDefault(x => x.Id == id);

    private LocalizationStorageSettings? FindStorage(string id) =>
        Draft.Localization.Storages.FirstOrDefault(x => x.Id == id);

    private void ReplaceOptions(SettingsComponentKind kind, string id, Dictionary<string, JsonElement> options)
    {
        if (kind == SettingsComponentKind.TranslationBackend)
            ReplaceProviders(Draft.Translation.Providers.Select(x => x.Id == id
                ? new TranslationBackendSettings { Id = x.Id, Type = x.Type, Enabled = x.Enabled, Options = options }
                : x).ToList());
        else if (kind == SettingsComponentKind.LocalizationStorage)
            ReplaceStorages(Draft.Localization.Storages.Select(x => x.Id == id
                ? new LocalizationStorageSettings
                {
                    Id = x.Id, Type = x.Type, SourceLanguage = x.SourceLanguage,
                    SetValueFromKey = x.SetValueFromKey, Options = options
                } : x).ToList());
        else
            throw new ArgumentOutOfRangeException(nameof(kind));
    }

    private void ReplaceProviders(List<TranslationBackendSettings> providers)
    {
        var old = Draft;
        Draft = new UnderstandMeSettings
        {
            SchemaVersion = old.SchemaVersion,
            Localization = old.Localization,
            Translation = new TranslationSettings { Providers = providers, Queues = old.Translation.Queues }
        };
    }

    private void ReplaceStorages(List<LocalizationStorageSettings> storages)
    {
        var old = Draft;
        Draft = new UnderstandMeSettings
        {
            SchemaVersion = old.SchemaVersion,
            Localization = new LocalizationSettings
            {
                DefaultSourceLanguage = old.Localization.DefaultSourceLanguage, Storages = storages
            },
            Translation = old.Translation
        };
    }
}
