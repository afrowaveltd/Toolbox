using System.Reflection;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Afrowave.Toolbox.WhenItFails.Tests.PublicApi;

public sealed class ProfileResolutionExtensionPointPublicApiContractTests
{
    [Fact]
    public void ProfileResolver_PreservesPublishedInterface()
    {
        Type contract = typeof(IErrorProfileResolver);
        AssertSingleMethodInterface(contract);

        RequireMethod(
            contract,
            "Resolve",
            typeof(IReadOnlyList<ErrorDefinition>),
            typeof(ErrorCatalogDocument),
            typeof(ErrorProfileDefinition));
    }

    [Fact]
    public void ProfileSelectionService_PreservesPublishedInterface()
    {
        Type contract = typeof(IErrorProfileSelectionService);
        AssertSingleMethodInterface(contract);

        RequireMethod(
            contract,
            "ResolveByProfileName",
            typeof(Response<IReadOnlyList<ErrorDefinition>>),
            typeof(ErrorCatalogContext),
            typeof(string));
    }

    [Fact]
    public void ProfileInterfaces_PreserveInputAndReturnNullability()
    {
        NullabilityInfoContext nullability = new();

        MethodInfo resolver = RequireMethod(
            typeof(IErrorProfileResolver),
            "Resolve",
            typeof(IReadOnlyList<ErrorDefinition>),
            typeof(ErrorCatalogDocument),
            typeof(ErrorProfileDefinition));

        ParameterInfo[] resolverParameters = resolver.GetParameters();
        Assert.Equal(
            NullabilityState.NotNull,
            nullability.Create(resolverParameters[0]).ReadState);
        Assert.Equal(
            NullabilityState.NotNull,
            nullability.Create(resolverParameters[1]).ReadState);
        Assert.Equal(
            NullabilityState.NotNull,
            nullability.Create(resolver.ReturnParameter).ReadState);

        MethodInfo selector = RequireMethod(
            typeof(IErrorProfileSelectionService),
            "ResolveByProfileName",
            typeof(Response<IReadOnlyList<ErrorDefinition>>),
            typeof(ErrorCatalogContext),
            typeof(string));

        ParameterInfo[] selectorParameters = selector.GetParameters();
        Assert.Equal(
            NullabilityState.Nullable,
            nullability.Create(selectorParameters[0]).ReadState);
        Assert.Equal(
            NullabilityState.NotNull,
            nullability.Create(selectorParameters[1]).ReadState);
        Assert.Equal(
            NullabilityState.NotNull,
            nullability.Create(selector.ReturnParameter).ReadState);
    }

    [Fact]
    public void AddWhenItFails_PreservesPreRegisteredProfileResolutionServices()
    {
        ServiceCollection services = new();
        CustomProfileResolver resolver = new();
        CustomProfileSelectionService selector = new();

        services.AddSingleton<IErrorProfileResolver>(resolver);
        services.AddSingleton<IErrorProfileSelectionService>(selector);

        services.AddWhenItFails();

        using ServiceProvider provider = services.BuildServiceProvider(
            new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });

        Assert.Same(
            resolver,
            provider.GetRequiredService<IErrorProfileResolver>());

        Assert.Same(
            selector,
            provider.GetRequiredService<IErrorProfileSelectionService>());

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

    private sealed class CustomProfileResolver : IErrorProfileResolver
    {
        public IReadOnlyList<ErrorDefinition> Resolve(
            ErrorCatalogDocument errorCatalog,
            ErrorProfileDefinition profile) =>
            Array.Empty<ErrorDefinition>();
    }

    private sealed class CustomProfileSelectionService : IErrorProfileSelectionService
    {
        public Response<IReadOnlyList<ErrorDefinition>> ResolveByProfileName(
            ErrorCatalogContext? context,
            string profileName) =>
            Response<IReadOnlyList<ErrorDefinition>>.Invalid(
                code: "TEST_PROFILE_SELECTION_NOT_CONFIGURED",
                message: "Test-only profile selection service.");
    }
}
