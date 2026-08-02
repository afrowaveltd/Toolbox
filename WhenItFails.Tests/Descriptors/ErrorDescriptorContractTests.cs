using Afrowave.Toolbox.WhenItFails.Descriptors;

namespace Afrowave.Toolbox.WhenItFails.Tests.Descriptors;

public sealed class ErrorDescriptorContractTests
{
    [Fact]
    public void NewDescriptor_ShouldUseSafeDefaults()
    {
        ErrorDescriptor descriptor = new();

        Assert.Equal(string.Empty, descriptor.Id);
        Assert.Equal(0, descriptor.Code);
        Assert.Equal(string.Empty, descriptor.Name);
        Assert.Equal(string.Empty, descriptor.Owner);
        Assert.Equal(string.Empty, descriptor.CodePrefix);
        Assert.Equal(string.Empty, descriptor.CodeGroup);
        Assert.Equal(string.Empty, descriptor.PrimaryCategory);
        Assert.Empty(descriptor.Categories);
        Assert.Empty(descriptor.Subcategories);
        Assert.Equal(string.Empty, descriptor.Title);
        Assert.Equal(string.Empty, descriptor.Message);
        Assert.Equal("Error", descriptor.Severity);
        Assert.Null(descriptor.Detail);
        Assert.Null(descriptor.OperationName);
        Assert.Null(descriptor.ComponentName);
        Assert.Null(descriptor.SourceName);
        Assert.Null(descriptor.DeveloperHint);
        Assert.Null(descriptor.DocumentationKey);
        Assert.Empty(descriptor.Tags);
        Assert.NotNull(descriptor.Metadata);
        Assert.Null(descriptor.Exception);
    }

    [Fact]
    public void NewDescriptors_ShouldNotShareMutableContainers()
    {
        ErrorDescriptor first = new();
        ErrorDescriptor second = new();

        Assert.NotSame(first.Categories, second.Categories);
        Assert.NotSame(first.Subcategories, second.Subcategories);
        Assert.NotSame(first.Tags, second.Tags);
        Assert.NotSame(first.Metadata, second.Metadata);

        first.Categories.Add("Runtime");
        first.Subcategories.Add("Initialization");
        first.Tags.Add("first");

        Assert.Empty(second.Categories);
        Assert.Empty(second.Subcategories);
        Assert.Empty(second.Tags);
    }
}
