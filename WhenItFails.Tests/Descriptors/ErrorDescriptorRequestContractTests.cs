using Afrowave.Toolbox.WhenItFails.Descriptors;

namespace Afrowave.Toolbox.WhenItFails.Tests.Descriptors;

public sealed class ErrorDescriptorRequestContractTests
{
    [Fact]
    public void NewRequest_UsesSafeNullDefaults()
    {
        ErrorDescriptorRequest request = new();

        Assert.Null(request.ErrorId);
        Assert.Null(request.ErrorName);
        Assert.Null(request.Code);
        Assert.Null(request.Title);
        Assert.Null(request.Message);
        Assert.Null(request.Severity);
        Assert.Null(request.DeveloperHint);
        Assert.Null(request.DocumentationKey);
    }

    [Fact]
    public void Properties_PreserveAssignedValues()
    {
        ErrorDescriptorRequest request = new()
        {
            ErrorId = "AFW_NET_0001",
            ErrorName = "NETWORKUNAVAILABLE",
            Code = 600001,
            Title = "Network unavailable",
            Message = "The network is unavailable.",
            Severity = "Error",
            DeveloperHint = "Check connectivity.",
            DocumentationKey = "when-it-fails/errors/network/network-unavailable"
        };

        Assert.Equal("AFW_NET_0001", request.ErrorId);
        Assert.Equal("NETWORKUNAVAILABLE", request.ErrorName);
        Assert.Equal(600001, request.Code);
        Assert.Equal("Network unavailable", request.Title);
        Assert.Equal("The network is unavailable.", request.Message);
        Assert.Equal("Error", request.Severity);
        Assert.Equal("Check connectivity.", request.DeveloperHint);
        Assert.Equal(
            "when-it-fails/errors/network/network-unavailable",
            request.DocumentationKey);
    }
}
