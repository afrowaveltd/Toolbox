# Value Objects

The value object types wrap common validated strings.

Namespace:

```csharp
Afrowave.Toolbox.Essentials.ValueObjects
```

## CultureCode

Represents a culture or locale code.

Examples:

```text
en
en-US
cs-CZ
```

Members:

- `Value`
- `IsNeutral`
- `IsSpecific`
- `GetNeutralPart()`
- `From(string value)`

Conversions:

- implicit conversion to `string`
- explicit conversion from `string`

Example:

```csharp
var culture = CultureCode.From("en-US");

Console.WriteLine(culture.IsSpecific);
Console.WriteLine(culture.GetNeutralPart());
```

## ProviderName

Represents a provider name, such as a service, backend, or engine identifier.

Members:

- `Value`
- `From(string value)`

Conversions:

- implicit conversion to `string`
- explicit conversion from `string`

Example:

```csharp
var provider = ProviderName.From("ollama-local");
string value = provider;
```

## ProfileName

Represents a profile name, such as a prompt profile or configuration profile.

Members:

- `Value`
- `From(string value)`

Conversions:

- implicit conversion to `string`
- explicit conversion from `string`

Example:

```csharp
var profile = ProfileName.From("markdown-refine");
```

## Extension helpers

Value object extensions include:

- `CultureCode.HasSameNeutralPartAs`
- `CultureCode.EqualsIgnoreCase`
- `CultureCode.ToLowerInvariantCode`
- `CultureCode.GetParentOrSelf`
- `ProviderName.EqualsIgnoreCase`
- `ProviderName.ToLowerInvariantName`
- `ProfileName.EqualsIgnoreCase`
- `ProfileName.ToLowerInvariantName`

## Validation

Value object constructors and factory methods reject invalid values according to the current implementation and tests.

Use `From(...)` when you want a clear factory style.
