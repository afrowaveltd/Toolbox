using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Views;
using Spectre.Console;

if (args.Length == 0 || IsHelpCommand(args[0]))
{
    HelpView.Show();

    return 0;
}

string command = args[0].Trim().ToLowerInvariant();

try
{
    if (command == "demo")
    {
        return DemoCommand.Execute();
    }

    if (command == "init")
    {
        return await InitCommand.ExecuteAsync(args);
    }

    if (command == "validate")
    {
        return await ValidateCommand.ExecuteAsync(args);
    }

    if (command is "summary" or "inspect")
    {
        return await SummaryCommand.ExecuteAsync(args);
    }

    if (command == "errors")
    {
        return await ErrorsCommand.ExecuteAsync(args);
    }

    if (command == "list-profiles")
    {
        return await ListProfilesCommand.ExecuteAsync(args);
    }

    if (command == "show-profile")
    {
        return await ShowProfileCommand.ExecuteAsync(args);
    }

    if (command == "list-categories")
    {
        return await ListCategoriesCommand.ExecuteAsync(args);
    }

    if (command == "show-category")
    {
        return await ShowCategoryCommand.ExecuteAsync(args);
    }

    if (command == "list-code-groups")
    {
        return await ListCodeGroupsCommand.ExecuteAsync(args);
    }

    if (command == "list-owners")
    {
        return await ListOwnersCommand.ExecuteAsync(args);
    }

    if (command is "details" or "detail")
    {
        return await DetailsCommand.ExecuteAsync(args);
    }

    if (command == "set-title")
    {
        return await SetTitleCommand.ExecuteAsync(args);
    }

    if (command == "set-message")
    {
        return await SetMessageCommand.ExecuteAsync(args);
    }

    if (command == "set-developer-hint")
    {
        return await SetDeveloperHintCommand.ExecuteAsync(args);
    }

    if (command == "set-severity")
    {
        return await SetSeverityCommand.ExecuteAsync(args);
    }

    if (command == "set-documentation-key")
    {
        return await SetDocumentationKeyCommand.ExecuteAsync(args);
    }

    AnsiConsole.MarkupLine(
        "[red]Unknown command:[/] {0}",
        Markup.Escape(args[0]));

    HelpView.Show();

    return 1;
}
catch (Exception exception)
{
    AnsiConsole.WriteException(
        exception,
        ExceptionFormats.ShortenEverything | ExceptionFormats.ShowLinks);

    return 3;
}

static bool IsHelpCommand(string command)
{
    return command is "-h" or "--help" or "help";
}
