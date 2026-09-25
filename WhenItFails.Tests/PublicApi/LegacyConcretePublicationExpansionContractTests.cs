using System.Reflection;
using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Descriptors;
using Afrowave.Toolbox.WhenItFails.Initialization;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Runtime;
using Afrowave.Toolbox.WhenItFails.Services;

namespace Afrowave.Toolbox.WhenItFails.Tests.PublicApi;

/// <summary>
/// Guards the legacy concrete entry points extended by optional publication
/// and completed-observation capabilities since the 0.1.0 API census.
/// </summary>
public sealed class LegacyConcretePublicationExpansionContractTests
{
    [Fact]
    public void ContextStore_KeepsItsParameterlessConstructorAndFourLegacyMembers()
    {
        Type store = typeof(ErrorCatalogContextStore);
        ConstructorInfo constructor = Assert.Single(store.GetConstructors());
        Assert.Empty(constructor.GetParameters());

        Type original = typeof(IErrorCatalogContextStore);
        Assert.Equal(2, original.GetProperties(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Length);
        Assert.Equal(2, original.GetMethods(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Count(method => !method.IsSpecialName));

        Assert.Equal(typeof(bool), original.GetProperty("IsInitialized")!.PropertyType);
        Assert.Equal(typeof(ErrorCatalogContext), original.GetProperty("Current")!.PropertyType);
        Assert.Null(original.GetProperty("Current")!.SetMethod);

        RequireMethod(store, "GetCurrent",
            typeof(Response<ErrorCatalogContext>));
        RequireMethod(store, "Set", typeof(void),
            typeof(ErrorCatalogContext));
        Assert.Equal(typeof(bool), store.GetProperty("IsInitialized")!.PropertyType);
        Assert.Equal(typeof(ErrorCatalogContext), store.GetProperty("Current")!.PropertyType);
    }

    [Fact]
    public void OptionalStoreInterfaces_ExposeOnlyTheTwoAddedPublicationMethods()
    {
        Type store = typeof(ErrorCatalogContextStore);
        Assert.Contains(typeof(IErrorCatalogContextStore), store.GetInterfaces());
        Assert.Contains(typeof(IErrorCatalogContextPublicationReader), store.GetInterfaces());
        Assert.Contains(typeof(IErrorCatalogContextPublisher), store.GetInterfaces());

        RequireMethod(store, "GetCurrentPublication",
            typeof(Response<ErrorCatalogContextPublication>));
        RequireMethod(store, "Publish",
            typeof(ErrorCatalogContextPublication),
            typeof(ErrorCatalogContext));

        Assert.Equal("GetCurrentPublication", Assert.Single(
            typeof(IErrorCatalogContextPublicationReader).GetMethods()).Name);
        Assert.Equal("Publish", Assert.Single(
            typeof(IErrorCatalogContextPublisher).GetMethods()).Name);
        Assert.DoesNotContain(typeof(IErrorCatalogContextStore).GetMethods(),
            method => method.Name is "Publish" or "GetCurrentPublication");
    }

    [Fact]
    public void Runtime_PreservesItsSixDependencyConstructorAndFiveAddedObservationMethods()
    {
        Type runtime = typeof(ErrorCatalogRuntime);
        ConstructorInfo constructor = Assert.Single(runtime.GetConstructors());
        Assert.Equal(new[]
        {
            typeof(IErrorCatalogInitializer),
            typeof(WhenItFailsOptions),
            typeof(IErrorCatalogContextStore),
            typeof(IBuiltInErrorCatalogContextProvider),
            typeof(IErrorDescriptorService),
            typeof(IErrorProfileSelectionService)
        }, constructor.GetParameters().Select(p => p.ParameterType).ToArray());

        RequireMethod(runtime, "GetCurrentContext",
            typeof(Response<ErrorCatalogContext>));
        RequireMethod(runtime, "GetStatus",
            typeof(Response<ErrorCatalogRuntimeStatus>));
        RequireMethod(runtime, "GetCurrentPublication",
            typeof(Response<ErrorCatalogContextPublication>));
        RequireMethod(runtime, "GetCompletedActivation",
            typeof(Response<ErrorCatalogActivationStatusSnapshot>));
        RequireMethod(runtime, "GetCompletedCombinedSnapshot",
            typeof(Response<ErrorCatalogCompletedCombinedSnapshot>));
        RequireMethod(runtime, "GetCompletedSupportingCatalogsSnapshot",
            typeof(Response<ErrorCatalogCompletedSupportingCatalogsSnapshot>));
        RequireMethod(runtime, "GetCompletedFullSnapshot",
            typeof(Response<ErrorCatalogCompletedFullSnapshot>));

        Assert.Contains(typeof(IErrorCatalogRuntime), runtime.GetInterfaces());
        Assert.DoesNotContain(typeof(IErrorCatalogRuntime).GetMethods(),
            method => method.Name.StartsWith("GetCompleted", StringComparison.Ordinal)
                || method.Name == "GetCurrentPublication");
    }

    private static void RequireMethod(
        Type type, string name, Type returnType, params Type[] parameterTypes)
    {
        MethodInfo? method = type.GetMethod(name,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly,
            binder: null, types: parameterTypes, modifiers: null);
        Assert.NotNull(method);
        Assert.Equal(returnType, method.ReturnType);
    }
}
