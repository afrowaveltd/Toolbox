using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.Cache;
using Spectre.Console;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>Read-only, cancellable selection of a public GitHub repository README.</summary>
public sealed class MdViewGitHubRepositoryPicker
{
    private readonly IAnsiConsole _console;
    private readonly GitHubRepositoryCatalogSource _catalog;

    /// <summary>Constructs a terminal selector with an injected catalog source.</summary>
    public MdViewGitHubRepositoryPicker(IAnsiConsole console, GitHubRepositoryCatalogSource catalog)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
    }

    /// <summary>
    /// Lists repositories for an owner and returns an explicit README command,
    /// or null when the user presses Escape. Never fetches any README itself.
    /// </summary>
    public async Task<MdViewCommand?> PickAsync(string owner, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(owner);
        if (!_console.Profile.Capabilities.Interactive)
            throw new InvalidOperationException(
                "Repository selection requires an interactive terminal; use 'git OWNER REPOSITORY' instead.");

        var result = await _catalog.ListAsync(owner, token);
        if (result.State == GitHubRepositoryCatalogState.Error)
        {
            _console.Write(new Text("GitHub repository catalog is unavailable." +
                Environment.NewLine));
            if (result.Warning is not null)
                _console.Write(new Text(result.Warning + Environment.NewLine));
            _console.Write(new Text(
                "Enter a public repository name directly to skip the catalog, " +
                "or leave it empty to go back." + Environment.NewLine));
            return await PickManualRepositoryAsync(owner, token).ConfigureAwait(false);
        }

        _console.Write(new Text("GitHub — public repositories of " + owner +
            Environment.NewLine));
        if (result.State == GitHubRepositoryCatalogState.Stale)
            _console.Write(new Text("Offline: using the last verified list (may be outdated)." +
                Environment.NewLine));
        else if (result.State == GitHubRepositoryCatalogState.FreshCache)
            _console.Write(new Text("Using locally cached repository list." + Environment.NewLine));
        if (result.Warning is not null)
            _console.Write(new Text(result.Warning + Environment.NewLine));

        if (result.Repositories.Count == 0)
        {
            _console.Write(new Text("No public repositories were found in the catalog." +
                Environment.NewLine));
            return await PickManualRepositoryAsync(owner, token).ConfigureAwait(false);
        }

        // A short tree is useful for orientation, but a 1,000-line tree would
        // overwhelm the terminal; the paged selection prompt handles large lists.
        if (result.Repositories.Count <= 20)
        {
            var tree = new Tree(new Text(owner));
            foreach (var repository in result.Repositories)
                tree.AddNode(new Text(repository.Name));
            _console.Write(tree);
        }

        var filter = "";
        while (true)
        {
            token.ThrowIfCancellationRequested();
            var shown = result.Repositories.Where(repo =>
                filter.Length == 0 || repo.Name.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                (repo.Description?.Contains(filter, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToArray();

            if (shown.Length == 0)
                _console.Write(new Text("No matches. Choose Filter to change the search." +
                    Environment.NewLine));

            var choices = shown.Select(repo =>
                new Entry(repo.Name, Format(repo), EntryAction.Repository)).ToList();
            var filterChoice = new Entry(null, "Filter / search by name or description", EntryAction.Filter);
            var backChoice = new Entry(null, "Back / cancel", EntryAction.Back);
            choices.Add(filterChoice);
            choices.Add(backChoice);
            var picked = await new SelectionPrompt<Entry>()
                .Title("Select a repository README from " + Markup.Escape(owner) +
                    (filter.Length == 0 ? "" : " (filter: " + Markup.Escape(filter) + ")"))
                .PageSize(Math.Clamp(choices.Count, 3, 15))
                .UseConverter(x => Markup.Escape(x.Label))
                .AddChoices(choices)
                .AddCancelResult(backChoice)
                .ShowAsync(_console, token);

            if (picked.Action == EntryAction.Back)
                return null;
            if (picked.Action == EntryAction.Repository)
                return new MdViewCommand(MdViewSourceKind.GitHub, owner, picked.Name!);

            var input = await new TextPrompt<string>("Filter text (empty = show all): ")
                .AllowEmpty().ShowAsync(_console, token);
            filter = input.Trim();
        }
    }

    private async Task<MdViewCommand?> PickManualRepositoryAsync(
        string owner, CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var input = await new TextPrompt<string>(
                "GitHub repository name (empty = back): ")
                .AllowEmpty()
                .ShowAsync(_console, cancellationToken).ConfigureAwait(false);
            var name = input.Trim();
            if (name.Length == 0)
                return null;

            // Match the repository-name constraints of GitHubReadmeSource.
            // Never treat the typed value as a verified catalog entry.
            if (name.Length > 100 || name is "." or ".." ||
                !name.All(c => char.IsAsciiLetterOrDigit(c) || c is '_' or '-' or '.'))
            {
                _console.Write(new Text(
                    "Invalid repository name. Use 1–100 ASCII letters, digits, _, - or ." +
                    Environment.NewLine));
                continue;
            }

            return new MdViewCommand(MdViewSourceKind.GitHub, owner, name);
        }
    }

    private static string Format(GitHubRepositoryInfo repo)
    {
        var description = repo.Description is null ? "" : " — " + repo.Description;
        return repo.Name + (repo.Archived ? " [archived]" : "") +
            (repo.Fork ? " [fork]" : "") + description;
    }

    private enum EntryAction { Repository, Filter, Back }
    private sealed record Entry(string? Name, string Label, EntryAction Action);
}
