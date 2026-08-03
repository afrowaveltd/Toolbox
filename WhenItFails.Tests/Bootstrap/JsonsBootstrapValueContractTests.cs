using Afrowave.Toolbox.WhenItFails.Bootstrap;

namespace Afrowave.Toolbox.WhenItFails.Tests.Bootstrap;

public sealed class JsonsBootstrapValueContractTests
{
    [Fact]
    public void TemplateFile_ShouldPreserveAssignedValues()
    {
        JsonsTemplateFile template = new()
        {
            Name = "Error catalog",
            TargetFileName = "errors.en.json",
            Content = "{\"schemaVersion\":\"1.0\"}"
        };

        Assert.Equal("Error catalog", template.Name);
        Assert.Equal("errors.en.json", template.TargetFileName);
        Assert.Equal("{\"schemaVersion\":\"1.0\"}", template.Content);
    }

    [Fact]
    public void FileResult_ShouldPreserveAssignedValues()
    {
        JsonsBootstrapFileResult result = new()
        {
            Name = "Error catalog",
            TargetFilePath = "Jsons/WhenItFails/errors.en.json",
            AlreadyExisted = true,
            Created = true,
            Skipped = true,
            Message = "Preserved result"
        };

        Assert.Equal("Error catalog", result.Name);
        Assert.Equal("Jsons/WhenItFails/errors.en.json", result.TargetFilePath);
        Assert.True(result.AlreadyExisted);
        Assert.True(result.Created);
        Assert.True(result.Skipped);
        Assert.Equal("Preserved result", result.Message);
    }
}
