using System.Reflection;
using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Descriptors;
using Afrowave.Toolbox.WhenItFails.Enums;
using Afrowave.Toolbox.WhenItFails.Initialization;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Runtime;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.PublicApi;

public sealed class ErrorCatalogCombinedSnapshotContractTests
{
    [Fact]
    public void CombinedCapture_DetachesDefinitionsCategoriesAndValidationFromOneContext()
    {
        ErrorCatalogContext context = CreateContext("FIRST");
        ErrorDefinition definition = Assert.Single(context.ErrorCatalog.GetAll());
        ErrorCategoryDefinition category = Assert.Single(
            context.CategoryCatalog.Categories);
        ErrorCatalogValidationIssue issue = Assert.Single(
            context.CrossValidationResult.Issues);

        StubRuntime runtime = new(() => Response<ErrorCatalogContext>.Ok(context));

        Response<ErrorCatalogCombinedSnapshot> response =
            runtime.GetCombinedSnapshot();

        Assert.True(response.IsSuccess);
        Assert.Equal(1, runtime.ContextReadCount);

        ErrorCatalogCombinedSnapshot snapshot =
            Assert.IsType<ErrorCatalogCombinedSnapshot>(response.Data);
        ErrorDefinitionSnapshot error = Assert.Single(snapshot.Definitions);
        ErrorCategoryDefinitionSnapshot capturedCategory =
            Assert.Single(snapshot.CategoryCatalog.Categories);
        ErrorCatalogValidationIssueSnapshot capturedIssue =
            Assert.Single(snapshot.Validation.Issues);

        Assert.Equal("FIRST-ERROR", error.Id);
        Assert.Equal("FIRST-CATEGORY", capturedCategory.Name);
        Assert.Equal("FIRST-WARNING", capturedIssue.Code);
        Assert.True(snapshot.Validation.IsValid);
        Assert.Equal("before", error.Metadata["note"]);
        Assert.Equal("before", capturedCategory.Metadata["note"]);

        definition.Id = "CHANGED-ERROR";
        definition.Metadata.Set("note", "after");
        category.Name = "CHANGED-CATEGORY";
        category.Metadata.Set("note", "after");
        context.CategoryCatalog.Categories.Add(
            new ErrorCategoryDefinition { Name = "ADDED-CATEGORY" });
        issue.Code = "CHANGED-WARNING";
        issue.Severity = ErrorCatalogValidationSeverity.Error;
        context.CrossValidationResult.AddError("ADDED-ERROR", "Later issue");

        Assert.Equal("FIRST-ERROR", error.Id);
        Assert.Equal("before", error.Metadata["note"]);
        Assert.Equal("FIRST-CATEGORY", capturedCategory.Name);
        Assert.Equal("before", capturedCategory.Metadata["note"]);
        Assert.Single(snapshot.CategoryCatalog.Categories);
        Assert.Equal("FIRST-WARNING", capturedIssue.Code);
        Assert.Single(snapshot.Validation.Issues);
        Assert.True(snapshot.Validation.IsValid);

        Assert.Throws<NotSupportedException>(
            () => ((IList<ErrorDefinitionSnapshot>)snapshot.Definitions).Clear());
    }

    [Fact]
    public void CombinedCapture_SelectsOneContextPerCallEvenIfActiveContextChanges()
    {
        ErrorCatalogContext first = CreateContext("FIRST");
        ErrorCatalogContext second = CreateContext("SECOND");
        Response<ErrorCatalogContext> current = Response<ErrorCatalogContext>.Ok(first);

        StubRuntime runtime = new(() =>
        {
            Response<ErrorCatalogContext> selected = current;
            current = Response<ErrorCatalogContext>.Ok(second);
            return selected;
        });

        ErrorCatalogCombinedSnapshot oldSnapshot =
            Assert.IsType<ErrorCatalogCombinedSnapshot>(
                runtime.GetCombinedSnapshot().Data);

        Assert.Equal(1, runtime.ContextReadCount);
        Assert.Equal("FIRST-ERROR", Assert.Single(oldSnapshot.Definitions).Id);
        Assert.Equal("FIRST-CATEGORY",
            Assert.Single(oldSnapshot.CategoryCatalog.Categories).Name);
        Assert.Equal("FIRST-WARNING",
            Assert.Single(oldSnapshot.Validation.Issues).Code);

        ErrorCatalogCombinedSnapshot newSnapshot =
            Assert.IsType<ErrorCatalogCombinedSnapshot>(
                runtime.GetCombinedSnapshot().Data);

        Assert.Equal(2, runtime.ContextReadCount);
        Assert.Equal("SECOND-ERROR", Assert.Single(newSnapshot.Definitions).Id);
        Assert.Equal("SECOND-CATEGORY",
            Assert.Single(newSnapshot.CategoryCatalog.Categories).Name);
        Assert.Equal("SECOND-WARNING",
            Assert.Single(newSnapshot.Validation.Issues).Code);
        Assert.Equal("FIRST-ERROR", Assert.Single(oldSnapshot.Definitions).Id);
    }

    [Fact]
    public void CombinedCapture_ForwardsUninitializedResponseWithoutData()
    {
        StubRuntime runtime = new(() => Response<ErrorCatalogContext>.Invalid(
            code: "ErrorCatalogContextNotInitialized",
            message: "Error catalog context has not been initialized."));

        Response<ErrorCatalogCombinedSnapshot> result =
            runtime.GetCombinedSnapshot();

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Null(result.Data);
        Assert.Contains(result.Issues,
            issue => issue.Code == "ErrorCatalogContextNotInitialized");
        Assert.Equal(1, runtime.ContextReadCount);
    }

    [Theory]
    [InlineData(0, "WIF_COMBINED_SNAPSHOT_ERROR_CATALOG_NULL")]
    [InlineData(1, "WIF_COMBINED_SNAPSHOT_CATEGORY_CATALOG_NULL")]
    [InlineData(2, "WIF_COMBINED_SNAPSHOT_VALIDATION_RESULT_NULL")]
    public void CombinedCapture_RejectsMissingMandatoryComponents(
        int missingComponent, string expectedIssueCode)
    {
        ErrorCatalogContext context = CreateContext("FIRST");

        switch (missingComponent)
        {
            case 0:
                context.ErrorCatalog = null!;
                break;
            case 1:
                context.CategoryCatalog = null!;
                break;
            case 2:
                context.CrossValidationResult = null!;
                break;
        }

        StubRuntime runtime = new(() => Response<ErrorCatalogContext>.Ok(context));

        Response<ErrorCatalogCombinedSnapshot> result =
            runtime.GetCombinedSnapshot();

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, result.Status);
        Assert.Null(result.Data);
        Assert.Contains(result.Issues, issue => issue.Code == expectedIssueCode);
        Assert.Equal(1, runtime.ContextReadCount);
    }

    [Fact]
    public void CombinedCapture_MalformedNestedCategoryFailsWithoutPartialData()
    {
        ErrorCatalogContext context = CreateContext("FIRST");
        context.CategoryCatalog.Categories[0].Aliases = null!;
        StubRuntime runtime = new(() => Response<ErrorCatalogContext>.Ok(context));

        Response<ErrorCatalogCombinedSnapshot> result =
            runtime.GetCombinedSnapshot();

        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Failed, result.Status);
        Assert.Null(result.Data);
        Assert.Contains(result.Issues,
            issue => issue.Code == "WIF_COMBINED_SNAPSHOT_FAILED");
        Assert.DoesNotContain("NullReferenceException", result.Message);
        Assert.Equal(1, runtime.ContextReadCount);
    }

    [Fact]
    public void CombinedCapture_PublicTypeIsGetterOnlyAndRuntimeInterfaceIsUnchanged()
    {
        Type type = typeof(ErrorCatalogCombinedSnapshot);
        Assert.True(type.IsPublic);
        Assert.True(type.IsSealed);
        Assert.Empty(type.GetConstructors(
            BindingFlags.Public | BindingFlags.Instance));

        PropertyInfo[] properties = type.GetProperties(
            BindingFlags.Public | BindingFlags.Instance |
            BindingFlags.DeclaredOnly);

        Assert.Equal(3, properties.Length);
        Assert.All(properties, property => Assert.Null(property.SetMethod));
        Assert.Equal(typeof(IReadOnlyList<ErrorDefinitionSnapshot>),
            type.GetProperty("Definitions")!.PropertyType);
        Assert.Equal(typeof(ErrorCategoryCatalogSnapshot),
            type.GetProperty("CategoryCatalog")!.PropertyType);
        Assert.Equal(typeof(ErrorCatalogValidationSnapshot),
            type.GetProperty("Validation")!.PropertyType);

        Assert.DoesNotContain(typeof(IErrorCatalogRuntime).GetMethods(),
            method => method.Name == "GetCombinedSnapshot");
        MethodInfo extension = Assert.Single(
            typeof(ErrorCatalogCombinedSnapshotExtensions).GetMethods(
                BindingFlags.Public | BindingFlags.Static |
                BindingFlags.DeclaredOnly),
            method => method.Name == "GetCombinedSnapshot");
        Assert.Equal(typeof(Response<ErrorCatalogCombinedSnapshot>),
            extension.ReturnType);
        Assert.Equal(typeof(IErrorCatalogRuntime),
            Assert.Single(extension.GetParameters()).ParameterType);
    }

    private static ErrorCatalogContext CreateContext(string prefix)
    {
        ErrorDefinition definition = new()
        {
            Id = prefix + "-ERROR",
            Name = prefix + "_ERROR",
            Categories = [prefix + "-CATEGORY"]
        };
        definition.Metadata.Set("note", "before");

        ErrorCategoryDefinition category = new()
        {
            Name = prefix + "-CATEGORY",
            Aliases = [prefix + "-ALIAS"]
        };
        category.Metadata.Set("note", "before");

        ErrorCatalogValidationResult validation = new();
        validation.AddWarning(prefix + "-WARNING", "Recorded warning");

        return new ErrorCatalogContext
        {
            ErrorCatalog = new ErrorCatalog([definition]),
            CategoryCatalog = new ErrorCategoryCatalogDocument
            {
                Categories = [category]
            },
            CrossValidationResult = validation
        };
    }

    private sealed class StubRuntime(
        Func<Response<ErrorCatalogContext>> current) : IErrorCatalogRuntime
    {
        public int ContextReadCount { get; private set; }

        public Response<ErrorCatalogContext> GetCurrentContext()
        {
            ContextReadCount++;
            return current();
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
