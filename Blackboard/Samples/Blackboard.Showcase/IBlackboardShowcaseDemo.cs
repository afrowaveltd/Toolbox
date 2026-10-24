using Spectre.Console;

namespace Afrowave.Toolbox.Blackboard.Showcase;

/// <summary>One independently extendable interactive scenario in the Blackboard showcase.</summary>
/// <remarks>Showcase modules are demonstration code, not reusable UI NuGet packages.</remarks>
public interface IBlackboardShowcaseDemo
{
    /// <summary>Stable unique identifier for the scenario.</summary>
    string Id { get; }

    /// <summary>English human-readable menu label.</summary>
    string Title { get; }

    /// <summary>Executes one demonstration and returns to the main menu.</summary>
    Task RunAsync(
        IAnsiConsole console, string startingDirectory, CancellationToken cancellationToken);
}
