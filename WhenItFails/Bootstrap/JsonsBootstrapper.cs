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

        if (options.RootDirectory is null)
        {
            return Response<JsonsBootstrapPayload>.Invalid(
                code: "WIF_JSONS_ROOT_DIRECTORY_NULL",
                message: "The JSON root directory cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(options.RootDirectory))
        {
            return Response<JsonsBootstrapPayload>.Invalid(
                code: "WIF_JSONS_ROOT_DIRECTORY_EMPTY",
                message: "The JSON root directory cannot be empty.");
        }

        string? packageDirectoryName = options.PackageDirectoryName;

        if (packageDirectoryName is null)
        {
            return Response<JsonsBootstrapPayload>.Invalid(
                code: "WIF_JSONS_PACKAGE_DIRECTORY_NAME_NULL",
                message: "The package directory name cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(packageDirectoryName))
        {
            return Response<JsonsBootstrapPayload>.Invalid(
                code: "WIF_JSONS_PACKAGE_DIRECTORY_NAME_EMPTY",
                message: "The package directory name cannot be empty.");
        }

        if (options.ErrorCatalogFileName is null)
        {
            return Response<JsonsBootstrapPayload>.Invalid(
                code: "WIF_JSONS_ERROR_CATALOG_FILE_NAME_NULL",
                message: "The error catalog file name cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(options.ErrorCatalogFileName))
        {
            return Response<JsonsBootstrapPayload>.Invalid(
                code: "WIF_JSONS_ERROR_CATALOG_FILE_NAME_EMPTY",
                message: "The error catalog file name cannot be empty.");
        }

        if (options.CategoryCatalogFileName is null)
        {
            return Response<JsonsBootstrapPayload>.Invalid(
                code: "WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_NULL",
                message: "The category catalog file name cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(options.CategoryCatalogFileName))
        {
            return Response<JsonsBootstrapPayload>.Invalid(
                code: "WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_EMPTY",
                message: "The category catalog file name cannot be empty.");
        }

        if (options.CodeGroupCatalogFileName is null)
        {
            return Response<JsonsBootstrapPayload>.Invalid(
                code: "WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_NULL",
                message: "The code group catalog file name cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(options.CodeGroupCatalogFileName))
        {
            return Response<JsonsBootstrapPayload>.Invalid(
                code: "WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_EMPTY",
                message: "The code group catalog file name cannot be empty.");
        }

        if (options.OwnerCatalogFileName is null)
        {
            return Response<JsonsBootstrapPayload>.Invalid(
                code: "WIF_JSONS_OWNER_CATALOG_FILE_NAME_NULL",
                message: "The owner catalog file name cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(options.OwnerCatalogFileName))
        {
            return Response<JsonsBootstrapPayload>.Invalid(
                code: "WIF_JSONS_OWNER_CATALOG_FILE_NAME_EMPTY",
                message: "The owner catalog file name cannot be empty.");
        }

        if (options.ProfilesFileName is null)
        {
            return Response<JsonsBootstrapPayload>.Invalid(
                code: "WIF_JSONS_PROFILE_CATALOG_FILE_NAME_NULL",
                message: "The profile catalog file name cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(options.ProfilesFileName))
        {
            return Response<JsonsBootstrapPayload>.Invalid(
                code: "WIF_JSONS_PROFILE_CATALOG_FILE_NAME_EMPTY",
                message: "The profile catalog file name cannot be empty.");
        }

        try
        {
            string rootDirectory = NormalizePath(options.RootDirectory);
            string normalizedPackageDirectoryName = NormalizePath(packageDirectoryName);

            try
            {
                _ = Path.GetFullPath(rootDirectory);
            }
            catch (ArgumentException)
            {
                return Response<JsonsBootstrapPayload>.Invalid(
                    code: "WIF_JSONS_ROOT_DIRECTORY_INVALID",
                    message: "The JSON root directory path is invalid.");
            }

            bool packageDirectoryInsideRoot;

            try
            {
                packageDirectoryInsideRoot =
                    IsPathInsideDirectory(
                        rootDirectory,
                        normalizedPackageDirectoryName);
            }
            catch (ArgumentException)
            {
                return Response<JsonsBootstrapPayload>.Invalid(
                    code: "WIF_JSONS_PACKAGE_DIRECTORY_NAME_INVALID",
                    message: "The package directory name is invalid.");
            }

            if (!packageDirectoryInsideRoot)
            {
                return Response<JsonsBootstrapPayload>.Invalid(
                    code: "WIF_JSONS_PACKAGE_DIRECTORY_NAME_OUTSIDE_ROOT",
                    message: "The package directory name must stay inside the JSON root directory.");
            }

            string packageDirectoryPath = Path.Combine(rootDirectory, normalizedPackageDirectoryName);

            string normalizedErrorCatalogFileName =
                NormalizePath(options.ErrorCatalogFileName);

            if (Path.EndsInDirectorySeparator(
                normalizedErrorCatalogFileName))
            {
                return Response<JsonsBootstrapPayload>.Invalid(
                    code: "WIF_JSONS_ERROR_CATALOG_FILE_NAME_INVALID",
                    message: "The error catalog file name is invalid.");
            }

            bool errorCatalogFileInsidePackage;

            try
            {
                errorCatalogFileInsidePackage =
                    IsPathInsideDirectory(
                        packageDirectoryPath,
                        normalizedErrorCatalogFileName);
            }
            catch (ArgumentException)
            {
                return Response<JsonsBootstrapPayload>.Invalid(
                    code: "WIF_JSONS_ERROR_CATALOG_FILE_NAME_INVALID",
                    message: "The error catalog file name is invalid.");
            }

            if (!errorCatalogFileInsidePackage)
            {
                string fullPackageDirectoryPath =
                    Path.GetFullPath(packageDirectoryPath);

                string fullErrorCatalogFilePath =
                    Path.GetFullPath(
                        Path.Combine(
                            packageDirectoryPath,
                            normalizedErrorCatalogFileName));

                StringComparison pathComparison =
                    OperatingSystem.IsWindows()
                        ? StringComparison.OrdinalIgnoreCase
                        : StringComparison.Ordinal;

                if (string.Equals(
                    fullErrorCatalogFilePath,
                    fullPackageDirectoryPath,
                    pathComparison))
                {
                    return Response<JsonsBootstrapPayload>.Invalid(
                        code: "WIF_JSONS_ERROR_CATALOG_FILE_NAME_INVALID",
                        message: "The error catalog file name is invalid.");
                }

                return Response<JsonsBootstrapPayload>.Invalid(
                    code: "WIF_JSONS_ERROR_CATALOG_FILE_NAME_OUTSIDE_PACKAGE",
                    message: "The error catalog file name must stay inside the package directory.");
            }

            string errorCatalogFilePath = Path.Combine(
                packageDirectoryPath,
                normalizedErrorCatalogFileName);

            if (Directory.Exists(errorCatalogFilePath))
            {
                return Response<JsonsBootstrapPayload>.Invalid(
                    code: "WIF_JSONS_ERROR_CATALOG_FILE_NAME_INVALID",
                    message: "The error catalog file name is invalid.");
            }

            string normalizedCategoryCatalogFileName =
                NormalizePath(options.CategoryCatalogFileName);

            if (Path.EndsInDirectorySeparator(
                normalizedCategoryCatalogFileName))
            {
                return Response<JsonsBootstrapPayload>.Invalid(
                    code: "WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_INVALID",
                    message: "The category catalog file name is invalid.");
            }

            bool categoryCatalogFileInsidePackage;

            try
            {
                categoryCatalogFileInsidePackage =
                    IsPathInsideDirectory(
                        packageDirectoryPath,
                        normalizedCategoryCatalogFileName);
            }
            catch (ArgumentException)
            {
                return Response<JsonsBootstrapPayload>.Invalid(
                    code: "WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_INVALID",
                    message: "The category catalog file name is invalid.");
            }

            if (!categoryCatalogFileInsidePackage)
            {
                string fullPackageDirectoryPath =
                    Path.GetFullPath(packageDirectoryPath);

                string fullCategoryCatalogFilePath =
                    Path.GetFullPath(
                        Path.Combine(
                            packageDirectoryPath,
                            normalizedCategoryCatalogFileName));

                StringComparison pathComparison =
                    OperatingSystem.IsWindows()
                        ? StringComparison.OrdinalIgnoreCase
                        : StringComparison.Ordinal;

                if (string.Equals(
                    fullCategoryCatalogFilePath,
                    fullPackageDirectoryPath,
                    pathComparison))
                {
                    return Response<JsonsBootstrapPayload>.Invalid(
                        code: "WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_INVALID",
                        message: "The category catalog file name is invalid.");
                }

                return Response<JsonsBootstrapPayload>.Invalid(
                    code: "WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_OUTSIDE_PACKAGE",
                    message: "The category catalog file name must stay inside the package directory.");
            }

            string categoryCatalogFilePath = Path.Combine(
                packageDirectoryPath,
                normalizedCategoryCatalogFileName);

            if (Directory.Exists(categoryCatalogFilePath))
            {
                return Response<JsonsBootstrapPayload>.Invalid(
                    code: "WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_INVALID",
                    message: "The category catalog file name is invalid.");
            }

            string normalizedCodeGroupCatalogFileName =
                NormalizePath(options.CodeGroupCatalogFileName);

            if (Path.EndsInDirectorySeparator(
                normalizedCodeGroupCatalogFileName))
            {
                return Response<JsonsBootstrapPayload>.Invalid(
                    code: "WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_INVALID",
                    message: "The code group catalog file name is invalid.");
            }

            bool codeGroupCatalogFileInsidePackage;

            try
            {
                codeGroupCatalogFileInsidePackage =
                    IsPathInsideDirectory(
                        packageDirectoryPath,
                        normalizedCodeGroupCatalogFileName);
            }
            catch (ArgumentException)
            {
                return Response<JsonsBootstrapPayload>.Invalid(
                    code: "WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_INVALID",
                    message: "The code group catalog file name is invalid.");
            }

            if (!codeGroupCatalogFileInsidePackage)
            {
                return Response<JsonsBootstrapPayload>.Invalid(
                    code: "WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_OUTSIDE_PACKAGE",
                    message: "The code group catalog file name must stay inside the package directory.");
            }

            string codeGroupCatalogFilePath = Path.Combine(
                packageDirectoryPath,
                normalizedCodeGroupCatalogFileName);

            if (Directory.Exists(codeGroupCatalogFilePath))
            {
                return Response<JsonsBootstrapPayload>.Invalid(
                    code: "WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_INVALID",
                    message: "The code group catalog file name is invalid.");
            }

            string normalizedOwnerCatalogFileName =
                NormalizePath(options.OwnerCatalogFileName);

            if (Path.EndsInDirectorySeparator(
                normalizedOwnerCatalogFileName))
            {
                return Response<JsonsBootstrapPayload>.Invalid(
                    code: "WIF_JSONS_OWNER_CATALOG_FILE_NAME_INVALID",
                    message: "The owner catalog file name is invalid.");
            }

            bool ownerCatalogFileInsidePackage;

            try
            {
                ownerCatalogFileInsidePackage =
                    IsPathInsideDirectory(
                        packageDirectoryPath,
                        normalizedOwnerCatalogFileName);
            }
            catch (ArgumentException)
            {
                return Response<JsonsBootstrapPayload>.Invalid(
                    code: "WIF_JSONS_OWNER_CATALOG_FILE_NAME_INVALID",
                    message: "The owner catalog file name is invalid.");
            }

            if (!ownerCatalogFileInsidePackage)
            {
                return Response<JsonsBootstrapPayload>.Invalid(
                    code: "WIF_JSONS_OWNER_CATALOG_FILE_NAME_OUTSIDE_PACKAGE",
                    message: "The owner catalog file name must stay inside the package directory.");
            }

            string ownerCatalogFilePath = Path.Combine(
                packageDirectoryPath,
                normalizedOwnerCatalogFileName);

            if (Directory.Exists(ownerCatalogFilePath))
            {
                return Response<JsonsBootstrapPayload>.Invalid(
                    code: "WIF_JSONS_OWNER_CATALOG_FILE_NAME_INVALID",
                    message: "The owner catalog file name is invalid.");
            }

            string normalizedProfilesFileName =
                NormalizePath(options.ProfilesFileName);

            if (Path.EndsInDirectorySeparator(
                normalizedProfilesFileName))
            {
                return Response<JsonsBootstrapPayload>.Invalid(
                    code: "WIF_JSONS_PROFILE_CATALOG_FILE_NAME_INVALID",
                    message: "The profile catalog file name is invalid.");
            }

            bool profileCatalogFileInsidePackage;

            try
            {
                profileCatalogFileInsidePackage =
                    IsPathInsideDirectory(
                        packageDirectoryPath,
                        normalizedProfilesFileName);
            }
            catch (ArgumentException)
            {
                return Response<JsonsBootstrapPayload>.Invalid(
                    code: "WIF_JSONS_PROFILE_CATALOG_FILE_NAME_INVALID",
                    message: "The profile catalog file name is invalid.");
            }

            if (!profileCatalogFileInsidePackage)
            {
                return Response<JsonsBootstrapPayload>.Invalid(
                    code: "WIF_JSONS_PROFILE_CATALOG_FILE_NAME_OUTSIDE_PACKAGE",
                    message: "The profile catalog file name must stay inside the package directory.");
            }

            string profilesFilePath = Path.Combine(
                packageDirectoryPath,
                normalizedProfilesFileName);

            if (Directory.Exists(profilesFilePath))
            {
                return Response<JsonsBootstrapPayload>.Invalid(
                    code: "WIF_JSONS_PROFILE_CATALOG_FILE_NAME_INVALID",
                    message: "The profile catalog file name is invalid.");
            }

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

            IReadOnlyList<JsonsTemplateFile>? templateFiles;

            try
            {
                templateFiles = _templateProvider.GetTemplateFiles(options);
            }
            catch (Exception exception)
                when (exception is not OperationCanceledException)
            {
                return Response<JsonsBootstrapPayload>.Fail(
                    code: "WIF_JSONS_TEMPLATE_PROVIDER_FAILED",
                    message: "The JSON template provider failed.");
            }

            if (templateFiles is null)
            {
                return Response<JsonsBootstrapPayload>.Invalid(
                    code: "WIF_JSONS_TEMPLATE_COLLECTION_NULL",
                    message:
                        "The JSON template provider returned a null template collection.");
            }

            JsonsTemplateFile[] templateFileSnapshot;

            try
            {
                templateFileSnapshot = templateFiles.ToArray();
            }
            catch (Exception exception)
                when (exception is not OperationCanceledException)
            {
                return Response<JsonsBootstrapPayload>.Fail(
                    code: "WIF_JSONS_TEMPLATE_PROVIDER_FAILED",
                    message: "The JSON template provider failed.");
            }

            foreach (JsonsTemplateFile? templateFile in templateFileSnapshot)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (templateFile is null)
                {
                    return Response<JsonsBootstrapPayload>.Invalid(
                        code: "WIF_JSONS_TEMPLATE_ITEM_NULL",
                        message:
                            "The JSON template provider returned a null template item.");
                }

                if (templateFile.Name is null)
                {
                    return Response<JsonsBootstrapPayload>.Invalid(
                        code: "WIF_JSONS_TEMPLATE_NAME_NULL",
                        message:
                            "The JSON template provider returned a template with a null name.");
                }

                if (string.IsNullOrWhiteSpace(templateFile.Name))
                {
                    return Response<JsonsBootstrapPayload>.Invalid(
                        code: "WIF_JSONS_TEMPLATE_NAME_EMPTY",
                        message:
                            "The JSON template provider returned a template with an empty name.");
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

                string normalizedTemplateTargetFileName =
                    NormalizePath(templateFile.TargetFileName);

                if (Path.EndsInDirectorySeparator(
                    normalizedTemplateTargetFileName))
                {
                    return Response<JsonsBootstrapPayload>.Invalid(
                        code: "WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_INVALID",
                        message:
                            "The JSON template provider returned a template with an invalid target file name.");
                }

                bool templateTargetFileInsidePackage;

                try
                {
                    templateTargetFileInsidePackage =
                        IsPathInsideDirectory(
                            packageDirectoryPath,
                            templateFile.TargetFileName);
                }
                catch (ArgumentException)
                {
                    return Response<JsonsBootstrapPayload>.Invalid(
                        code: "WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_INVALID",
                        message:
                            "The JSON template provider returned a template with an invalid target file name.");
                }

                if (!templateTargetFileInsidePackage)
                {
                    string fullPackageDirectoryPath =
                        Path.GetFullPath(packageDirectoryPath);

                    string fullTemplateTargetFilePath =
                        Path.GetFullPath(
                            Path.Combine(
                                packageDirectoryPath,
                                normalizedTemplateTargetFileName));

                    StringComparison pathComparison =
                        OperatingSystem.IsWindows()
                            ? StringComparison.OrdinalIgnoreCase
                            : StringComparison.Ordinal;

                    if (string.Equals(
                        fullTemplateTargetFilePath,
                        fullPackageDirectoryPath,
                        pathComparison))
                    {
                        return Response<JsonsBootstrapPayload>.Invalid(
                            code: "WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_INVALID",
                            message:
                                "The JSON template provider returned a template with an invalid target file name.");
                    }

                    return Response<JsonsBootstrapPayload>.Invalid(
                        code: "WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_OUTSIDE_PACKAGE",
                        message:
                            "The JSON template provider returned a target file name outside the package directory.");
                }

                string templateTargetFilePath = Path.Combine(
                    packageDirectoryPath,
                    normalizedTemplateTargetFileName);

                if (Directory.Exists(templateTargetFilePath))
                {
                    return Response<JsonsBootstrapPayload>.Invalid(
                        code: "WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_INVALID",
                        message:
                            "The JSON template provider returned a template with an invalid target file name.");
                }

                if (templateFile.Content is null)
                {
                    return Response<JsonsBootstrapPayload>.Invalid(
                        code: "WIF_JSONS_TEMPLATE_CONTENT_NULL",
                        message:
                            "The JSON template provider returned a template with null content.");
                }
            }

            foreach (JsonsTemplateFile templateFile in templateFileSnapshot)
            {
                cancellationToken.ThrowIfCancellationRequested();

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

        string? targetDirectoryPath =
            Path.GetDirectoryName(targetFilePath);

        if (!string.IsNullOrEmpty(targetDirectoryPath)
            && !Directory.Exists(targetDirectoryPath))
        {
            Directory.CreateDirectory(targetDirectoryPath);
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

    private static bool IsPathInsideDirectory(
        string directoryPath,
        string targetFileName)
    {
        string fullDirectoryPath =
            Path.GetFullPath(directoryPath);

        string fullTargetPath =
            Path.GetFullPath(
                Path.Combine(
                    fullDirectoryPath,
                    NormalizePath(targetFileName)));

        string directoryPrefix =
            Path.EndsInDirectorySeparator(fullDirectoryPath)
                ? fullDirectoryPath
                : fullDirectoryPath + Path.DirectorySeparatorChar;

        StringComparison comparison =
            OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

        return fullTargetPath.StartsWith(
            directoryPrefix,
            comparison);
    }

    private static string NormalizePath(string path)
    {
        return string.IsNullOrWhiteSpace(path)
            ? string.Empty
            : path.Trim();
    }
}
