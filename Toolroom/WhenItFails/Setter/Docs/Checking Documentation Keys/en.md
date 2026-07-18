# Checking documentation keys

Each error definition may point to its documentation through `documentationKey`.

The key is intended to be stable, non-empty, and unique across the complete error catalog. Setter provides a read-only command that checks these rules without modifying the workspace.

## Command

From the Toolbox repository:

```bash
dotnet run --project Toolroom/WhenItFails/Setter -- check-doc-keys .
```

Published executable form:

```bash
when-it-fails-setter check-doc-keys <path>
```

`<path>` may point either to the project root or directly to `Jsons/WhenItFails`.

## Output modes

Human-readable output:

```bash
when-it-fails-setter check-doc-keys .
```

Tab-separated output suitable for scripts:

```bash
when-it-fails-setter check-doc-keys . --plain
```

Structured JSON output:

```bash
when-it-fails-setter check-doc-keys . --json
```

`--plain` and `--json` are mutually exclusive.

## Rules

The command reports:

- missing, empty, or whitespace-only documentation keys;
- duplicate keys using case-insensitive comparison;
- duplicate keys after trimming surrounding whitespace.

Results are ordered deterministically by numeric error code and stable error ID.

## Exit codes

```text
0  every error has a unique non-empty documentation key
1  command syntax or options are invalid
2  the workspace could not be loaded, or documentation-key issues were found
3  an unexpected command-level failure occurred
```

## Recommended use

Run the check after adding an error or changing a documentation key:

```bash
when-it-fails-setter set-documentation-key . \
  AFW_NET_0001 \
  when-it-fails/errors/network/network-unavailable

when-it-fails-setter check-doc-keys .
```

The Setter test suite also checks the repository catalog directly, so a missing or duplicate documentation key causes the relevant CI test to fail.
