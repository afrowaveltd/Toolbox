using System.Collections.ObjectModel;
using Afrowave.Toolbox.Essentials.Metadata;
using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Runtime;

/// <summary>
/// Detached, read-only projection of one supporting category definition.
/// </summary>
public sealed class ErrorCategoryDefinitionSnapshot
{
    internal ErrorCategoryDefinitionSnapshot(ErrorCategoryDefinition source)
    {
        ArgumentNullException.ThrowIfNull(source);

        Name = source.Name;
        DisplayName = source.DisplayName;
        Description = source.Description;
        Aliases = Array.AsReadOnly(source.Aliases.ToArray());
        ParentCategories = Array.AsReadOnly(source.ParentCategories.ToArray());
        DefaultTags = Array.AsReadOnly(source.DefaultTags.ToArray());
        DefaultMappings = new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>(
                source.DefaultMappings, source.DefaultMappings.Comparer));
        Metadata = CopyMetadata(source.Metadata);
    }

    /// <summary>Gets the captured normalized category name.</summary>
    public string Name { get; }

    /// <summary>Gets the captured human-readable category name.</summary>
    public string DisplayName { get; }

    /// <summary>Gets the optional captured description.</summary>
    public string? Description { get; }

    /// <summary>Gets detached category aliases.</summary>
    public IReadOnlyList<string> Aliases { get; }

    /// <summary>Gets detached parent-category identifiers.</summary>
    public IReadOnlyList<string> ParentCategories { get; }

    /// <summary>Gets detached recommended tags.</summary>
    public IReadOnlyList<string> DefaultTags { get; }

    /// <summary>Gets detached, read-only default behavior mappings.</summary>
    public IReadOnlyDictionary<string, string> DefaultMappings { get; }

    /// <summary>Gets detached, read-only category metadata.</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; }

    internal static IReadOnlyDictionary<string, string> CopyMetadata(
        MetadataBag source)
    {
        ArgumentNullException.ThrowIfNull(source);

        Dictionary<string, string> values =
            new(StringComparer.OrdinalIgnoreCase);

        foreach (KeyValuePair<string, string> item in source.Items)
        {
            values.Add(item.Key, item.Value);
        }

        return new ReadOnlyDictionary<string, string>(values);
    }
}
