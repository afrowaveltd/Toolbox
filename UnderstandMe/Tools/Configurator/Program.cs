using Afrowave.Toolbox.UnderstandMe.Configurator;

var validate = args.Length > 0 && args[0].Equals("validate", StringComparison.OrdinalIgnoreCase);
var rest = validate ? args.Skip(1).ToArray() : args;
if (rest.Length > 0 && rest[0].Equals("config", StringComparison.OrdinalIgnoreCase))
    rest = rest.Skip(1).ToArray();

string? path = null;
if (rest.Length == 2 && rest[0].Equals("--config", StringComparison.OrdinalIgnoreCase))
    path = rest[1];
else if (rest.Length != 0)
{
    Console.Error.WriteLine("Usage: Configurator [config] [--config <path>] | validate [--config <path>]");
    return 2;
}

if (validate)
{
    path ??= Path.Combine(Environment.CurrentDirectory, "UnderstandMeSettings.json");
    try
    {
        var issues = ConfiguratorApp.ValidateFile(path);
        foreach (var issue in issues)
            Console.Error.WriteLine(issue.Path + ": " + issue.Message);
        if (issues.Count == 0)
            Console.WriteLine("Settings valid: " + Path.GetFullPath(path));
        return issues.Count == 0 ? 0 : 1;
    }
    catch (Exception e) when (e is IOException or UnauthorizedAccessException or ArgumentException)
    {
        Console.Error.WriteLine("Cannot validate settings: " + e.Message);
        return 1;
    }
}

using var stop = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; stop.Cancel(); };
try
{
    await new ConfiguratorApp().RunAsync(path, stop.Token);
    return 0;
}
catch (OperationCanceledException)
{
    Console.WriteLine("Cancelled; no implicit save performed.");
    return 1;
}
catch (InvalidOperationException e)
{
    Console.Error.WriteLine(e.Message);
    return 2;
}
