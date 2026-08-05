using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Descriptors;

namespace Afrowave.Toolbox.WhenItFails.Tests.Descriptors;

public sealed class ErrorDescriptorFactoryNullBoundaryContractTests
{
    [Fact]
    public void Create_ShouldUseEmptyLists_WhenDefinitionCollectionsAreNullAtRuntime()
    {
        ErrorDefinition definition = new()
        {
            Categories = null!,
            Subcategories = null!,
            Tags = null!
        };

        ErrorDescriptor descriptor = new ErrorDescriptorFactory().Create(definition);

        Assert.NotNull(descriptor.Categories);
        Assert.Empty(descriptor.Categories);
        Assert.NotNull(descriptor.Subcategories);
        Assert.Empty(descriptor.Subcategories);
        Assert.NotNull(descriptor.Tags);
        Assert.Empty(descriptor.Tags);
    }

    [Fact]
    public void Create_ShouldUseNewMetadataBag_WhenDefinitionMetadataIsNullAtRuntime()
    {
        ErrorDefinition definition = new()
        {
            Metadata = null!
        };

        ErrorDescriptor descriptor = new ErrorDescriptorFactory().Create(definition);

        Assert.NotNull(descriptor.Metadata);
    }
}
