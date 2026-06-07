using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Descriptors;

namespace Afrowave.Toolbox.WhenItFails.Interfaces;

/// <summary>
/// Resolves runtime error descriptors from a loaded error catalog context.
/// </summary>
public interface IErrorDescriptorResolver
{
    /// <summary>
    /// Creates an error descriptor by error identifier.
    /// </summary>
    /// <param name="context">Loaded error catalog context.</param>
    /// <param name="errorId">Error identifier.</param>
    /// <returns>Response containing the created error descriptor.</returns>
    Response<ErrorDescriptor> CreateById(
        ErrorCatalogContext? context,
        string errorId);

    /// <summary>
    /// Creates an error descriptor by error name.
    /// </summary>
    /// <param name="context">Loaded error catalog context.</param>
    /// <param name="errorName">Error name.</param>
    /// <returns>Response containing the created error descriptor.</returns>
    Response<ErrorDescriptor> CreateByName(
        ErrorCatalogContext? context,
        string errorName);

    /// <summary>
    /// Creates an error descriptor by numeric error code.
    /// </summary>
    /// <param name="context">Loaded error catalog context.</param>
    /// <param name="code">Numeric error code.</param>
    /// <returns>Response containing the created error descriptor.</returns>
    Response<ErrorDescriptor> CreateByCode(
        ErrorCatalogContext? context,
        int code);
}