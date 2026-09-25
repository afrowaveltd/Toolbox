using System.Reflection;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;

namespace Afrowave.Toolbox.WhenItFails.Tests.PublicApi;

public sealed class JsonCatalogDocumentLoaderPublicApiContractTests
{
    private const BindingFlags DeclaredPublic =
        BindingFlags.Public | BindingFlags.Instance |
        BindingFlags.Static | BindingFlags.DeclaredOnly;

    [Fact]
    public void Loader_PreservesPublicGenericLoadingSignature()
    {
        Type type = typeof(JsonCatalogDocumentLoader);

        Assert.True(type.IsPublic);
        Assert.True(type.IsSealed);
        Assert.NotNull(type.GetConstructor(Type.EmptyTypes));

        MethodInfo method = Assert.Single(
            type.GetMethods(DeclaredPublic),
            candidate => !candidate.IsSpecialName);

        Assert.Equal("LoadFromFileAsync", method.Name);
        Assert.False(method.IsStatic);
        Assert.True(method.IsGenericMethodDefinition);

        Type documentType = Assert.Single(method.GetGenericArguments());
        Assert.Equal("TDocument", documentType.Name);
        Assert.True(
            (documentType.GenericParameterAttributes &
             GenericParameterAttributes.ReferenceTypeConstraint) != 0);

        Assert.Equal(
            typeof(Task<>).MakeGenericType(
                typeof(Response<>).MakeGenericType(documentType)),
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

        MethodInfo concrete = method.MakeGenericMethod(
            typeof(ErrorCategoryCatalogDocument));

        Assert.Equal(
            typeof(Task<Response<ErrorCategoryCatalogDocument>>),
            concrete.ReturnType);
    }

    [Fact]
    public async Task Loader_IsUsableWithoutDi_AndRejectsEmptyPathWithoutFileAccess()
    {
        JsonCatalogDocumentLoader loader = new();

        Response<ErrorCategoryCatalogDocument> result =
            await loader.LoadFromFileAsync<ErrorCategoryCatalogDocument>(
                string.Empty);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Contains(
            result.Issues,
            issue => issue.Code == "FilePathIsEmpty");
    }

    [Fact]
    public async Task Loader_PreservesCancellationBeforeAttemptingFileAccess()
    {
        JsonCatalogDocumentLoader loader = new();
        using CancellationTokenSource source = new();
        source.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => loader.LoadFromFileAsync<ErrorCategoryCatalogDocument>(
                string.Empty,
                source.Token));
    }
}
