using Afrowave.Toolbox.WhenItFails.Descriptors;

namespace Afrowave.Toolbox.WhenItFails.Tests.Descriptors;

public sealed class ErrorDescriptorOfTContractTests
{
    [Fact]
    public void NewDescriptor_ShouldUseNullAttachmentDefault()
    {
        ErrorDescriptor<object> descriptor = new();

        Assert.Null(descriptor.Attachment);
    }

    [Fact]
    public void AssignedReferenceAttachment_ShouldBePreservedExactly()
    {
        object attachment = new();
        ErrorDescriptor<object> descriptor = new()
        {
            Attachment = attachment
        };

        Assert.Same(attachment, descriptor.Attachment);
    }

    [Fact]
    public void AssignedValueAttachment_ShouldBePreservedExactly()
    {
        ErrorDescriptor<int> descriptor = new()
        {
            Attachment = 42
        };

        Assert.Equal(42, descriptor.Attachment);
    }
}
