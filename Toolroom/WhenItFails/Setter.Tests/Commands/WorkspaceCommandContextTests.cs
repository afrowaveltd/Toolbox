using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

public sealed class WorkspaceCommandContextTests
{
    [Fact]
    public async Task TryLoadAsync_WithValidWorkspace_ReturnsLoadedSummary()
    {
        WhenItFailsWorkspaceSummary expectedSummary = new()
        {
            DisplayPath = "workspace"
        };
        bool validationFailureShown = false;

        WorkspaceCommandContext? context = await WorkspaceCommandContextLoader.TryLoadAsync(
            "input",
            _ => Task.FromResult(CreateValidationOutcome(isValid: true)),
            _ => Task.FromResult(expectedSummary),
            _ => validationFailureShown = true);

        Assert.NotNull(context);
        Assert.Same(expectedSummary, context.Summary);
        Assert.False(validationFailureShown);
    }

    [Fact]
    public async Task TryLoadAsync_WithInvalidWorkspace_ShowsFailureAndSkipsSummaryLoad()
    {
        bool summaryLoaded = false;
        WhenItFailsWorkspaceValidationOutcome expectedOutcome =
            CreateValidationOutcome(isValid: false);
        WhenItFailsWorkspaceValidationOutcome? shownOutcome = null;

        WorkspaceCommandContext? context = await WorkspaceCommandContextLoader.TryLoadAsync(
            "input",
            _ => Task.FromResult(expectedOutcome),
            _ =>
            {
                summaryLoaded = true;
                return Task.FromResult(new WhenItFailsWorkspaceSummary());
            },
            outcome => shownOutcome = outcome);

        Assert.Null(context);
        Assert.False(summaryLoaded);
        Assert.Same(expectedOutcome, shownOutcome);
    }

    private static WhenItFailsWorkspaceValidationOutcome CreateValidationOutcome(
        bool isValid)
    {
        ErrorCatalogValidationResult validationResult = new();

        if (!isValid)
        {
            validationResult.AddError(
                code: "InvalidWorkspace",
                message: "The workspace is invalid.");
        }

        return new WhenItFailsWorkspaceValidationOutcome
        {
            DisplayPath = "workspace",
            ValidationResult = validationResult
        };
    }
}
