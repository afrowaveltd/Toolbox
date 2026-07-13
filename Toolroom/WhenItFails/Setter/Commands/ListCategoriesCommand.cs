using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Views;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'list-categories' command: lists categories from a validated WhenItFails workspace.
/// </summary>
internal static class ListCategoriesCommand
{
    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            CommandInputError.Show(
                code: "MissingListCategoriesPath",
                message: "The list-categories command requires a project root or Jsons/WhenItFails directory path.",
                path: "list-categories <path> [--plain]");
            return 1;
        }

        if (!TryParseOptions(args, out bool usePlainOutput))
        {
            CommandInputError.Show(
                code: "InvalidListCategoriesArguments",
                message: "The list-categories command accepts only a path and the optional --plain switch.",
                path: "list-categories <path> [--plain]");
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
            CategoriesView.ShowPlain(context.Summary);
        }
        else
        {
            CategoriesView.Show(context.Summary);
        }

        return 0;
    }

    public static bool TryParseOptions(string[] args, out bool usePlainOutput)
    {
        return PlainOutputOptionParser.TryParse(args, optionStartIndex: 2, out usePlainOutput);
    }
}
