using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Resolution;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides dependency injection registrations for WhenItFails services.
/// </summary>
public static class WhenItFailsServiceCollectionExtensions
{
    /// <summary>
    /// Registers core WhenItFails services.
    /// </summary>
    /// <param name="services">
    /// Service collection receiving the registrations.
    /// </param>
    /// <returns>
    /// The same service collection for fluent configuration.
    /// </returns>
    public static IServiceCollection AddWhenItFails(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<
            IErrorProfileResolver,
            ErrorProfileResolver>();

        services.TryAddSingleton<
            IErrorProfileSelectionService,
            ErrorProfileSelectionService>();

        return services;
    }
}