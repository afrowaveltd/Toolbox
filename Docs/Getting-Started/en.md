# Getting Started with Afrowave.Toolbox.Essentials

`Afrowave.Toolbox.Essentials` is the foundation package of the current Afrowave.Toolbox generation.

It provides small, reusable building blocks for .NET 10 applications:

- result and response models
- issue and diagnostic models
- metadata helpers
- guard methods
- common interfaces
- value objects
- enum and model extension methods

## Installation

When the package is published, install it through NuGet:

```bash
dotnet add package Afrowave.Toolbox.Essentials
```

For local development inside the Toolbox solution, reference the project directly.

## Basic result

Use `Result` when an operation does not return a data payload.

```csharp
using Afrowave.Toolbox.Essentials.Results;

public Result SaveSettings()
{
   return Result.Ok("Settings were saved.");
}
```

A failure result can carry an issue code and message:

```csharp
return Result.Fail(
   "SETTINGS_SAVE_FAILED",
   "Settings could not be saved.");
```

## Typed response

Use `Response<T>` when an operation returns data.

```csharp
using Afrowave.Toolbox.Essentials.Results;

public Response<UserDto> GetUser()
{
   var user = new UserDto
   {
      Name = "Ada"
   };

   return Response<UserDto>.Ok(user);
}
```

If no data is available, use a suitable status:

```csharp
return Response<UserDto>.NotFound(
   "USER_NOT_FOUND",
   "The requested user was not found.");
```

## Issues

Use `IssueInfo` and `IssueInfoFactory` to create structured warnings and errors.

```csharp
using Afrowave.Toolbox.Essentials.Issues;

var issue = IssueInfoFactory.Warning(
   "CONFIG_DEPRECATED",
   "The configuration key is deprecated.");
```

Issue lists can be created with `IssueInfoListFactory`:

```csharp
var issues = IssueInfoListFactory.Warning(
   "CONFIG_DEPRECATED",
   "The configuration key is deprecated.");
```

## Metadata

Use `MetadataBag` for small string-based contextual information.

```csharp
using Afrowave.Toolbox.Essentials.Metadata;

var metadata = MetadataBagFactory.From(
   "provider",
   "ollama-local");
```

Metadata keys are case-insensitive. Metadata values cannot be null.

## Guards

Use `Guard` for direct argument validation.

```csharp
using Afrowave.Toolbox.Essentials.Guards;

var name = Guard.NotNullOrWhiteSpace(
   inputName,
   nameof(inputName));
```

## Important behavior rules

Essentials follows a few important defensive rules:

- null root objects passed to helpers usually throw `ArgumentNullException`
- required codes, names, messages, and metadata keys reject null, empty, or whitespace values
- `MetadataBag` values reject null
- result and response copy methods copy metadata instead of sharing a mutable metadata instance
- result and response copy methods normalize null issue collections to empty read-only lists

## Next topics

Recommended next reading:

- [Essentials Overview](../Essentials-Overview/en.md)
- [Results and Responses](../Results-and-Responses/en.md)
- [Issues](../Issues/en.md)
- [Metadata](../Metadata/en.md)
