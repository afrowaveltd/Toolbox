using Spectre.Console;

namespace Afrowave.Toolbox.Blackboard.Showcase;

/// <summary>
/// Reusable host for menu-based demonstrations of independent Blackboard tools.
/// </summary>
public sealed class BlackboardShowcaseHost
{
    private readonly IReadOnlyList<IBlackboardShowcaseDemo> _demos;
    private readonly IAnsiConsole _console;

    /// <summary>Registers available scenarios in deterministic menu order.</summary>
    public BlackboardShowcaseHost(
        IEnumerable<IBlackboardShowcaseDemo> demos, IAnsiConsole? console = null)
    {
        ArgumentNullException.ThrowIfNull(demos);
        _console = console ?? AnsiConsole.Console;
        _demos = demos.ToArray();

        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var demo in _demos)
        {
            if (demo is null || string.IsNullOrWhiteSpace(demo.Id) ||
                string.IsNullOrWhiteSpace(demo.Title) || !ids.Add(demo.Id))
                throw new ArgumentException("Showcase demos must be non-null and have unique nonempty IDs and labels.",
                    nameof(demos));
        }
    }

    /// <summary>Runs the interactive main menu until Exit is selected.</summary>
    public async Task RunAsync(string startingDirectory, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(startingDirectory))
            throw new ArgumentException("A starting directory is required.", nameof(startingDirectory));
        if (!_console.Profile.Capabilities.Interactive)
            throw new InvalidOperationException("Blackboard Showcase requires an interactive terminal.");

        // Clear prior shell output once, not on every demo/menu navigation.
        _console.Clear();

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _console.Write(new Text("Blackboard Showcase — Afrowave.Toolbox" + Environment.NewLine));
            var items = _demos
                .Select(demo => new ShowcaseChoice(demo.Title, demo))
                .Append(new ShowcaseChoice("Exit", null))
                .ToArray();

            var selection = await new SelectionPrompt<ShowcaseChoice>()
                .Title("Choose a demonstration")
                .PageSize(Math.Clamp(items.Length, 3, 15))
                .UseConverter(item => Markup.Escape(item.Label))
                .AddChoices(items)
                .AddCancelResult(items[^1])
                .ShowAsync(_console, cancellationToken);

            if (selection.Demo is null)
                return;

            await selection.Demo.RunAsync(_console, startingDirectory, cancellationToken);
        }
    }

    private sealed record ShowcaseChoice(string Label, IBlackboardShowcaseDemo? Demo);
}
