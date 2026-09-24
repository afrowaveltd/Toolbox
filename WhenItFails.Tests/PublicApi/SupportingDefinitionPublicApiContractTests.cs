using System.Reflection;
using System.Text.Json.Serialization;
using Afrowave.Toolbox.Essentials.Metadata;
using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Tests.PublicApi;

public sealed class SupportingDefinitionPublicApiContractTests
{
    [Fact]
    public void SupportingDefinitions_PreservePublishedPropertyShape()
    {
        foreach (ModelContract model in Models)
        {
            Type type = model.Type;

            Assert.True(type.IsPublic);
            Assert.True(type.IsSealed);
            Assert.NotNull(type.GetConstructor(Type.EmptyTypes));

            PropertyInfo[] properties = type.GetProperties(
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly);

            Assert.Equal(model.Properties.Count, properties.Length);

            foreach (PropertyContract property in model.Properties)
            {
                PropertyInfo? reflected = type.GetProperty(
                    property.Name,
                    BindingFlags.Public |
                    BindingFlags.Instance |
                    BindingFlags.DeclaredOnly);

                Assert.NotNull(reflected);
                Assert.Equal(property.Type, reflected.PropertyType);
                Assert.True(reflected.GetMethod?.IsPublic == true);
                Assert.True(reflected.SetMethod?.IsPublic == true);
            }
        }
    }

    [Fact]
    public void SupportingDefinitions_PreservePublishedJsonFieldNames()
    {
        foreach (ModelContract model in Models)
        {
            foreach (PropertyContract property in model.Properties)
            {
                PropertyInfo reflected = model.Type.GetProperty(property.Name)!;

                JsonPropertyNameAttribute? attribute =
                    reflected.GetCustomAttribute<JsonPropertyNameAttribute>();

                Assert.NotNull(attribute);
                Assert.Equal(property.JsonName, attribute.Name);
            }
        }
    }

    [Fact]
    public void SupportingDefinitions_PreserveReferenceNullabilityAnnotations()
    {
        NullabilityInfoContext nullability = new();

        foreach (ModelContract model in Models)
        {
            foreach (PropertyContract property in model.Properties)
            {
                if (property.Nullable is null)
                {
                    continue;
                }

                PropertyInfo reflected = model.Type.GetProperty(property.Name)!;
                NullabilityState expected = property.Nullable.Value
                    ? NullabilityState.Nullable
                    : NullabilityState.NotNull;

                Assert.Equal(
                    expected,
                    nullability.Create(reflected).ReadState);
            }
        }
    }

    private static IReadOnlyList<ModelContract> Models { get; } =
    [
        new(
            typeof(ErrorCategoryDefinition),
            [
                P("Name", "name", typeof(string), false),
                P("DisplayName", "displayName", typeof(string), false),
                P("Description", "description", typeof(string), true),
                P("Aliases", "aliases", typeof(List<string>), false),
                P("ParentCategories", "parentCategories", typeof(List<string>), false),
                P("DefaultTags", "defaultTags", typeof(List<string>), false),
                P("DefaultMappings", "defaultMappings", typeof(Dictionary<string, string>), false),
                P("Metadata", "metadata", typeof(MetadataBag), false)
            ]),
        new(
            typeof(ErrorOwnerDefinition),
            [
                P("Name", "name", typeof(string), false),
                P("DisplayName", "displayName", typeof(string), false),
                P("Description", "description", typeof(string), true),
                P("CodeFrom", "codeFrom", typeof(int)),
                P("CodeTo", "codeTo", typeof(int)),
                P("IsBuiltIn", "isBuiltIn", typeof(bool)),
                P("Aliases", "aliases", typeof(List<string>), false),
                P("DefaultMappings", "defaultMappings", typeof(Dictionary<string, string>), false),
                P("Metadata", "metadata", typeof(MetadataBag), false)
            ]),
        new(
            typeof(ErrorCodeGroupDefinition),
            [
                P("Name", "name", typeof(string), false),
                P("DisplayName", "displayName", typeof(string), false),
                P("CodePrefix", "codePrefix", typeof(string), false),
                P("CodeFrom", "codeFrom", typeof(int)),
                P("CodeTo", "codeTo", typeof(int)),
                P("Description", "description", typeof(string), true),
                P("DefaultCategories", "defaultCategories", typeof(List<string>), false),
                P("DefaultTags", "defaultTags", typeof(List<string>), false),
                P("DefaultMappings", "defaultMappings", typeof(Dictionary<string, string>), false),
                P("Metadata", "metadata", typeof(MetadataBag), false)
            ]),
        new(
            typeof(ErrorProfileDefinition),
            [
                P("Name", "name", typeof(string), false),
                P("DisplayName", "displayName", typeof(string), false),
                P("Description", "description", typeof(string), true),
                P("Source", "source", typeof(string), false),
                P("IncludeOwners", "includeOwners", typeof(List<string>), false),
                P("IncludeCodeGroups", "includeCodeGroups", typeof(List<string>), false),
                P("IncludeCategories", "includeCategories", typeof(List<string>), false),
                P("IncludeSubcategories", "includeSubcategories", typeof(List<string>), false),
                P("IncludeTags", "includeTags", typeof(List<string>), false),
                P("IncludeErrors", "includeErrors", typeof(List<string>), false),
                P("ExcludeTags", "excludeTags", typeof(List<string>), false),
                P("ExcludeErrors", "excludeErrors", typeof(List<string>), false),
                P("DefaultMappings", "defaultMappings", typeof(Dictionary<string, string>), false),
                P("Metadata", "metadata", typeof(MetadataBag), false)
            ])
    ];

    private static PropertyContract P(
        string name,
        string jsonName,
        Type type,
        bool? nullable = null) =>
        new(name, jsonName, type, nullable);

    private sealed record ModelContract(
        Type Type,
        IReadOnlyList<PropertyContract> Properties);

    private sealed record PropertyContract(
        string Name,
        string JsonName,
        Type Type,
        bool? Nullable);
}
