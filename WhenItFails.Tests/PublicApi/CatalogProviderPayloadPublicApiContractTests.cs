using System.Reflection;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.PublicApi;

public sealed class CatalogProviderPayloadPublicApiContractTests
{
    [Fact]
    public void ProviderPayloads_PreservePublishedPropertyShape()
    {
        foreach (PayloadContract contract in Payloads)
        {
            Type type = contract.Type;

            Assert.True(type.IsPublic);
            Assert.True(type.IsSealed);
            Assert.NotNull(type.GetConstructor(Type.EmptyTypes));

            PropertyInfo[] properties = type.GetProperties(
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly);

            Assert.Equal(contract.CatalogType is null ? 2 : 3, properties.Length);

            AssertProperty(type, "Document", contract.DocumentType);
            AssertProperty(type, "ValidationResult", typeof(ErrorCatalogValidationResult));

            if (contract.CatalogType is not null)
            {
                AssertProperty(type, "Catalog", contract.CatalogType);
            }
        }
    }

    [Fact]
    public void ProviderPayloads_PreserveNonNullableAnnotationsAndEmptyConstructorState()
    {
        NullabilityInfoContext nullability = new();

        foreach (PayloadContract contract in Payloads)
        {
            object instance = Activator.CreateInstance(contract.Type)!;

            foreach (PropertyInfo property in contract.Type.GetProperties(
                         BindingFlags.Public |
                         BindingFlags.Instance |
                         BindingFlags.DeclaredOnly))
            {
                Assert.Equal(
                    NullabilityState.NotNull,
                    nullability.Create(property).ReadState);

                // All reference properties currently use null! for construction.
                // A manually constructed payload is not a completed provider result.
                Assert.Null(property.GetValue(instance));
            }
        }
    }

    [Fact]
    public void ProviderPayloads_PreserveAssignedDocumentAndValidationReferences()
    {
        foreach (PayloadContract contract in Payloads)
        {
            object instance = Activator.CreateInstance(contract.Type)!;
            object document = Activator.CreateInstance(contract.DocumentType)!;
            ErrorCatalogValidationResult validation = new();

            PropertyInfo documentProperty = contract.Type.GetProperty("Document")!;
            PropertyInfo validationProperty = contract.Type.GetProperty("ValidationResult")!;

            documentProperty.SetValue(instance, document);
            validationProperty.SetValue(instance, validation);

            Assert.Same(document, documentProperty.GetValue(instance));
            Assert.Same(validation, validationProperty.GetValue(instance));
        }
    }

    private static void AssertProperty(Type type, string name, Type expectedType)
    {
        PropertyInfo? property = type.GetProperty(
            name,
            BindingFlags.Public |
            BindingFlags.Instance |
            BindingFlags.DeclaredOnly);

        Assert.NotNull(property);
        Assert.Equal(expectedType, property.PropertyType);
        Assert.True(property.GetMethod?.IsPublic == true);
        Assert.True(property.SetMethod?.IsPublic == true);
    }

    private static IReadOnlyList<PayloadContract> Payloads { get; } =
    [
        new(
            typeof(ErrorCatalogProviderPayload),
            typeof(ErrorCatalogDocument),
            typeof(IErrorCatalog)),
        new(
            typeof(ErrorCategoryCatalogProviderPayload),
            typeof(ErrorCategoryCatalogDocument)),
        new(
            typeof(ErrorOwnerCatalogProviderPayload),
            typeof(ErrorOwnerCatalogDocument)),
        new(
            typeof(ErrorCodeGroupCatalogProviderPayload),
            typeof(ErrorCodeGroupCatalogDocument)),
        new(
            typeof(ErrorProfileCatalogProviderPayload),
            typeof(ErrorProfileCatalogDocument))
    ];

    private sealed record PayloadContract(
        Type Type,
        Type DocumentType,
        Type? CatalogType = null);
}
