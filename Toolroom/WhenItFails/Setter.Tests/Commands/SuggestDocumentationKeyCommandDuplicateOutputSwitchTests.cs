using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

public sealed class SuggestDocumentationKeyCommandDuplicateOutputSwitchTests
{
    [Fact]
    public async Task ExecuteAsync_WithCaseVariantDuplicatePlainSwitch_ReturnsCommandInputError()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        int exitCode = await SuggestDocumentationKeyCommand.ExecuteAsync(
        [
            "suggest-doc-key",
            workspace.ProjectRootPath,
            "NETWORK",
            "Duplicate output switch sample",
            "--plain",
            "--PLAIN"
        ]);

        Assert.Equal(1, exitCode);
    }
}
