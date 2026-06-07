using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCatalogTests
{
   [Fact]
   public void GetAll_ShouldReturnAllDefinitions()
   {
      ErrorCatalog catalog = CreateTestCatalog();

      IReadOnlyList<ErrorDefinition> errors = catalog.GetAll();

      Assert.Equal(3, errors.Count);
   }

   [Fact]
   public void FindById_ShouldReturnMatchingDefinition()
   {
      ErrorCatalog catalog = CreateTestCatalog();

      ErrorDefinition? error = catalog.FindById("AFW-CFG-0001");

      Assert.NotNull(error);
      Assert.Equal("MissingConfigurationValue", error.Name);
   }

   [Fact]
   public void FindById_ShouldBeNormalized()
   {
      ErrorCatalog catalog = CreateTestCatalog();

      ErrorDefinition? error = catalog.FindById("afw cfg 0001");

      Assert.NotNull(error);
      Assert.Equal("AFW-CFG-0001", error.Id);
   }

   [Fact]
   public void FindById_ShouldReturnNull_WhenIdIsUnknown()
   {
      ErrorCatalog catalog = CreateTestCatalog();

      ErrorDefinition? error = catalog.FindById("NOPE-9999");

      Assert.Null(error);
   }

   [Fact]
   public void FindById_ShouldReturnNull_WhenIdIsEmpty()
   {
      ErrorCatalog catalog = CreateTestCatalog();

      ErrorDefinition? error = catalog.FindById(string.Empty);

      Assert.Null(error);
   }

   [Fact]
   public void FindByCode_ShouldReturnMatchingDefinition()
   {
      ErrorCatalog catalog = CreateTestCatalog();

      ErrorDefinition? error = catalog.FindByCode(200001);

      Assert.NotNull(error);
      Assert.Equal("AFW-CFG-0001", error.Id);
   }

   [Fact]
   public void FindByCode_ShouldReturnNull_WhenCodeIsUnknown()
   {
      ErrorCatalog catalog = CreateTestCatalog();

      ErrorDefinition? error = catalog.FindByCode(999999);

      Assert.Null(error);
   }

   [Fact]
   public void FindByName_ShouldReturnMatchingDefinition()
   {
      ErrorCatalog catalog = CreateTestCatalog();

      ErrorDefinition? error = catalog.FindByName("MissingConfigurationValue");

      Assert.NotNull(error);
      Assert.Equal("AFW-CFG-0001", error.Id);
   }

   [Fact]
   public void FindByName_ShouldBeNormalized()
   {
      ErrorCatalog catalog = CreateTestCatalog();

      ErrorDefinition? error = catalog.FindByName("missingconfigurationvalue");

      Assert.NotNull(error);
      Assert.Equal("MissingConfigurationValue", error.Name);
   }

   [Fact]
   public void FindByOwner_ShouldReturnMatchingDefinitions()
   {
      ErrorCatalog catalog = CreateTestCatalog();

      IReadOnlyList<ErrorDefinition> errors = catalog.FindByOwner("Afrowave");

      Assert.Equal(3, errors.Count);
   }

   [Fact]
   public void FindByOwner_ShouldBeNormalized()
   {
      ErrorCatalog catalog = CreateTestCatalog();

      IReadOnlyList<ErrorDefinition> errors = catalog.FindByOwner("afrowave");

      Assert.Equal(3, errors.Count);
   }

   [Fact]
   public void FindByCodePrefix_ShouldReturnMatchingDefinitions()
   {
      ErrorCatalog catalog = CreateTestCatalog();

      IReadOnlyList<ErrorDefinition> errors = catalog.FindByCodePrefix("CFG");

      Assert.Equal(2, errors.Count);
      Assert.Contains(errors, error => error.Id == "AFW-CFG-0001");
      Assert.Contains(errors, error => error.Id == "AFW-CFG-0002");
   }

   [Fact]
   public void FindByCodePrefix_ShouldBeNormalized()
   {
      ErrorCatalog catalog = CreateTestCatalog();

      IReadOnlyList<ErrorDefinition> errors = catalog.FindByCodePrefix("cfg");

      Assert.Equal(2, errors.Count);
   }

   [Fact]
   public void FindByCodeGroup_ShouldReturnMatchingDefinitions()
   {
      ErrorCatalog catalog = CreateTestCatalog();

      IReadOnlyList<ErrorDefinition> errors = catalog.FindByCodeGroup("Configuration");

      Assert.Equal(2, errors.Count);
      Assert.Contains(errors, error => error.Id == "AFW-CFG-0001");
      Assert.Contains(errors, error => error.Id == "AFW-CFG-0002");
   }

   [Fact]
   public void FindByCategory_ShouldReturnMatchingDefinitions_FromPrimaryCategory()
   {
      ErrorCatalog catalog = CreateTestCatalog();

      IReadOnlyList<ErrorDefinition> errors = catalog.FindByCategory("Configuration");

      Assert.Equal(2, errors.Count);
      Assert.Contains(errors, error => error.Id == "AFW-CFG-0001");
      Assert.Contains(errors, error => error.Id == "AFW-CFG-0002");
   }

   [Fact]
   public void FindByCategory_ShouldReturnMatchingDefinitions_FromAdditionalCategories()
   {
      ErrorCatalog catalog = CreateTestCatalog();

      IReadOnlyList<ErrorDefinition> errors = catalog.FindByCategory("Startup");

      Assert.Equal(2, errors.Count);
      Assert.Contains(errors, error => error.Id == "AFW-CFG-0001");
      Assert.Contains(errors, error => error.Id == "AFW-CFG-0002");
   }

   [Fact]
   public void FindByCategory_ShouldNotReturnDuplicate_WhenPrimaryCategoryIsAlsoInCategories()
   {
      ErrorCatalog catalog = CreateTestCatalog();

      IReadOnlyList<ErrorDefinition> errors = catalog.FindByCategory("Configuration");

      ErrorDefinition error = Assert.Single(errors, error => error.Id == "AFW-CFG-0001");

      Assert.Equal("MissingConfigurationValue", error.Name);
   }

   [Fact]
   public void FindByCategory_ShouldBeNormalized()
   {
      ErrorCatalog catalog = CreateTestCatalog();

      IReadOnlyList<ErrorDefinition> errors = catalog.FindByCategory("startup");

      Assert.Equal(2, errors.Count);
   }

   [Fact]
   public void FindBySubcategory_ShouldReturnMatchingDefinitions()
   {
      ErrorCatalog catalog = CreateTestCatalog();

      IReadOnlyList<ErrorDefinition> errors = catalog.FindBySubcategory("RequiredValue");

      Assert.Equal(2, errors.Count);
      Assert.Contains(errors, error => error.Id == "AFW-CFG-0001");
      Assert.Contains(errors, error => error.Id == "AFW-VAL-0001");
   }

   [Fact]
   public void FindBySubcategory_ShouldBeNormalized()
   {
      ErrorCatalog catalog = CreateTestCatalog();

      IReadOnlyList<ErrorDefinition> errors = catalog.FindBySubcategory("requiredvalue");

      Assert.Equal(2, errors.Count);
   }

   [Fact]
   public void FindByTag_ShouldReturnMatchingDefinitions()
   {
      ErrorCatalog catalog = CreateTestCatalog();

      IReadOnlyList<ErrorDefinition> errors = catalog.FindByTag("startup");

      Assert.Equal(2, errors.Count);
      Assert.Contains(errors, error => error.Id == "AFW-CFG-0001");
      Assert.Contains(errors, error => error.Id == "AFW-CFG-0002");
   }

   [Fact]
   public void FindByTag_ShouldBeNormalized()
   {
      ErrorCatalog catalog = CreateTestCatalog();

      IReadOnlyList<ErrorDefinition> errors = catalog.FindByTag("USER VISIBLE");

      Assert.Equal(2, errors.Count);
      Assert.Contains(errors, error => error.Id == "AFW-CFG-0001");
      Assert.Contains(errors, error => error.Id == "AFW-VAL-0001");
   }

   [Fact]
   public void Constructor_ShouldThrowArgumentNullException_WhenErrorsIsNull()
   {
      Assert.Throws<ArgumentNullException>(() => new ErrorCatalog(null!));
   }

   private static ErrorCatalog CreateTestCatalog()
   {
      ErrorDefinition[] errors =
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
                Tags = ["configuration", "startup", "user-visible"]
            },
            new ErrorDefinition
            {
                Id = "AFW-CFG-0002",
                Code = 200002,
                Name = "InvalidConfigurationValue",
                Owner = "Afrowave",
                CodePrefix = "CFG",
                CodeGroup = "Configuration",
                PrimaryCategory = "Configuration",
                Categories = ["Startup", "Validation"],
                Subcategories = ["InvalidValue", "AppSettings"],
                Title = "Invalid configuration value",
                Message = "A configuration value is invalid.",
                DefaultSeverity = "Error",
                Tags = ["configuration", "startup"]
            },
            new ErrorDefinition
            {
                Id = "AFW-VAL-0001",
                Code = 300001,
                Name = "RequiredValueMissing",
                Owner = "Afrowave",
                CodePrefix = "VAL",
                CodeGroup = "Validation",
                PrimaryCategory = "Validation",
                Categories = ["Validation", "Input"],
                Subcategories = ["RequiredValue"],
                Title = "Required value missing",
                Message = "A required value was not provided.",
                DefaultSeverity = "Warning",
                Tags = ["validation", "user-visible"]
            }
      ];

      return new ErrorCatalog(errors);
   }
}
