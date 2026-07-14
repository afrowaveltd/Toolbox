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

    if (command == "add-profile")
    {
        return await AddProfileCommand.ExecuteAsync(args);
    }

    if (command == "remove-profile")
    {
        return await RemoveProfileCommand.ExecuteAsync(args);
    }

    if (command == "set-profile-display-name")
    {
        return await SetProfileDisplayNameCommand.ExecuteAsync(args);
    }

    if (command == "set-profile-description")
    {
        return await SetProfileDescriptionCommand.ExecuteAsync(args);
    }

    if (command == "profile-add-owner")
    {
        return await ProfileAddOwnerCommand.ExecuteAsync(args);
    }

    if (command == "profile-remove-owner")
    {
        return await ProfileRemoveOwnerCommand.ExecuteAsync(args);
    }

    if (command == "profile-add-category")
    {
        return await ProfileAddCategoryCommand.ExecuteAsync(args);
    }

    if (command == "profile-remove-category")
    {
        return await ProfileRemoveCategoryCommand.ExecuteAsync(args);
    }

    if (command == "profile-add-code-group")
    {
        return await ProfileAddCodeGroupCommand.ExecuteAsync(args);
    }

    if (command == "profile-remove-code-group")
    {
        return await ProfileRemoveCodeGroupCommand.ExecuteAsync(args);
    }

    if (command == "profile-add-subcategory")
    {
        return await ProfileAddSubcategoryCommand.ExecuteAsync(args);
    }

    if (command == "profile-remove-subcategory")
    {
        return await ProfileRemoveSubcategoryCommand.ExecuteAsync(args);
    }

    if (command == "profile-add-tag")
    {
        return await ProfileAddTagCommand.ExecuteAsync(args);
    }

    if (command == "profile-remove-tag")
    {
        return await ProfileRemoveTagCommand.ExecuteAsync(args);
    }

    if (command == "profile-add-excluded-tag")
    {
        return await ProfileAddExcludedTagCommand.ExecuteAsync(args);
    }

    if (command == "profile-remove-excluded-tag")
    {
        return await ProfileRemoveExcludedTagCommand.ExecuteAsync(args);
    }

    if (command == "profile-add-error")
    {
        return await ProfileAddErrorCommand.ExecuteAsync(args);
    }

    if (command == "profile-remove-error")
    {
        return await ProfileRemoveErrorCommand.ExecuteAsync(args);
    }

    if (command == "profile-add-excluded-error")
    {
        return await ProfileAddExcludedErrorCommand.ExecuteAsync(args);
    }

    if (command == "profile-remove-excluded-error")
    {
        return await ProfileRemoveExcludedErrorCommand.ExecuteAsync(args);
    }

    if (command == "profile-set-default-mapping")
    {
        return await ProfileSetDefaultMappingCommand.ExecuteAsync(args);
    }

    if (command == "profile-remove-default-mapping")
    {
        return await ProfileRemoveDefaultMappingCommand.ExecuteAsync(args);
    }

    if (command == "profile-set-metadata")
    {
        return await ProfileSetMetadataCommand.ExecuteAsync(args);
    }

    if (command == "profile-remove-metadata")
    {
        return await ProfileRemoveMetadataCommand.ExecuteAsync(args);
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

    if (command == "show-code-group")
    {
        return await ShowCodeGroupCommand.ExecuteAsync(args);
    }

    if (command == "list-owners")
    {
        return await ListOwnersCommand.ExecuteAsync(args);
    }

    if (command == "show-owner")
    {
        return await ShowOwnerCommand.ExecuteAsync(args);
    }

    if (command is "details" or "detail")
    {
        return await DetailsCommand.ExecuteAsync(args);
    }

    if (command == "error-add-tag")
    {
        return await ErrorAddTagCommand.ExecuteAsync(args);
    }

    if (command == "error-remove-tag")
    {
        return await ErrorRemoveTagCommand.ExecuteAsync(args);
    }

    if (command == "error-add-category")
    {
        return await ErrorAddCategoryCommand.ExecuteAsync(args);
    }

    if (command == "error-remove-category")
    {
        return await ErrorRemoveCategoryCommand.ExecuteAsync(args);
    }

    if (command == "error-add-subcategory")
    {
        return await ErrorAddSubcategoryCommand.ExecuteAsync(args);
    }

    if (command == "error-remove-subcategory")
    {
        return await ErrorRemoveSubcategoryCommand.ExecuteAsync(args);
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
