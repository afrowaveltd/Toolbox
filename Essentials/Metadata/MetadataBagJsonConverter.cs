using System.Text.Json;
using System.Text.Json.Serialization;

namespace Afrowave.Toolbox.Essentials.Metadata;

/// <summary>
/// Serializes <see cref="MetadataBag"/> as a plain JSON object of string values.
/// </summary>
public sealed class MetadataBagJsonConverter : JsonConverter<MetadataBag>
{
    /// <inheritdoc />
    public override MetadataBag Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return new MetadataBag();
        }

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Metadata must be a JSON object.");
        }

        using JsonDocument document = JsonDocument.ParseValue(ref reader);
        JsonElement root = document.RootElement;

        if (TryGetLegacyItems(root, out JsonElement legacyItems))
        {
            return ReadItems(legacyItems);
        }

        return ReadItems(root);
    }

    /// <inheritdoc />
    public override void Write(
        Utf8JsonWriter writer,
        MetadataBag value,
        JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);

        writer.WriteStartObject();

        foreach (KeyValuePair<string, string> item in value.Items
            .OrderBy(item => item.Key, StringComparer.OrdinalIgnoreCase))
        {
            writer.WriteString(item.Key, item.Value);
        }

        writer.WriteEndObject();
    }

    private static bool TryGetLegacyItems(
        JsonElement root,
        out JsonElement items)
    {
        items = default;

        return root.TryGetProperty("Count", out JsonElement count)
            && count.ValueKind == JsonValueKind.Number
            && root.TryGetProperty("IsEmpty", out JsonElement isEmpty)
            && (isEmpty.ValueKind == JsonValueKind.True
                || isEmpty.ValueKind == JsonValueKind.False)
            && root.TryGetProperty("Items", out items)
            && items.ValueKind == JsonValueKind.Object;
    }

    private static MetadataBag ReadItems(JsonElement itemsElement)
    {
        Dictionary<string, string> items = new(StringComparer.OrdinalIgnoreCase);

        foreach (JsonProperty property in itemsElement.EnumerateObject())
        {
            if (property.Value.ValueKind != JsonValueKind.String)
            {
                throw new JsonException(
                    $"Metadata value for '{property.Name}' must be a string.");
            }

            if (!string.IsNullOrWhiteSpace(property.Name))
            {
                items[property.Name] = property.Value.GetString() ?? string.Empty;
            }
        }

        return new MetadataBag(items);
    }
}
