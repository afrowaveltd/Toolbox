using Spectre.Console;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>Action chosen at the end of a fully displayed Markdown document.</summary>
public enum MdViewCompletionAction
{
    /// <summary>End the viewer session.</summary>
    Exit,

    /// <summary>Return to the source-selection menu.</summary>
    MainMenu
}

/// <summary>
/// Reserves simple, explicit document-end keys. Unknown keys are ignored,
/// leaving them available for future search and page-navigation commands.
/// </summary>
public sealed class MdViewDocumentCompletionPrompt
{
    private readonly IAnsiConsole _console;

    /// <summary>Build a prompt using the same console as the Markdown renderer.</summary>
    public MdViewDocumentCompletionPrompt(IAnsiConsole console) =>
        _console = console ?? throw new ArgumentNullException(nameof(console));

    /// <summary>
    /// Wait for Enter (main menu) or Escape (exit). Returns Exit immediately
    /// for noninteractive consoles or when the input stream ends.
    /// The caller must also check redirected stdin/stdout before invoking this
    /// prompt in a CLI pipeline.
    /// </summary>
    public async Task<MdViewCompletionAction> WaitAsync(
        CancellationToken cancellationToken = default)
    {
        if (!_console.Profile.Capabilities.Interactive)
            return MdViewCompletionAction.Exit;

        _console.Write(new Text(Environment.NewLine +
            "Enter: main menu  |  Esc: exit" + Environment.NewLine));

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var key = await _console.Input.ReadKeyAsync(
                intercept: true, cancellationToken).ConfigureAwait(false);
            if (key is null || key.Value.Key == ConsoleKey.Escape)
                return MdViewCompletionAction.Exit;
            if (key.Value.Key == ConsoleKey.Enter)
                return MdViewCompletionAction.MainMenu;
            // Other keys intentionally do nothing for now.
        }
    }
}
