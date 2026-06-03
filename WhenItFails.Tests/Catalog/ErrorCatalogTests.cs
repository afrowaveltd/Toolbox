using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;

namespace WhenItFails.Tests.Catalog;

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

        ErrorDefinition? error = catalog.FindById("CFG-0001");

        Assert.NotNull(error);
        Assert.Equal("MissingConfigurationValue", error.Name);
    }

    [Fact]
    public void FindById_ShouldBeCaseInsensitive()
    {
        ErrorCatalog catalog = CreateTestCatalog();

        ErrorDefinition? error = catalog.FindById("cfg-0001");

        Assert.NotNull(error);
        Assert.Equal("CFG-0001", error.Id);
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

        ErrorDefinition? error = catalog.FindByCode(1001);

        Assert.NotNull(error);
        Assert.Equal("CFG-0001", error.Id);
    }

    [Fact]
    public void FindByCode_ShouldReturnNull_WhenCodeIsUnknown()
    {
        ErrorCatalog catalog = CreateTestCatalog();

        ErrorDefinition? error = catalog.FindByCode(9999);

        Assert.Null(error);
    }

    [Fact]
    public void FindByName_ShouldReturnMatchingDefinition()
    {
        ErrorCatalog catalog = CreateTestCatalog();

        ErrorDefinition? error = catalog.FindByName("MissingConfigurationValue");

        Assert.NotNull(error);
        Assert.Equal("CFG-0001", error.Id);
    }

    [Fact]
    public void FindByName_ShouldBeCaseInsensitive()
    {
        ErrorCatalog catalog = CreateTestCatalog();

        ErrorDefinition? error = catalog.FindByName("missingconfigurationvalue");

        Assert.NotNull(error);
        Assert.Equal("MissingConfigurationValue", error.Name);
    }

    [Fact]
    public void FindByCategory_ShouldReturnMatchingDefinitions()
    {
        ErrorCatalog catalog = CreateTestCatalog();

        IReadOnlyList<ErrorDefinition> errors = catalog.FindByCategory("Configuration");

        Assert.Equal(2, errors.Count);
        Assert.Contains(errors, error => error.Id == "CFG-0001");
        Assert.Contains(errors, error => error.Id == "CFG-0002");
    }

    [Fact]
    public void FindByCategoryPrefix_ShouldReturnMatchingDefinitions()
    {
        ErrorCatalog catalog = CreateTestCatalog();

        IReadOnlyList<ErrorDefinition> errors = catalog.FindByCategoryPrefix("CFG");

        Assert.Equal(2, errors.Count);
        Assert.Contains(errors, error => error.Id == "CFG-0001");
        Assert.Contains(errors, error => error.Id == "CFG-0002");
    }

    [Fact]
    public void FindByTag_ShouldReturnMatchingDefinitions()
    {
        ErrorCatalog catalog = CreateTestCatalog();

        IReadOnlyList<ErrorDefinition> errors = catalog.FindByTag("startup");

        Assert.Equal(2, errors.Count);
        Assert.Contains(errors, error => error.Id == "CFG-0001");
        Assert.Contains(errors, error => error.Id == "CFG-0002");
    }

    [Fact]
    public void FindByTag_ShouldBeCaseInsensitive()
    {
        ErrorCatalog catalog = CreateTestCatalog();

        IReadOnlyList<ErrorDefinition> errors = catalog.FindByTag("STARTUP");

        Assert.Equal(2, errors.Count);
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
                Id = "CFG-0001",
                Code = 1001,
                Name = "MissingConfigurationValue",
                Category = "Configuration",
                CategoryPrefix = "CFG",
                Title = "Missing configuration value",
                Message = "A required configuration value is missing.",
                DefaultSeverity = "Error",
                Tags = ["configuration", "startup", "user-visible"]
            },
            new ErrorDefinition
            {
                Id = "CFG-0002",
                Code = 1002,
                Name = "InvalidConfigurationValue",
                Category = "Configuration",
                CategoryPrefix = "CFG",
                Title = "Invalid configuration value",
                Message = "A configuration value is invalid.",
                DefaultSeverity = "Error",
                Tags = ["configuration", "startup"]
            },
            new ErrorDefinition
            {
                Id = "VAL-0001",
                Code = 2001,
                Name = "RequiredValueMissing",
                Category = "Validation",
                CategoryPrefix = "VAL",
                Title = "Required value missing",
                Message = "A required value was not provided.",
                DefaultSeverity = "Warning",
                Tags = ["validation", "user-visible"]
            }
        ];

        return new ErrorCatalog(errors);
    }
}