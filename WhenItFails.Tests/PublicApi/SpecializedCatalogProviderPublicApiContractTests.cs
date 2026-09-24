using System.Reflection;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Afrowave.Toolbox.WhenItFails.Tests.PublicApi;

public sealed class SpecializedCatalogProviderPublicApiContractTests
{
    [Fact]
    public void CategoryCatalogProvider_PreservesPublishedInterface()
    {
        AssertProviderContract(
            typeof(IErrorCategoryCatalogProvider),
            typeof(ErrorCategoryCatalogProviderPayload));
    }

    [Fact]
    public void OwnerCatalogProvider_PreservesPublishedInterface()
    {
        AssertProviderContract(
            typeof(IErrorOwnerCatalogProvider),
            typeof(ErrorOwnerCatalogProviderPayload));
    }

    [Fact]
    public void CodeGroupCatalogProvider_PreservesPublishedInterface()
    {
        AssertProviderContract(
            typeof(IErrorCodeGroupCatalogProvider),
            typeof(ErrorCodeGroupCatalogProviderPayload));
    }

    [Fact]
    public void ProfileCatalogProvider_PreservesPublishedInterface()
    {
        AssertProviderContract(
            typeof(IErrorProfileCatalogProvider),
            typeof(ErrorProfileCatalogProviderPayload));
    }

    [Fact]
    public void AddWhenItFails_PreservesPreRegisteredSpecializedCatalogProviders()
    {
        ServiceCollection services = new();

        CategoryProvider category = new();
        OwnerProvider owner = new();
        CodeGroupProvider codeGroup = new();
        ProfileProvider profile = new();

        services.AddSingleton<IErrorCategoryCatalogProvider>(category);
        services.AddSingleton<IErrorOwnerCatalogProvider>(owner);
        services.AddSingleton<IErrorCodeGroupCatalogProvider>(codeGroup);
        services.AddSingleton<IErrorProfileCatalogProvider>(profile);

        services.AddWhenItFails();

        using ServiceProvider provider = services.BuildServiceProvider(
            new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });

        Assert.Same(
            category,
            provider.GetRequiredService<IErrorCategoryCatalogProvider>());

        Assert.Same(
            owner,
            provider.GetRequiredService<IErrorOwnerCatalogProvider>());

        Assert.Same(
            codeGroup,
            provider.GetRequiredService<IErrorCodeGroupCatalogProvider>());

        Assert.Same(
            profile,
            provider.GetRequiredService<IErrorProfileCatalogProvider>());

        Assert.NotNull(provider.GetRequiredService<IErrorCatalogRuntime>());
    }

    private static void AssertProviderContract(
        Type contract,
        Type payloadType)
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

        Assert.Equal("LoadFromFileAsync", method.Name);
        Assert.Equal(
            typeof(Task<>).MakeGenericType(
                typeof(Response<>).MakeGenericType(payloadType)),
            method.ReturnType);

        ParameterInfo[] parameters = method.GetParameters();

        Assert.Equal(2, parameters.Length);
        Assert.Equal("filePath", parameters[0].Name);
        Assert.Equal(typeof(string), parameters[0].ParameterType);
        Assert.False(parameters[0].IsOptional);

        Assert.Equal("cancellationToken", parameters[1].Name);
        Assert.Equal(typeof(CancellationToken), parameters[1].ParameterType);
        Assert.True(parameters[1].IsOptional);
        Assert.True(parameters[1].HasDefaultValue);
    }

    private sealed class CategoryProvider : IErrorCategoryCatalogProvider
    {
        public Task<Response<ErrorCategoryCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(
                Response<ErrorCategoryCatalogProviderPayload>.Invalid(
                    code: "TEST_CATEGORY_PROVIDER_NOT_CONFIGURED",
                    message: "Test-only category catalog provider."));
    }

    private sealed class OwnerProvider : IErrorOwnerCatalogProvider
    {
        public Task<Response<ErrorOwnerCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(
                Response<ErrorOwnerCatalogProviderPayload>.Invalid(
                    code: "TEST_OWNER_PROVIDER_NOT_CONFIGURED",
                    message: "Test-only owner catalog provider."));
    }

    private sealed class CodeGroupProvider : IErrorCodeGroupCatalogProvider
    {
        public Task<Response<ErrorCodeGroupCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(
                Response<ErrorCodeGroupCatalogProviderPayload>.Invalid(
                    code: "TEST_CODE_GROUP_PROVIDER_NOT_CONFIGURED",
                    message: "Test-only code group catalog provider."));
    }

    private sealed class ProfileProvider : IErrorProfileCatalogProvider
    {
        public Task<Response<ErrorProfileCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(
                Response<ErrorProfileCatalogProviderPayload>.Invalid(
                    code: "TEST_PROFILE_PROVIDER_NOT_CONFIGURED",
                    message: "Test-only profile catalog provider."));
    }
}
