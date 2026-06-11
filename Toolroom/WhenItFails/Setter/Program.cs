using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Definitions;
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

   if (command == "init")
   {
      return await InitializeAsync(args);
   }

   if (command == "validate")
   {
      return await ValidateAsync(args);
   }

   if (command is "summary" or "inspect")
   {
      return await SummaryAsync(args);
   }

   if (command == "errors")
   {
      return await ErrorsAsync(args);
   }

   if (command is "details" or "detail")
   {
      return await DetailsAsync(args);
   }

   if (command == "set-title")
   {
      return await SetTitleAsync(args);
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

   commandGrid.AddRow("[green]init[/] [grey]<path>[/]", "Create missing WhenItFails JSON files.");
   commandGrid.AddRow("[green]validate[/] [grey]<path>[/]", "Validate WhenItFails JSON files.");
   commandGrid.AddRow("[green]summary[/] [grey]<path>[/]", "Show a read-only summary of a WhenItFails JSON workspace.");
   commandGrid.AddRow("[green]inspect[/] [grey]<path>[/]", "Alias for summary.");
   commandGrid.AddRow(
      "[green]errors[/] [grey]<path>[/] [grey][--plain][/]",
      "List error definitions. Supports --owner, --group, --category, --severity, --profile, --search.");
   commandGrid.AddRow(
      "[green]details[/] [grey]<path>[/] [grey]<id|code|name>[/] [grey][--plain][/]",
      "Show one error definition in detail.");
   commandGrid.AddRow("[green]detail[/] [grey]<path>[/] [grey]<id|code|name>[/]", "Alias for details.");
   commandGrid.AddRow("[green]demo[/]", "Show a sample WhenItFails validation result.");
   commandGrid.AddRow("[green]help[/]", "Show this help screen.");

   Panel helpPanel = new Panel(commandGrid)
      .Header("[bold aqua]WhenItFails Setter[/]")
      .Border(BoxBorder.Rounded)
      .BorderColor(Color.Aqua);

   AnsiConsole.Write(helpPanel);
}

static async Task<int> InitializeAsync(string[] args)
{
   if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
   {
      ErrorCatalogValidationResult missingPathResult = new();

      missingPathResult.AddError(
         code: "MissingInitPath",
         message: "The init command requires a project root path.",
         path: "init <path>");

      new ConsoleValidationResultShow().Show(
         missingPathResult,
         new ConsoleShowOptions
         {
            SourcePath = "command line"
         });

      return 1;
   }

   string projectRootPath = args[1];

   WhenItFailsWorkspaceInitializer initializer = new();

   Response<JsonsBootstrapPayload> response =
      await initializer.InitializeAsync(projectRootPath);

   if (!response.IsSuccess || response.Data is null)
   {
      ErrorCatalogValidationResult failureResult = new();

      string failureCode = response.Issues.Count > 0
         ? response.Issues[0].Code
         : "WorkspaceInitializationFailed";

      string failureMessage = string.IsNullOrWhiteSpace(response.Message)
         ? "WhenItFails workspace initialization failed."
         : response.Message;

      failureResult.AddError(
         code: failureCode,
         message: failureMessage,
         path: projectRootPath);

      new ConsoleValidationResultShow().Show(
         failureResult,
         new ConsoleShowOptions
         {
            SourcePath = projectRootPath
         });

      return 3;
   }

   ShowBootstrapResult(response.Data);

   return 0;
}

static void ShowBootstrapResult(JsonsBootstrapPayload payload)
{
   string directoryStatus = payload.PackageDirectoryCreated
      ? "[green]created[/]"
      : "[grey]already existed[/]";

   AnsiConsole.MarkupLine(
      "[bold aqua]WhenItFails JSON workspace:[/] {0} ({1})",
      Markup.Escape(payload.PackageDirectoryPath),
      directoryStatus);

   Table table = new Table();

   ConfigureWideTable(
      table,
      Color.Aqua);

   table.AddColumn("File");
   table.AddColumn("Status");
   table.AddColumn("Path");
   table.AddColumn("Message");

   foreach (JsonsBootstrapFileResult fileResult in payload.Files)
   {
      string status = fileResult.Created
         ? "[green]Created[/]"
         : "[yellow]Skipped[/]";

      string displayPath = Path.GetRelativePath(
         payload.PackageDirectoryPath,
         fileResult.TargetFilePath);

      table.AddRow(
         Markup.Escape(fileResult.Name),
         status,
         Markup.Escape(displayPath),
         Markup.Escape(fileResult.Message ?? string.Empty));
   }

   AnsiConsole.Write(table);
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
         SourcePath = outcome.DisplayPath
      });

   return outcome.ValidationResult.IsValid
      ? 0
      : 2;
}

static async Task<int> SummaryAsync(string[] args)
{
   if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
   {
      ErrorCatalogValidationResult missingPathResult = new();

      missingPathResult.AddError(
         code: "MissingSummaryPath",
         message: "The summary command requires a project root or Jsons/WhenItFails directory path.",
         path: "summary <path>");

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

   WhenItFailsWorkspaceValidationOutcome validationOutcome =
      await validator.ValidateAsync(inputPath);

   if (!validationOutcome.ValidationResult.IsValid)
   {
      new ConsoleValidationResultShow().Show(
         validationOutcome.ValidationResult,
         new ConsoleShowOptions
         {
            SourcePath = validationOutcome.DisplayPath
         });

      return 2;
   }

   WhenItFailsWorkspaceSummarizer summarizer = new();

   WhenItFailsWorkspaceSummary summary =
      await summarizer.LoadAsync(inputPath);

   ShowSummary(summary);

   return 0;
}

static async Task<int> ErrorsAsync(string[] args)
{
   if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
   {
      ErrorCatalogValidationResult missingPathResult = new();

      missingPathResult.AddError(
         code: "MissingErrorsPath",
         message: "The errors command requires a project root or Jsons/WhenItFails directory path.",
         path: "errors <path>");

      new ConsoleValidationResultShow().Show(
         missingPathResult,
         new ConsoleShowOptions
         {
            SourcePath = "command line"
         });

      return 1;
   }

   string inputPath = args[1];
   ErrorListOptions errorListOptions = ParseErrorListOptions(args);

   WhenItFailsWorkspaceValidator validator = new();

   WhenItFailsWorkspaceValidationOutcome validationOutcome =
      await validator.ValidateAsync(inputPath);

   if (!validationOutcome.ValidationResult.IsValid)
   {
      new ConsoleValidationResultShow().Show(
         validationOutcome.ValidationResult,
         new ConsoleShowOptions
         {
            SourcePath = validationOutcome.DisplayPath
         });

      return 2;
   }

   WhenItFailsWorkspaceSummarizer summarizer = new();

   WhenItFailsWorkspaceSummary summary =
      await summarizer.LoadAsync(inputPath);

   if (!string.IsNullOrWhiteSpace(errorListOptions.Profile)
       && FindProfile(summary, errorListOptions.Profile) is null)
   {
      ErrorCatalogValidationResult unknownProfileResult = new();

      unknownProfileResult.AddError(
         code: "UnknownProfileFilter",
         message: $"The selected profile '{errorListOptions.Profile}' does not exist.",
         path: "--profile");

      new ConsoleValidationResultShow().Show(
         unknownProfileResult,
         new ConsoleShowOptions
         {
            SourcePath = summary.DisplayPath
         });

      return 1;
   }

   IReadOnlyList<ErrorDefinition> filteredErrors =
      ApplyErrorFilters(summary, errorListOptions).ToList();

   if (errorListOptions.UsePlainOutput)
   {
      ShowErrorsPlain(
         summary,
         filteredErrors,
         errorListOptions);
   }
   else
   {
      ShowErrors(
         summary,
         filteredErrors,
         errorListOptions);
   }

   return 0;
}

static async Task<int> DetailsAsync(string[] args)
{
   if (args.Length < 3
       || string.IsNullOrWhiteSpace(args[1])
       || string.IsNullOrWhiteSpace(args[2]))
   {
      ErrorCatalogValidationResult missingDetailsArgumentsResult = new();

      missingDetailsArgumentsResult.AddError(
         code: "MissingDetailsArguments",
         message: "The details command requires a project root or Jsons/WhenItFails directory path and an error id, code or name.",
         path: "details <path> <id|code|name>");

      new ConsoleValidationResultShow().Show(
         missingDetailsArgumentsResult,
         new ConsoleShowOptions
         {
            SourcePath = "command line"
         });

      return 1;
   }

   string inputPath = args[1];
   string lookupValue = args[2];
   bool usePlainOutput = HasSwitch(args, "--plain");

   WhenItFailsWorkspaceValidator validator = new();

   WhenItFailsWorkspaceValidationOutcome validationOutcome =
      await validator.ValidateAsync(inputPath);

   if (!validationOutcome.ValidationResult.IsValid)
   {
      new ConsoleValidationResultShow().Show(
         validationOutcome.ValidationResult,
         new ConsoleShowOptions
         {
            SourcePath = validationOutcome.DisplayPath
         });

      return 2;
   }

   WhenItFailsWorkspaceSummarizer summarizer = new();

   WhenItFailsWorkspaceSummary summary =
      await summarizer.LoadAsync(inputPath);

   ErrorDefinition? errorDefinition = FindErrorDefinition(
      summary,
      lookupValue);

   if (errorDefinition is null)
   {
      ErrorCatalogValidationResult notFoundResult = new();

      notFoundResult.AddError(
         code: "ErrorDefinitionNotFound",
         message: $"No error definition was found for '{lookupValue}'. Search by Id, Code or Name.",
         path: lookupValue);

      new ConsoleValidationResultShow().Show(
         notFoundResult,
         new ConsoleShowOptions
         {
            SourcePath = summary.DisplayPath
         });

      return 1;
   }

   if (usePlainOutput)
   {
      ShowDetailsPlain(
         summary,
         errorDefinition);
   }
   else
   {
      ShowDetails(
         summary,
         errorDefinition);
   }

   return 0;
}

static async Task<int> SetTitleAsync(string[] args)
{
   if (args.Length < 4
       || string.IsNullOrWhiteSpace(args[1])
       || string.IsNullOrWhiteSpace(args[2])
       || string.IsNullOrWhiteSpace(args[3]))
   {
      ErrorCatalogValidationResult missingSetTitleArgumentsResult = new();

      missingSetTitleArgumentsResult.AddError(
         code: "MissingSetTitleArguments",
         message: "The set-title command requires a project root or Jsons/WhenItFails directory path, an error id/code/name, and a new title.",
         path: "set-title <path> <id|code|name> <title>");

      new ConsoleValidationResultShow().Show(
         missingSetTitleArgumentsResult,
         new ConsoleShowOptions
         {
            SourcePath = "command line"
         });

      return 1;
   }

   string inputPath = args[1];
   string lookupValue = args[2];

   string newTitle = string.Join(
      " ",
      args.Skip(3));

   WhenItFailsWorkspaceEditor editor = new();

   Response<ErrorDefinition> response =
      await editor.SetErrorTitleAsync(
         inputPath,
         lookupValue,
         newTitle);

   if (!response.IsSuccess || response.Data is null)
   {
      ErrorCatalogValidationResult editFailureResult = new();

      string failureCode = response.Issues.Count > 0
         ? response.Issues[0].Code
         : "SetTitleFailed";

      string failureMessage = string.IsNullOrWhiteSpace(response.Message)
         ? "Error title could not be changed."
         : response.Message;

      editFailureResult.AddError(
         code: failureCode,
         message: failureMessage,
         path: lookupValue);

      new ConsoleValidationResultShow().Show(
         editFailureResult,
         new ConsoleShowOptions
         {
            SourcePath = inputPath
         });

      return 2;
   }

   AnsiConsole.MarkupLine(
      "[green]Updated title:[/] {0}",
      Markup.Escape(response.Data.Id));

   AnsiConsole.MarkupLine(
      "[bold]New title:[/] {0}",
      Markup.Escape(response.Data.Title));

   if (!string.IsNullOrWhiteSpace(response.Message))
   {
      AnsiConsole.MarkupLine(
         "[grey]{0}[/]",
         Markup.Escape(response.Message));
   }

   return 0;
}

static void ShowSummary(WhenItFailsWorkspaceSummary summary)
{
   AnsiConsole.Write(
      new Rule("[bold aqua]WhenItFails Workspace Summary[/]")
         .RuleStyle("aqua"));

   AnsiConsole.MarkupLine(
      "[bold]Workspace:[/] {0}",
      Markup.Escape(summary.DisplayPath));

   AnsiConsole.MarkupLine(
      "[bold]Directory:[/] {0}",
      Markup.Escape(summary.PackageDirectoryPath));

   AnsiConsole.WriteLine();

   ShowCatalogOverview(summary);
   ShowOwnerTable(summary.OwnerCatalog.Owners);
   ShowCodeGroupTable(summary.CodeGroupCatalog.CodeGroups);
   ShowProfileTable(summary.ProfileCatalog.Profiles);
   ShowTopCategoryTable(summary.ErrorCatalog.Errors);
}

static void ShowErrors(
   WhenItFailsWorkspaceSummary summary,
   IReadOnlyCollection<ErrorDefinition> errors,
   ErrorListOptions errorListOptions)
{
   AnsiConsole.Write(
      new Rule("[bold aqua]WhenItFails Error Definitions[/]")
         .RuleStyle("aqua"));

   AnsiConsole.MarkupLine(
      "[bold]Workspace:[/] {0}",
      Markup.Escape(summary.DisplayPath));

   AnsiConsole.MarkupLine(
      "[bold]Errors:[/] {0} shown from {1}",
      errors.Count.ToString(),
      summary.ErrorCount.ToString());

   ShowActiveErrorFilters(errorListOptions);

   AnsiConsole.WriteLine();

   Table errorTable = new Table();

   ConfigureWideTable(
      errorTable,
      Color.Aqua);

   errorTable.AddColumn(new TableColumn("Code").NoWrap());
   errorTable.AddColumn(new TableColumn("Id").NoWrap());
   errorTable.AddColumn("Name");
   errorTable.AddColumn(new TableColumn("Area").NoWrap());
   errorTable.AddColumn(new TableColumn("Severity").NoWrap());
   errorTable.AddColumn("Title");

   foreach (ErrorDefinition errorDefinition in errors
      .OrderBy(errorDefinition => errorDefinition.Code)
      .ThenBy(errorDefinition => errorDefinition.Id))
   {
      string area = string.Join(
         " / ",
         [
            errorDefinition.Owner,
            errorDefinition.CodeGroup,
            errorDefinition.PrimaryCategory
         ]);

      errorTable.AddRow(
         errorDefinition.Code.ToString(),
         Escape(errorDefinition.Id),
         Escape(errorDefinition.Name),
         Escape(area),
         Escape(errorDefinition.DefaultSeverity),
         Escape(errorDefinition.Title));
   }

   AnsiConsole.Write(errorTable);
}

static void ShowErrorsPlain(
   WhenItFailsWorkspaceSummary summary,
   IReadOnlyCollection<ErrorDefinition> errors,
   ErrorListOptions errorListOptions)
{
   Console.WriteLine("WhenItFails Error Definitions");
   Console.WriteLine($"Workspace: {summary.DisplayPath}");
   Console.WriteLine($"Errors: {errors.Count} shown from {summary.ErrorCount}");

   string activeFilters = CreateActiveFiltersText(errorListOptions);

   if (!string.IsNullOrWhiteSpace(activeFilters))
   {
      Console.WriteLine($"Filters: {activeFilters}");
   }

   Console.WriteLine();

   Console.WriteLine(
      "Code\tId\tName\tOwner\tGroup\tCategory\tSeverity\tTitle");

   foreach (ErrorDefinition errorDefinition in errors
      .OrderBy(errorDefinition => errorDefinition.Code)
      .ThenBy(errorDefinition => errorDefinition.Id))
   {
      Console.WriteLine(
         string.Join(
            "\t",
            [
               errorDefinition.Code.ToString(),
               errorDefinition.Id,
               errorDefinition.Name,
               errorDefinition.Owner,
               errorDefinition.CodeGroup,
               errorDefinition.PrimaryCategory,
               errorDefinition.DefaultSeverity,
               errorDefinition.Title
            ]));
   }
}

static void ShowDetails(
   WhenItFailsWorkspaceSummary summary,
   ErrorDefinition errorDefinition)
{
   AnsiConsole.Write(
      new Rule("[bold aqua]WhenItFails Error Detail[/]")
         .RuleStyle("aqua"));

   AnsiConsole.MarkupLine(
      "[bold]Workspace:[/] {0}",
      Markup.Escape(summary.DisplayPath));

   AnsiConsole.WriteLine();

   Table detailTable = new Table();

   ConfigureWideTable(
      detailTable,
      Color.Aqua);

   detailTable.AddColumn(new TableColumn("Field").NoWrap());
   detailTable.AddColumn("Value");

   detailTable.AddRow("Code", errorDefinition.Code.ToString());
   detailTable.AddRow("Id", Escape(errorDefinition.Id));
   detailTable.AddRow("Name", Escape(errorDefinition.Name));
   detailTable.AddRow("Title", Escape(errorDefinition.Title));
   detailTable.AddRow("Message", Escape(errorDefinition.Message));
   detailTable.AddRow("Severity", Escape(errorDefinition.DefaultSeverity));
   detailTable.AddRow("Owner", Escape(errorDefinition.Owner));
   detailTable.AddRow("Code prefix", Escape(errorDefinition.CodePrefix));
   detailTable.AddRow("Code group", Escape(errorDefinition.CodeGroup));
   detailTable.AddRow("Primary category", Escape(errorDefinition.PrimaryCategory));
   detailTable.AddRow("Categories", Escape(JoinPlainValues(errorDefinition.Categories)));
   detailTable.AddRow("Subcategories", Escape(JoinPlainValues(errorDefinition.Subcategories)));
   detailTable.AddRow("Tags", Escape(JoinPlainValues(errorDefinition.Tags)));
   detailTable.AddRow("Developer hint", Escape(errorDefinition.DeveloperHint));
   detailTable.AddRow("Documentation key", Escape(errorDefinition.DocumentationKey));

   AnsiConsole.Write(detailTable);
}

static void ShowDetailsPlain(
   WhenItFailsWorkspaceSummary summary,
   ErrorDefinition errorDefinition)
{
   Console.WriteLine("WhenItFails Error Detail");
   Console.WriteLine($"Workspace: {summary.DisplayPath}");
   Console.WriteLine();

   Console.WriteLine($"Code: {errorDefinition.Code}");
   Console.WriteLine($"Id: {errorDefinition.Id}");
   Console.WriteLine($"Name: {errorDefinition.Name}");
   Console.WriteLine($"Title: {errorDefinition.Title}");
   Console.WriteLine($"Message: {errorDefinition.Message}");
   Console.WriteLine($"Severity: {errorDefinition.DefaultSeverity}");
   Console.WriteLine($"Owner: {errorDefinition.Owner}");
   Console.WriteLine($"Code prefix: {errorDefinition.CodePrefix}");
   Console.WriteLine($"Code group: {errorDefinition.CodeGroup}");
   Console.WriteLine($"Primary category: {errorDefinition.PrimaryCategory}");
   Console.WriteLine($"Categories: {JoinPlainValues(errorDefinition.Categories)}");
   Console.WriteLine($"Subcategories: {JoinPlainValues(errorDefinition.Subcategories)}");
   Console.WriteLine($"Tags: {JoinPlainValues(errorDefinition.Tags)}");
   Console.WriteLine($"Developer hint: {errorDefinition.DeveloperHint ?? string.Empty}");
   Console.WriteLine($"Documentation key: {errorDefinition.DocumentationKey ?? string.Empty}");
}

static void ShowCatalogOverview(WhenItFailsWorkspaceSummary summary)
{
   Table overviewTable = new Table();

   ConfigureWideTable(
      overviewTable,
      Color.Aqua);

   overviewTable.AddColumn("Catalog");
   overviewTable.AddColumn("Name");
   overviewTable.AddColumn("Items");
   overviewTable.AddColumn("Language");

   overviewTable.AddRow(
      "Errors",
      Escape(summary.ErrorCatalog.CatalogName),
      summary.ErrorCount.ToString(),
      Escape(summary.ErrorCatalog.Language));

   overviewTable.AddRow(
      "Categories",
      Escape(summary.CategoryCatalog.CatalogName),
      summary.CategoryCount.ToString(),
      Escape(summary.CategoryCatalog.Language));

   overviewTable.AddRow(
      "Code groups",
      Escape(summary.CodeGroupCatalog.CatalogName),
      summary.CodeGroupCount.ToString(),
      Escape(summary.CodeGroupCatalog.Language));

   overviewTable.AddRow(
      "Owners",
      Escape(summary.OwnerCatalog.CatalogName),
      summary.OwnerCount.ToString(),
      Escape(summary.OwnerCatalog.Language));

   overviewTable.AddRow(
      "Profiles",
      Escape(summary.ProfileCatalog.CatalogName),
      summary.ProfileCount.ToString(),
      Escape(summary.ProfileCatalog.Language));

   AnsiConsole.Write(overviewTable);
}

static void ShowOwnerTable(IReadOnlyCollection<ErrorOwnerDefinition> owners)
{
   Table ownerTable = new Table();

   ConfigureWideTable(
      ownerTable,
      Color.Blue);

   ownerTable.AddColumn("Owner");
   ownerTable.AddColumn("Display name");
   ownerTable.AddColumn("Range");
   ownerTable.AddColumn("Built-in");

   foreach (ErrorOwnerDefinition owner in owners.OrderBy(owner => owner.CodeFrom))
   {
      ownerTable.AddRow(
         Escape(owner.Name),
         Escape(owner.DisplayName),
         $"{owner.CodeFrom} - {owner.CodeTo}",
         owner.IsBuiltIn ? "[green]yes[/]" : "[grey]no[/]");
   }

   AnsiConsole.Write(ownerTable);
}

static void ShowCodeGroupTable(IReadOnlyCollection<ErrorCodeGroupDefinition> codeGroups)
{
   Table codeGroupTable = new Table();

   ConfigureWideTable(
      codeGroupTable,
      Color.Green);

   codeGroupTable.AddColumn("Code group");
   codeGroupTable.AddColumn("Prefix");
   codeGroupTable.AddColumn("Display name");
   codeGroupTable.AddColumn("Range");

   foreach (ErrorCodeGroupDefinition codeGroup in codeGroups.OrderBy(codeGroup => codeGroup.CodeFrom))
   {
      codeGroupTable.AddRow(
         Escape(codeGroup.Name),
         Escape(codeGroup.CodePrefix),
         Escape(codeGroup.DisplayName),
         $"{codeGroup.CodeFrom} - {codeGroup.CodeTo}");
   }

   AnsiConsole.Write(codeGroupTable);
}

static void ShowProfileTable(IReadOnlyCollection<ErrorProfileDefinition> profiles)
{
   Table profileTable = new Table();

   ConfigureWideTable(
      profileTable,
      Color.Yellow);

   profileTable.AddColumn("Profile");
   profileTable.AddColumn("Display name");
   profileTable.AddColumn("Owners");
   profileTable.AddColumn("Code groups");
   profileTable.AddColumn("Categories");

   foreach (ErrorProfileDefinition profile in profiles.OrderBy(profile => profile.Name))
   {
      profileTable.AddRow(
         Escape(profile.Name),
         Escape(profile.DisplayName),
         JoinValues(profile.IncludeOwners),
         JoinValues(profile.IncludeCodeGroups),
         JoinValues(profile.IncludeCategories));
   }

   AnsiConsole.Write(profileTable);
}

static void ShowTopCategoryTable(IReadOnlyCollection<ErrorDefinition> errors)
{
   Table categoryTable = new Table();

   ConfigureWideTable(
      categoryTable,
      Color.Purple);

   categoryTable.AddColumn("Primary category");
   categoryTable.AddColumn("Errors");

   IEnumerable<IGrouping<string, ErrorDefinition>> groupedErrors =
      errors
         .GroupBy(error => string.IsNullOrWhiteSpace(error.PrimaryCategory)
            ? "(empty)"
            : error.PrimaryCategory)
         .OrderByDescending(group => group.Count())
         .ThenBy(group => group.Key);

   foreach (IGrouping<string, ErrorDefinition> group in groupedErrors)
   {
      categoryTable.AddRow(
         Escape(group.Key),
         group.Count().ToString());
   }

   AnsiConsole.Write(categoryTable);
}

static ErrorListOptions ParseErrorListOptions(string[] args)
{
   return new ErrorListOptions
   {
      UsePlainOutput = HasSwitch(args, "--plain"),
      Owner = ReadOptionValue(args, "--owner"),
      CodeGroup = ReadOptionValue(args, "--group")
                  ?? ReadOptionValue(args, "--code-group"),
      Category = ReadOptionValue(args, "--category"),
      Severity = ReadOptionValue(args, "--severity"),
      Profile = ReadOptionValue(args, "--profile"),
      SearchText = ReadOptionValue(args, "--search")
   };
}

static IEnumerable<ErrorDefinition> ApplyErrorFilters(
   WhenItFailsWorkspaceSummary summary,
   ErrorListOptions errorListOptions)
{
   IEnumerable<ErrorDefinition> errors = summary.ErrorCatalog.Errors;

   ErrorProfileDefinition? profile = string.IsNullOrWhiteSpace(errorListOptions.Profile)
      ? null
      : FindProfile(summary, errorListOptions.Profile);

   if (profile is not null)
   {
      errors = errors.Where(errorDefinition =>
         MatchesProfile(errorDefinition, profile));
   }

   errors = errors.Where(errorDefinition =>
      MatchesOptionalFilter(errorDefinition.Owner, errorListOptions.Owner)
      && MatchesOptionalFilter(errorDefinition.CodeGroup, errorListOptions.CodeGroup)
      && MatchesOptionalFilter(errorDefinition.PrimaryCategory, errorListOptions.Category)
      && MatchesOptionalFilter(errorDefinition.DefaultSeverity, errorListOptions.Severity)
      && MatchesSearchText(errorDefinition, errorListOptions.SearchText));

   return errors;
}

static bool MatchesProfile(
   ErrorDefinition errorDefinition,
   ErrorProfileDefinition profile)
{
   bool ownerMatches = profile.IncludeOwners.Count == 0
      || ContainsExactText(profile.IncludeOwners, errorDefinition.Owner);

   bool codeGroupMatches = profile.IncludeCodeGroups.Count == 0
      || ContainsExactText(profile.IncludeCodeGroups, errorDefinition.CodeGroup);

   bool categoryMatches = profile.IncludeCategories.Count == 0
      || ContainsExactText(profile.IncludeCategories, errorDefinition.PrimaryCategory);

   return ownerMatches
          && codeGroupMatches
          && categoryMatches;
}

static ErrorProfileDefinition? FindProfile(
   WhenItFailsWorkspaceSummary summary,
   string profileName)
{
   return summary.ProfileCatalog.Profiles.FirstOrDefault(profile =>
      string.Equals(
         profile.Name,
         profileName,
         StringComparison.OrdinalIgnoreCase)
      || string.Equals(
         profile.DisplayName,
         profileName,
         StringComparison.OrdinalIgnoreCase));
}

static ErrorDefinition? FindErrorDefinition(
   WhenItFailsWorkspaceSummary summary,
   string lookupValue)
{
   if (int.TryParse(lookupValue, out int numericCode))
   {
      ErrorDefinition? byCode = summary.ErrorCatalog.Errors.FirstOrDefault(errorDefinition =>
         errorDefinition.Code == numericCode);

      if (byCode is not null)
      {
         return byCode;
      }
   }

   return summary.ErrorCatalog.Errors.FirstOrDefault(errorDefinition =>
      string.Equals(
         errorDefinition.Id,
         lookupValue,
         StringComparison.OrdinalIgnoreCase)
      || string.Equals(
         errorDefinition.Name,
         lookupValue,
         StringComparison.OrdinalIgnoreCase));
}

static bool MatchesOptionalFilter(
   string value,
   string? filter)
{
   if (string.IsNullOrWhiteSpace(filter))
   {
      return true;
   }

   return string.Equals(
      value,
      filter,
      StringComparison.OrdinalIgnoreCase);
}

static bool MatchesSearchText(
   ErrorDefinition errorDefinition,
   string? searchText)
{
   if (string.IsNullOrWhiteSpace(searchText))
   {
      return true;
   }

   return ContainsText(errorDefinition.Id, searchText)
          || ContainsText(errorDefinition.Name, searchText)
          || ContainsText(errorDefinition.Title, searchText)
          || ContainsText(errorDefinition.Message, searchText)
          || ContainsText(errorDefinition.DeveloperHint, searchText)
          || ContainsText(errorDefinition.DocumentationKey, searchText)
          || ContainsText(errorDefinition.Code.ToString(), searchText)
          || ContainsText(errorDefinition.Owner, searchText)
          || ContainsText(errorDefinition.CodeGroup, searchText)
          || ContainsText(errorDefinition.PrimaryCategory, searchText)
          || ContainsAnyText(errorDefinition.Categories, searchText)
          || ContainsAnyText(errorDefinition.Subcategories, searchText)
          || ContainsAnyText(errorDefinition.Tags, searchText);
}

static bool ContainsText(
   string? value,
   string searchText)
{
   if (string.IsNullOrWhiteSpace(value))
   {
      return false;
   }

   return value.Contains(
      searchText,
      StringComparison.OrdinalIgnoreCase);
}

static bool ContainsAnyText(
   IReadOnlyCollection<string> values,
   string searchText)
{
   return values.Any(value =>
      ContainsText(
         value,
         searchText));
}

static bool ContainsExactText(
   IReadOnlyCollection<string> values,
   string searchedValue)
{
   return values.Any(value =>
      string.Equals(
         value,
         searchedValue,
         StringComparison.OrdinalIgnoreCase));
}

static bool HasSwitch(
   string[] args,
   string switchName)
{
   return args.Any(argument =>
      string.Equals(
         argument,
         switchName,
         StringComparison.OrdinalIgnoreCase));
}

static string? ReadOptionValue(
   string[] args,
   string optionName)
{
   for (int index = 0; index < args.Length; index++)
   {
      if (!string.Equals(
             args[index],
             optionName,
             StringComparison.OrdinalIgnoreCase))
      {
         continue;
      }

      int valueIndex = index + 1;

      if (valueIndex >= args.Length)
      {
         return null;
      }

      string value = args[valueIndex];

      if (value.StartsWith("--", StringComparison.Ordinal))
      {
         return null;
      }

      return value;
   }

   return null;
}

static void ShowActiveErrorFilters(ErrorListOptions errorListOptions)
{
   string activeFilters = CreateActiveFiltersText(errorListOptions);

   if (string.IsNullOrWhiteSpace(activeFilters))
   {
      return;
   }

   AnsiConsole.MarkupLine(
      "[bold]Filters:[/] {0}",
      Markup.Escape(activeFilters));
}

static string CreateActiveFiltersText(ErrorListOptions errorListOptions)
{
   List<string> filters = [];

   AddFilterText(filters, "owner", errorListOptions.Owner);
   AddFilterText(filters, "group", errorListOptions.CodeGroup);
   AddFilterText(filters, "category", errorListOptions.Category);
   AddFilterText(filters, "severity", errorListOptions.Severity);
   AddFilterText(filters, "profile", errorListOptions.Profile);
   AddFilterText(filters, "search", errorListOptions.SearchText);

   return string.Join(
      ", ",
      filters);
}

static void AddFilterText(
   List<string> filters,
   string name,
   string? value)
{
   if (string.IsNullOrWhiteSpace(value))
   {
      return;
   }

   filters.Add($"{name}={value}");
}

static void ConfigureWideTable(
   Table table,
   Color borderColor)
{
   int consoleWidth = GetConsoleWidth();
   int tableWidth = Math.Max(80, consoleWidth - 2);

   table.Border(TableBorder.Rounded);
   table.BorderColor(borderColor);
   table.Width(tableWidth);
}

static int GetConsoleWidth()
{
   try
   {
      if (Console.WindowWidth > 0)
      {
         return Console.WindowWidth;
      }
   }
   catch
   {
      // Some redirected/piped outputs do not expose a real console width.
   }

   return 120;
}

static string JoinValues(IReadOnlyCollection<string> values)
{
   if (values.Count == 0)
   {
      return "[grey](all)[/]";
   }

   return Markup.Escape(string.Join(", ", values));
}

static string JoinPlainValues(IReadOnlyCollection<string> values)
{
   if (values.Count == 0)
   {
      return string.Empty;
   }

   return string.Join(
      ", ",
      values);
}

static string Escape(string? value)
{
   return Markup.Escape(value ?? string.Empty);
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

internal sealed class ErrorListOptions
{
   public bool UsePlainOutput { get; set; }

   public string? Owner { get; set; }

   public string? CodeGroup { get; set; }

   public string? Category { get; set; }

   public string? Severity { get; set; }

   public string? Profile { get; set; }

   public string? SearchText { get; set; }
}