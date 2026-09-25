using System.Reflection;
using System.Runtime.CompilerServices;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Runtime;
using Afrowave.Toolbox.WhenItFails.Services;

namespace Afrowave.Toolbox.WhenItFails.Tests.PublicApi;

/// <summary>
/// Pins the separation between the legacy application interface, optional
/// publication/activation readers and additive snapshot extension methods.
/// </summary>
public sealed class SnapshotCapabilityBoundaryContractTests
{
    [Fact]
    public void OriginalRuntimeInterface_RemainsNineMethodsWithoutSnapshotAdditions()
    {
        MethodInfo[] methods = typeof(IErrorCatalogRuntime).GetMethods(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        Assert.Equal(9, methods.Length);
        Assert.Equal(2, methods.Count(method => method.Name == "InitializeAsync"));
        Assert.Contains(methods, method => method.Name == "GetCurrentContext");
        Assert.Contains(methods, method => method.Name == "GetStatus");

        foreach (string optionalName in new[]
                 {
                     "GetCurrentPublication", "GetCompletedActivation",
                     "GetCompletedCombinedSnapshot",
                     "GetCompletedSupportingCatalogsSnapshot",
                     "GetCompletedFullSnapshot", "GetCombinedSnapshot",
                     "GetSupportingCatalogsSnapshot"
                 })
        {
            Assert.DoesNotContain(methods, method => method.Name == optionalName);
        }
    }

    [Fact]
    public void OptionalRuntimeReaders_KeepIndependentSingleMethodContracts()
    {
        foreach ((Type capability, string name, Type payload) in new[]
                 {
                     (typeof(IErrorCatalogRuntimePublicationReader),
                         "GetCurrentPublication", typeof(ErrorCatalogContextPublication)),
                     (typeof(IErrorCatalogRuntimeActivationReader),
                         "GetCompletedActivation", typeof(ErrorCatalogActivationStatusSnapshot)),
                     (typeof(IErrorCatalogRuntimeCombinedObservationReader),
                         "GetCompletedCombinedSnapshot",
                         typeof(ErrorCatalogCompletedCombinedSnapshot)),
                     (typeof(IErrorCatalogRuntimeSupportingObservationReader),
                         "GetCompletedSupportingCatalogsSnapshot",
                         typeof(ErrorCatalogCompletedSupportingCatalogsSnapshot)),
                     (typeof(IErrorCatalogRuntimeFullObservationReader),
                         "GetCompletedFullSnapshot", typeof(ErrorCatalogCompletedFullSnapshot))
                 })
        {
            Assert.True(capability.IsPublic);
            Assert.True(capability.IsInterface);
            Assert.Empty(capability.GetInterfaces());

            MethodInfo method = Assert.Single(capability.GetMethods(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            Assert.Equal(name, method.Name);
            Assert.Equal(typeof(Response<>).MakeGenericType(payload), method.ReturnType);
            Assert.Empty(method.GetParameters());
        }

        Type runtime = typeof(ErrorCatalogRuntime);
        Assert.Contains(typeof(IErrorCatalogRuntime), runtime.GetInterfaces());
        foreach (Type capability in new[]
                 {
                     typeof(IErrorCatalogRuntimePublicationReader),
                     typeof(IErrorCatalogRuntimeActivationReader),
                     typeof(IErrorCatalogRuntimeCombinedObservationReader),
                     typeof(IErrorCatalogRuntimeSupportingObservationReader),
                     typeof(IErrorCatalogRuntimeFullObservationReader)
                 })
        {
            Assert.Contains(capability, runtime.GetInterfaces());
        }
    }

    [Fact]
    public void DetachedContextExtensions_AreAdditiveSingleArgumentEntryPoints()
    {
        foreach ((Type extension, string methodName, Type payload) in new[]
                 {
                     (typeof(ErrorCategoryCatalogSnapshotExtensions),
                         "GetCategoryCatalogSnapshot", typeof(ErrorCategoryCatalogSnapshot)),
                     (typeof(ErrorOwnerCatalogSnapshotExtensions),
                         "GetOwnerCatalogSnapshot", typeof(ErrorOwnerCatalogSnapshot)),
                     (typeof(ErrorCodeGroupCatalogSnapshotExtensions),
                         "GetCodeGroupCatalogSnapshot", typeof(ErrorCodeGroupCatalogSnapshot)),
                     (typeof(ErrorProfileCatalogSnapshotExtensions),
                         "GetProfileCatalogSnapshot", typeof(ErrorProfileCatalogSnapshot)),
                     (typeof(ErrorCatalogCombinedSnapshotExtensions),
                         "GetCombinedSnapshot", typeof(ErrorCatalogCombinedSnapshot)),
                     (typeof(ErrorSupportingCatalogsSnapshotExtensions),
                         "GetSupportingCatalogsSnapshot", typeof(ErrorSupportingCatalogsSnapshot))
                 })
        {
            AssertRuntimeExtension(extension, methodName, payload);
        }
    }

    [Fact]
    public void PublicationAwareExtensions_UseTheSameLegacyRuntimeParameter()
    {
        AssertRuntimeExtension(
            typeof(ErrorCatalogPublishedCombinedSnapshotExtensions),
            "GetPublishedCombinedSnapshot",
            typeof(ErrorCatalogPublishedCombinedSnapshot));
        AssertRuntimeExtension(
            typeof(ErrorCatalogPublishedSupportingCatalogsSnapshotExtensions),
            "GetPublishedSupportingCatalogsSnapshot",
            typeof(ErrorCatalogPublishedSupportingCatalogsSnapshot));
    }

    private static void AssertRuntimeExtension(Type type, string name, Type payload)
    {
        Assert.True(type.IsPublic);
        Assert.True(type.IsAbstract);
        Assert.True(type.IsSealed);
        MethodInfo method = Assert.Single(type.GetMethods(
            BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));
        Assert.Equal(name, method.Name);
        Assert.True(method.IsDefined(typeof(ExtensionAttribute), inherit: false));
        Assert.Equal(typeof(Response<>).MakeGenericType(payload), method.ReturnType);
        ParameterInfo parameter = Assert.Single(method.GetParameters());
        Assert.Equal(typeof(IErrorCatalogRuntime), parameter.ParameterType);
    }
}
