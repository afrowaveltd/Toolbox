using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

public sealed class SetNameCommandTests
{
    [Fact]
    public async Task ExecuteAsync_WithExistingError_RenamesItAndCreatesOneBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorCatalogDocument before = await LoadErrorsAsync(workspace.WhenItFailsJsonsPath);
        ErrorDefinition target = before.Errors.First();
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);

        int exitCode = await SetNameCommand.ExecuteAsync(
        [
            "set-name",
            workspace.ProjectRootPath,
            target.Id,
            "Renamed command sample"
        ]);

        Assert.Equal(0, exitCode);
        ErrorCatalogDocument after = await LoadErrorsAsync(workspace.WhenItFailsJsonsPath);
        ErrorDefinition? renamed = after.Errors.FirstOrDefault(error => error.Id == target.Id);
        Assert.NotNull(renamed);
        Assert.Equal("RENAMED_COMMAND_SAMPLE", renamed.Name);
        Assert.Equal(target.Code, renamed.Code);
        Assert.Equal(backupsBefore + 1, CountErrorBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ExecuteAsync_WithDuplicateName_ReturnsDomainFailureWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorCatalogDocument catalog = await LoadErrorsAsync(workspace.WhenItFailsJsonsPath);
        ErrorDefinition target = catalog.Errors[0];
        ErrorDefinition existing = catalog.Errors[1];
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);

        int exitCode = await SetNameCommand.ExecuteAsync(
        [
            "set-name",
            workspace.ProjectRootPath,
            target.Id,
            existing.Name
        ]);

        Assert.Equal(2, exitCode);
        Assert.Equal(backupsBefore, CountErrorBackups(workspace.WhenItFailsJsonsPath));
        ErrorCatalogDocument after = await LoadErrorsAsync(workspace.WhenItFailsJsonsPath);
        Assert.Equal(target.Name, after.Errors.First(error => error.Id == target.Id).Name);
    }

    [Fact]
    public async Task ExecuteAsync_WithAlreadySetName_ReturnsDomainFailureWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorDefinition target = (await LoadErrorsAsync(workspace.WhenItFailsJsonsPath)).Errors.First();
        int backupsBefore = CountErrorBackups(workspace.WhenItFailsJsonsPath);

        int exitCode = await SetNameCommand.ExecuteAsync(
        [
            "set-name",
            workspace.ProjectRootPath,
            target.Id,
            target.Name.ToLowerInvariant().Replace('_', ' ')
        ]);

        Assert.Equal(2, exitCode);
        Assert.Equal(backupsBefore, CountErrorBackups(workspace.WhenItFailsJsonsPath));
    }

    public static TheoryData<string[]> InvalidArgumentCases =>
        new()
        {
            new[] { "set-name" },
            new[] { "set-name", "." },
            new[] { "set-name", ".", "AFW_GEN_0001" },
            new[] { "set-name", ".", "", "NEW_NAME" },
            new[] { "set-name", ".", "AFW_GEN_0001", "" }
        };

    [Theory]
    [MemberData(nameof(InvalidArgumentCases))]
    public async Task ExecuteAsync_WithInvalidArguments_ReturnsCommandInputError(string[] args)
    {
        Assert.Equal(1, await SetNameCommand.ExecuteAsync(args));
    }

    private static async Task<ErrorCatalogDocument> LoadErrorsAsync(string jsonsPath)
    {
        Response<ErrorCatalogDocument> response =
            await new JsonErrorCatalogLoader().LoadFromFileAsync(
                Path.Combine(jsonsPath, "errors.en.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        return new ErrorCatalogDocumentNormalizer().Normalize(response.Data);
    }

    private static int CountErrorBackups(string jsonsPath) => Directory.GetFiles(
        jsonsPath,
        "errors.en.*.bak.json",
        SearchOption.TopDirectoryOnly).Length;
}
