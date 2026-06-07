using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Descriptors;

namespace Afrowave.Toolbox.WhenItFails.Tests.Descriptors;

public sealed class ErrorDescriptorFactoryTests
{
    [Fact]
    public void Create_ShouldThrowArgumentNullException_WhenDefinitionIsNull()
    {
        ErrorDescriptorFactory factory = new();

        Assert.Throws<ArgumentNullException>(() => factory.Create(null!));
    }

    [Fact]
    public void Create_ShouldCopyScalarFieldsFromDefinition()
    {
        ErrorDescriptorFactory factory = new();

        ErrorDefinition definition = CreateDefinition();

        ErrorDescriptor descriptor = factory.Create(definition);

        Assert.Equal(definition.Id, descriptor.Id);
        Assert.Equal(definition.Code, descriptor.Code);
        Assert.Equal(definition.Name, descriptor.Name);

        Assert.Equal(definition.Owner, descriptor.Owner);
        Assert.Equal(definition.CodePrefix, descriptor.CodePrefix);
        Assert.Equal(definition.CodeGroup, descriptor.CodeGroup);

        Assert.Equal(definition.PrimaryCategory, descriptor.PrimaryCategory);

        Assert.Equal(definition.Title, descriptor.Title);
        Assert.Equal(definition.Message, descriptor.Message);
        Assert.Equal(definition.DefaultSeverity, descriptor.Severity);

        Assert.Equal(definition.DeveloperHint, descriptor.DeveloperHint);
        Assert.Equal(definition.DocumentationKey, descriptor.DocumentationKey);
    }

    [Fact]
    public void Create_ShouldCopyListFieldsIntoNewLists()
    {
        ErrorDescriptorFactory factory = new();

        ErrorDefinition definition = CreateDefinition();

        ErrorDescriptor descriptor = factory.Create(definition);

        Assert.Equal(definition.Categories, descriptor.Categories);
        Assert.Equal(definition.Subcategories, descriptor.Subcategories);
        Assert.Equal(definition.Tags, descriptor.Tags);

        Assert.NotSame(definition.Categories, descriptor.Categories);
        Assert.NotSame(definition.Subcategories, descriptor.Subcategories);
        Assert.NotSame(definition.Tags, descriptor.Tags);
    }

    [Fact]
    public void Create_ShouldKeepMetadataReference()
    {
        ErrorDescriptorFactory factory = new();

        ErrorDefinition definition = CreateDefinition();

        ErrorDescriptor descriptor = factory.Create(definition);

        Assert.Same(definition.Metadata, descriptor.Metadata);
    }

    [Fact]
    public void Create_ShouldNotChangeDescriptorLists_WhenDefinitionListsChangeLater()
    {
        ErrorDescriptorFactory factory = new();

        ErrorDefinition definition = CreateDefinition();

        ErrorDescriptor descriptor = factory.Create(definition);

        definition.Categories.Add("DATABASE");
        definition.Subcategories.Add("CONNECTION");
        definition.Tags.Add("LATE_CHANGE");

        Assert.DoesNotContain("DATABASE", descriptor.Categories);
        Assert.DoesNotContain("CONNECTION", descriptor.Subcategories);
        Assert.DoesNotContain("LATE_CHANGE", descriptor.Tags);
    }

    private static ErrorDefinition CreateDefinition()
    {
        return new ErrorDefinition
        {
            Id = "AFW_CFG_0001",
            Code = 200001,
            Name = "MISSING_CONFIGURATION_VALUE",

            Owner = "AFW",
            CodePrefix = "CFG",
            CodeGroup = "CONFIGURATION",

            PrimaryCategory = "CONFIGURATION",
            Categories = ["CONFIGURATION", "STARTUP", "VALIDATION"],
            Subcategories = ["REQUIRED_VALUE", "APP_SETTINGS"],

            Title = "Missing configuration value",
            Message = "A required configuration value is missing.",
            DefaultSeverity = "Error",

            DeveloperHint = "Check application configuration.",
            DocumentationKey = "errors.configuration.missing-value",

            Tags = ["CONFIGURATION", "STARTUP", "USER_VISIBLE"]
        };
    }
}