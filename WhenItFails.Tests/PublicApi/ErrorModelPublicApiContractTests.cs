using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Afrowave.Toolbox.Essentials.Metadata;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Descriptors;

namespace Afrowave.Toolbox.WhenItFails.Tests.PublicApi;

public sealed class ErrorModelPublicApiContractTests
{
    [Fact]
    public void ErrorDescriptor_PreservesPublishedPropertyAndJsonShape()
    {
        Type type = typeof(ErrorDescriptor);

        Assert.True(type.IsPublic);
        Assert.False(type.IsSealed);
        Assert.Equal("Afrowave.Toolbox.WhenItFails.Descriptors", type.Namespace);

        AssertPublicModel(
            type,
            ("Id", typeof(string), "id"),
            ("Code", typeof(int), "code"),
            ("Name", typeof(string), "name"),
            ("Owner", typeof(string), "owner"),
            ("CodePrefix", typeof(string), "codePrefix"),
            ("CodeGroup", typeof(string), "codeGroup"),
            ("PrimaryCategory", typeof(string), "primaryCategory"),
            ("Categories", typeof(List<string>), "categories"),
            ("Subcategories", typeof(List<string>), "subcategories"),
            ("Title", typeof(string), "title"),
            ("Message", typeof(string), "message"),
            ("Severity", typeof(string), "severity"),
            ("Detail", typeof(string), "detail"),
            ("OperationName", typeof(string), "operationName"),
            ("ComponentName", typeof(string), "componentName"),
            ("SourceName", typeof(string), "sourceName"),
            ("DeveloperHint", typeof(string), "developerHint"),
            ("DocumentationKey", typeof(string), "documentationKey"),
            ("Tags", typeof(List<string>), "tags"),
            ("Metadata", typeof(MetadataBag), "metadata"),
            ("Exception", typeof(Exception), null));
    }

    [Fact]
    public void ErrorDefinition_PreservesPublishedPropertyAndJsonShape()
    {
        Type type = typeof(ErrorDefinition);

        Assert.True(type.IsPublic);
        Assert.True(type.IsSealed);
        Assert.Equal("Afrowave.Toolbox.WhenItFails.Definitions", type.Namespace);

        AssertPublicModel(
            type,
            ("Id", typeof(string), "id"),
            ("Code", typeof(int), "code"),
            ("Name", typeof(string), "name"),
            ("Owner", typeof(string), "owner"),
            ("CodePrefix", typeof(string), "codePrefix"),
            ("CodeGroup", typeof(string), "codeGroup"),
            ("PrimaryCategory", typeof(string), "primaryCategory"),
            ("Categories", typeof(List<string>), "categories"),
            ("Subcategories", typeof(List<string>), "subcategories"),
            ("Title", typeof(string), "title"),
            ("Message", typeof(string), "message"),
            ("DefaultSeverity", typeof(string), "defaultSeverity"),
            ("DeveloperHint", typeof(string), "developerHint"),
            ("DocumentationKey", typeof(string), "documentationKey"),
            ("Tags", typeof(List<string>), "tags"),
            ("Metadata", typeof(MetadataBag), "metadata"));
    }

    [Fact]
    public void ErrorModels_DefaultValuesAndCollectionsArePerInstance()
    {
        ErrorDescriptor firstDescriptor = new();
        ErrorDescriptor secondDescriptor = new();

        Assert.Equal(string.Empty, firstDescriptor.Id);
        Assert.Equal(string.Empty, firstDescriptor.Message);
        Assert.Equal(0, firstDescriptor.Code);
        Assert.Equal("Error", firstDescriptor.Severity);
        Assert.Null(firstDescriptor.Detail);
        Assert.Null(firstDescriptor.Exception);
        Assert.Empty(firstDescriptor.Categories);
        Assert.Empty(firstDescriptor.Subcategories);
        Assert.Empty(firstDescriptor.Tags);
        Assert.True(firstDescriptor.Metadata.IsEmpty);

        Assert.NotSame(firstDescriptor.Categories, secondDescriptor.Categories);
        Assert.NotSame(firstDescriptor.Subcategories, secondDescriptor.Subcategories);
        Assert.NotSame(firstDescriptor.Tags, secondDescriptor.Tags);
        Assert.NotSame(firstDescriptor.Metadata, secondDescriptor.Metadata);

        ErrorDefinition firstDefinition = new();
        ErrorDefinition secondDefinition = new();

        Assert.Equal(string.Empty, firstDefinition.Id);
        Assert.Equal(string.Empty, firstDefinition.Message);
        Assert.Equal(0, firstDefinition.Code);
        Assert.Equal("Error", firstDefinition.DefaultSeverity);
        Assert.Null(firstDefinition.DeveloperHint);
        Assert.Empty(firstDefinition.Categories);
        Assert.Empty(firstDefinition.Subcategories);
        Assert.Empty(firstDefinition.Tags);
        Assert.True(firstDefinition.Metadata.IsEmpty);

        Assert.NotSame(firstDefinition.Categories, secondDefinition.Categories);
        Assert.NotSame(firstDefinition.Subcategories, secondDefinition.Subcategories);
        Assert.NotSame(firstDefinition.Tags, secondDefinition.Tags);
        Assert.NotSame(firstDefinition.Metadata, secondDefinition.Metadata);
    }

    [Fact]
    public void ErrorModels_SerializeStableFieldsWithoutRuntimeException_AndRoundTripMetadata()
    {
        ErrorDescriptor descriptor = new()
        {
            Id = "AFW-TEST-0001",
            Code = 12345,
            Severity = "Warning",
            Exception = new InvalidOperationException("private exception detail")
        };

        descriptor.Metadata.Set("traceId", "trace-123");
        descriptor.Tags.Add("RETRYABLE");

        string descriptorJson = JsonSerializer.Serialize(descriptor);

        using (JsonDocument document = JsonDocument.Parse(descriptorJson))
        {
            JsonElement root = document.RootElement;

            Assert.Equal("AFW-TEST-0001", root.GetProperty("id").GetString());
            Assert.Equal(12345, root.GetProperty("code").GetInt32());
            Assert.Equal("Warning", root.GetProperty("severity").GetString());
            Assert.Equal("RETRYABLE", root.GetProperty("tags")[0].GetString());
            Assert.Equal(
                "trace-123",
                root.GetProperty("metadata").GetProperty("traceId").GetString());

            Assert.False(root.TryGetProperty("exception", out _));
            Assert.False(root.TryGetProperty("Exception", out _));
            Assert.DoesNotContain("private exception detail", descriptorJson);
        }

        ErrorDescriptor? restoredDescriptor =
            JsonSerializer.Deserialize<ErrorDescriptor>(descriptorJson);

        Assert.NotNull(restoredDescriptor);
        Assert.Equal(descriptor.Id, restoredDescriptor.Id);
        Assert.Null(restoredDescriptor.Exception);
        Assert.Equal("trace-123", restoredDescriptor.Metadata["traceId"]);

        ErrorDefinition definition = new()
        {
            Id = "AFW-TEST-0002",
            DefaultSeverity = "Critical"
        };

        definition.Metadata.Set("module", "catalog");
        definition.Categories.Add("NETWORK");

        string definitionJson = JsonSerializer.Serialize(definition);

        using (JsonDocument document = JsonDocument.Parse(definitionJson))
        {
            JsonElement root = document.RootElement;

            Assert.Equal("Critical", root.GetProperty("defaultSeverity").GetString());
            Assert.False(root.TryGetProperty("severity", out _));
            Assert.Equal("NETWORK", root.GetProperty("categories")[0].GetString());
            Assert.Equal(
                "catalog",
                root.GetProperty("metadata").GetProperty("module").GetString());
        }

        ErrorDefinition? restoredDefinition =
            JsonSerializer.Deserialize<ErrorDefinition>(definitionJson);

        Assert.NotNull(restoredDefinition);
        Assert.Equal(definition.Id, restoredDefinition.Id);
        Assert.Equal("Critical", restoredDefinition.DefaultSeverity);
        Assert.Equal("catalog", restoredDefinition.Metadata["module"]);
    }

    private static void AssertPublicModel(
        Type modelType,
        params (string Name, Type PropertyType, string? JsonName)[] expected)
    {
        ConstructorInfo? constructor =
            modelType.GetConstructor(Type.EmptyTypes);

        Assert.NotNull(constructor);
        Assert.True(constructor.IsPublic);

        PropertyInfo[] properties = modelType.GetProperties(
            BindingFlags.Public |
            BindingFlags.Instance |
            BindingFlags.DeclaredOnly);

        Assert.Equal(expected.Length, properties.Length);

        foreach (var (name, propertyType, jsonName) in expected)
        {
            PropertyInfo? property = modelType.GetProperty(
                name,
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly);

            Assert.NotNull(property);
            Assert.Equal(propertyType, property.PropertyType);
            Assert.True(property.GetMethod?.IsPublic == true);
            Assert.True(property.SetMethod?.IsPublic == true);

            if (jsonName is null)
            {
                Assert.True(
                    property.IsDefined(typeof(JsonIgnoreAttribute), inherit: false));
                Assert.Null(property.GetCustomAttribute<JsonPropertyNameAttribute>());
            }
            else
            {
                Assert.False(
                    property.IsDefined(typeof(JsonIgnoreAttribute), inherit: false));
                Assert.Equal(
                    jsonName,
                    property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name);
            }
        }
    }
}
