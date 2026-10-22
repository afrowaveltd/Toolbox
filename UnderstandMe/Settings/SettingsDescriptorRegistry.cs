using System.Text.Json;

namespace Afrowave.Toolbox.UnderstandMe.Settings;

/// <summary>Component families supported by the generic configuration editor.</summary>
public enum SettingsComponentKind
{
    /// <summary>A translation provider or translator adapter.</summary>
    TranslationBackend,

    /// <summary>A dictionary or translation storage adapter.</summary>
    LocalizationStorage
}

/// <summary>Value types that a generic editor can render without provider-specific screens.</summary>
public enum SettingsFieldKind
{
    /// <summary>Plain text.</summary>
    Text,

    /// <summary>An absolute HTTP or HTTPS endpoint without embedded credentials.</summary>
    HttpEndpoint,

    /// <summary>An integral numeric value.</summary>
    Integer,

    /// <summary>A true/false value.</summary>
    Boolean,

    /// <summary>A value selected from an allowed set.</summary>
    Choice,

    /// <summary>A reference to an external secret, not a literal credential.</summary>
    SecretReference
}

/// <summary>Metadata needed to render and validate one component option.</summary>
/// <param name="Key">Stable option key used in JSON and validation paths.</param>
/// <param name="Label">Human-facing English label.</param>
/// <param name="Help">Description displayed next to the field in the editor.</param>
/// <param name="Kind">Input type and base validation behavior.</param>
/// <param name="DefaultValue">Optional typed JSON default, not written until explicitly selected.</param>
/// <param name="Required">Whether the option must be explicitly present.</param>
/// <param name="Minimum">Inclusive lower bound for integer options.</param>
/// <param name="Maximum">Inclusive upper bound for integer options.</param>
/// <param name="Choices">Permitted values for a choice option.</param>
/// <param name="RequiredWhenKey">Optional controlling field that makes this option required.</param>
/// <param name="RequiredWhenValue">String choice value that activates the conditional requirement.</param>
public sealed record SettingsFieldDescriptor(
    string Key,
    string Label,
    string Help,
    SettingsFieldKind Kind,
    JsonElement? DefaultValue = null,
    bool Required = false,
    int? Minimum = null,
    int? Maximum = null,
    IReadOnlyList<string>? Choices = null,
    string? RequiredWhenKey = null,
    string? RequiredWhenValue = null);

/// <summary>A provider-supplied form description independent of a particular TUI framework.</summary>
/// <param name="Kind">The component family.</param>
/// <param name="Type">Stable backend/storage adapter type ID.</param>
/// <param name="DisplayName">Human-facing component name.</param>
/// <param name="Fields">Fields to render and validate.</param>
public sealed record SettingsComponentDescriptor(
    SettingsComponentKind Kind,
    string Type,
    string DisplayName,
    IReadOnlyList<SettingsFieldDescriptor> Fields);

/// <summary>Registry of component schemas shared by configuration validation and all editors.</summary>
/// <remarks>
/// Register descriptors during host startup, before serving concurrent requests.
/// A descriptor describes options; it neither connects to nor activates its component.
/// Unknown enabled or disabled types are rejected until their descriptors are registered.
/// </remarks>
public sealed class SettingsDescriptorRegistry
{
    private readonly Dictionary<string, SettingsComponentDescriptor> _descriptors =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Registers an immutable snapshot of a component's editor schema.</summary>
    /// <param name="descriptor">Metadata to register.</param>
    /// <exception cref="ArgumentException">The schema has an invalid or duplicate key.</exception>
    public void Register(SettingsComponentDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        if (!Enum.IsDefined(descriptor.Kind) || string.IsNullOrWhiteSpace(descriptor.Type) ||
            string.IsNullOrWhiteSpace(descriptor.DisplayName) || descriptor.Fields is null)
            throw new ArgumentException("Component descriptor identity and fields are required.", nameof(descriptor));

        var fieldKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var field in descriptor.Fields)
        {
            if (field is null || string.IsNullOrWhiteSpace(field.Key) ||
                !string.Equals(field.Key, field.Key.Trim(), StringComparison.Ordinal) ||
                string.IsNullOrWhiteSpace(field.Label) || string.IsNullOrWhiteSpace(field.Help) ||
                !Enum.IsDefined(field.Kind) || !fieldKeys.Add(field.Key))
                throw new ArgumentException("Duplicate or malformed settings field.", nameof(descriptor));

            if (field.Minimum > field.Maximum)
                throw new ArgumentException("Field minimum exceeds maximum.", nameof(descriptor));
            if (field.Kind == SettingsFieldKind.Choice && (field.Choices is null || field.Choices.Count == 0 ||
                field.Choices.Any(string.IsNullOrWhiteSpace) ||
                field.Choices.Distinct(StringComparer.Ordinal).Count() != field.Choices.Count))
                throw new ArgumentException("Choice field must declare distinct choices.", nameof(descriptor));

            if ((field.RequiredWhenKey is null) != (field.RequiredWhenValue is null))
                throw new ArgumentException("Conditional required fields must declare both selector and value.", nameof(descriptor));
            if (field.DefaultValue is { } value && FieldProblem(field, value) is not null)
                throw new ArgumentException("Field default does not satisfy its schema.", nameof(descriptor));
        }

        foreach (var field in descriptor.Fields.Where(x => x.RequiredWhenKey is not null))
        {
            var selector = descriptor.Fields.FirstOrDefault(x =>
                string.Equals(x.Key, field.RequiredWhenKey, StringComparison.Ordinal));
            if (selector is null || selector.Kind != SettingsFieldKind.Choice ||
                !selector.Choices!.Contains(field.RequiredWhenValue!, StringComparer.Ordinal))
                throw new ArgumentException("Conditional field references an unknown choice value.", nameof(descriptor));
        }

        var key = Identity(descriptor.Kind, descriptor.Type);
        if (_descriptors.ContainsKey(key))
            throw new ArgumentException("Component type is already registered.", nameof(descriptor));

        _descriptors.Add(key, descriptor with
        {
            Fields = Array.AsReadOnly(descriptor.Fields.Select(x => x with
            {
                Choices = x.Choices is null ? null : Array.AsReadOnly(x.Choices.ToArray())
            }).ToArray())
        });
    }

    /// <summary>Finds the metadata for a component type.</summary>
    /// <param name="kind">Component category.</param>
    /// <param name="type">Registered adapter type.</param>
    /// <returns>The descriptor, or null for an unknown type.</returns>
    public SettingsComponentDescriptor? Find(SettingsComponentKind kind, string type) =>
        string.IsNullOrWhiteSpace(type) ? null :
        _descriptors.GetValueOrDefault(Identity(kind, type));

    /// <summary>Lists all registered component schemas.</summary>
    /// <returns>A separate collection of current schemas.</returns>
    public IReadOnlyList<SettingsComponentDescriptor> GetAll() => _descriptors.Values.ToArray();

    /// <summary>Validates a component's option values against its registered metadata.</summary>
    /// <param name="kind">Component category.</param>
    /// <param name="type">Adapter type.</param>
    /// <param name="options">Stored option values.</param>
    /// <param name="path">Base JSON path for reported errors.</param>
    /// <returns>All field-path errors, or an empty collection.</returns>
    public IReadOnlyList<SettingsValidationIssue> ValidateOptions(
        SettingsComponentKind kind, string type, IReadOnlyDictionary<string, JsonElement>? options, string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var errors = new List<SettingsValidationIssue>();
        var descriptor = Find(kind, type);
        if (descriptor is null)
        {
            errors.Add(new(path + ".Type", "No settings descriptor is registered for this component type."));
            return errors;
        }

        if (options is null)
        {
            errors.Add(new(path + ".Options", "Component options cannot be null."));
            return errors;
        }

        var fields = descriptor.Fields.ToDictionary(x => x.Key, StringComparer.Ordinal);
        foreach (var option in options)
        {
            var optionPath = path + ".Options." + option.Key;
            if (!fields.TryGetValue(option.Key, out var field))
            {
                errors.Add(new(optionPath, "Unknown or incorrectly cased component option."));
                continue;
            }

            var problem = FieldProblem(field, option.Value);
            if (problem is not null)
                errors.Add(new(optionPath, problem));
        }

        foreach (var field in descriptor.Fields)
        {
            var conditional = field.RequiredWhenKey is not null &&
                options.TryGetValue(field.RequiredWhenKey, out var selectorValue) &&
                selectorValue.ValueKind == JsonValueKind.String &&
                string.Equals(selectorValue.GetString(), field.RequiredWhenValue, StringComparison.Ordinal);

            if ((field.Required || conditional) && !options.ContainsKey(field.Key))
                errors.Add(new(path + ".Options." + field.Key, "Required option is missing."));
        }

        return errors;
    }

    /// <summary>Creates the initial built-in provider and storage configuration schemas.</summary>
    /// <returns>A new registry that hosts can extend with additional adapters.</returns>
    public static SettingsDescriptorRegistry CreateDefault()
    {
        var registry = new SettingsDescriptorRegistry();
        registry.Register(new(SettingsComponentKind.TranslationBackend, "LibreTranslate", "LibreTranslate",
        [
            new("Endpoint", "Server URL", "HTTP or HTTPS address of the LibreTranslate server.",
                SettingsFieldKind.HttpEndpoint, JsonSerializer.SerializeToElement("http://localhost:5000")),
            new("AuthenticationMode", "Authentication", "Select whether this endpoint requires an API key.",
                SettingsFieldKind.Choice, JsonSerializer.SerializeToElement("None"),
                Choices: ["None", "ApiKey"]),
            new("ApiKeyReference", "API key reference",
                "Reference an environment variable (env:NAME) or secure secret entry (secret:NAME).",
                SettingsFieldKind.SecretReference, RequiredWhenKey: "AuthenticationMode",
                RequiredWhenValue: "ApiKey"),
            new("RequestTimeoutSeconds", "Request timeout", "Timeout for one translation HTTP request in seconds.",
                SettingsFieldKind.Integer, JsonSerializer.SerializeToElement(10), Minimum: 1, Maximum: 600),
            new("RequestThrottleMs", "Request throttle", "Minimum interval between requests in milliseconds.",
                SettingsFieldKind.Integer, JsonSerializer.SerializeToElement(80), Minimum: 0, Maximum: 60000),
            new("MaxRetryAttempts", "Retry attempts", "Maximum additional retry attempts after the initial call.",
                SettingsFieldKind.Integer, JsonSerializer.SerializeToElement(3), Minimum: 0, Maximum: 20)
        ]));
        registry.Register(new(SettingsComponentKind.LocalizationStorage, "FileJson", "JSON dictionary files",
        [
            new("Directory", "Dictionary directory", "Directory containing locale JSON files.",
                SettingsFieldKind.Text, JsonSerializer.SerializeToElement("Locales")),
            new("ReadOnly", "Read-only storage", "Prevent the adapter from modifying dictionary files.",
                SettingsFieldKind.Boolean, JsonSerializer.SerializeToElement(false))
        ]));
        return registry;
    }

    private static string Identity(SettingsComponentKind kind, string type) =>
        ((int)kind).ToString(System.Globalization.CultureInfo.InvariantCulture) + ":" + type.Trim();

    private static string? FieldProblem(SettingsFieldDescriptor field, JsonElement value)
    {
        switch (field.Kind)
        {
            case SettingsFieldKind.Text:
                return value.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(value.GetString())
                    ? "Expected a non-empty string." : null;
            case SettingsFieldKind.HttpEndpoint:
                if (value.ValueKind != JsonValueKind.String ||
                    !Uri.TryCreate(value.GetString(), UriKind.Absolute, out var uri) ||
                    (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) ||
                    !string.IsNullOrEmpty(uri.UserInfo))
                    return "Expected an absolute HTTP(S) URL without embedded credentials.";
                return null;
            case SettingsFieldKind.Integer:
                if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out var number))
                    return "Expected a 32-bit integer.";
                return (field.Minimum is { } min && number < min) ||
                       (field.Maximum is { } max && number > max)
                    ? "Integer is outside the permitted range." : null;
            case SettingsFieldKind.Boolean:
                return value.ValueKind is JsonValueKind.True or JsonValueKind.False
                    ? null : "Expected true or false.";
            case SettingsFieldKind.Choice:
                return value.ValueKind == JsonValueKind.String &&
                    field.Choices?.Contains(value.GetString()!, StringComparer.Ordinal) == true
                    ? null : "Value is not one of the permitted choices.";
            case SettingsFieldKind.SecretReference:
                if (value.ValueKind != JsonValueKind.String)
                    return "Expected a secret reference, not a literal credential.";
                var reference = value.GetString()!;
                return (reference.StartsWith("env:", StringComparison.Ordinal) && reference.Length > 4) ||
                       (reference.StartsWith("secret:", StringComparison.Ordinal) && reference.Length > 7)
                    ? null : "Use env:NAME or secret:NAME; never store an API key directly.";
            default:
                return "Unsupported settings field type.";
        }
    }
}
