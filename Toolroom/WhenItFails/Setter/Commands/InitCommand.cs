using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Validation;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Views;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'init' command: creates missing WhenItFails JSON files.
/// </summary>
internal static class InitCommand
{
    private const string Usage = "init <path> [--json]";

    /// <summary>
    /// Executes the init command.
    /// </summary>
    /// <param name="args">The full command-line arguments (args[0] is "init").</param>
    /// <returns>Exit code: 0 on success, 1 on missing or invalid arguments, 3 on failure.</returns>
    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            ErrorCatalogValidationResult missingPathResult = new();

            missingPathResult.AddError(
                code: "MissingInitPath",
                message: "The init command requires a project root path.",
                path: Usage);

            new ConsoleValidationResultShow().Show(
                missingPathResult,
                new ConsoleShowOptions
                {
                    SourcePath = "command line"
                });

            return 1;
        }

        if (!TryParseOptions(args, out bool useJsonOutput))
        {
            CommandInputError.Show(
                "InvalidInitArguments",
                "The init command accepts only a project root path and one optional --json switch.",
                Usage);
            return 1;
        }

        string projectRootPath = args[1];

        WhenItFailsWorkspaceInitializer initializer = new();

        Response<JsonsBootstrapPayload> response =
            await initializer.InitializeAsync(projectRootPath);

        if (!response.IsSuccess || response.Data is null)
        {
            string failureCode = response.Issues.Count > 0
                ? response.Issues[0].Code
                : "WorkspaceInitializationFailed";

            string failureMessage = string.IsNullOrWhiteSpace(response.Message)
                ? "WhenItFails workspace initialization failed."
                : response.Message;

            if (useJsonOutput)
            {
                CommandJsonOutput.Write(
                    "init",
                    new InitResult(
                        Initialized: false,
                        Bootstrap: null,
                        FailureCode: failureCode,
                        FailureMessage: failureMessage));
            }
            else
            {
                ErrorCatalogValidationResult failureResult = new();
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
            }

            return 3;
        }

        if (useJsonOutput)
        {
            CommandJsonOutput.Write(
                "init",
                new InitResult(
                    Initialized: true,
                    Bootstrap: response.Data,
                    FailureCode: null,
                    FailureMessage: null));
        }
        else
        {
            BootstrapResultView.Show(response.Data);
        }

        return 0;
    }

    public static bool TryParseOptions(string[] args, out bool useJsonOutput)
    {
        useJsonOutput = false;

        for (int index = 2; index < args.Length; index++)
        {
            if (!string.Equals(args[index], "--json", StringComparison.OrdinalIgnoreCase)
                || useJsonOutput)
            {
                return false;
            }

            useJsonOutput = true;
        }

        return true;
    }

    private sealed record InitResult(
        bool Initialized,
        JsonsBootstrapPayload? Bootstrap,
        string? FailureCode,
        string? FailureMessage);
}
