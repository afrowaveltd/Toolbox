# Thermal errors

Thermal errors describe temperature-related states that can apply to computer hardware, batteries, power systems, motors, converters, servers, switchboards, industrial sensors, and general monitoring systems.

## Temperature limit exceeded

| Field | Value |
|---|---|
| ID | `AFW_THM_0001` |
| Code | `1000001` |
| Name | `TEMPERATURELIMITEXCEEDED` |
| Code group | `THERMAL` |
| Primary category | `THERMAL` |
| Default severity | `Warning` |
| Documentation key | `when-it-fails/errors/thermal/temperature-limit-exceeded` |

Use this definition when a reported temperature is syntactically and structurally valid but exceeds the configured safe operating limit.

### Message

```text
The reported temperature {temperature}{unit} exceeds the configured safe limit of {limit}{unit}.
```

Required message parameters:

- `temperature` — the reported numeric temperature;
- `unit` — the unit suffix used consistently for the reported value and limit;
- `limit` — the configured safe operating limit.

The application should preserve the numeric values and unit in structured runtime data or metadata when available. Message formatting must not replace structured values as the source of truth.

### Developer guidance

Verify the sensor reading, unit conversion, cooling path, workload, configured limits, and shutdown policy.

The safe limit is a configured operational boundary, not necessarily the device's critical or shutdown temperature. Critical-limit and shutdown-policy failures should use separate future catalog definitions.

### Humorous alternative messages

Extremely unusual but still valid values may eventually use restrained alternative wording. Such wording must never change the error ID, code, severity, categories, structured data, control flow, or shutdown decision. It is deliberately outside the first implementation slice.