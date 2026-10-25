using System.Text.Json;
using Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser;
using Afrowave.Toolbox.UnderstandMe.Settings;
using Spectre.Console;

namespace Afrowave.Toolbox.UnderstandMe.Configurator;

/// <summary>Interactive configuration shell, separate from the UnderstandMe core library.</summary>
public sealed class ConfiguratorApp
{
    private readonly IAnsiConsole _console;
    private readonly SpectreFileBrowser _picker;

    /// <summary>Create the editor with an optional injected terminal for testing.</summary>
    public ConfiguratorApp(IAnsiConsole? console = null)
    {
        _console = console ?? AnsiConsole.Console;
        _picker = new SpectreFileBrowser(new FileBrowserLocalSource(), _console);
    }

    /// <summary>Run until exit or cancellation. Exiting never saves drafts.</summary>
    public async Task RunAsync(string? path = null, CancellationToken cancellationToken = default)
    {
        if (!_console.Profile.Capabilities.Interactive)
            throw new InvalidOperationException("Configurator requires an interactive console.");

        ConfiguratorSession? session = null;
        if (path is not null)
            session = TryOpen(path);
        else
        {
            var local = System.IO.Path.Combine(Environment.CurrentDirectory, "UnderstandMeSettings.json");
            if (File.Exists(local))
                session = TryOpen(local);
        }

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _console.Write(new Text(session is null
                ? "UnderstandMe Configurator - no settings loaded\n"
                : $"UnderstandMe Configurator - {session.Path} ({(session.IsDirty ? "draft" : "saved")})\n"));
            var choices = session is null
                ? new[] { "Browse JSON", "Open path", "Create new", "Exit" }
                : new[] { "Show summary", "Edit language", "Edit worker capacity", "Validate", "Review & Save", "Browse JSON", "Exit" };

            var action = await new SelectionPrompt<string>()
                .Title("Choose an action")
                .PageSize(choices.Length)
                .AddChoices(choices)
                .AddCancelResult("Exit")
                .ShowAsync(_console, cancellationToken);
            if (action == "Exit")
            {
                if (session?.IsDirty == true)
                    _console.Write(new Text("Unsaved draft discarded.\n"));
                return;
            }

            switch (action)
            {
                case "Browse JSON":
                    var picked = await _picker.PickAsync(
                        new FileBrowserLocation("local", Environment.CurrentDirectory),
                        FileBrowserPickMode.File,
                        new SpectreFileBrowserOptions
                        {
                            FileFilter = new FileBrowserFileFilter(nameMatcher: new FileBrowserNameMatcher("UnderstandMeSettings.json;*.json"))
                        }, cancellationToken);
                    if (picked.IsSelected && picked.Location is not null)
                        session = TryOpen(picked.Location.Value) ?? session;
                    break;
                case "Open path":
                    session = TryOpen(await Prompt("Existing JSON path", "UnderstandMeSettings.json", cancellationToken)) ?? session;
                    break;
                case "Create new":
                    try
                    {
                        session = ConfiguratorSession.CreateNew(await Prompt("New JSON path", "UnderstandMeSettings.json", cancellationToken));
                        _console.Write(new Text("New draft only - no file created yet.\n"));
                    }
                    catch (Exception e) when (e is IOException or ArgumentException or UnauthorizedAccessException)
                    {
                        _console.Write(new Text("Cannot create draft: " + e.Message + "\n"));
                    }
                    break;
                case "Show summary":
                    _console.Write(new Text(
                        $"Default source language: {session!.Draft.Localization.DefaultSourceLanguage}\n" +
                        $"Storages: {session.Draft.Localization.Storages.Count}; Providers: {session.Draft.Translation.Providers.Count}\n" +
                        $"Worker buffer capacity: {session.Draft.Translation.Queues.DefaultWorkerBufferCapacity}\n"));
                    break;
                case "Edit language":
                    session!.SetSourceLanguage(await Prompt("Default source language", session.Draft.Localization.DefaultSourceLanguage, cancellationToken));
                    break;
                case "Edit worker capacity":
                    var entered = await Prompt("Worker buffer capacity",
                        session!.Draft.Translation.Queues.DefaultWorkerBufferCapacity.ToString(), cancellationToken);
                    if (int.TryParse(entered, out var number))
                        session.SetWorkerBufferCapacity(number);
                    else
                        _console.Write(new Text("Expected an integer.\n"));
                    break;
                case "Validate":
                    Report(session!.Validate());
                    break;
                case "Review & Save":
                    await ReviewAndSave(session!, cancellationToken);
                    break;
            }
        }
    }

    /// <summary>Validate a file without modifying it (useful for CI).</summary>
    public static IReadOnlyList<SettingsValidationIssue> ValidateFile(string path)
    {
        try
        {
            UnderstandMeSettingsJsonStore.Load(path);
            return [];
        }
        catch (UnderstandMeSettingsValidationException e)
        {
            return e.Issues;
        }
        catch (JsonException e)
        {
            return [new SettingsValidationIssue("$", "Invalid or unsupported JSON: " + e.Message)];
        }
    }

    private ConfiguratorSession? TryOpen(string path)
    {
        try
        {
            var session = ConfiguratorSession.Open(path);
            _console.Write(new Text("Opened: " + session.Path + "\n"));
            return session;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or ArgumentException or
            JsonException or UnderstandMeSettingsValidationException)
        {
            _console.Write(new Text("Cannot open settings: " + e.Message + "\n"));
            return null;
        }
    }

    private Task<string> Prompt(string title, string suggested, CancellationToken cancellationToken) =>
        new TextPrompt<string>(Markup.Escape(title))
            .DefaultValue(suggested)
            .AllowEmpty()
            .ShowAsync(_console, cancellationToken);

    private void Report(IReadOnlyList<SettingsValidationIssue> issues)
    {
        if (issues.Count == 0)
            _console.Write(new Text("Valid draft; nothing written.\n"));
        else
            foreach (var issue in issues)
                _console.Write(new Text(issue.Path + ": " + issue.Message + "\n"));
    }

    private async Task ReviewAndSave(ConfiguratorSession session, CancellationToken cancellationToken)
    {
        if (!session.IsDirty)
        {
            _console.Write(new Text("No changes to save.\n"));
            return;
        }

        // Do not display provider options or credentials. Only two nonsensitive fields are editable here.
        _console.Write(new Text($"Target: {session.Path}\n" +
            $"Language: {session.Draft.Localization.DefaultSourceLanguage}\n" +
            $"Worker buffer capacity: {session.Draft.Translation.Queues.DefaultWorkerBufferCapacity}\n"));
        if (!await new ConfirmationPrompt("Save these settings?")
            .ShowAsync(_console, cancellationToken))
        {
            _console.Write(new Text("Save cancelled; draft retained.\n"));
            return;
        }

        var issues = session.Validate();
        if (issues.Count != 0)
        {
            Report(issues);
            return;
        }

        try
        {
            session.Save();
            _console.Write(new Text("Saved successfully.\n"));
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or
            UnderstandMeSettingsValidationException)
        {
            _console.Write(new Text("Save failed; draft retained: " + e.Message + "\n"));
        }
    }
}
