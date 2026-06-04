# WhenItFails Philosophy

## Purpose

`Afrowave.Toolbox.WhenItFails` is not designed to make error handling heavier.

It is designed to keep everyday application code simple by moving error knowledge into reusable catalogs, presets, profiles, and tooling.

In normal application code, developers should not manually construct large error objects. Most of the time, they should use a small and readable call such as:

```csharp
errorFactory.FireError("InvalidInput", "Name is required.");
```

or, in the most minimal case:

```csharp
errorFactory.FireError();
```

The rich error model exists behind the scenes so that errors can be identified, searched, documented, filtered, translated, logged, mapped to responses, and handled consistently across different kinds of applications.

---

## Main Idea

Application code should describe what happened.

The error catalog should describe what the error means.

For example, application code may say:

```csharp
errorFactory.FireError(
    type: "MissingConfigurationValue",
    message: "The key 'LibreTranslate:DefaultServer' is missing.");
```

The catalog can provide the rest:

- stable error ID
- numeric error code
- owner
- code prefix
- code group
- primary category
- additional categories
- subcategories
- severity
- tags
- documentation key
- developer hint
- metadata
- optional response mapping information

This means the developer does not need to repeatedly write the same error metadata throughout the application.

---

## Why the Model Looks Rich

At first glance, the error model may look larger than a simple `string errorMessage`.

That is intentional, but it is not meant to make common usage harder.

The full model exists because real applications often need more than a message:

- support needs stable error IDs
- logs need searchable codes
- APIs need consistent response mapping
- web applications need safe user-facing errors
- desktop applications need exportable texts for translation
- documentation needs a list of known errors
- diagnostics need context and metadata
- profiles need filtering by categories, tags, and scenarios
- future tools need structured data instead of scattered strings

The complexity is stored in catalogs and presets, not in everyday business code.

---

## Simple Use First

The most important rule of `WhenItFails` is:

> Common usage must stay simple.

A developer should be able to start with:

```csharp
errorFactory.FireError();
```

Then move to:

```csharp
errorFactory.FireError("InvalidInput");
```

Then, only when needed:

```csharp
errorFactory.FireError(
    type: "InvalidInput",
    message: "Name is required.");
```

And for advanced scenarios:

```csharp
errorFactory.FireError(
    type: "MissingConfigurationValue",
    configure: options =>
    {
        options.Message = "The translation server is not configured.";
        options.Detail = "Missing key: LibreTranslate:DefaultServer";
        options.OperationName = "LoadTranslationSettings";
        options.SourceName = "appsettings.json";
    });
```

The full model should be available, but not forced.

---

## Definitions, Presets, and Descriptors

`WhenItFails` separates three important concepts.

### ErrorDefinition

An `ErrorDefinition` describes a known error type in a catalog.

It is usually loaded from JSON.

It is not something developers should normally create manually inside application logic.

Example:

```json
{
  "id": "AFW-CFG-0001",
  "code": 200001,
  "name": "MissingConfigurationValue",
  "owner": "Afrowave",
  "codePrefix": "CFG",
  "codeGroup": "Configuration",
  "primaryCategory": "Configuration",
  "categories": [ "Configuration", "Startup", "Validation" ],
  "subcategories": [ "RequiredValue", "AppSettings" ],
  "title": "Missing configuration value",
  "message": "A required configuration value is missing.",
  "defaultSeverity": "Error",
  "tags": [ "configuration", "startup", "user-visible" ]
}
```

### ErrorPreset

An error preset is a convenient application-facing alias or prepared scenario.

For example:

```text
MissingConfigurationValue -> AFW-CFG-0001
InvalidInput              -> AFW-VAL-0001
UnknownError              -> AFW-GEN-0001
```

Application code can use the preset name instead of knowing the full catalog structure.

### ErrorDescriptor

An `ErrorDescriptor` describes a concrete runtime occurrence of an error.

It is what the application actually returns, logs, maps, or passes to a response object.

A descriptor may be created from a catalog definition and then enriched with runtime information such as:

- detail
- operation name
- component name
- source name
- exception
- metadata
- optional typed attachment

---

## ErrorDescriptor and ErrorDescriptor&lt;TAttachment&gt;

The package follows the same idea as `Response` and `Response<T>`.

There is a normal descriptor:

```csharp
ErrorDescriptor
```

and a generic descriptor with strongly typed additional data:

```csharp
ErrorDescriptor<TAttachment>
```

The generic version is useful when an error should carry structured details.

Example:

```csharp
ErrorDescriptor<MissingConfigurationAttachment> error =
    errorFactory.FireError(
        type: "MissingConfigurationValue",
        attachment: new MissingConfigurationAttachment
        {
            ConfigurationKey = "LibreTranslate:DefaultServer",
            SourceName = "appsettings.json"
        });
```

The attachment is optional. Most errors do not need it.

---

## Catalogs Are the Source of Error Knowledge

`WhenItFails` uses JSON catalogs as the primary storage format for error definitions.

This keeps catalogs:

- human-readable
- easy to edit
- easy to version in Git
- easy to copy
- easy to customize
- easy to export
- easy to load into memory
- independent from a database server

The library treats JSON files as a lightweight file-backed store. The expected data size is small, and the typical workflow is:

```text
load JSON
validate document
normalize if needed
build in-memory indexes
query in memory
save only when configuration changes
```

This is intentionally simpler than using SQLite for this scenario.

---

## Built-in Catalogs and Project Copies

Built-in Afrowave catalogs are source catalogs.

They should be treated as immutable.

A project may create a local copy in its `Jsons` folder. The built-in catalog should be copied only if the project copy does not already exist.

Recommended rule:

```text
If the project catalog does not exist:
    copy the built-in catalog.

If the project catalog already exists:
    do not overwrite it.
```

This protects user and project changes.

Future tooling may compare the built-in catalog with the project copy and suggest a merge, but automatic overwrite should be avoided.

---

## Code Ranges and Ownership

Numeric error codes should be block-based.

The purpose is similar to HTTP status code families: a knowledgeable person should be able to look at a code and roughly understand the error family.

Example direction:

```text
100000-199999  General / Core
200000-299999  Configuration
300000-399999  Validation / Input
400000-499999  Authentication / Authorization
500000-599999  File system / I/O
600000-699999  Network / external communication
700000-799999  Database / storage
800000-849999  Serialization / data format
850000-899999  Text / language helper areas
900000-999999  Framework / internal / unexpected
```

Application and user-defined errors should have their own reserved code space so they do not collide with future built-in Afrowave errors.

Example direction:

```text
0-999999        reserved for official catalogs
1000000-1999999 project/application errors
2000000-2999999 user/customer errors
3000000-3999999 integration/plugin errors
```

This avoids a dangerous situation where a user-defined error code collides with a new built-in error introduced by a later package update.

---

## Categories and Subcategories

An error can belong to more than one category.

For example, a database connection failure may also be relevant to startup, network, and external service handling.

Because of that, the model should support:

- primary category
- multiple categories
- multiple subcategories
- tags

Recommended distinction:

```text
CodePrefix / CodeGroup
    Defines where the error lives in the numbering and identity system.

PrimaryCategory / Categories / Subcategories / Tags
    Defines how the error can be searched, filtered, profiled, and understood.
```

This allows one error to be found from multiple useful viewpoints.

---

## Profiles

Profiles are scenario-specific views of the catalog.

Examples:

- Default
- Web
- API
- Desktop
- CLI
- Database
- Development
- Production

A web profile may care about HTTP status mapping and user-safe messages.

A database profile may care about connection, migration, query, and transaction failures.

A production profile may hide internal details.

A development profile may expose more diagnostic hints.

Profiles should not replace the catalog. They should select, filter, or adapt catalog definitions for a specific scenario.

---

## Localization Boundary

`WhenItFails` is not a localization system.

Localization belongs to the central Afrowave localization layer, such as `TalkToMe`.

However, `WhenItFails` should be localization-friendly.

It should be able to expose or export all texts that may need translation, such as:

- title
- message
- developer hint
- documentation label
- profile-specific text variants

For web applications, a localization system may automatically capture texts when code uses something like:

```csharp
localize[result.ErrorDescription.Message]
```

For desktop applications, where some errors may never appear during normal runtime, an explicit export helper can provide all catalog texts so they can be added to translation dictionaries.

So the rule is:

```text
WhenItFails provides error texts and export helpers.
TalkToMe or the consuming application handles translation.
```

---

## Web Error Handling

Web error handling is important but should not be forced into the core package.

The core `WhenItFails` package should remain independent from ASP.NET Core.

A future integration package may provide web-specific tools, for example:

```text
Afrowave.Toolbox.WhenItFails.AspNetCore
```

That package may handle:

- exception middleware
- mapping `ErrorDescriptor` to `ProblemDetails`
- mapping error definitions to HTTP status codes
- request ID and trace ID metadata
- safe production responses
- validation error formatting
- logging integration

The core package should provide enough metadata and descriptors to make this possible, but it should not depend on web-specific libraries.

---

## Why This Helps

Without a structured error system, applications often grow inconsistent error handling over time:

```csharp
return new Response
{
    Success = false,
    ErrorCode = 123,
    ErrorMessage = "Invalid input",
    ErrorCategory = "Validation"
};
```

Later, somewhere else:

```csharp
return new Response
{
    Success = false,
    ErrorCode = 124,
    ErrorMessage = "Bad input",
    ErrorCategory = "Input"
};
```

The result is usually:

- duplicated messages
- inconsistent codes
- unclear categories
- weak diagnostics
- harder translation
- harder support
- harder API documentation
- harder logging and filtering

With `WhenItFails`, application code can stay small:

```csharp
return Response.Fail(
    errorFactory.FireError("InvalidInput", "Name is required."));
```

The catalog and preset system provide the rest.

---

## Design Rule

A useful way to summarize the design is:

> The rich model is for catalogs and tools.  
> The simple API is for everyday application code.

If developers feel forced to fill the whole error model manually, the package has failed its purpose.

If developers can use a small `FireError(...)` call while still getting consistent, searchable, documentable, and exportable errors, the package is doing its job.
