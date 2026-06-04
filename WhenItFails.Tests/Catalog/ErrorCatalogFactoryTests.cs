using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCatalogFactoryTests
{
   [Fact]
   public void Create_ShouldThrowArgumentNullException_WhenDocumentIsNull()
   {
      ErrorCatalogFactory factory = new();

      Assert.Throws<ArgumentNullException>(() => factory.Create(null!));
   }

   [Fact]
   public void Create_ShouldReturnErrorCatalog()
   {
      ErrorCatalogFactory factory = new();

      ErrorCatalogDocument document = CreateDocument();

      IErrorCatalog catalog = factory.Create(document);

      Assert.NotNull(catalog);
      Assert.Single(catalog.GetAll());
   }

   [Fact]
   public void Create_ShouldCreateSearchableCatalog()
   {
      ErrorCatalogFactory factory = new();

      ErrorCatalogDocument document = CreateDocument();

      IErrorCatalog catalog = factory.Create(document);

      ErrorDefinition? error = catalog.FindById("AFW-CFG-0001");

      Assert.NotNull(error);
      Assert.Equal("MissingConfigurationValue", error.Name);
   }

   [Fact]
   public void Create_ShouldCreateCatalogSearchableByCodePrefix()
   {
      ErrorCatalogFactory factory = new();

      ErrorCatalogDocument document = CreateDocument();

      IErrorCatalog catalog = factory.Create(document);

      IReadOnlyList<ErrorDefinition> errors = catalog.FindByCodePrefix("CFG");

      ErrorDefinition error = Assert.Single(errors);
      Assert.Equal("AFW-CFG-0001", error.Id);
   }

   private static ErrorCatalogDocument CreateDocument()
   {
      return new ErrorCatalogDocument
      {
         SchemaVersion = "1.0",
         CatalogId = "test.catalog",
         CatalogName = "Test Catalog",
         Language = "en",
         Errors =
          [
              new ErrorDefinition
                {
                    Id = "AFW-CFG-0001",
                    Code = 200001,
                    Name = "MissingConfigurationValue",
                    Owner = "Afrowave",
                    CodePrefix = "CFG",
                    CodeGroup = "Configuration",
                    PrimaryCategory = "Configuration",
                    Categories = ["Configuration", "Startup", "Validation"],
                    Subcategories = ["RequiredValue", "AppSettings"],
                    Title = "Missing configuration value",
                    Message = "A required configuration value is missing.",
                    DefaultSeverity = "Error",
                    Tags = ["configuration", "startup"]
                }
          ]
      };
   }
}