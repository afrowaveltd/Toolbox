using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Resolution;
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