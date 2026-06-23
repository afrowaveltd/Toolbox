using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Descriptors;
using Afrowave.Toolbox.WhenItFails.Initialization;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Resolution;
using Afrowave.Toolbox.WhenItFails.Services;
using Afrowave.Toolbox.WhenItFails.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;


namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides dependency injection registrations for WhenItFails services.
/// </summary>
public static class WhenItFailsServiceCollectionExtensions
{
    /// <summary>
    /// Registers the default WhenItFails runtime services
    /// with the default configuration.
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

        services.TryAddSingleton(
            new WhenItFailsOptions());

        return RegisterWhenItFailsServices(services);
    }

    /// <summary>
    /// Registers the default WhenItFails runtime services
    /// using the supplied configuration.
    /// </summary>
    /// <param name="services">
    /// Service collection receiving the registrations.
    /// </param>
    /// <param name="options">
    /// Configuration used by the WhenItFails runtime.
    /// </param>
    /// <returns>
    /// The same service collection for fluent configuration.
    /// </returns>
    public static IServiceCollection AddWhenItFails(
        this IServiceCollection services,
        WhenItFailsOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        WhenItFailsOptions optionsSnapshot =
            CreateOptionsSnapshot(options);

        services.Replace(
            ServiceDescriptor.Singleton(optionsSnapshot));

        return RegisterWhenItFailsServices(services);
    }

    /// <summary>
    /// Registers the default WhenItFails runtime services
    /// using configuration values from the supplied section.
    /// </summary>
    /// <param name="services">
    /// Service collection receiving the registrations.
    /// </param>
    /// <param name="configurationSection">
    /// Configuration section containing WhenItFails settings.
    /// </param>
    /// <returns>
    /// The same service collection for fluent configuration.
    /// </returns>
    public static IServiceCollection AddWhenItFails(
        this IServiceCollection services,
        IConfigurationSection configurationSection)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configurationSection);

        WhenItFailsOptions configuredOptions = new();

        configurationSection.Bind(
            configuredOptions);

        return services.AddWhenItFails(
            configuredOptions);
    }

    /// <summary>
    /// Registers the default WhenItFails runtime services
    /// and applies configuration supplied by code.
    /// </summary>
    /// <param name="services">
    /// Service collection receiving the registrations.
    /// </param>
    /// <param name="configure">
    /// Configuration action applied to a new
    /// <see cref="WhenItFailsOptions"/> instance.
    /// </param>
    /// <returns>
    /// The same service collection for fluent configuration.
    /// </returns>
    public static IServiceCollection AddWhenItFails(
        this IServiceCollection services,
        Action<WhenItFailsOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        WhenItFailsOptions configuredOptions = new();

        configure(configuredOptions);

        return services.AddWhenItFails(
            configuredOptions);
    }

    private static IServiceCollection RegisterWhenItFailsServices(
        IServiceCollection services)
    {
        RegisterBootstrapServices(services);
        RegisterCatalogServices(services);
        RegisterResolutionServices(services);
        RegisterDescriptorServices(services);

        return services;
    }
    private static void RegisterBootstrapServices(
        IServiceCollection services)
    {
        services.TryAddSingleton<
            IJsonsTemplateProvider,
            DefaultJsonsTemplateProvider>();

        services.TryAddSingleton<
            IJsonsBootstrapper,
            JsonsBootstrapper>();
    }

    private static void RegisterCatalogServices(
        IServiceCollection services)
    {
        RegisterMainErrorCatalogServices(services);
        RegisterCategoryCatalogServices(services);
        RegisterCodeGroupCatalogServices(services);
        RegisterOwnerCatalogServices(services);
        RegisterProfileCatalogServices(services);

        services.TryAddSingleton<
            IErrorCatalogContextProvider,
            ErrorCatalogContextProvider>();

        services.TryAddSingleton<
   IBuiltInErrorCatalogContextProvider,
   BuiltInErrorCatalogContextProvider>();

        services.TryAddSingleton<
            IErrorCatalogContextStore,
            ErrorCatalogContextStore>();

        services.TryAddSingleton<
            IErrorCatalogInitializer,
           ErrorCatalogInitializer>();
    }

    private static void RegisterMainErrorCatalogServices(
        IServiceCollection services)
    {
        services.TryAddSingleton<
            IErrorCatalogLoader,
            JsonErrorCatalogLoader>();

        services.TryAddSingleton<
            IErrorCatalogDocumentNormalizer,
            ErrorCatalogDocumentNormalizer>();

        services.TryAddSingleton<
            IErrorCatalogValidator,
            ErrorCatalogValidator>();

        services.TryAddSingleton<
            IErrorCatalogFactory,
            ErrorCatalogFactory>();

        services.TryAddSingleton<
            IErrorCatalogProvider,
            ErrorCatalogProvider>();
    }

    private static void RegisterCategoryCatalogServices(
        IServiceCollection services)
    {
        services.TryAddSingleton<
            IErrorCategoryCatalogLoader,
            JsonErrorCategoryCatalogLoader>();

        services.TryAddSingleton<
            ErrorCategoryCatalogDocumentNormalizer>();

        services.TryAddSingleton<
            IErrorCategoryCatalogValidator,
            ErrorCategoryCatalogValidator>();

        services.TryAddSingleton<
            IErrorCategoryCatalogProvider,
            ErrorCategoryCatalogProvider>();
    }

    private static void RegisterCodeGroupCatalogServices(
        IServiceCollection services)
    {
        services.TryAddSingleton<
            IErrorCodeGroupCatalogLoader,
            JsonErrorCodeGroupCatalogLoader>();

        services.TryAddSingleton<
            ErrorCodeGroupCatalogDocumentNormalizer>();

        services.TryAddSingleton<
            IErrorCodeGroupCatalogValidator,
            ErrorCodeGroupCatalogValidator>();

        services.TryAddSingleton<
            IErrorCodeGroupCatalogProvider,
            ErrorCodeGroupCatalogProvider>();
    }

    private static void RegisterOwnerCatalogServices(
        IServiceCollection services)
    {
        services.TryAddSingleton<
            IErrorOwnerCatalogLoader,
            JsonErrorOwnerCatalogLoader>();

        services.TryAddSingleton<
            ErrorOwnerCatalogDocumentNormalizer>();

        services.TryAddSingleton<
            IErrorOwnerCatalogValidator,
            ErrorOwnerCatalogValidator>();

        services.TryAddSingleton<
            IErrorOwnerCatalogProvider,
            ErrorOwnerCatalogProvider>();
    }

    private static void RegisterProfileCatalogServices(
        IServiceCollection services)
    {
        services.TryAddSingleton<
            IErrorProfileCatalogLoader,
            JsonErrorProfileCatalogLoader>();

        services.TryAddSingleton<
            ErrorProfileCatalogDocumentNormalizer>();

        services.TryAddSingleton<
            IErrorProfileCatalogValidator,
            ErrorProfileCatalogValidator>();

        services.TryAddSingleton<
            IErrorProfileCatalogProvider,
            ErrorProfileCatalogProvider>();
    }

    private static void RegisterResolutionServices(
        IServiceCollection services)
    {
        services.TryAddSingleton<
            IErrorDefinitionResolver,
            ErrorDefinitionResolver>();

        services.TryAddSingleton<
            IErrorProfileResolver,
            ErrorProfileResolver>();

        services.TryAddSingleton<
            IErrorProfileSelectionService,
            ErrorProfileSelectionService>();
    }

    private static void RegisterDescriptorServices(
        IServiceCollection services)
    {
        services.TryAddSingleton<
            IErrorDescriptorFactory,
            ErrorDescriptorFactory>();

        services.TryAddSingleton<
            IErrorDescriptorResolver,
            ErrorDescriptorResolver>();

        services.TryAddSingleton<
            IErrorDescriptorService,
            ErrorDescriptorService>();

        services.TryAddSingleton<
            IErrorCatalogRuntime,
            ErrorCatalogRuntime>();
    }
    private static WhenItFailsOptions CreateOptionsSnapshot(
     WhenItFailsOptions source)
    {
        JsonsOptions sourceJsons =
            source.Jsons ?? new JsonsOptions();

        return new WhenItFailsOptions
        {
            InitializationMode =
                source.InitializationMode,

            HideRecoverableFailures =
                source.HideRecoverableFailures,

            Jsons = new JsonsOptions
            {
                RootDirectory =
                    sourceJsons.RootDirectory,

                PackageDirectoryName =
                    sourceJsons.PackageDirectoryName,

                ErrorCatalogFileName =
                    sourceJsons.ErrorCatalogFileName,

                CategoryCatalogFileName =
                    sourceJsons.CategoryCatalogFileName,

                CodeGroupCatalogFileName =
                    sourceJsons.CodeGroupCatalogFileName,

                OwnerCatalogFileName =
                    sourceJsons.OwnerCatalogFileName,

                ProfilesFileName =
                    sourceJsons.ProfilesFileName
            }
        };
    }
}