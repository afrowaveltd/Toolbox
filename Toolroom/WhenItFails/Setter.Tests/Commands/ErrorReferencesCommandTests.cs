using System.Text.Json;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

[Collection(ConsoleOutputTestCollection.Name)]
public sealed class ErrorReferencesCommandTests
{
    [Fact]
    public async Task ExecuteAsync_WithPlainOutput_ReturnsStableReferenceRowsWithoutChangingWorkspace()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorDefinition error = await LoadFirstErrorAsync(workspace.WhenItFailsJsonsPath);
        ErrorProfileDefinition profile = await LoadFirstProfileAsync(workspace.WhenItFailsJsonsPath);
        Response<ErrorProfileDefinition> addResponse =
            await new WhenItFailsProfileWorkspaceEditor().ProfileAddErrorAsync(
                workspace.ProjectRootPath,
                profile.Name,
                error.Id);
        Assert.True(addResponse.IsSuccess);
        Dictionary<string, string> before = await ReadCatalogFilesAsync(workspace.WhenItFailsJsonsPath);
        int backupsBefore = CountBackups(workspace.WhenItFailsJsonsPath);
        TextWriter originalOut = Console.Out;
        using StringWriter writer = new();

        try
        {
            Console.SetOut(writer);
            int exitCode = await ErrorReferencesCommand.ExecuteAsync(
            [
                "error-references",
                workspace.ProjectRootPath,
                error.Code.ToString(),
                "--plain"
            ]);

            Assert.Equal(0, exitCode);
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        string output = writer.ToString();
        Assert.Contains(error.Id, output, StringComparison.Ordinal);
        Assert.Contains(error.Name, output, StringComparison.Ordinal);
        Assert.Contains("INCLUDE", output, StringComparison.Ordinal);
        Assert.Contains(profile.Name, output, StringComparison.Ordinal);
        Assert.Equal(backupsBefore, CountBackups(workspace.WhenItFailsJsonsPath));
        Assert.Equal(before, await ReadCatalogFilesAsync(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ExecuteAsync_WithJsonOutput_WritesStableEnvelopeWithoutChangingWorkspace()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorDefinition error = await LoadFirstErrorAsync(workspace.WhenItFailsJsonsPath);
        Dictionary<string, string> before = await ReadCatalogFilesAsync(workspace.WhenItFailsJsonsPath);
        int backupsBefore = CountBackups(workspace.WhenItFailsJsonsPath);
        TextWriter originalOut = Console.Out;
        using StringWriter writer = new();

        try
        {
            Console.SetOut(writer);
            int exitCode = await ErrorReferencesCommand.ExecuteAsync(
            [
                "error-references",
                workspace.ProjectRootPath,
                error.Name,
                "--json"
            ]);

            Assert.Equal(0, exitCode);
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        using JsonDocument document = JsonDocument.Parse(writer.ToString());
        JsonElement root = document.RootElement;
        Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("error-references", root.GetProperty("command").GetString());
        JsonElement data = root.GetProperty("data");
        Assert.Equal(error.Id, data.GetProperty("errorId").GetString());
        Assert.Equal(error.Code, data.GetProperty("errorCode").GetInt32());
        Assert.Equal(error.Name, data.GetProperty("errorName").GetString());
        Assert.Equal(0, data.GetProperty("includedByProfiles").GetInt32());
        Assert.Equal(0, data.GetProperty("excludedByProfiles").GetInt32());
        Assert.Equal(0, data.GetProperty("references").GetArrayLength());
        Assert.Equal(backupsBefore, CountBackups(workspace.WhenItFailsJsonsPath));
        Assert.Equal(before, await ReadCatalogFilesAsync(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownError_ReturnsDomainFailure()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        int exitCode = await ErrorReferencesCommand.ExecuteAsync(
        [
            "error-references",
            workspace.ProjectRootPath,
            "DOES_NOT_EXIST"
        ]);

        Assert.Equal(2, exitCode);
    }

    public static TheoryData<string[]> InvalidArgumentCases =>
        new()
        {
            new[] { "error-references" },
            new[] { "error-references", "." },
            new[] { "error-references", ".", "AFW_GEN_0001", "--unknown" },
            new[] { "error-references", ".", "AFW_GEN_0001", "--plain", "--plain" },
            new[] { "error-references", ".", "AFW_GEN_0001", "--json", "--json" },
            new[] { "error-references", ".", "AFW_GEN_0001", "--plain", "--json" }
        };

    [Theory]
    [MemberData(nameof(InvalidArgumentCases))]
    public async Task ExecuteAsync_WithInvalidArguments_ReturnsCommandInputError(string[] args)
    {
        Assert.Equal(1, await ErrorReferencesCommand.ExecuteAsync(args));
    }

    private static async Task<ErrorDefinition> LoadFirstErrorAsync(string jsonsPath)
    {
        Response<ErrorCatalogDocument> response =
            await new JsonErrorCatalogLoader().LoadFromFileAsync(
                Path.Combine(jsonsPath, "errors.en.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        ErrorDefinition? error = new ErrorCatalogDocumentNormalizer()
            .Normalize(response.Data)
            .Errors
            .FirstOrDefault();
        Assert.NotNull(error);
        return error;
    }

    private static async Task<ErrorProfileDefinition> LoadFirstProfileAsync(string jsonsPath)
    {
        Response<ErrorProfileCatalogDocument> response =
            await new JsonErrorProfileCatalogLoader().LoadFromFileAsync(
                Path.Combine(jsonsPath, "profiles.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        ErrorProfileDefinition? profile = new ErrorProfileCatalogDocumentNormalizer()
            .Normalize(response.Data)
            .Profiles
            .FirstOrDefault();
        Assert.NotNull(profile);
        return profile;
    }

    private static async Task<Dictionary<string, string>> ReadCatalogFilesAsync(string jsonsPath)
    {
        string[] names =
        [
            "errors.en.json",
            "categories.en.json",
            "code-groups.en.json",
            "owners.en.json",
            "profiles.json"
        ];

        Dictionary<string, string> contents = new(StringComparer.OrdinalIgnoreCase);
        foreach (string name in names)
        {
            contents[name] = await File.ReadAllTextAsync(Path.Combine(jsonsPath, name));
        }

        return contents;
    }

    private static int CountBackups(string jsonsPath) => Directory.GetFiles(
        jsonsPath,
        "*.bak.json",
        SearchOption.TopDirectoryOnly).Length;
}
