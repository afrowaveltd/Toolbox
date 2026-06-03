# Afrowave.Toolbox Documentation

This section is the documentation entry point for the current Afrowave.Toolbox generation.

The documentation is organized by topic. Each topic has its own directory and the default English document is always named `en.md`.

## Recommended reading order

Start here if you are new to the package:

1. [Getting Started](../Getting-Started/en.md)
2. [Essentials Overview](../Essentials-Overview/en.md)
3. [Results and Responses](../Results-and-Responses/en.md)
4. [Issues](../Issues/en.md)
5. [Metadata](../Metadata/en.md)

Then continue with the more specific reference topics:

- [Diagnostics](../Diagnostics/en.md)
- [Extensions](../Extensions/en.md)
- [Interfaces](../Interfaces/en.md)
- [Enums](../Enums/en.md)
- [Guards](../Guards/en.md)
- [Value Objects](../Value-Objects/en.md)
- [API Reference](../API-Reference/en.md)

## Documentation structure

The documentation follows the Afrowave localization-friendly structure:

```text
Docs/
  Topic-Name/
    en.md
```

Each directory name describes the topic. The `en.md` file is the default English source document for that topic.

Future translation tools can generate additional localized files next to `en.md`, for example:

```text
Docs/
  Results-and-Responses/
    en.md
    cs.md
    fr.md
```

## Current package

The current documented package is:

```text
Afrowave.Toolbox.Essentials
```

It provides foundational building blocks for .NET applications:

- result and response models
- issue and diagnostic models
- metadata helpers
- common interfaces
- guard helpers
- enum and model extension methods
- value objects for common validated strings

## Important behavior rules

Several behavior rules are used consistently across the package.

### Null root objects

Public helper methods generally reject a null root object.

For example, extension methods and copy helpers throw `ArgumentNullException` when the object being operated on is null.

### Invalid arguments

Invalid required values are rejected.

Common examples:

- null, empty, or whitespace keys
- null, empty, or whitespace codes
- null, empty, or whitespace messages
- null required collections
- null metadata values where metadata values are required

### Nullable model properties

Some model properties are intentionally handled defensively.

For example, issue collections on result and response models may be normalized to an empty read-only list when a method creates a copy.

This protects callers from broken or manually constructed objects where a property was set to `null!`.

### Metadata copying

`MetadataBag` is mutable.

For this reason, copy and conversion methods for result and response models create a copy of metadata instead of sharing the same mutable metadata instance.

This rule is important for predictable behavior:

```text
changing metadata on a copied result or response
  must not unexpectedly change the original object
```

### Issue list handling

Issue lists are exposed as read-only lists.

When copying result and response models, existing non-null issue lists are usually kept as references. Null issue lists are normalized to safe empty lists.

This keeps copying lightweight while still preventing null collection failures.

## Documentation quality rules

Documentation should describe the actual public behavior of the package.

Avoid documenting planned APIs as if they already exist.

Especially avoid:

- invented method names
- old type names
- outdated examples
- overly broad serialization claims
- marketing language that does not describe real behavior

## Known naming note

The generic response type is named:

```csharp
Response<T>
```

The source file may be named `ResponseOfT.cs`, but documentation and examples should always use the real public type name:

```csharp
Response<User>
```

Do not document it as `ResponseOfT<User>`.
