namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;

/// <summary>
/// Serializes tests that temporarily replace the process-wide Console.Out writer.
/// </summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class ConsoleOutputTestCollection
{
    public const string Name = "Console output";
}
