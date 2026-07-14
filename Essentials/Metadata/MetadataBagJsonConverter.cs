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

        Dictionary<string, string> items = new(StringComparer.OrdinalIgnoreCase);

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return new MetadataBag(items);
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Metadata contains an invalid property token.");
            }

            string key = reader.GetString() ?? string.Empty;

            if (!reader.Read())
            {
                throw new JsonException("Metadata property value is missing.");
            }

            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException($"Metadata value for '{key}' must be a string.");
            }

            string value = reader.GetString() ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(key))
            {
                items[key] = value;
            }
        }

        throw new JsonException("Metadata JSON object was not closed.");
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
}
