# Metadata

The metadata area provides `MetadataBag`, a lightweight mutable string key-value container.

## MetadataBag

Namespace:

```csharp
Afrowave.Toolbox.Essentials.Metadata
```

`MetadataBag` stores string keys and string values.

Features:

- case-insensitive keys
- mutable operations
- read-only view through `Items`
- copy support through `MetadataBagFactory`
- extension helpers for common operations

## Core members

Properties:

- `Count`
- `IsEmpty`
- `Items`
- indexer `this[string key]`

Methods:

- `Set(string key, string value)`
- `TryGet(string key, out string? value)`
- `Remove(string key)`
- `Clear()`

## Validation rules

Metadata keys are required.

A key cannot be:

- null
- empty
- whitespace

Metadata values cannot be null.

Empty strings and whitespace strings are allowed values.

This is intentional: a metadata value may sometimes be an empty marker, but null is not stored.

## Example

```csharp
var metadata = new MetadataBag();

metadata.Set("provider", "ollama-local");
metadata.Set("profile", "markdown-refine");

if(metadata.TryGet("PROVIDER", out var provider))
{
   Console.WriteLine(provider);
}
```

The lookup is case-insensitive.

## MetadataBagFactory

Factory methods:

- `Empty()`
- `From(string key, string value)`
- `From(IEnumerable<KeyValuePair<string, string>> items)`
- `From(params (string Key, string Value)[] items)`
- `CopyFrom(MetadataBag metadata)`
- `CopyWith(MetadataBag metadata, string key, string value)`

`CopyFrom` creates a new metadata bag with copied values.

`CopyWith` creates a copy and then adds or updates one value in the copy.

## Copy behavior

`MetadataBag` is mutable.

Because of that, result and response copy/conversion methods copy metadata instead of sharing the same metadata instance.

This is different from some issue and diagnostic factory methods, which may keep the metadata reference.

## MetadataBagExtensions

Extension helpers include:

- `SetIfNotWhiteSpace`
- `SetIfNotNull`
- `MergeFrom`
- `Copy`

`Copy()` delegates to metadata copying behavior and returns a separate metadata bag.
