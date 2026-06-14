using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Resolution;
using Afrowave.Toolbox.WhenItFails.Validation;



namespace Afrowave.Toolbox.WhenItFails.Tests.Resolution;

public sealed class ErrorProfileSelectionServiceTests
{
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenResolverIsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ErrorProfileSelectionService(null!));
    }

    [Fact]
    public void ResolveByProfileName_ShouldReturnInvalidResponse_WhenContextIsNull()
    {
        ErrorProfileSelectionService service = CreateService();

        Response<IReadOnlyList<ErrorDefinition>> response =
            service.ResolveByProfileName(
                null,
                "WEB_API");

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Equal(
            "ErrorCatalogContextIsNull",
            response.Issues[0].Code);
    }

    [Fact]
    public void ResolveByProfileName_ShouldReturnInvalidResponse_WhenErrorCatalogDocumentIsNull()
    {
        ErrorProfileSelectionService service = CreateService();

        ErrorCatalogContext context = CreateContext();
        context.ErrorCatalogDocument = null!;

        Response<IReadOnlyList<ErrorDefinition>> response =
            service.ResolveByProfileName(
                context,
                "WEB_API");

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Equal(
            "ErrorCatalogDocumentIsNull",
            response.Issues[0].Code);
    }

    [Fact]
    public void ResolveByProfileName_ShouldReturnInvalidResponse_WhenProfileCatalogIsNull()
    {
        ErrorProfileSelectionService service = CreateService();

        ErrorCatalogContext context = CreateContext();
        context.ProfileCatalog = null!;

        Response<IReadOnlyList<ErrorDefinition>> response =
            service.ResolveByProfileName(
                context,
                "WEB_API");

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Equal(
            "ErrorProfileCatalogIsNull",
            response.Issues[0].Code);
    }

    [Fact]
    public void ResolveByProfileName_ShouldReturnInvalidResponse_WhenProfileNameIsEmpty()
    {
        ErrorProfileSelectionService service = CreateService();

        Response<IReadOnlyList<ErrorDefinition>> response =
            service.ResolveByProfileName(
                CreateContext(),
                string.Empty);

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Equal(
            "ProfileNameIsEmpty",
            response.Issues[0].Code);
    }

    [Fact]
    public void ResolveByProfileName_ShouldReturnNotFoundResponse_WhenProfileDoesNotExist()
    {
        ErrorProfileSelectionService service = CreateService();

        Response<IReadOnlyList<ErrorDefinition>> response =
            service.ResolveByProfileName(
                CreateContext(),
                "DOES_NOT_EXIST");

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.NotFound, response.Status);
        Assert.Equal(
            "ErrorProfileNotFoundByName",
            response.Issues[0].Code);
    }

    [Fact]
    public void ResolveByProfileName_ShouldResolveErrors_WhenProfileExists()
    {
        ErrorProfileSelectionService service = CreateService();

        Response<IReadOnlyList<ErrorDefinition>> response =
            service.ResolveByProfileName(
                CreateContext(),
                "WEB_API");

        Assert.True(response.IsSuccess);
        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.NotNull(response.Data);

        ErrorDefinition error = Assert.Single(response.Data);

        Assert.Equal("AFW-WEB-0001", error.Id);
    }

    [Fact]
    public void ResolveByProfileName_ShouldNormalizeProfileNameBeforeSearch()
    {
        ErrorProfileSelectionService service = CreateService();

        Response<IReadOnlyList<ErrorDefinition>> response =
            service.ResolveByProfileName(
                CreateContext(),
                "web api");

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);

        ErrorDefinition error = Assert.Single(response.Data);

        Assert.Equal("AFW-WEB-0001", error.Id);
    }

    [Fact]
    public void ResolveByProfileName_ShouldReturnSuccessfulEmptyCollection_WhenProfileMatchesNoErrors()
    {
        ErrorProfileSelectionService service = CreateService();

        ErrorCatalogContext context = CreateContext();

        context.ProfileCatalog.Profiles.Add(
            new ErrorProfileDefinition
            {
                Name = "EMPTY",
                DisplayName = "Empty",
                IncludeCategories = ["NOT_PRESENT"]
            });

        Response<IReadOnlyList<ErrorDefinition>> response =
            service.ResolveByProfileName(
                context,
                "EMPTY");

        Assert.True(response.IsSuccess);
        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.NotNull(response.Data);
        Assert.Empty(response.Data);
    }

    private static ErrorProfileSelectionService CreateService()
    {
        return new ErrorProfileSelectionService(
            new ErrorProfileResolver());
    }

    private static ErrorCatalogContext CreateContext()
    {
        ErrorDefinition webError = new()
        {
            Id = "AFW-WEB-0001",
            Code = 100_001,
            Name = "WebRequestFailed",
            Owner = "AFW",
            CodePrefix = "WEB",
            CodeGroup = "WEB",
            PrimaryCategory = "WEB",
            Categories = ["WEB", "NETWORK"],
            Subcategories = ["REQUEST"],
            Tags = ["USER_VISIBLE"],
            Title = "Web request failed",
            Message = "The web request failed.",
            DefaultSeverity = "Error"
        };

        ErrorDefinition databaseError = new()
        {
            Id = "AFW-DB-0001",
            Code = 200_001,
            Name = "DatabaseQueryFailed",
            Owner = "AFW",
            CodePrefix = "DB",
            CodeGroup = "DATABASE",
            PrimaryCategory = "DATABASE",
            Categories = ["DATABASE"],
            Subcategories = ["QUERY"],
            Tags = ["INTERNAL_ONLY"],
            Title = "Database query failed",
            Message = "The database query failed.",
            DefaultSeverity = "Error"
        };

        ErrorCatalogDocument errorCatalogDocument = new()
        {
            Errors =
            [
                webError,
                databaseError
            ]
        };

        ErrorProfileCatalogDocument profileCatalog = new()
        {
            Profiles =
            [
                new ErrorProfileDefinition
                {
                    Name = "WEB_API",
                    DisplayName = "Web API",
                    IncludeTags = ["USER_VISIBLE"],
                    ExcludeTags = ["INTERNAL_ONLY"]
                }
            ]
        };

        return new ErrorCatalogContext
        {
            ErrorCatalog = new ErrorCatalog(
                errorCatalogDocument.Errors),

            ErrorCatalogDocument = errorCatalogDocument,

            CategoryCatalog =
                new ErrorCategoryCatalogDocument(),

            CodeGroupCatalog =
                new ErrorCodeGroupCatalogDocument(),

            OwnerCatalog =
                new ErrorOwnerCatalogDocument(),

            ProfileCatalog = profileCatalog,

            CrossValidationResult = new ErrorCatalogValidationResult()
        };
    }
}
