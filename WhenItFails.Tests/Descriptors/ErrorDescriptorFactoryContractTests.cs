using Afrowave.Toolbox.Essentials.Metadata;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Descriptors;

namespace Afrowave.Toolbox.WhenItFails.Tests.Descriptors;

public sealed class ErrorDescriptorFactoryContractTests
{
    [Fact]
    public void Create_ShouldMapScalarValuesExactly()
    {
        ErrorDefinition definition = CreateDefinition();
        ErrorDescriptorFactory factory = new();

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
    public void Create_ShouldCopyCollectionValuesIntoIndependentLists()
    {
        ErrorDefinition definition = CreateDefinition();
        ErrorDescriptorFactory factory = new();

        ErrorDescriptor descriptor = factory.Create(definition);

        Assert.Equal(definition.Categories, descriptor.Categories);
        Assert.Equal(definition.Subcategories, descriptor.Subcategories);
        Assert.Equal(definition.Tags, descriptor.Tags);
        Assert.NotSame(definition.Categories, descriptor.Categories);
        Assert.NotSame(definition.Subcategories, descriptor.Subcategories);
        Assert.NotSame(definition.Tags, descriptor.Tags);

        definition.Categories.Add("SourceOnlyCategory");
        definition.Subcategories.Add("SourceOnlySubcategory");
        definition.Tags.Add("source-only-tag");

        Assert.DoesNotContain("SourceOnlyCategory", descriptor.Categories);
        Assert.DoesNotContain("SourceOnlySubcategory", descriptor.Subcategories);
        Assert.DoesNotContain("source-only-tag", descriptor.Tags);
    }

    [Fact]
    public void Create_ShouldPreserveMetadataInstance()
    {
        ErrorDefinition definition = CreateDefinition();
        ErrorDescriptorFactory factory = new();

        ErrorDescriptor descriptor = factory.Create(definition);

        Assert.Same(definition.Metadata, descriptor.Metadata);
    }

    private static ErrorDefinition CreateDefinition()
    {
        return new ErrorDefinition
        {
            Id = "AFW_TST_0001",
            Code = 900001,
            Name = "TEST_ERROR",
            Owner = "AFW",
            CodePrefix = "TST",
            CodeGroup = "TESTING",
            PrimaryCategory = "TESTING",
            Categories = ["TESTING", "RUNTIME"],
            Subcategories = ["FACTORY"],
            Title = "Test error",
            Message = "A test error occurred.",
            DefaultSeverity = "Warning",
            DeveloperHint = "Inspect the test input.",
            DocumentationKey = "test-error",
            Tags = ["test", "factory"],
            Metadata = new MetadataBag()
        };
    }
}
