using System.Text;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Catalog;

/// <summary>
/// Default provider that creates a validated catalog context
/// from the bundled Afrowave JSON templates.
/// </summary>
/// <remarks>
/// Built-in templates are materialized into an isolated temporary workspace,
/// loaded through the normal catalog pipeline and removed after loading.
/// The resulting context remains fully in memory.
/// </remarks>
public sealed class BuiltInErrorCatalogContextProvider
    : IBuiltInErrorCatalogContextProvider
{
    private readonly IJsonsTemplateProvider _templateProvider;
    private readonly IErrorCatalogContextProvider _contextProvider;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="BuiltInErrorCatalogContextProvider"/> class.
    /// </summary>
    public BuiltInErrorCatalogContextProvider(
        IJsonsTemplateProvider templateProvider,
        IErrorCatalogContextProvider contextProvider)
    {
        _templateProvider = templateProvider
            ?? throw new ArgumentNullException(nameof(templateProvider));

        _contextProvider = contextProvider
            ?? throw new ArgumentNullException(nameof(contextProvider));
    }

    /// <inheritdoc />
    public async Task<Response<ErrorCatalogContext>> LoadAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string temporaryRootDirectory =
            CreateTemporaryRootDirectory();

        JsonsOptions options = new()
        {
            RootDirectory = temporaryRootDirectory,
            PackageDirectoryName = "BuiltInDefaults"
        };

        try
        {
            Directory.CreateDirectory(
                options.PackageDirectoryPath);

            IReadOnlyList<JsonsTemplateFile> templates =
                _templateProvider.GetTemplateFiles(options);

            if (templates.Count == 0)
            {
                return Response<ErrorCatalogContext>.Invalid(
                    code: "WIF_BUILT_IN_TEMPLATES_EMPTY",
                    message:
                        "The bundled WhenItFails catalog templates are empty.");
            }

            foreach (JsonsTemplateFile template in templates)
            {
                cancellationToken.ThrowIfCancellationRequested();

                Response<ErrorCatalogContext>? validationFailure =
                    ValidateTemplate(template);

                if (validationFailure is not null)
                {
                    return validationFailure;
                }

                string targetFilePath =
                    Path.Combine(
                        options.PackageDirectoryPath,
                        template.TargetFileName);

                await File.WriteAllTextAsync(
                    targetFilePath,
                    template.Content,
                    new UTF8Encoding(
                        encoderShouldEmitUTF8Identifier: false),
                    cancellationToken);
            }

            return await _contextProvider.LoadFromJsonsAsync(
                options,
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            return Response<ErrorCatalogContext>.Fail(
                code: "WIF_BUILT_IN_CONTEXT_LOAD_FAILED",
                message:
                    $"The bundled WhenItFails catalog context could not "
                    + $"be loaded: {exception.Message}");
        }
        finally
        {
            DeleteTemporaryDirectory(
                temporaryRootDirectory);
        }
    }

    private static Response<ErrorCatalogContext>? ValidateTemplate(
        JsonsTemplateFile template)
    {
        if (template is null)
        {
            return Response<ErrorCatalogContext>.Invalid(
                code: "WIF_BUILT_IN_TEMPLATE_NULL",
                message:
                    "A bundled WhenItFails catalog template is null.");
        }

        if (string.IsNullOrWhiteSpace(
            template.TargetFileName))
        {
            return Response<ErrorCatalogContext>.Invalid(
                code: "WIF_BUILT_IN_TEMPLATE_FILE_NAME_MISSING",
                message:
                    "A bundled WhenItFails catalog template "
                    + "does not define a target file name.");
        }

        if (string.IsNullOrWhiteSpace(
            template.Content))
        {
            return Response<ErrorCatalogContext>.Invalid(
                code: "WIF_BUILT_IN_TEMPLATE_CONTENT_MISSING",
                message:
                    $"The bundled catalog template "
                    + $"'{template.TargetFileName}' has no content.");
        }

        return null;
    }

    private static string CreateTemporaryRootDirectory()
    {
        return Path.Combine(
            Path.GetTempPath(),
            "Afrowave",
            "WhenItFails",
            "BuiltInDefaults",
            Guid.NewGuid().ToString("N"));
    }

    private static void DeleteTemporaryDirectory(
        string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            return;
        }

        try
        {
            Directory.Delete(
                directoryPath,
                recursive: true);
        }
        catch
        {
            // Temporary cleanup must not replace the actual load result.
        }
    }
}