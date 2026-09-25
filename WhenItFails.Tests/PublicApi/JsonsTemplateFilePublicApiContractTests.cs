using System.Reflection;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Configuration;

namespace Afrowave.Toolbox.WhenItFails.Tests.PublicApi;

public sealed class JsonsTemplateFilePublicApiContractTests
{
    [Fact]
    public void JsonsTemplateFile_PreservesPublishedClrShapeAndNullability()
    {
        Type model = typeof(JsonsTemplateFile);

        Assert.True(model.IsPublic);
        Assert.True(model.IsSealed);
        Assert.Equal(
            "Afrowave.Toolbox.WhenItFails.Bootstrap",
            model.Namespace);
        Assert.NotNull(model.GetConstructor(Type.EmptyTypes));

        PropertyInfo[] properties = model.GetProperties(
            BindingFlags.Public |
            BindingFlags.Instance |
            BindingFlags.DeclaredOnly);

        Assert.Equal(3, properties.Length);

        NullabilityInfoContext nullability = new();

        foreach (string name in new[] { "Name", "TargetFileName", "Content" })
        {
            PropertyInfo? property = model.GetProperty(
                name,
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly);

            Assert.NotNull(property);
            Assert.Equal(typeof(string), property.PropertyType);
            Assert.True(property.GetMethod?.IsPublic == true);
            Assert.True(property.SetMethod?.IsPublic == true);
            Assert.Equal(
                NullabilityState.NotNull,
                nullability.Create(property).ReadState);
        }
    }

    [Fact]
    public void JsonsTemplateFile_PreservesDefaultsAndIndependentAssignments()
    {
        JsonsTemplateFile first = new();
        JsonsTemplateFile second = new();

        Assert.Equal(string.Empty, first.Name);
        Assert.Equal(string.Empty, first.TargetFileName);
        Assert.Equal(string.Empty, first.Content);

        first.Name = "Error catalog";
        first.TargetFileName = "errors.en.json";
        first.Content = "{\"schemaVersion\":\"1.0\"}";

        Assert.Equal("Error catalog", first.Name);
        Assert.Equal("errors.en.json", first.TargetFileName);
        Assert.Equal("{\"schemaVersion\":\"1.0\"}", first.Content);

        Assert.Equal(string.Empty, second.Name);
        Assert.Equal(string.Empty, second.TargetFileName);
        Assert.Equal(string.Empty, second.Content);
    }

    [Fact]
    public void JsonsTemplateProvider_ExposesTypedTemplateCollection()
    {
        Type contract = typeof(IJsonsTemplateProvider);

        MethodInfo? method = contract.GetMethod(
            "GetTemplateFiles",
            BindingFlags.Public |
            BindingFlags.Instance |
            BindingFlags.DeclaredOnly,
            binder: null,
            types: [typeof(JsonsOptions)],
            modifiers: null);

        Assert.NotNull(method);
        Assert.Equal(
            typeof(IReadOnlyList<JsonsTemplateFile>),
            method.ReturnType);

        NullabilityInfoContext nullability = new();
        Assert.Equal(
            NullabilityState.NotNull,
            nullability.Create(method.ReturnParameter).ReadState);
    }
}
