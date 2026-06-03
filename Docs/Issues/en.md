# Issues

The `Issues` area provides structured models for warnings, errors, and other problem reports.

## IssueInfo

`IssueInfo` represents one issue.

Namespace:

```csharp
Afrowave.Toolbox.Essentials.Issues
```

Main properties:

- `Code`
- `Number`
- `Message`
- `Details`
- `Severity`
- `Metadata`

`Code` and `Message` are required for factory-created issues.

`Details` and `Number` are optional.

`Metadata` is a `MetadataBag`.

## Issue severity

Issues use `IssueSeverity`:

- `None`
- `Trace`
- `Debug`
- `Information`
- `Warning`
- `Error`
- `Critical`
- `Fatal`

Severity extension helpers include:

- `IsInformationOrLower`
- `IsWarningOrHigher`
- `IsErrorOrHigher`
- `IsCriticalOrHigher`

## IssueInfoFactory

`IssueInfoFactory` creates and copies issue objects.

Common factory methods:

- `Create(string code, string message, IssueSeverity severity)`
- `Create(string code, string message, string? details, IssueSeverity severity)`
- `Create(string code, int? number, string message, IssueSeverity severity)`
- `Information(string code, string message)`
- `Warning(string code, string message)`
- `Error(string code, string message)`
- `Critical(string code, string message)`
- `Fatal(string code, string message)`

Copy helpers:

- `WithMetadata`
- `WithNumber`
- `WithDetails`
- `WithMessage`
- `WithSeverity`
- `WithCode`

Current issue copy helpers keep the metadata reference. They do not copy `MetadataBag`.

## IssueInfoListFactory

`IssueInfoListFactory` creates read-only issue lists.

Methods:

- `Empty()`
- `From(params IssueInfo[] issues)`
- `From(IEnumerable<IssueInfo> issues)`
- `Information(string code, string message)`
- `Warning(string code, string message)`
- `Error(string code, string message)`
- `Critical(string code, string message)`
- `Fatal(string code, string message)`

`Empty()` returns a safe empty read-only list.

`From(...)` methods create a snapshot of the supplied issues.

## Collection extensions

Issue collection extensions help filter and summarize issues.

Common helpers include:

- `HasErrors`
- `HasWarningsOrErrors`
- `Errors`
- `Warnings`
- `Informational`
- `WithSeverity`
- `AppendIssue`
- `AppendIssues`
- `HasIssueCode`
- `TryGetIssueByCode`
- `WhereSeverity`
- `WhereWarningOrHigher`
- `WhereErrorOrHigher`
- `WhereCriticalOrHigher`
- `CountSeverity`
- `CountWarningOrHigher`
- `CountErrorOrHigher`
- `CountCriticalOrHigher`
- `GetHighestSeverity`
- `ToResultStatus`

## Example

```csharp
var issue = IssueInfoFactory.Error(
   "VALIDATION_FAILED",
   "The input is invalid.");

var issues = IssueInfoListFactory.From(issue);

var hasErrors = issues.HasErrors();
```

## Design notes

Issues are intended to be stable, structured messages.

Prefer stable codes over ad-hoc message parsing.

Good code examples:

```text
VALIDATION_FAILED
USER_NOT_FOUND
CONFIG_DEPRECATED
```
