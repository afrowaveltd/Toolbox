using Afrowave.Toolbox.WhenItFails.Bootstrap;

namespace Afrowave.Toolbox.WhenItFails.Tests.Bootstrap;

public sealed class JsonsBootstrapPayloadContractTests
{
    [Fact]
    public void NewPayload_ShouldExposeSafeEmptyDefaults()
    {
        JsonsBootstrapPayload payload = new();

        Assert.Equal(string.Empty, payload.RootDirectory);
        Assert.Equal(string.Empty, payload.PackageDirectoryPath);
        Assert.False(payload.PackageDirectoryAlreadyExisted);
        Assert.False(payload.PackageDirectoryCreated);
        Assert.NotNull(payload.Files);
        Assert.Empty(payload.Files);
    }

    [Fact]
    public void Files_ShouldRemainTheSameMutableCollection()
    {
        JsonsBootstrapPayload payload = new();
        List<JsonsBootstrapFileResult> files = payload.Files;
        JsonsBootstrapFileResult file = new()
        {
            Name = "Error catalog",
            TargetFilePath = "Jsons/WhenItFails/errors.en.json",
            Created = true
        };

        files.Add(file);

        Assert.Same(files, payload.Files);
        Assert.Single(payload.Files);
        Assert.Same(file, payload.Files[0]);
    }

    [Fact]
    public void NewFileResult_ShouldExposeSafeEmptyDefaults()
    {
        JsonsBootstrapFileResult result = new();

        Assert.Equal(string.Empty, result.Name);
        Assert.Equal(string.Empty, result.TargetFilePath);
        Assert.False(result.AlreadyExisted);
        Assert.False(result.Created);
        Assert.False(result.Skipped);
        Assert.Null(result.Message);
    }

    [Fact]
    public void NewTemplateFile_ShouldExposeSafeEmptyDefaults()
    {
        JsonsTemplateFile template = new();

        Assert.Equal(string.Empty, template.Name);
        Assert.Equal(string.Empty, template.TargetFileName);
        Assert.Equal(string.Empty, template.Content);
    }
}
