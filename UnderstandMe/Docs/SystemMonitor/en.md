# Hierarchical SystemMonitor

Status: future design only.

`/SystemMonitor` aggregates events from registered modules. `/AfroLT/SystemMonitor`, `/Afutrans/SystemMonitor`, and `/UnderstandMe/SystemMonitor` expose only the named module's events.

Each emitted event uses one typed model with module sender identity and instance ID. A host fans out each event to both global and module feeds. These feeds are outbound-only, best-effort monitoring; durable records and administrative commands are handled separately.
