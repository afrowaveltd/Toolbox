using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Views;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'list-code-groups' command: lists code groups from a validated WhenItFails workspace.
/// </summary>
internal static class ListCodeGroupsCommand
{
    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            CommandInputError.Show(
                code: "MissingListCodeGroupsPath",
                message: "The list-code-groups command requires a project root or Jsons/WhenItFails directory path.",
                path: "list-code-groups <path> [--plain]");
            return 1;
        }

        if (!TryParseOptions(args, out bool usePlainOutput))
        {
            CommandInputError.Show(
                code: "InvalidListCodeGroupsArguments",
                message: "The list-code-groups command accepts only a path and the optional --plain switch.",
                path: "list-code-groups <path> [--plain]");
            return 1;
        }

        WorkspaceCommandContext? context =
            await WorkspaceCommandContextLoader.TryLoadAsync(args[1]);

        if (context is null)
        {
            return 2;
        }

        if (usePlainOutput)
        {
            CodeGroupsView.ShowPlain(context.Summary);
        }
        else
        {
            CodeGroupsView.Show(context.Summary);
        }

        return 0;
    }

    public static bool TryParseOptions(string[] args, out bool usePlainOutput)
    {
        return PlainOutputOptionParser.TryParse(args, optionStartIndex: 2, out usePlainOutput);
    }
}
