# Thermal errors

Thermal errors describe temperature-related states that can apply to computer hardware, batteries, power systems, motors, converters, servers, switchboards, industrial sensors, and general monitoring systems.

The thermal family deliberately separates an ordinary safe-limit warning, a critical shutdown-limit condition, an invalid sensor reading, a stale reading, and an excessive rate of temperature change. Applications must not infer that every thermal warning requires an emergency stop, must not downgrade a critical shutdown-limit state to an ordinary warning, must not treat an invalid sensor value as a confirmed temperature, must not treat an old measurement as current, and must not confuse a fast trend with an already-crossed absolute limit.

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

Use this definition when a temperature sensor reports a value that is unavailable, malformed, physically implausible, outside the sensor's supported range, equal to a known sentinel value, or otherwise unreliable because of its content or acquisition state.

This contract does not say that the monitored component is hot, cold, safe, or unsafe. It says that the application does not have a trustworthy temperature value from that sensor. A structurally valid but outdated measurement belongs to `TEMPERATUREREADINGSTALE` instead.

### Message

```text
Temperature sensor {sensor} reported an invalid or unreliable reading.
```

Required message parameter:

- `sensor` — a stable sensor name, channel, identifier, or other safe label that tells the operator which input cannot be trusted.

Structured runtime data should preserve the raw reading, sensor identifier, expected unit, acquisition timestamp, source protocol, validation reason, and whether a fallback source was available when those values are safe to expose.

### Developer guidance

Verify sensor availability, wiring or bus communication, raw values, unit conversion, stale-data handling, sentinel values, and the configured fail-safe policy.

Do not silently convert an invalid reading into zero, the last known value, an average, a safe default, or a fabricated temperature unless the consuming system has an explicit and documented policy for that fallback. Any substituted value must remain distinguishable from a current validated measurement.

The error definition does not itself choose whether the system should continue, throttle, stop, switch to a redundant sensor, or enter an emergency state. That decision belongs to the application's fail-safe policy and should consider the monitored equipment, redundancy, recent validated readings, and consequences of operating without temperature feedback.

A later valid sample may clear the immediate reading error, but applications should consider hysteresis, repeated-failure counts, sensor-health history, and independent verification before restoring full trust or automatically resuming operation.

## Temperature reading stale

| Field | Value |
|---|---|
| ID | `AFW_THM_0004` |
| Code | `1000004` |
| Name | `TEMPERATUREREADINGSTALE` |
| Code group | `THERMAL` |
| Primary category | `THERMAL` |
| Default severity | `Error` |
| Documentation key | `when-it-fails/errors/thermal/temperature-reading-stale` |

Use this definition when the temperature value may be structurally valid and plausible, but its age exceeds the maximum allowed by the consuming application's freshness policy.

A stale reading is not automatically an invalid numeric value and does not prove that a thermal limit was crossed. It means that the system no longer has sufficiently recent evidence to treat the value as the current temperature.

### Message

```text
Temperature reading from sensor {sensor} is stale; its age of {age} exceeds the configured maximum age of {maxAge}.
```

Required message parameters:

- `sensor` — the sensor or channel whose measurement is stale;
- `age` — the calculated age of the available reading;
- `maxAge` — the configured maximum acceptable age.

`age` and `maxAge` must use the same unambiguous representation. Prefer structured durations or a documented invariant duration format over locale-dependent free text.

Structured runtime data should preserve the measurement timestamp, evaluation timestamp, calculated age, configured maximum age, clock source, sensor identity, transport or polling state, and whether a newer fallback source was available.

### Developer guidance

Verify sensor polling, timestamps, clock synchronization, transport delays, buffering, cache invalidation, and the configured stale-data fail-safe policy.

Do not silently refresh the timestamp of an old value, and do not present a cached value as current merely because its number looks plausible. Reusing the last known reading is an application policy decision and must preserve the fact that the value is stale.

A stale-reading error may clear when a new validated sample arrives. Consumers should still consider repeated polling failures, clock jumps, queue backlogs, delayed telemetry, and redundant-sensor agreement before restoring normal operation.

The catalog does not prescribe whether stale data causes throttling, degraded operation, sensor failover, shutdown, or operator intervention. Those actions belong to the consuming application's freshness and fail-safe policies.

## Temperature rate of change exceeded

| Field | Value |
|---|---|
| ID | `AFW_THM_0005` |
| Code | `1000005` |
| Name | `TEMPERATURERATEOFCHANGEEXCEEDED` |
| Code group | `THERMAL` |
| Primary category | `THERMAL` |
| Default severity | `Warning` |
| Documentation key | `when-it-fails/errors/thermal/temperature-rate-of-change-exceeded` |

Use this definition when trusted, current temperature samples show a rate of change whose magnitude exceeds the maximum allowed by the configured thermal-trend policy.

The current absolute temperature may still be below both the safe and critical limits. This contract therefore describes the trend itself, not proof that an absolute threshold has already been crossed.

### Message

```text
Temperature from sensor {sensor} changed at {rate}{unitPerTime}, exceeding the configured maximum rate of {maxRate}{unitPerTime}.
```

Required message parameters:

- `sensor` — the sensor or channel used for the trend calculation;
- `rate` — the calculated signed or absolute rate of temperature change, according to policy;
- `unitPerTime` — the shared temperature-per-time unit, such as `°C/s`;
- `maxRate` — the configured maximum permitted rate in the same unit.

The consuming application must define whether the policy evaluates only heating, only cooling, or the absolute magnitude of either direction. The message must not hide that choice by silently changing the sign of `rate`.

Structured runtime data should preserve both samples, their timestamps, the elapsed interval, calculated rate, selected unit, configured maximum rate, filtering or smoothing method, sensor identity, and any absolute limits that were also crossed.

### Developer guidance

Verify sensor sampling intervals, timestamp order, unit conversion, filtering, workload changes, cooling response, and the configured thermal trend policy.

A single short interval can amplify sensor noise into an unrealistic rate. Consumers should validate timestamp order, minimum observation interval, sample quality, smoothing rules, and whether the calculation spans a sensor reset or unit change.

This warning does not itself command throttling, shutdown, or process termination. The application may increase sampling, reduce workload, increase cooling, raise operator attention, or escalate to another error when its configured policy or an absolute limit requires it.

A later slower sample does not necessarily prove recovery. Applications should consider hysteresis, a stable observation window, repeated trend breaches, and the resulting absolute temperature before clearing the condition.

## Choosing the correct definition

Use `AFW_THM_0001` when:

- the temperature reading is trusted and current;
- the safe operating limit was exceeded;
- continued operation may still be allowed by policy;
- throttling, cooling, workload reduction, or operator attention is appropriate;
- the configured critical shutdown threshold has not been crossed.

Use `AFW_THM_0002` when:

- the temperature reading is trusted and current;
- the configured critical shutdown limit was exceeded;
- the application must evaluate or activate its emergency thermal policy;
- continued operation may cause damage or create an unsafe condition;
- inspection is required before restart.

Use `AFW_THM_0003` when:

- the reported value cannot be trusted because it is missing, malformed, implausible, out of sensor range, or a sentinel value;
- no limit or trend comparison can be made safely from that value;
- the application must follow its configured sensor-loss or invalid-reading policy.

Use `AFW_THM_0004` when:

- the available value may be numerically valid and plausible;
- the measurement timestamp is known or its age can be established;
- the reading is older than the configured maximum age;
- the application must follow its stale-data or freshness fail-safe policy.

Use `AFW_THM_0005` when:

- the samples are trusted, current, correctly ordered, and use compatible units;
- the calculated temperature change per unit of time exceeds the configured trend limit;
- the current absolute temperature may still be below its safe or critical threshold;
- the application must evaluate its thermal-trend response policy.

More than one definition may legitimately apply at the same time. For example, a fast temperature rise can trigger `AFW_THM_0005` before later samples also trigger `AFW_THM_0001` or `AFW_THM_0002`. Do not suppress the absolute-limit condition merely because the trend warning was emitted first.

Do not select between these definitions from severity text alone. For limit errors, a trusted current measurement and configured threshold are the source of truth. For `AFW_THM_0003`, content or acquisition validity is the source of truth. For `AFW_THM_0004`, measurement age is the source of truth. For `AFW_THM_0005`, the validated rate calculation and configured trend threshold are the source of truth.

## Humorous alternative messages

Extremely unusual but still valid values may eventually use restrained alternative wording. Such wording must never change the error ID, code, severity, categories, structured data, control flow, shutdown decision, restart policy, sensor-trust decision, data-freshness decision, thermal-trend decision, or fail-safe policy. It is deliberately outside the current implementation slice.
