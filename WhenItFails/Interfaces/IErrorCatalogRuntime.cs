using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Descriptors;

namespace Afrowave.Toolbox.WhenItFails.Interfaces;

/// <summary>
/// Provides high-level access to the initialized WhenItFails runtime.
/// </summary>
/// <remarks>
/// The runtime obtains the current catalog context from
/// <see cref="IErrorCatalogContextStore"/> automatically.
/// </remarks>
public interface IErrorCatalogRuntime
{
    /// <summary>
    /// Creates an error descriptor from an error identifier.
    /// </summary>
    /// <param name="errorId">Error identifier.</param>
    /// <returns>Response containing the resolved descriptor.</returns>
    Response<ErrorDescriptor> FromId(string errorId);

    /// <summary>
    /// Creates an error descriptor from an error name.
    /// </summary>
    /// <param name="errorName">Error name.</param>
    /// <returns>Response containing the resolved descriptor.</returns>
    Response<ErrorDescriptor> FromName(string errorName);

    /// <summary>
    /// Creates an error descriptor from a numeric error code.
    /// </summary>
    /// <param name="code">Numeric error code.</param>
    /// <returns>Response containing the resolved descriptor.</returns>
    Response<ErrorDescriptor> FromCode(int code);

    /// <summary>
    /// Resolves errors selected by a named profile.
    /// </summary>
    /// <param name="profileName">Raw or normalized profile name.</param>
    /// <returns>Response containing errors selected by the profile.</returns>
    Response<IReadOnlyList<ErrorDefinition>> ResolveProfile(
        string profileName);
}