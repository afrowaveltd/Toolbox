using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Descriptors;

namespace Afrowave.Toolbox.WhenItFails.Interfaces;

/// <summary>
/// Provides simple high-level access to runtime error descriptors.
/// </summary>
public interface IErrorDescriptorService
{
    /// <summary>
    /// Creates an error descriptor from an error identifier.
    /// </summary>
    /// <param name="context">Loaded error catalog context.</param>
    /// <param name="errorId">Error identifier.</param>
    /// <returns>Response containing the error descriptor.</returns>
    Response<ErrorDescriptor> FromId(
        ErrorCatalogContext? context,
        string errorId);

    /// <summary>
    /// Creates an error descriptor from an error name.
    /// </summary>
    /// <param name="context">Loaded error catalog context.</param>
    /// <param name="errorName">Error name.</param>
    /// <returns>Response containing the error descriptor.</returns>
    Response<ErrorDescriptor> FromName(
        ErrorCatalogContext? context,
        string errorName);

    /// <summary>
    /// Creates an error descriptor from a numeric error code.
    /// </summary>
    /// <param name="context">Loaded error catalog context.</param>
    /// <param name="code">Numeric error code.</param>
    /// <returns>Response containing the error descriptor.</returns>
    Response<ErrorDescriptor> FromCode(
        ErrorCatalogContext? context,
        int code);
}