using System.Reflection;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Afrowave.Toolbox.WhenItFails.Tests.PublicApi;

public sealed class CatalogPipelineExtensionPointPublicApiContractTests
{
    [Fact]
    public void CatalogLoader_PreservesPublishedInterface()
    {
        Type contract = typeof(IErrorCatalogLoader);
        AssertSingleMethodInterface(contract);

        MethodInfo method = RequireMethod(
            contract,
            "LoadFromFileAsync",
            typeof(Task<Response<ErrorCatalogDocument>>),
            typeof(string),
            typeof(CancellationToken));

        AssertOptionalCancellationToken(method);
    }

    [Fact]
    public void CatalogFactory_PreservesPublishedInterface()
    {
        Type contract = typeof(IErrorCatalogFactory);
        AssertSingleMethodInterface(contract);

        RequireMethod(
            contract,
            "Create",
            typeof(IErrorCatalog),
            typeof(ErrorCatalogDocument));
    }

    [Fact]
    public void CatalogProvider_PreservesPublishedInterface()
    {
        Type contract = typeof(IErrorCatalogProvider);
        AssertSingleMethodInterface(contract);

        MethodInfo method = RequireMethod(
            contract,
            "LoadFromFileAsync",
            typeof(Task<Response<ErrorCatalogProviderPayload>>),
            typeof(string),
            typeof(CancellationToken));

        AssertOptionalCancellationToken(method);
    }

    [Fact]
    public void AddWhenItFails_PreservesPreRegisteredCatalogPipelineImplementations()
    {
        ServiceCollection services = new();

        CustomCatalogLoader loader = new();
        CustomCatalogFactory factory = new();
        CustomCatalogProvider catalogProvider = new();

        services.AddSingleton<IErrorCatalogLoader>(loader);
        services.AddSingleton<IErrorCatalogFactory>(factory);
        services.AddSingleton<IErrorCatalogProvider>(catalogProvider);

        services.AddWhenItFails();

        using ServiceProvider provider = services.BuildServiceProvider(
            new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });

        Assert.Same(
            loader,
            provider.GetRequiredService<IErrorCatalogLoader>());

        Assert.Same(
            factory,
            provider.GetRequiredService<IErrorCatalogFactory>());

        Assert.Same(
            catalogProvider,
            provider.GetRequiredService<IErrorCatalogProvider>());

        Assert.NotNull(provider.GetRequiredService<IErrorCatalogRuntime>());
    }

    private static void AssertSingleMethodInterface(Type contract)
    {
        Assert.True(contract.IsPublic);
        Assert.True(contract.IsInterface);
        Assert.Equal(
            "Afrowave.Toolbox.WhenItFails.Interfaces",
            contract.Namespace);

        Assert.Single(
            contract.GetMethods(
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly));

        Assert.Empty(
            contract.GetProperties(
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly));
    }

    private static MethodInfo RequireMethod(
        Type contract,
        string name,
        Type returnType,
        params Type[] parameterTypes)
    {
        MethodInfo? method = contract.GetMethod(
            name,
            BindingFlags.Public |
            BindingFlags.Instance |
            BindingFlags.DeclaredOnly,
            binder: null,
            types: parameterTypes,
            modifiers: null);

        Assert.NotNull(method);
        Assert.Equal(returnType, method.ReturnType);
        return method;
    }

    private static void AssertOptionalCancellationToken(MethodInfo method)
    {
        ParameterInfo token = Assert.Single(
            method.GetParameters(),
            parameter => parameter.ParameterType == typeof(CancellationToken));

        Assert.True(token.IsOptional);
        Assert.True(token.HasDefaultValue);
    }

    private sealed class CustomCatalogLoader : IErrorCatalogLoader
    {
        public Task<Response<ErrorCatalogDocument>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(
                Response<ErrorCatalogDocument>.Invalid(
                    code: "CUSTOM_CATALOG_LOADER_NOT_CONFIGURED",
                    message: "Test-only catalog loader."));
    }

    private sealed class CustomCatalogFactory : IErrorCatalogFactory
    {
        public IErrorCatalog Create(ErrorCatalogDocument document) =>
            throw new NotSupportedException("Test-only catalog factory.");
    }

    private sealed class CustomCatalogProvider : IErrorCatalogProvider
    {
        public Task<Response<ErrorCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(
                Response<ErrorCatalogProviderPayload>.Invalid(
                    code: "CUSTOM_CATALOG_PROVIDER_NOT_CONFIGURED",
                    message: "Test-only catalog provider."));
    }
}
