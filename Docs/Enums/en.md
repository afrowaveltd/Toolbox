# Enums

The Essentials package defines shared enums for common status, severity, format, risk, and sorting concepts.

## DataFormat

Represents a general data format.

Values:

- `Unknown`
- `GenericText`
- `PlainText`
- `Json`
- `Ajis`
- `Xml`
- `Csv`
- `Markdown`
- `Html`
- `Yaml`
- `GenericBinary`
- `Custom`

Useful helpers include:

- `IsTextBased`
- `IsBinary`
- `IsStructuredText`
- `IsUnknown`
- `IsCustom`

## IssueSeverity

Represents the severity of an issue or diagnostic message.

Values:

- `None`
- `Trace`
- `Debug`
- `Information`
- `Warning`
- `Error`
- `Critical`
- `Fatal`

Useful helpers include:

- `IsInformationOrLower`
- `IsWarningOrHigher`
- `IsErrorOrHigher`
- `IsCriticalOrHigher`

## OperationRiskLevel

Represents the expected risk of an operation.

Values:

- `None`
- `ReadOnly`
- `Low`
- `Medium`
- `High`
- `Dangerous`

Useful helpers include:

- `IsReadOnly`
- `IsMediumOrHigher`
- `IsHighOrHigher`
- `IsDangerous`
- `UsuallyRequiresApproval`
- `IsUsuallySafeForAutomaticExecution`

## OperationStatus

Represents the current or final status of an operation.

Values:

- `Unknown`
- `Pending`
- `Running`
- `Completed`
- `CompletedWithWarnings`
- `PartiallyCompleted`
- `Failed`
- `Cancelled`
- `Skipped`
- `NotSupported`

Useful helpers include:

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

## ResultStatus

Represents the status of a produced result.

Values:

- `Unknown`
- `Success`
- `SuccessWithWarnings`
- `Partial`
- `NotFound`
- `Invalid`
- `NotSupported`
- `Cancelled`
- `Failed`

Useful helpers include:

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

Current behavior note: `NotFound` is not treated as failure by `ResultStatusExtensions.IsFailure()`.

## SortDirection

Represents sort direction.

Values:

- `None`
- `Ascending`
- `Descending`

Useful helpers include:

- `IsAscending`
- `IsDescending`
- `IsSpecified`
- `Reverse`
