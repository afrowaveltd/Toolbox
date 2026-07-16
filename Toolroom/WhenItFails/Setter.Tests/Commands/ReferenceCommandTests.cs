using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

public sealed class ReferenceCommandTests
{
    [Theory]
    [InlineData("reference")]
    [InlineData("reference", "summary")]
    [InlineData("reference", "profiles")]
    [InlineData("reference", "categories")]
    [InlineData("reference", "code-groups")]
    [InlineData("reference", "errors")]
    [InlineData("reference", "errors", "--all")]
    [InlineData("reference", "errors", "--group", "CONFIGURATION")]
    [InlineData("reference", "errors", "--category", "NETWORK")]
    [InlineData("reference", "errors", "--all", "--group", "NETWORK")]
    [InlineData("reference", "errors", "--category", "VALIDATION", "--all")]
    [InlineData("reference", "profile", "WEB_API")]
    [InlineData("reference", "profile", "web_api")]
    [InlineData("reference", "error", "AFW-CFG-0001")]
    [InlineData("reference", "error", "MissingConfigurationValue")]
    public async Task ExecuteAsync_WithSupportedReferenceCommand_ReturnsSuccess(
        params string[] args)
    {
        int exitCode = await ReferenceCommand.ExecuteAsync(args);

        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownSubcommand_ReturnsCommandInputError()
    {
        int exitCode = await ReferenceCommand.ExecuteAsync(
            ["reference", "unknown"]);

        Assert.Equal(1, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithTooManyArguments_ReturnsCommandInputError()
    {
        int exitCode = await ReferenceCommand.ExecuteAsync(
            ["reference", "summary", "extra"]);

        Assert.Equal(1, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnsupportedErrorsArgument_ReturnsCommandInputError()
    {
        int exitCode = await ReferenceCommand.ExecuteAsync(
            ["reference", "errors", "--unknown"]);

        Assert.Equal(1, exitCode);
    }

    [Theory]
    [InlineData("reference", "errors", "--group")]
    [InlineData("reference", "errors", "--category")]
    [InlineData("reference", "errors", "--all", "--all")]
    [InlineData("reference", "errors", "--group", "NETWORK", "--group", "HTTP")]
    [InlineData("reference", "errors", "--category", "NETWORK", "--category", "HTTP")]
    public async Task ExecuteAsync_WithInvalidErrorsFilter_ReturnsCommandInputError(
        params string[] args)
    {
        int exitCode = await ReferenceCommand.ExecuteAsync(args);

        Assert.Equal(1, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithErrorSubcommandWithoutIdentifier_ReturnsCommandInputError()
    {
        int exitCode = await ReferenceCommand.ExecuteAsync(
            ["reference", "error"]);

        Assert.Equal(1, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithProfileSubcommandWithoutName_ReturnsCommandInputError()
    {
        int exitCode = await ReferenceCommand.ExecuteAsync(
            ["reference", "profile"]);

        Assert.Equal(1, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownReferenceError_ReturnsCommandInputError()
    {
        int exitCode = await ReferenceCommand.ExecuteAsync(
            ["reference", "error", "AFW-NOPE-0001"]);

        Assert.Equal(1, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownReferenceProfile_ReturnsCommandInputError()
    {
        int exitCode = await ReferenceCommand.ExecuteAsync(
            ["reference", "profile", "NOPE"]);

        Assert.Equal(1, exitCode);
    }

    [Fact]
    public async Task SummarizeAsync_WithBundledReferenceCatalog_ReturnsExpectedCounts()
    {
        WhenItFailsReferenceCatalogSummarizer summarizer = new();

        WhenItFailsReferenceCatalogSummary summary =
            await summarizer.SummarizeAsync(Environment.CurrentDirectory);

        Assert.Equal(1, summary.OwnerCount);
        Assert.Equal(16, summary.CategoryCount);
        Assert.Equal(9, summary.CodeGroupCount);
        Assert.Equal(5, summary.ProfileCount);
        Assert.Equal(37, summary.ErrorCount);

        Assert.Contains("MINIMAL", summary.ProfileNames);
        Assert.Contains("CONSOLE_TOOL", summary.ProfileNames);
        Assert.Contains("WEB_API", summary.ProfileNames);
        Assert.Contains("DESKTOP_APP", summary.ProfileNames);
        Assert.Contains("FULL", summary.ProfileNames);

        Assert.Equal(5, summary.Profiles.Count);
        Assert.Contains(
            summary.Profiles,
            profile => profile.Name == "WEB_API"
                       && profile.IncludedCodeGroupNames.Contains("NETWORK")
                       && profile.IncludedCategoryNames.Contains("HTTP"));

        Assert.Equal(16, summary.Categories.Count);
        Assert.Contains(summary.Categories, category => category.Name == "GENERAL");
        Assert.Contains(summary.Categories, category => category.Name == "HTTP");
        Assert.Contains(summary.Categories, category => category.Name == "AUTHENTICATION");
        Assert.Contains(
            summary.Categories,
            category => category.Name == "HTTP"
                        && category.ParentCategoryNames.Contains("NETWORK"));

        Assert.Equal(9, summary.CodeGroups.Count);
        Assert.Contains(summary.CodeGroups, codeGroup => codeGroup.Name == "GENERAL");
        Assert.Contains(summary.CodeGroups, codeGroup => codeGroup.Name == "FILE_SYSTEM");
        Assert.Contains(summary.CodeGroups, codeGroup => codeGroup.Name == "SERIALIZATION");
        Assert.Contains(
            summary.CodeGroups,
            codeGroup => codeGroup.Name == "CONFIGURATION"
                         && codeGroup.CodePrefix == "CFG"
                         && codeGroup.CodeFrom == 200000
                         && codeGroup.CodeTo == 299999);

        Assert.Equal(37, summary.Errors.Count);
        Assert.Contains(summary.Errors, error => error.Id == "AFW-GEN-0001");
        Assert.Contains(summary.Errors, error => error.Id == "AFW-CFG-0001");
        Assert.Contains(summary.Errors, error => error.Id == "AFW-FMT-0003");
        Assert.Contains(
            summary.Errors,
            error => error.Id == "AFW-CFG-0001"
                     && error.Code == 200001
                     && error.Name == "MissingConfigurationValue"
                     && error.CodeGroup == "CONFIGURATION"
                     && error.PrimaryCategory == "CONFIGURATION"
                     && error.DefaultSeverity == "Error"
                     && error.CategoryNames.Contains("CONFIGURATION")
                     && error.TagNames.Contains("CONFIGURATION"));
    }
}
