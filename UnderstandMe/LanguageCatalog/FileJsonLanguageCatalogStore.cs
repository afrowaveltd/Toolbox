using System.Text.Json;
using System.Text.Json.Nodes;

namespace Afrowave.Toolbox.UnderstandMe.LanguageCatalog;

/// <summary>
/// Standalone JSON adapter for the existing Afutrans languages.json array schema.
/// Reading and editing are independent capabilities exposed through interfaces.
/// </summary>
/// <remarks>
/// Synchronized within one instance, not across separate processes. External changes should
/// use a separate revision-aware provider when multiple writers need transactional guarantees.
/// </remarks>
public sealed class FileJsonLanguageCatalogStore : ILanguageCatalogReader, ILanguageCatalogEditor
{
    private readonly string _path;
    private readonly object _gate = new();
    private static readonly JsonSerializerOptions WriteOptions = new() { WriteIndented = true };

    /// <summary>Uses the requested JSON file; a missing file represents an empty catalog.</summary>
    public FileJsonLanguageCatalogStore(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _path = Path.GetFullPath(path);
    }

    /// <inheritdoc/>
    public IReadOnlyList<LanguageCatalogEntry> GetAll()
    {
        lock (_gate)
        {
            var root = ReadValidated();
            var list = root.Select(node => Parse((JsonObject)node!))
                .OrderBy(language => language.Code, StringComparer.OrdinalIgnoreCase)
                .ThenBy(language => language.Code, StringComparer.Ordinal)
                .ToArray();
            return Array.AsReadOnly(list);
        }
    }

    /// <inheritdoc/>
    public LanguageCatalogEntry? Find(string code)
    {
        ValidateCode(code);
        lock (_gate)
        {
            var root = ReadValidated();
            foreach (JsonNode? node in root)
            {
                var language = Parse((JsonObject)node!);
                if (StringComparer.OrdinalIgnoreCase.Equals(language.Code, code))
                    return language;
            }
            return null;
        }
    }

    /// <inheritdoc/>
    public bool Upsert(LanguageCatalogEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ValidateEntry(entry);

        lock (_gate)
        {
            var root = ReadValidated();
            JsonObject? existing = null;
            foreach (JsonNode? node in root)
            {
                var candidate = (JsonObject)node!;
                if (StringComparer.OrdinalIgnoreCase.Equals(
                    Parse(candidate).Code, entry.Code))
                {
                    existing = candidate;
                    break;
                }
            }

            if (existing is not null)
            {
                var previous = Parse(existing);
                if (previous.Name == entry.Name &&
                    previous.NativeName == entry.NativeName &&
                    previous.RightToLeft == entry.RightToLeft)
                    return false;

                // Keep the existing canonical code and any vendor/custom metadata fields.
                existing["name"] = entry.Name;
                existing["native"] = entry.NativeName;
                if (entry.RightToLeft is bool rtl)
                    existing["rtl"] = rtl;
                else
                    existing.Remove("rtl");
            }
            else
            {
                var created = new JsonObject
                {
                    ["code"] = entry.Code,
                    ["name"] = entry.Name,
                    ["native"] = entry.NativeName
                };
                if (entry.RightToLeft is bool rtl)
                    created["rtl"] = rtl;
                root.Add(created);
            }

            WriteSorted(root);
            return true;
        }
    }

    /// <inheritdoc/>
    public bool Remove(string code)
    {
        ValidateCode(code);
        lock (_gate)
        {
            var root = ReadValidated();
            for (int i = 0; i < root.Count; i++)
            {
                if (StringComparer.OrdinalIgnoreCase.Equals(
                    Parse((JsonObject)root[i]!).Code, code))
                {
                    root.RemoveAt(i);
                    WriteSorted(root);
                    return true;
                }
            }
            return false;
        }
    }

    private JsonArray ReadValidated()
    {
        if (!File.Exists(_path))
            return new JsonArray();

        var root = JsonNode.Parse(File.ReadAllText(_path)) as JsonArray
            ?? throw new JsonException("Language catalog must be an array of language objects.");

        var codes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (JsonNode? node in root)
        {
            if (node is not JsonObject obj)
                throw new JsonException("Each language must be a JSON object.");

            LanguageCatalogEntry language = Parse(obj);
            ValidateCode(language.Code);
            if (!codes.Add(language.Code))
                throw new JsonException("Duplicate language identifier: " + language.Code);
        }
        return root;
    }

    private static LanguageCatalogEntry Parse(JsonObject obj)
    {
        string code = ReadString(obj, "code");
        string name = ReadString(obj, "name");
        string native = ReadString(obj, "native");
        bool? rightToLeft = null;
        if (obj["rtl"] is JsonNode direction)
        {
            if (direction is not JsonValue jsonValue ||
                !jsonValue.TryGetValue<bool>(out bool value))
                throw new JsonException("The optional rtl field must be a boolean.");
            rightToLeft = value;
        }

        return new LanguageCatalogEntry
        {
            Code = code,
            Name = name,
            NativeName = native,
            RightToLeft = rightToLeft
        };
    }

    private static string ReadString(JsonObject obj, string property)
    {
        if (obj[property] is not JsonValue value ||
            !value.TryGetValue<string>(out string? text) ||
            string.IsNullOrWhiteSpace(text))
            throw new JsonException("Missing or invalid language field: " + property);
        return text;
    }

    private static void ValidateEntry(LanguageCatalogEntry entry)
    {
        ValidateCode(entry.Code);
        ArgumentException.ThrowIfNullOrWhiteSpace(entry.Name);
        ArgumentException.ThrowIfNullOrWhiteSpace(entry.NativeName);
    }

    private static void ValidateCode(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        if (code.StartsWith('-') || code.EndsWith('-') ||
            code.Split('-').Any(part => part.Length is < 1 or > 8 ||
                !part.All(char.IsAsciiLetterOrDigit)))
            throw new ArgumentException("Invalid language identifier.", nameof(code));
        // This validates a safe tag shape, not the entire BCP 47 grammar.
    }

    private void WriteSorted(JsonArray root)
    {
        var sorted = root.Select(node => (JsonObject)node!)
            .OrderBy(obj => ReadString(obj, "code"), StringComparer.OrdinalIgnoreCase)
            .ThenBy(obj => ReadString(obj, "code"), StringComparer.Ordinal)
            .Select(obj => (JsonNode?)obj.DeepClone())
            .ToArray();

        var result = new JsonArray(sorted);
        string directory = Path.GetDirectoryName(_path)!;
        Directory.CreateDirectory(directory);
        string tempPath = Path.Combine(directory, "." + Path.GetFileName(_path) +
            "." + Guid.NewGuid().ToString("N") + ".tmp");

        try
        {
            File.WriteAllText(tempPath, result.ToJsonString(WriteOptions) + Environment.NewLine);
            File.Move(tempPath, _path, overwrite: true);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }
}
