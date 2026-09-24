using System.Reflection;
using System.Runtime.CompilerServices;
using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Enums;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Runtime;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.PublicApi;

public sealed class RuntimeStatePublicApiContractTests
{
    [Fact]
    public void ErrorCatalogContext_PreservesPublishedMutableModelShape()
    {
        Type type = typeof(ErrorCatalogContext);

        Assert.True(type.IsPublic);
        Assert.True(type.IsSealed);
        Assert.Equal(
            "Afrowave.Toolbox.WhenItFails.Catalog",
            type.Namespace);

        AssertPublicParameterlessConstructor(type);

        AssertExactPropertySet(
            type,
            ("ErrorCatalog", typeof(IErrorCatalog)),
            ("ErrorCatalogDocument", typeof(ErrorCatalogDocument)),
            ("CrossValidationResult", typeof(ErrorCatalogValidationResult)),
            ("CategoryCatalog", typeof(ErrorCategoryCatalogDocument)),
            ("CodeGroupCatalog", typeof(ErrorCodeGroupCatalogDocument)),
            ("OwnerCatalog", typeof(ErrorOwnerCatalogDocument)),
            ("ProfileCatalog", typeof(ErrorProfileCatalogDocument)));

        foreach (PropertyInfo property in DeclaredPublicProperties(type))
        {
            Assert.True(property.GetMethod?.IsPublic == true);
            Assert.True(property.SetMethod?.IsPublic == true);
            Assert.False(IsInitOnly(property));
        }
    }

    [Fact]
    public void ErrorCatalogRuntimeStatus_PreservesPublishedInitAndComputedShape()
    {
        Type type = typeof(ErrorCatalogRuntimeStatus);

        Assert.True(type.IsPublic);
        Assert.True(type.IsSealed);
        Assert.Equal(
            "Afrowave.Toolbox.WhenItFails.Runtime",
            type.Namespace);

        AssertPublicParameterlessConstructor(type);

        AssertExactPropertySet(
            type,
            ("ContextSource", typeof(ErrorCatalogContextSource)),
            ("State", typeof(ErrorCatalogRuntimeState)),
            ("IsConsistent", typeof(bool)),
            ("IsDegraded", typeof(bool)),
            ("KeptPreviousContext", typeof(bool)),
            ("UsedFallback", typeof(bool)),
            ("RecoveryReasonCode", typeof(string)),
            ("RecoveryStatus", typeof(ResultStatus?)),
            ("RecoveryMessage", typeof(string)),
            ("ActivatedAtUtc", typeof(DateTimeOffset)),
            ("PackageDirectoryPath", typeof(string)));

        foreach (PropertyInfo property in DeclaredPublicProperties(type))
        {
            Assert.True(property.GetMethod?.IsPublic == true);

            if (property.Name is nameof(ErrorCatalogRuntimeStatus.State)
                or nameof(ErrorCatalogRuntimeStatus.IsConsistent))
            {
                Assert.Null(property.SetMethod);
            }
            else
            {
                Assert.True(property.SetMethod?.IsPublic == true);
                Assert.True(IsInitOnly(property));
            }
        }
    }

    private static PropertyInfo[] DeclaredPublicProperties(Type type) =>
        type.GetProperties(
            BindingFlags.Public |
            BindingFlags.Instance |
            BindingFlags.DeclaredOnly);

    private static bool IsInitOnly(PropertyInfo property) =>
        property.SetMethod?.ReturnParameter
            .GetRequiredCustomModifiers()
            .Contains(typeof(IsExternalInit)) == true;

    private static void AssertPublicParameterlessConstructor(Type type)
    {
        ConstructorInfo? constructor =
            type.GetConstructor(Type.EmptyTypes);

        Assert.NotNull(constructor);
        Assert.True(constructor.IsPublic);
    }

    private static void AssertExactPropertySet(
        Type type,
        params (string Name, Type PropertyType)[] expected)
    {
        PropertyInfo[] actual = DeclaredPublicProperties(type);
        Assert.Equal(expected.Length, actual.Length);

        foreach (var (name, propertyType) in expected)
        {
            PropertyInfo? property = type.GetProperty(
                name,
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly);

            Assert.NotNull(property);
            Assert.Equal(propertyType, property.PropertyType);
        }
    }
}
