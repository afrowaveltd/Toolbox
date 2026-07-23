using System.Text.Json;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

[Collection(ConsoleOutputTestCollection.Name)]
public sealed class CheckDocKeysCommandTests
{
    [Fact]
    public async Task ExecuteAsync_WithInitializedWorkspace_ReturnsSuccess()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        int exitCode = await CheckDocKeysCommand.ExecuteAsync(
        [
            "check-doc-keys",
            workspace.ProjectRootPath
        ]);

        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithPlainOutput_ReturnsSuccess()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        int exitCode = await CheckDocKeysCommand.ExecuteAsync(
        [
            "check-doc-keys",
            workspace.ProjectRootPath,
            "--plain"
        ]);

        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithJsonOutput_WritesStableEnvelopeAndCompleteReport()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        int backupsBefore = CountBackups(workspace.WhenItFailsJsonsPath);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "check-doc-keys",
            workspace.ProjectRootPath,
            "--json"
        ]);

        Assert.Equal(0, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement root = document.RootElement;
        JsonElement data = root.GetProperty("data");

        Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("check-doc-keys", root.GetProperty("command").GetString());
        Assert.True(data.GetProperty("isValid").GetBoolean());
        Assert.True(data.GetProperty("totalErrors").GetInt32() > 0);
        Assert.Equal(JsonValueKind.Object, data.GetProperty("keys").ValueKind);
        Assert.Equal(JsonValueKind.Object, data.GetProperty("format").ValueKind);
        Assert.Equal(backupsBefore, CountBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ExecuteAsync_WithNonCanonicalKeyAndJson_WritesStableFailureReport()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        int backupsBefore = CountBackups(workspace.WhenItFailsJsonsPath);
        string catalogPath = Path.Combine(
            workspace.WhenItFailsJsonsPath,
            "errors.en.json");
        string json = await File.ReadAllTextAsync(catalogPath);
        json = json.Replace(
            "when-it-fails/errors/general/unknown-error",
            "When-It-Fails/errors/general/unknown-error",
            StringComparison.Ordinal);
        await File.WriteAllTextAsync(catalogPath, json);

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "check-doc-keys",
            workspace.ProjectRootPath,
            "--json"
        ]);

        Assert.Equal(2, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement root = document.RootElement;
        JsonElement data = root.GetProperty("data");
        JsonElement format = data.GetProperty("format");

        Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("check-doc-keys", root.GetProperty("command").GetString());
        Assert.False(data.GetProperty("isValid").GetBoolean());
        Assert.True(data.GetProperty("totalErrors").GetInt32() > 0);
        Assert.Equal(JsonValueKind.Array, format.GetProperty("invalidKeys").ValueKind);
        Assert.NotEmpty(format.GetProperty("invalidKeys").EnumerateArray());
        Assert.Equal(backupsBefore, CountBackups(workspace.WhenItFailsJsonsPath));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("--plain")]
    [InlineData("--json")]
    public async Task ExecuteAsync_WithNonCanonicalKey_ReturnsCheckFailure(string? outputOption)
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        string catalogPath = Path.Combine(
            workspace.WhenItFailsJsonsPath,
            "errors.en.json");
        string json = await File.ReadAllTextAsync(catalogPath);
        json = json.Replace(
            "when-it-fails/errors/general/unknown-error",
            "When-It-Fails/errors/general/unknown-error",
            StringComparison.Ordinal);
        await File.WriteAllTextAsync(catalogPath, json);

        List<string> args =
        [
            "check-doc-keys",
            workspace.ProjectRootPath
        ];
        if (outputOption is not null)
        {
            args.Add(outputOption);
        }

        int exitCode = await CheckDocKeysCommand.ExecuteAsync(args.ToArray());

        Assert.Equal(2, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingDirectory_ReturnsCheckFailure()
    {
        string missingPath = Path.Combine(
            Path.GetTempPath(),
            "WhenItFailsSetterTests",
            Guid.NewGuid().ToString("N"));

        int exitCode = await CheckDocKeysCommand.ExecuteAsync(
        [
            "check-doc-keys",
            missingPath
        ]);

        Assert.Equal(2, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingDirectoryAndJson_WritesStableFailureEnvelope()
    {
        string missingPath = Path.Combine(
            Path.GetTempPath(),
            "WhenItFailsSetterTests",
            Guid.NewGuid().ToString("N"));

        (int exitCode, string output) = await ExecuteWithCapturedOutputAsync(
        [
            "check-doc-keys",
            missingPath,
            "--json"
        ]);

        Assert.Equal(2, exitCode);
        using JsonDocument document = JsonDocument.Parse(output);
        JsonElement root = document.RootElement;
        JsonElement data = root.GetProperty("data");

        Assert.Equal("1.0", root.GetProperty("schemaVersion").GetString());
        Assert.Equal("check-doc-keys", root.GetProperty("command").GetString());
        Assert.False(data.GetProperty("checked").GetBoolean());
        Assert.Equal(JsonValueKind.Object, data.GetProperty("validation").ValueKind);
    }

    public static TheoryData<string[]> InvalidArgumentCases =>
        new()
        {
            new[] { "check-doc-keys" },
            new[] { "check-doc-keys", ".", "--unknown" },
            new[] { "check-doc-keys", ".", "--plain", "--json" },
            new[] { "check-doc-keys", ".", "--plain", "--plain" },
            new[] { "check-doc-keys", ".", "--json", "--json" }
        };

    [Theory]
    [MemberData(nameof(InvalidArgumentCases))]
    public async Task ExecuteAsync_WithInvalidArguments_ReturnsCommandInputError(string[] args)
    {
        Assert.Equal(1, await CheckDocKeysCommand.ExecuteAsync(args));
    }

    private static async Task<(int ExitCode, string Output)> ExecuteWithCapturedOutputAsync(string[] args)
    {
        TextWriter originalOutput = Console.Out;
        using StringWriter output = new();

        try
        {
            Console.SetOut(output);
            int exitCode = await CheckDocKeysCommand.ExecuteAsync(args);
            return (exitCode, output.ToString());
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }

    private static int CountBackups(string jsonsPath)
    {
        return Directory.Exists(jsonsPath)
            ? Directory.GetFiles(
                jsonsPath,
                "*.bak.json",
                SearchOption.TopDirectoryOnly).Length
            : 0;
    }
}
