using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Descriptors;

namespace Afrowave.Toolbox.WhenItFails.Interfaces;

/// <summary>
/// Creates runtime error descriptors from catalog error definitions.
/// </summary>
public interface IErrorDescriptorFactory
{
    /// <summary>
    /// Creates an error descriptor from the specified error definition.
    /// </summary>
    /// <param name="definition">Catalog error definition.</param>
    /// <returns>Runtime error descriptor.</returns>
    ErrorDescriptor Create(ErrorDefinition definition);
}