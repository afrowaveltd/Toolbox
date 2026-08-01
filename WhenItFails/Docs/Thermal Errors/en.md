# Thermal errors

Thermal errors describe temperature-related states that can apply to computer hardware, batteries, power systems, motors, converters, servers, switchboards, industrial sensors, and general monitoring systems.

The thermal family deliberately separates an ordinary safe-limit warning, a critical shutdown-limit condition, and the loss of a trustworthy sensor reading. Applications must not infer that every thermal warning requires an emergency stop, must not downgrade a critical shutdown-limit state to an ordinary warning, and must not treat an invalid sensor value as a confirmed temperature.

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

The safe limit is a configured operational boundary, not necessarily the device's critical or shutdown temperature. Crossing it can justify throttling, reduced workload, increased cooling, operator notification, or closer monitoring without necessarily requiring immediate shutdown.

## Critical temperature limit exceeded

| Field | Value |
|---|---|
| ID | `AFW_THM_0002` |
| Code | `1000002` |
| Name | `CRITICALTEMPERATURELIMITEXCEEDED` |
| Code group | `THERMAL` |
| Primary category | `THERMAL` |
| Default severity | `Critical` |
| Documentation key | `when-it-fails/errors/thermal/critical-temperature-limit-exceeded` |

Use this definition when a reported temperature exceeds the configured critical shutdown limit. This is a separate contract from `TEMPERATURELIMITEXCEEDED`; it represents a boundary at which continued operation may be unsafe or may cause hardware damage, data loss, fire risk, battery failure, or another system-specific hazardous condition.

### Message

```text
The reported temperature {temperature}{unit} exceeds the configured critical shutdown limit of {limit}{unit}.
```

Required message parameters:

- `temperature` — the reported numeric temperature;
- `unit` — the unit suffix used consistently for the reported value and limit;
- `limit` — the configured critical shutdown limit.

Structured runtime data should also preserve the measured value, unit, configured limit, sensor identity, affected component, timestamp, and shutdown-policy result when those values are available and safe to expose.

### Developer guidance

Verify the sensor reading and unit conversion, activate the configured thermal shutdown policy, and inspect cooling, workload, and hardware before restart.

The error definition describes the critical state; it does not itself stop hardware, terminate a process, disconnect power, or decide whether restart is permitted. Those actions belong to the consuming application's safety policy and platform-specific control layer.

A consumer should normally treat this state as non-retryable until the temperature has returned to an acceptable range and the underlying cause has been inspected. Automatic restart must not be inferred from the disappearance of a single high-temperature reading.

## Temperature sensor reading invalid

| Field | Value |
|---|---|
| ID | `AFW_THM_0003` |
| Code | `1000003` |
| Name | `TEMPERATURESENSORREADINGINVALID` |
| Code group | `THERMAL` |
| Primary category | `THERMAL` |
| Default severity | `Error` |
| Documentation key | `when-it-fails/errors/thermal/temperature-sensor-reading-invalid` |

Use this definition when a temperature sensor reports a value that is invalid, stale, unavailable, physically implausible, outside the sensor's supported range, equal to a known sentinel value, or otherwise unreliable.

This contract does not say that the monitored component is hot, cold, safe, or unsafe. It says that the application no longer has a trustworthy temperature input for that sensor.

### Message

```text
Temperature sensor {sensor} reported an invalid or unreliable reading.
```

Required message parameter:

- `sensor` — a stable sensor name, channel, identifier, or other safe label that tells the operator which input cannot be trusted.

Structured runtime data should preserve the raw reading, sensor identifier, expected unit, acquisition timestamp, data age, source protocol, validation reason, and whether a fallback source was available when those values are safe to expose.

### Developer guidance

Verify sensor availability, wiring or bus communication, raw values, unit conversion, stale-data handling, sentinel values, and the configured fail-safe policy.

Do not silently convert an invalid reading into zero, the last known value, an average, a safe default, or a fabricated temperature unless the consuming system has an explicit and documented policy for that fallback. Any substituted value must remain distinguishable from a current validated measurement.

The error definition does not itself choose whether the system should continue, throttle, stop, switch to a redundant sensor, or enter an emergency state. That decision belongs to the application's fail-safe policy and should consider the monitored equipment, redundancy, recent validated readings, and consequences of operating without temperature feedback.

A later valid sample may clear the immediate reading error, but applications should consider hysteresis, repeated-failure counts, sensor-health history, and independent verification before restoring full trust or automatically resuming operation.

## Choosing the correct definition

Use `AFW_THM_0001` when:

- the temperature reading is trusted;
- the safe operating limit was exceeded;
- continued operation may still be allowed by policy;
- throttling, cooling, workload reduction, or operator attention is appropriate;
- the configured critical shutdown threshold has not been crossed.

Use `AFW_THM_0002` when:

- the temperature reading is trusted;
- the configured critical shutdown limit was exceeded;
- the application must evaluate or activate its emergency thermal policy;
- continued operation may cause damage or create an unsafe condition;
- inspection is required before restart.

Use `AFW_THM_0003` when:

- the reported value cannot be trusted as a valid current temperature;
- the sensor is unavailable, stale, implausible, malformed, or returning a sentinel value;
- no limit comparison can be made safely from that reading;
- the application must follow its configured sensor-loss or fail-safe policy.

Do not select between these definitions from severity text alone. For limit errors, the trusted measurement and configured threshold are the source of truth. For the sensor-reading error, the loss of measurement trust is the source of truth.

## Humorous alternative messages

Extremely unusual but still valid values may eventually use restrained alternative wording. Such wording must never change the error ID, code, severity, categories, structured data, control flow, shutdown decision, restart policy, sensor-trust decision, or fail-safe policy. It is deliberately outside the current implementation slice.
