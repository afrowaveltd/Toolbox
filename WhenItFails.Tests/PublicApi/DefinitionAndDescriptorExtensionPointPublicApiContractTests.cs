using System.Reflection;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Descriptors;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Afrowave.Toolbox.WhenItFails.Tests.PublicApi;

public sealed class DefinitionAndDescriptorExtensionPointPublicApiContractTests
{
    [Fact]
    public void DefinitionResolver_PreservesPublishedInterface()
    {
        Type contract = typeof(IErrorDefinitionResolver);
        AssertInterfaceShape(contract, declaredMethodCount: 3);

        RequireMethod(
            contract,
            "FindById",
            typeof(Response<ErrorDefinition>),
            typeof(ErrorCatalogContext),
            typeof(string));

        RequireMethod(
            contract,
            "FindByName",
            typeof(Response<ErrorDefinition>),
            typeof(ErrorCatalogContext),
            typeof(string));

        RequireMethod(
            contract,
            "FindByCode",
            typeof(Response<ErrorDefinition>),
            typeof(ErrorCatalogContext),
            typeof(int));
    }

    [Fact]
    public void DescriptorFactory_PreservesPublishedInterface()
    {
        Type contract = typeof(IErrorDescriptorFactory);
        AssertInterfaceShape(contract, declaredMethodCount: 1);

        RequireMethod(
            contract,
            "Create",
            typeof(ErrorDescriptor),
            typeof(ErrorDefinition));
    }

    [Fact]
    public void DescriptorResolver_PreservesPublishedInterface()
    {
        Type contract = typeof(IErrorDescriptorResolver);
        AssertInterfaceShape(contract, declaredMethodCount: 3);

        RequireMethod(
            contract,
            "CreateById",
            typeof(Response<ErrorDescriptor>),
            typeof(ErrorCatalogContext),
            typeof(string));

        RequireMethod(
            contract,
            "CreateByName",
            typeof(Response<ErrorDescriptor>),
            typeof(ErrorCatalogContext),
            typeof(string));

        RequireMethod(
            contract,
            "CreateByCode",
            typeof(Response<ErrorDescriptor>),
            typeof(ErrorCatalogContext),
            typeof(int));
    }

    [Fact]
    public void DefinitionAndDescriptorInterfaces_PreserveNullableContextAnnotations()
    {
        NullabilityInfoContext nullability = new();

        foreach (Type contract in new[]
                 {
                     typeof(IErrorDefinitionResolver),
                     typeof(IErrorDescriptorResolver)
                 })
        {
            foreach (MethodInfo method in contract.GetMethods(
                         BindingFlags.Public |
                         BindingFlags.Instance |
                         BindingFlags.DeclaredOnly))
            {
                ParameterInfo[] parameters = method.GetParameters();
                Assert.Equal(2, parameters.Length);

                Assert.Equal(
                    typeof(ErrorCatalogContext),
                    parameters[0].ParameterType);

                Assert.Equal(
                    NullabilityState.Nullable,
                    nullability.Create(parameters[0]).ReadState);

                Assert.Equal(
                    NullabilityState.NotNull,
                    nullability.Create(method.ReturnParameter).ReadState);
            }
        }

        MethodInfo create = RequireMethod(
            typeof(IErrorDescriptorFactory),
            "Create",
            typeof(ErrorDescriptor),
            typeof(ErrorDefinition));

        Assert.Equal(
            NullabilityState.NotNull,
            nullability.Create(Assert.Single(create.GetParameters())).ReadState);

        Assert.Equal(
            NullabilityState.NotNull,
            nullability.Create(create.ReturnParameter).ReadState);
    }

    [Fact]
    public void AddWhenItFails_PreservesPreRegisteredDefinitionAndDescriptorServices()
    {
        ServiceCollection services = new();

        CustomDefinitionResolver definitions = new();
        CustomDescriptorFactory factory = new();
        CustomDescriptorResolver descriptors = new();

        services.AddSingleton<IErrorDefinitionResolver>(definitions);
        services.AddSingleton<IErrorDescriptorFactory>(factory);
        services.AddSingleton<IErrorDescriptorResolver>(descriptors);

        services.AddWhenItFails();

        using ServiceProvider provider = services.BuildServiceProvider(
            new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });

        Assert.Same(
            definitions,
            provider.GetRequiredService<IErrorDefinitionResolver>());

        Assert.Same(
            factory,
            provider.GetRequiredService<IErrorDescriptorFactory>());

        Assert.Same(
            descriptors,
            provider.GetRequiredService<IErrorDescriptorResolver>());

        Assert.NotNull(provider.GetRequiredService<IErrorCatalogRuntime>());
    }

    private static void AssertInterfaceShape(Type contract, int declaredMethodCount)
    {
        Assert.True(contract.IsPublic);
        Assert.True(contract.IsInterface);
        Assert.Equal(
            "Afrowave.Toolbox.WhenItFails.Interfaces",
            contract.Namespace);

        Assert.Equal(
            declaredMethodCount,
            contract.GetMethods(
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly).Length);

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

    private sealed class CustomDefinitionResolver : IErrorDefinitionResolver
    {
        public Response<ErrorDefinition> FindById(
            ErrorCatalogContext? context,
            string errorId) => NotConfigured();

        public Response<ErrorDefinition> FindByName(
            ErrorCatalogContext? context,
            string errorName) => NotConfigured();

        public Response<ErrorDefinition> FindByCode(
            ErrorCatalogContext? context,
            int code) => NotConfigured();

        private static Response<ErrorDefinition> NotConfigured() =>
            Response<ErrorDefinition>.Invalid(
                code: "TEST_DEFINITION_RESOLVER_NOT_CONFIGURED",
                message: "Test-only definition resolver.");
    }

    private sealed class CustomDescriptorFactory : IErrorDescriptorFactory
    {
        public ErrorDescriptor Create(ErrorDefinition definition) => new();
    }

    private sealed class CustomDescriptorResolver : IErrorDescriptorResolver
    {
        public Response<ErrorDescriptor> CreateById(
            ErrorCatalogContext? context,
            string errorId) => NotConfigured();

        public Response<ErrorDescriptor> CreateByName(
            ErrorCatalogContext? context,
            string errorName) => NotConfigured();

        public Response<ErrorDescriptor> CreateByCode(
            ErrorCatalogContext? context,
            int code) => NotConfigured();

        private static Response<ErrorDescriptor> NotConfigured() =>
            Response<ErrorDescriptor>.Invalid(
                code: "TEST_DESCRIPTOR_RESOLVER_NOT_CONFIGURED",
                message: "Test-only descriptor resolver.");
    }
}
