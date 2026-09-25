using System.Collections.ObjectModel;
using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Runtime;

/// <summary>
/// Detached, getter-only projection of an error owner definition.
/// </summary>
public sealed class ErrorOwnerDefinitionSnapshot
{
    internal ErrorOwnerDefinitionSnapshot(ErrorOwnerDefinition source)
    {
        ArgumentNullException.ThrowIfNull(source);

        Name = source.Name;
        DisplayName = source.DisplayName;
        Description = source.Description;
        CodeFrom = source.CodeFrom;
        CodeTo = source.CodeTo;
        IsBuiltIn = source.IsBuiltIn;
        Aliases = Array.AsReadOnly(source.Aliases.ToArray());
        DefaultMappings = new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>(
                source.DefaultMappings, source.DefaultMappings.Comparer));
        Metadata = ErrorCategoryDefinitionSnapshot.CopyMetadata(source.Metadata);
    }

    /// <summary>Gets the captured owner identifier.</summary>
    public string Name { get; }

    /// <summary>Gets the captured display name.</summary>
    public string DisplayName { get; }

    /// <summary>Gets the optional captured description.</summary>
    public string? Description { get; }

    /// <summary>Gets the beginning of the captured numeric code range.</summary>
    public int CodeFrom { get; }

    /// <summary>Gets the end of the captured numeric code range.</summary>
    public int CodeTo { get; }

    /// <summary>Gets the captured built-in flag.</summary>
    public bool IsBuiltIn { get; }

    /// <summary>Gets independently copied, read-only aliases.</summary>
    public IReadOnlyList<string> Aliases { get; }

    /// <summary>Gets independently copied, read-only default mappings.</summary>
    public IReadOnlyDictionary<string, string> DefaultMappings { get; }

    /// <summary>Gets independently copied, read-only metadata.</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; }
}
