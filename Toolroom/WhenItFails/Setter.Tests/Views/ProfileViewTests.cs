using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Views;
using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Views;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class ConsoleOutputCollection
{
    public const string Name = "Console output";
}

[Collection(ConsoleOutputCollection.Name)]
public sealed class ProfileViewTests
{
    [Fact]
    public void ShowPlain_WithMetadata_PrintsSortedMetadata()
    {
        WhenItFailsWorkspaceSummary summary = new()
        {
            DisplayPath = "test-workspace"
        };
        ErrorProfileDefinition profile = new()
        {
            Name = "DITA_TEST",
            DisplayName = "DiTa Test",
            Source = "Project"
        };
        profile.Metadata.Set("ZETA", "last");
        profile.Metadata.Set("ALPHA", "first");

        string output = CaptureConsoleOutput(() =>
            ProfileView.ShowPlain(summary, profile));

        Assert.Contains(
            $"Metadata: ALPHA=first, ZETA=last{Environment.NewLine}",
            output,
            StringComparison.Ordinal);
    }

    [Fact]
    public void ShowPlain_WithoutMetadata_PrintsNone()
    {
        WhenItFailsWorkspaceSummary summary = new()
        {
            DisplayPath = "test-workspace"
        };
        ErrorProfileDefinition profile = new()
        {
            Name = "DITA_TEST",
            DisplayName = "DiTa Test",
            Source = "Project"
        };

        string output = CaptureConsoleOutput(() =>
            ProfileView.ShowPlain(summary, profile));

        Assert.Contains(
            $"Metadata: None{Environment.NewLine}",
            output,
            StringComparison.Ordinal);
    }

    private static string CaptureConsoleOutput(Action action)
    {
        TextWriter originalOutput = Console.Out;
        using StringWriter output = new();

        try
        {
            Console.SetOut(output);
            action();
            return output.ToString();
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
    }
}
