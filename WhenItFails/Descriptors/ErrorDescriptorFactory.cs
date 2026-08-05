using Afrowave.Toolbox.Essentials.Metadata;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Descriptors;

/// <summary>
/// Default implementation that creates runtime error descriptors from catalog definitions.
/// </summary>
public sealed class ErrorDescriptorFactory : IErrorDescriptorFactory
{
    /// <inheritdoc />
    public ErrorDescriptor Create(ErrorDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        return new ErrorDescriptor
        {
            Id = definition.Id,
            Code = definition.Code,
            Name = definition.Name,

            Owner = definition.Owner,
            CodePrefix = definition.CodePrefix,
            CodeGroup = definition.CodeGroup,

            PrimaryCategory = definition.PrimaryCategory,
            Categories = definition.Categories is null
                ? []
                : [.. definition.Categories],
            Subcategories = definition.Subcategories is null
                ? []
                : [.. definition.Subcategories],

            Title = definition.Title,
            Message = definition.Message,
            Severity = definition.DefaultSeverity,

            DeveloperHint = definition.DeveloperHint,
            DocumentationKey = definition.DocumentationKey,

            Tags = definition.Tags is null
                ? []
                : [.. definition.Tags],
            Metadata = definition.Metadata ?? new MetadataBag()
        };
    }
}