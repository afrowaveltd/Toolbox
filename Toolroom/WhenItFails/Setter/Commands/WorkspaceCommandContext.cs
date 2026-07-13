using Afrowave.Toolbox.SeeMe.WhenItFails.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Contains a validated and loaded WhenItFails workspace for command execution.
/// </summary>
internal sealed class WorkspaceCommandContext
{
    public required WhenItFailsWorkspaceSummary Summary { get; init; }
}

/// <summary>
/// Validates and loads a WhenItFails workspace for read-only commands.
/// </summary>
internal static class WorkspaceCommandContextLoader
{
    public static Task<WorkspaceCommandContext?> TryLoadAsync(string inputPath)
    {
        WhenItFailsWorkspaceValidator validator = new();
        WhenItFailsWorkspaceSummarizer summarizer = new();

        return TryLoadAsync(
            inputPath,
            path => validator.ValidateAsync(path),
            path => summarizer.LoadAsync(path),
            ShowValidationFailure);
    }

    internal static async Task<WorkspaceCommandContext?> TryLoadAsync(
        string inputPath,
        Func<string, Task<WhenItFailsWorkspaceValidationOutcome>> validateAsync,
        Func<string, Task<WhenItFailsWorkspaceSummary>> loadSummaryAsync,
        Action<WhenItFailsWorkspaceValidationOutcome> showValidationFailure)
    {
        ArgumentNullException.ThrowIfNull(validateAsync);
        ArgumentNullException.ThrowIfNull(loadSummaryAsync);
        ArgumentNullException.ThrowIfNull(showValidationFailure);

        WhenItFailsWorkspaceValidationOutcome validationOutcome =
            await validateAsync(inputPath);

        if (!validationOutcome.ValidationResult.IsValid)
        {
            showValidationFailure(validationOutcome);
            return null;
        }

        WhenItFailsWorkspaceSummary summary =
            await loadSummaryAsync(inputPath);

        return new WorkspaceCommandContext
        {
            Summary = summary
        };
    }

    private static void ShowValidationFailure(
        WhenItFailsWorkspaceValidationOutcome validationOutcome)
    {
        new ConsoleValidationResultShow().Show(
            validationOutcome.ValidationResult,
            new ConsoleShowOptions
            {
                SourcePath = validationOutcome.DisplayPath
            });
    }
}
