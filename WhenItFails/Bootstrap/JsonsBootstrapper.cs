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

            if (File.Exists(rootDirectory))
            {
                return Response<JsonsBootstrapPayload>.Invalid(
                    code: "WIF_JSONS_ROOT_DIRECTORY_INVALID",
                    message: "The JSON root directory path is invalid.");
            }

            string? rootParentPath =
                Path.GetDirectoryName(
                    Path.GetFullPath(rootDirectory));

            while (rootParentPath is not null)
            {
                if (File.Exists(rootParentPath))
                {
                    return Response<JsonsBootstrapPayload>.Invalid(
                        code: "WIF_JSONS_ROOT_DIRECTORY_INVALID",
                        message: "The JSON root directory path is invalid.");
                }

                rootParentPath =
                    Path.GetDirectoryName(rootParentPath);
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
                string fullRootDirectoryPath =
                    Path.GetFullPath(rootDirectory);

                string fullPackageDirectoryPath =
                    Path.GetFullPath(
                        Path.Combine(
                            rootDirectory,
                            normalizedPackageDirectoryName));

                StringComparison pathComparison =
                    OperatingSystem.IsWindows()
                        ? StringComparison.OrdinalIgnoreCase
                        : StringComparison.Ordinal;

                if (string.Equals(
                    fullPackageDirectoryPath,
                    fullRootDirectoryPath,
                    pathComparison))
                {
                    return Response<JsonsBootstrapPayload>.Invalid(
                        code: "WIF_JSONS_PACKAGE_DIRECTORY_NAME_INVALID",
                        message: "The package directory name is invalid.");
                }

                return Response<JsonsBootstrapPayload>.Invalid(
                    code: "WIF_JSONS_PACKAGE_DIRECTORY_NAME_OUTSIDE_ROOT",
                    message: "The package directory name must stay inside the JSON root directory.");
            }

            string packageDirectoryPath = Path.Combine(rootDirectory, normalizedPackageDirectoryName);

            if (File.Exists(packageDirectoryPath))
            {
                return Response<JsonsBootstrapPayload>.Invalid(
                    code: "WIF_JSONS_PACKAGE_DIRECTORY_NAME_INVALID",
                    message: "The package directory name is invalid.");
            }

            string fullPackageRootPath =
                Path.GetFullPath(rootDirectory);

            string? packageParentPath =
                Path.GetDirectoryName(
                    Path.GetFullPath(packageDirectoryPath));

            StringComparison packageParentPathComparison =
                OperatingSystem.IsWindows()
                    ? StringComparison.OrdinalIgnoreCase
                    : StringComparison.Ordinal;

            while (packageParentPath is not null
                && !string.Equals(
                    packageParentPath,
                    fullPackageRootPath,
                    packageParentPathComparison))
            {
                if (File.Exists(packageParentPath))
                {
                    return Response<JsonsBootstrapPayload>.Invalid(
                        code: "WIF_JSONS_PACKAGE_DIRECTORY_NAME_INVALID",
                        message: "The package directory name is invalid.");
                }

                packageParentPath =
                    Path.GetDirectoryName(packageParentPath);
            }

            string normalizedErrorCatalogFileName =
                NormalizePath(options.ErrorCatalogFileName);

            if (Path.EndsInDirectorySeparator(
                normalizedErrorCatalogFileName)
                || string.Equals(
                    Path.GetFileName(normalizedErrorCatalogFileName),
                    ".",
                    StringComparison.Ordinal))
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

            if (string.Equals(
                Path.GetFileName(normalizedErrorCatalogFileName),
                "..",
                StringComparison.Ordinal))
            {
                return Response<JsonsBootstrapPayload>.Invalid(
                    code: "WIF_JSONS_ERROR_CATALOG_FILE_NAME_INVALID",
                    message: "The error catalog file name is invalid.");
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

            string fullErrorCatalogPackagePath =
                Path.GetFullPath(packageDirectoryPath);

            string? errorCatalogParentPath =
                Path.GetDirectoryName(
                    Path.GetFullPath(errorCatalogFilePath));

            StringComparison errorCatalogParentPathComparison =
                OperatingSystem.IsWindows()
                    ? StringComparison.OrdinalIgnoreCase
                    : StringComparison.Ordinal;

            while (errorCatalogParentPath is not null
                && !string.Equals(
                    errorCatalogParentPath,
                    fullErrorCatalogPackagePath,
                    errorCatalogParentPathComparison))
            {
                if (File.Exists(errorCatalogParentPath))
                {
                    return Response<JsonsBootstrapPayload>.Invalid(
                        code: "WIF_JSONS_ERROR_CATALOG_FILE_NAME_INVALID",
                        message: "The error catalog file name is invalid.");
                }

                errorCatalogParentPath =
                    Path.GetDirectoryName(errorCatalogParentPath);
            }

            string normalizedCategoryCatalogFileName =
                NormalizePath(options.CategoryCatalogFileName);

            if (Path.EndsInDirectorySeparator(
                normalizedCategoryCatalogFileName)
                || string.Equals(
                    Path.GetFileName(normalizedCategoryCatalogFileName),
                    ".",
                    StringComparison.Ordinal))
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

            if (string.Equals(
                Path.GetFileName(normalizedCategoryCatalogFileName),
                "..",
                StringComparison.Ordinal))
            {
                return Response<JsonsBootstrapPayload>.Invalid(
                    code: "WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_INVALID",
                    message: "The category catalog file name is invalid.");
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

            string fullCategoryCatalogPackagePath =
                Path.GetFullPath(packageDirectoryPath);

            string? categoryCatalogParentPath =
                Path.GetDirectoryName(
                    Path.GetFullPath(categoryCatalogFilePath));

            StringComparison categoryCatalogParentPathComparison =
                OperatingSystem.IsWindows()
                    ? StringComparison.OrdinalIgnoreCase
                    : StringComparison.Ordinal;

            while (categoryCatalogParentPath is not null
                && !string.Equals(
                    categoryCatalogParentPath,
                    fullCategoryCatalogPackagePath,
                    categoryCatalogParentPathComparison))
            {
                if (File.Exists(categoryCatalogParentPath))
                {
                    return Response<JsonsBootstrapPayload>.Invalid(
                        code: "WIF_JSONS_CATEGORY_CATALOG_FILE_NAME_INVALID",
                        message: "The category catalog file name is invalid.");
                }

                categoryCatalogParentPath =
                    Path.GetDirectoryName(categoryCatalogParentPath);
            }

            string normalizedCodeGroupCatalogFileName =
                NormalizePath(options.CodeGroupCatalogFileName);

            if (Path.EndsInDirectorySeparator(
                normalizedCodeGroupCatalogFileName)
                || string.Equals(
                    Path.GetFileName(normalizedCodeGroupCatalogFileName),
                    ".",
                    StringComparison.Ordinal))
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
                string fullPackageDirectoryPath =
                    Path.GetFullPath(packageDirectoryPath);

                string fullCodeGroupCatalogFilePath =
                    Path.GetFullPath(
                        Path.Combine(
                            packageDirectoryPath,
                            normalizedCodeGroupCatalogFileName));

                StringComparison pathComparison =
                    OperatingSystem.IsWindows()
                        ? StringComparison.OrdinalIgnoreCase
                        : StringComparison.Ordinal;

                if (string.Equals(
                    fullCodeGroupCatalogFilePath,
                    fullPackageDirectoryPath,
                    pathComparison))
                {
                    return Response<JsonsBootstrapPayload>.Invalid(
                        code: "WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_INVALID",
                        message: "The code group catalog file name is invalid.");
                }

                return Response<JsonsBootstrapPayload>.Invalid(
                    code: "WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_OUTSIDE_PACKAGE",
                    message: "The code group catalog file name must stay inside the package directory.");
            }

            if (string.Equals(
                Path.GetFileName(normalizedCodeGroupCatalogFileName),
                "..",
                StringComparison.Ordinal))
            {
                return Response<JsonsBootstrapPayload>.Invalid(
                    code: "WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_INVALID",
                    message: "The code group catalog file name is invalid.");
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

            string fullCodeGroupCatalogPackagePath =
                Path.GetFullPath(packageDirectoryPath);

            string? codeGroupCatalogParentPath =
                Path.GetDirectoryName(
                    Path.GetFullPath(codeGroupCatalogFilePath));

            StringComparison codeGroupCatalogParentPathComparison =
                OperatingSystem.IsWindows()
                    ? StringComparison.OrdinalIgnoreCase
                    : StringComparison.Ordinal;

            while (codeGroupCatalogParentPath is not null
                && !string.Equals(
                    codeGroupCatalogParentPath,
                    fullCodeGroupCatalogPackagePath,
                    codeGroupCatalogParentPathComparison))
            {
                if (File.Exists(codeGroupCatalogParentPath))
                {
                    return Response<JsonsBootstrapPayload>.Invalid(
                        code: "WIF_JSONS_CODE_GROUP_CATALOG_FILE_NAME_INVALID",
                        message: "The code group catalog file name is invalid.");
                }

                codeGroupCatalogParentPath =
                    Path.GetDirectoryName(codeGroupCatalogParentPath);
            }

            string normalizedOwnerCatalogFileName =
                NormalizePath(options.OwnerCatalogFileName);

            if (Path.EndsInDirectorySeparator(
                normalizedOwnerCatalogFileName)
                || string.Equals(
                    Path.GetFileName(normalizedOwnerCatalogFileName),
                    ".",
                    StringComparison.Ordinal))
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
                string fullPackageDirectoryPath =
                    Path.GetFullPath(packageDirectoryPath);

                string fullOwnerCatalogFilePath =
                    Path.GetFullPath(
                        Path.Combine(
                            packageDirectoryPath,
                            normalizedOwnerCatalogFileName));

                StringComparison pathComparison =
                    OperatingSystem.IsWindows()
                        ? StringComparison.OrdinalIgnoreCase
                        : StringComparison.Ordinal;

                if (string.Equals(
                    fullOwnerCatalogFilePath,
                    fullPackageDirectoryPath,
                    pathComparison))
                {
                    return Response<JsonsBootstrapPayload>.Invalid(
                        code: "WIF_JSONS_OWNER_CATALOG_FILE_NAME_INVALID",
                        message: "The owner catalog file name is invalid.");
                }

                return Response<JsonsBootstrapPayload>.Invalid(
                    code: "WIF_JSONS_OWNER_CATALOG_FILE_NAME_OUTSIDE_PACKAGE",
                    message: "The owner catalog file name must stay inside the package directory.");
            }

            if (string.Equals(
                Path.GetFileName(normalizedOwnerCatalogFileName),
                "..",
                StringComparison.Ordinal))
            {
                return Response<JsonsBootstrapPayload>.Invalid(
                    code: "WIF_JSONS_OWNER_CATALOG_FILE_NAME_INVALID",
                    message: "The owner catalog file name is invalid.");
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

            string fullOwnerCatalogPackagePath =
                Path.GetFullPath(packageDirectoryPath);

            string? ownerCatalogParentPath =
                Path.GetDirectoryName(
                    Path.GetFullPath(ownerCatalogFilePath));

            StringComparison ownerCatalogParentPathComparison =
                OperatingSystem.IsWindows()
                    ? StringComparison.OrdinalIgnoreCase
                    : StringComparison.Ordinal;

            while (ownerCatalogParentPath is not null
                && !string.Equals(
                    ownerCatalogParentPath,
                    fullOwnerCatalogPackagePath,
                    ownerCatalogParentPathComparison))
            {
                if (File.Exists(ownerCatalogParentPath))
                {
                    return Response<JsonsBootstrapPayload>.Invalid(
                        code: "WIF_JSONS_OWNER_CATALOG_FILE_NAME_INVALID",
                        message: "The owner catalog file name is invalid.");
                }

                ownerCatalogParentPath =
                    Path.GetDirectoryName(ownerCatalogParentPath);
            }

            string normalizedProfilesFileName =
                NormalizePath(options.ProfilesFileName);

            if (Path.EndsInDirectorySeparator(
                normalizedProfilesFileName)
                || string.Equals(
                    Path.GetFileName(normalizedProfilesFileName),
                    ".",
                    StringComparison.Ordinal))
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
                string fullPackageDirectoryPath =
                    Path.GetFullPath(packageDirectoryPath);

                string fullProfilesFilePath =
                    Path.GetFullPath(
                        Path.Combine(
                            packageDirectoryPath,
                            normalizedProfilesFileName));

                StringComparison pathComparison =
                    OperatingSystem.IsWindows()
                        ? StringComparison.OrdinalIgnoreCase
                        : StringComparison.Ordinal;

                if (string.Equals(
                    fullProfilesFilePath,
                    fullPackageDirectoryPath,
                    pathComparison))
                {
                    return Response<JsonsBootstrapPayload>.Invalid(
                        code: "WIF_JSONS_PROFILE_CATALOG_FILE_NAME_INVALID",
                        message: "The profile catalog file name is invalid.");
                }

                return Response<JsonsBootstrapPayload>.Invalid(
                    code: "WIF_JSONS_PROFILE_CATALOG_FILE_NAME_OUTSIDE_PACKAGE",
                    message: "The profile catalog file name must stay inside the package directory.");
            }

            if (string.Equals(
                Path.GetFileName(normalizedProfilesFileName),
                "..",
                StringComparison.Ordinal))
            {
                return Response<JsonsBootstrapPayload>.Invalid(
                    code: "WIF_JSONS_PROFILE_CATALOG_FILE_NAME_INVALID",
                    message: "The profile catalog file name is invalid.");
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

            string fullProfilesPackagePath =
                Path.GetFullPath(packageDirectoryPath);

            string? profilesParentPath =
                Path.GetDirectoryName(
                    Path.GetFullPath(profilesFilePath));

            StringComparison profilesParentPathComparison =
                OperatingSystem.IsWindows()
                    ? StringComparison.OrdinalIgnoreCase
                    : StringComparison.Ordinal;

            while (profilesParentPath is not null
                && !string.Equals(
                    profilesParentPath,
                    fullProfilesPackagePath,
                    profilesParentPathComparison))
            {
                if (File.Exists(profilesParentPath))
                {
                    return Response<JsonsBootstrapPayload>.Invalid(
                        code: "WIF_JSONS_PROFILE_CATALOG_FILE_NAME_INVALID",
                        message: "The profile catalog file name is invalid.");
                }

                profilesParentPath =
                    Path.GetDirectoryName(profilesParentPath);
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

            StringComparer templateTargetPathComparer =
                OperatingSystem.IsWindows()
                    ? StringComparer.OrdinalIgnoreCase
                    : StringComparer.Ordinal;

            HashSet<string> templateTargetPaths =
                new(templateTargetPathComparer);

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
                    normalizedTemplateTargetFileName)
                    || string.Equals(
                        Path.GetFileName(normalizedTemplateTargetFileName),
                        ".",
                        StringComparison.Ordinal))
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

                if (string.Equals(
                    Path.GetFileName(normalizedTemplateTargetFileName),
                    "..",
                    StringComparison.Ordinal))
                {
                    return Response<JsonsBootstrapPayload>.Invalid(
                        code: "WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_INVALID",
                        message:
                            "The JSON template provider returned a template with an invalid target file name.");
                }

                string templateTargetFilePath = Path.Combine(
                    packageDirectoryPath,
                    normalizedTemplateTargetFileName);

                string canonicalTemplateTargetFilePath =
                    Path.GetFullPath(templateTargetFilePath);

                if (!templateTargetPaths.Add(canonicalTemplateTargetFilePath))
                {
                    return Response<JsonsBootstrapPayload>.Invalid(
                        code: "WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_DUPLICATE",
                        message:
                            "The JSON template provider returned multiple templates for the same target file.");
                }

                if (Directory.Exists(templateTargetFilePath))
                {
                    return Response<JsonsBootstrapPayload>.Invalid(
                        code: "WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_INVALID",
                        message:
                            "The JSON template provider returned a template with an invalid target file name.");
                }

                string fullPackageDirectoryPathForParents =
                    Path.GetFullPath(packageDirectoryPath);

                string? templateTargetParentPath =
                    Path.GetDirectoryName(
                        Path.GetFullPath(templateTargetFilePath));

                StringComparison parentPathComparison =
                    OperatingSystem.IsWindows()
                        ? StringComparison.OrdinalIgnoreCase
                        : StringComparison.Ordinal;

                while (templateTargetParentPath is not null
                    && !string.Equals(
                        templateTargetParentPath,
                        fullPackageDirectoryPathForParents,
                        parentPathComparison))
                {
                    if (File.Exists(templateTargetParentPath))
                    {
                        return Response<JsonsBootstrapPayload>.Invalid(
                            code: "WIF_JSONS_TEMPLATE_TARGET_FILE_NAME_INVALID",
                            message:
                                "The JSON template provider returned a template with an invalid target file name.");
                    }

                    templateTargetParentPath =
                        Path.GetDirectoryName(templateTargetParentPath);
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
