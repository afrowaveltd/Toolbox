using System.Reflection;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace Afrowave.Toolbox.WhenItFails.Tests.PublicApi;

public sealed class CatalogNormalizationValidationPublicApiContractTests
{
    [Fact]
    public void DocumentNormalizer_PreservesPublishedInterfaceAndNullability()
    {
        Type contract = typeof(IErrorCatalogDocumentNormalizer);
        AssertSingleMethodInterface(contract);

        MethodInfo method = RequireMethod(
            contract,
            "Normalize",
            typeof(ErrorCatalogDocument),
            typeof(ErrorCatalogDocument));

        NullabilityInfoContext nullability = new();

        Assert.Equal(
            NullabilityState.NotNull,
            nullability.Create(method.ReturnParameter).ReadState);

        Assert.Equal(
            NullabilityState.NotNull,
            nullability.Create(Assert.Single(method.GetParameters())).ReadState);
    }

    [Fact]
    public void CatalogValidator_PreservesPublishedInterfaceAndNullableInput()
    {
        Type contract = typeof(IErrorCatalogValidator);
        AssertSingleMethodInterface(contract);

        MethodInfo method = RequireMethod(
            contract,
            "Validate",
            typeof(ErrorCatalogValidationResult),
            typeof(ErrorCatalogDocument));

        NullabilityInfoContext nullability = new();

        Assert.Equal(
            NullabilityState.NotNull,
            nullability.Create(method.ReturnParameter).ReadState);

        Assert.Equal(
            NullabilityState.Nullable,
            nullability.Create(Assert.Single(method.GetParameters())).ReadState);
    }

    [Fact]
    public void AddWhenItFails_PreservesCustomNormalizerAndValidatorRegistrations()
    {
        ServiceCollection services = new();
        CustomDocumentNormalizer normalizer = new();
        CustomCatalogValidator validator = new();

        services.AddSingleton<IErrorCatalogDocumentNormalizer>(normalizer);
        services.AddSingleton<IErrorCatalogValidator>(validator);
        services.AddWhenItFails();

        using ServiceProvider provider = services.BuildServiceProvider(
            new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });

        Assert.Same(
            normalizer,
            provider.GetRequiredService<IErrorCatalogDocumentNormalizer>());

        Assert.Same(
            validator,
            provider.GetRequiredService<IErrorCatalogValidator>());

        Assert.NotNull(provider.GetRequiredService<IErrorCatalogRuntime>());
    }

    [Fact]
    public void DefaultImplementations_PreserveDistinctNullInputBehavior()
    {
        IErrorCatalogDocumentNormalizer normalizer =
            new ErrorCatalogDocumentNormalizer();

        IErrorCatalogValidator validator =
            new ErrorCatalogValidator();

        Assert.Throws<ArgumentNullException>(
            () => normalizer.Normalize(null!));

        ErrorCatalogValidationResult validationResult =
            validator.Validate(null);

        Assert.False(validationResult.IsValid);
        Assert.Contains(
            validationResult.Issues,
            issue => issue.Code == "CatalogDocumentIsNull");
    }

    private static void AssertSingleMethodInterface(Type contract)
    {
        Assert.True(contract.IsPublic);
        Assert.True(contract.IsInterface);
        Assert.Equal(
            "Afrowave.Toolbox.WhenItFails.Interfaces",
            contract.Namespace);

        Assert.Single(
            contract.GetMethods(
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly));

        Assert.Empty(
            contract.GetProperties(
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly));
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

    private sealed class CustomDocumentNormalizer : IErrorCatalogDocumentNormalizer
    {
        public ErrorCatalogDocument Normalize(ErrorCatalogDocument document) =>
            document;
    }

    private sealed class CustomCatalogValidator : IErrorCatalogValidator
    {
        public ErrorCatalogValidationResult Validate(ErrorCatalogDocument? document) =>
            new();
    }
}
