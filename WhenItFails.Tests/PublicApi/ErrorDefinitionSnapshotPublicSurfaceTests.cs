using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Descriptors;
using Afrowave.Toolbox.WhenItFails.Initialization;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Runtime;

namespace Afrowave.Toolbox.WhenItFails.Tests.PublicApi;

public sealed class ErrorDefinitionSnapshotPublicSurfaceTests
{
    [Fact]
    public void Snapshot_PreservesGetterOnlyClrShapeAndNullableAnnotations()
    {
        Type type = typeof(ErrorDefinitionSnapshot);
        Assert.True(type.IsPublic);
        Assert.True(type.IsSealed);
        Assert.Empty(type.GetConstructors(BindingFlags.Public | BindingFlags.Instance));

        PropertyInfo[] properties = type.GetProperties(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        Assert.Equal(16, properties.Length);
        Assert.All(properties, property =>
        {
            Assert.True(property.GetMethod?.IsPublic == true);
            Assert.Null(property.SetMethod);
        });

        NullabilityInfoContext nullability = new();

        foreach (string name in new[] { "DeveloperHint", "DocumentationKey" })
        {
            PropertyInfo property = Assert.Single(
                properties,
                candidate => candidate.Name == name);

            Assert.Equal(typeof(string), property.PropertyType);
            Assert.Equal(
                NullabilityState.Nullable,
                nullability.Create(property).ReadState);
        }

        foreach (string name in new[]
                 {
                     "Id", "Name", "Owner", "CodePrefix", "CodeGroup",
                     "PrimaryCategory", "Title", "Message", "DefaultSeverity"
                 })
        {
            PropertyInfo property = Assert.Single(
                properties,
                candidate => candidate.Name == name);
            Assert.Equal(typeof(string), property.PropertyType);
            Assert.Equal(
                NullabilityState.NotNull,
                nullability.Create(property).ReadState);
        }

        foreach (string name in new[] { "Categories", "Subcategories", "Tags" })
        {
            PropertyInfo property = Assert.Single(
                properties,
                candidate => candidate.Name == name);

            Assert.Equal(typeof(IReadOnlyList<string>), property.PropertyType);
            NullabilityInfo info = nullability.Create(property);
            Assert.Equal(NullabilityState.NotNull, info.ReadState);
            Assert.Equal(
                NullabilityState.NotNull,
                Assert.Single(info.GenericTypeArguments).ReadState);
        }

        PropertyInfo metadata = Assert.Single(
            properties,
            candidate => candidate.Name == "Metadata");
        Assert.Equal(
            typeof(IReadOnlyDictionary<string, string>),
            metadata.PropertyType);
        Assert.Equal(
            NullabilityState.NotNull,
            nullability.Create(metadata).ReadState);

        PropertyInfo code = Assert.Single(
            properties,
            candidate => candidate.Name == "Code");
        Assert.Equal(typeof(int), code.PropertyType);
    }

    [Fact]
    public void Extension_PreservesAdditivePublicSignatureWithoutChangingRuntimeInterface()
    {
        Type type = typeof(ErrorCatalogSnapshotExtensions);
        Assert.True(type.IsPublic);
        Assert.True(type.IsAbstract && type.IsSealed);

        MethodInfo method = Assert.Single(
            type.GetMethods(
                BindingFlags.Public | BindingFlags.Static |
                BindingFlags.DeclaredOnly),
            candidate => candidate.Name == "GetErrorDefinitionSnapshots");

        Assert.NotNull(method.GetCustomAttribute<ExtensionAttribute>());
        Assert.Equal(
            typeof(Response<IReadOnlyList<ErrorDefinitionSnapshot>>),
            method.ReturnType);
        ParameterInfo parameter = Assert.Single(method.GetParameters());
        Assert.Equal("runtime", parameter.Name);
        Assert.Equal(typeof(IErrorCatalogRuntime), parameter.ParameterType);
        Assert.False(parameter.IsOptional);

        Assert.DoesNotContain(
            typeof(IErrorCatalogRuntime).GetMethods(
                BindingFlags.Public | BindingFlags.Instance |
                BindingFlags.DeclaredOnly),
            candidate => candidate.Name == method.Name);
    }

    [Fact]
    public void Snapshot_DefaultJsonSerializationCapturesDetachedValuesAndMetadata()
    {
        ErrorDefinition source = CreateDefinition();
        source.Categories.Add("CONFIGURATION");
        source.Subcategories.Add("REQUIRED_VALUE");
        source.Tags.Add("startup");
        source.Metadata.Set("OwnerNote", "before");

        StubRuntime runtime = new(() => Response<ErrorCatalogContext>.Ok(
            new ErrorCatalogContext
            {
                ErrorCatalog = new ErrorCatalog([source])
            }));

        ErrorDefinitionSnapshot snapshot = Assert.Single(
            runtime.GetErrorDefinitionSnapshots().Data!);

        source.Title = "changed after capture";
        source.Tags.Add("later");
        source.Metadata.Set("OwnerNote", "after");

        using JsonDocument json = JsonDocument.Parse(
            JsonSerializer.Serialize(snapshot));
        JsonElement root = json.RootElement;

        // Observed default System.Text.Json output; no versioned JSON schema
        // or deserialization constructor guarantee is declared by this test.
        Assert.Equal("AFW-CFG-0001", root.GetProperty("Id").GetString());
        Assert.Equal(200001, root.GetProperty("Code").GetInt32());
        Assert.Equal("Original title", root.GetProperty("Title").GetString());
        Assert.Equal("CONFIGURATION",
            root.GetProperty("Categories")[0].GetString());
        Assert.Equal("REQUIRED_VALUE",
            root.GetProperty("Subcategories")[0].GetString());
        Assert.Equal("startup", root.GetProperty("Tags")[0].GetString());
        Assert.Equal(1, root.GetProperty("Tags").GetArrayLength());
        Assert.Equal("before",
            root.GetProperty("Metadata").GetProperty("OwnerNote").GetString());
        Assert.Equal(JsonValueKind.Null,
            root.GetProperty("DeveloperHint").ValueKind);
        Assert.Equal(JsonValueKind.Null,
            root.GetProperty("DocumentationKey").ValueKind);
        Assert.False(root.TryGetProperty("ErrorCatalog", out _));
        Assert.True(snapshot.Metadata.TryGetValue("ownernote", out string? note));
        Assert.Equal("before", note);
    }

    [Fact]
    public void Snapshot_MalformedSourceMetadataFailsWithoutLeakingExceptionDetails()
    {
        ErrorDefinition source = CreateDefinition();
        source.Metadata = null!;
        StubRuntime runtime = new(() => Response<ErrorCatalogContext>.Ok(
            new ErrorCatalogContext
            {
                ErrorCatalog = new ErrorCatalog([source])
            }));

        Response<IReadOnlyList<ErrorDefinitionSnapshot>> result =
            runtime.GetErrorDefinitionSnapshots();

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Failed, result.Status);
        Assert.Null(result.Data);
        Assert.Contains(
            result.Issues,
            issue => issue.Code == "WIF_ERROR_DEFINITION_SNAPSHOT_FAILED");
        Assert.DoesNotContain("NullReferenceException", result.Message);
    }

    private static ErrorDefinition CreateDefinition() => new()
    {
        Id = "AFW-CFG-0001",
        Code = 200001,
        Name = "KNOWN_CONFIGURATION_ERROR",
        Owner = "AFW",
        CodePrefix = "CFG",
        CodeGroup = "CONFIGURATION",
        PrimaryCategory = "CONFIGURATION",
        Title = "Original title",
        Message = "Original message"
    };

    private sealed class StubRuntime(
        Func<Response<ErrorCatalogContext>> getContext) : IErrorCatalogRuntime
    {
        public Response<ErrorCatalogContext> GetCurrentContext() => getContext();

        public Task<Response<ErrorCatalogInitializationPayload>> InitializeAsync(
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<Response<ErrorCatalogInitializationPayload>> InitializeAsync(
            JsonsOptions options,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<Response<ErrorCatalogInitializationPayload>> ResetToDefaultsAsync(
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Response<ErrorCatalogRuntimeStatus> GetStatus() =>
            throw new NotSupportedException();

        public Response<ErrorDescriptor> FromId(string errorId) =>
            throw new NotSupportedException();

        public Response<ErrorDescriptor> FromName(string errorName) =>
            throw new NotSupportedException();

        public Response<ErrorDescriptor> FromCode(int code) =>
            throw new NotSupportedException();

        public Response<IReadOnlyList<ErrorDefinition>> ResolveProfile(
            string profileName) =>
            throw new NotSupportedException();
    }
}
