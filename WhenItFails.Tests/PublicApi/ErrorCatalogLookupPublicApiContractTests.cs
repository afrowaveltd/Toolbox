using System.Reflection;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.PublicApi;

public sealed class ErrorCatalogLookupPublicApiContractTests
{
    [Fact]
    public void LookupInterface_PreservesTenPublishedMethodsAndReturnTypes()
    {
        Type contract = typeof(IErrorCatalog);

        Assert.True(contract.IsPublic);
        Assert.True(contract.IsInterface);
        Assert.Equal(
            "Afrowave.Toolbox.WhenItFails.Interfaces",
            contract.Namespace);

        Assert.Equal(
            10,
            contract.GetMethods(
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly).Length);

        Assert.Empty(
            contract.GetProperties(
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly));

        RequireMethod(
            contract,
            "GetAll",
            typeof(IReadOnlyList<ErrorDefinition>));

        foreach (string name in new[] { "FindById", "FindByName" })
        {
            RequireMethod(
                contract,
                name,
                typeof(ErrorDefinition),
                typeof(string));
        }

        RequireMethod(
            contract,
            "FindByCode",
            typeof(ErrorDefinition),
            typeof(int));

        foreach (string name in new[]
                 {
                     "FindByOwner",
                     "FindByCodePrefix",
                     "FindByCodeGroup",
                     "FindByCategory",
                     "FindBySubcategory",
                     "FindByTag"
                 })
        {
            RequireMethod(
                contract,
                name,
                typeof(IReadOnlyList<ErrorDefinition>),
                typeof(string));
        }
    }

    [Fact]
    public void LookupInterface_PreservesNullableSingleResultAndNonNullableCollectionReturns()
    {
        Type contract = typeof(IErrorCatalog);
        NullabilityInfoContext nullability = new();

        foreach (string name in new[] { "FindById", "FindByName" })
        {
            MethodInfo method = RequireMethod(
                contract,
                name,
                typeof(ErrorDefinition),
                typeof(string));

            Assert.Equal(
                NullabilityState.Nullable,
                nullability.Create(method.ReturnParameter).ReadState);
        }

        MethodInfo byCode = RequireMethod(
            contract,
            "FindByCode",
            typeof(ErrorDefinition),
            typeof(int));

        Assert.Equal(
            NullabilityState.Nullable,
            nullability.Create(byCode.ReturnParameter).ReadState);

        foreach (string name in new[]
                 {
                     "GetAll",
                     "FindByOwner",
                     "FindByCodePrefix",
                     "FindByCodeGroup",
                     "FindByCategory",
                     "FindBySubcategory",
                     "FindByTag"
                 })
        {
            Type[] parameters = name == "GetAll"
                ? Type.EmptyTypes
                : new[] { typeof(string) };

            MethodInfo method = RequireMethod(
                contract,
                name,
                typeof(IReadOnlyList<ErrorDefinition>),
                parameters);

            NullabilityInfo result =
                nullability.Create(method.ReturnParameter);

            Assert.Equal(NullabilityState.NotNull, result.ReadState);
            Assert.Equal(
                NullabilityState.NotNull,
                Assert.Single(result.GenericTypeArguments).ReadState);
        }
    }

    [Fact]
    public void PublicFactory_ProducesCatalogUsableThroughLookupInterface()
    {
        ErrorDefinition definition = new()
        {
            Id = "AFW-CFG-0001",
            Code = 200001,
            Name = "KnownConfigurationError",
            Owner = "AFW",
            CodePrefix = "CFG",
            CodeGroup = "Configuration",
            PrimaryCategory = "Configuration"
        };

        ErrorCatalogDocument document = new()
        {
            Errors = new List<ErrorDefinition> { definition }
        };

        IErrorCatalogFactory factory = new ErrorCatalogFactory();
        IErrorCatalog catalog = factory.Create(document);

        Assert.Same(definition, catalog.FindById(definition.Id));
        Assert.Same(definition, catalog.FindByCode(definition.Code));
        Assert.Single(catalog.GetAll());
        Assert.Null(catalog.FindById("UNKNOWN-ID"));
        Assert.Empty(catalog.FindByTag("UNKNOWN-TAG"));
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
}
