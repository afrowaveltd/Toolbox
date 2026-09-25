using System.Reflection;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Documentation;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.PublicApi;

public sealed class SetterUtilityPublicApiContractTests
{
    private const BindingFlags DeclaredPublic =
        BindingFlags.Public | BindingFlags.Instance |
        BindingFlags.Static | BindingFlags.DeclaredOnly;

    [Fact]
    public void JsonCatalogDocumentWriter_PreservesPublicGenericSaveEntryPoint()
    {
        Type type = typeof(JsonCatalogDocumentWriter);
        AssertStandaloneClass(type);

        MethodInfo method = Assert.Single(
            type.GetMethods(DeclaredPublic),
            m => !m.IsSpecialName);

        Assert.Equal("SaveToFileAsync", method.Name);
        Assert.True(method.IsGenericMethodDefinition);
        Assert.False(method.IsStatic);
        Assert.Equal(typeof(Task<Response>), method.ReturnType);

        Type genericArgument = Assert.Single(method.GetGenericArguments());
        Assert.Equal("TDocument", genericArgument.Name);
        Assert.True(
            (genericArgument.GenericParameterAttributes &
             GenericParameterAttributes.ReferenceTypeConstraint) != 0);

        ParameterInfo[] parameters = method.GetParameters();
        Assert.Equal(3, parameters.Length);
        Assert.Equal(genericArgument, parameters[0].ParameterType);
        Assert.Equal("document", parameters[0].Name);
        Assert.Equal(typeof(string), parameters[1].ParameterType);
        Assert.Equal("filePath", parameters[1].Name);
        Assert.False(parameters[0].IsOptional);
        Assert.False(parameters[1].IsOptional);
        AssertOptionalToken(parameters[2]);

        Assert.Equal(
            typeof(Task<Response>),
            method.MakeGenericMethod(typeof(ErrorCatalogDocument)).ReturnType);
    }

    [Fact]
    public void DocumentationKeyGenerator_PreservesPublicConstructorAndMethods()
    {
        Type type = typeof(DocumentationKeyGenerator);
        AssertStandaloneClass(type);

        Assert.Equal(2, type.GetMethods(DeclaredPublic).Count(m => !m.IsSpecialName));

        MethodInfo generate = RequireMethod(
            type, "Generate", typeof(string),
            typeof(string), typeof(string), typeof(IEnumerable<string>));

        Assert.False(generate.IsStatic);
        Assert.Equal(
            new[] { "categoryName", "title", "existingKeys" },
            generate.GetParameters().Select(p => p.Name).ToArray());

        MethodInfo segment = RequireMethod(
            type, "ToSegment", typeof(string), typeof(string));

        Assert.True(segment.IsStatic);
        Assert.Equal("value", Assert.Single(segment.GetParameters()).Name);
    }

    [Fact]
    public void DocumentationKeyFormat_PreservesPublicStaticValidationEntryPoint()
    {
        Type type = typeof(DocumentationKeyFormat);

        Assert.True(type.IsPublic);
        Assert.True(type.IsAbstract && type.IsSealed);
        Assert.Empty(type.GetConstructors(DeclaredPublic));

        MethodInfo method = Assert.Single(
            type.GetMethods(DeclaredPublic),
            m => !m.IsSpecialName);

        Assert.Equal("IsCanonical", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.True(method.IsStatic);
        ParameterInfo parameter = Assert.Single(method.GetParameters());
        Assert.Equal(typeof(string), parameter.ParameterType);
        Assert.Equal("documentationKey", parameter.Name);
    }

    [Fact]
    public void CrossValidator_PreservesStandaloneFiveCatalogSignature()
    {
        Type type = typeof(ErrorCatalogCrossValidator);
        AssertStandaloneClass(type);

        MethodInfo method = Assert.Single(
            type.GetMethods(DeclaredPublic),
            m => !m.IsSpecialName);

        Assert.Equal("Validate", method.Name);
        Assert.False(method.IsStatic);
        Assert.Equal(typeof(ErrorCatalogValidationResult), method.ReturnType);

        ParameterInfo[] parameters = method.GetParameters();
        Assert.Equal(
            new[]
            {
                typeof(ErrorCatalogDocument),
                typeof(ErrorOwnerCatalogDocument),
                typeof(ErrorCodeGroupCatalogDocument),
                typeof(ErrorCategoryCatalogDocument),
                typeof(ErrorProfileCatalogDocument)
            },
            parameters.Select(p => p.ParameterType).ToArray());

        Assert.Equal(
            new[]
            {
                "errorCatalog", "ownerCatalog", "codeGroupCatalog",
                "categoryCatalog", "profileCatalog"
            },
            parameters.Select(p => p.Name).ToArray());

        Assert.All(parameters.Take(4), p => Assert.False(p.IsOptional));
        Assert.True(parameters[4].IsOptional);
        Assert.True(parameters[4].HasDefaultValue);
        Assert.Null(parameters[4].DefaultValue);
    }

    [Fact]
    public async Task SetterUtilities_WorkWithoutDependencyInjectionOrWorkspace()
    {
        DocumentationKeyGenerator generator = new();
        string key = generator.Generate(
            "Network", "Connection timeout", Array.Empty<string>());

        Assert.Equal("when-it-fails/errors/network/connection-timeout", key);
        Assert.True(DocumentationKeyFormat.IsCanonical(key));

        ErrorCatalogValidationResult validation =
            new ErrorCatalogCrossValidator().Validate(
                null,
                new ErrorOwnerCatalogDocument(),
                new ErrorCodeGroupCatalogDocument(),
                new ErrorCategoryCatalogDocument());

        Assert.False(validation.IsValid);
        Assert.Contains(
            validation.Issues,
            issue => issue.Code == "CatalogDocumentIsNull");

        // An empty path is rejected before any filesystem mutation.
        Response save = await new JsonCatalogDocumentWriter()
            .SaveToFileAsync(new ErrorCatalogDocument(), string.Empty);

        Assert.False(save.IsSuccess);
        Assert.Contains(
            save.Issues,
            issue => issue.Code == "FilePathIsEmpty");
    }

    private static void AssertStandaloneClass(Type type)
    {
        Assert.True(type.IsPublic);
        Assert.True(type.IsSealed);
        ConstructorInfo? constructor = type.GetConstructor(Type.EmptyTypes);
        Assert.NotNull(constructor);
        Assert.True(constructor.IsPublic);
    }

    private static MethodInfo RequireMethod(
        Type type,
        string name,
        Type returnType,
        params Type[] parameterTypes)
    {
        MethodInfo? method = type.GetMethod(
            name,
            DeclaredPublic,
            binder: null,
            types: parameterTypes,
            modifiers: null);

        Assert.NotNull(method);
        Assert.Equal(returnType, method.ReturnType);
        return method;
    }

    private static void AssertOptionalToken(ParameterInfo parameter)
    {
        Assert.Equal("cancellationToken", parameter.Name);
        Assert.Equal(typeof(CancellationToken), parameter.ParameterType);
        Assert.True(parameter.IsOptional);
        Assert.True(parameter.HasDefaultValue);
        // A value-type default may be represented as null by reflection.
    }
}
