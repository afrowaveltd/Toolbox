# Potential format converters — roadmap, not implementation

Possible independently packaged helpers:

| Direction | Notes |
|---|---|
| Markdown -> Plain text | Strip formatting while preserving readable content; choose how to show links and images |
| Plain text -> Markdown | Cannot reconstruct formatting; explicit default wrapping/escaping policy |
| HTML -> Plain text | Use an HTML-aware parser, preserve meaningful whitespace and block boundaries |
| Plain text -> HTML | Encode special characters; choose paragraph/line-break semantics |
| Markdown <-> HTML | Escape safely, preserve links/structure when possible; document unsupported syntax |
| JSON <-> AJIS | Define a compatibility subset and preserve AJIS-only directives/attachments or report loss |
| JSON <-> XML | Explicit model for arrays, attributes, types, namespaces and ordering |
| AJIS <-> XML | Separate compatibility rules for aliases, directives, binary data and types |

**None of these converters is implemented in this first ExchangeHouse increment.** Convert only where the mapping is defined, with tests and clear information-loss policies. Do not implement a universal format AST merely to make all pairs look interchangeable.

Each future independent project gets its own README, `Docs/*/en.md`, tests and implementation status.
