using Spectre.Console;
using Spectre.Console.Rendering;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>
/// Indents the actual live Spectre selection prompt at the render-pipeline
/// stage, including its selected arrow, title and all list choices.
/// </summary>
/// <remarks>
/// Spectre's SelectionPrompt creates a ListPromptRenderHook and emits a
/// live renderable *after* IAnsiConsole.Write has been called. Wrapping
/// IAnsiConsole.Write with a Padder therefore cannot position that menu.
/// This hook is attached before opening the prompt, so Spectre's own
/// later-attached live hook runs first and this hook pads its result.
/// Control sequences are passed through to preserve native cursor
/// movement, Enter, Escape and the live redraw machinery.
/// </remarks>
internal sealed class MdViewCenteredPromptRenderHook : IRenderHook
{
    private readonly int _leftGutter;

    /// <summary>Creates a render hook for an already validated horizontal gutter.</summary>
    /// <param name="leftGutter">Nonnegative columns to indent the live menu.</param>
    public MdViewCenteredPromptRenderHook(int leftGutter)
    {
        if (leftGutter < 0)
            throw new ArgumentOutOfRangeException(nameof(leftGutter));
        _leftGutter = leftGutter;
    }

    /// <inheritdoc />
    public IEnumerable<IRenderable> Process(
        RenderOptions options, IEnumerable<IRenderable> renderables)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(renderables);

        foreach (var renderable in renderables)
        {
            // In particular, never wrap cursor-home/erase/up control
            // codes that Spectre emits when refreshing a live prompt.
            if (_leftGutter == 0 || renderable is ControlCode)
                yield return renderable;
            else
                yield return new Padder(
                    renderable, new Padding(_leftGutter, 0, 0, 0));
        }
    }
}

/// <summary>
/// Opens native Spectre selection prompts with optional horizontal
/// positioning of their real-time live renderable.
/// </summary>
internal static class MdViewCenteredPrompt
{
    /// <summary>
    /// Shows a bounded native selection prompt without replacing its
    /// keyboard input, cursor or terminal profile.
    /// </summary>
    /// <typeparam name="T">Non-null value type of selectable choices.</typeparam>
    /// <param name="console">Original Spectre console.</param>
    /// <param name="prompt">Native Spectre selection prompt.</param>
    /// <param name="leftGutter">Reading-column horizontal gutter.</param>
    /// <param name="cancellationToken">Prompt cancellation token.</param>
    /// <returns>Selected value, or the prompt's own Escape result.</returns>
    public static async Task<T> ShowAsync<T>(
        IAnsiConsole console, SelectionPrompt<T> prompt, int leftGutter,
        CancellationToken cancellationToken = default)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(console);
        ArgumentNullException.ThrowIfNull(prompt);
        if (leftGutter < 0 || leftGutter >= console.Profile.Width)
            throw new ArgumentOutOfRangeException(nameof(leftGutter));
        cancellationToken.ThrowIfCancellationRequested();

        if (leftGutter == 0)
            return await prompt.ShowAsync(console, cancellationToken)
                .ConfigureAwait(false);

        // Scope ends after Spectre clears/restores its live list. This is
        // essential: normal Markdown rows must never be indented twice.
        using var scope = new RenderHookScope(
            console, new MdViewCenteredPromptRenderHook(leftGutter));
        return await prompt.ShowAsync(console, cancellationToken)
            .ConfigureAwait(false);
    }
}
