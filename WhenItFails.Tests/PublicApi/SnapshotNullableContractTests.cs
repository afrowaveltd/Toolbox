using System.Reflection;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Runtime;

namespace Afrowave.Toolbox.WhenItFails.Tests.PublicApi;

/// <summary>
/// Protects C# nullable reference annotations separately from runtime
/// malformed-source behavior. These assertions cover CLR metadata rather
/// than inferred source-level spelling alone.
/// </summary>
public sealed class SnapshotNullableContractTests
{
    private static readonly NullabilityInfoContext Nullability = new();

    [Fact]
    public void CompletedObservationModels_RequireTheirSnapshotAndRecordedStatus()
    {
        foreach (Type type in new[]
                 {
                     typeof(ErrorCatalogCompletedFullSnapshot),
                     typeof(ErrorCatalogCompletedSupportingCatalogsSnapshot),
                     typeof(ErrorCatalogCompletedCombinedSnapshot)
                 })
        {
            AssertNonNullable(type, "Status", "Snapshot");
        }
    }

    [Fact]
    public void FullAndSupportingViews_ExposeNonNullableReferenceProperties()
    {
        AssertNonNullable(typeof(ErrorCatalogFullSnapshot),
            "Definitions", "CategoryCatalog", "OwnerCatalog",
            "CodeGroupCatalog", "ProfileCatalog", "Validation");

        AssertNonNullable(typeof(ErrorSupportingCatalogsSnapshot),
            "CategoryCatalog", "OwnerCatalog", "CodeGroupCatalog", "ProfileCatalog");

        AssertNonNullable(typeof(ErrorCatalogCombinedSnapshot),
            "Definitions", "CategoryCatalog", "Validation");

        AssertNonNullable(typeof(ErrorCatalogPublishedSupportingCatalogsSnapshot),
            "Snapshot");
    }

    [Fact]
    public void CapturedListsAndMappings_AnnotateTheirElementsAsNonNullable()
    {
        foreach ((Type type, string name) in new[]
                 {
                     (typeof(ErrorCatalogFullSnapshot), "Definitions"),
                     (typeof(ErrorCategoryCatalogSnapshot), "Categories"),
                     (typeof(ErrorOwnerCatalogSnapshot), "Owners"),
                     (typeof(ErrorCodeGroupCatalogSnapshot), "CodeGroups"),
                     (typeof(ErrorProfileCatalogSnapshot), "Profiles"),
                     (typeof(ErrorProfileDefinitionSnapshot), "IncludeOwners"),
                     (typeof(ErrorProfileDefinitionSnapshot), "ExcludeErrors"),
                     (typeof(ErrorDefinitionSnapshot), "Tags")
                 })
        {
            NullabilityInfo info = GetInfo(type, name);
            Assert.Equal(NullabilityState.NotNull, info.ReadState);
            Assert.Single(info.GenericTypeArguments);
            Assert.Equal(NullabilityState.NotNull,
                info.GenericTypeArguments[0].ReadState);
        }

        foreach ((Type type, string name) in new[]
                 {
                     (typeof(ErrorProfileDefinitionSnapshot), "DefaultMappings"),
                     (typeof(ErrorProfileDefinitionSnapshot), "Metadata"),
                     (typeof(ErrorDefinitionSnapshot), "Metadata"),
                     (typeof(ErrorCategoryCatalogSnapshot), "Metadata")
                 })
        {
            NullabilityInfo info = GetInfo(type, name);
            Assert.Equal(NullabilityState.NotNull, info.ReadState);
            Assert.Equal(2, info.GenericTypeArguments.Length);
            Assert.All(info.GenericTypeArguments,
                element => Assert.Equal(NullabilityState.NotNull, element.ReadState));
        }
    }

    [Fact]
    public void OptionalCatalogAndDefinitionFields_RemainAnnotatedNullable()
    {
        foreach (Type type in new[]
                 {
                     typeof(ErrorCategoryCatalogSnapshot),
                     typeof(ErrorOwnerCatalogSnapshot),
                     typeof(ErrorCodeGroupCatalogSnapshot),
                     typeof(ErrorProfileCatalogSnapshot)
                 })
        {
            AssertNullable(type, "Description", "SourceCatalogId",
                "SourceCatalogVersion");
        }

        AssertNullable(typeof(ErrorProfileDefinitionSnapshot), "Description");
        AssertNullable(typeof(ErrorDefinitionSnapshot),
            "DeveloperHint", "DocumentationKey");

        AssertNonNullable(typeof(ErrorProfileCatalogSnapshot),
            "CatalogId", "CatalogName", "Language", "Tags", "Metadata", "Profiles");
        AssertNonNullable(typeof(ErrorProfileDefinitionSnapshot),
            "Name", "DisplayName", "Source", "IncludeOwners",
            "DefaultMappings", "Metadata");
    }

    [Fact]
    public void OptionalObservationReaders_ReturnNonNullableResponseEnvelopes()
    {
        foreach ((Type reader, string name, Type payload) in new[]
                 {
                     (typeof(IErrorCatalogRuntimeFullObservationReader),
                         "GetCompletedFullSnapshot", typeof(ErrorCatalogCompletedFullSnapshot)),
                     (typeof(IErrorCatalogRuntimeSupportingObservationReader),
                         "GetCompletedSupportingCatalogsSnapshot",
                         typeof(ErrorCatalogCompletedSupportingCatalogsSnapshot)),
                     (typeof(IErrorCatalogRuntimeCombinedObservationReader),
                         "GetCompletedCombinedSnapshot",
                         typeof(ErrorCatalogCompletedCombinedSnapshot)),
                     (typeof(IErrorCatalogRuntimePublicationReader),
                         "GetCurrentPublication", typeof(ErrorCatalogContextPublication))
                 })
        {
            MethodInfo method = Assert.IsAssignableFrom<MethodInfo>(reader.GetMethod(name));
            Assert.Equal(typeof(Response<>).MakeGenericType(payload), method.ReturnType);
            Assert.Equal(NullabilityState.NotNull,
                Nullability.Create(method.ReturnParameter).ReadState);
        }
    }

    [Fact]
    public void RuntimeStatus_DistinguishesOptionalRecoveryDetailsFromRequiredPath()
    {
        AssertNullable(typeof(ErrorCatalogRuntimeStatus),
            "RecoveryReasonCode", "RecoveryMessage");

        AssertNonNullable(typeof(ErrorCatalogRuntimeStatus),
            "PackageDirectoryPath");
        Assert.Equal(typeof(Afrowave.Toolbox.Essentials.Enums.ResultStatus?),
            typeof(ErrorCatalogRuntimeStatus).GetProperty("RecoveryStatus")!.PropertyType);

        // The enclosing Essentials response can represent success with no data:
        // consumers must check both success and a non-null Data payload.
        Response<ErrorCatalogCompletedFullSnapshot> response =
            Response<ErrorCatalogCompletedFullSnapshot>.Ok(null);
        Assert.True(response.IsSuccess);
        Assert.Null(response.Data);
    }

    private static void AssertNullable(Type type, params string[] names)
    {
        foreach (string name in names)
        {
            Assert.Equal(NullabilityState.Nullable, GetInfo(type, name).ReadState);
        }
    }

    private static void AssertNonNullable(Type type, params string[] names)
    {
        foreach (string name in names)
        {
            Assert.Equal(NullabilityState.NotNull, GetInfo(type, name).ReadState);
        }
    }

    private static NullabilityInfo GetInfo(Type type, string name)
    {
        PropertyInfo property = Assert.IsAssignableFrom<PropertyInfo>(
            type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance));
        return Nullability.Create(property);
    }
}
