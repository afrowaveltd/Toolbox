using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Afrowave.Toolbox.WhenItFails.Descriptors;

namespace Afrowave.Toolbox.WhenItFails.Tests.PublicApi;

public sealed class DescriptorAuxiliaryModelsPublicApiContractTests
{
    private const BindingFlags DeclaredPublic =
        BindingFlags.Public | BindingFlags.Instance |
        BindingFlags.DeclaredOnly;

    [Fact]
    public void DescriptorRequest_PreservesEightOptionalPublicProperties()
    {
        Type type = typeof(ErrorDescriptorRequest);

        Assert.True(type.IsPublic);
        Assert.True(type.IsSealed);
        Assert.NotNull(type.GetConstructor(Type.EmptyTypes));

        PropertyInfo[] properties = type.GetProperties(DeclaredPublic);
        Assert.Equal(8, properties.Length);

        NullabilityInfoContext nullability = new();

        foreach (string name in new[]
                 {
                     "ErrorId",
                     "ErrorName",
                     "Title",
                     "Message",
                     "Severity",
                     "DeveloperHint",
                     "DocumentationKey"
                 })
        {
            PropertyInfo property = RequireProperty(type, name, typeof(string));
            Assert.Equal(
                NullabilityState.Nullable,
                nullability.Create(property).ReadState);
        }

        PropertyInfo code = RequireProperty(type, "Code", typeof(int?));
        Assert.Equal(
            NullabilityState.Nullable,
            nullability.Create(code).ReadState);
    }

    [Fact]
    public void GenericDescriptor_PreservesInheritanceAndTypedAttachmentSurface()
    {
        Type generic = typeof(ErrorDescriptor<>);

        Assert.True(generic.IsPublic);
        Assert.True(generic.IsSealed);
        Assert.True(generic.IsGenericTypeDefinition);
        Assert.Equal(typeof(ErrorDescriptor), generic.BaseType);

        Type attachmentType = Assert.Single(generic.GetGenericArguments());
        Assert.Equal("TAttachment", attachmentType.Name);
        Assert.Equal(
            GenericParameterAttributes.None,
            attachmentType.GenericParameterAttributes &
            GenericParameterAttributes.SpecialConstraintMask);

        Assert.NotNull(generic.GetConstructor(Type.EmptyTypes));

        PropertyInfo attachment = Assert.Single(
            generic.GetProperties(DeclaredPublic));

        Assert.Equal("Attachment", attachment.Name);
        Assert.Equal(attachmentType, attachment.PropertyType);
        Assert.True(attachment.GetMethod?.IsPublic == true);
        Assert.True(attachment.SetMethod?.IsPublic == true);

        JsonPropertyNameAttribute? jsonName =
            attachment.GetCustomAttribute<JsonPropertyNameAttribute>();

        Assert.NotNull(jsonName);
        Assert.Equal("attachment", jsonName.Name);
    }

    [Fact]
    public void GenericDescriptor_JsonContainsTypedAttachmentButExcludesRuntimeException()
    {
        ErrorDescriptor<int> descriptor = new()
        {
            Id = "AFW-CFG-0001",
            Attachment = 42,
            Exception = new InvalidOperationException("runtime-only exception")
        };

        using JsonDocument json = JsonDocument.Parse(
            JsonSerializer.Serialize(descriptor));

        JsonElement root = json.RootElement;

        Assert.Equal("AFW-CFG-0001", root.GetProperty("id").GetString());
        Assert.Equal(42, root.GetProperty("attachment").GetInt32());

        Assert.False(root.TryGetProperty("Exception", out _));
        Assert.False(root.TryGetProperty("exception", out _));

        PropertyInfo inheritedException =
            typeof(ErrorDescriptor).GetProperty(nameof(ErrorDescriptor.Exception))!;

        Assert.NotNull(
            inheritedException.GetCustomAttribute<JsonIgnoreAttribute>());
    }

    private static PropertyInfo RequireProperty(
        Type type,
        string name,
        Type propertyType)
    {
        PropertyInfo? property = type.GetProperty(name, DeclaredPublic);

        Assert.NotNull(property);
        Assert.Equal(propertyType, property.PropertyType);
        Assert.True(property.GetMethod?.IsPublic == true);
        Assert.True(property.SetMethod?.IsPublic == true);

        return property;
    }
}
