using Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser;

var mode = args.Contains("--directory", StringComparer.OrdinalIgnoreCase)
    ? FileBrowserPickMode.Directory : FileBrowserPickMode.File;

var icons = args.Contains("--icons-on", StringComparer.OrdinalIgnoreCase)
    ? FileBrowserIconMode.On
    : args.Contains("--icons-off", StringComparer.OrdinalIgnoreCase)
        ? FileBrowserIconMode.Off : FileBrowserIconMode.Auto;

var maskIndex = Array.FindIndex(args, a =>
    string.Equals(a, "--filter", StringComparison.OrdinalIgnoreCase));
var regexIndex = Array.FindIndex(args, a =>
    string.Equals(a, "--regex", StringComparison.OrdinalIgnoreCase));

if ((maskIndex >= 0 && maskIndex == args.Length - 1) ||
    (regexIndex >= 0 && regexIndex == args.Length - 1) ||
    (maskIndex >= 0 && regexIndex >= 0))
{
    Console.Error.WriteLine("Use --filter 'semicolon;separated;masks' OR --regex 'expression'.");
    return 2;
}

FileBrowserFileFilter? initialFilter;
try
{
    initialFilter = maskIndex >= 0
        ? new FileBrowserFileFilter(nameMatcher: new FileBrowserNameMatcher(args[maskIndex + 1]))
        : regexIndex >= 0
            ? new FileBrowserFileFilter(nameMatcher: new FileBrowserNameMatcher(
                args[regexIndex + 1], FileBrowserPatternMode.RegularExpression))
            : null;
}
catch (ArgumentException ex)
{
    Console.Error.WriteLine(ex.Message);
    return 2;
}

var path = args.FirstOrDefault(a => !a.StartsWith("--", StringComparison.Ordinal) &&
    (maskIndex < 0 || args[maskIndex + 1] != a) &&
    (regexIndex < 0 || args[regexIndex + 1] != a));
path = Path.GetFullPath(path ?? Environment.CurrentDirectory);
if (!Directory.Exists(path))
{
    Console.Error.WriteLine("Starting directory does not exist: " + path);
    return 2;
}

using var stop = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; stop.Cancel(); };

try
{
    var browser = new SpectreFileBrowser(new FileBrowserLocalSource());
    var selected = await browser.PickAsync(
        new FileBrowserLocation("local", path),
        mode,
        new SpectreFileBrowserOptions
        {
            IconMode = icons,
            EmojiConfirmed = icons == FileBrowserIconMode.On,
            FileFilter = initialFilter
        },
        stop.Token);

    if (selected.IsSelected)
    {
        Console.WriteLine("Selected: " + selected.Location!.Value);
        return 0;
    }

    Console.WriteLine("Cancelled; no filesystem changes were made.");
    return 1;
}
catch (OperationCanceledException)
{
    Console.WriteLine("Cancelled.");
    return 1;
}
catch (InvalidOperationException ex)
{
    Console.Error.WriteLine(ex.Message);
    return 2;
}
