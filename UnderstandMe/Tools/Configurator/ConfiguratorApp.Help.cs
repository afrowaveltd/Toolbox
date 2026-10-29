using Spectre.Console;

namespace Afrowave.Toolbox.UnderstandMe.Configurator;

public sealed partial class ConfiguratorApp
{
    /// <summary>Open one context topic, or let the user browse the offline topic catalog.</summary>
    private async Task ShowHelpAsync(string? topicId, CancellationToken cancellationToken)
    {
        if (topicId is not null)
        {
            await ShowHelpTopicAsync(topicId, cancellationToken);
            return;
        }

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _console.Clear();
            Write(T("help.title") + "\n");

            var choices = ConfiguratorHelpCatalog.Topics
                .Select(topic => new HelpChoice(topic.Id, T(topic.TitleKey)))
                .Append(new HelpChoice(null, T("menu.back")))
                .ToArray();

            var selection = await new SelectionPrompt<HelpChoice>()
                .Title(Markup.Escape(T("menu.heading")))
                .PageSize(Math.Clamp(choices.Length, 3, 15))
                .UseConverter(choice => Markup.Escape(choice.Label))
                .AddChoices(choices)
                .AddCancelResult(choices[^1])
                .ShowAsync(_console, cancellationToken);

            if (selection.Id is null)
                return;
            await ShowHelpTopicAsync(selection.Id, cancellationToken);
        }
    }

    private async Task ShowHelpTopicAsync(string topicId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _console.Clear();
        Write(T("help.english.note") + "\n\n");
        ConfiguratorHelpRenderer.Render(_console, ConfiguratorHelpCatalog.ReadEnglish(topicId));

        var choice = T("help.return");
        await new SelectionPrompt<string>()
            .Title(Markup.Escape(T("menu.result.heading")))
            .AddChoices(choice)
            .AddCancelResult(choice)
            .ShowAsync(_console, cancellationToken);
    }

    private sealed record HelpChoice(string? Id, string Label);
}
