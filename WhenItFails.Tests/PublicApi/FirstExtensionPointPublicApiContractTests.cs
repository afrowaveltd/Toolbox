using System.Reflection;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Descriptors;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Afrowave.Toolbox.WhenItFails.Tests.PublicApi;

public sealed class FirstExtensionPointPublicApiContractTests
{
    [Fact]
    public void JsonsTemplateProvider_PreservesPublishedInterface()
    {
        Type type = typeof(IJsonsTemplateProvider);

        AssertInterfaceWithMethodCount(type, 1);

        RequireMethod(
            type,
            "GetTemplateFiles",
            typeof(IReadOnlyList<JsonsTemplateFile>),
            typeof(JsonsOptions));
    }

    [Fact]
    public void CatalogContextProvider_PreservesPublishedInterface()
    {
        Type type = typeof(IErrorCatalogContextProvider);

        AssertInterfaceWithMethodCount(type, 1);

        MethodInfo method = RequireMethod(
            type,
            "LoadFromJsonsAsync",
            typeof(Task<Response<ErrorCatalogContext>>),
            typeof(JsonsOptions),
            typeof(CancellationToken));

        AssertOptionalCancellationToken(method);
    }

    [Fact]
    public void DescriptorService_PreservesPublishedInterface()
    {
        Type type = typeof(IErrorDescriptorService);

        AssertInterfaceWithMethodCount(type, 3);

        RequireMethod(
            type,
            "FromId",
            typeof(Response<ErrorDescriptor>),
            typeof(ErrorCatalogContext),
            typeof(string));

        RequireMethod(
            type,
            "FromName",
            typeof(Response<ErrorDescriptor>),
            typeof(ErrorCatalogContext),
            typeof(string));

        RequireMethod(
            type,
            "FromCode",
            typeof(Response<ErrorDescriptor>),
            typeof(ErrorCatalogContext),
            typeof(int));
    }

    [Fact]
    public void AddWhenItFails_PreservesPreRegisteredExtensionImplementations()
    {
        ServiceCollection services = new();
        CustomTemplateProvider templates = new();
        CustomContextProvider contexts = new();
        CustomDescriptorService descriptors = new();

        services.AddSingleton<IJsonsTemplateProvider>(templates);
        services.AddSingleton<IErrorCatalogContextProvider>(contexts);
        services.AddSingleton<IErrorDescriptorService>(descriptors);

        services.AddWhenItFails();

        using ServiceProvider provider = services.BuildServiceProvider();

        Assert.Same(
            templates,
            provider.GetRequiredService<IJsonsTemplateProvider>());
        Assert.Same(
            contexts,
            provider.GetRequiredService<IErrorCatalogContextProvider>());
        Assert.Same(
            descriptors,
            provider.GetRequiredService<IErrorDescriptorService>());
    }

    private static void AssertInterfaceWithMethodCount(Type type, int count)
    {
        Assert.True(type.IsPublic);
        Assert.True(type.IsInterface);
        Assert.Equal(
            "Afrowave.Toolbox.WhenItFails.Interfaces",
            type.Namespace);
        Assert.Equal(
            count,
            type.GetMethods(
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly).Length);
    }

    private static MethodInfo RequireMethod(
        Type type,
        string name,
        Type returnType,
        params Type[] parameterTypes)
    {
        MethodInfo? method = type.GetMethod(
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

    private static void AssertOptionalCancellationToken(MethodInfo method)
    {
        ParameterInfo token = Assert.Single(
            method.GetParameters(),
            parameter => parameter.ParameterType == typeof(CancellationToken));

        Assert.True(token.IsOptional);
        Assert.True(token.HasDefaultValue);
    }

    private sealed class CustomTemplateProvider : IJsonsTemplateProvider
    {
        public IReadOnlyList<JsonsTemplateFile> GetTemplateFiles(
            JsonsOptions options) =>
            Array.Empty<JsonsTemplateFile>();
    }

    private sealed class CustomContextProvider : IErrorCatalogContextProvider
    {
        public Task<Response<ErrorCatalogContext>> LoadFromJsonsAsync(
            JsonsOptions options,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(
                Response<ErrorCatalogContext>.Invalid(
                    code: "CUSTOM_CONTEXT_NOT_CONFIGURED",
                    message: "This extension stub does not load a catalog."));
    }

    private sealed class CustomDescriptorService : IErrorDescriptorService
    {
        public Response<ErrorDescriptor> FromId(
            ErrorCatalogContext? context,
            string errorId) =>
            NotConfigured();

        public Response<ErrorDescriptor> FromName(
            ErrorCatalogContext? context,
            string errorName) =>
            NotConfigured();

        public Response<ErrorDescriptor> FromCode(
            ErrorCatalogContext? context,
            int code) =>
            NotConfigured();

        private static Response<ErrorDescriptor> NotConfigured() =>
            Response<ErrorDescriptor>.Invalid(
                code: "CUSTOM_DESCRIPTOR_NOT_CONFIGURED",
                message: "This extension stub does not resolve descriptors.");
    }
}
