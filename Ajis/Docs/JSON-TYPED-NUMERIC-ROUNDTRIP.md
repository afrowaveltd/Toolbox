# AJIS <-> JSON: compact typed-numeric key prefixes (design note)

> **Status:** design proposal, recorded 2026-11-20. No converter implementation or conformance tests are claimed here.
>
> **Scope:** a small, human-readable JSON mapping for AJIS hexadecimal, binary and octal **numeric literals**, distinct from strings that merely look like those literals. This complements, rather than replaces, the broader `#ajisData` envelope explored in [DESIGN_NOTES.md](../DESIGN_NOTES.md), section 13.

## Why

AJIS distinguishes quoted keys, quoted string values and unquoted typed numeric literals:

```text
{
    "Count": 0x0000AB12,
    "Description": "0x0000AB12"
}
```

Plain JSON does not support `0x`, `0b` or `0o` numeric literal syntax. Converting both AJIS values to ordinary JSON strings would lose their type distinction. Inferring a number by looking at the value's text would also incorrectly reinterpret legitimate strings.

The proposed solution attaches a short **type marker to the exported JSON key**. The marker belongs to the conversion representation; it is not part of the original AJIS key.

## Reserved leading markers

| Marker on JSON key | Meaning during JSON -> AJIS conversion |
| --- | --- |
| `(0x)` | Restore a hex numeric literal |
| `(0b)` | Restore a binary numeric literal |
| `(0o)` | Restore an octal numeric literal |
| `(str)` | Restore a literal string value; preserve the rest of the key verbatim |

Only the **first** marker is interpreted. The remainder of the key is original user data, even when it begins with another marker. An ordinary AJIS key remains quoted; the numeric value is unquoted in AJIS and quoted in JSON.

## Exact examples

**AJIS:**

```text
{
    "(0x)0x123": "0xABC",
    "HexNumber": 0x0000AB12,
    "(0x)0x123-Number": 0xABC,
    "Bits": 0b1010,
    "Permissions": 0o755,
    "Description": "0x0000AB12",
    "(str)Literal": "Hello"
}
```

**Tagged JSON:**

```json
{
    "(str)(0x)0x123": "0xABC",
    "(0x)HexNumber": "0x0000AB12",
    "(0x)(0x)0x123-Number": "0xABC",
    "(0b)Bits": "0b1010",
    "(0o)Permissions": "0o755",
    "Description": "0x0000AB12",
    "(str)(str)Literal": "Hello"
}
```

In particular, the two cases that motivated the design are:

```text
AJIS string:  "(0x)0x123": "0xABC"
JSON:         "(str)(0x)0x123": "0xABC"

AJIS number:  "(0x)0x123": 0xABC
JSON:         "(0x)(0x)0x123": "0xABC"
```

The first key marker signals how to restore the **value**. A second apparent marker is part of the **original key**, not a second instruction.

## Conversion rules (proposed)

**AJIS -> JSON**

1. For an AJIS hex/binary/octal *numeric value*, add `(0x)`, `(0b)` or `(0o)` to the front of the original key. Emit the original numeric literal as a JSON **string**. Prefer preserving its original textual spelling (for example `0x0000AB12`) rather than converting it to decimal.
2. For an AJIS *string value*, retain it as a JSON string. If its original key starts with one of the reserved markers, prepend `(str)` to protect the literal key name. Otherwise leave the key untouched.
3. Always use valid JSON string quoting and escaping for keys and string contents. The marker does not remove that requirement.
4. The conversion must use the actual AJIS value type, never guess the type from a string beginning `0x`, `0b` or `0o`.

**JSON -> AJIS**

1. Inspect **at most the first leading marker** on the JSON key.
2. For `(0x)`, `(0b)` and `(0o)`, require a JSON string whose contents are a valid numeric literal in the corresponding AJIS notation; strip one marker from the key, write a quoted AJIS key and an **unquoted** numeric value. Invalid marked values must be reported, not guessed or silently coerced.
3. For `(str)`, strip one marker and retain the JSON string as a quoted AJIS string.
4. Without a recognized leading marker, use the ordinary JSON-to-AJIS mapping; a string like `"0xABC"` remains a string.

## Guarantees and boundaries

- The tagged representation is **valid JSON**. Ordinary JSON tools can read the document without understanding the markers.
- For the defined string and radix-numeric cases, this is a small, reversible convention, including keys that themselves start with `(str)`, `(0x)`, `(0b)` or `(0o)`.
- Avoid rewriting numeric literal spelling when the AJIS parser has retained it: initial zeros, letter case and any permitted digit grouping may matter to source-form fidelity.
- An arbitrary third-party JSON document that happens to use these reserved key prefixes may be interpreted as tagged input. The format/profile must explicitly opt into or document this convention.
- Handling collisions for **other JSON value types** (e.g. a native JSON number, boolean, array or object under a key beginning with a reserved marker) is **not defined by this small scalar rule**. Do not claim a universal lossless round-trip until the general collision policy is specified. The separately proposed `#ajisData` envelope remains available for more general AJIS-only semantics, including directives, binary attachments, schemas and other extension types.
- Logical round-trip is distinct from byte-for-byte preservation of whitespace, comments or original document formatting.

## Minimal conformance cases to add later

- `"Count": 0xABC` round-trips as a numeric literal, while `"Count": "0xABC"` remains a string.
- `"(0x)0x123": "0xABC"` exports as `"(str)(0x)0x123": "0xABC"` and restores exactly.
- `"(0x)0x123": 0xABC` exports as `"(0x)(0x)0x123": "0xABC"` and restores exactly.
- `"(str)Name": "Hello"` exports as `"(str)(str)Name": "Hello"`.
- The same pattern works for `0b` and `0o`.
- The importer rejects malformed marked numerals and does not infer types from unmarked strings.
- A separate test suite must cover general JSON/AJIS metadata interoperability, nested data and unsupported direct mappings.

**Design principle:** no auxiliary metadata object for these common scalar cases; a single reserved leading marker on the JSON key is enough. Keep the general lossless-envelope discussion intact until full-format coverage is specified.
