using System.Reflection;
using System.Runtime.CompilerServices;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Descriptors;
using Afrowave.Toolbox.WhenItFails.Initialization;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Runtime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Afrowave.Toolbox.WhenItFails.Tests.PublicApi;

public sealed class CoreEntryPointPublicApiContractTests
{
    [Fact]
    public void RuntimeInterface_PreservesPublishedMethodSignatures()
    {
        Type contract = typeof(IErrorCatalogRuntime);

        Assert.True(contract.IsPublic);
        Assert.True(contract.IsInterface);
        Assert.Equal(
            "Afrowave.Toolbox.WhenItFails.Interfaces",
            contract.Namespace);

        Assert.Equal(
            9,
            contract.GetMethods(
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly).Length);

        MethodInfo registeredInitialization = RequireMethod(
            contract,
            "InitializeAsync",
            typeof(Task<Response<ErrorCatalogInitializationPayload>>),
            typeof(CancellationToken));

        MethodInfo overrideInitialization = RequireMethod(
            contract,
            "InitializeAsync",
            typeof(Task<Response<ErrorCatalogInitializationPayload>>),
            typeof(JsonsOptions),
            typeof(CancellationToken));

        MethodInfo reset = RequireMethod(
            contract,
            "ResetToDefaultsAsync",
            typeof(Task<Response<ErrorCatalogInitializationPayload>>),
            typeof(CancellationToken));

        foreach (MethodInfo method in new[]
                 {
                     registeredInitialization,
                     overrideInitialization,
                     reset
                 })
        {
            ParameterInfo token = Assert.Single(
                method.GetParameters(),
                parameter => parameter.ParameterType == typeof(CancellationToken));

            Assert.True(token.IsOptional);
            Assert.True(token.HasDefaultValue);
        }

        RequireMethod(
            contract,
            "GetCurrentContext",
            typeof(Response<ErrorCatalogContext>));

        RequireMethod(
            contract,
            "GetStatus",
            typeof(Response<ErrorCatalogRuntimeStatus>));

        RequireMethod(
            contract,
            "FromId",
            typeof(Response<ErrorDescriptor>),
            typeof(string));

        RequireMethod(
            contract,
            "FromName",
            typeof(Response<ErrorDescriptor>),
            typeof(string));

        RequireMethod(
            contract,
            "FromCode",
            typeof(Response<ErrorDescriptor>),
            typeof(int));

        RequireMethod(
            contract,
            "ResolveProfile",
            typeof(Response<IReadOnlyList<ErrorDefinition>>),
            typeof(string));
    }

    [Fact]
    public void DependencyInjectionEntryPoint_PreservesFourPublishedOverloads()
    {
        Type entryPoint = typeof(WhenItFailsServiceCollectionExtensions);

        Assert.True(entryPoint.IsPublic);
        Assert.True(entryPoint.IsAbstract && entryPoint.IsSealed);
        Assert.Equal(
            "Microsoft.Extensions.DependencyInjection",
            entryPoint.Namespace);

        MethodInfo[] overloads = entryPoint.GetMethods(
                BindingFlags.Public |
                BindingFlags.Static |
                BindingFlags.DeclaredOnly)
            .Where(method => method.Name == "AddWhenItFails")
            .ToArray();

        Assert.Equal(4, overloads.Length);

        MethodInfo[] expectedOverloads =
        {
            RequireMethod(
                entryPoint,
                "AddWhenItFails",
                typeof(IServiceCollection),
                typeof(IServiceCollection)),

            RequireMethod(
                entryPoint,
                "AddWhenItFails",
                typeof(IServiceCollection),
                typeof(IServiceCollection),
                typeof(WhenItFailsOptions)),

            RequireMethod(
                entryPoint,
                "AddWhenItFails",
                typeof(IServiceCollection),
                typeof(IServiceCollection),
                typeof(IConfigurationSection)),

            RequireMethod(
                entryPoint,
                "AddWhenItFails",
                typeof(IServiceCollection),
                typeof(IServiceCollection),
                typeof(Action<WhenItFailsOptions>))
        };

        foreach (MethodInfo method in expectedOverloads)
        {
            Assert.True(method.IsStatic);
            Assert.True(
                method.IsDefined(
                    typeof(ExtensionAttribute),
                    inherit: false));

            ParameterInfo services = method.GetParameters()[0];
            Assert.Equal("services", services.Name);
        }
    }

    private static MethodInfo RequireMethod(
        Type declaringType,
        string methodName,
        Type returnType,
        params Type[] parameterTypes)
    {
        MethodInfo? method = declaringType.GetMethod(
            methodName,
            BindingFlags.Public |
            BindingFlags.Instance |
            BindingFlags.Static |
            BindingFlags.DeclaredOnly,
            binder: null,
            types: parameterTypes,
            modifiers: null);

        Assert.NotNull(method);
        Assert.Equal(returnType, method.ReturnType);

        return method;
    }
}
