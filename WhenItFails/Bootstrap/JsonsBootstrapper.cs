using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Bootstrap;

/// <summary>
/// Default implementation that prepares the project-local JSON workspace.
/// </summary>
public sealed class JsonsBootstrapper : IJsonsBootstrapper
{
    private readonly IJsonsTemplateProvider _templateProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonsBootstrapper"/> class.
    /// </summary>
    /// <param name="templateProvider">JSON template provider.</param>
    public JsonsBootstrapper(IJsonsTemplateProvider templateProvider)
    {
        _templateProvider = templateProvider
            ?? throw new ArgumentNullException(nameof(templateProvider));
    }

    /// <inheritdoc />
    public async Task<Response<JsonsBootstrapPayload>> EnsureWorkspaceAsync(
        JsonsOptions options,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(options);

        try
        {
            string rootDirectory = NormalizePath(options.RootDirectory);
            string packageDirectoryName = NormalizePath(options.PackageDirectoryName);
            string packageDirectoryPath = Path.Combine(rootDirectory, packageDirectoryName);

            bool packageDirectoryAlreadyExisted =
                Directory.Exists(packageDirectoryPath);

            if (!packageDirectoryAlreadyExisted)
            {
                Directory.CreateDirectory(packageDirectoryPath);
            }

            JsonsBootstrapPayload payload = new()
            {
                RootDirectory = rootDirectory,
                PackageDirectoryPath = packageDirectoryPath,
                PackageDirectoryAlreadyExisted = packageDirectoryAlreadyExisted,
                PackageDirectoryCreated = !packageDirectoryAlreadyExisted
            };

            IReadOnlyList<JsonsTemplateFile>? templateFiles =
                _templateProvider.GetTemplateFiles(options);

            if (templateFiles is null)
            {
                return Response<JsonsBootstrapPayload>.Invalid(
                    code: "WIF_JSONS_TEMPLATE_COLLECTION_NULL",
                    message:
                        "The JSON template provider returned a null template collection.");
            }

            foreach (JsonsTemplateFile? templateFile in templateFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (templateFile is null)
                {
                    return Response<JsonsBootstrapPayload>.Invalid(
                        code: "WIF_JSONS_TEMPLATE_ITEM_NULL",
                        message:
                            "The JSON template provider returned a null template item.");
                }

                if (templateFile.TargetFileName is null)
                {
                    return Response<JsonsBootstrapPayload>.Invalid(
                        code: "WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_NULL",
                        message:
                            "The JSON template provider returned a template with a null target file name.");
                }

                if (string.IsNullOrWhiteSpace(templateFile.TargetFileName))
                {
                    return Response<JsonsBootstrapPayload>.Invalid(
                        code: "WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_EMPTY",
                        message:
                            "The JSON template provider returned a template with an empty target file name.");
                }

                if (templateFile.Content is null)
                {
                    return Response<JsonsBootstrapPayload>.Invalid(
                        code: "WIF_JSONS_TEMPLATE_CONTENT_NULL",
                        message:
                            "The JSON template provider returned a template with null content.");
                }

                JsonsBootstrapFileResult fileResult =
                    await EnsureTemplateFileAsync(
                        packageDirectoryPath,
                        templateFile,
                        cancellationToken);

                payload.Files.Add(fileResult);
            }

            return Response<JsonsBootstrapPayload>.Ok(payload);
        }
        catch (UnauthorizedAccessException exception)
        {
            return Response<JsonsBootstrapPayload>.Fail(
                code: "JsonsWorkspaceAccessDenied",
                message: $"Access to JSON workspace was denied. {exception.Message}");
        }
        catch (IOException exception)
        {
            return Response<JsonsBootstrapPayload>.Fail(
                code: "JsonsWorkspaceInputOutputError",
                message: $"An I/O error occurred while preparing JSON workspace. {exception.Message}");
        }
    }

    private static async Task<JsonsBootstrapFileResult> EnsureTemplateFileAsync(
        string packageDirectoryPath,
        JsonsTemplateFile templateFile,
        CancellationToken cancellationToken)
    {
        string targetFileName = NormalizePath(templateFile.TargetFileName);
        string targetFilePath = Path.Combine(packageDirectoryPath, targetFileName);

        if (File.Exists(targetFilePath))
        {
            return new JsonsBootstrapFileResult
            {
                Name = templateFile.Name,
                TargetFilePath = targetFilePath,
                AlreadyExisted = true,
                Created = false,
                Skipped = true,
                Message = "File already exists and was not overwritten."
            };
        }

        await File.WriteAllTextAsync(
            targetFilePath,
            templateFile.Content,
            cancellationToken);

        return new JsonsBootstrapFileResult
        {
            Name = templateFile.Name,
            TargetFilePath = targetFilePath,
            AlreadyExisted = false,
            Created = true,
            Skipped = false,
            Message = "File was created from template."
        };
    }

    private static string NormalizePath(string path)
    {
        return string.IsNullOrWhiteSpace(path)
            ? string.Empty
            : path.Trim();
    }
}
