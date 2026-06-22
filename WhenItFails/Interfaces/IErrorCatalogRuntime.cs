using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Descriptors;
using Afrowave.Toolbox.WhenItFails.Initialization;

namespace Afrowave.Toolbox.WhenItFails.Interfaces;

/// <summary>
/// Provides high-level access to the complete WhenItFails runtime.
/// </summary>
/// <remarks>
/// This interface is the main entry point for applications that want to
/// initialize WhenItFails and use the currently loaded catalog context
/// without passing that context manually to individual services.
/// </remarks>
public interface IErrorCatalogRuntime
{
    /// <summary>
    /// Initializes the complete WhenItFails catalog runtime
    /// using the registered configuration.
    /// </summary>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// Response containing bootstrap information and the initialized context.
    /// </returns>
    Task<Response<ErrorCatalogInitializationPayload>> InitializeAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Initializes the complete WhenItFails catalog runtime
    /// using the supplied JSON workspace configuration.
    /// </summary>
    /// <param name="options">
    /// JSON workspace configuration overriding the registered workspace
    /// configuration for this initialization call.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// Response containing bootstrap information and the initialized context.
    /// </returns>
    Task<Response<ErrorCatalogInitializationPayload>> InitializeAsync(
        JsonsOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces the currently active catalog context with the bundled
    /// Afrowave default catalog context.
    /// </summary>
    /// <remarks>
    /// This operation does not overwrite or otherwise modify the
    /// project-local JSON catalog files.
    /// </remarks>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// Response containing the activated built-in catalog context.
    /// </returns>
    Task<Response<ErrorCatalogInitializationPayload>> ResetToDefaultsAsync(
        CancellationToken cancellationToken = default);

/// <summary>
/// Gets the currently active error catalog context.
/// </summary>
/// <returns>
/// Response containing the active context, or a failure response
/// when the runtime has not been initialized yet.
/// </returns>
Response<ErrorCatalogContext> GetCurrentContext();

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