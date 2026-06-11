using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Bootstrap;

/// <summary>
/// Provides default JSON template files used to initialize the project-local WhenItFails workspace.
/// </summary>
/// <remarks>
/// This provider contains built-in default catalog templates directly in code.
/// The bootstrapper copies these templates into the project-local Jsons workspace
/// only when the target files do not exist yet.
///
/// The copied files are intended to become editable shadow copies. The package
/// templates remain the source defaults and should not overwrite user changes.
/// </remarks>
public sealed class DefaultJsonsTemplateProvider : IJsonsTemplateProvider
{
    /// <inheritdoc />
    public IReadOnlyList<JsonsTemplateFile> GetTemplateFiles(JsonsOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return
        [
            new JsonsTemplateFile
            {
                Name = "Error catalog",
                TargetFileName = options.ErrorCatalogFileName,
                Content = CreateErrorCatalogJson()
            },
            new JsonsTemplateFile
            {
                Name = "Category catalog",
                TargetFileName = options.CategoryCatalogFileName,
                Content = CreateCategoryCatalogJson()
            },
            new JsonsTemplateFile
            {
                Name = "Code group catalog",
                TargetFileName = options.CodeGroupCatalogFileName,
                Content = CreateCodeGroupCatalogJson()
            },
            new JsonsTemplateFile
            {
                Name = "Owner catalog",
                TargetFileName = options.OwnerCatalogFileName,
                Content = CreateOwnerCatalogJson()
            },
            new JsonsTemplateFile
            {
                Name = "Profiles",
                TargetFileName = options.ProfilesFileName,
                Content = CreateProfilesJson()
            }
        ];
    }

    private static string CreateErrorCatalogJson()
    {
        return """
        {
          "schemaVersion": "1.0",
          "catalogId": "afw.when-it-fails.errors",
          "catalogName": "Afrowave WhenItFails default error catalog",
          "description": "Default error catalog template for project-local customization.",
          "language": "en",
          "sourceCatalogId": "afw.when-it-fails.errors",
          "sourceCatalogVersion": "1.0",
          "isShadowCopy": true,
          "tags": [ "default", "errors", "shadow-copy-template" ],
          "errors": [
            {
              "id": "AFW-GEN-0001",
              "code": 100001,
              "name": "UnknownError",
              "owner": "AFW",
              "codePrefix": "GEN",
              "codeGroup": "GENERAL",
              "primaryCategory": "GENERAL",
              "categories": [ "GENERAL" ],
              "subcategories": [ "FALLBACK" ],
              "title": "Unknown error",
              "message": "An unknown error occurred.",
              "defaultSeverity": "Error",
              "developerHint": "Use this error only as a final fallback when no more specific definition fits.",
              "documentationKey": "when-it-fails/errors/general/unknown-error",
              "tags": [ "GENERAL", "FALLBACK", "USER_VISIBLE" ]
            },
            {
              "id": "AFW-GEN-0002",
              "code": 100002,
              "name": "OperationFailed",
              "owner": "AFW",
              "codePrefix": "GEN",
              "codeGroup": "GENERAL",
              "primaryCategory": "GENERAL",
              "categories": [ "GENERAL" ],
              "subcategories": [ "OPERATION" ],
              "title": "Operation failed",
              "message": "The requested operation failed.",
              "defaultSeverity": "Error",
              "developerHint": "Prefer a more specific error when the failure reason is known.",
              "documentationKey": "when-it-fails/errors/general/operation-failed",
              "tags": [ "GENERAL", "USER_VISIBLE" ]
            },
            {
              "id": "AFW-GEN-0003",
              "code": 100003,
              "name": "UnsupportedOperation",
              "owner": "AFW",
              "codePrefix": "GEN",
              "codeGroup": "GENERAL",
              "primaryCategory": "GENERAL",
              "categories": [ "GENERAL", "VALIDATION" ],
              "subcategories": [ "UNSUPPORTED" ],
              "title": "Unsupported operation",
              "message": "The requested operation is not supported in this context.",
              "defaultSeverity": "Warning",
              "developerHint": "Check feature flags, platform capabilities, current state, or selected profile.",
              "documentationKey": "when-it-fails/errors/general/unsupported-operation",
              "tags": [ "GENERAL", "USER_VISIBLE" ]
            },
            {
              "id": "AFW-GEN-0004",
              "code": 100004,
              "name": "FeatureNotImplemented",
              "owner": "AFW",
              "codePrefix": "GEN",
              "codeGroup": "GENERAL",
              "primaryCategory": "GENERAL",
              "categories": [ "GENERAL" ],
              "subcategories": [ "NOT_IMPLEMENTED" ],
              "title": "Feature not implemented",
              "message": "This feature is not implemented yet.",
              "defaultSeverity": "Warning",
              "developerHint": "Use for planned features that are intentionally present in the API but not active yet.",
              "documentationKey": "when-it-fails/errors/general/feature-not-implemented",
              "tags": [ "GENERAL", "DEVELOPMENT", "USER_VISIBLE" ]
            },
            {
              "id": "AFW-CFG-0001",
              "code": 200001,
              "name": "MissingConfigurationValue",
              "owner": "AFW",
              "codePrefix": "CFG",
              "codeGroup": "CONFIGURATION",
              "primaryCategory": "CONFIGURATION",
              "categories": [ "CONFIGURATION", "STARTUP", "VALIDATION" ],
              "subcategories": [ "REQUIRED_VALUE", "APP_SETTINGS" ],
              "title": "Missing configuration value",
              "message": "A required configuration value is missing.",
              "defaultSeverity": "Error",
              "developerHint": "Check appsettings.json, environment variables, command-line options, or code-based configuration.",
              "documentationKey": "when-it-fails/errors/configuration/missing-configuration-value",
              "tags": [ "CONFIGURATION", "STARTUP", "USER_VISIBLE" ]
            },
            {
              "id": "AFW-CFG-0002",
              "code": 200002,
              "name": "InvalidConfigurationValue",
              "owner": "AFW",
              "codePrefix": "CFG",
              "codeGroup": "CONFIGURATION",
              "primaryCategory": "CONFIGURATION",
              "categories": [ "CONFIGURATION", "VALIDATION" ],
              "subcategories": [ "INVALID_VALUE", "APP_SETTINGS" ],
              "title": "Invalid configuration value",
              "message": "A configuration value is invalid.",
              "defaultSeverity": "Error",
              "developerHint": "Validate the configured value against the expected type, range, enum value, file path, or URI format.",
              "documentationKey": "when-it-fails/errors/configuration/invalid-configuration-value",
              "tags": [ "CONFIGURATION", "USER_VISIBLE" ]
            },
            {
              "id": "AFW-CFG-0003",
              "code": 200003,
              "name": "ConfigurationFileNotFound",
              "owner": "AFW",
              "codePrefix": "CFG",
              "codeGroup": "CONFIGURATION",
              "primaryCategory": "CONFIGURATION",
              "categories": [ "CONFIGURATION", "FILE_SYSTEM", "STARTUP" ],
              "subcategories": [ "MISSING_FILE" ],
              "title": "Configuration file not found",
              "message": "The expected configuration file was not found.",
              "defaultSeverity": "Error",
              "developerHint": "Check the configured path and whether the file is copied to the output directory or mounted in the runtime environment.",
              "documentationKey": "when-it-fails/errors/configuration/configuration-file-not-found",
              "tags": [ "CONFIGURATION", "FILE_SYSTEM", "STARTUP", "USER_VISIBLE" ]
            },
            {
              "id": "AFW-CFG-0004",
              "code": 200004,
              "name": "ConfigurationSectionMissing",
              "owner": "AFW",
              "codePrefix": "CFG",
              "codeGroup": "CONFIGURATION",
              "primaryCategory": "CONFIGURATION",
              "categories": [ "CONFIGURATION", "STARTUP" ],
              "subcategories": [ "MISSING_SECTION" ],
              "title": "Configuration section missing",
              "message": "A required configuration section is missing.",
              "defaultSeverity": "Error",
              "developerHint": "Check the expected section name and whether options binding points to the right configuration path.",
              "documentationKey": "when-it-fails/errors/configuration/configuration-section-missing",
              "tags": [ "CONFIGURATION", "STARTUP", "USER_VISIBLE" ]
            },
            {
              "id": "AFW-CFG-0005",
              "code": 200005,
              "name": "ConfigurationBindingFailed",
              "owner": "AFW",
              "codePrefix": "CFG",
              "codeGroup": "CONFIGURATION",
              "primaryCategory": "CONFIGURATION",
              "categories": [ "CONFIGURATION", "SERIALIZATION", "STARTUP" ],
              "subcategories": [ "BINDING" ],
              "title": "Configuration binding failed",
              "message": "Configuration could not be bound to the expected options object.",
              "defaultSeverity": "Error",
              "developerHint": "Check property names, value types, nullable requirements, and custom converters.",
              "documentationKey": "when-it-fails/errors/configuration/configuration-binding-failed",
              "tags": [ "CONFIGURATION", "SERIALIZATION", "STARTUP", "USER_VISIBLE" ]
            },
            {
              "id": "AFW-VAL-0001",
              "code": 300001,
              "name": "RequiredValueMissing",
              "owner": "AFW",
              "codePrefix": "VAL",
              "codeGroup": "VALIDATION",
              "primaryCategory": "VALIDATION",
              "categories": [ "VALIDATION" ],
              "subcategories": [ "REQUIRED_VALUE" ],
              "title": "Required value missing",
              "message": "A required value is missing.",
              "defaultSeverity": "Warning",
              "developerHint": "Use for input, command, request, options, or document fields that must be present.",
              "documentationKey": "when-it-fails/errors/validation/required-value-missing",
              "tags": [ "VALIDATION", "USER_VISIBLE" ]
            },
            {
              "id": "AFW-VAL-0002",
              "code": 300002,
              "name": "InvalidValue",
              "owner": "AFW",
              "codePrefix": "VAL",
              "codeGroup": "VALIDATION",
              "primaryCategory": "VALIDATION",
              "categories": [ "VALIDATION" ],
              "subcategories": [ "INVALID_VALUE" ],
              "title": "Invalid value",
              "message": "A provided value is invalid.",
              "defaultSeverity": "Warning",
              "developerHint": "Attach the field name and rejected value in runtime metadata when safe to do so.",
              "documentationKey": "when-it-fails/errors/validation/invalid-value",
              "tags": [ "VALIDATION", "USER_VISIBLE" ]
            },
            {
              "id": "AFW-VAL-0003",
              "code": 300003,
              "name": "ValueOutOfRange",
              "owner": "AFW",
              "codePrefix": "VAL",
              "codeGroup": "VALIDATION",
              "primaryCategory": "VALIDATION",
              "categories": [ "VALIDATION" ],
              "subcategories": [ "RANGE" ],
              "title": "Value out of range",
              "message": "A provided value is outside the allowed range.",
              "defaultSeverity": "Warning",
              "developerHint": "Attach minimum and maximum values in runtime metadata when available.",
              "documentationKey": "when-it-fails/errors/validation/value-out-of-range",
              "tags": [ "VALIDATION", "USER_VISIBLE" ]
            },
            {
              "id": "AFW-VAL-0004",
              "code": 300004,
              "name": "InvalidFormat",
              "owner": "AFW",
              "codePrefix": "VAL",
              "codeGroup": "VALIDATION",
              "primaryCategory": "VALIDATION",
              "categories": [ "VALIDATION", "SERIALIZATION" ],
              "subcategories": [ "FORMAT" ],
              "title": "Invalid format",
              "message": "A provided value has an invalid format.",
              "defaultSeverity": "Warning",
              "developerHint": "Useful for invalid dates, numbers, codes, identifiers, JSON fragments, or URIs.",
              "documentationKey": "when-it-fails/errors/validation/invalid-format",
              "tags": [ "VALIDATION", "FORMAT", "USER_VISIBLE" ]
            },
            {
              "id": "AFW-VAL-0005",
              "code": 300005,
              "name": "DuplicateValue",
              "owner": "AFW",
              "codePrefix": "VAL",
              "codeGroup": "VALIDATION",
              "primaryCategory": "VALIDATION",
              "categories": [ "VALIDATION" ],
              "subcategories": [ "DUPLICATE" ],
              "title": "Duplicate value",
              "message": "A value appears more than once where uniqueness is required.",
              "defaultSeverity": "Warning",
              "developerHint": "Use for duplicate names, aliases, IDs, codes, tags, or user-provided collection items.",
              "documentationKey": "when-it-fails/errors/validation/duplicate-value",
              "tags": [ "VALIDATION", "USER_VISIBLE" ]
            },
            {
              "id": "AFW-VAL-0006",
              "code": 300006,
              "name": "UnknownReference",
              "owner": "AFW",
              "codePrefix": "VAL",
              "codeGroup": "VALIDATION",
              "primaryCategory": "VALIDATION",
              "categories": [ "VALIDATION" ],
              "subcategories": [ "REFERENCE" ],
              "title": "Unknown reference",
              "message": "A value references an item that is not known.",
              "defaultSeverity": "Warning",
              "developerHint": "Use when a category, code group, owner, profile, ID, or foreign key reference cannot be resolved.",
              "documentationKey": "when-it-fails/errors/validation/unknown-reference",
              "tags": [ "VALIDATION", "USER_VISIBLE" ]
            },
            {
              "id": "AFW-IO-0001",
              "code": 400001,
              "name": "FileNotFound",
              "owner": "AFW",
              "codePrefix": "IO",
              "codeGroup": "FILE_SYSTEM",
              "primaryCategory": "FILE_SYSTEM",
              "categories": [ "FILE_SYSTEM" ],
              "subcategories": [ "MISSING_FILE" ],
              "title": "File not found",
              "message": "The requested file was not found.",
              "defaultSeverity": "Error",
              "developerHint": "Check the resolved absolute path and runtime working directory.",
              "documentationKey": "when-it-fails/errors/file-system/file-not-found",
              "tags": [ "FILE_SYSTEM", "USER_VISIBLE" ]
            },
            {
              "id": "AFW-IO-0002",
              "code": 400002,
              "name": "DirectoryNotFound",
              "owner": "AFW",
              "codePrefix": "IO",
              "codeGroup": "FILE_SYSTEM",
              "primaryCategory": "FILE_SYSTEM",
              "categories": [ "FILE_SYSTEM" ],
              "subcategories": [ "MISSING_DIRECTORY" ],
              "title": "Directory not found",
              "message": "The requested directory was not found.",
              "defaultSeverity": "Error",
              "developerHint": "Check whether bootstrap initialization has created the expected workspace folders.",
              "documentationKey": "when-it-fails/errors/file-system/directory-not-found",
              "tags": [ "FILE_SYSTEM", "USER_VISIBLE" ]
            },
            {
              "id": "AFW-IO-0003",
              "code": 400003,
              "name": "FileReadFailed",
              "owner": "AFW",
              "codePrefix": "IO",
              "codeGroup": "FILE_SYSTEM",
              "primaryCategory": "FILE_SYSTEM",
              "categories": [ "FILE_SYSTEM" ],
              "subcategories": [ "READ" ],
              "title": "File read failed",
              "message": "The file could not be read.",
              "defaultSeverity": "Error",
              "developerHint": "Check permissions, file locks, encoding, path length, and whether the file is still present.",
              "documentationKey": "when-it-fails/errors/file-system/file-read-failed",
              "tags": [ "FILE_SYSTEM", "USER_VISIBLE" ]
            },
            {
              "id": "AFW-IO-0004",
              "code": 400004,
              "name": "FileWriteFailed",
              "owner": "AFW",
              "codePrefix": "IO",
              "codeGroup": "FILE_SYSTEM",
              "primaryCategory": "FILE_SYSTEM",
              "categories": [ "FILE_SYSTEM" ],
              "subcategories": [ "WRITE" ],
              "title": "File write failed",
              "message": "The file could not be written.",
              "defaultSeverity": "Error",
              "developerHint": "Check permissions, available disk space, target folder existence, safe-write settings, and file locks.",
              "documentationKey": "when-it-fails/errors/file-system/file-write-failed",
              "tags": [ "FILE_SYSTEM", "USER_VISIBLE" ]
            },
            {
              "id": "AFW-IO-0005",
              "code": 400005,
              "name": "JsonsWorkspaceUnavailable",
              "owner": "AFW",
              "codePrefix": "IO",
              "codeGroup": "FILE_SYSTEM",
              "primaryCategory": "FILE_SYSTEM",
              "categories": [ "FILE_SYSTEM", "CONFIGURATION", "STARTUP" ],
              "subcategories": [ "WORKSPACE" ],
              "title": "JSON workspace unavailable",
              "message": "The project-local JSON workspace is unavailable.",
              "defaultSeverity": "Error",
              "developerHint": "Check JsonsOptions.RootPath, directory permissions, application identity, and deployment layout.",
              "documentationKey": "when-it-fails/errors/file-system/jsons-workspace-unavailable",
              "tags": [ "FILE_SYSTEM", "CONFIGURATION", "STARTUP", "USER_VISIBLE" ]
            },
            {
              "id": "AFW-SEC-0001",
              "code": 500001,
              "name": "Unauthorized",
              "owner": "AFW",
              "codePrefix": "SEC",
              "codeGroup": "SECURITY",
              "primaryCategory": "SECURITY",
              "categories": [ "SECURITY" ],
              "subcategories": [ "AUTHENTICATION" ],
              "title": "Unauthorized",
              "message": "Authentication is required to access this resource.",
              "defaultSeverity": "Warning",
              "developerHint": "For web profiles this usually maps to HTTP 401.",
              "documentationKey": "when-it-fails/errors/security/unauthorized",
              "tags": [ "SECURITY", "USER_VISIBLE" ]
            },
            {
              "id": "AFW-SEC-0002",
              "code": 500002,
              "name": "Forbidden",
              "owner": "AFW",
              "codePrefix": "SEC",
              "codeGroup": "SECURITY",
              "primaryCategory": "SECURITY",
              "categories": [ "SECURITY" ],
              "subcategories": [ "AUTHORIZATION" ],
              "title": "Forbidden",
              "message": "You do not have permission to access this resource.",
              "defaultSeverity": "Warning",
              "developerHint": "For web profiles this usually maps to HTTP 403.",
              "documentationKey": "when-it-fails/errors/security/forbidden",
              "tags": [ "SECURITY", "USER_VISIBLE" ]
            },
            {
              "id": "AFW-SEC-0003",
              "code": 500003,
              "name": "AuthenticationFailed",
              "owner": "AFW",
              "codePrefix": "SEC",
              "codeGroup": "SECURITY",
              "primaryCategory": "SECURITY",
              "categories": [ "SECURITY" ],
              "subcategories": [ "AUTHENTICATION" ],
              "title": "Authentication failed",
              "message": "Authentication failed.",
              "defaultSeverity": "Warning",
              "developerHint": "Avoid exposing sensitive authentication details in user-facing messages.",
              "documentationKey": "when-it-fails/errors/security/authentication-failed",
              "tags": [ "SECURITY", "USER_VISIBLE" ]
            },
            {
              "id": "AFW-SEC-0004",
              "code": 500004,
              "name": "TokenExpired",
              "owner": "AFW",
              "codePrefix": "SEC",
              "codeGroup": "SECURITY",
              "primaryCategory": "SECURITY",
              "categories": [ "SECURITY" ],
              "subcategories": [ "TOKEN" ],
              "title": "Token expired",
              "message": "The security token has expired.",
              "defaultSeverity": "Warning",
              "developerHint": "Refresh the token or require the user to sign in again.",
              "documentationKey": "when-it-fails/errors/security/token-expired",
              "tags": [ "SECURITY", "USER_VISIBLE" ]
            },
            {
              "id": "AFW-NET-0001",
              "code": 600001,
              "name": "NetworkUnavailable",
              "owner": "AFW",
              "codePrefix": "NET",
              "codeGroup": "NETWORK",
              "primaryCategory": "NETWORK",
              "categories": [ "NETWORK" ],
              "subcategories": [ "CONNECTIVITY" ],
              "title": "Network unavailable",
              "message": "The network is unavailable.",
              "defaultSeverity": "Error",
              "developerHint": "Check connectivity, DNS, firewall, proxy, VPN, and host availability.",
              "documentationKey": "when-it-fails/errors/network/network-unavailable",
              "tags": [ "NETWORK", "USER_VISIBLE" ]
            },
            {
              "id": "AFW-NET-0002",
              "code": 600002,
              "name": "HttpRequestFailed",
              "owner": "AFW",
              "codePrefix": "NET",
              "codeGroup": "NETWORK",
              "primaryCategory": "NETWORK",
              "categories": [ "NETWORK", "EXTERNAL_SERVICE" ],
              "subcategories": [ "HTTP" ],
              "title": "HTTP request failed",
              "message": "The HTTP request failed.",
              "defaultSeverity": "Error",
              "developerHint": "Attach status code, method, endpoint name, correlation ID, and retry information when safe.",
              "documentationKey": "when-it-fails/errors/network/http-request-failed",
              "tags": [ "NETWORK", "HTTP", "USER_VISIBLE" ]
            },
            {
              "id": "AFW-NET-0003",
              "code": 600003,
              "name": "RemoteTimeout",
              "owner": "AFW",
              "codePrefix": "NET",
              "codeGroup": "NETWORK",
              "primaryCategory": "NETWORK",
              "categories": [ "NETWORK", "EXTERNAL_SERVICE" ],
              "subcategories": [ "TIMEOUT" ],
              "title": "Remote operation timed out",
              "message": "The remote operation timed out.",
              "defaultSeverity": "Error",
              "developerHint": "Check timeout configuration, retry policy, remote service health, and network latency.",
              "documentationKey": "when-it-fails/errors/network/remote-timeout",
              "tags": [ "NETWORK", "TIMEOUT", "RETRYABLE", "USER_VISIBLE" ]
            },
            {
              "id": "AFW-NET-0004",
              "code": 600004,
              "name": "DnsLookupFailed",
              "owner": "AFW",
              "codePrefix": "NET",
              "codeGroup": "NETWORK",
              "primaryCategory": "NETWORK",
              "categories": [ "NETWORK" ],
              "subcategories": [ "DNS" ],
              "title": "DNS lookup failed",
              "message": "The host name could not be resolved.",
              "defaultSeverity": "Error",
              "developerHint": "Check DNS servers, split-DNS rules, search suffixes, VPN routes, and public/private zone configuration.",
              "documentationKey": "when-it-fails/errors/network/dns-lookup-failed",
              "tags": [ "NETWORK", "DNS", "USER_VISIBLE" ]
            },
            {
              "id": "AFW-DB-0001",
              "code": 700001,
              "name": "DatabaseUnavailable",
              "owner": "AFW",
              "codePrefix": "DB",
              "codeGroup": "DATABASE",
              "primaryCategory": "DATABASE",
              "categories": [ "DATABASE", "EXTERNAL_SERVICE" ],
              "subcategories": [ "CONNECTIVITY" ],
              "title": "Database unavailable",
              "message": "The database is unavailable.",
              "defaultSeverity": "Error",
              "developerHint": "Check connection string, credentials, network reachability, server health, migrations, and pool exhaustion.",
              "documentationKey": "when-it-fails/errors/database/database-unavailable",
              "tags": [ "DATABASE", "USER_VISIBLE" ]
            },
            {
              "id": "AFW-DB-0002",
              "code": 700002,
              "name": "DatabaseQueryFailed",
              "owner": "AFW",
              "codePrefix": "DB",
              "codeGroup": "DATABASE",
              "primaryCategory": "DATABASE",
              "categories": [ "DATABASE" ],
              "subcategories": [ "QUERY" ],
              "title": "Database query failed",
              "message": "A database query failed.",
              "defaultSeverity": "Error",
              "developerHint": "Attach query name, operation type, and provider-specific error code when safe. Do not expose raw SQL to users.",
              "documentationKey": "when-it-fails/errors/database/database-query-failed",
              "tags": [ "DATABASE", "INTERNAL_ONLY" ]
            },
            {
              "id": "AFW-DB-0003",
              "code": 700003,
              "name": "EntityNotFound",
              "owner": "AFW",
              "codePrefix": "DB",
              "codeGroup": "DATABASE",
              "primaryCategory": "DATABASE",
              "categories": [ "DATABASE", "VALIDATION" ],
              "subcategories": [ "NOT_FOUND" ],
              "title": "Entity not found",
              "message": "The requested item was not found.",
              "defaultSeverity": "Warning",
              "developerHint": "Use for missing records or aggregate roots. For web profiles this often maps to HTTP 404.",
              "documentationKey": "when-it-fails/errors/database/entity-not-found",
              "tags": [ "DATABASE", "USER_VISIBLE" ]
            },
            {
              "id": "AFW-DB-0004",
              "code": 700004,
              "name": "ConcurrencyConflict",
              "owner": "AFW",
              "codePrefix": "DB",
              "codeGroup": "DATABASE",
              "primaryCategory": "DATABASE",
              "categories": [ "DATABASE", "VALIDATION" ],
              "subcategories": [ "CONCURRENCY" ],
              "title": "Concurrency conflict",
              "message": "The item was changed by another operation.",
              "defaultSeverity": "Warning",
              "developerHint": "Reload the entity, merge changes, or ask the user to retry with the latest version.",
              "documentationKey": "when-it-fails/errors/database/concurrency-conflict",
              "tags": [ "DATABASE", "CONCURRENCY", "USER_VISIBLE" ]
            },
            {
              "id": "AFW-EXT-0001",
              "code": 800001,
              "name": "ExternalServiceUnavailable",
              "owner": "AFW",
              "codePrefix": "EXT",
              "codeGroup": "EXTERNAL_SERVICE",
              "primaryCategory": "EXTERNAL_SERVICE",
              "categories": [ "EXTERNAL_SERVICE", "NETWORK" ],
              "subcategories": [ "SERVICE_UNAVAILABLE" ],
              "title": "External service unavailable",
              "message": "An external service is unavailable.",
              "defaultSeverity": "Error",
              "developerHint": "Attach endpoint name, provider name, retry policy, and fallback information when safe.",
              "documentationKey": "when-it-fails/errors/external-service/external-service-unavailable",
              "tags": [ "EXTERNAL_SERVICE", "RETRYABLE", "USER_VISIBLE" ]
            },
            {
              "id": "AFW-EXT-0002",
              "code": 800002,
              "name": "ExternalServiceReturnedInvalidData",
              "owner": "AFW",
              "codePrefix": "EXT",
              "codeGroup": "EXTERNAL_SERVICE",
              "primaryCategory": "EXTERNAL_SERVICE",
              "categories": [ "EXTERNAL_SERVICE", "SERIALIZATION" ],
              "subcategories": [ "INVALID_RESPONSE" ],
              "title": "External service returned invalid data",
              "message": "An external service returned data that could not be used.",
              "defaultSeverity": "Error",
              "developerHint": "Store a sanitized response sample or schema mismatch summary in diagnostics, not in user-facing text.",
              "documentationKey": "when-it-fails/errors/external-service/external-service-returned-invalid-data",
              "tags": [ "EXTERNAL_SERVICE", "SERIALIZATION", "INTERNAL_ONLY" ]
            },
            {
              "id": "AFW-FMT-0001",
              "code": 900001,
              "name": "SerializationFailed",
              "owner": "AFW",
              "codePrefix": "FMT",
              "codeGroup": "SERIALIZATION",
              "primaryCategory": "SERIALIZATION",
              "categories": [ "SERIALIZATION" ],
              "subcategories": [ "SERIALIZE" ],
              "title": "Serialization failed",
              "message": "The value could not be serialized.",
              "defaultSeverity": "Error",
              "developerHint": "Check unsupported types, cyclic references, converters, naming policy, and nullable values.",
              "documentationKey": "when-it-fails/errors/serialization/serialization-failed",
              "tags": [ "SERIALIZATION", "INTERNAL_ONLY" ]
            },
            {
              "id": "AFW-FMT-0002",
              "code": 900002,
              "name": "DeserializationFailed",
              "owner": "AFW",
              "codePrefix": "FMT",
              "codeGroup": "SERIALIZATION",
              "primaryCategory": "SERIALIZATION",
              "categories": [ "SERIALIZATION", "VALIDATION" ],
              "subcategories": [ "DESERIALIZE" ],
              "title": "Deserialization failed",
              "message": "The value could not be deserialized.",
              "defaultSeverity": "Error",
              "developerHint": "Check JSON shape, property names, converters, required properties, and schema version.",
              "documentationKey": "when-it-fails/errors/serialization/deserialization-failed",
              "tags": [ "SERIALIZATION", "VALIDATION", "USER_VISIBLE" ]
            },
            {
              "id": "AFW-FMT-0003",
              "code": 900003,
              "name": "InvalidJsonDocument",
              "owner": "AFW",
              "codePrefix": "FMT",
              "codeGroup": "SERIALIZATION",
              "primaryCategory": "SERIALIZATION",
              "categories": [ "SERIALIZATION", "VALIDATION" ],
              "subcategories": [ "JSON" ],
              "title": "Invalid JSON document",
              "message": "The JSON document is invalid.",
              "defaultSeverity": "Warning",
              "developerHint": "Report the JSON path, line number, byte position, and schema version when available.",
              "documentationKey": "when-it-fails/errors/serialization/invalid-json-document",
              "tags": [ "SERIALIZATION", "VALIDATION", "JSON", "USER_VISIBLE" ]
            }
          ]
        }
        """;
    }

    private static string CreateCategoryCatalogJson()
    {
        return """
        {
          "schemaVersion": "1.0",
          "catalogId": "afw.when-it-fails.categories",
          "catalogName": "Afrowave WhenItFails default category catalog",
          "description": "Default category catalog template for project-local customization.",
          "language": "en",
          "sourceCatalogId": "afw.when-it-fails.categories",
          "sourceCatalogVersion": "1.0",
          "isShadowCopy": true,
          "tags": [ "default", "categories", "shadow-copy-template" ],
          "categories": [
            {
              "name": "GENERAL",
              "displayName": "General",
              "description": "General or fallback errors that do not fit a more specific category.",
              "aliases": [ "COMMON", "BASIC" ],
              "parentCategories": [],
              "defaultTags": [ "GENERAL" ],
              "defaultMappings": {}
            },
            {
              "name": "CONFIGURATION",
              "displayName": "Configuration",
              "description": "Errors related to configuration values, configuration files, and application setup.",
              "aliases": [ "SETTINGS", "OPTIONS" ],
              "parentCategories": [],
              "defaultTags": [ "CONFIGURATION" ],
              "defaultMappings": {
                "defaultSeverity": "Error"
              }
            },
            {
              "name": "VALIDATION",
              "displayName": "Validation",
              "description": "Errors related to invalid, missing, unsupported, duplicate, or inconsistent input values.",
              "aliases": [ "INPUT", "BAD_REQUEST" ],
              "parentCategories": [],
              "defaultTags": [ "VALIDATION", "USER_VISIBLE" ],
              "defaultMappings": {
                "defaultSeverity": "Warning",
                "web.httpStatusCode": "400"
              }
            },
            {
              "name": "STARTUP",
              "displayName": "Startup",
              "description": "Errors that usually happen while an application is starting or initializing.",
              "aliases": [ "INITIALIZATION", "BOOTSTRAP" ],
              "parentCategories": [],
              "defaultTags": [ "STARTUP" ],
              "defaultMappings": {
                "defaultSeverity": "Error"
              }
            },
            {
              "name": "FILE_SYSTEM",
              "displayName": "File system",
              "description": "Errors related to files, directories, paths, permissions, and project-local workspaces.",
              "aliases": [ "FILES", "IO", "STORAGE" ],
              "parentCategories": [],
              "defaultTags": [ "FILE_SYSTEM" ],
              "defaultMappings": {
                "defaultSeverity": "Error"
              }
            },
            {
              "name": "SECURITY",
              "displayName": "Security",
              "description": "Errors related to authentication, authorization, tokens, secrets, and permission checks.",
              "aliases": [ "AUTH", "AUTHENTICATION", "AUTHORIZATION" ],
              "parentCategories": [],
              "defaultTags": [ "SECURITY" ],
              "defaultMappings": {
                "defaultSeverity": "Warning"
              }
            },
            {
              "name": "NETWORK",
              "displayName": "Network",
              "description": "Errors related to connectivity, DNS, HTTP, proxies, VPN, timeouts, and remote endpoints.",
              "aliases": [ "NETWORKING", "COMMUNICATION", "HTTP" ],
              "parentCategories": [],
              "defaultTags": [ "NETWORK" ],
              "defaultMappings": {
                "defaultSeverity": "Error",
                "web.httpStatusCode": "503"
              }
            },
            {
              "name": "DATABASE",
              "displayName": "Database",
              "description": "Errors related to database connectivity, queries, records, migrations, and concurrency.",
              "aliases": [ "DATA", "PERSISTENCE", "DB" ],
              "parentCategories": [],
              "defaultTags": [ "DATABASE" ],
              "defaultMappings": {
                "defaultSeverity": "Error"
              }
            },
            {
              "name": "EXTERNAL_SERVICE",
              "displayName": "External service",
              "description": "Errors related to third-party services, remote APIs, gateways, and provider integrations.",
              "aliases": [ "REMOTE_SERVICE", "THIRD_PARTY", "INTEGRATION" ],
              "parentCategories": [ "NETWORK" ],
              "defaultTags": [ "EXTERNAL_SERVICE" ],
              "defaultMappings": {
                "defaultSeverity": "Error",
                "web.httpStatusCode": "502"
              }
            },
            {
              "name": "SERIALIZATION",
              "displayName": "Serialization",
              "description": "Errors related to JSON, serialization, deserialization, schema versions, and data formats.",
              "aliases": [ "FORMAT", "JSON", "PARSING" ],
              "parentCategories": [ "VALIDATION" ],
              "defaultTags": [ "SERIALIZATION" ],
              "defaultMappings": {
                "defaultSeverity": "Error"
              }
            }
          ]
        }
        """;
    }

    private static string CreateCodeGroupCatalogJson()
    {
        return """
        {
          "schemaVersion": "1.0",
          "catalogId": "afw.when-it-fails.code-groups",
          "catalogName": "Afrowave WhenItFails default code group catalog",
          "description": "Default code group catalog template for project-local customization.",
          "language": "en",
          "sourceCatalogId": "afw.when-it-fails.code-groups",
          "sourceCatalogVersion": "1.0",
          "isShadowCopy": true,
          "tags": [ "default", "code-groups", "shadow-copy-template" ],
          "codeGroups": [
            {
              "name": "GENERAL",
              "displayName": "General",
              "codePrefix": "GEN",
              "codeFrom": 100000,
              "codeTo": 199999,
              "description": "General and fallback errors.",
              "defaultCategories": [ "GENERAL" ],
              "defaultTags": [ "GENERAL" ],
              "defaultMappings": {}
            },
            {
              "name": "CONFIGURATION",
              "displayName": "Configuration",
              "codePrefix": "CFG",
              "codeFrom": 200000,
              "codeTo": 299999,
              "description": "Configuration-related errors.",
              "defaultCategories": [ "CONFIGURATION" ],
              "defaultTags": [ "CONFIGURATION" ],
              "defaultMappings": {}
            },
            {
              "name": "VALIDATION",
              "displayName": "Validation",
              "codePrefix": "VAL",
              "codeFrom": 300000,
              "codeTo": 399999,
              "description": "Validation and input errors.",
              "defaultCategories": [ "VALIDATION" ],
              "defaultTags": [ "VALIDATION", "USER_VISIBLE" ],
              "defaultMappings": {}
            },
            {
              "name": "FILE_SYSTEM",
              "displayName": "File system",
              "codePrefix": "IO",
              "codeFrom": 400000,
              "codeTo": 499999,
              "description": "File system, path, storage, and workspace errors.",
              "defaultCategories": [ "FILE_SYSTEM" ],
              "defaultTags": [ "FILE_SYSTEM" ],
              "defaultMappings": {}
            },
            {
              "name": "SECURITY",
              "displayName": "Security",
              "codePrefix": "SEC",
              "codeFrom": 500000,
              "codeTo": 599999,
              "description": "Authentication, authorization, token, and permission errors.",
              "defaultCategories": [ "SECURITY" ],
              "defaultTags": [ "SECURITY" ],
              "defaultMappings": {}
            },
            {
              "name": "NETWORK",
              "displayName": "Network",
              "codePrefix": "NET",
              "codeFrom": 600000,
              "codeTo": 699999,
              "description": "Network, HTTP, DNS, connectivity, and timeout errors.",
              "defaultCategories": [ "NETWORK" ],
              "defaultTags": [ "NETWORK" ],
              "defaultMappings": {}
            },
            {
              "name": "DATABASE",
              "displayName": "Database",
              "codePrefix": "DB",
              "codeFrom": 700000,
              "codeTo": 799999,
              "description": "Database, persistence, query, and concurrency errors.",
              "defaultCategories": [ "DATABASE" ],
              "defaultTags": [ "DATABASE" ],
              "defaultMappings": {}
            },
            {
              "name": "EXTERNAL_SERVICE",
              "displayName": "External service",
              "codePrefix": "EXT",
              "codeFrom": 800000,
              "codeTo": 899999,
              "description": "Third-party service, provider, gateway, and integration errors.",
              "defaultCategories": [ "EXTERNAL_SERVICE" ],
              "defaultTags": [ "EXTERNAL_SERVICE" ],
              "defaultMappings": {}
            },
            {
              "name": "SERIALIZATION",
              "displayName": "Serialization",
              "codePrefix": "FMT",
              "codeFrom": 900000,
              "codeTo": 999999,
              "description": "Serialization, deserialization, JSON, schema, and data format errors.",
              "defaultCategories": [ "SERIALIZATION" ],
              "defaultTags": [ "SERIALIZATION" ],
              "defaultMappings": {}
            }
          ]
        }
        """;
    }

    private static string CreateOwnerCatalogJson()
    {
        return """
        {
          "schemaVersion": "1.0",
          "catalogId": "afw.when-it-fails.owners",
          "catalogName": "Afrowave WhenItFails default owner catalog",
          "description": "Default owner catalog template for project-local customization.",
          "language": "en",
          "sourceCatalogId": "afw.when-it-fails.owners",
          "sourceCatalogVersion": "1.0",
          "isShadowCopy": true,
          "tags": [ "default", "owners", "shadow-copy-template" ],
          "owners": [
            {
              "name": "AFW",
              "displayName": "Afrowave",
              "description": "Built-in Afrowave error definitions.",
              "codeFrom": 0,
              "codeTo": 999999,
              "isBuiltIn": true,
              "aliases": [ "AFROWAVE", "AFROWAVE_TOOLBOX" ],
              "defaultMappings": {
                "catalogRole": "built-in",
                "editable": "false"
              }
            },
            {
              "name": "APP",
              "displayName": "Application",
              "description": "Project-local application error definitions.",
              "codeFrom": 1000000,
              "codeTo": 1999999,
              "isBuiltIn": false,
              "aliases": [ "APPLICATION", "PROJECT" ],
              "defaultMappings": {
                "catalogRole": "application",
                "editable": "true"
              }
            },
            {
              "name": "PLUGIN",
              "displayName": "Plugin",
              "description": "Error definitions owned by optional plugins or extension packages.",
              "codeFrom": 2000000,
              "codeTo": 2999999,
              "isBuiltIn": false,
              "aliases": [ "EXTENSION", "ADDON" ],
              "defaultMappings": {
                "catalogRole": "plugin",
                "editable": "true"
              }
            },
            {
              "name": "USER",
              "displayName": "User",
              "description": "User-defined local error definitions, experiments, and temporary project-specific additions.",
              "codeFrom": 9000000,
              "codeTo": 9999999,
              "isBuiltIn": false,
              "aliases": [ "CUSTOM", "LOCAL" ],
              "defaultMappings": {
                "catalogRole": "user",
                "editable": "true"
              }
            }
          ]
        }
        """;
    }

    private static string CreateProfilesJson()
    {
        return """
        {
          "schemaVersion": "1.0",
          "catalogId": "afw.when-it-fails.profiles",
          "catalogName": "Afrowave WhenItFails default profiles",
          "description": "Default profile template for project-local customization.",
          "language": "en",
          "sourceCatalogId": "afw.when-it-fails.profiles",
          "sourceCatalogVersion": "1.0",
          "isShadowCopy": true,
          "tags": [ "default", "profiles", "shadow-copy-template" ],
          "profiles": [
            {
              "name": "WEB",
              "displayName": "Web",
              "description": "Default profile for web applications with user-safe error presentation.",
              "includeOwners": [ "AFW", "APP", "PLUGIN", "USER" ],
              "includeCodeGroups": [],
              "includeCategories": [ "GENERAL", "CONFIGURATION", "VALIDATION", "FILE_SYSTEM", "SECURITY", "NETWORK", "DATABASE", "EXTERNAL_SERVICE", "SERIALIZATION" ],
              "includeSubcategories": [],
              "includeTags": [ "USER_VISIBLE" ],
              "excludeTags": [ "INTERNAL_ONLY", "DEBUG_ONLY" ],
              "defaultMappings": {
                "web.problemDetails": "true",
                "web.includeTraceId": "true",
                "web.includeExceptionDetails": "false",
                "web.includeStackTrace": "false"
              }
            },
            {
              "name": "API",
              "displayName": "API",
              "description": "Default profile for HTTP APIs and service endpoints.",
              "includeOwners": [ "AFW", "APP", "PLUGIN", "USER" ],
              "includeCodeGroups": [],
              "includeCategories": [ "GENERAL", "CONFIGURATION", "VALIDATION", "SECURITY", "NETWORK", "DATABASE", "EXTERNAL_SERVICE", "SERIALIZATION" ],
              "includeSubcategories": [],
              "includeTags": [ "USER_VISIBLE" ],
              "excludeTags": [ "INTERNAL_ONLY", "DEBUG_ONLY" ],
              "defaultMappings": {
                "web.problemDetails": "true",
                "web.includeTraceId": "true",
                "web.includeExceptionDetails": "false",
                "web.includeStackTrace": "false"
              }
            },
            {
              "name": "CLI",
              "displayName": "CLI",
              "description": "Default profile for command-line applications.",
              "includeOwners": [ "AFW", "APP", "PLUGIN", "USER" ],
              "includeCodeGroups": [],
              "includeCategories": [],
              "includeSubcategories": [],
              "includeTags": [],
              "excludeTags": [],
              "defaultMappings": {
                "cli.includeHints": "true",
                "cli.includeExitCode": "true",
                "cli.includeColors": "true"
              }
            },
            {
              "name": "DESKTOP",
              "displayName": "Desktop",
              "description": "Default profile for desktop applications.",
              "includeOwners": [ "AFW", "APP", "PLUGIN", "USER" ],
              "includeCodeGroups": [],
              "includeCategories": [ "GENERAL", "CONFIGURATION", "VALIDATION", "FILE_SYSTEM", "SECURITY", "NETWORK", "DATABASE", "EXTERNAL_SERVICE", "SERIALIZATION" ],
              "includeSubcategories": [],
              "includeTags": [ "USER_VISIBLE" ],
              "excludeTags": [ "INTERNAL_ONLY", "DEBUG_ONLY" ],
              "defaultMappings": {
                "desktop.showDialog": "true",
                "desktop.includeDetailsButton": "true",
                "desktop.includeExceptionDetails": "false"
              }
            },
            {
              "name": "SERVICE",
              "displayName": "Service",
              "description": "Default profile for background services, workers, daemons, scheduled jobs, and hosted services.",
              "includeOwners": [ "AFW", "APP", "PLUGIN", "USER" ],
              "includeCodeGroups": [],
              "includeCategories": [],
              "includeSubcategories": [],
              "includeTags": [],
              "excludeTags": [ "DEBUG_ONLY" ],
              "defaultMappings": {
                "service.includeTraceId": "true",
                "service.includeRetryInformation": "true",
                "service.includeExceptionDetails": "false"
              }
            },
            {
              "name": "DEVELOPMENT",
              "displayName": "Development",
              "description": "Profile intended for development and debugging.",
              "includeOwners": [ "AFW", "APP", "PLUGIN", "USER" ],
              "includeCodeGroups": [],
              "includeCategories": [],
              "includeSubcategories": [],
              "includeTags": [],
              "excludeTags": [],
              "defaultMappings": {
                "development.includeExceptionDetails": "true",
                "development.includeStackTrace": "true",
                "development.includeCatalogDiagnostics": "true"
              }
            },
            {
              "name": "PRODUCTION",
              "displayName": "Production",
              "description": "Profile intended for production-safe error presentation.",
              "includeOwners": [ "AFW", "APP", "PLUGIN", "USER" ],
              "includeCodeGroups": [],
              "includeCategories": [],
              "includeSubcategories": [],
              "includeTags": [],
              "excludeTags": [ "INTERNAL_ONLY", "DEBUG_ONLY" ],
              "defaultMappings": {
                "production.includeExceptionDetails": "false",
                "production.includeStackTrace": "false",
                "production.includeSensitiveMetadata": "false"
              }
            }
          ]
        }
        """;
    }
}
