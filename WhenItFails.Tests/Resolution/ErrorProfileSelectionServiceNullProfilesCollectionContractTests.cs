using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Resolution;

namespace Afrowave.Toolbox.WhenItFails.Tests.Resolution;

public sealed class ErrorProfileSelectionServiceNullProfilesCollectionContractTests
{
    [Fact]
    public void ResolveByProfileName_ShouldReturnInvalidResponse_WhenProfilesCollectionIsNull()
    {
        ErrorProfileSelectionService service = new(
            new ErrorProfileResolver());

        ErrorCatalogContext context = new()
        {
            ErrorCatalogDocument = new ErrorCatalogDocument(),
            ProfileCatalog = new ErrorProfileCatalogDocument
            {
                Profiles = null!
            }
        };

        Response<IReadOnlyList<ErrorDefinition>> response =
            service.ResolveByProfileName(
                context,
                "WEB_API");

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Equal(
            "ErrorProfileCatalogProfilesCollectionIsNull",
            response.Issues[0].Code);
    }
}
