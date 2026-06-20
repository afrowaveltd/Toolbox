using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Descriptors;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Resolution;
using Afrowave.Toolbox.WhenItFails.Services;
using Afrowave.Toolbox.WhenItFails.Validation;
using Microsoft.Extensions.DependencyInjection;
using Afrowave.Toolbox.WhenItFails.Initialization;

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