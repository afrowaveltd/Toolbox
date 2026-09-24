using System.Reflection;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace Afrowave.Toolbox.WhenItFails.Tests.PublicApi;

public sealed class SpecializedCatalogValidatorPublicApiContractTests
{
    [Fact]
    public void CategoryCatalogValidator_PreservesPublishedInterface()
    {
        AssertValidatorContract(
            typeof(IErrorCategoryCatalogValidator),
            typeof(ErrorCategoryCatalogDocument));
    }

    [Fact]
    public void OwnerCatalogValidator_PreservesPublishedInterface()
    {
        AssertValidatorContract(
            typeof(IErrorOwnerCatalogValidator),
            typeof(ErrorOwnerCatalogDocument));
    }

    [Fact]
    public void CodeGroupCatalogValidator_PreservesPublishedInterface()
    {
        AssertValidatorContract(
            typeof(IErrorCodeGroupCatalogValidator),
            typeof(ErrorCodeGroupCatalogDocument));
    }

    [Fact]
    public void ProfileCatalogValidator_PreservesPublishedInterface()
    {
        AssertValidatorContract(
            typeof(IErrorProfileCatalogValidator),
            typeof(ErrorProfileCatalogDocument));
    }

    [Fact]
    public void AddWhenItFails_PreservesPreRegisteredSpecializedCatalogValidators()
    {
        ServiceCollection services = new();

        CategoryValidator category = new();
        OwnerValidator owner = new();
        CodeGroupValidator codeGroup = new();
        ProfileValidator profile = new();

        services.AddSingleton<IErrorCategoryCatalogValidator>(category);
        services.AddSingleton<IErrorOwnerCatalogValidator>(owner);
        services.AddSingleton<IErrorCodeGroupCatalogValidator>(codeGroup);
        services.AddSingleton<IErrorProfileCatalogValidator>(profile);

        services.AddWhenItFails();

        using ServiceProvider provider = services.BuildServiceProvider(
            new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });

        Assert.Same(
            category,
            provider.GetRequiredService<IErrorCategoryCatalogValidator>());

        Assert.Same(
            owner,
            provider.GetRequiredService<IErrorOwnerCatalogValidator>());

        Assert.Same(
            codeGroup,
            provider.GetRequiredService<IErrorCodeGroupCatalogValidator>());

        Assert.Same(
            profile,
            provider.GetRequiredService<IErrorProfileCatalogValidator>());

        Assert.NotNull(provider.GetRequiredService<IErrorCatalogRuntime>());
    }

    private static void AssertValidatorContract(
        Type contract,
        Type documentType)
    {
        Assert.True(contract.IsPublic);
        Assert.True(contract.IsInterface);
        Assert.Equal(
            "Afrowave.Toolbox.WhenItFails.Interfaces",
            contract.Namespace);

        MethodInfo method = Assert.Single(
            contract.GetMethods(
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly));

        Assert.Empty(
            contract.GetProperties(
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly));

        Assert.Equal("Validate", method.Name);
        Assert.Equal(
            typeof(ErrorCatalogValidationResult),
            method.ReturnType);

        ParameterInfo parameter = Assert.Single(method.GetParameters());
        Assert.Equal("document", parameter.Name);
        Assert.Equal(documentType, parameter.ParameterType);
        Assert.False(parameter.IsOptional);

        NullabilityInfoContext nullability = new();
        Assert.Equal(
            NullabilityState.Nullable,
            nullability.Create(parameter).ReadState);
        Assert.Equal(
            NullabilityState.NotNull,
            nullability.Create(method.ReturnParameter).ReadState);
    }

    private sealed class CategoryValidator : IErrorCategoryCatalogValidator
    {
        public ErrorCatalogValidationResult Validate(
            ErrorCategoryCatalogDocument? document) => new();
    }

    private sealed class OwnerValidator : IErrorOwnerCatalogValidator
    {
        public ErrorCatalogValidationResult Validate(
            ErrorOwnerCatalogDocument? document) => new();
    }

    private sealed class CodeGroupValidator : IErrorCodeGroupCatalogValidator
    {
        public ErrorCatalogValidationResult Validate(
            ErrorCodeGroupCatalogDocument? document) => new();
    }

    private sealed class ProfileValidator : IErrorProfileCatalogValidator
    {
        public ErrorCatalogValidationResult Validate(
            ErrorProfileCatalogDocument? document) => new();
    }
}
