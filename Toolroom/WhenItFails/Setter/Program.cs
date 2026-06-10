using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

if (args.Length == 0 || IsHelpCommand(args[0]))
{
   ShowHelp();

   return 0;
}

string command = args[0].Trim().ToLowerInvariant();

try
{
   if (command == "demo")
   {
      ShowDemoValidationResult();

      return 0;
   }

   if (command == "validate")
   {
      return await ValidateAsync(args);
   }

   AnsiConsole.MarkupLine(
      "[red]Unknown command:[/] {0}",
      Markup.Escape(args[0]));

   ShowHelp();

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

static void ShowHelp()
{
   Grid commandGrid = new Grid();
   commandGrid.AddColumn(new GridColumn().NoWrap());
   commandGrid.AddColumn();

   commandGrid.AddRow("[green]demo[/]", "Show a sample WhenItFails validation result.");
   commandGrid.AddRow("[green]validate[/] [grey]<path>[/]", "Validate WhenItFails JSON files.");
   commandGrid.AddRow("[green]help[/]", "Show this help screen.");

   Panel helpPanel = new Panel(commandGrid)
      .Header("[bold aqua]WhenItFails Setter[/]")
      .Border(BoxBorder.Rounded)
      .BorderColor(Color.Aqua);

   AnsiConsole.Write(helpPanel);
}

static async Task<int> ValidateAsync(string[] args)
{
   if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
   {
      ErrorCatalogValidationResult missingPathResult = new();

      missingPathResult.AddError(
         code: "MissingValidatePath",
         message: "The validate command requires a project root or Jsons/WhenItFails directory path.",
         path: "validate <path>");

      new ConsoleValidationResultShow().Show(
         missingPathResult,
         new ConsoleShowOptions
         {
            SourcePath = "command line"
         });

      return 1;
   }

   string inputPath = args[1];

   WhenItFailsWorkspaceValidator validator = new();

   WhenItFailsWorkspaceValidationOutcome outcome =
      await validator.ValidateAsync(inputPath);

   new ConsoleValidationResultShow().Show(
      outcome.ValidationResult,
      new ConsoleShowOptions
      {
         SourcePath = outcome.PackageDirectoryPath
      });

   return outcome.ValidationResult.IsValid
      ? 0
      : 2;
}

static void ShowDemoValidationResult()
{
   ErrorCatalogValidationResult validationResult = new();

   validationResult.AddError(
      code: "MissingCatalogId",
      message: "Catalog id is missing.",
      path: "catalogId");

   validationResult.AddWarning(
      code: "UnknownProfileIncludeOwner",
      message: "Profile references an unknown owner.",
      path: "profiles[0].includeOwners[0]");

   validationResult.AddInformation(
      code: "PrimaryCategoryNotListed",
      message: "Primary category is not listed in additional categories. This is allowed, but listing it can make filtering easier.",
      errorId: "CFG-0001",
      errorName: "MissingConfigurationValue",
      path: "errors[0].primaryCategory");

   ConsoleValidationResultShow validationResultShow = new();

   validationResultShow.Show(
      validationResult,
      new ConsoleShowOptions
      {
         SourcePath = "Jsons/WhenItFails/errors.json"
      });
}
