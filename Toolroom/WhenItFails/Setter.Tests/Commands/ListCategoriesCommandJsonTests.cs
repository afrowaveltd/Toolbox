using System.Text.Json;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

[Collection(ConsoleOutputTestCollection.Name)]
public sealed class ListCategoriesCommandJsonTests
{
    [Fact]
    public async Task ExecuteAsync_WithJsonOutput_WritesStableEnvelopeAndCompleteCategories()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        int backupsBefore = CountCategoryBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "list-categories",
            workspace.ProjectRootPath,
            "--json"
        ]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement root = document.RootElement;
        JsonElement data = root.GetProperty("data");

        Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("list-categories", root.GetProperty("command").GetString());
        Assert.True(data.GetProperty("loaded").GetBoolean());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("validation").ValueKind);

        JsonElement categories = data.GetProperty("categories");
        Assert.Equal(JsonValueKind.Array, categories.ValueKind);
        Assert.NotEmpty(categories.EnumerateArray());

        JsonElement firstCategory = categories.EnumerateArray().First();
        Assert.False(string.IsNullOrWhiteSpace(firstCategory.GetProperty("name").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(firstCategory.GetProperty("displayName").GetString()));
        Assert.True(firstCategory.TryGetProperty("aliases", out _));
        Assert.Equal(backupsBefore, CountCategoryBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidWorkspaceAndJson_WritesStructuredValidationFailure()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            TemporaryWhenItFailsWorkspace.CreateEmpty();
        int backupsBefore = CountCategoryBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "list-categories",
            workspace.ProjectRootPath,
            "--json"
        ]);

        Assert.Equal(2, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement root = document.RootElement;
        JsonElement data = root.GetProperty("data");

        Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("list-categories", root.GetProperty("command").GetString());
        Assert.False(data.GetProperty("loaded").GetBoolean());
        Assert.Equal(JsonValueKind.Null, data.GetProperty("categories").ValueKind);
        Assert.Equal(JsonValueKind.Object, data.GetProperty("validation").ValueKind);
        Assert.Equal(backupsBefore, CountCategoryBackups(workspace.WhenItFailsJsonsPath));
    }

    [Theory]
    [InlineData("--json", "--json")]
    [InlineData("--plain", "--json")]
    public async Task ExecuteAsync_WithConflictingOutputSwitches_ReturnsInputErrorWithoutBackup(
        string first,
        string second)
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        int backupsBefore = CountCategoryBackups(workspace.WhenItFailsJsonsPath);

        int exitCode = await ListCategoriesCommand.ExecuteAsync(
        [
            "list-categories",
            workspace.ProjectRootPath,
            first,
            second
        ]);

        Assert.Equal(1, exitCode);
        Assert.Equal(backupsBefore, CountCategoryBackups(workspace.WhenItFailsJsonsPath));
    }

    private static async Task<(int ExitCode, string Output)> ExecuteWithCapturedOutputAsync(string[] args)
    {
        TextWriter originalOutput = Console.Out;
        using StringWriter output = new();

        try
        {
            Console.SetOut(output);
            int exitCode = await ListCategoriesCommand.ExecuteAsync(args);
            return (exitCode, output.ToString());
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }

    private static int CountCategoryBackups(string jsonsPath)
    {
        return Directory.Exists(jsonsPath)
            ? Directory.GetFiles(
                jsonsPath,
                "categories.*.bak.json",
                SearchOption.TopDirectoryOnly).Length
            : 0;
    }
}
