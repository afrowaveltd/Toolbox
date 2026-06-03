# Diagnostics

Diagnostics provide richer reporting than simple issues.

They can include source locations, spans, hints, details, severity, and metadata.

## Main types

Namespace:

```csharp
Afrowave.Toolbox.Essentials.Diagnostics
```

### DiagnosticInfo

Represents one diagnostic message.

Main properties:

- `Code`
- `Message`
- `Details`
- `Severity`
- `Location`
- `Spans`
- `Hints`
- `Metadata`

`DiagnosticInfo` implements:

- `IHasCode`
- `IHasSeverity`
- `IHasMetadata`

### DiagnosticLocation

Represents a position in a source document, file, stream, or logical input.

Properties:

- `Source`
- `Line`
- `Column`
- `Offset`

Line and column values are one-based when available.

Offset is zero-based when available.

### DiagnosticSpan

Represents a range in a source document.

Properties:

- `Start`
- `End`
- `Label`

### DiagnosticHint

Represents a note, help text, suggestion, or example.

Properties:

- `Kind`
- `Message`

### DiagnosticHintKind

Values:

- `Note`
- `Help`
- `Suggestion`
- `Example`

## Factories

Diagnostics include factory classes for convenient construction:

- `DiagnosticInfoFactory`
- `DiagnosticInfoListFactory`
- `DiagnosticLocationFactory`
- `DiagnosticSpanFactory`
- `DiagnosticSpanListFactory`
- `DiagnosticHintFactory`

## Example

```csharp
var location = DiagnosticLocationFactory.FromSourceLineColumn(
   "Program.cs",
   42,
   15);

var diagnostic = DiagnosticInfoFactory.Warning(
   "AFW_DIAG001",
   "A possible issue was found.");

diagnostic = DiagnosticInfoFactory.WithLocation(
   diagnostic,
   location);
```

## Collection helpers

Diagnostic collection extensions can:

- detect errors
- detect warnings or errors
- filter errors
- filter warnings
- filter informational diagnostics
- filter by severity

## Copy behavior

Diagnostic factory copy helpers generally create a new diagnostic object and copy scalar values.

Some reference properties such as metadata, spans, hints, and location may be carried as references depending on the specific helper.

Do not assume all diagnostic copy helpers deep-copy nested objects.
