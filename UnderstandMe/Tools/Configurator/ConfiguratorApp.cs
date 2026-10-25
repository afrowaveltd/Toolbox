using System.Text.Json;
using Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser;
using Afrowave.Toolbox.UnderstandMe.Settings;
using Spectre.Console;

namespace Afrowave.Toolbox.UnderstandMe.Configurator;

/// <summary>Standalone offline TUI over UnderstandMe's shared configuration contracts.</summary>
public sealed class ConfiguratorApp
{
    private readonly IAnsiConsole _console;
    private readonly SpectreFileBrowser _picker;
    private readonly string _startingDirectory;
    private readonly ConfiguratorPreferencesStore _preferencesStore;
    private ConfiguratorPreferencesDocument _preferences = new(new ConfiguratorPreferences(), null);
    private ConfiguratorStrings _strings = new("en");

    /// <summary>Create an editor; optional injected console, directory and preferences path support tests.</summary>
    public ConfiguratorApp(
        IAnsiConsole? console = null, string? startingDirectory = null, string? preferencesPath = null)
    {
        _console = console ?? AnsiConsole.Console;
        _startingDirectory = System.IO.Path.GetFullPath(startingDirectory ?? Environment.CurrentDirectory);
        _preferencesStore = new ConfiguratorPreferencesStore(
            preferencesPath ?? ConfiguratorPreferencesStore.DefaultFilePath());
        _picker = new SpectreFileBrowser(new FileBrowserLocalSource(), _console);
    }

    /// <summary>Run the parameterless launcher, or open an optional explicit configuration path.</summary>
    /// <remarks>No configuration or user preference is implicitly saved on exit.</remarks>
    public async Task RunAsync(string? path = null, CancellationToken cancellationToken = default)
    {
        if (!_console.Profile.Capabilities.Interactive)
            throw new InvalidOperationException("Configurator requires an interactive console.");

        // A clean startup screen, then a clean screen whenever we return to a menu.
        // Do not clear validation or diagnostic output until the user has dismissed it.
        _console.Clear();
        LoadPreferences();

        var clearBeforeMainMenu = false;
        if (!string.IsNullOrWhiteSpace(path))
        {
            var direct = TryOpen(path);
            if (direct is not null)
                await EditAsync(direct, cancellationToken);
            else
                await WaitForReturnAsync(cancellationToken);
            clearBeforeMainMenu = true;
        }

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (clearBeforeMainMenu)
                _console.Clear();
            clearBeforeMainMenu = true;
            Write(T("app.title") + "\n");
            var choice = await Choose(
            [
                Item(Action.Create, "menu.create"),
                Item(Action.Edit, "menu.edit"),
                Item(Action.Test, "menu.test"),
                Item(Action.Preferences, "menu.preferences"),
                Item(Action.Exit, "menu.exit")
            ], Action.Exit, cancellationToken);

            switch (choice)
            {
                case Action.Create:
                    await CreateAsync(cancellationToken);
                    break;
                case Action.Edit:
                    await OpenEditorAsync(cancellationToken);
                    break;
                case Action.Test:
                    await TestAsync(cancellationToken);
                    break;
                case Action.Preferences:
                    await PreferencesAsync(cancellationToken);
                    break;
                case Action.Exit:
                    return;
            }
        }
    }

    /// <summary>Validate strict JSON and settings schemas without any network call or file write.</summary>
    public static IReadOnlyList<SettingsValidationIssue> ValidateFile(string path)
    {
        try
        {
            UnderstandMeSettingsJsonStore.Load(path);
            return [];
        }
        catch (UnderstandMeSettingsValidationException ex)
        {
            return ex.Issues;
        }
        catch (JsonException ex)
        {
            return [new SettingsValidationIssue("$", "Invalid or unsupported JSON: " + ex.Message)];
        }
    }

    private void LoadPreferences()
    {
        try
        {
            _preferences = _preferencesStore.Load();
            _strings = new ConfiguratorStrings(_preferences.Settings.Language);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or InvalidDataException)
        {
            // A broken app preference must never prevent repairing a host configuration.
            // Do not silently overwrite the broken preference document on the next save.
            _preferences = new(new ConfiguratorPreferences(), null);
            _strings = new ConfiguratorStrings("en");
            Write(T("prefs.load.error") + ex.Message + "\n");
        }
    }

    private async Task PreferencesAsync(CancellationToken cancellationToken)
    {
        Write(T("prefs.title") + "\n" + T("prefs.current") + _strings.Language + "\n");
        var choice = await Choose(
        [
            Item(Action.English, "prefs.english"),
            Item(Action.Czech, "prefs.czech"),
            Item(Action.Back, "menu.back")
        ], Action.Back, cancellationToken);

        var language = choice switch
        {
            Action.English => "en",
            Action.Czech => "cs",
            _ => null
        };
        if (language is null || language == _preferences.Settings.Language)
            return;

        try
        {
            var candidate = new ConfiguratorPreferences { Language = language };
            var revision = _preferencesStore.Save(candidate, _preferences.Revision);
            _preferences = new(candidate, revision);
            _strings = new ConfiguratorStrings(language);
            Write(T("prefs.saved") + "\n");
            await WaitForReturnAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException)
        {
            Write(T("prefs.error") + ex.Message + "\n");
            await WaitForReturnAsync(cancellationToken);
        }
    }

    private async Task CreateAsync(CancellationToken cancellationToken)
    {
        var newPath = await Prompt("editor.path.new", "UnderstandMeSettings.json", cancellationToken);
        try
        {
            var session = ConfiguratorSession.CreateNew(newPath);
            Write(T("editor.created") + "\n");
            await EditAsync(session, cancellationToken);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        {
            Write(T("editor.create.error") + ex.Message + "\n");
            await WaitForReturnAsync(cancellationToken);
        }
    }

    private async Task OpenEditorAsync(CancellationToken cancellationToken)
    {
        var path = await SelectFileAsync(cancellationToken);
        if (path is null)
            return;
        var session = TryOpen(path);
        if (session is not null)
            await EditAsync(session, cancellationToken);
        else
            await WaitForReturnAsync(cancellationToken);
    }

    private async Task TestAsync(CancellationToken cancellationToken)
    {
        Write(T("test.title") + "\n");
        var path = await SelectFileAsync(cancellationToken);
        if (path is null)
            return;

        try
        {
            var issues = ValidateFile(path);
            if (issues.Count == 0)
                Write(T("test.valid") + "\n");
            else
                ShowIssues(issues);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        {
            Write(T("test.error") + ex.Message + "\n");
        }

        // Validation results are useful until acknowledged, not after the next menu appears.
        await WaitForReturnAsync(cancellationToken);
    }

    private async Task<string?> SelectFileAsync(CancellationToken cancellationToken)
    {
        var method = await Choose(
        [
            Item(Action.Browse, "select.browse"),
            Item(Action.Path, "select.path"),
            Item(Action.Back, "menu.back")
        ], Action.Back, cancellationToken, "select.heading");

        if (method == Action.Path)
            return await Prompt("editor.path.existing", "UnderstandMeSettings.json", cancellationToken);
        if (method != Action.Browse)
            return null;

        var result = await _picker.PickAsync(
            new FileBrowserLocation("local", _startingDirectory),
            FileBrowserPickMode.File,
            new SpectreFileBrowserOptions
            {
                FileFilter = new FileBrowserFileFilter(
                    nameMatcher: new FileBrowserNameMatcher("UnderstandMeSettings.json;*.json")),
                Labels = BrowserLabels()
            }, cancellationToken);
        return result.IsSelected ? result.Location?.Value : null;
    }

    private SpectreFileBrowserLabels BrowserLabels() => new()
    {
        Browse = T("picker.browse"), Choose = T("picker.choose"),
        SelectFolder = T("picker.folder"), Parent = T("picker.parent"),
        Search = T("picker.search"), Tree = T("picker.tree"),
        Filter = T("picker.filter"), Back = T("picker.back"),
        Pattern = T("picker.pattern"), Masks = T("picker.masks"),
        Regex = T("picker.regex"), All = T("picker.all"),
        Json = T("picker.json"), Text = T("picker.text"),
        Images = T("picker.images"), UseFolder = T("picker.usefolder"),
        NothingFound = T("picker.notfound"), Limited = T("picker.limited"),
        Partial = T("picker.partial"), ListingError = T("picker.list.error"),
        Empty = T("picker.empty"), Matches = T("picker.matches")
    };

    private ConfiguratorSession? TryOpen(string path)
    {
        try
        {
            var session = ConfiguratorSession.Open(path);
            Write(T("editor.opened") + session.Path + "\n");
            return session;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or
            JsonException or UnderstandMeSettingsValidationException)
        {
            Write(T("editor.open.error") + ex.Message + "\n");
            return null;
        }
    }

    private async Task EditAsync(ConfiguratorSession session, CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _console.Clear();
            Write((session.Revision is null ? T("editor.new") : T("editor.title")) +
                  session.Path + " [" +
                  T(session.IsDirty ? "editor.unsaved" : "editor.saved") + "]\n");

            var action = await Choose(
            [
                Item(Action.Summary, "editor.summary"),
                Item(Action.Source, "editor.source"),
                Item(Action.Buffer, "editor.buffer"),
                Item(Action.Validate, "editor.validate"),
                Item(Action.Save, "editor.save"),
                Item(Action.Back, "menu.back")
            ], Action.Back, cancellationToken);

            switch (action)
            {
                case Action.Summary:
                    Write(T("editor.summary.source") + session.Draft.Localization.DefaultSourceLanguage + "\n" +
                          T("editor.summary.storages") + session.Draft.Localization.Storages.Count + "; " +
                          T("editor.summary.providers") + session.Draft.Translation.Providers.Count + "\n" +
                          T("editor.summary.buffer") + session.Draft.Translation.Queues.DefaultWorkerBufferCapacity + "\n");
                    await WaitForReturnAsync(cancellationToken);
                    break;
                case Action.Source:
                    session.SetSourceLanguage(await Prompt("editor.language.prompt",
                        session.Draft.Localization.DefaultSourceLanguage, cancellationToken));
                    break;
                case Action.Buffer:
                    var entered = await Prompt("editor.buffer.prompt",
                        session.Draft.Translation.Queues.DefaultWorkerBufferCapacity.ToString(), cancellationToken);
                    if (int.TryParse(entered, out var capacity))
                        session.SetWorkerBufferCapacity(capacity);
                    else
                    {
                        Write(T("editor.expected.integer") + "\n");
                        await WaitForReturnAsync(cancellationToken);
                    }
                    break;
                case Action.Validate:
                    ShowIssues(session.Validate());
                    await WaitForReturnAsync(cancellationToken);
                    break;
                case Action.Save:
                    await ReviewAndSaveAsync(session, cancellationToken);
                    await WaitForReturnAsync(cancellationToken);
                    break;
                case Action.Back:
                    if (!session.IsDirty || await Confirm("editor.discard", cancellationToken))
                        return;
                    break;
            }
        }
    }

    private void ShowIssues(IReadOnlyList<SettingsValidationIssue> issues)
    {
        if (issues.Count == 0)
        {
            Write(T("editor.valid") + "\n");
            return;
        }

        Write(T("editor.invalid") + "\n");
        // Core validation messages remain in English for now. Paths remain stable machine-readable identifiers.
        foreach (var issue in issues)
            Write(issue.Path + ": " + issue.Message + "\n");
    }

    private async Task ReviewAndSaveAsync(ConfiguratorSession session, CancellationToken cancellationToken)
    {
        if (!session.IsDirty)
        {
            Write(T("editor.nochanges") + "\n");
            return;
        }

        // Never reveal provider options/secrets; only the two nonsensitive editable fields.
        Write(T("editor.review") + "\n" +
              T("editor.target") + session.Path + "\n" +
              T("editor.summary.source") + session.Draft.Localization.DefaultSourceLanguage + "\n" +
              T("editor.summary.buffer") + session.Draft.Translation.Queues.DefaultWorkerBufferCapacity + "\n");
        if (!await Confirm("editor.confirm", cancellationToken))
        {
            Write(T("editor.cancel") + "\n");
            return;
        }

        var issues = session.Validate();
        if (issues.Count != 0)
        {
            ShowIssues(issues);
            return;
        }

        try
        {
            session.Save();
            Write(T("editor.saved.ok") + "\n");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or
            UnderstandMeSettingsValidationException)
        {
            Write(T("editor.save.error") + ex.Message + "\n");
        }
    }

    // Use an ordinary localized prompt rather than a key-binding: works over SSH,
    // Spectre.Console.Testing, and terminals that intercept Ctrl or flow-control keys.
    // The caller clears the screen only after this acknowledgement.
    private async Task WaitForReturnAsync(CancellationToken cancellationToken)
    {
        var resume = Item(Action.Continue, "menu.continue");
        await new SelectionPrompt<Choice>()
            .Title(Markup.Escape(T("menu.result.heading")))
            .AddChoices(resume)
            .AddCancelResult(resume)
            .ShowAsync(_console, cancellationToken);
    }

    private Task<string> Prompt(string labelKey, string defaultValue, CancellationToken cancellationToken) =>
        new TextPrompt<string>(Markup.Escape(T(labelKey)))
            .DefaultValue(defaultValue)
            .AllowEmpty()
            .ShowAsync(_console, cancellationToken);

    private Task<bool> Confirm(string labelKey, CancellationToken cancellationToken) =>
        new ConfirmationPrompt(Markup.Escape(T(labelKey))).ShowAsync(_console, cancellationToken);

    private Task<Action> Choose(
        Choice[] items, Action cancel, CancellationToken cancellationToken, string heading = "menu.heading")
    {
        var back = items.First(x => x.Kind == cancel);
        return ChooseCore(items, back, cancellationToken, heading);
    }

    private async Task<Action> ChooseCore(
        Choice[] items, Choice cancel, CancellationToken cancellationToken, string heading)
    {
        var selection = await new SelectionPrompt<Choice>()
            .Title(Markup.Escape(T(heading)))
            .PageSize(Math.Clamp(items.Length, 3, 15))
            .UseConverter(option => Markup.Escape(option.Label))
            .AddChoices(items)
            .AddCancelResult(cancel)
            .ShowAsync(_console, cancellationToken);
        return selection.Kind;
    }

    private Choice Item(Action kind, string key) => new(kind, T(key));
    private string T(string key) => _strings[key];
    private void Write(string text) => _console.Write(new Text(text));

    private sealed record Choice(Action Kind, string Label);

    private enum Action
    {
        Create, Edit, Test, Preferences, Exit,
        Browse, Path, Back, English, Czech,
        Summary, Source, Buffer, Validate, Save, Continue
    }
}
