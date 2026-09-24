using System.Reflection;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Afrowave.Toolbox.WhenItFails.Tests.PublicApi;

public sealed class BuiltInCatalogContextProviderPublicApiContractTests
{
    [Fact]
    public void BuiltInCatalogContextProvider_PreservesPublishedInterface()
    {
        Type contract = typeof(IBuiltInErrorCatalogContextProvider);

        Assert.True(contract.IsPublic);
        Assert.True(contract.IsInterface);
        Assert.Equal(
            "Afrowave.Toolbox.WhenItFails.Interfaces",
            contract.Namespace);

        MethodInfo method = Assert.Single(
            contract.GetMethods(
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly));

        Assert.Empty(
            contract.GetProperties(
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly));

        Assert.Equal("LoadAsync", method.Name);
        Assert.Equal(
            typeof(Task<Response<ErrorCatalogContext>>),
            method.ReturnType);

        ParameterInfo token = Assert.Single(method.GetParameters());

        Assert.Equal("cancellationToken", token.Name);
        Assert.Equal(typeof(CancellationToken), token.ParameterType);
        Assert.True(token.IsOptional);
        Assert.True(token.HasDefaultValue);
    }

    [Fact]
    public void AddWhenItFails_PreservesPreRegisteredBuiltInCatalogContextProvider()
    {
        ServiceCollection services = new();
        CustomBuiltInContextProvider custom = new();

        services.AddSingleton<IBuiltInErrorCatalogContextProvider>(custom);
        services.AddWhenItFails();

        using ServiceProvider provider = services.BuildServiceProvider(
            new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });

        Assert.Same(
            custom,
            provider.GetRequiredService<IBuiltInErrorCatalogContextProvider>());

        Assert.NotNull(provider.GetRequiredService<IErrorCatalogRuntime>());
    }

    private sealed class CustomBuiltInContextProvider
        : IBuiltInErrorCatalogContextProvider
    {
        public Task<Response<ErrorCatalogContext>> LoadAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult(
                Response<ErrorCatalogContext>.Invalid(
                    code: "TEST_BUILT_IN_CONTEXT_NOT_CONFIGURED",
                    message: "Test-only built-in catalog context provider."));
    }
}
