using System.Collections;
using System.Reflection;
using System.Text.Json.Serialization;
using Afrowave.Toolbox.Essentials.Metadata;
using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Tests.PublicApi;

public sealed class CatalogDocumentPublicApiContractTests
{
    [Fact]
    public void CatalogDocuments_PreservePublishedPropertyShape()
    {
        foreach (DocumentContract contract in Documents)
        {
            Type type = contract.Type;

            Assert.True(type.IsPublic);
            Assert.True(type.IsSealed);
            Assert.NotNull(type.GetConstructor(Type.EmptyTypes));

            PropertyInfo[] properties = type.GetProperties(
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly);

            Assert.Equal(11, properties.Length);

            AssertProperty(type, "SchemaVersion", typeof(string));
            AssertProperty(type, "CatalogId", typeof(string));
            AssertProperty(type, "CatalogName", typeof(string));
            AssertProperty(type, "Description", typeof(string));
            AssertProperty(type, "Language", typeof(string));
            AssertProperty(type, "SourceCatalogId", typeof(string));
            AssertProperty(type, "SourceCatalogVersion", typeof(string));
            AssertProperty(type, "IsShadowCopy", typeof(bool));
            AssertProperty(type, "Tags", typeof(List<string>));
            AssertProperty(type, "Metadata", typeof(MetadataBag));
            AssertProperty(
                type,
                contract.ContentPropertyName,
                contract.ContentPropertyType);
        }
    }

    [Fact]
    public void CatalogDocuments_PreservePublishedJsonFieldNames()
    {
        Dictionary<string, string> commonJsonNames = new()
        {
            ["SchemaVersion"] = "schemaVersion",
            ["CatalogId"] = "catalogId",
            ["CatalogName"] = "catalogName",
            ["Description"] = "description",
            ["Language"] = "language",
            ["SourceCatalogId"] = "sourceCatalogId",
            ["SourceCatalogVersion"] = "sourceCatalogVersion",
            ["IsShadowCopy"] = "isShadowCopy",
            ["Tags"] = "tags",
            ["Metadata"] = "metadata"
        };

        foreach (DocumentContract contract in Documents)
        {
            Type type = contract.Type;

            foreach ((string propertyName, string jsonName) in commonJsonNames)
            {
                AssertJsonName(type, propertyName, jsonName);
            }

            AssertJsonName(
                type,
                contract.ContentPropertyName,
                contract.ContentJsonName);
        }
    }

    [Fact]
    public void CatalogDocuments_PreserveDefaultsAndPerInstanceCollections()
    {
        foreach (DocumentContract contract in Documents)
        {
            object first = Activator.CreateInstance(contract.Type)!;
            object second = Activator.CreateInstance(contract.Type)!;

            Assert.Equal("1.0", Get<string>(first, "SchemaVersion"));
            Assert.Equal(string.Empty, Get<string>(first, "CatalogId"));
            Assert.Equal(string.Empty, Get<string>(first, "CatalogName"));
            Assert.Null(Get<object?>(first, "Description"));
            Assert.Equal("en", Get<string>(first, "Language"));
            Assert.Null(Get<object?>(first, "SourceCatalogId"));
            Assert.Null(Get<object?>(first, "SourceCatalogVersion"));
            Assert.False(Get<bool>(first, "IsShadowCopy"));

            List<string> firstTags = Get<List<string>>(first, "Tags");
            List<string> secondTags = Get<List<string>>(second, "Tags");
            Assert.Empty(firstTags);
            Assert.Empty(secondTags);
            Assert.NotSame(firstTags, secondTags);

            MetadataBag firstMetadata = Get<MetadataBag>(first, "Metadata");
            MetadataBag secondMetadata = Get<MetadataBag>(second, "Metadata");
            Assert.True(firstMetadata.IsEmpty);
            Assert.True(secondMetadata.IsEmpty);
            Assert.NotSame(firstMetadata, secondMetadata);

            IList firstContent =
                Assert.IsAssignableFrom<IList>(
                    Get<object>(first, contract.ContentPropertyName));

            IList secondContent =
                Assert.IsAssignableFrom<IList>(
                    Get<object>(second, contract.ContentPropertyName));

            Assert.Empty(firstContent);
            Assert.Empty(secondContent);
            Assert.NotSame(firstContent, secondContent);
        }
    }

    [Fact]
    public void CatalogDocuments_PreserveNullableAndNonNullableReferenceAnnotations()
    {
        NullabilityInfoContext nullability = new();

        foreach (DocumentContract contract in Documents)
        {
            Type type = contract.Type;

            foreach (string propertyName in new[]
                     {
                         "Description",
                         "SourceCatalogId",
                         "SourceCatalogVersion"
                     })
            {
                Assert.Equal(
                    NullabilityState.Nullable,
                    nullability.Create(type.GetProperty(propertyName)!).ReadState);
            }

            foreach (string propertyName in new[]
                     {
                         "SchemaVersion",
                         "CatalogId",
                         "CatalogName",
                         "Language",
                         "Tags",
                         "Metadata",
                         contract.ContentPropertyName
                     })
            {
                Assert.Equal(
                    NullabilityState.NotNull,
                    nullability.Create(type.GetProperty(propertyName)!).ReadState);
            }
        }
    }

    private static IReadOnlyList<DocumentContract> Documents { get; } =
    [
        new(
            typeof(ErrorCatalogDocument),
            "Errors",
            "errors",
            typeof(List<ErrorDefinition>)),
        new(
            typeof(ErrorCategoryCatalogDocument),
            "Categories",
            "categories",
            typeof(List<ErrorCategoryDefinition>)),
        new(
            typeof(ErrorOwnerCatalogDocument),
            "Owners",
            "owners",
            typeof(List<ErrorOwnerDefinition>)),
        new(
            typeof(ErrorCodeGroupCatalogDocument),
            "CodeGroups",
            "codeGroups",
            typeof(List<ErrorCodeGroupDefinition>)),
        new(
            typeof(ErrorProfileCatalogDocument),
            "Profiles",
            "profiles",
            typeof(List<ErrorProfileDefinition>))
    ];

    private static void AssertProperty(
        Type type,
        string name,
        Type propertyType)
    {
        PropertyInfo? property = type.GetProperty(
            name,
            BindingFlags.Public |
            BindingFlags.Instance |
            BindingFlags.DeclaredOnly);

        Assert.NotNull(property);
        Assert.Equal(propertyType, property.PropertyType);
        Assert.True(property.GetMethod?.IsPublic == true);
        Assert.True(property.SetMethod?.IsPublic == true);
    }

    private static void AssertJsonName(
        Type type,
        string propertyName,
        string expectedJsonName)
    {
        PropertyInfo property = type.GetProperty(propertyName)!;

        JsonPropertyNameAttribute? attribute =
            property.GetCustomAttribute<JsonPropertyNameAttribute>();

        Assert.NotNull(attribute);
        Assert.Equal(expectedJsonName, attribute.Name);
    }

    private static T Get<T>(object instance, string propertyName)
    {
        object? value =
            instance.GetType().GetProperty(propertyName)!.GetValue(instance);

        return (T)value!;
    }

    private sealed record DocumentContract(
        Type Type,
        string ContentPropertyName,
        string ContentJsonName,
        Type ContentPropertyType);
}
