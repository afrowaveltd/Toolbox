using Afrowave.Toolbox.Blackboard.Showcase;

string root;
try
{
    root = Path.GetFullPath(args.Length == 0 ? Environment.CurrentDirectory : args[0]);
}
catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
{
    Console.Error.WriteLine("Invalid starting directory: " + ex.Message);
    return 2;
}

if (args.Length > 1 || !Directory.Exists(root))
{
    Console.Error.WriteLine("Usage: Blackboard.Showcase [existing-starting-directory]");
    return 2;
}

using var cancel = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancel.Cancel();
};

// Additional Blackboard tools register their own IBlackboardShowcaseDemo here.
// No reflection, implicit dependencies, or loading untrusted modules is required.
IBlackboardShowcaseDemo[] modules =
[
    new FileBrowserPreviewDemo()
];

try
{
    await new BlackboardShowcaseHost(modules).RunAsync(root, cancel.Token);
    return 0;
}
catch (OperationCanceledException)
{
    Console.WriteLine("Showcase cancelled.");
    return 1;
}
catch (InvalidOperationException ex)
{
    Console.Error.WriteLine(ex.Message);
    return 2;
}
