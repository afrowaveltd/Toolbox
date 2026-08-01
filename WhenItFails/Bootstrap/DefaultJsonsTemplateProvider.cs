using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Bootstrap;

/// <summary>
/// Provides default JSON template files used to initialize the project-local WhenItFails workspace.
/// </summary>
/// <remarks>
/// The default templates are embedded from the authoritative catalogs under
/// <c>Jsons/WhenItFails</c>. Error IDs and names are normalized to the established
/// bootstrap representation so existing bootstrap consumers retain their contract.
/// </remarks>
public sealed class DefaultJsonsTemplateProvider : IJsonsTemplateProvider
{
    private const string ResourcePrefix =
        "Afrowave.Toolbox.WhenItFails.Bootstrap.Templates.";

    /// <inheritdoc />
    public IReadOnlyList<JsonsTemplateFile> GetTemplateFiles(JsonsOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return
        [
            CreateTemplateFile(
                "Error catalog",
                options.ErrorCatalogFileName,
                "errors.en.json",
                normalizeErrorCatalog: true),
            CreateTemplateFile(
                "Category catalog",
                options.CategoryCatalogFileName,
                "categories.en.json"),
            CreateTemplateFile(
                "Code group catalog",
                options.CodeGroupCatalogFileName,
                "code-groups.en.json"),
            CreateTemplateFile(
                "Owner catalog",
                options.OwnerCatalogFileName,
                "owners.en.json"),
            CreateTemplateFile(
                "Profiles",
                options.ProfilesFileName,
                "profiles.json")
        ];
    }

    private static JsonsTemplateFile CreateTemplateFile(
        string name,
        string targetFileName,
        string resourceFileName,
        bool normalizeErrorCatalog = false)
    {
        string content = ReadEmbeddedCatalog(resourceFileName);

        if (normalizeErrorCatalog)
        {
            content = NormalizeErrorCatalog(content);
        }

        return new JsonsTemplateFile
        {
            Name = name,
            TargetFileName = targetFileName,
            Content = content
        };
    }

    private static string ReadEmbeddedCatalog(string resourceFileName)
    {
        string resourceName = ResourcePrefix + resourceFileName;
        System.Reflection.Assembly assembly = typeof(DefaultJsonsTemplateProvider).Assembly;
        Stream? stream = assembly.GetManifestResourceStream(resourceName);

        if (stream is null)
        {
            string[] availableResourceNames = assembly.GetManifestResourceNames();
            string availableResources = availableResourceNames.Length == 0
                ? "<none>"
                : string.Join(", ", availableResourceNames.OrderBy(name => name, StringComparer.Ordinal));

            throw new InvalidOperationException(
                $"The embedded WhenItFails catalog resource '{resourceName}' was not found. " +
                $"Available manifest resources: {availableResources}.");
        }

        using (stream)
        using (StreamReader reader = new(
            stream,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true))
        {
            return reader.ReadToEnd();
        }
    }

    private static string NormalizeErrorCatalog(string content)
    {
        JsonNode root = JsonNode.Parse(content)
            ?? throw new InvalidOperationException(
                "The embedded WhenItFails error catalog is empty.");

        JsonArray errors = root["errors"] as JsonArray
            ?? throw new InvalidOperationException(
                "The embedded WhenItFails error catalog does not contain an errors array.");

        foreach (JsonNode? node in errors)
        {
            if (node is not JsonObject error)
            {
                continue;
            }

            string? id = error["id"]?.GetValue<string>();
            if (!string.IsNullOrWhiteSpace(id))
            {
                error["id"] = id.Replace('_', '-');
            }

            string? documentationKey = error["documentationKey"]?.GetValue<string>();
            string? bootstrapName = CreateBootstrapName(documentationKey);
            if (!string.IsNullOrWhiteSpace(bootstrapName))
            {
                error["name"] = bootstrapName;
            }
        }

        return root.ToJsonString(new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    private static string? CreateBootstrapName(string? documentationKey)
    {
        if (string.IsNullOrWhiteSpace(documentationKey))
        {
            return null;
        }

        string slug = documentationKey
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .LastOrDefault() ?? string.Empty;

        string[] words = slug.Split(
            '-',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (words.Length == 0)
        {
            return null;
        }

        StringBuilder builder = new();

        foreach (string word in words)
        {
            builder.Append(char.ToUpperInvariant(word[0]));

            if (word.Length > 1)
            {
                builder.Append(word.AsSpan(1).ToString().ToLowerInvariant());
            }
        }

        return builder.ToString();
    }
}
