using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Descriptors;
using Afrowave.Toolbox.WhenItFails.Enums;
using Afrowave.Toolbox.WhenItFails.Initialization;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Resolution;
using Afrowave.Toolbox.WhenItFails.Services;
using Afrowave.Toolbox.WhenItFails.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Afrowave.Toolbox.WhenItFails.Tests.DependencyInjection;

public sealed class WhenItFailsServiceCollectionExtensionsTests
{
    [Fact]
    public void AddWhenItFails_ShouldThrowArgumentNullException_WhenServicesIsNull()
    {
        IServiceCollection services = null!;

        Assert.Throws<ArgumentNullException>(
            () => services.AddWhenItFails());
    }

    [Fact]
    public void AddWhenItFails_ShouldReturnSameServiceCollection()
    {
        IServiceCollection services = new ServiceCollection();

        IServiceCollection result =
            services.AddWhenItFails();

        Assert.Same(services, result);
    }

    [Fact]
    public void AddWhenItFails_ShouldRegisterErrorProfileResolverAsSingleton()
    {
        ServiceCollection services = new();

        services.AddWhenItFails();

        using ServiceProvider serviceProvider =
            services.BuildServiceProvider();

        IErrorProfileResolver first =
            serviceProvider.GetRequiredService<IErrorProfileResolver>();

        IErrorProfileResolver second =
            serviceProvider.GetRequiredService<IErrorProfileResolver>();

        Assert.IsType<ErrorProfileResolver>(first);
        Assert.Same(first, second);
    }

    [Fact]
    public void AddWhenItFails_ShouldRegisterErrorProfileSelectionServiceAsSingleton()
    {
        ServiceCollection services = new();

        services.AddWhenItFails();

        using ServiceProvider serviceProvider =
            services.BuildServiceProvider();

        IErrorProfileSelectionService first =
            serviceProvider.GetRequiredService<
                IErrorProfileSelectionService>();

        IErrorProfileSelectionService second =
            serviceProvider.GetRequiredService<
                IErrorProfileSelectionService>();

        Assert.IsType<ErrorProfileSelectionService>(first);
        Assert.Same(first, second);
    }

    [Fact]
    public void AddWhenItFails_ShouldPreserveExistingErrorProfileResolverRegistration()
    {
        ServiceCollection services = new();

        FakeErrorProfileResolver customResolver = new();

        services.AddSingleton<IErrorProfileResolver>(
            customResolver);

        services.AddWhenItFails();

        using ServiceProvider serviceProvider =
            services.BuildServiceProvider();

        IErrorProfileResolver resolved =
            serviceProvider.GetRequiredService<IErrorProfileResolver>();

        Assert.Same(customResolver, resolved);
    }

    [Fact]
    public void AddWhenItFails_ShouldBuildCompleteServiceProvider()
    {
        ServiceCollection services = new();

        services.AddWhenItFails();

        using ServiceProvider serviceProvider =
            services.BuildServiceProvider(
                new ServiceProviderOptions
                {
                    ValidateOnBuild = true,
                    ValidateScopes = true
                });

        Assert.NotNull(serviceProvider);
    }

    [Fact]
    public void AddWhenItFails_ShouldResolveCompleteRuntimeServiceGraph()
    {
        ServiceCollection services = new();

        services.AddWhenItFails();

        using ServiceProvider serviceProvider =
            services.BuildServiceProvider(
                new ServiceProviderOptions
                {
                    ValidateOnBuild = true,
                    ValidateScopes = true
                });

        Assert.IsType<DefaultJsonsTemplateProvider>(
            serviceProvider.GetRequiredService<IJsonsTemplateProvider>());

        Assert.IsType<JsonsBootstrapper>(
            serviceProvider.GetRequiredService<IJsonsBootstrapper>());

        Assert.IsType<JsonErrorCatalogLoader>(
            serviceProvider.GetRequiredService<IErrorCatalogLoader>());

        Assert.IsType<ErrorCatalogDocumentNormalizer>(
            serviceProvider.GetRequiredService<
                IErrorCatalogDocumentNormalizer>());

        Assert.IsType<ErrorCatalogValidator>(
            serviceProvider.GetRequiredService<IErrorCatalogValidator>());

        Assert.IsType<ErrorCatalogFactory>(
            serviceProvider.GetRequiredService<IErrorCatalogFactory>());

        Assert.IsType<ErrorCatalogProvider>(
            serviceProvider.GetRequiredService<IErrorCatalogProvider>());

        Assert.IsType<ErrorCategoryCatalogProvider>(
            serviceProvider.GetRequiredService<
                IErrorCategoryCatalogProvider>());

        Assert.IsType<ErrorCodeGroupCatalogProvider>(
            serviceProvider.GetRequiredService<
                IErrorCodeGroupCatalogProvider>());

        Assert.IsType<ErrorOwnerCatalogProvider>(
            serviceProvider.GetRequiredService<
                IErrorOwnerCatalogProvider>());

        Assert.IsType<ErrorProfileCatalogProvider>(
            serviceProvider.GetRequiredService<
                IErrorProfileCatalogProvider>());

        Assert.IsType<ErrorCatalogContextProvider>(
            serviceProvider.GetRequiredService<
                IErrorCatalogContextProvider>());

        Assert.IsType<ErrorDefinitionResolver>(
            serviceProvider.GetRequiredService<
                IErrorDefinitionResolver>());

        Assert.IsType<ErrorProfileResolver>(
            serviceProvider.GetRequiredService<
                IErrorProfileResolver>());

        Assert.IsType<ErrorProfileSelectionService>(
            serviceProvider.GetRequiredService<
                IErrorProfileSelectionService>());

        Assert.IsType<ErrorDescriptorFactory>(
            serviceProvider.GetRequiredService<
                IErrorDescriptorFactory>());

        Assert.IsType<ErrorDescriptorResolver>(
            serviceProvider.GetRequiredService<
                IErrorDescriptorResolver>());

        Assert.IsType<ErrorDescriptorService>(
            serviceProvider.GetRequiredService<
                IErrorDescriptorService>());
    }

    [Fact]
    public void AddWhenItFails_ShouldRegisterDescriptorServiceAsSingleton()
    {
        ServiceCollection services = new();

        services.AddWhenItFails();

        using ServiceProvider serviceProvider =
            services.BuildServiceProvider();

        IErrorDescriptorService first =
            serviceProvider.GetRequiredService<
                IErrorDescriptorService>();

        IErrorDescriptorService second =
            serviceProvider.GetRequiredService<
                IErrorDescriptorService>();

        Assert.IsType<ErrorDescriptorService>(first);
        Assert.Same(first, second);
    }

    [Fact]
    public void AddWhenItFails_ShouldRegisterErrorCatalogContextStoreAsSingleton()
    {
        ServiceCollection services = new();

        services.AddWhenItFails();

        using ServiceProvider serviceProvider =
            services.BuildServiceProvider();

        IErrorCatalogContextStore first =
            serviceProvider.GetRequiredService<
                IErrorCatalogContextStore>();

        IErrorCatalogContextStore second =
            serviceProvider.GetRequiredService<
                IErrorCatalogContextStore>();

        Assert.IsType<ErrorCatalogContextStore>(first);
        Assert.Same(first, second);
    }

    [Fact]
    public void AddWhenItFails_ShouldRegisterErrorCatalogInitializerAsSingleton()
    {
        ServiceCollection services = new();

        services.AddWhenItFails();

        using ServiceProvider serviceProvider =
            services.BuildServiceProvider(
                new ServiceProviderOptions
                {
                    ValidateOnBuild = true,
                    ValidateScopes = true
                });

        IErrorCatalogInitializer first =
            serviceProvider.GetRequiredService<
                IErrorCatalogInitializer>();

        IErrorCatalogInitializer second =
            serviceProvider.GetRequiredService<
                IErrorCatalogInitializer>();

        Assert.IsType<ErrorCatalogInitializer>(first);
        Assert.Same(first, second);
    }

    [Fact]
    public void AddWhenItFails_ShouldRegisterErrorCatalogRuntimeAsSingleton()
    {
        ServiceCollection services = new();

        services.AddWhenItFails();

        using ServiceProvider serviceProvider =
            services.BuildServiceProvider(
                new ServiceProviderOptions
                {
                    ValidateOnBuild = true,
                    ValidateScopes = true
                });

        IErrorCatalogRuntime first =
            serviceProvider.GetRequiredService<IErrorCatalogRuntime>();

        IErrorCatalogRuntime second =
            serviceProvider.GetRequiredService<IErrorCatalogRuntime>();

        Assert.IsType<ErrorCatalogRuntime>(first);
        Assert.Same(first, second);
    }

    [Fact]
    public void AddWhenItFails_ShouldRegisterDefaultOptions()
    {
        ServiceCollection services = new();

        services.AddWhenItFails();

        using ServiceProvider serviceProvider =
            services.BuildServiceProvider();

        WhenItFailsOptions options =
            serviceProvider.GetRequiredService<
                WhenItFailsOptions>();

        Assert.NotNull(options.Jsons);

        Assert.Equal(
            ErrorCatalogInitializationMode.Flexible,
            options.InitializationMode);

        Assert.Null(
            options.HideRecoverableFailures);

        Assert.Equal(
            "Jsons",
            options.Jsons.RootDirectory);

        Assert.Equal(
            "WhenItFails",
            options.Jsons.PackageDirectoryName);
    }

    [Fact]
    public void AddWhenItFails_ShouldRegisterConfiguredOptions()
    {
        ServiceCollection services = new();

        services.AddWhenItFails(options =>
        {
            options.InitializationMode =
                ErrorCatalogInitializationMode.Strict;

            options.HideRecoverableFailures = true;

            options.Jsons.RootDirectory =
                "CustomJsons";

            options.Jsons.PackageDirectoryName =
                "CustomWhenItFails";

            options.Jsons.ErrorCatalogFileName =
                "custom-errors.json";
        });

        using ServiceProvider serviceProvider =
            services.BuildServiceProvider();

        WhenItFailsOptions options =
            serviceProvider.GetRequiredService<
                WhenItFailsOptions>();

        Assert.Equal(
            ErrorCatalogInitializationMode.Strict,
            options.InitializationMode);

        Assert.True(
            options.HideRecoverableFailures);

        Assert.Equal(
            "CustomJsons",
            options.Jsons.RootDirectory);

        Assert.Equal(
            "CustomWhenItFails",
            options.Jsons.PackageDirectoryName);

        Assert.Equal(
            "custom-errors.json",
            options.Jsons.ErrorCatalogFileName);
    }

    [Fact]
    public void AddWhenItFails_ShouldRegisterOptionsAsSingleton()
    {
        ServiceCollection services = new();

        services.AddWhenItFails(options =>
        {
            options.Jsons.RootDirectory =
                "CustomJsons";
        });

        using ServiceProvider serviceProvider =
            services.BuildServiceProvider();

        WhenItFailsOptions first =
            serviceProvider.GetRequiredService<
                WhenItFailsOptions>();

        WhenItFailsOptions second =
            serviceProvider.GetRequiredService<
                WhenItFailsOptions>();

        Assert.Same(first, second);
    }

    [Fact]
    public void AddWhenItFails_ShouldThrowArgumentNullException_WhenConfigureIsNull()
    {
        ServiceCollection services = new();

        Assert.Throws<ArgumentNullException>(
            () => services.AddWhenItFails(
                (Action<WhenItFailsOptions>)null!));
    }

    [Fact]
    public void AddWhenItFails_ShouldNormalizeNullJsonsOptions()
    {
        ServiceCollection services = new();

        services.AddWhenItFails(options =>
        {
            options.Jsons = null!;
        });

        using ServiceProvider serviceProvider =
            services.BuildServiceProvider();

        WhenItFailsOptions options =
            serviceProvider.GetRequiredService<
                WhenItFailsOptions>();

        Assert.NotNull(options.Jsons);

        Assert.Equal(
            "Jsons",
            options.Jsons.RootDirectory);

        Assert.Equal(
            "WhenItFails",
            options.Jsons.PackageDirectoryName);
    }

    [Fact]
    public void AddWhenItFails_ShouldStoreConfigurationSnapshot()
    {
        ServiceCollection services = new();

        WhenItFailsOptions? configuredOptions = null;

        services.AddWhenItFails(options =>
        {
            configuredOptions = options;

            options.InitializationMode =
                ErrorCatalogInitializationMode.Strict;

            options.Jsons.RootDirectory =
                "OriginalJsons";
        });

        Assert.NotNull(configuredOptions);

        configuredOptions.InitializationMode =
            ErrorCatalogInitializationMode.Flexible;

        configuredOptions.Jsons.RootDirectory =
            "ChangedLater";

        using ServiceProvider serviceProvider =
            services.BuildServiceProvider();

        WhenItFailsOptions registeredOptions =
            serviceProvider.GetRequiredService<
                WhenItFailsOptions>();

        Assert.Equal(
            ErrorCatalogInitializationMode.Strict,
            registeredOptions.InitializationMode);

        Assert.Equal(
            "OriginalJsons",
            registeredOptions.Jsons.RootDirectory);

        Assert.NotSame(
            configuredOptions,
            registeredOptions);

        Assert.NotSame(
            configuredOptions.Jsons,
            registeredOptions.Jsons);
    }

    [Fact]
    public void AddWhenItFails_WithConfiguration_ShouldReplacePreviouslyRegisteredOptions()
    {
        ServiceCollection services = new();

        services.AddSingleton(
            new WhenItFailsOptions
            {
                InitializationMode =
                    ErrorCatalogInitializationMode.Flexible,

                HideRecoverableFailures = null
            });

        services.AddWhenItFails(options =>
        {
            options.InitializationMode =
                ErrorCatalogInitializationMode.Strict;

            options.HideRecoverableFailures = true;
        });

        using ServiceProvider serviceProvider =
            services.BuildServiceProvider();

        WhenItFailsOptions options =
            serviceProvider.GetRequiredService<
                WhenItFailsOptions>();

        Assert.Equal(
            ErrorCatalogInitializationMode.Strict,
            options.InitializationMode);

        Assert.True(
            options.HideRecoverableFailures);
    }
    [Fact]
    public void AddWhenItFails_ShouldRegisterSuppliedOptions()
    {
        ServiceCollection services = new();

        WhenItFailsOptions suppliedOptions = new()
        {
            InitializationMode =
                ErrorCatalogInitializationMode.Strict,

            HideRecoverableFailures = true,

            Jsons = new JsonsOptions
            {
                RootDirectory = "ServiceJsons",
                PackageDirectoryName = "ServiceErrors",
                ErrorCatalogFileName = "service-errors.json",
                CategoryCatalogFileName = "service-categories.json",
                CodeGroupCatalogFileName = "service-groups.json",
                OwnerCatalogFileName = "service-owners.json",
                ProfilesFileName = "service-profiles.json"
            }
        };

        services.AddWhenItFails(
            suppliedOptions);

        using ServiceProvider serviceProvider =
            services.BuildServiceProvider();

        WhenItFailsOptions registeredOptions =
            serviceProvider.GetRequiredService<
                WhenItFailsOptions>();

        Assert.Equal(
            ErrorCatalogInitializationMode.Strict,
            registeredOptions.InitializationMode);

        Assert.True(
            registeredOptions.HideRecoverableFailures);

        Assert.Equal(
            "ServiceJsons",
            registeredOptions.Jsons.RootDirectory);

        Assert.Equal(
            "ServiceErrors",
            registeredOptions.Jsons.PackageDirectoryName);

        Assert.Equal(
            "service-errors.json",
            registeredOptions.Jsons.ErrorCatalogFileName);

        Assert.Equal(
            "service-categories.json",
            registeredOptions.Jsons.CategoryCatalogFileName);

        Assert.Equal(
            "service-groups.json",
            registeredOptions.Jsons.CodeGroupCatalogFileName);

        Assert.Equal(
            "service-owners.json",
            registeredOptions.Jsons.OwnerCatalogFileName);

        Assert.Equal(
            "service-profiles.json",
            registeredOptions.Jsons.ProfilesFileName);
    }

    [Fact]
    public void AddWhenItFails_ShouldThrowArgumentNullException_WhenOptionsIsNull()
    {
        ServiceCollection services = new();

        Assert.Throws<ArgumentNullException>(
            () => services.AddWhenItFails(
                (WhenItFailsOptions)null!));
    }

    [Fact]
    public void AddWhenItFails_ShouldNormalizeNullJsonsInSuppliedOptions()
    {
        ServiceCollection services = new();

        WhenItFailsOptions suppliedOptions = new()
        {
            Jsons = null!,
            InitializationMode =
                ErrorCatalogInitializationMode.Strict
        };

        services.AddWhenItFails(
            suppliedOptions);

        using ServiceProvider serviceProvider =
            services.BuildServiceProvider();

        WhenItFailsOptions registeredOptions =
            serviceProvider.GetRequiredService<
                WhenItFailsOptions>();

        Assert.NotNull(
            registeredOptions.Jsons);

        Assert.Equal(
            "Jsons",
            registeredOptions.Jsons.RootDirectory);

        Assert.Equal(
            "WhenItFails",
            registeredOptions.Jsons.PackageDirectoryName);

        Assert.Equal(
            ErrorCatalogInitializationMode.Strict,
            registeredOptions.InitializationMode);
    }

    [Fact]
    public void AddWhenItFails_ShouldStoreSnapshotOfSuppliedOptions()
    {
        ServiceCollection services = new();

        WhenItFailsOptions suppliedOptions = new()
        {
            InitializationMode =
                ErrorCatalogInitializationMode.Strict,

            HideRecoverableFailures = true,

            Jsons = new JsonsOptions
            {
                RootDirectory = "OriginalJsons",
                PackageDirectoryName = "OriginalPackage"
            }
        };

        services.AddWhenItFails(
            suppliedOptions);

        suppliedOptions.InitializationMode =
            ErrorCatalogInitializationMode.Flexible;

        suppliedOptions.HideRecoverableFailures = false;

        suppliedOptions.Jsons.RootDirectory =
            "ChangedJsons";

        suppliedOptions.Jsons.PackageDirectoryName =
            "ChangedPackage";

        using ServiceProvider serviceProvider =
            services.BuildServiceProvider();

        WhenItFailsOptions registeredOptions =
            serviceProvider.GetRequiredService<
                WhenItFailsOptions>();

        Assert.Equal(
            ErrorCatalogInitializationMode.Strict,
            registeredOptions.InitializationMode);

        Assert.True(
            registeredOptions.HideRecoverableFailures);

        Assert.Equal(
            "OriginalJsons",
            registeredOptions.Jsons.RootDirectory);

        Assert.Equal(
            "OriginalPackage",
            registeredOptions.Jsons.PackageDirectoryName);

        Assert.NotSame(
            suppliedOptions,
            registeredOptions);

        Assert.NotSame(
            suppliedOptions.Jsons,
            registeredOptions.Jsons);
    }

    [Fact]
    public void AddWhenItFails_WithSuppliedOptions_ShouldReplacePreviousOptions()
    {
        ServiceCollection services = new();

        services.AddSingleton(
            new WhenItFailsOptions
            {
                InitializationMode =
                    ErrorCatalogInitializationMode.Flexible,

                HideRecoverableFailures = null
            });

        WhenItFailsOptions suppliedOptions = new()
        {
            InitializationMode =
                ErrorCatalogInitializationMode.Strict,

            HideRecoverableFailures = true
        };

        services.AddWhenItFails(
            suppliedOptions);

        using ServiceProvider serviceProvider =
            services.BuildServiceProvider();

        WhenItFailsOptions registeredOptions =
            serviceProvider.GetRequiredService<
                WhenItFailsOptions>();

        Assert.Equal(
            ErrorCatalogInitializationMode.Strict,
            registeredOptions.InitializationMode);

        Assert.True(
            registeredOptions.HideRecoverableFailures);
    }

    [Fact]
    public void AddWhenItFails_ShouldBindConfigurationSection()
    {
        Dictionary<string, string?> configurationValues = new()
        {
            ["Afrowave:WhenItFails:InitializationMode"] =
                "Strict",

            ["Afrowave:WhenItFails:HideRecoverableFailures"] =
                "true",

            ["Afrowave:WhenItFails:Jsons:RootDirectory"] =
                "ConfiguredJsons",

            ["Afrowave:WhenItFails:Jsons:PackageDirectoryName"] =
                "ConfiguredWhenItFails",

            ["Afrowave:WhenItFails:Jsons:ErrorCatalogFileName"] =
                "configured-errors.json",

            ["Afrowave:WhenItFails:Jsons:CategoryCatalogFileName"] =
                "configured-categories.json",

            ["Afrowave:WhenItFails:Jsons:CodeGroupCatalogFileName"] =
                "configured-groups.json",

            ["Afrowave:WhenItFails:Jsons:OwnerCatalogFileName"] =
                "configured-owners.json",

            ["Afrowave:WhenItFails:Jsons:ProfilesFileName"] =
                "configured-profiles.json"
        };

        IConfiguration configuration =
            new ConfigurationBuilder()
                .AddInMemoryCollection(configurationValues)
                .Build();

        ServiceCollection services = new();

        services.AddWhenItFails(
            configuration.GetSection(
                "Afrowave:WhenItFails"));

        using ServiceProvider serviceProvider =
            services.BuildServiceProvider();

        WhenItFailsOptions options =
            serviceProvider.GetRequiredService<
                WhenItFailsOptions>();

        Assert.Equal(
            ErrorCatalogInitializationMode.Strict,
            options.InitializationMode);

        Assert.True(
            options.HideRecoverableFailures);

        Assert.Equal(
            "ConfiguredJsons",
            options.Jsons.RootDirectory);

        Assert.Equal(
            "ConfiguredWhenItFails",
            options.Jsons.PackageDirectoryName);

        Assert.Equal(
            "configured-errors.json",
            options.Jsons.ErrorCatalogFileName);

        Assert.Equal(
            "configured-categories.json",
            options.Jsons.CategoryCatalogFileName);

        Assert.Equal(
            "configured-groups.json",
            options.Jsons.CodeGroupCatalogFileName);

        Assert.Equal(
            "configured-owners.json",
            options.Jsons.OwnerCatalogFileName);

        Assert.Equal(
            "configured-profiles.json",
            options.Jsons.ProfilesFileName);
    }

    [Fact]
    public void AddWhenItFails_ShouldUseDefaultsForMissingConfigurationValues()
    {
        IConfiguration configuration =
            new ConfigurationBuilder()
                .AddInMemoryCollection(
                    new Dictionary<string, string?>())
                .Build();

        ServiceCollection services = new();

        services.AddWhenItFails(
            configuration.GetSection(
                "Afrowave:WhenItFails"));

        using ServiceProvider serviceProvider =
            services.BuildServiceProvider();

        WhenItFailsOptions options =
            serviceProvider.GetRequiredService<
                WhenItFailsOptions>();

        Assert.Equal(
            ErrorCatalogInitializationMode.Flexible,
            options.InitializationMode);

        Assert.Null(
            options.HideRecoverableFailures);

        Assert.Equal(
            "Jsons",
            options.Jsons.RootDirectory);

        Assert.Equal(
            "WhenItFails",
            options.Jsons.PackageDirectoryName);
    }

    [Fact]
    public void AddWhenItFails_ShouldBindExplicitFalseForHideRecoverableFailures()
    {
        Dictionary<string, string?> configurationValues = new()
        {
            ["WhenItFails:HideRecoverableFailures"] =
                "false"
        };

        IConfiguration configuration =
            new ConfigurationBuilder()
                .AddInMemoryCollection(configurationValues)
                .Build();

        ServiceCollection services = new();

        services.AddWhenItFails(
            configuration.GetSection(
                "WhenItFails"));

        using ServiceProvider serviceProvider =
            services.BuildServiceProvider();

        WhenItFailsOptions options =
            serviceProvider.GetRequiredService<
                WhenItFailsOptions>();

        Assert.False(
            options.HideRecoverableFailures);
    }

    [Fact]
    public void AddWhenItFails_ShouldThrowArgumentNullException_WhenConfigurationSectionIsNull()
    {
        ServiceCollection services = new();

        Assert.Throws<ArgumentNullException>(
            () => services.AddWhenItFails(
                (IConfigurationSection)null!));
    }

    [Fact]
    public void AddWhenItFails_WithConfigurationSection_ShouldReplacePreviousOptions()
    {
        ServiceCollection services = new();

        services.AddSingleton(
            new WhenItFailsOptions
            {
                InitializationMode =
                    ErrorCatalogInitializationMode.Flexible,

                HideRecoverableFailures = null
            });

        Dictionary<string, string?> configurationValues = new()
        {
            ["WhenItFails:InitializationMode"] =
                "Strict",

            ["WhenItFails:HideRecoverableFailures"] =
                "true"
        };

        IConfiguration configuration =
            new ConfigurationBuilder()
                .AddInMemoryCollection(configurationValues)
                .Build();

        services.AddWhenItFails(
            configuration.GetSection(
                "WhenItFails"));

        using ServiceProvider serviceProvider =
            services.BuildServiceProvider();

        WhenItFailsOptions options =
            serviceProvider.GetRequiredService<
                WhenItFailsOptions>();

        Assert.Equal(
            ErrorCatalogInitializationMode.Strict,
            options.InitializationMode);

        Assert.True(
            options.HideRecoverableFailures);
    }

    private sealed class FakeErrorProfileResolver
        : IErrorProfileResolver
    {
        public IReadOnlyList<Definitions.ErrorDefinition> Resolve(
            Definitions.ErrorCatalogDocument errorCatalog,
            Definitions.ErrorProfileDefinition profile)
        {
            return [];
        }
    }
}