using System.Reflection;
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

public sealed class ErrorProfileCatalogSnapshotContractTests
{
    [Fact]
    public void ProfileSnapshot_DetachesEveryFilterDocumentCollectionMappingAndMetadata()
    {
        ErrorProfileCatalogDocument document = CreateCatalog();
        ErrorProfileDefinition source = Assert.Single(document.Profiles);
        StubRuntime runtime = new(Response<ErrorCatalogContext>.Ok(
            new ErrorCatalogContext { ProfileCatalog = document }));

        Response<ErrorProfileCatalogSnapshot> response =
            runtime.GetProfileCatalogSnapshot();

        Assert.True(response.IsSuccess);
        Assert.Equal(1, runtime.ContextReads);
        ErrorProfileCatalogSnapshot captured =
            Assert.IsType<ErrorProfileCatalogSnapshot>(response.Data);
        ErrorProfileDefinitionSnapshot profile = Assert.Single(captured.Profiles);

        Assert.Equal("profiles.example", captured.CatalogId);
        Assert.Equal("Initial profiles", captured.CatalogName);
        Assert.Equal("en", captured.Language);
        Assert.Equal("WEB", profile.Name);
        Assert.Equal("Web", profile.DisplayName);
        Assert.Equal("Project", profile.Source);
        Assert.Equal("OWNER", Assert.Single(profile.IncludeOwners));
        Assert.Equal("GROUP", Assert.Single(profile.IncludeCodeGroups));
        Assert.Equal("CATEGORY", Assert.Single(profile.IncludeCategories));
        Assert.Equal("SUBCATEGORY", Assert.Single(profile.IncludeSubcategories));
        Assert.Equal("IMPORTANT", Assert.Single(profile.IncludeTags));
        Assert.Equal("AFW-0001", Assert.Single(profile.IncludeErrors));
        Assert.Equal("HIDDEN", Assert.Single(profile.ExcludeTags));
        Assert.Equal("AFW-0002", Assert.Single(profile.ExcludeErrors));
        Assert.Equal("true", profile.DefaultMappings["web.problemDetails"]);
        Assert.Equal("before", profile.Metadata["note"]);
        Assert.Equal("before", captured.Metadata["note"]);
        Assert.Equal("catalog-tag", Assert.Single(captured.Tags));

        document.CatalogName = "Changed";
        document.Tags.Add("later");
        document.Metadata.Set("note", "after");
        document.Profiles.Add(new ErrorProfileDefinition { Name = "ADDED" });
        source.Name = "CHANGED";
        source.Source = "User";
        source.IncludeOwners.Clear();
        source.IncludeCodeGroups.Add("CHANGED");
        source.IncludeCategories.Clear();
        source.IncludeSubcategories.Clear();
        source.IncludeTags.Clear();
        source.IncludeErrors.Clear();
        source.ExcludeTags.Clear();
        source.ExcludeErrors.Clear();
        source.DefaultMappings["web.problemDetails"] = "false";
        source.Metadata.Set("note", "after");

        Assert.Equal("Initial profiles", captured.CatalogName);
        Assert.Equal("before", captured.Metadata["note"]);
        Assert.Single(captured.Tags);
        Assert.Single(captured.Profiles);
        Assert.Equal("WEB", profile.Name);
        Assert.Equal("Project", profile.Source);
        Assert.Equal("OWNER", Assert.Single(profile.IncludeOwners));
        Assert.Equal("GROUP", Assert.Single(profile.IncludeCodeGroups));
        Assert.Equal("CATEGORY", Assert.Single(profile.IncludeCategories));
        Assert.Equal("SUBCATEGORY", Assert.Single(profile.IncludeSubcategories));
        Assert.Equal("IMPORTANT", Assert.Single(profile.IncludeTags));
        Assert.Equal("AFW-0001", Assert.Single(profile.IncludeErrors));
        Assert.Equal("HIDDEN", Assert.Single(profile.ExcludeTags));
        Assert.Equal("AFW-0002", Assert.Single(profile.ExcludeErrors));
        Assert.Equal("true", profile.DefaultMappings["web.problemDetails"]);
        Assert.Equal("before", profile.Metadata["note"]);
        Assert.True(profile.DefaultMappings.ContainsKey("WEB.PROBLEMDETAILS"));
        Assert.True(profile.Metadata.ContainsKey("NOTE"));

        Assert.Throws<NotSupportedException>(
            () => ((IList<ErrorProfileDefinitionSnapshot>)captured.Profiles).Clear());
        Assert.Throws<NotSupportedException>(
            () => ((IList<string>)captured.Tags).Add("DENIED"));
        foreach (IReadOnlyList<string> list in new[]
                 {
                     profile.IncludeOwners, profile.IncludeCodeGroups,
                     profile.IncludeCategories, profile.IncludeSubcategories,
                     profile.IncludeTags, profile.IncludeErrors,
                     profile.ExcludeTags, profile.ExcludeErrors
                 })
        {
            Assert.Throws<NotSupportedException>(
                () => ((IList<string>)list).Add("DENIED"));
        }
        Assert.Throws<NotSupportedException>(
            () => ((IDictionary<string, string>)profile.DefaultMappings)
                .Add("DENIED", "value"));
        Assert.Throws<NotSupportedException>(
            () => ((IDictionary<string, string>)profile.Metadata)
                .Add("DENIED", "value"));
        Assert.Throws<NotSupportedException>(
            () => ((IDictionary<string, string>)captured.Metadata)
                .Add("DENIED", "value"));
    }

    [Fact]
    public void ProfileSnapshot_ReadsOneContextPerCallAndRetainsOldValuesAfterReplacement()
    {
        StubRuntime runtime = new(Response<ErrorCatalogContext>.Ok(
            new ErrorCatalogContext { ProfileCatalog = CreateCatalog() }));
        ErrorProfileCatalogSnapshot before = Assert.IsType<ErrorProfileCatalogSnapshot>(
            runtime.GetProfileCatalogSnapshot().Data);
        Assert.Equal(1, runtime.ContextReads);

        ErrorProfileCatalogDocument replacement = CreateCatalog();
        replacement.Profiles[0].Name = "NEW";
        runtime.Current = Response<ErrorCatalogContext>.Ok(
            new ErrorCatalogContext { ProfileCatalog = replacement });

        ErrorProfileCatalogSnapshot after = Assert.IsType<ErrorProfileCatalogSnapshot>(
            runtime.GetProfileCatalogSnapshot().Data);

        Assert.Equal(2, runtime.ContextReads);
        Assert.Equal("WEB", Assert.Single(before.Profiles).Name);
        Assert.Equal("NEW", Assert.Single(after.Profiles).Name);
        Assert.NotSame(before, after);
    }

    [Fact]
    public void ProfileSnapshot_ForwardsUninitializedContextWithoutData()
    {
        StubRuntime runtime = new(Response<ErrorCatalogContext>.Invalid(
            code: "ErrorCatalogContextNotInitialized",
            message: "No active context."));

        Response<ErrorProfileCatalogSnapshot> result =
            runtime.GetProfileCatalogSnapshot();

        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Null(result.Data);
        Assert.Contains(result.Issues,
            issue => issue.Code == "ErrorCatalogContextNotInitialized");
        Assert.Equal(1, runtime.ContextReads);
    }

    [Fact]
    public void ProfileSnapshot_MissingProfileCatalogReturnsStableInvalid()
    {
        StubRuntime runtime = new(Response<ErrorCatalogContext>.Ok(
            new ErrorCatalogContext()));

        Response<ErrorProfileCatalogSnapshot> result =
            runtime.GetProfileCatalogSnapshot();

        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Null(result.Data);
        Assert.Contains(result.Issues,
            issue => issue.Code == "WIF_PROFILE_SNAPSHOT_CATALOG_NULL");
    }

    [Fact]
    public void ProfileSnapshot_MalformedNestedFilterFailsWithoutExceptionDetails()
    {
        ErrorProfileCatalogDocument document = CreateCatalog();
        document.Profiles[0].IncludeErrors = null!;
        StubRuntime runtime = new(Response<ErrorCatalogContext>.Ok(
            new ErrorCatalogContext { ProfileCatalog = document }));

        Response<ErrorProfileCatalogSnapshot> result =
            runtime.GetProfileCatalogSnapshot();

        Assert.Equal(ResultStatus.Failed, result.Status);
        Assert.Null(result.Data);
        Assert.Contains(result.Issues,
            issue => issue.Code == "WIF_PROFILE_SNAPSHOT_FAILED");
        Assert.DoesNotContain("NullReferenceException", result.Message);
    }

    [Fact]
    public void ProfileSnapshot_PublicTypesRemainSealedGetterOnlyAndInterfaceUnchanged()
    {
        foreach (Type type in new[]
                 {
                     typeof(ErrorProfileCatalogSnapshot),
                     typeof(ErrorProfileDefinitionSnapshot)
                 })
        {
            Assert.True(type.IsPublic);
            Assert.True(type.IsSealed);
            Assert.Empty(type.GetConstructors(
                BindingFlags.Public | BindingFlags.Instance));
            Assert.All(type.GetProperties(
                    BindingFlags.Public | BindingFlags.Instance |
                    BindingFlags.DeclaredOnly),
                property => Assert.Null(property.SetMethod));
        }

        Assert.Equal(11, typeof(ErrorProfileCatalogSnapshot).GetProperties().Length);
        Assert.Equal(14, typeof(ErrorProfileDefinitionSnapshot).GetProperties().Length);
        Assert.DoesNotContain(typeof(IErrorCatalogRuntime).GetMethods(),
            method => method.Name == "GetProfileCatalogSnapshot");
    }

    private static ErrorProfileCatalogDocument CreateCatalog()
    {
        ErrorProfileDefinition profile = new()
        {
            Name = "WEB",
            DisplayName = "Web",
            Source = "Project",
            IncludeOwners = ["OWNER"],
            IncludeCodeGroups = ["GROUP"],
            IncludeCategories = ["CATEGORY"],
            IncludeSubcategories = ["SUBCATEGORY"],
            IncludeTags = ["IMPORTANT"],
            IncludeErrors = ["AFW-0001"],
            ExcludeTags = ["HIDDEN"],
            ExcludeErrors = ["AFW-0002"],
            DefaultMappings = new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase)
            {
                ["web.problemDetails"] = "true"
            }
        };
        profile.Metadata.Set("note", "before");

        ErrorProfileCatalogDocument document = new()
        {
            CatalogId = "profiles.example",
            CatalogName = "Initial profiles",
            Tags = ["catalog-tag"],
            Profiles = [profile]
        };
        document.Metadata.Set("note", "before");
        return document;
    }

    private sealed class StubRuntime(
        Response<ErrorCatalogContext> initial) : IErrorCatalogRuntime
    {
        public Response<ErrorCatalogContext> Current { get; set; } = initial;
        public int ContextReads { get; private set; }

        public Response<ErrorCatalogContext> GetCurrentContext()
        {
            ContextReads++;
            return Current;
        }

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
