using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Configuration;

namespace Afrowave.Toolbox.WhenItFails.Interfaces;

/// <summary>
/// Prepares the project-local JSON workspace used by this package.
/// </summary>
public interface IJsonsBootstrapper
{
    /// <summary>
    /// Ensures that the project-local JSON workspace exists.
    /// </summary>
    /// <param name="options">JSON workspace options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Response containing bootstrap details.</returns>
    Task<Response<JsonsBootstrapPayload>> EnsureWorkspaceAsync(
        JsonsOptions options,
        CancellationToken cancellationToken = default);
}