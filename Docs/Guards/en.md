# Guards

The `Guard` class provides simple argument validation helpers.

Namespace:

```csharp
Afrowave.Toolbox.Essentials.Guards
```

## Methods

### NotNull

```csharp
public static T NotNull<T>(T? value, string paramName)
   where T : class
```

Returns the value when it is not null.

Throws `ArgumentNullException` when the value is null.

### NotNullOrWhiteSpace

```csharp
public static string NotNullOrWhiteSpace(
   string? value,
   string paramName)
```

Returns the value when it contains non-whitespace text.

Throws `ArgumentException` for null, empty, or whitespace strings.

### NotNegative

```csharp
public static int NotNegative(int value, string paramName)
public static long NotNegative(long value, string paramName)
```

Returns the value when it is zero or greater.

Throws `ArgumentOutOfRangeException` for negative values.

### Positive

```csharp
public static int Positive(int value, string paramName)
public static long Positive(long value, string paramName)
```

Returns the value when it is greater than zero.

Throws `ArgumentOutOfRangeException` for zero or negative values.

### InRange

```csharp
public static int InRange(
   int value,
   int minimum,
   int maximum,
   string paramName)
```

Returns the value when it is within the inclusive range.

Throws `ArgumentOutOfRangeException` when the value is outside the range.

## Example

```csharp
public void SetTimeout(int milliseconds)
{
   var timeout = Guard.Positive(
      milliseconds,
      nameof(milliseconds));
}
```

## Design note

The guard helpers are intentionally small.

They do not replace domain validation. They are meant for common argument checks at API boundaries.
