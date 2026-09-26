using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class CatalogSchemaVersionCompatibilityContractTests
{
    [Fact]
    public void Validate_AllCatalogFamilies_RejectUnsupportedSchemaVersion()
    {
        ErrorCatalogValidationResult[] results =
        [
            new ErrorCatalogValidator().Validate(
                new ErrorCatalogDocument
                {
                    SchemaVersion = "2.0",
                    CatalogId = "test.errors",
                    CatalogName = "Test Errors",
                    Language = "en"
                }),
            new ErrorCategoryCatalogValidator().Validate(
                new ErrorCategoryCatalogDocument
                {
                    SchemaVersion = "2.0",
                    CatalogId = "test.categories",
                    CatalogName = "Test Categories",
                    Language = "en"
                }),
            new ErrorOwnerCatalogValidator().Validate(
                new ErrorOwnerCatalogDocument
                {
                    SchemaVersion = "2.0",
                    CatalogId = "test.owners",
                    CatalogName = "Test Owners",
                    Language = "en"
                }),
            new ErrorCodeGroupCatalogValidator().Validate(
                new ErrorCodeGroupCatalogDocument
                {
                    SchemaVersion = "2.0",
                    CatalogId = "test.code-groups",
                    CatalogName = "Test Code Groups",
                    Language = "en"
                }),
            new ErrorProfileCatalogValidator().Validate(
                new ErrorProfileCatalogDocument
                {
                    SchemaVersion = "2.0",
                    CatalogId = "test.profiles",
                    CatalogName = "Test Profiles",
                    Language = "en"
                })
        ];

        foreach(ErrorCatalogValidationResult result in results)
        {
            Assert.False(result.IsValid);

            ErrorCatalogValidationIssue issue =
                Assert.Single(
                    result.Issues,
                    candidate => candidate.Code == "UnsupportedSchemaVersion");

            Assert.Equal("schemaVersion", issue.Path);
        }
    }
}
