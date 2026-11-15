using Spectre.Console;
using Spectre.Console.Rendering;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>
/// Positions interactive Spectre list prompts at the same left gutter as
/// an already-centered Markdown reading column.
/// </summary>
/// <remarks>
/// Only use this adapter for whole-widget interactive list prompts. An
/// incremental free-text prompt must use the original console with an
/// indented title so typed characters do not receive repeated padding.
/// The original console input, cursor, profile and render pipeline remain
/// shared; no fake terminal width or custom key-handling is introduced.
/// </remarks>
internal sealed class MdViewIndentedPromptConsole : IAnsiConsole
{
    private readonly IAnsiConsole _inner;
    private readonly int _leftGutter;

    /// <summary>Wraps the source console with zero-vertical list-prompt padding.</summary>
    /// <param name="inner">Original interactive Spectre console.</param>
    /// <param name="leftGutter">Number of horizontal columns to indent prompts.</param>
    public MdViewIndentedPromptConsole(IAnsiConsole inner, int leftGutter)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        if (leftGutter < 0 || leftGutter >= inner.Profile.Width)
            throw new ArgumentOutOfRangeException(nameof(leftGutter));
        _leftGutter = leftGutter;
    }

    /// <inheritdoc />
    public Profile Profile => _inner.Profile;

    /// <inheritdoc />
    public IAnsiConsoleCursor Cursor => _inner.Cursor;

    /// <inheritdoc />
    public IAnsiConsoleInput Input => _inner.Input;

    /// <inheritdoc />
    public IExclusivityMode ExclusivityMode => _inner.ExclusivityMode;

    /// <inheritdoc />
    public RenderPipeline Pipeline => _inner.Pipeline;

    /// <inheritdoc />
    public void Clear(bool home) => _inner.Clear(home);

    /// <inheritdoc />
    public void Write(IRenderable renderable)
    {
        ArgumentNullException.ThrowIfNull(renderable);
        _inner.Write(_leftGutter == 0
            ? renderable
            : new Padder(renderable, new Padding(_leftGutter, 0, 0, 0)));
    }

    /// <inheritdoc />
    public void WriteAnsi(Action<AnsiWriter> action) =>
        _inner.WriteAnsi(action);
}
