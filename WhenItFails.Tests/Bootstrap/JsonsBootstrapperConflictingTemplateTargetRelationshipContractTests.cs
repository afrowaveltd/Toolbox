using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Bootstrap;

public sealed class JsonsBootstrapperConflictingTemplateTargetRelationshipContractTests
{
    [Fact]
    public async Task EnsureWorkspaceAsync_WhenTemplateTargetIsParentFileOfAnotherTarget_ReturnsInvalidWithoutPartialWrites()
    {
        string rootDirectory = Path.Combine(
            Path.GetTempPath(),
            "Afrowave",
            "WhenItFails.Tests",
            Guid.NewGuid().ToString("N"));

        string packageDirectoryPath = Path.Combine(
            rootDirectory,
            "WhenItFails");

        string parentTargetPath = Path.Combine(
            packageDirectoryPath,
            "Nested");

        string childTargetPath = Path.Combine(
            parentTargetPath,
            "item.json");

        try
        {
            JsonsBootstrapper bootstrapper = new(
                new ConflictingTargetRelationshipTemplateProvider());

            Response<JsonsBootstrapPayload> response =
                await bootstrapper.EnsureWorkspaceAsync(
                    new JsonsOptions
                    {
                        RootDirectory = rootDirectory,
                        PackageDirectoryName = "WhenItFails"
                    });

            Assert.Equal(ResultStatus.Invalid, response.Status);
            Assert.False(response.IsSuccess);
            Assert.Null(response.Data);

            var issue = Assert.Single(response.Issues);
            Assert.Equal(
                "WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_CONFLICT",
                issue.Code);
            Assert.Equal(
                "The JSON template provider returned conflicting target file paths.",
                response.Message);
            Assert.Equal(
                "The JSON template provider returned conflicting target file paths.",
                issue.Message);

            Assert.True(Directory.Exists(packageDirectoryPath));
            Assert.False(File.Exists(parentTargetPath));
            Assert.False(Directory.Exists(parentTargetPath));
            Assert.False(File.Exists(childTargetPath));
        }
        finally
        {
            if (Directory.Exists(rootDirectory))
            {
                Directory.Delete(rootDirectory, recursive: true);
            }
        }
    }

    private sealed class ConflictingTargetRelationshipTemplateProvider
        : IJsonsTemplateProvider
    {
        public IReadOnlyList<JsonsTemplateFile> GetTemplateFiles(
            JsonsOptions options)
        {
            return
            [
                new JsonsTemplateFile
                {
                    Name = "Parent file",
                    TargetFileName = "Nested",
                    Content = "{}"
                },
                new JsonsTemplateFile
                {
                    Name = "Child file",
                    TargetFileName = Path.Combine(
                        "Nested",
                        "item.json"),
                    Content = "{}"
                }
            ];
        }
    }
}
