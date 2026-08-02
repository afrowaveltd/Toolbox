# Thermal errors

Thermal errors describe temperature-related states that can apply to computer hardware, batteries, power systems, motors, converters, servers, switchboards, industrial sensors, and general monitoring systems.

The thermal family deliberately separates ordinary and critical upper limits, ordinary and critical lower limits, sensor validity, data freshness, excessive temperature change, redundant-sensor disagreement, confirmed protection-action failure, an action whose outcome could not be verified, and confirmed failure of an approved fallback response. Applications must not infer that every warning requires an emergency stop, downgrade a critical boundary to an ordinary warning, treat invalid or stale data as current, confuse a fast trend with an absolute-limit breach, assume sensor disagreement identifies the faulty input, hide the triggering thermal condition, treat an unknown protection result as either success or confirmed failure, or report fallback failure merely because a fallback was available or considered.

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

Use this definition when a trusted, current temperature exceeds the configured ordinary safe operating limit but has not crossed the critical upper boundary.

### Message

```text
The reported temperature {temperature}{unit} exceeds the configured safe limit of {limit}{unit}.
```

Required parameters are `temperature`, `unit`, and `limit`. Preserve their numeric values and unit in structured runtime data.

### Developer guidance

Verify the sensor reading, unit conversion, cooling path, workload, configured limits, and shutdown policy. The catalog does not itself throttle, cool, stop equipment, or authorize restart.

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

Use this definition when a trusted, current temperature exceeds the configured critical shutdown limit and continued operation may cause damage or create an unsafe condition.

### Message

```text
The reported temperature {temperature}{unit} exceeds the configured critical shutdown limit of {limit}{unit}.
```

Required parameters are `temperature`, `unit`, and `limit`.

### Developer guidance

Verify the reading and conversion, activate the configured critical-temperature policy, and inspect cooling, workload, and hardware before restart. The definition describes the condition; the consuming application owns shutdown and restart decisions.

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

Use this definition when a temperature value is unavailable, malformed, physically implausible, outside the sensor range, a known sentinel, or otherwise untrustworthy.

### Message

```text
Temperature sensor {sensor} reported an invalid or unreliable reading.
```

Required parameter `sensor` identifies the affected input.

### Developer guidance

Verify sensor availability, wiring or bus communication, raw values, unit conversion, sentinel handling, and fail-safe policy. Do not silently turn an invalid value into zero, a cached value, an average, or a fabricated safe value.

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

Use this definition when the available value may be plausible but its age exceeds the configured freshness limit.

### Message

```text
Temperature reading from sensor {sensor} is stale; its age of {age} exceeds the configured maximum age of {maxAge}.
```

Required parameters are `sensor`, `age`, and `maxAge`. The two durations must use the same unambiguous representation.

### Developer guidance

Verify polling, timestamps, clock synchronization, transport delays, buffering, and cache invalidation. Do not refresh an old timestamp or present cached data as current.

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

Use this definition when trusted, current samples exceed the maximum temperature-change rate selected by policy. The absolute temperature may still be within all configured limits.

### Message

```text
Temperature from sensor {sensor} changed at {rate}{unitPerTime}, exceeding the configured maximum rate of {maxRate}{unitPerTime}.
```

Required parameters are `sensor`, `rate`, `unitPerTime`, and `maxRate`.

### Developer guidance

Verify sampling intervals, timestamp order, unit conversion, filtering, workload changes, cooling response, and whether the policy evaluates heating, cooling, or absolute magnitude.

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

Use this definition when trusted, current, comparable sensor values differ by more than the configured redundancy tolerance.

### Message

```text
Temperature sensors {sensorA} and {sensorB} disagree by {difference}{unit}, exceeding the configured maximum difference of {maxDifference}{unit}.
```

Required parameters are `sensorA`, `sensorB`, `difference`, `unit`, and `maxDifference`.

### Developer guidance

Verify placement, calibration, timestamps, units, thermal gradients, wiring, and transport. Disagreement alone does not identify which sensor is wrong; source selection and voting belong to application policy.

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

Use this definition when a trusted, current temperature is below the ordinary minimum operating limit but has not crossed the critical lower boundary.

### Message

```text
The reported temperature {temperature}{unit} is below the configured minimum operating limit of {limit}{unit}.
```

Required parameters are `temperature`, `unit`, and `limit`.

### Developer guidance

Verify the reading, units, ambient conditions, heating path, warm-up requirements, and low-temperature policy. Delayed startup, reduced load, inhibited charging, preheating, or degraded operation remain application decisions.

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

Use this definition when a trusted, current temperature crosses the configured critical lower boundary and continued operation or another activity may cause damage or create an unsafe condition.

### Message

```text
The reported temperature {temperature}{unit} is below the configured critical minimum operating limit of {limit}{unit}.
```

Required parameters are `temperature`, `unit`, and `limit`.

### Developer guidance

Verify the reading and conversion, activate the configured critical low-temperature policy, and inspect ambient conditions, heating, fluids, batteries, and hardware before restart. The catalog does not itself stop equipment, isolate a battery, start a heater, or authorize restart.

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

Use this definition when a thermal condition was detected, a concrete protective action was selected and attempted, and evidence shows that the action failed, was rejected, timed out as defined by policy, or did not reach its required result.

The triggering thermal condition remains valid and should normally be reported alongside this failure.

### Message

```text
Thermal protection action {action} failed for {component} while handling {condition}.
```

Required parameters are `action`, `component`, and `condition`.

### Developer guidance

Verify the selected policy, actuator or control path, permissions, command result, hardware state, fallback action, operator escalation, and evidence required before restart. Do not emit this contract merely because a threshold was crossed, and do not replace the triggering thermal error with it.

Automatic retries are not implied. Repeating an actuator command can be unsafe or non-idempotent. Retry timing, fallback, independent state verification, and escalation belong to application policy.

## Thermal protection action unverified

| Field | Value |
|---|---|
| ID | `AFW_THM_0010` |
| Code | `1000010` |
| Name | `THERMALPROTECTIONACTIONUNVERIFIED` |
| Code group | `THERMAL` |
| Primary category | `THERMAL` |
| Default severity | `Critical` |
| Documentation key | `when-it-fails/errors/thermal/thermal-protection-action-unverified` |

Use this definition when a thermal protection command was issued or initiated, but the application cannot obtain trustworthy evidence that the required physical or logical result completed. Examples include a missing acknowledgement, expired verification deadline, stale actuator telemetry, lost correlation, contradictory feedback, or an unavailable independent state signal.

This is an indeterminate outcome, not confirmed success and not confirmed failure. Use `THERMALPROTECTIONACTIONFAILED` only when evidence establishes failure. The triggering thermal condition remains visible in either case.

### Message

```text
Thermal protection action {action} for {component} could not be verified while handling {condition}.
```

Required message parameters:

- `action` — the concrete protection action whose result is unknown;
- `component` — the affected component, device, process, zone, or safe operational identifier;
- `condition` — the triggering thermal condition, preferably represented by a stable error ID or name.

Structured runtime data should preserve the triggering error, command and correlation identifiers, target, request time, acknowledgement state, verification deadline, last trustworthy telemetry, feedback source, retry count, fallback result, operator escalation, and the evidence required before restart. Sensitive command payloads and credentials must not appear in user-facing text.

### Developer guidance

Verify command delivery, acknowledgement, actuator feedback, telemetry freshness, correlation identifiers, timeout policy, fallback action, operator escalation, and evidence required before restart.

Do not mark the action successful merely because the command was accepted or transmitted. Command acceptance proves only that a request entered some part of the control path. Where safety matters, completion should be verified from the resulting state or another policy-approved independent signal.

Do not automatically reissue the command. The first command may have completed even though its acknowledgement was lost, so an uncontrolled retry can repeat a destructive or non-idempotent action. Retry, reconciliation, fallback, and manual intervention belong to application policy.

Clear this critical state only after the action or approved fallback is verified, the triggering thermal condition is controlled, and restart or return to normal operation is explicitly authorized. A later normal temperature or delayed acknowledgement alone may be insufficient when correlation and freshness cannot be established.

## Thermal fallback protection action failed

| Field | Value |
|---|---|
| ID | `AFW_THM_0011` |
| Code | `1000011` |
| Name | `THERMALFALLBACKPROTECTIONACTIONFAILED` |
| Code group | `THERMAL` |
| Primary category | `THERMAL` |
| Default severity | `Critical` |
| Documentation key | `when-it-fails/errors/thermal/thermal-fallback-protection-action-failed` |

Use this definition when the application has already detected a thermal condition, evaluated the result of its primary protective response, selected an approved fallback according to policy, attempted that fallback, and obtained evidence that the fallback failed to reach its required result.

This contract does not mean merely that a fallback was configured, available, recommended, or considered. It requires runtime evidence that the specific fallback was selected and attempted. The triggering thermal condition and the result of the primary action remain separate facts and should normally remain visible.

### Message

```text
Thermal fallback protection action {fallbackAction} failed for {component} after {primaryAction} while handling {condition}.
```

Required message parameters:

- `fallbackAction` — the approved fallback response that was selected and attempted;
- `component` — the affected component, device, process, zone, or safe operational identifier;
- `primaryAction` — the primary protective action whose failure or unverified result caused fallback evaluation;
- `condition` — the triggering thermal condition, preferably represented by a stable error ID or name.

Structured runtime data should preserve the triggering error, primary-action ID and result, fallback-selection reason, fallback command and correlation identifiers, target, attempt time, deadline, returned status, observed state, remaining approved options, operator escalation, and evidence required before restart. Sensitive command payloads, credentials, and raw control-channel data must not appear in user-facing text.

### Developer guidance

Verify the primary action result, fallback selection policy, fallback actuator or control path, command result, hardware state, remaining safe options, operator escalation, and evidence required before restart.

Do not emit this definition when the primary action fails unless a distinct fallback was actually selected and attempted. Do not use it for a repeated attempt of the same primary command unless policy explicitly models that attempt as a separate fallback action.

Failure of a fallback does not automatically prove that every possible protective option is exhausted. The application must evaluate only approved remaining actions and must not invent an unsafe third response. Conversely, absence of another automated fallback must not be treated as permission to continue normal operation.

Automatic retries are not implied. A failed fallback may involve an actuator or state transition for which repetition is unsafe, destructive, or non-idempotent. Retry, alternate fallback, manual intervention, isolation, emergency stop, evacuation, and external escalation belong to the consuming application's safety policy.

Clear this critical state only after a policy-approved protective result is independently verified, the triggering thermal condition is controlled, and restart or return to normal operation is explicitly authorized. A normal temperature sample alone does not prove that either the primary or fallback protection path is healthy.

## Choosing the correct definition

Use `AFW_THM_0001` for an ordinary upper-limit breach and `AFW_THM_0002` for the critical upper boundary.

Use `AFW_THM_0003` when the value itself is untrustworthy and `AFW_THM_0004` when a plausible value is too old.

Use `AFW_THM_0005` for an excessive validated trend and `AFW_THM_0006` for redundant-sensor disagreement.

Use `AFW_THM_0007` for an ordinary lower-limit breach and `AFW_THM_0008` for the critical lower boundary.

Use `AFW_THM_0009` when evidence confirms that the selected primary protection action failed.

Use `AFW_THM_0010` when the primary action was issued or initiated but its required result cannot be verified. Do not collapse an unknown outcome into either success or confirmed failure.

Use `AFW_THM_0011` when policy selected and attempted a distinct fallback action and evidence confirms that this fallback also failed.

More than one definition may legitimately apply at the same time. Measurement, validity, freshness, trend, disagreement, and threshold contracts describe thermal evidence. Protection-action contracts describe what happened after policy selected a response and therefore normally coexist with the triggering thermal error. `AFW_THM_0011` may also coexist with `AFW_THM_0009` or `AFW_THM_0010` because primary and fallback outcomes are separate facts.

Do not select definitions from severity text alone. The validated measurement, timestamp, comparison, configured threshold, selected policy action, command evidence, fallback-selection evidence, and verified resulting state are the relevant sources of truth.

## Humorous alternative messages

Extremely unusual but valid values may eventually use restrained alternative wording. Such wording must never change the error ID, code, severity, categories, structured data, control flow, shutdown decision, restart policy, sensor-trust decision, data-freshness decision, thermal-trend decision, redundancy decision, low-temperature decision, critical low-temperature decision, protection-action decision, action-verification decision, fallback-action decision, or fail-safe policy. It is deliberately outside the current implementation slice.
