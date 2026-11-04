using Spectre.Console;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>Result of opening a source, distinguishing displayed content from cancellation.</summary>
/// <param name="ExitCode">CLI exit code (zero for success/cancel).</param>
/// <param name="Displayed">Whether a document was actually shown to the user.</param>
public readonly record struct MdViewDisplayResult(int ExitCode, bool Displayed);

/// <summary>
/// Coordinates repeated document selection without coupling navigation to HTTP,
/// FileBrowser or Markdown rendering. All document readers remain independent.
/// </summary>
public sealed class MdViewSession
{
    private readonly IAnsiConsole _console;

    /// <summary>Create a session using the viewer's existing terminal.</summary>
    public MdViewSession(IAnsiConsole console) =>
        _console = console ?? throw new ArgumentNullException(nameof(console));

    /// <summary>
    /// Runs the initial command, optionally returning to the source menu after
    /// each successful full-document render. Noninteractive callers do not wait.
    /// The raw source pager owns its own navigation and is not double-prompted.
    /// </summary>
    public async Task<int> RunAsync(
        MdViewCommand initial,
        Func<CancellationToken, Task<MdViewCommand?>> chooseSource,
        Func<MdViewCommand, CancellationToken, Task<MdViewDisplayResult>> display,
        bool waitForCompletion,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(initial);
        ArgumentNullException.ThrowIfNull(chooseSource);
        ArgumentNullException.ThrowIfNull(display);

        var current = initial;
        var completion = new MdViewDocumentCompletionPrompt(_console);
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (current.Kind == MdViewSourceKind.Interactive)
            {
                // The actual source picker checks terminal capabilities, too.
                var selected = await chooseSource(cancellationToken).ConfigureAwait(false);
                if (selected is null)
                    return 0;
                current = selected;
            }

            var result = await display(current, cancellationToken).ConfigureAwait(false);
            if (result.ExitCode != 0 || !result.Displayed ||
                current.Kind == MdViewSourceKind.SourcePager || !waitForCompletion)
                return result.ExitCode;

            var action = await completion.WaitAsync(cancellationToken).ConfigureAwait(false);
            if (action == MdViewCompletionAction.Exit)
                return 0;

            current = new MdViewCommand(MdViewSourceKind.Interactive);
        }
    }
}
