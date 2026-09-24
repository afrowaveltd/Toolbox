using System.Reflection;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Enums;
using Afrowave.Toolbox.WhenItFails.Initialization;

namespace Afrowave.Toolbox.WhenItFails.Tests.PublicApi;

public sealed class InitializationAndBootstrapPayloadPublicApiContractTests
{
    [Fact]
    public void InitializationPayload_PreservesPublishedPropertyShapeAndAnnotations()
    {
        Type type = typeof(ErrorCatalogInitializationPayload);

        AssertPublicSealedParameterlessModel(type, declaredPropertyCount: 6);

        AssertProperty(type, "Bootstrap", typeof(JsonsBootstrapPayload), hasSetter: true);
        AssertProperty(type, "Context", typeof(ErrorCatalogContext), hasSetter: true);
        AssertProperty(type, "ContextSource", typeof(ErrorCatalogContextSource), hasSetter: true);
        AssertProperty(type, "KeptPreviousContext", typeof(bool), hasSetter: true);
        AssertProperty(type, "UsedFallback", typeof(bool), hasSetter: true);
        AssertProperty(type, "IsDegraded", typeof(bool), hasSetter: false);

        NullabilityInfoContext nullability = new();

        foreach (string name in new[] { "Bootstrap", "Context" })
        {
            PropertyInfo property = type.GetProperty(name)!;
            Assert.Equal(
                NullabilityState.NotNull,
                nullability.Create(property).ReadState);
        }
    }

    [Fact]
    public void InitializationPayload_DefaultsAndDegradedStateFollowRecoveryFlags()
    {
        ErrorCatalogInitializationPayload payload = new();

        // Non-nullable annotations do not initialize these two references.
        // The producer must populate them before returning a success payload.
        Assert.Null(payload.Bootstrap);
        Assert.Null(payload.Context);
        Assert.Equal(
            ErrorCatalogContextSource.ProjectCatalog,
            payload.ContextSource);
        Assert.False(payload.KeptPreviousContext);
        Assert.False(payload.UsedFallback);
        Assert.False(payload.IsDegraded);

        payload.KeptPreviousContext = true;
        Assert.True(payload.IsDegraded);

        payload.KeptPreviousContext = false;
        payload.UsedFallback = true;
        Assert.True(payload.IsDegraded);

        payload.KeptPreviousContext = true;
        Assert.True(payload.IsDegraded);

        payload.KeptPreviousContext = false;
        payload.UsedFallback = false;
        Assert.False(payload.IsDegraded);
    }

    [Fact]
    public void BootstrapPayload_PreservesPublishedShapeDefaultsAndPerInstanceFileLists()
    {
        Type type = typeof(JsonsBootstrapPayload);

        AssertPublicSealedParameterlessModel(type, declaredPropertyCount: 5);
        AssertProperty(type, "RootDirectory", typeof(string), hasSetter: true);
        AssertProperty(type, "PackageDirectoryPath", typeof(string), hasSetter: true);
        AssertProperty(type, "PackageDirectoryAlreadyExisted", typeof(bool), hasSetter: true);
        AssertProperty(type, "PackageDirectoryCreated", typeof(bool), hasSetter: true);
        AssertProperty(type, "Files", typeof(List<JsonsBootstrapFileResult>), hasSetter: false);

        JsonsBootstrapPayload first = new();
        JsonsBootstrapPayload second = new();

        Assert.Equal(string.Empty, first.RootDirectory);
        Assert.Equal(string.Empty, first.PackageDirectoryPath);
        Assert.False(first.PackageDirectoryAlreadyExisted);
        Assert.False(first.PackageDirectoryCreated);
        Assert.Empty(first.Files);
        Assert.NotSame(first.Files, second.Files);

        JsonsBootstrapFileResult entry = new() { Name = "Error catalog" };
        first.Files.Add(entry);

        Assert.Same(entry, Assert.Single(first.Files));
        Assert.Empty(second.Files);
    }

    [Fact]
    public void BootstrapFileResult_PreservesPublishedShapeDefaultsAndNullableMessage()
    {
        Type type = typeof(JsonsBootstrapFileResult);

        AssertPublicSealedParameterlessModel(type, declaredPropertyCount: 6);
        AssertProperty(type, "Name", typeof(string), hasSetter: true);
        AssertProperty(type, "TargetFilePath", typeof(string), hasSetter: true);
        AssertProperty(type, "AlreadyExisted", typeof(bool), hasSetter: true);
        AssertProperty(type, "Created", typeof(bool), hasSetter: true);
        AssertProperty(type, "Skipped", typeof(bool), hasSetter: true);
        AssertProperty(type, "Message", typeof(string), hasSetter: true);

        NullabilityInfoContext nullability = new();
        Assert.Equal(
            NullabilityState.Nullable,
            nullability.Create(type.GetProperty("Message")!).ReadState);

        JsonsBootstrapFileResult entry = new();

        Assert.Equal(string.Empty, entry.Name);
        Assert.Equal(string.Empty, entry.TargetFilePath);
        Assert.False(entry.AlreadyExisted);
        Assert.False(entry.Created);
        Assert.False(entry.Skipped);
        Assert.Null(entry.Message);
    }

    private static void AssertPublicSealedParameterlessModel(
        Type type,
        int declaredPropertyCount)
    {
        Assert.True(type.IsPublic);
        Assert.True(type.IsSealed);
        Assert.NotNull(type.GetConstructor(Type.EmptyTypes));

        Assert.Equal(
            declaredPropertyCount,
            type.GetProperties(
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly).Length);
    }

    private static void AssertProperty(
        Type type,
        string name,
        Type propertyType,
        bool hasSetter)
    {
        PropertyInfo? property = type.GetProperty(
            name,
            BindingFlags.Public |
            BindingFlags.Instance |
            BindingFlags.DeclaredOnly);

        Assert.NotNull(property);
        Assert.Equal(propertyType, property.PropertyType);
        Assert.True(property.GetMethod?.IsPublic == true);

        if (hasSetter)
        {
            Assert.True(property.SetMethod?.IsPublic == true);
        }
        else
        {
            Assert.Null(property.SetMethod);
        }
    }
}
