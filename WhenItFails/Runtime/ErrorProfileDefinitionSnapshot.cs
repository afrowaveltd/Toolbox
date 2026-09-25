using System.Collections.ObjectModel;
using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Runtime;

/// <summary>Detached, getter-only projection of a supporting error profile definition.</summary>
public sealed class ErrorProfileDefinitionSnapshot
{
    internal ErrorProfileDefinitionSnapshot(ErrorProfileDefinition source)
    {
        ArgumentNullException.ThrowIfNull(source);

        Name = source.Name;
        DisplayName = source.DisplayName;
        Description = source.Description;
        Source = source.Source;
        IncludeOwners = Array.AsReadOnly(source.IncludeOwners.ToArray());
        IncludeCodeGroups = Array.AsReadOnly(source.IncludeCodeGroups.ToArray());
        IncludeCategories = Array.AsReadOnly(source.IncludeCategories.ToArray());
        IncludeSubcategories = Array.AsReadOnly(source.IncludeSubcategories.ToArray());
        IncludeTags = Array.AsReadOnly(source.IncludeTags.ToArray());
        IncludeErrors = Array.AsReadOnly(source.IncludeErrors.ToArray());
        ExcludeTags = Array.AsReadOnly(source.ExcludeTags.ToArray());
        ExcludeErrors = Array.AsReadOnly(source.ExcludeErrors.ToArray());
        DefaultMappings = new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>(
                source.DefaultMappings, source.DefaultMappings.Comparer));
        Metadata = ErrorCategoryDefinitionSnapshot.CopyMetadata(source.Metadata);
    }

    /// <summary>Gets the captured profile identifier.</summary>
    public string Name { get; }

    /// <summary>Gets the captured human-readable name.</summary>
    public string DisplayName { get; }

    /// <summary>Gets the optional captured description.</summary>
    public string? Description { get; }

    /// <summary>Gets the captured profile source.</summary>
    public string Source { get; }

    /// <summary>Gets detached included owners.</summary>
    public IReadOnlyList<string> IncludeOwners { get; }

    /// <summary>Gets detached included code groups.</summary>
    public IReadOnlyList<string> IncludeCodeGroups { get; }

    /// <summary>Gets detached included categories.</summary>
    public IReadOnlyList<string> IncludeCategories { get; }

    /// <summary>Gets detached included subcategories.</summary>
    public IReadOnlyList<string> IncludeSubcategories { get; }

    /// <summary>Gets detached included tags.</summary>
    public IReadOnlyList<string> IncludeTags { get; }

    /// <summary>Gets detached explicitly included error identifiers.</summary>
    public IReadOnlyList<string> IncludeErrors { get; }

    /// <summary>Gets detached excluded tags.</summary>
    public IReadOnlyList<string> ExcludeTags { get; }

    /// <summary>Gets detached explicitly excluded error identifiers.</summary>
    public IReadOnlyList<string> ExcludeErrors { get; }

    /// <summary>Gets detached default behavior mappings.</summary>
    public IReadOnlyDictionary<string, string> DefaultMappings { get; }

    /// <summary>Gets detached profile metadata.</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; }
}
