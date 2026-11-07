# JSignalR assessment and optional AJIS/Backpack integration

**Source review:** 2026-10-27. Repository: [afrowaveltd/JSignalR](https://github.com/afrowaveltd/JSignalR), branch `main`; source inspection **only**, no tests executed. This is **not** part of Backpack's implementation.

## What JSignalR actually is

A **Node.js SignalR-compatible WebSocket server**, not a custom JS SignalR client and not a wrapper around the existing ASP.NET server. [README](https://github.com/afrowaveltd/JSignalR/blob/main/README.md) explicitly limits it to **SignalR JSON Hub Protocol over WebSockets**; no SSE, long polling, MessagePack, streaming, durable resume or scale-out. [`package.json`](https://github.com/afrowaveltd/JSignalR/blob/main/package.json): `jsignalr` `0.1.0`, runtime dependency `ws`; `@microsoft/signalr` is currently a development/test dependency.

Verified in sources:
- `ConnectionManager`, `GroupManager`, `NegotiationManager`, `PendingMessageQueue`; small module separation;
- handshake/protocol parser, record separator `0x1E`, invocation and completion, server messages, group broadcasts, ping/idle control, lifecycle callbacks;
- standard Microsoft `@microsoft/signalr` client in [integration compatibility tests](https://github.com/afrowaveltd/JSignalR/blob/main/tests/integration/compatibility.test.ts), covering two connect paths, invocation, push, groups.

## Distinct roles alongside Backpack and JSLinq

```text
Backpack/AJIS      => stable portable data model + serialization
AjisFS             => persistent objects, index, scan/read/offset primitives
JSLinq             => query/filter/project/aggregate in JS/TS
JSignalR           => online updates, hub calls, event notifications
(optional bridges) => translation between these boundaries
```

**Example:** An AjisFS index holds records; JSLinq queries them locally. JSignalR sends `RecordChanged({id, revision})` to connected clients. Clients then load changed AJIS objects using appropriate data-access methods. Do not send an entire large data store on every update.

**Protocol constraint:** JSignalR's SignalR wire format remains JSON framed with `0x1E`. Ordinary JSON-safe AJIS values can be carried as Hub method arguments, but an arbitrary extended AJIS document containing directives, references and binary attachments **cannot simply replace that wire framing** while claiming compatibility with an unmodified `@microsoft/signalr` client. Options are AJIS text encoded as a string, a JSON-compatible projection, an opaque payload with explicit application-level encoding/versioning, or references to separately streamed/stored binary data. A custom SignalR Hub protocol requires both parties to opt in and must be separately tested. JSignalR should stay **optional** for AJIS/Backpack.

## Verified issues and follow-ups before external production use

1. **README/API mismatch:** README quickstart uses `new SignalRServer({port:5000,...}); server.start()`; current [`SignalROptions`](https://github.com/afrowaveltd/JSignalR/blob/main/src/server/signalr-server.ts) instead *requires* `server: HttpServer | HttpsServer`, and `SignalRServer` has no `start()` method. Fix example or add explicitly designed convenience overload; do not claim docs run as written.
2. **Negotiate token identification:** `WebSocketServer` emits a server-side `connection(socket, request)` event, but current handler only captures `socket` and tries `(socket as any).url`. The URL/query is normally on the **upgrade request**, not reliably on the accepted server-side socket. The token-consumption path is therefore suspect/nonfunctional, while current integration tests mainly assert client connect/invoke, not that the *negotiated token was consumed*. Wire the upgrade request into `_handleConnection`, test unknown/expired/valid IDs, and explicitly decide reject-vs-guest policy.
3. **Authentication/security:** README itself says no auth and no user claims. Add an explicit authentication/authorization mechanism, hub/method permission checks, TLS/proxy/origin policy and defensible token validation before exposing privileged hubs publicly. A negotiate token/connection ID is not user authorization.
4. **Bounded resource use:** pre-handshake outbound queue is bounded, but raw incoming text buffer (`state.buffer`) and `/negotiate` request body currently have no explicit application-level length cap. Impose frame/body size limits, backpressure and per-client message-rate controls. Node's default WS limits do not substitute for application-level quotas.
5. **Async ordering/lifecycle:** `_handleMessage` invokes an asynchronous `_handleProtocolMessage` without awaiting or serializing it, so concurrent invocations and late completions after disconnect need explicit design/tests. Distinguish allowed concurrent execution from guaranteed message order; avoid sending to a closed connection.
6. **Hub registration:** decorator discovery walks only the immediate prototype, not inherited hub methods; test inheritance/overrides before promising them. Avoid brittle reflection if a small explicit registration API is sufficient.
7. **Repo hygiene/build:** `dist/` and especially `node_modules/` are checked into the main Git tree (and no root `.gitignore` found); generated artifacts may be appropriate for publication bundles, but normal repository history should exclude dependencies. Root `build.mjs` contains `require()` despite the `.mjs` extension and a hard-coded local path, so it is not portable as written; use portable `npm run build`.
8. **Scope claims:** tests in repo demonstrate compatibility for the covered cases, but no executed test result or production load test was obtained during this review. Add protocol corner cases, reconnect behavior, concurrency, malformed/oversized frames and security tests before claiming hardened interoperability.

## Design stance

Preserve JSignalR as a **standalone library**; use it as an optional event/control channel in the Afrowave JS toolkit, not as a compulsory dependency of Backpack or AjisFS. Offer a small integration to emit/receive document revisions, stream *references* to large data, and attach idiomatic JS callbacks. Build only what concrete use cases need; do not recreate a full ASP.NET SignalR stack by default.
