using System.Reflection;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Initialization;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Afrowave.Toolbox.WhenItFails.Tests.PublicApi;

public sealed class SecondExtensionPointPublicApiContractTests
{
    [Fact]
    public void CatalogInitializer_PreservesPublishedInterface()
    {
        Type contract = typeof(IErrorCatalogInitializer);
        AssertInterfaceShape(contract, declaredMethodCount: 1, declaredPropertyCount: 0);

        MethodInfo method = RequireMethod(
            contract,
            "InitializeAsync",
            typeof(Task<Response<ErrorCatalogInitializationPayload>>),
            typeof(JsonsOptions),
            typeof(CancellationToken));

        AssertOptionalCancellationToken(method);
    }

    [Fact]
    public void CatalogContextStore_PreservesPublishedInterface()
    {
        Type contract = typeof(IErrorCatalogContextStore);
        AssertInterfaceShape(contract, declaredMethodCount: 2, declaredPropertyCount: 2);

        AssertReadOnlyProperty(contract, "IsInitialized", typeof(bool));
        AssertReadOnlyProperty(contract, "Current", typeof(ErrorCatalogContext));

        RequireMethod(
            contract,
            "GetCurrent",
            typeof(Response<ErrorCatalogContext>));

        RequireMethod(
            contract,
            "Set",
            typeof(void),
            typeof(ErrorCatalogContext));
    }

    [Fact]
    public void JsonsBootstrapper_PreservesPublishedInterface()
    {
        Type contract = typeof(IJsonsBootstrapper);
        AssertInterfaceShape(contract, declaredMethodCount: 1, declaredPropertyCount: 0);

        MethodInfo method = RequireMethod(
            contract,
            "EnsureWorkspaceAsync",
            typeof(Task<Response<JsonsBootstrapPayload>>),
            typeof(JsonsOptions),
            typeof(CancellationToken));

        AssertOptionalCancellationToken(method);
    }

    [Fact]
    public void AddWhenItFails_PreservesPreRegisteredSecondGroupExtensionServices()
    {
        ServiceCollection services = new();

        CustomInitializer initializer = new();
        CustomContextStore store = new();
        CustomBootstrapper bootstrapper = new();

        services.AddSingleton<IErrorCatalogInitializer>(initializer);
        services.AddSingleton<IErrorCatalogContextStore>(store);
        services.AddSingleton<IJsonsBootstrapper>(bootstrapper);

        services.AddWhenItFails();

        using ServiceProvider provider = services.BuildServiceProvider(
            new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });

        Assert.Same(
            initializer,
            provider.GetRequiredService<IErrorCatalogInitializer>());
        Assert.Same(
            store,
            provider.GetRequiredService<IErrorCatalogContextStore>());
        Assert.Same(
            bootstrapper,
            provider.GetRequiredService<IJsonsBootstrapper>());

        Assert.NotNull(provider.GetRequiredService<IErrorCatalogRuntime>());
    }

    private static void AssertInterfaceShape(
        Type contract,
        int declaredMethodCount,
        int declaredPropertyCount)
    {
        Assert.True(contract.IsPublic);
        Assert.True(contract.IsInterface);
        Assert.Equal(
            "Afrowave.Toolbox.WhenItFails.Interfaces",
            contract.Namespace);

        // Property accessors count as methods in reflection.
        MethodInfo[] ordinaryMethods = contract.GetMethods(
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .ToArray();

        Assert.Equal(declaredMethodCount, ordinaryMethods.Length);
        Assert.Equal(
            declaredPropertyCount,
            contract.GetProperties(
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly).Length);
    }

    private static void AssertReadOnlyProperty(
        Type contract,
        string name,
        Type expectedType)
    {
        PropertyInfo? property = contract.GetProperty(
            name,
            BindingFlags.Public |
            BindingFlags.Instance |
            BindingFlags.DeclaredOnly);

        Assert.NotNull(property);
        Assert.Equal(expectedType, property.PropertyType);
        Assert.True(property.GetMethod?.IsPublic == true);
        Assert.Null(property.SetMethod);
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

    private sealed class CustomInitializer : IErrorCatalogInitializer
    {
        public Task<Response<ErrorCatalogInitializationPayload>> InitializeAsync(
            JsonsOptions options,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(
                Response<ErrorCatalogInitializationPayload>.Invalid(
                    code: "CUSTOM_INITIALIZER_NOT_CONFIGURED",
                    message: "Test-only initializer."));
    }

    private sealed class CustomContextStore : IErrorCatalogContextStore
    {
        private ErrorCatalogContext? _current;

        public bool IsInitialized => _current is not null;

        public ErrorCatalogContext? Current => _current;

        public Response<ErrorCatalogContext> GetCurrent() =>
            _current is null
                ? Response<ErrorCatalogContext>.Invalid(
                    code: "CUSTOM_STORE_NOT_INITIALIZED",
                    message: "Test-only store has no context.")
                : Response<ErrorCatalogContext>.Ok(_current);

        public void Set(ErrorCatalogContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            _current = context;
        }
    }

    private sealed class CustomBootstrapper : IJsonsBootstrapper
    {
        public Task<Response<JsonsBootstrapPayload>> EnsureWorkspaceAsync(
            JsonsOptions options,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(
                Response<JsonsBootstrapPayload>.Invalid(
                    code: "CUSTOM_BOOTSTRAPPER_NOT_CONFIGURED",
                    message: "Test-only bootstrapper."));
    }
}
