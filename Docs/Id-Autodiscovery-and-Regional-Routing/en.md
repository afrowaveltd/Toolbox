# Afrowave Id Autodiscovery and Region-Aware Client Routing

**Status:** product/architecture proposal, 2026-10-20; **not implemented**. Based on the owner's request for a live service returning available Id authorities, supporting regional latency optimization (e.g. USA client → USA VPS despite preferred Id1 leader in Europe). Read alongside [Id High Availability](../Id-High-Availability-and-Replication/en.md), [Observer and Uplink](../Id-Observer-and-Uplink/en.md), and [Id Rebuild Review](../Id-Rebuild-Review/en.md).

## Goal: one authority, many suitable regional entry points

Id must remain **one logical identity issuer and registry authority**, with a consistent user identity, token issuer, trusted key lifecycle and authorized write history. Multiple physical Id server instances may be placed in different regions and used as local **frontends** for compatible reads and authentication workflows, while a single legitimately fenced write leader (or rigorously consistent backend) governs globally authoritative identity/account/MFA/entitlement mutations.

**Important distinction:** a regional `Ready` server is not necessarily the `Leader` and must not autonomously commit password, MFA, account grant, session revocation or registration changes to an independent copy. A regional Id entry may perform trusted verification on sufficiently current authorized state, or securely forward leader-dependent operations over the Id cluster to the authoritative backend. A nearer frontend **does not guarantee faster end-to-end login** when database round trips or MFA authorization still cross the Atlantic; measure it.

## Two discovery layers; never conflate them

1. **Standard identity discovery (stable):** first-party HTTPS issuer such as illustrative `https://id.afrowave.ltd`; standard OpenID Connect `/.well-known/openid-configuration` (or RFC 8414 metadata path where applicable) provides the stable issuer and corresponding `authorization_endpoint`, `token_endpoint`, `jwks_uri`, algorithms and supported protocols. Clients verify issuer and tokens using exactly that identity, with no regional mix-up or silent change of trust anchor. Standard OIDC is first-party Id as provider to registered applications; no third-party Google/Facebook login dependency.
2. **Afrowave operational/region discovery (new optional extension):** illustrative `https://id.afrowave.ltd/.well-known/afrowave-id-instances` or a separate resilient discovery service lists **eligible public regional Id entry points** with health, capability/readiness, routing hints and bounded freshness. **Nonstandard path/name — do not advertise it as part of OIDC.** Internal Observer's privileged view also knows private node-to-node links, raw synchronization position, quorum/epoch and incidents; a public discovery response must not leak those internal addresses or sensitive topology.

### Preferred browser topology

For interactive web SSO, prefer a **single canonical public HTTPS hostname** (`id.afrowave.ltd`) served by healthy regional ingresses selected via global routing/geo-aware DNS/Anycast/load-balancing. The browser uses one first-party Id session cookie/origin even when different regions answer the requests. Configure shared secure data-protection keys, state/session material and consistent token signing/verification across these ingresses; never solve regional routing with wildcard browser cookies or raw cookie forwarding among unrelated app domains.

**Explicit regional URLs** (e.g. illustrative `https://id-us.afrowave.ltd`) are possible for trusted backends/SDK clients or region-specific service APIs, but should not randomly substitute for a verified OIDC issuer/endpoint during an active authorization flow. If exposed for login, their OIDC redirect/origin, session, discovery, cookie and token-hosting consequences must be designed and tested as one scheme, not assumed safe. Canonical login continuity is the default.

### How to pick a suitable Id endpoint

Choose **eligible** rather than merely 'closest' instances:

- `Ready` for the requested operation, with authenticated current cluster membership/health and a bounded-freshness report from Observer plus active public endpoint checks. Do not use raw `IAmHere` heartbeat as proof the public auth endpoint works.
- Advertised operation capabilities, e.g. `CanAuthenticate`, `CanReadDirectory`, `CanChangeAccount` **via leader routing**, `CanProcessMfa`, `CanIssueToken`, and actual leader/epoch/write readiness for privileged control endpoints. Client must not infer capabilities from a human-readable `Role` or a geographic location.
- Regional preference may use a client-selected region, approximate network proximity, measured RTT/error rate, capacity and legal/data residency policy. Do not collect precise user location or behavior histories merely to route a login. A nearer region may be slower if backend operations traverse distant data replicas.
- Deterministic priorities and fallback candidates: healthy local first for supported operations; then another region. Returning stale or failing endpoints must degrade out of the list after bounded checks. A registered owner may configure permitted residency regions, but this does not silently create globally writable replicas.
- A chosen region is **sticky for the lifetime of a security-sensitive request/exchange** where needed; handle auth `state`, `nonce`, PKCE, one-use authorization codes, cookies and transaction/correlation IDs consistently across failover. If interrupted, fail clearly and initiate a safe new flow when replay is impossible; never silently redeem a code against an untrusted/mismatched issuer.

## Example: American user's regional authentication

```text
USA user / App-US
       |
       v
  https://id.afrowave.ltd  (stable HTTPS issuer/origin)
       |  global ingress selects healthy USA endpoint
       v
  Id2 / US region       -- cluster control / approved leader-dependent work --> Id1 / EU leader
       |                                             ^
       +-- authorized local read/verification only ---+ (according to consistency policy)
       |
       v
  Signed identity assertion / app-specific session at App-US

  Observer -> internal healthy instance/capability view -> public discovery projection
```

Id2 may provide local login UI/session and suitable verification operations; account state and token issuance must still honor current authorized identity state, revocation/MFA rules and issuer keys. Application user sessions stay app-owned and survive Id outages within bounded expiry/authorized cache policy.

## Illustrative discovery response (not a frozen API)

The external list exposes **public ingress URIs only** and minimal routing metadata; private Observer addresses, database nodes, leadership secrets and connected client identities belong in the privileged admin view. Candidate JSON (illustrative values and hostnames, not deployed endpoints):

```json
{
  "issuer": "https://id.afrowave.ltd",
  "generatedAt": "2026-10-20T09:00:00Z",
  "validForSeconds": 30,
  "revision": "cluster-view-152",
  "instances": [
    {
      "id": "id-us-01",
      "region": "us-east",
      "publicEndpoint": "https://id-us.afrowave.ltd",
      "health": "Ready",
      "capabilities": ["CanAuthenticate", "CanReadDirectory", "CanProxyWrite"],
      "priority": 10
    },
    {
      "id": "id-eu-01",
      "region": "eu-central",
      "publicEndpoint": "https://id-eu.afrowave.ltd",
      "health": "Ready",
      "capabilities": ["CanAuthenticate", "CanReadDirectory", "CanWrite"],
      "priority": 20
    }
  ]
}
```

`issuer` is the stable OIDC identity, not a per-region token issuer. `publicEndpoint` is merely routing metadata and must not automatically override the standard OIDC `authorization_endpoint`/`token_endpoint`. `generatedAt` is informational, not an unauthenticated proof; `validForSeconds` must be constrained to avoid serving stale authority indefinitely. Bind discovery to a known HTTPS trust anchor; optionally sign canonical snapshots if offline validation is required. Never treat editable client-side priority/region or a spoofed response as authorization.

## Observer, Id election and routing ownership

- **Observer** and active health probes gather liveness, public reachability, direct-vs-relay path availability, replication lag, capability snapshots and notices of cluster leadership/epoch changes. It can produce the authorized **candidate set**, not independently grant `CanWrite` by editing an API list.
- **Cluster election/DB fencing** owns actual leader authority and durability. `Ready` vs `Degraded` and `Leader` vs `Follower` remain different dimensions. A region that lost quorum cannot issue independent writes simply because Autodiscovery still lists it. Validate capabilities at every request boundary, not just discovery time.
- **Global ingress/SDK** owns the final route selection and retry. Do not route an authentication secret, callback URL, PKCE verifier or API bearer token to an arbitrary endpoint suggested by a caller; use approved destinations, TLS, issuer/audience binding and anti-mix-up validation.
- **Uplink relay/secure overlay** may restore connectivity between Id nodes; region routing and failover need not cause a leader change when a healthy authorized route exists.
- **Admin dashboard** may visualize Observer-discovered instances, country/region, current leader, state (`Ready`, `Degraded`, `Fail`), public vs internal accessibility, direct or relayed links, observed RTT, replication lag, read/write capabilities and the currently selected regional route. It must label measured values honestly and not expose private topology in public discovery.

## The discovery service itself must not become a new single point of failure

- The canonical discovery/bootstrap URI should have its own multi-node/global-ingress availability. Observer is useful to supply the source topology, but a temporarily unavailable Observer must not force all still-healthy Id frontend routing to fail. Serve **last known-good bounded/cached route information** or an independent stable ingress fallback according to explicit TTL/readiness policy.
- SDK clients keep a small authenticated, expiry-checked list of trusted regional endpoints (or stable bootstrap seeds) and try the next eligible endpoint on connection errors, avoiding repeated retry storms. Do **not** bypass security checks when cache expires; an expired record is not perpetual permission for critical writes.
- Id and its own login remain operable even when Afutrans is unavailable, and local UI translation snapshots are used. No translation engine or community service is required for discovery or login.
- Rate limit discovery, prevent unauthorized mass node enumeration of internal addresses, use authenticated control-plane updates, validate any redirects/hostnames to avoid SSRF/open redirects, and publish only minimal public health.

## Standard security and identity invariants

- OIDC Discovery issuer identifier is HTTPS and equals token `iss`; trusted relying parties must verify issuer/signature/audience and prevent mix-up when interacting with distinct issuers. Publishing multiple physical endpoints **does not create multiple independent OIDC issuers** in this design.
- Keep the RFC 9700 authorization-code + PKCE security boundaries. If the request is redirected/fails over mid-authentication, safely preserve or restart transactional state and prevent code replay. Test local MFA vs region failover and immediate revocation under stale replica.
- JWT/JWKS public signing keys can be replicated/cached with rotation; **private signing secrets must not be returned in discovery** or handed to arbitrary registered application hosts. SSO is still first-party and uses technically necessary first-party cookies.

## Future acceptance tests

1. Healthy USA Id2 and EU leader Id1: an American browser lands on USA ingress while the issuer and first-party cookie origin remain canonical; any required EU write follows authorized cluster path.
2. USA endpoint down: stable ingress or authenticated SDK fallback selects an eligible EU endpoint without changing issuer or accepting a stale/degraded write authority.
3. Observer down: signed/bounded last-good discovery or global ingress maintains suitable reads/logins according to explicit freshness policy; a new untrusted Id endpoint cannot enroll itself.
4. USA frontend `Ready` but behind on critical revocation/MFA state: do not issue incorrectly authorized tokens or accept password changes from stale data; proxy to leader or fail with explicit safe status.
5. Same browser login travels through two ingress regions: enforce stable issuer/cookie/session/PKCE and single-use codes; no duplicated login grants.
6. Distinguish public Id routes from control-plane/private cluster endpoints; fuzz malicious discovery URLs and forged region/priority requests to prevent SSRF/mix-up/open redirects.
7. A new Id in another region is `Degraded` during catch-up; not publicly eligible until validated `Ready` with the required capabilities.
8. Client-selected region/residency preference cannot override app owner authorization, identity consistency or security gates. Capture RTT and measured actual end-to-end login latency to assess whether regional routing helps.

## Reference specifications

- OpenID Connect Discovery 1.0: https://openid.net/specs/openid-connect-discovery-1_0.html
- OAuth 2.0 Authorization Server Metadata (RFC 8414): https://www.rfc-editor.org/rfc/rfc8414
- OAuth 2.0 Security Best Current Practice (RFC 9700): https://www.rfc-editor.org/rfc/rfc9700

**Implementation:** no Id Autodiscovery service, regional endpoint catalog, discovery SDK, global ingress routing, DNS/georouting configuration, Observer projection, authentication-region failover or related tests are implemented. Product requirements and candidate contracts only.

