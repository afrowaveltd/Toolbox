# Results and Responses

The `Results` area provides standardized return models for operations.

Use these types when an operation needs to return status, messages, issues, metadata, and optionally a data payload.

## Main types

### Result

`Result` represents an operation result without a data payload.

Namespace:

```csharp
Afrowave.Toolbox.Essentials.Results
```

Main members:

- `Status`
- `Message`
- `Issues`
- `Metadata`
- `IsSuccess`
- `IsFailure`
- `HasWarnings`

Common factory methods:

- `Result.Ok()`
- `Result.Ok(string message)`
- `Result.OkWithWarnings(IReadOnlyList<IssueInfo> issues)`
- `Result.Fail(string code, string message)`
- `Result.Fail(IReadOnlyList<IssueInfo> issues)`
- `Result.Invalid(string code, string message)`
- `Result.NotFound(string code, string message)`
- `Result.Partial(string code, string message)`
- `Result.NotSupported(string code, string message)`
- `Result.Cancelled(string code, string message)`

Copy helpers include:

- `WithMessage`
- `WithStatus`
- `WithIssues`
- `WithMetadata`
- `AddIssue`
- `AddIssues`
- `AddMetadata`
- `FromIssue`
- `FromIssues`

Example:

```csharp
var result = Result.Ok("Operation completed.");
```

### Response

`Response` represents a response without typed data.

It implements `IResponse`, which extends `IResult`.

`Response.HasData` is always `false`.

Example:

```csharp
var response = Response.Fail(
   "REQUEST_FAILED",
   "The request failed.");
```

### Response<T>

`Response<T>` represents a response with a typed data payload.

The public type name is `Response<T>`.

Do not document or use it as `ResponseOfT<T>`. `ResponseOfT.cs` may be the source file name, but the public type is `Response<T>`.

Example:

```csharp
var response = Response<UserDto>.Ok(user);
```

`Response<T>.HasData` is true when `Data` is not null.

For nullable reference types, `Data = null` means no data.

For value types such as `int`, default values such as `0` still count as data because `Data` is not null.

## Status behavior

`IsSuccess`, `IsFailure`, and `HasWarnings` are derived from `ResultStatus` and attached issues.

Important points:

- `Success` and `SuccessWithWarnings` are success statuses
- `NotFound` is not treated as failure in the current status extension behavior
- `Failed`, `Invalid`, `NotSupported`, and `Cancelled` are failure statuses
- warning or higher issues can make `HasWarnings` true

## Metadata copy behavior

`MetadataBag` is mutable.

For that reason, copy and conversion methods for `Result`, `Response`, and `Response<T>` copy metadata instead of sharing the same mutable metadata object.

Example:

```csharp
var original = Result.Ok();
var withMetadata = Result.AddMetadata(
   original,
   "source",
   "unit-test");
```

Changing metadata on the copied result should not unexpectedly modify the original result.

## Issue list behavior

Issue lists are exposed as `IReadOnlyList<IssueInfo>`.

When a result or response is copied:

- a non-null issue list is usually kept as the same reference
- a null issue list is normalized to an empty read-only list

This keeps copying lightweight while still protecting against manually broken objects where `Issues` was set to `null!`.

## Conversion extensions

`ResultConversionExtensions` provides conversions between result and response types:

- `ToResponse(this IResult result)`
- `ToResponse<T>(this IResult result, T? data)`
- `ToResult(this IResponse response)`
- `ToTypedResponse<T>(this IResponse response, T? data)`
- `ToNonGenericResponse<T>(this IResponse<T> response)`

Conversion methods also follow the metadata copy and issue list normalization rules.

## Result extension helpers

`ResultExtensions` adds convenience checks such as:

- `HasStatus`
- `IsUnknown`
- `IsInvalid`
- `IsNotFound`
- `HasIssues`
- `HasErrors`
- `HasMessage`
- `HasIssueCode`
- `TryGetIssueByCode`
- `IsCleanSuccess`
- `IsDirtySuccess`
- `NeedsAttention`
- `GetHighestIssueSeverity`
- `GetStatusFromIssues`
- `HasStatusMatchingIssues`
- `HasStatusMismatchWithIssues`

These helpers validate the root result object and handle null issue collections defensively where appropriate.
