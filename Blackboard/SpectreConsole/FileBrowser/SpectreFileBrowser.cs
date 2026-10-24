using System.Text.RegularExpressions;
using Spectre.Console;

namespace Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser;

/// <summary>Select an existing file or directory.</summary>
public enum FileBrowserPickMode
{
    /// <summary>Select an existing file.</summary>
    File,
    /// <summary>Select an existing directory.</summary>
    Directory
}

/// <summary>Selected provider-qualified location, or an explicit cancellation.</summary>
/// <param name="Location">Null on cancellation.</param>
/// <param name="IsDirectory">Whether the selected location is a directory.</param>
public sealed record FileBrowserPickResult(FileBrowserLocation? Location, bool IsDirectory)
{
    /// <summary>Whether the user selected a location.</summary>
    public bool IsSelected => Location is not null;

    /// <summary>Returns an explicit cancellation result.</summary>
    public static FileBrowserPickResult Cancelled() => new(null, false);
}

/// <summary>Options for an independent, read-only Spectre.Console file picker.</summary>
public sealed record SpectreFileBrowserOptions
{
    /// <summary>Number of visible items in each paged selection prompt.</summary>
    public int PageSize { get; init; } = 12;

    /// <summary>Choose Unicode/emoji/plain-text icons.</summary>
    public FileBrowserIconMode IconMode { get; init; } = FileBrowserIconMode.Auto;

    /// <summary>Optional initial filename filter.</summary>
    public FileBrowserFileFilter? FileFilter { get; init; }

    /// <summary>Safety limits for recursive searching.</summary>
    public FileBrowserSearchOptions SearchOptions { get; init; } = new();

    /// <summary>Maximum number of matches shown in the optional tree preview.</summary>
    public int SearchTreePreviewLimit { get; init; } = 40;

    /// <summary>True only when the user or host has explicitly confirmed emoji rendering.</summary>
    public bool EmojiConfirmed { get; init; }

    /// <summary>Caller-provided localization; English is the reliable fallback.</summary>
    public SpectreFileBrowserLabels Labels { get; init; } = new();

    internal void Validate()
    {
        if (PageSize is < 3 or > 100)
            throw new ArgumentOutOfRangeException(nameof(PageSize));
        if (SearchTreePreviewLimit is < 0 or > 5000)
            throw new ArgumentOutOfRangeException(nameof(SearchTreePreviewLimit));
        ArgumentNullException.ThrowIfNull(Labels);
        ArgumentNullException.ThrowIfNull(SearchOptions);
        SearchOptions.Validate();
    }
}

/// <summary>Replaceable user-interface strings, without a localization-framework dependency.</summary>
public sealed record SpectreFileBrowserLabels
{
    /// <summary>File picker heading.</summary>
    public string Browse { get; init; } = "FileBrowser";
    /// <summary>Selection prompt heading.</summary>
    public string Choose { get; init; } = "Choose an entry or action";
    /// <summary>Action for current folder selection.</summary>
    public string SelectFolder { get; init; } = "Select current folder";
    /// <summary>Action for navigating upward.</summary>
    public string Parent { get; init; } = "Parent folder ..";
    /// <summary>Search action.</summary>
    public string Search { get; init; } = "Recursive search";
    /// <summary>Directory tree action.</summary>
    public string Tree { get; init; } = "Browse directory tree";
    /// <summary>File filter action.</summary>
    public string Filter { get; init; } = "File filter";
    /// <summary>Cancel/back action.</summary>
    public string Back { get; init; } = "Back / cancel";
    /// <summary>Prompt for user-entered mask/regex.</summary>
    public string Pattern { get; init; } = "Filename mask or regular expression (empty = cancel)";
    /// <summary>Wildcard mode label.</summary>
    public string Masks { get; init; } = "Masks (*, ?, e.g. *.ajis;*.json)";
    /// <summary>Regex mode label.</summary>
    public string Regex { get; init; } = "Regular expression";
    /// <summary>All filenames filter.</summary>
    public string All { get; init; } = "All files (*)";
    /// <summary>JSON files filter.</summary>
    public string Json { get; init; } = "JSON files";
    /// <summary>Text files filter.</summary>
    public string Text { get; init; } = "Text files";
    /// <summary>Image files filter.</summary>
    public string Images { get; init; } = "Images";
    /// <summary>Folder tree label.</summary>
    public string UseFolder { get; init; } = "Open this folder in browser";
    /// <summary>Search completed without results.</summary>
    public string NothingFound { get; init; } = "Nothing found; original directory unchanged.";
    /// <summary>Search was not exhaustive.</summary>
    public string Limited { get; init; } = "Search safety limit reached: results may be incomplete.";
    /// <summary>Some directories were inaccessible.</summary>
    public string Partial { get; init; } = "Some directories were inaccessible: results may be incomplete.";
    /// <summary>Directory listing failure.</summary>
    public string ListingError { get; init; } = "Unable to list directory";
    /// <summary>No visible file/folder entries.</summary>
    public string Empty { get; init; } = "No visible entries in this folder.";
    /// <summary>Heading for all matching paths.</summary>
    public string Matches { get; init; } = "Matching relative paths";
}

/// <summary>
/// Interactive Spectre.Console adapter for read-only sources, with folder selection,
/// universal filename filtering, a lazy folder tree and multi-result recursive search.
/// </summary>
/// <remarks>
/// Prompts are sequential, never clear the host screen, and never change process-wide
/// CWD, output encoding, files or credentials. Do not share one console between concurrent
/// picker sessions. An interactive terminal is required for selection.
/// </remarks>
public sealed class SpectreFileBrowser
{
    private readonly IFileBrowserSource _source;
    private readonly IAnsiConsole _console;
    private readonly FileBrowserFileIcons _icons;
    private readonly FileBrowserKnownFolders? _knownFolders;

    /// <summary>Constructs a picker with a chosen storage provider and console.</summary>
    public SpectreFileBrowser(
        IFileBrowserSource source, IAnsiConsole? console = null,
        FileBrowserFileIcons? icons = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        _source = source;
        _console = console ?? AnsiConsole.Console;
        _icons = icons ?? new FileBrowserFileIcons();
        _knownFolders = source is FileBrowserLocalSource
            ? FileBrowserKnownFolders.ForCurrentPlatform() : null;
    }

    /// <summary>Opens a picker and returns an existing file/folder or explicit cancellation.</summary>
    public async Task<FileBrowserPickResult> PickAsync(
        FileBrowserLocation start, FileBrowserPickMode mode = FileBrowserPickMode.File,
        SpectreFileBrowserOptions? options = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(start);
        if (!Enum.IsDefined(mode))
            throw new ArgumentOutOfRangeException(nameof(mode));
        if (start.SourceId != _source.Id)
            throw new ArgumentException("Location belongs to another source.", nameof(start));
        options ??= new SpectreFileBrowserOptions();
        options.Validate();

        if (!_console.Profile.Capabilities.Interactive)
            throw new InvalidOperationException("An interactive terminal is required to pick a file.");

        var labels = options.Labels;
        var terminal = new FileBrowserTerminalCapabilities(
            _console.Profile.Capabilities.Unicode, true, options.EmojiConfirmed);
        var session = new FileBrowserNavigationSession(_source, start);
        var filter = options.FileFilter;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            WriteLine(labels.Browse + " — " + session.CurrentDirectory.Value);

            IReadOnlyList<FileBrowserSourceEntry> entries;
            try
            {
                entries = await _source.ListAsync(session.CurrentDirectory, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                WriteLine(labels.ListingError + ": " + ex.Message);
                entries = [];
            }

            var actions = new List<MenuEntry>();
            foreach (var entry in entries)
            {
                if (mode == FileBrowserPickMode.Directory && !entry.IsDirectory)
                    continue;
                if (filter is not null)
                {
                    bool permitted;
                    try
                    {
                        permitted = filter.Allows(entry.Name, entry.IsDirectory);
                    }
                    catch (RegexMatchTimeoutException)
                    {
                        // Invalid expensive regex must not make the picker unusable.
                        // Revert to an unfiltered listing and show a diagnostic.
                        WriteLine("File filter regular expression timed out; filter was cleared.");
                        filter = null;
                        permitted = true;
                    }

                    if (!permitted)
                        continue;
                }
                var special = entry.IsDirectory
                    ? _knownFolders?.Classify(entry.Location.Value, true) ?? FileBrowserKnownFolderKind.None
                    : FileBrowserKnownFolderKind.None;
                var rendered = FileBrowserEntryPresentation.Format(
                    entry.Name, entry.IsDirectory, _icons, options.IconMode, terminal, special);
                actions.Add(new MenuEntry(BrowserAction.Entry, rendered, entry));
            }

            if (actions.Count == 0)
                WriteLine(labels.Empty);

            actions.Add(new MenuEntry(BrowserAction.Parent, labels.Parent));
            if (mode == FileBrowserPickMode.Directory)
                actions.Add(new MenuEntry(BrowserAction.SelectFolder, labels.SelectFolder));
            actions.Add(new MenuEntry(BrowserAction.Search, labels.Search));
            actions.Add(new MenuEntry(BrowserAction.Tree, labels.Tree));
            actions.Add(new MenuEntry(BrowserAction.Filter, labels.Filter));
            var back = new MenuEntry(BrowserAction.Back, labels.Back);
            actions.Add(back);

            var choice = await ChooseAsync(labels.Choose, actions, back, options, cancellationToken);
            switch (choice.Action)
            {
                case BrowserAction.Back:
                    return FileBrowserPickResult.Cancelled();
                case BrowserAction.SelectFolder:
                    return new FileBrowserPickResult(session.CurrentDirectory, true);
                case BrowserAction.Parent:
                {
                    try
                    {
                        var parent = await _source.GetParentAsync(session.CurrentDirectory, cancellationToken);
                        if (parent is not null)
                            session.NavigateTo(parent);
                    }
                    catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                    {
                        WriteLine(labels.ListingError + ": " + ex.Message);
                    }
                    break;
                }
                case BrowserAction.Entry when choice.Entry is not null:
                    if (choice.Entry.IsDirectory)
                    {
                        if (choice.Entry.CanTraverse)
                            session.NavigateTo(choice.Entry.Location);
                        else
                            WriteLine(labels.ListingError + ": directory cannot be traversed.");
                    }
                    else if (mode == FileBrowserPickMode.File)
                        return new FileBrowserPickResult(choice.Entry.Location, false);
                    break;
                case BrowserAction.Search:
                    await SearchAsync(session, options, terminal, cancellationToken);
                    break;
                case BrowserAction.Tree:
                    var target = await TreeAsync(session.CurrentDirectory, options, terminal,
                        cancellationToken);
                    if (target is not null)
                        session.NavigateTo(target);
                    break;
                case BrowserAction.Filter:
                    filter = await FilterAsync(filter, options, cancellationToken);
                    break;
            }
        }
    }

    private async Task SearchAsync(
        FileBrowserNavigationSession session, SpectreFileBrowserOptions options,
        FileBrowserTerminalCapabilities terminal, CancellationToken token)
    {
        var labels = options.Labels;
        var mask = new MenuEntry(BrowserAction.Mask, labels.Masks);
        var regex = new MenuEntry(BrowserAction.Regex, labels.Regex);
        var back = new MenuEntry(BrowserAction.Back, labels.Back);
        var mode = await ChooseAsync(labels.Search, [mask, regex, back], back, options, token);
        if (mode.Action == BrowserAction.Back)
            return;

        var pattern = await AskAsync(labels.Pattern, token);
        if (string.IsNullOrWhiteSpace(pattern))
            return;

        FileBrowserSearchResult result;
        try
        {
            result = await session.SearchAsync(pattern, options.SearchOptions with
            {
                PatternMode = mode.Action == BrowserAction.Regex
                    ? FileBrowserPatternMode.RegularExpression : FileBrowserPatternMode.Wildcard
            }, token);
        }
        catch (Exception ex) when (ex is ArgumentException or RegexMatchTimeoutException)
        {
            WriteLine(ex.Message);
            return;
        }

        if (result.Status == FileBrowserSearchStatus.LimitReached)
            WriteLine(labels.Limited);
        if (result.Status == FileBrowserSearchStatus.Partial)
            WriteLine(labels.Partial);

        if (result.Matches.Count == 0)
        {
            WriteLine(labels.NothingFound);
            return;
        }

        var rows = FileBrowserSearchResultsPresentation.CreateRows(
            result, _icons, _source.IsCaseSensitive);
        if (rows.Count <= options.SearchTreePreviewLimit)
        {
            // Display a pruned, read-only tree first, then let the user choose among
            // ALL returned paths. A group label can never become a navigation target.
            WriteLine(labels.Matches + ": " + rows.Count);
            var matches = new Tree(new Text(labels.Matches))
                .Guide(terminal.SupportsUnicode ? TreeGuide.Line : TreeGuide.Ascii);
            AddSearchTreeNodes(matches,
                FileBrowserSearchResultsPresentation.CreateTree(result, _source.IsCaseSensitive),
                terminal, options.IconMode);
            _console.Write(matches);
        }

        var actions = rows.Select(row => new MenuEntry(
            BrowserAction.SearchMatch,
            FileBrowserFileIcons.GetIcon(row.Kind, options.IconMode, terminal) is { Length: > 0 } glyph
                ? glyph + " " + row.RelativePath : row.RelativePath,
            MatchIndex: row.MatchIndex)).ToList();
        var cancel = new MenuEntry(BrowserAction.Back, labels.Back);
        actions.Add(cancel);
        var choice = await ChooseAsync(labels.Matches, actions, cancel, options, token);
        if (choice.Action == BrowserAction.SearchMatch && choice.MatchIndex is int index)
            session.SelectSearchResult(result, index);
    }

    private async Task<FileBrowserLocation?> TreeAsync(
        FileBrowserLocation origin, SpectreFileBrowserOptions options,
        FileBrowserTerminalCapabilities terminal, CancellationToken token)
    {
        var labels = options.Labels;
        var tree = new FileBrowserDirectoryTree(_source);
        var current = origin;

        while (true)
        {
            token.ThrowIfCancellationRequested();
            WriteLine(labels.Tree + " — " + current.Value);
            IReadOnlyList<FileBrowserTreeNode> children;
            try
            {
                children = await tree.GetChildrenAsync(current, token);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                WriteLine(labels.ListingError + ": " + ex.Message);
                children = [];
            }

            // The actual Spectre tree is a noninteractive preview; the paged
            // selection prompt below provides keyboard navigation and expansion.
            var visual = new Tree(new Text(current.Value))
                .Guide(terminal.SupportsUnicode ? TreeGuide.Line : TreeGuide.Ascii);
            foreach (var child in children)
                visual.AddNode(new Text(child.Name + "/"));
            _console.Write(visual);

            var actions = children.Select(child => new MenuEntry(BrowserAction.TreeChild,
                FileBrowserEntryPresentation.Format(
                    child.Name, true, _icons, options.IconMode, terminal,
                    _knownFolders?.Classify(child.Location.Value, true)
                    ?? FileBrowserKnownFolderKind.None),
                FolderLocation: child.CanExpand ? child.Location : null)).ToList();

            actions.Add(new MenuEntry(BrowserAction.TreeUse, labels.UseFolder));
            actions.Add(new MenuEntry(BrowserAction.Parent, labels.Parent));
            var back = new MenuEntry(BrowserAction.Back, labels.Back);
            actions.Add(back);
            var choice = await ChooseAsync(labels.Tree, actions, back, options, token);
            switch (choice.Action)
            {
                case BrowserAction.Back:
                    return null;
                case BrowserAction.TreeUse:
                    return current;
                case BrowserAction.TreeChild when choice.FolderLocation is not null:
                    current = choice.FolderLocation;
                    break;
                case BrowserAction.Parent:
                    var parent = await _source.GetParentAsync(current, token);
                    if (parent is not null)
                        current = parent;
                    break;
            }
        }
    }

    private async Task<FileBrowserFileFilter?> FilterAsync(
        FileBrowserFileFilter? current, SpectreFileBrowserOptions options, CancellationToken token)
    {
        var l = options.Labels;
        var all = new MenuEntry(BrowserAction.All, l.All);
        var masks = new MenuEntry(BrowserAction.Mask, l.Masks);
        var regex = new MenuEntry(BrowserAction.Regex, l.Regex);
        var json = new MenuEntry(BrowserAction.Json, l.Json);
        var text = new MenuEntry(BrowserAction.Text, l.Text);
        var images = new MenuEntry(BrowserAction.Images, l.Images);
        var back = new MenuEntry(BrowserAction.Back, l.Back);
        var selection = await ChooseAsync(l.Filter,
            [all, masks, regex, json, text, images, back], back, options, token);

        switch (selection.Action)
        {
            case BrowserAction.All:
                return null;
            case BrowserAction.Json:
                return new FileBrowserFileFilter(includedKinds: [FileBrowserFileKind.Json]);
            case BrowserAction.Text:
                return new FileBrowserFileFilter(includedKinds: [FileBrowserFileKind.Text]);
            case BrowserAction.Images:
                return new FileBrowserFileFilter(includedKinds: [FileBrowserFileKind.Image]);
            case BrowserAction.Back:
                return current;
            case BrowserAction.Mask:
            case BrowserAction.Regex:
            {
                var pattern = await AskAsync(l.Pattern, token);
                if (string.IsNullOrWhiteSpace(pattern))
                    return current;
                try
                {
                    var matcher = new FileBrowserNameMatcher(pattern,
                        selection.Action == BrowserAction.Regex
                            ? FileBrowserPatternMode.RegularExpression
                            : FileBrowserPatternMode.Wildcard,
                        ignoreCase: !_source.IsCaseSensitive);
                    return new FileBrowserFileFilter(nameMatcher: matcher);
                }
                catch (ArgumentException ex)
                {
                    WriteLine(ex.Message);
                    return current;
                }
            }
            default:
                return current;
        }
    }

    private async Task<MenuEntry> ChooseAsync(
        string title, IReadOnlyList<MenuEntry> actions, MenuEntry cancel,
        SpectreFileBrowserOptions options, CancellationToken token)
    {
        var prompt = new SelectionPrompt<MenuEntry>()
            .Title(Markup.Escape(title))
            .PageSize(options.PageSize)
            .MoreChoicesText(Markup.Escape("Use arrows to browse more choices"))
            .UseConverter(a => Markup.Escape(a.Label))
            .AddChoices(actions)
            .AddCancelResult(cancel);
        return await prompt.ShowAsync(_console, token);
    }

    private Task<string> AskAsync(string label, CancellationToken token)
        => new TextPrompt<string>(Markup.Escape(label) + ": ")
            .AllowEmpty().ShowAsync(_console, token);

    private void WriteLine(string value)
        => _console.Write(new Text(value + Environment.NewLine));

    private static void AddSearchTreeNodes(
        Tree root, IReadOnlyList<FileBrowserSearchTreeNode> nodes,
        FileBrowserTerminalCapabilities terminal, FileBrowserIconMode mode)
    {
        foreach (var node in nodes)
        {
            var kind = node.IsDirectory ? FileBrowserFileKind.Directory : FileBrowserFileKind.Unknown;
            var glyph = FileBrowserFileIcons.GetIcon(kind, mode, terminal);
            var label = (glyph.Length > 0 ? glyph + " " : "") +
                node.Name + (node.IsDirectory ? "/" : "");
            var child = root.AddNode(new Text(label));
            AddSearchChildNodes(child, node.Children, terminal, mode);
        }
    }

    private static void AddSearchChildNodes(
        TreeNode parent, IReadOnlyList<FileBrowserSearchTreeNode> nodes,
        FileBrowserTerminalCapabilities terminal, FileBrowserIconMode mode)
    {
        foreach (var node in nodes)
        {
            var kind = node.IsDirectory ? FileBrowserFileKind.Directory : FileBrowserFileKind.Unknown;
            var glyph = FileBrowserFileIcons.GetIcon(kind, mode, terminal);
            var label = (glyph.Length > 0 ? glyph + " " : "") +
                node.Name + (node.IsDirectory ? "/" : "");
            var child = parent.AddNode(new Text(label));
            AddSearchChildNodes(child, node.Children, terminal, mode);
        }
    }

    private enum BrowserAction
    {
        Entry, Parent, SelectFolder, Search, Tree, Filter, Back,
        Mask, Regex, All, Json, Text, Images, SearchMatch, TreeChild, TreeUse
    }

    private sealed record MenuEntry(
        BrowserAction Action, string Label, FileBrowserSourceEntry? Entry = null,
        int? MatchIndex = null, FileBrowserLocation? FolderLocation = null);
}
