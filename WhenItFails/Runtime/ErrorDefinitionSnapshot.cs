using System.Collections.ObjectModel;
using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Runtime;

/// <summary>
/// Detached, read-only representation of one indexed error definition.
/// </summary>
/// <remarks>
/// Scalar fields, category/tag collections and metadata are captured at construction.
/// The snapshot does not retain the mutable source definition or its MetadataBag.
/// </remarks>
public sealed class ErrorDefinitionSnapshot
{
    internal ErrorDefinitionSnapshot(ErrorDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        Id = definition.Id;
        Code = definition.Code;
        Name = definition.Name;
        Owner = definition.Owner;
        CodePrefix = definition.CodePrefix;
        CodeGroup = definition.CodeGroup;
        PrimaryCategory = definition.PrimaryCategory;
        Categories = CopyList(definition.Categories);
        Subcategories = CopyList(definition.Subcategories);
        Title = definition.Title;
        Message = definition.Message;
        DefaultSeverity = definition.DefaultSeverity;
        DeveloperHint = definition.DeveloperHint;
        DocumentationKey = definition.DocumentationKey;
        Tags = CopyList(definition.Tags);

        ArgumentNullException.ThrowIfNull(definition.Metadata);

        Dictionary<string, string> copiedMetadata =
            new(StringComparer.OrdinalIgnoreCase);

        foreach (KeyValuePair<string, string> entry in definition.Metadata.Items)
        {
            copiedMetadata.Add(entry.Key, entry.Value);
        }

        Metadata = new ReadOnlyDictionary<string, string>(copiedMetadata);
    }

    /// <summary>Gets the captured stable error identifier.</summary>
    public string Id { get; }

    /// <summary>Gets the captured numeric code.</summary>
    public int Code { get; }

    /// <summary>Gets the captured machine-friendly name.</summary>
    public string Name { get; }

    /// <summary>Gets the captured owner.</summary>
    public string Owner { get; }

    /// <summary>Gets the captured code prefix.</summary>
    public string CodePrefix { get; }

    /// <summary>Gets the captured code group.</summary>
    public string CodeGroup { get; }

    /// <summary>Gets the captured primary category.</summary>
    public string PrimaryCategory { get; }

    /// <summary>Gets a detached read-only list of captured categories.</summary>
    public IReadOnlyList<string> Categories { get; }

    /// <summary>Gets a detached read-only list of captured subcategories.</summary>
    public IReadOnlyList<string> Subcategories { get; }

    /// <summary>Gets the captured title.</summary>
    public string Title { get; }

    /// <summary>Gets the captured message.</summary>
    public string Message { get; }

    /// <summary>Gets the captured default severity.</summary>
    public string DefaultSeverity { get; }

    /// <summary>Gets the captured developer hint, if present.</summary>
    public string? DeveloperHint { get; }

    /// <summary>Gets the captured documentation key, if present.</summary>
    public string? DocumentationKey { get; }

    /// <summary>Gets a detached read-only list of captured tags.</summary>
    public IReadOnlyList<string> Tags { get; }

    /// <summary>Gets detached, read-only string metadata.</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; }

    private static IReadOnlyList<string> CopyList(IReadOnlyList<string> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        return Array.AsReadOnly(values.ToArray());
    }
}
