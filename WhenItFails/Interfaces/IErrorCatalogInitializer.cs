using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Initialization;

namespace Afrowave.Toolbox.WhenItFails.Interfaces;

/// <summary>
/// Initializes the complete WhenItFails catalog runtime.
/// </summary>
public interface IErrorCatalogInitializer
{
   /// <summary>
   /// Prepares the JSON workspace, loads all catalogs,
   /// validates their combined state and stores the resulting context.
   /// </summary>
   /// <param name="options">JSON workspace configuration.</param>
   /// <param name="cancellationToken">Cancellation token.</param>
   /// <returns>
   /// Response containing bootstrap information and the initialized context.
   /// </returns>
   Task<Response<ErrorCatalogInitializationPayload>> InitializeAsync(
       JsonsOptions options,
       CancellationToken cancellationToken = default);
}