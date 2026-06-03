# Extensions

The `Extensions` area provides convenience methods for enums, models, interfaces, metadata, diagnostics, issues, results, responses, and value objects.

The goal is to keep calling code readable and avoid repeated boilerplate.

## Enum extensions

### DataFormatExtensions

Methods:

- `IsTextBased`
- `IsBinary`
- `IsStructuredText`
- `IsUnknown`
- `IsCustom`

### IssueSeverityExtensions

Methods:

- `IsErrorOrHigher`
- `IsWarningOrHigher`
- `IsInformationOrLower`
- `IsCriticalOrHigher`

### OperationRiskLevelExtensions

Methods:

- `IsReadOnly`
- `IsMediumOrHigher`
- `IsHighOrHigher`
- `IsDangerous`
- `UsuallyRequiresApproval`
- `IsUsuallySafeForAutomaticExecution`

### OperationStatusExtensions

Methods:

- `IsPending`
- `IsRunning`
- `IsCompleted`
- `IsSuccessfullyCompleted`
- `HasWarnings`
- `IsPartiallyCompleted`
- `IsFailed`
- `IsCancelled`
- `IsSkipped`
- `IsFinal`

### ResultStatusExtensions

Methods:

- `IsSuccess`
- `HasWarnings`
- `IsPartial`
- `IsFailure`
- `IsNotFound`
- `IsFinal`
- `IsUnknown`
- `IsNonSuccess`
- `IsInvalid`
- `IsNotSupported`
- `IsCancelled`
- `IsFailed`

### SortDirectionExtensions

Methods:

- `IsAscending`
- `IsDescending`
- `IsSpecified`
- `Reverse`

## Result and response extensions

### ResultExtensions

Methods include:

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
- `HasWarningOrHigherIssues`
- `HasErrorOrHigherIssues`
- `HasCriticalOrHigherIssues`
- `GetHighestIssueSeverity`
- `GetStatusFromIssues`
- `HasStatusMatchingIssues`
- `HasStatusMismatchWithIssues`
- `IsPartial`
- `IsFailed`
- `IsNotSupported`
- `IsCancelled`
- `IsFinal`
- `IsNonSuccess`

### ResultMetadataExtensions

Methods:

- `HasMetadata`
- `HasMetadataKey`
- `TryGetMetadata`
- `GetMetadataOrDefault`

### ResultConversionExtensions

Methods:

- `ToResponse`
- `ToResponse<T>`
- `ToResult`
- `ToTypedResponse<T>`
- `ToNonGenericResponse<T>`

### ResponseExtensions

Methods:

- `HasNoData`
- `IsSuccessWithData`
- `IsSuccessWithoutData`
- `IsFailureWithoutData`

### ResponseOfTExtensions

Methods:

- `GetDataOrThrow`
- `GetDataOrDefault`
- `IsSuccessWithData`
- `HasDataMatching`

## Issue extensions

Issue-related extensions include:

- `IssueInfoExtensions`
- `IssueInfoCollectionExtensions`
- `IssueInfoListExtensions`
- `IssueSeverityExtensions`

These helpers can check severity, filter issue collections, append issue snapshots, count severities, and infer `ResultStatus` from issue severity.

## Metadata extensions

`MetadataBagExtensions` provides:

- `SetIfNotWhiteSpace`
- `SetIfNotNull`
- `MergeFrom`
- `Copy`

## Diagnostic extensions

Diagnostic extensions provide checks for:

- diagnostic code
- message
- details
- location
- spans
- hints
- severity filtering in collections

## IHas... extensions

The package includes extension methods for small interfaces such as:

- `IHasCode`
- `IHasMessage`
- `IHasMetadata`
- `IHasIssues`
- `IHasSeverity`
- `IHasStatus<TStatus>`
- `IHasRiskLevel`
- `IHasDataFormat`
- `IHasCultureCode`

These helpers generally validate the root object and then inspect the relevant property.

## Null handling note

Many extension methods intentionally throw when the root object is null.

Collection-oriented helpers may treat null entries inside collections defensively, depending on the specific helper and test coverage.
