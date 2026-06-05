using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Bootstrap;

/// <summary>
/// Provides default JSON template files used to initialize the project-local WhenItFails workspace.
/// </summary>
/// <remarks>
/// This provider currently contains small built-in templates directly in code.
/// Later it can be replaced or extended with embedded resource loading without changing
/// the bootstrapper contract.
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
          "tags": [ "default", "errors" ],
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
              "subcategories": [],
              "title": "Unknown error",
              "message": "An unknown error occurred.",
              "defaultSeverity": "Error",
              "tags": [ "general", "fallback" ]
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
              "tags": [ "configuration", "startup", "user-visible" ]
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
          "tags": [ "default", "categories" ],
          "categories": [
            {
              "name": "GENERAL",
              "displayName": "General",
              "description": "General or fallback errors.",
              "aliases": [ "COMMON" ],
              "parentCategories": [],
              "defaultTags": [ "GENERAL" ],
              "defaultMappings": {}
            },
            {
              "name": "CONFIGURATION",
              "displayName": "Configuration",
              "description": "Errors related to configuration values, configuration files, and application setup.",
              "aliases": [ "SETTINGS" ],
              "parentCategories": [],
              "defaultTags": [ "CONFIGURATION" ],
              "defaultMappings": {
                "defaultSeverity": "Error"
              }
            },
            {
              "name": "VALIDATION",
              "displayName": "Validation",
              "description": "Errors related to invalid or missing input values.",
              "aliases": [ "INPUT" ],
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
              "description": "Errors that usually happen while an application is starting.",
              "aliases": [],
              "parentCategories": [],
              "defaultTags": [ "STARTUP" ],
              "defaultMappings": {}
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
          "tags": [ "default", "code-groups" ],
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
          "tags": [ "default", "owners" ],
          "owners": [
            {
              "name": "AFW",
              "displayName": "Afrowave",
              "description": "Built-in Afrowave error definitions.",
              "codeFrom": 0,
              "codeTo": 999999,
              "isBuiltIn": true,
              "aliases": [ "AFROWAVE" ],
              "defaultMappings": {
                "catalogRole": "built-in"
              }
            },
            {
              "name": "APP",
              "displayName": "Application",
              "description": "Project-local application error definitions.",
              "codeFrom": 1000000,
              "codeTo": 1999999,
              "isBuiltIn": false,
              "aliases": [ "APPLICATION" ],
              "defaultMappings": {
                "catalogRole": "application"
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
      "tags": [ "default", "profiles" ],
      "profiles": [
        {
          "name": "WEB",
          "displayName": "Web",
          "description": "Default profile for web applications.",
          "includeOwners": [ "AFW", "APP" ],
          "includeCodeGroups": [],
          "includeCategories": [ "GENERAL", "CONFIGURATION", "VALIDATION" ],
          "includeSubcategories": [],
          "includeTags": [ "USER_VISIBLE" ],
          "excludeTags": [ "INTERNAL_ONLY" ],
          "defaultMappings": {
            "web.problemDetails": "true",
            "web.includeTraceId": "true",
            "web.includeExceptionDetails": "false"
          }
        },
        {
          "name": "API",
          "displayName": "API",
          "description": "Default profile for HTTP APIs and service endpoints.",
          "includeOwners": [ "AFW", "APP" ],
          "includeCodeGroups": [],
          "includeCategories": [ "GENERAL", "CONFIGURATION", "VALIDATION" ],
          "includeSubcategories": [],
          "includeTags": [ "USER_VISIBLE" ],
          "excludeTags": [ "INTERNAL_ONLY" ],
          "defaultMappings": {
            "web.problemDetails": "true",
            "web.includeTraceId": "true",
            "web.includeExceptionDetails": "false"
          }
        },
        {
          "name": "CLI",
          "displayName": "CLI",
          "description": "Default profile for command-line applications.",
          "includeOwners": [ "AFW", "APP" ],
          "includeCodeGroups": [],
          "includeCategories": [ "GENERAL", "CONFIGURATION", "VALIDATION" ],
          "includeSubcategories": [],
          "includeTags": [],
          "excludeTags": [],
          "defaultMappings": {
            "cli.includeHints": "true",
            "cli.includeExitCode": "true"
          }
        },
        {
          "name": "DESKTOP",
          "displayName": "Desktop",
          "description": "Default profile for desktop applications.",
          "includeOwners": [ "AFW", "APP" ],
          "includeCodeGroups": [],
          "includeCategories": [ "GENERAL", "CONFIGURATION", "VALIDATION" ],
          "includeSubcategories": [],
          "includeTags": [ "USER_VISIBLE" ],
          "excludeTags": [ "INTERNAL_ONLY" ],
          "defaultMappings": {
            "desktop.showDialog": "true",
            "desktop.includeDetailsButton": "true"
          }
        },
        {
          "name": "DEVELOPMENT",
          "displayName": "Development",
          "description": "Profile intended for development and debugging.",
          "includeOwners": [ "AFW", "APP" ],
          "includeCodeGroups": [],
          "includeCategories": [],
          "includeSubcategories": [],
          "includeTags": [],
          "excludeTags": [],
          "defaultMappings": {
            "development.includeExceptionDetails": "true",
            "development.includeStackTrace": "true"
          }
        },
        {
          "name": "PRODUCTION",
          "displayName": "Production",
          "description": "Profile intended for production-safe error presentation.",
          "includeOwners": [ "AFW", "APP" ],
          "includeCodeGroups": [],
          "includeCategories": [],
          "includeSubcategories": [],
          "includeTags": [],
          "excludeTags": [ "INTERNAL_ONLY", "DEBUG_ONLY" ],
          "defaultMappings": {
            "production.includeExceptionDetails": "false",
            "production.includeStackTrace": "false"
          }
        }
      ]
    }
    """;
    }
}