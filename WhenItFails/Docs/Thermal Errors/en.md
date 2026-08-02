# Thermal errors

Thermal errors describe temperature-related states that can apply to computer hardware, batteries, power systems, motors, converters, servers, switchboards, industrial sensors, and general monitoring systems.

The thermal family deliberately separates ordinary and critical upper limits, ordinary and critical lower limits, sensor validity, data freshness, excessive temperature change, redundant-sensor disagreement, and failure of an application-selected thermal protection action. Applications must not infer that every warning requires an emergency stop, downgrade a critical boundary to an ordinary warning, treat invalid or stale sensor data as a current temperature, confuse a fast trend with an already-crossed absolute limit, assume that disagreement identifies which sensor is wrong, or hide the original thermal condition when its protective response fails.

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

Use this definition when a reported temperature exceeds the configured critical shutdown limit. It represents a boundary at which continued operation may be unsafe or may cause hardware damage, data loss, fire risk, battery failure, or another system-specific hazardous condition.

### Message

```text
The reported temperature {temperature}{unit} exceeds the configured critical shutdown limit of {limit}{unit}.
```

Required message parameters:

- `temperature` — the reported numeric temperature;
- `unit` — the unit suffix used consistently for the reported value and limit;
- `limit` — the configured critical shutdown limit.

Structured runtime data should preserve the measured value, unit, configured limit, sensor identity, affected component, timestamp, and shutdown-policy result when available and safe to expose.

### Developer guidance

Verify the sensor reading and unit conversion, activate the configured thermal shutdown policy, and inspect cooling, workload, and hardware before restart.

The definition describes the critical state; it does not itself stop hardware, terminate a process, disconnect power, or authorize restart. Those actions belong to the consuming application's safety policy and platform-specific control layer.

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

Use this definition when a sensor reports a value that is unavailable, malformed, physically implausible, outside its supported range, equal to a known sentinel value, or otherwise unreliable because of its content or acquisition state.

This contract does not say that the monitored component is hot, cold, safe, or unsafe. It says that the application does not have a trustworthy temperature value from that sensor. A structurally valid but outdated measurement belongs to `TEMPERATUREREADINGSTALE` instead.

### Message

```text
Temperature sensor {sensor} reported an invalid or unreliable reading.
```

Required message parameter:

- `sensor` — a stable sensor name, channel, identifier, or other safe label identifying the untrusted input.

Structured runtime data should preserve the raw reading, sensor identifier, expected unit, acquisition timestamp, source protocol, validation reason, and fallback availability when safe to expose.

### Developer guidance

Verify sensor availability, wiring or bus communication, raw values, unit conversion, stale-data handling, sentinel values, and the configured fail-safe policy.

Do not silently convert an invalid reading into zero, the last known value, an average, a safe default, or a fabricated temperature unless an explicit policy requires that fallback. Any substituted value must remain distinguishable from a current validated measurement.

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

A stale reading is not automatically an invalid numeric value and does not prove that a thermal limit was crossed. It means that the system no longer has sufficiently recent evidence to treat the value as current.

### Message

```text
Temperature reading from sensor {sensor} is stale; its age of {age} exceeds the configured maximum age of {maxAge}.
```

Required message parameters:

- `sensor` — the sensor or channel whose measurement is stale;
- `age` — the calculated age of the available reading;
- `maxAge` — the configured maximum acceptable age.

`age` and `maxAge` must use the same unambiguous representation. Prefer structured durations or a documented invariant duration format over locale-dependent free text.

### Developer guidance

Verify sensor polling, timestamps, clock synchronization, transport delays, buffering, cache invalidation, and the configured stale-data fail-safe policy.

Do not silently refresh an old timestamp or present a cached value as current merely because its number looks plausible. Reusing the last known reading is an application-policy decision and must preserve the fact that the value is stale.

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

Use this definition when trusted, current samples show a rate of change whose magnitude exceeds the maximum allowed by the configured thermal-trend policy.

The current absolute temperature may still be within all configured limits. This contract describes the trend itself, not proof that an absolute threshold has already been crossed.

### Message

```text
Temperature from sensor {sensor} changed at {rate}{unitPerTime}, exceeding the configured maximum rate of {maxRate}{unitPerTime}.
```

Required message parameters:

- `sensor` — the sensor or channel used for the trend calculation;
- `rate` — the calculated signed or absolute rate according to policy;
- `unitPerTime` — the shared temperature-per-time unit, such as `°C/s`;
- `maxRate` — the configured maximum permitted rate in the same unit.

The application must define whether the policy evaluates heating, cooling, or the absolute magnitude of either direction. Message formatting must not silently change that calculation.

### Developer guidance

Verify sampling intervals, timestamp order, unit conversion, filtering, workload changes, cooling response, and the configured thermal-trend policy.

A short interval can amplify noise into an unrealistic rate. Validate timestamp order, minimum observation interval, sample quality, smoothing rules, and whether the calculation spans a sensor reset or unit change.

## Temperature sensor disagreement

| Field | Value |
|---|---|
| ID | `AFW_THM_0006` |
| Code | `1000006` |
| Name | `TEMPERATURESENSORDISAGREEMENT` |
| Code group | `THERMAL` |
| Primary category | `THERMAL` |
| Default severity | `Warning` |
| Documentation key | `when-it-fails/errors/thermal/temperature-sensor-disagreement` |

Use this definition when two sensors expected to agree report trusted, current, unit-compatible values whose difference exceeds the maximum permitted by the configured redundancy policy.

This condition does not prove that either sensor is invalid. A real thermal gradient, different placement, response lag, calibration drift, timestamp skew, or a developing sensor fault can all produce disagreement.

### Message

```text
Temperature sensors {sensorA} and {sensorB} disagree by {difference}{unit}, exceeding the configured maximum difference of {maxDifference}{unit}.
```

Required message parameters:

- `sensorA` — the first sensor or channel;
- `sensorB` — the second sensor or channel;
- `difference` — the calculated difference;
- `unit` — the common temperature unit;
- `maxDifference` — the configured maximum permitted difference.

### Developer guidance

Verify placement, calibration, sampling timestamps, unit conversion, thermal gradients, wiring or transport integrity, and the configured redundancy policy.

Do not automatically mark one sensor as failed merely because two readings disagree. Selecting a preferred sensor, voting, entering degraded mode, or stopping operation belongs to the application's redundancy and fail-safe policy.

## Temperature below minimum limit

| Field | Value |
|---|---|
| ID | `AFW_THM_0007` |
| Code | `1000007` |
| Name | `TEMPERATUREBELOWMINIMUMLIMIT` |
| Code group | `THERMAL` |
| Primary category | `THERMAL` |
| Default severity | `Warning` |
| Documentation key | `when-it-fails/errors/thermal/temperature-below-minimum-limit` |

Use this definition when a trusted, current temperature is below the configured minimum operating limit for the monitored component, material, battery, fluid, process, or environment.

This condition is the lower-bound counterpart to the ordinary upper safe-limit warning. It does not mean that the sensor is invalid and does not automatically imply emergency shutdown.

### Message

```text
The reported temperature {temperature}{unit} is below the configured minimum operating limit of {limit}{unit}.
```

Required message parameters:

- `temperature` — the reported numeric temperature;
- `unit` — the unit suffix used for the reading and limit;
- `limit` — the configured minimum operating limit.

### Developer guidance

Verify the sensor reading, unit conversion, ambient conditions, heating path, warm-up requirements, configured limits, and low-temperature operating policy.

A low-temperature condition can justify delayed startup, reduced load, inhibited charging, preheating, viscosity checks, condensation precautions, notification, or degraded operation. The catalog does not prescribe the action and does not itself start a heater, stop a process, or authorize restart.

## Critical temperature below minimum limit

| Field | Value |
|---|---|
| ID | `AFW_THM_0008` |
| Code | `1000008` |
| Name | `CRITICALTEMPERATUREBELOWMINIMUMLIMIT` |
| Code group | `THERMAL` |
| Primary category | `THERMAL` |
| Default severity | `Critical` |
| Documentation key | `when-it-fails/errors/thermal/critical-temperature-below-minimum-limit` |

Use this definition when a trusted, current temperature falls below the configured critical minimum operating limit. Continued operation, charging, fluid circulation, movement, startup, or another activity may cause damage or create an unsafe condition.

The critical state does not mean that the sensor is invalid. It means that a trusted value crossed the application's critical lower threshold.

### Message

```text
The reported temperature {temperature}{unit} is below the configured critical minimum operating limit of {limit}{unit}.
```

Required message parameters:

- `temperature` — the reported numeric temperature;
- `unit` — the unit suffix used for the reading and critical minimum;
- `limit` — the configured critical minimum operating limit.

### Developer guidance

Verify the sensor reading and unit conversion, activate the configured critical low-temperature policy, and inspect ambient conditions, heating, fluids, batteries, and hardware before restart.

The definition describes the critical condition; it does not itself stop equipment, isolate a battery, inhibit charging, start a heater, manipulate fluid, terminate a process, or authorize restart.

## Thermal protection action failed

| Field | Value |
|---|---|
| ID | `AFW_THM_0009` |
| Code | `1000009` |
| Name | `THERMALPROTECTIONACTIONFAILED` |
| Code group | `THERMAL` |
| Primary category | `THERMAL` |
| Default severity | `Critical` |
| Documentation key | `when-it-fails/errors/thermal/thermal-protection-action-failed` |

Use this definition when an application has already detected a thermal condition, selected a protective response according to its policy, attempted that response, and could not complete or verify it.

The original thermal condition remains valid and should normally be reported alongside this failure. For example, a critical high-temperature condition does not disappear merely because the shutdown command failed. This contract records the second fact: the selected response did not reach its required result.

### Message

```text
Thermal protection action {action} failed for {component} while handling {condition}.
```

Required message parameters:

- `action` — the concrete protection action selected by the application, such as shutdown, workload reduction, charging inhibition, battery isolation, heater activation, valve movement, or operator escalation;
- `component` — the affected component, device, process, zone, or other safe operational identifier;
- `condition` — the triggering thermal condition, preferably represented by a stable error ID or name rather than free-form text alone.

Structured runtime data should preserve the triggering error ID and code, selected action, command or actuator target, request timestamp, completion deadline, returned status, exception or provider code, observed hardware state, retry count, fallback result, operator escalation state, and evidence required before restart. Sensitive command payloads, credentials, network details, and raw hardware responses must not be copied into user-facing text.

### Developer guidance

Verify the selected protection policy, actuator or control path, permissions, command result, hardware state, fallback action, operator escalation, and evidence required before restart.

Do not emit this definition merely because a thermal threshold was crossed. It applies only after a specific protection action was selected and then failed, timed out, was rejected, or could not be verified. A policy decision not to shut down is not a shutdown failure.

Do not replace the triggering thermal error with this definition. Preserve both contracts so diagnostics can answer two separate questions: what thermal condition occurred, and why the intended response did not protect the system.

The catalog does not prescribe automatic retries. Repeating an actuator command can itself be unsafe, destructive, or misleading. Retry count, timing, idempotency, fallback actions, independent state verification, and escalation belong to the consuming application's safety policy.

A successful command acknowledgement is not always proof that the physical action completed. Safety-sensitive consumers should verify the resulting state through an independent signal when available. Until completion is verified, the system should not infer that shutdown, isolation, cooling, heating, throttling, or another protection is active.

Clearing this critical error normally requires evidence that the selected action or approved fallback completed, the triggering thermal condition is controlled, and restart or return to normal operation has been explicitly authorized by policy. A later normal temperature alone does not prove that the protection path is healthy.

## Choosing the correct definition

Use `AFW_THM_0001` when a trusted current temperature crosses the ordinary upper limit but not the critical upper limit.

Use `AFW_THM_0002` when a trusted current temperature crosses the critical upper boundary and emergency policy must be evaluated.

Use `AFW_THM_0003` when the sensor value itself cannot be trusted.

Use `AFW_THM_0004` when an otherwise plausible value is too old to be treated as current.

Use `AFW_THM_0005` when trusted current samples exceed the configured temperature-change rate.

Use `AFW_THM_0006` when trusted comparable sensors disagree beyond the redundancy tolerance and the disagreement alone cannot identify the faulty input.

Use `AFW_THM_0007` when a trusted current temperature crosses the ordinary lower operating limit but not the critical lower limit.

Use `AFW_THM_0008` when a trusted current temperature crosses the critical lower boundary and critical low-temperature policy must be evaluated.

Use `AFW_THM_0009` when a thermal condition has already been detected, a specific protective response was selected and attempted, and that response failed or could not be verified.

More than one definition may legitimately apply at the same time. Threshold, validity, freshness, trend, and disagreement errors describe thermal evidence. `AFW_THM_0009` describes failure of the selected response and therefore normally coexists with the triggering thermal error rather than replacing it.

Do not select definitions from severity text alone. The validated measurement, timestamp, comparison, configured threshold, selected policy action, and verified action result are the relevant sources of truth.

## Humorous alternative messages

Extremely unusual but still valid values may eventually use restrained alternative wording. Such wording must never change the error ID, code, severity, categories, structured data, control flow, shutdown decision, restart policy, sensor-trust decision, data-freshness decision, thermal-trend decision, redundancy decision, low-temperature decision, critical low-temperature decision, protection-action decision, or fail-safe policy. It is deliberately outside the current implementation slice.
