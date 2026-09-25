using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Enums;
using Afrowave.Toolbox.WhenItFails.Services;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.PublicApi;

public sealed class SupportingCatalogLiveStateBoundaryTests
{
    [Fact]
    public void CrossValidationResult_DoesNotAutomaticallyRevalidateMutatedSupportingCatalog()
    {
        ErrorCatalogDocument errors = new()
        {
            Errors =
            [
                new ErrorDefinition
                {
                    Id = "AFW-CFG-0001",
                    PrimaryCategory = "CONFIGURATION",
                    Categories = ["CONFIGURATION"]
                }
            ]
        };
        ErrorCategoryCatalogDocument categories = new()
        {
            Categories =
            [
                new ErrorCategoryDefinition { Name = "CONFIGURATION" }
            ]
        };
        ErrorOwnerCatalogDocument owners = new();
        ErrorCodeGroupCatalogDocument codeGroups = new();
        ErrorProfileCatalogDocument profiles = new();
        ErrorCatalogCrossValidator validator = new();

        ErrorCatalogValidationResult validated = validator.Validate(
            errors, owners, codeGroups, categories, profiles);

        Assert.True(validated.IsValid);
        Assert.DoesNotContain(
            validated.Issues,
            issue => issue.Code == "UnknownPrimaryCategory");

        categories.Categories[0].Name = "OTHER_CATEGORY";

        // The previous validation result is a record of an earlier check,
        // not an automatic watcher of the mutable source documents.
        Assert.True(validated.IsValid);

        ErrorCatalogValidationResult revalidated = validator.Validate(
            errors, owners, codeGroups, categories, profiles);

        Assert.False(revalidated.IsValid);
        Assert.Contains(
            revalidated.Issues,
            issue => issue.Code == "UnknownPrimaryCategory");
    }

    [Fact]
    public void PublishedValidationResult_SharesMutableIssueObjectsAcrossReaders()
    {
        ErrorCatalogValidationIssue warning = new()
        {
            Severity = ErrorCatalogValidationSeverity.Warning,
            Code = "InitialWarning"
        };
        ErrorCatalogValidationResult validation = new();
        validation.AddIssue(warning);

        ErrorCatalogContext context = new()
        {
            CrossValidationResult = validation
        };
        ErrorCatalogContextStore store = new();
        store.Set(context);

        ErrorCatalogContext read = store.GetCurrent().Data!;
        Assert.Same(validation, read.CrossValidationResult);
        Assert.True(read.CrossValidationResult.IsValid);

        ErrorCatalogValidationIssue issue = Assert.Single(
            read.CrossValidationResult.Issues);
        issue.Severity = ErrorCatalogValidationSeverity.Error;

        Assert.False(validation.IsValid);
        Assert.False(store.GetCurrent().Data!.CrossValidationResult.IsValid);
        Assert.Same(issue, Assert.Single(
            store.GetCurrent().Data!.CrossValidationResult.Issues));
    }

    [Fact]
    public void PublishedSupportingProfile_ExposesLiveNestedCollectionsAndMappings()
    {
        ErrorProfileDefinition profile = new()
        {
            Name = "WEB",
            IncludeTags = ["INITIAL"]
        };
        ErrorProfileCatalogDocument profiles = new()
        {
            Profiles = [profile]
        };
        ErrorCatalogContext context = new()
        {
            ProfileCatalog = profiles
        };
        ErrorCatalogContextStore store = new();
        store.Set(context);

        ErrorProfileDefinition fromReader = Assert.Single(
            store.GetCurrent().Data!.ProfileCatalog.Profiles);
        Assert.Same(profile, fromReader);

        fromReader.IncludeTags.Add("LATER");
        fromReader.DefaultMappings["web.includeTraceId"] = "true";
        fromReader.Metadata.Set("review", "changed");

        Assert.Equal(2, profile.IncludeTags.Count);
        Assert.Contains("LATER", profile.IncludeTags);
        Assert.Equal("true", profile.DefaultMappings["web.includeTraceId"]);
        Assert.Equal("changed", profiles.Profiles[0].Metadata["review"]);
        Assert.Same(profiles, store.GetCurrent().Data!.ProfileCatalog);
    }
}
