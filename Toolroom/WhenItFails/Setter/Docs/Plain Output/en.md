# Plain Output

Several commands support a `--plain` switch that changes output from rich Spectre.Console formatting to plain tab-separated text.

## Supported Commands

- `errors --plain` — Outputs error definitions as TSV (tab-separated values).
- `details --plain` — Outputs a single error definition as key-value lines.

## Use Cases

Plain output is useful for:

- **Scripting** — Parse output with `cut`, `awk`, or other Unix tools.
- **Piping** — Redirect output to files or other processes.
- **CI/CD logs** — Plain text renders correctly in log viewers that don't support ANSI escape codes.
- **Diffing** — Compare output between workspaces or versions.

## Examples

### Plain error list

```
when-it-fails-setter errors . --plain --category NETWORK
```

Output:

```
WhenItFails Error Definitions
Workspace: Jsons/WhenItFails
Errors: 4 shown from 37
Filters: category=NETWORK

Code	Id	Name	Owner	Group	Category	Severity	Title
600001	AFW_NET_0001	NETWORKUNAVAILABLE	AFW	NETWORK	NETWORK	Error	Network unavailable
600002	AFW_NET_0002	HTTPREQUESTFAILED	AFW	NETWORK	NETWORK	Error	HTTP request failed
600003	AFW_NET_0003	REMOTETIMEOUT	AFW	NETWORK	NETWORK	Error	Remote operation timed out
600004	AFW_NET_0004	DNSLOOKUPFAILED	AFW	NETWORK	NETWORK	Error	DNS lookup failed
```

### Plain error detail

```
when-it-fails-setter details . AFW_NET_0001 --plain
```

Output:

```
WhenItFails Error Detail
Workspace: Jsons/WhenItFails

Code: 600001
Id: AFW_NET_0001
Name: NETWORKUNAVAILABLE
Title: Network unavailable
Message: The network is unavailable.
Severity: Error
Owner: AFW
Code prefix: NET
Code group: NETWORK
Primary category: NETWORK
Categories: NETWORK
Subcategories: CONNECTIVITY
Tags: NETWORK, USER_VISIBLE
Developer hint: Check connectivity, DNS, firewall, proxy, VPN, and host availability.
Documentation key: when-it-fails/errors/network/network-unavailable
```

## Without --plain

When `--plain` is omitted, the same data is rendered using Spectre.Console with rounded bordered tables, color-coded values, and proper column alignment. This is the default for interactive terminal use.

> Localized versions of this documentation may be generated later by Afrowave translation tooling.
