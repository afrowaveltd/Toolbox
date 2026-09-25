using System.Collections.ObjectModel;
using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Runtime;

/// <summary>Detached, getter-only view of a supporting error code group.</summary>
public sealed class ErrorCodeGroupDefinitionSnapshot
{
    internal ErrorCodeGroupDefinitionSnapshot(ErrorCodeGroupDefinition source)
    {
        ArgumentNullException.ThrowIfNull(source);

        Name = source.Name;
        DisplayName = source.DisplayName;
        CodePrefix = source.CodePrefix;
        CodeFrom = source.CodeFrom;
        CodeTo = source.CodeTo;
        Description = source.Description;
        DefaultCategories = Array.AsReadOnly(source.DefaultCategories.ToArray());
        DefaultTags = Array.AsReadOnly(source.DefaultTags.ToArray());
        DefaultMappings = new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>(
                source.DefaultMappings, source.DefaultMappings.Comparer));
        Metadata = ErrorCategoryDefinitionSnapshot.CopyMetadata(source.Metadata);
    }

    /// <summary>Gets the captured group identifier.</summary>
    public string Name { get; }

    /// <summary>Gets the captured display name.</summary>
    public string DisplayName { get; }

    /// <summary>Gets the captured code prefix.</summary>
    public string CodePrefix { get; }

    /// <summary>Gets the captured numeric code range start.</summary>
    public int CodeFrom { get; }

    /// <summary>Gets the captured numeric code range end.</summary>
    public int CodeTo { get; }

    /// <summary>Gets the optional captured description.</summary>
    public string? Description { get; }

    /// <summary>Gets detached, read-only recommended categories.</summary>
    public IReadOnlyList<string> DefaultCategories { get; }

    /// <summary>Gets detached, read-only recommended tags.</summary>
    public IReadOnlyList<string> DefaultTags { get; }

    /// <summary>Gets detached, read-only default behavior mappings.</summary>
    public IReadOnlyDictionary<string, string> DefaultMappings { get; }

    /// <summary>Gets detached, read-only group metadata.</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; }
}
