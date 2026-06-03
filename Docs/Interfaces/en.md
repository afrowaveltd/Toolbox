# Interfaces

The interfaces define small contracts used by models and extension methods.

Namespace:

```csharp
Afrowave.Toolbox.Essentials.Interfaces
```

## Result and response contracts

### IResult

Represents a result-like object.

Members include:

- `Status`
- `Message`
- `Issues`
- `Metadata`
- `IsSuccess`
- `IsFailure`
- `HasWarnings`

### IResponse

Extends `IResult` and adds:

- `HasData`

### IResponse<T>

Extends `IResponse` and adds:

- `Data`

The public generic response model is `Response<T>`.

## Common property contracts

The package includes small `IHas...` interfaces:

- `IHasCode`
- `IHasCultureCode`
- `IHasDataFormat`
- `IHasDescription`
- `IHasDetails`
- `IHasDiagnostics`
- `IHasId<TId>`
- `IHasIssues`
- `IHasMessage`
- `IHasMetadata`
- `IHasName`
- `IHasNumber`
- `IHasProfileName`
- `IHasProviderName`
- `IHasRiskLevel`
- `IHasSeverity`
- `IHasStatus<TStatus>`

These interfaces are intentionally simple. They make extension methods possible without requiring inheritance from a shared base class.

## Example

```csharp
public void LogMessage(IHasMessage value)
{
   if(value.HasMessage())
   {
      Console.WriteLine(value.Message);
   }
}
```

## Design note

Prefer implementing only the interfaces that describe the type accurately.

Do not implement an interface only to get access to an extension method if the property does not naturally belong to the type.
