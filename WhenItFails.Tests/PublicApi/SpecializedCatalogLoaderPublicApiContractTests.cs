using System.Reflection;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Afrowave.Toolbox.WhenItFails.Tests.PublicApi;

public sealed class SpecializedCatalogLoaderPublicApiContractTests
{
    [Fact]
    public void CategoryCatalogLoader_PreservesPublishedInterface()
    {
        AssertLoaderContract(
            typeof(IErrorCategoryCatalogLoader),
            typeof(ErrorCategoryCatalogDocument));
    }

    [Fact]
    public void OwnerCatalogLoader_PreservesPublishedInterface()
    {
        AssertLoaderContract(
            typeof(IErrorOwnerCatalogLoader),
            typeof(ErrorOwnerCatalogDocument));
    }

    [Fact]
    public void CodeGroupCatalogLoader_PreservesPublishedInterface()
    {
        AssertLoaderContract(
            typeof(IErrorCodeGroupCatalogLoader),
            typeof(ErrorCodeGroupCatalogDocument));
    }

    [Fact]
    public void ProfileCatalogLoader_PreservesPublishedInterface()
    {
        AssertLoaderContract(
            typeof(IErrorProfileCatalogLoader),
            typeof(ErrorProfileCatalogDocument));
    }

    [Fact]
    public void AddWhenItFails_PreservesPreRegisteredSpecializedCatalogLoaders()
    {
        ServiceCollection services = new();

        CategoryLoader category = new();
        OwnerLoader owner = new();
        CodeGroupLoader codeGroup = new();
        ProfileLoader profile = new();

        services.AddSingleton<IErrorCategoryCatalogLoader>(category);
        services.AddSingleton<IErrorOwnerCatalogLoader>(owner);
        services.AddSingleton<IErrorCodeGroupCatalogLoader>(codeGroup);
        services.AddSingleton<IErrorProfileCatalogLoader>(profile);

        services.AddWhenItFails();

        using ServiceProvider provider = services.BuildServiceProvider(
            new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });

        Assert.Same(
            category,
            provider.GetRequiredService<IErrorCategoryCatalogLoader>());

        Assert.Same(
            owner,
            provider.GetRequiredService<IErrorOwnerCatalogLoader>());

        Assert.Same(
            codeGroup,
            provider.GetRequiredService<IErrorCodeGroupCatalogLoader>());

        Assert.Same(
            profile,
            provider.GetRequiredService<IErrorProfileCatalogLoader>());

        Assert.NotNull(provider.GetRequiredService<IErrorCatalogRuntime>());
    }

    private static void AssertLoaderContract(
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

        Assert.Equal("LoadFromFileAsync", method.Name);
        Assert.Equal(
            typeof(Task<>).MakeGenericType(
                typeof(Response<>).MakeGenericType(documentType)),
            method.ReturnType);

        ParameterInfo[] parameters = method.GetParameters();

        Assert.Equal(2, parameters.Length);
        Assert.Equal(typeof(string), parameters[0].ParameterType);
        Assert.Equal(typeof(CancellationToken), parameters[1].ParameterType);
        Assert.True(parameters[1].IsOptional);
        Assert.True(parameters[1].HasDefaultValue);
    }

    private sealed class CategoryLoader : IErrorCategoryCatalogLoader
    {
        public Task<Response<ErrorCategoryCatalogDocument>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(
                Response<ErrorCategoryCatalogDocument>.Invalid(
                    code: "TEST_CATEGORY_LOADER_NOT_CONFIGURED",
                    message: "Test-only category catalog loader."));
    }

    private sealed class OwnerLoader : IErrorOwnerCatalogLoader
    {
        public Task<Response<ErrorOwnerCatalogDocument>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(
                Response<ErrorOwnerCatalogDocument>.Invalid(
                    code: "TEST_OWNER_LOADER_NOT_CONFIGURED",
                    message: "Test-only owner catalog loader."));
    }

    private sealed class CodeGroupLoader : IErrorCodeGroupCatalogLoader
    {
        public Task<Response<ErrorCodeGroupCatalogDocument>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(
                Response<ErrorCodeGroupCatalogDocument>.Invalid(
                    code: "TEST_CODE_GROUP_LOADER_NOT_CONFIGURED",
                    message: "Test-only code group catalog loader."));
    }

    private sealed class ProfileLoader : IErrorProfileCatalogLoader
    {
        public Task<Response<ErrorProfileCatalogDocument>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(
                Response<ErrorProfileCatalogDocument>.Invalid(
                    code: "TEST_PROFILE_LOADER_NOT_CONFIGURED",
                    message: "Test-only profile catalog loader."));
    }
}
