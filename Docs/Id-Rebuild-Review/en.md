# Afrowave Id: legacy architecture findings and future rebuild notes

**Status: architecture survey / inspiration (2026-10-20); not an implementation plan, security audit or release claim.** Source of historical facts: [`afrowaveltd/AfrowaveWebPublic/Id`](https://github.com/afrowaveltd/AfrowaveWebPublic/tree/master/Id) on `master`, inspected 2026-10-20. Legacy repository untouched; no source changes or tests executed for this review.

## Intent to preserve

The intended `Id` (Afrowave identity and community application manager) is both the **central system orchestrator** and an application registered through the **same base application model** as third-party community applications. Individual people should be able to register a brand/developer identity and one or more applications under that brand, configure application roles and operating policies, and enroll or invite users. A central user can have different roles and **application-scoped privacy/sharing settings** in each application.

Self-registration must mean **shared application lifecycle and data model**, *not* that any registered application can obtain system-level administration authority. Future `Id` can be a platform application with narrowly defined reserved system capabilities, isolated from ordinary tenant/application role assignment.

## Legacy entities and relationships found

- [`Brand`](https://github.com/afrowaveltd/AfrowaveWebPublic/blob/master/Id/Models/DatabaseModels/Brand.cs) has Id, Name, OwnerId, logo/website/email/description and multiple Applications.
- [`Application`](https://github.com/afrowaveltd/AfrowaveWebPublic/blob/master/Id/Models/DatabaseModels/Application.cs) has a string/GUID Id, BrandId, OwnerId, name/description, URL/redirects, policy flags, application-specific roles and users, `IsAuthenticator`, enabled/deleted status, SMTP settings, client secret and policy links.
- [`ApplicationUser`](https://github.com/afrowaveltd/AfrowaveWebPublic/blob/master/Id/Models/DatabaseModels/ApplicationUser.cs) connects central user and application, has explicit agreements, lock state, description and per-application disclosure flags (email, phone, address, name, picture, birthday, gender).
- [`ApplicationRole`](https://github.com/afrowaveltd/AfrowaveWebPublic/blob/master/Id/Models/DatabaseModels/ApplicationRole.cs) references ApplicationId and captures normalized role name plus `CanAdministerRoles`; [`UserRole`](https://github.com/afrowaveltd/AfrowaveWebPublic/blob/master/Id/Models/DatabaseModels/UserRole.cs) links an ApplicationUser to an ApplicationRole.
- [`User`](https://github.com/afrowaveltd/AfrowaveWebPublic/blob/master/Id/Models/DatabaseModels/User.cs) is central; `IsOwner` indicates system-owner bootstrap. There are separate entity models for suspensions, app policies, translations, themes and tokens.
- [`LanguagesManager`](https://github.com/afrowaveltd/AfrowaveWebPublic/blob/master/Id/Services/LanguagesManager.cs), [`TranslationFilesManager`](https://github.com/afrowaveltd/AfrowaveWebPublic/blob/master/Id/Services/TranslationFilesManager.cs), `I...Manager`/`I...Service`, `ApiResponse<T>`, constructor DI, XML docs and `...Async` method naming reflect the author's existing naming conventions. Prefer these conventions for approachable public APIs while separating smaller responsibilities.

## Existing first-run installation sequence

The legacy [`InstallationStatusService`](https://github.com/afrowaveltd/AfrowaveWebPublic/blob/master/Id/Services/InstallationStatusService.cs) calculates the next step from database existence and configuration flags: Administrator → Brand → Application → ApplicationRoles → SMTP → LoginRules → PasswordRules → Cookies → JWT → CORS → Result → Finish. See [`Pages/Install`](https://github.com/afrowaveltd/AfrowaveWebPublic/tree/master/Id/Pages/Install) and `IdentificatorSettings.InstallationFinished`.

1. The administrator installation page creates the initial central User (`IsOwner=true`).
2. The brand step creates the first brand associated with that owner.
3. The application step calls `RegisterApplicationAsync` with `IsAuthenticator=true` and stores the new application ID in settings. Conceptually the Id application is self-registered just like other apps.
4. The role step creates **Owner, Administrator, Moderator, Translator, User** roles under the initial application's ID; it then registers the owner as an ApplicationUser and assigns roles.
5. Other steps configure app-specific SMTP and service security/settings; the result step marks installation finished.

The [`Program.cs`](https://github.com/afrowaveltd/AfrowaveWebPublic/blob/master/Id/Program.cs) startup already runs migrations, language and country seeding, translation preparation, and locale-file discovery before serving HTTP. Preserve the principle that the first served page has coherent required data, while separating optional network-heavy translation from mandatory readiness.

## Verified implementation gaps to address in future tests

1. In the legacy [`ApplicationsManager.RegisterApplicationAsync`](https://github.com/afrowaveltd/AfrowaveWebPublic/blob/master/Id/Services/ApplicationsManager.cs), the method validates `input.IsAuthenticator` against existing authenticator apps but the observed creation block does **not assign** `application.IsAuthenticator = input.IsAuthenticator`; the new entity keeps its default `false`. The installer does pass `IsAuthenticator=true`, so the intended invariant is not reliably persisted. Test the single reserved system app and stable identity on rerun.
2. In [`RolesManager.SetApplicationUserRoleAsync`](https://github.com/afrowaveltd/AfrowaveWebPublic/blob/master/Id/Services/RolesManager.cs), the inspected method loads a role and an application user but does **not verify** `role.ApplicationId == user.ApplicationId` before creating `UserRole`. Another overload looks up a user's first ApplicationUser without an application scope. Future centralized permission checks need explicit current actor, target application, tenant/brand boundary and role, with DB constraints and test coverage; do not assume callers always enforce checks.
3. Setup stage detection currently relies on `AnyAsync` / record counts; administrator and brand creation are independent requests. A new public installation wizard should be **one-time, authenticated/bootstrap-protected, concurrency-safe, resumable and idempotent** (especially first-owner creation), with an installation state/version and transactional milestones. Do not derive ownership from the first arbitrary user/brand after activation.
4. `IsOwner`, `CanAdministerRoles`, `OwnerId` and ordinary role names are different authorization concepts; isolate system administration from tenant ownership and tenant application roles. No app or user should self-assign a platform-wide role through ordinary registration. Validate `RedirectUri` and consent during OAuth/OIDC-style client registration, and maintain audited application secret rotation/revocation.
5. The legacy state is **partial implementation**, not proof that arbitrary external developers can already complete public brand/application self-service registration end-to-end. The repository's Id-specific `docs/introduction.md` and `docs/getting-started.md` are currently stubs. Document and test full journeys as part of any rebuild.

These are findings from specific inspected code paths, **not proof of exploitable deployed vulnerabilities** and not a full authentication/security review.

## Suggested independent responsibilities (design exploration)

- `IInstallationManager` / bootstrap workflow: state machine, recovery and activation; at bootstrap use a minimal trusted local installation authority to create the first owner/brand/system app atomically. After activation use normal authorization services.
- `IIdentityManager`: **first-party** central user identity, credentials, sessions, account recovery, optional native MFA and independent credential-storage adapters; **no external social/enterprise identity-provider sign-in is in scope** for the stated Id vision. Id itself may expose standards-compatible authentication endpoints to *its registered applications*.
- `IBrandsManager`: manage brands/developer organizations, their owners/members and transfers. Legacy `OwnerId` can remain a migration compatibility field, while new members and permissions can use scoped records.
- `IApplicationsManager`: application registration, settings, lifecycle, ownership and brand membership; treat Id itself as a registered application in the same domain model.
- `IApplicationUsersManager`, `IRolesManager` and a distinct authorization/policy evaluator: application-scoped membership, roles, consent, privacy claims and explicit system-level capabilities.
- `IClientRegistrationManager` or similar: manage client types, redirect URI allowlists, authenticated API clients and secret/key lifecycle. Keep client and user permissions distinct.
- `ISettingsStore` / `IDataAccessor`: database-neutral persistence with revisions, transactions where supported, cache, audit and controlled migrations; startup snapshot/readiness independent from translation services.
- `ILanguageCatalogReader`/language picker and `UnderstandMe` localization: read reusable language data and optional translated labels without coupling identity/access control to Afutrans or LibreTranslate.

These are **proposed names/responsibilities, not frozen public APIs**. Keep normal user registration and any developer self-service workflow subject to explicit platform and brand policies, audit and privacy boundaries.

## Target workflows to implement and test later

- Fresh offline installation: create initial admin, initial brand, reserved Id application, required platform roles and typed settings; rerun after interruption without duplicates or unsafe privilege elevation.
- A normal registered developer creates brand B and applications B1/B2; configures independent roles, callbacks, policies and users; cannot read/modify brand A or any other application's users without an explicit permitted relationship.
- Same central User participates in apps B1 and B2 with different role sets and per-app data-sharing grants; revoking sharing or membership in one app does not damage their central account or permissions in the other app.
- Role assignment rejects cross-application role IDs, system-owner elevation by application administrators and attempts to use a different brand's ownership IDs.
- The Id application uses the generic registry/application model but can invoke reserved system orchestration capabilities only through an explicit platform authorization boundary, not through ordinary app role names.
- Language picker is complete before HTTP readiness; later newly discovered `Locales` trigger safe incremental catalog/translation reconciliation without network-dependent failure of basic sign-in.
- Critical credential/key/role changes have revision/audit records; source repositories and documentation remain current. Future tests include interruption, retries, conflicts, concurrent requests and database-backed persistence.

## Source-of-truth / non-actions

Historical code stays in `afrowaveltd/AfrowaveWebPublic`. This note lives in the Toolbox's general `Docs/Id-Rebuild-Review/en.md` as a cross-project continuation record only. No code changes, dependencies, data migration or actual Id redesign have been committed. Future Id project should receive its own `README.md`, topic-specific `Docs/<topic>/en.md` and `IMPLEMENTATION_STATUS.md` when development starts. Future architecture must be confirmed with the user before treating these proposals as established contracts.

## Product intent clarified by the author (2026-10-20)

These are **confirmed product-intent requirements from the owner**, which clarify and supersede earlier exploratory wording in this review. They do not mean the old project implemented them.

### One first-party identity across Afrowave applications

- A person **registers once in Id**; their same central user identity is recognized by all registered Afrowave applications. Applications do not hold copies of central login passwords and do not require a second sign-up to establish another login identity.
- Being centrally authenticated does **not** confer membership or a role in every application. When following a link from the Id portal, the target application's owner-defined admission policy decides whether to assign an initial user role automatically, require manual approval, use invitation or another configured flow, or deny enrollment.
- App membership, role changes, and app-specific data-sharing grants are **separately scoped**. An app cannot read unrelated apps' role assignments or use its own application-role IDs to obtain access to another application's resources. Application-scoped access tokens/claims disclose only authorized information for the intended application.

### `IsAuthenticator`, Id's reserved role scope, and platform oversight

- The **Id application** is a normal `Application` entity under the same registration model, but designated as the system's authentication/administration application by a **trusted bootstrap invariant** represented by `IsAuthenticator`. That flag is not editable by ordinary app registrations and must be verified against the reserved application identity in the persisted platform settings/authorization layer.
- Roles belonging to the **Id application** carry carefully scoped **platform-management capabilities**: e.g. register/manage brands and apps under policy, enforce platform service rules, temporarily suspend/disable a registered app, revoke its platform authentication/client access, or impose a central login lock where warranted.
- **Explicit prohibition / responsibility boundary:** platform administration must **not** edit another application's roles, memberships or internal authorization policies simply because an Id operator has a system role. These belong to that application's owner/delegated administrators. Platform interventions affect Id/platform-level availability and access, not the target app's business state or private records.
- Distinguish **user global authentication lock** (one person cannot authenticate through Id), **application suspension** (a registered application is blocked from using the platform), and **per-application admission/ban** (a decision made by the application's owner). These must be independent actions and separately auditable; do not delete data or roles as a side effect of an access suspension.
- For abusive/illegal published material, Id can suspend platform access or the app's registration according to its published policy, with recorded actor, reason, affected subject, scope, timestamp, review/appeal path and reversal. Platform ability to suspend must **not** imply unfettered access to read private application content. Proportional emergency controls and normal review can be separate workflows.
- An ordinary community developer must not set `IsAuthenticator=true` or gain Id's system capabilities by copying role names such as `Owner` or `Administrator`. Bootstrap the trusted Id registration exactly once and test privilege separation exhaustively.

### First-party authentication, MFA without third-party sign-in, and privacy

- Id provides authentication **itself**. The current intended design specifically excludes sign-in through third-party providers (Google, Facebook, Microsoft/social or external enterprise IdPs); do **not** introduce mandatory or speculative external identity-provider connectors. The central Id service may expose open/standardized protocols to its first-party and other registered client applications; this is not external-provider sign-in.
- **MFA is supported but not a universal prerequisite**. Offer native options such as TOTP, passkeys/WebAuthn and recovery codes where appropriate, plus host/admin-configurable additional requirements for sensitive privileged operations. Users without MFA can still use permitted basic functions under platform policy.
- Privacy by design: collect only **the minimum account information needed** for identification, recovery and the user's chosen features, and store permitted per-application consent and role data; do not collect analytics, behavioral tracking, profiling, unnecessary personal fields or share unnecessary identity attributes with third parties or between apps.
- No third-party tracking or advertising cookies, SDKs or silent external-login dependencies. Essential **first-party session/security cookies** are permissible and must use appropriate Secure, HttpOnly, SameSite, expiration and rotation practices; distinguish these from surveillance cookies.
- Security/audit records required to operate a trustworthy identity platform are distinct from behavioral surveillance: minimize content, retention and access; record privileged interventions without collecting browsing histories. Do not claim literally zero persisted user data when account identity, credentials and scoped consent must exist for the product to function.

### First-installation and readiness

- The first-run installation wizard **creates the initial administrator, brand and Id application** and configures their policies before opening the operational portal. Platform roles are bound to Id's reserved application identity, never implicitly made global merely by role-name equality.
- Bootstrap/activation must have transactional/idempotent checkpoints, harden first-owner creation against parallel unauthenticated installation attempts, and reliably record the reserved `IsAuthenticator` designation. After activation, the installation privileged endpoint is disabled; subsequent brand and app registrations use authenticated, authorized ordinary workflows.
- The ready-to-use language selector and necessary local resources should be prepared **before the first page is served**; optional external translation outages should not block basic Id authentication by default.

### Target acceptance and isolation tests

1. User U signs in to Id once, then visits apps A and B: A assigns `User` automatically, B requires manual approval; B's approval decision does not alter A's membership or Id's central identity.
2. Id system moderator suspends A or locks U's **central login** according to policy, with audit and review, while A's internal roles and business data are unchanged. A developer cannot suspend B or impose a global lock.
3. A developer with `Administrator` role **only in app A** cannot request platform operations, grant themselves an Id role, make A an authenticator or administer app B.
4. Even an authorized Id platform operator cannot issue a normal cross-app `CreateRole`, `AssignRole` or unscoped user-data read merely because they can suspend apps; separate explicit extraordinary mechanisms, if ever introduced, require their own policy and authority.
5. Native password/session and optional TOTP/passkeys work without calling an external login/identity provider or delivering any third-party tracking cookies; no mandatory MFA for ordinary users.
6. App A receives only approved identity claims and consents; no user analytics or app B membership leaks. Session/security logs use constrained purpose and retention.
7. Fresh setup establishes exactly one reserved authenticator application even after interruption/restart and untrusted parallel requests.

**Boundary:** These are design decisions and planned tests, not implemented or security-tested Id code. Keep them in the future Id's own README/Docs/IMPLEMENTATION_STATUS once the rebuild project is created; never silently widen platform operators' powers during implementation.

## Service applications, first-party cookie sessions and Afutrans integration (2026-10-20)

**Further confirmed product intent**: Id is a registry and central identity provider for ordinary applications **and service applications**. Afutrans will be one Afrowave-owned or independently owned service entry: Id uses it internally for translation, and other registered app owners may discover, subscribe to and use its REST/business SignalR capabilities under policy. The original `Id` remains a legacy/incomplete prototype; neither registration nor subscription is implemented by this documentation.

- App model needs separate conceptual dimensions for service/capabilities, discoverability/visibility, endpoint exposure and authorization/entitlements. A `Public` or `Listed` flag is **not** an instruction to permit anonymous calls or disclose tenant dictionaries. Use structured capability descriptors/policies where appropriate rather than an ever-growing collection of Afutrans-specific flags.
- The registered `Id` application retains protected `IsAuthenticator` and platform enforcement authority (e.g. app suspension or central-login lock). Service-provider status (`Afutrans`, others) must **not** grant platform powers, and Id operators may not rewrite a third-party app's roles or internal permissions merely through platform oversight.
- Each application owner determines admission and role assignment for their **own** users. A single user logs in centrally at Id; apps use their own first-party session cookies. User's conceptual *cookie router* means Id coordinates authentication while each app issues/owns its own session; no raw cookie forwarding or shared wildcard-domain cookie. Id may maintain a narrow first-party login-session cookie for redirects and session management.
- Id is a first-party identity provider (potential authorization-code + PKCE style integration for browser apps); applications validate the issued identity and create local sessions. This does **not** introduce sign-in through external Google/Facebook/etc. Nor does it require tracking cookies.
- Registered application-to-service grants must be scoped to the **caller application**, provider service, allowed capabilities and optionally specific target(s), with denial/revocation and audience-restricted authentication for direct API/SignalR calls. A GUID or `clientId` supplied in a payload is not authentication.
- Id should record service discovery, relationship/grant and safe endpoint/capability metadata. **Do not upload target database passwords, full SQL connection strings, SFTP credentials or private dictionary contents into Id** as a by-product of service registration. The service owner or connecting application manages encrypted/secret-referenced connectors in Afutrans or an app-side agent.
- A developer configures an Afutrans Translation Target with chosen source/default language, desired target languages, source adapter (local files, secured SFTP, SQL or business API), read/write scope, sync trigger and provider/privacy rules. Afutrans/UnderstandMe validate, translate, persist and report results; Id neither translates content nor gains permissions to modify third-party roles.
- Id's own authentication, bootstrap and language-selector readiness must **not depend on live Afutrans availability**. Reuse the last validated locally available translations and endonyms; start/sync optional Afutrans integration after base readiness.

Reference: [Afutrans Registered Service Integration](https://github.com/afrowaveltd/Afutrans/blob/main/Docs/Registered-Service-Integration/en.md). Proposed acceptance scenarios include public discovery without free access; B1/B2 independent service grants; Id app using local translation while Afutrans offline; safe target secrets; platform suspend without role/data alteration; and per-app first-party cookies. **Design only; no new app/Id functionality is implemented.**

## Replication and availability are minimum Id platform requirements (2026-10-20)

**Confirmed:** Id is the logical source of truth for first-party identity, service registry, authentication, reserved platform privileges and app/service grants (not third-party applications' own internal roles). Global outages would block new logins across Afrowave. Require at least two independent Id service instances, a replicated authoritative database with safe failover, a third independent quorum/fencing voter/witness for typical consensus-based two-data-node deployments, resilient ingress/key management, tested offsite backups and measured RPO/RTO. **Two active writers without quorum are not acceptable**; a minority partition must never issue conflicting identity mutations.

Existing app-owned sessions may remain usable under bounded expiry/revocation policy when Id is unreachable, but new global authentication and high-impact writes depend on an available authoritative Id quorum. Test node loss, network partition, site loss, revocation, crypto-key rotation and restore. Id remains independent of Afutrans availability and can load localization resources offline.

Owners may elect plain FTP/HTTP for their own test/public applications if the chosen connector supports it, with honest warning and strict isolated worker boundaries. This **does not allow** unencrypted Id authorization-code/token transport on remote HTTP callbacks (see RFC 9700). Details: [Id High Availability and Replication](../Id-High-Availability-and-Replication/en.md). **Design only.**

## Preferred-primary Uplink deployment pattern clarified (2026-10-20)

The author prefers **Id1 as normal master**, **Id2 as synchronized standby**, a dedicated bidirectional **SignalR Uplink Hub** for node events/status/recovery, and a separate SMTP service application for failure notifications. Id2 may take over on confirmed Id1 failure; after return, the recovering node contacts the **current authorized leader**, performs durable catch-up and only then becomes publicly ready.

**Architecture correction:** persist a `PreferredPrimary` preference rather than treating `IsMaster` as permanent permission to write. Dynamic `LeaderId`/`LeaderEpoch` plus quorum/fencing prevent dual-writer split brain when SignalR connectivity fails but both servers are publicly accessible. Original Id1 rejoins as follower after Id2 takeover; optional planned switchover restores normal preference.

Two data servers and one voting witness do **not** imply three durable data copies. Choose documented synchronous durability vs asynchronous continued-write/RPO trade-offs, or add a third data-bearing node for stronger combined guarantees. SignalR is control/notifications; transactional DB replication is the authoritative sync. SMTP alerts use an outbox and external monitoring because failed hosts cannot report their own death.

See [Id High Availability and Replication](../Id-High-Availability-and-Replication/en.md). **Documented design only; no code/test changes.**

## Observer as supervisor, relay and Id witness (2026-10-20)

User refined the HA architecture: a separate `Observer` application receives small authenticated `IAmHere` health signals from registered Afrowave services. For **Id1/Id2 specifically**, Observer may relay Uplink messages if their direct link fails, and participates as an independent third vote **only through an actual cluster consensus/fencing system**. If both Ids see Observer but not each other, relay can preserve control communication without needless takeover. If Id1 loses both links but Id2 sees Observer, Id1 must stop writes and Id2 can promote only with quorum and valid replicated data. If Observer alone is lost but the two Ids communicate, they may continue at two of three votes.

Only designated Id cluster members/voters may affect the Id election; unrelated applications reporting heartbeat have no vote. SignalR relay is **not** authoritative DB replication and Observer must not become the single required request router. Prefer optional direct and alternate paths, incident alerts and external watchdog checks. See [Observer and Uplink](../Id-Observer-and-Uplink/en.md). **Design only.**

## Three health states and explicit leadership fencing (2026-10-20)

The Id deployment should expose exactly three high-level health values: **`Ready`, `Degraded`, `Fail`**, while keeping the **`Leader`/`Follower`** node role and actual quorum-fenced write authorization as independent dimensions. A returning Id is `Degraded` until reconciled with the **currently authorized leader**, verified and only then `Ready`. Id2 must be able to fully take over when Id1 disappears, subject to safe consensus, replica freshness and configured durability guarantees.

An Observer report can detect a lost node and trigger recovery, but **cannot demote a disconnected Id node merely by announcing a different state**; a lease/epoch and database fencing must make any old leader reject authoritative writes. Losing one link with an intact majority should not automatically force takeover. To add Id3 or later nodes, first authenticate/register and sync them in `Degraded`, then enable `Ready` follower; change voting membership by deliberate quorum-safe reconfiguration rather than automatically making every node a voter.

See [Observer and Uplink](../Id-Observer-and-Uplink/en.md) and [High Availability](../Id-High-Availability-and-Replication/en.md). **Architecture only; no source implementation/tests.**

## Resilient Id-to-Observer route preference (2026-10-20)

When Id1 (leader) cannot connect **directly** to Observer but can reach Id2, and Id2 can reach Observer, the preferred behavior is an **authorized alternate route Id1 → Id2 → Observer**, not immediate failover to Id2. The Id1 node can remain `Ready` while its specific direct link is `Degraded`; leader authority continues only while consensus/fencing and database durability remain satisfied. Relay permission must be scoped to trusted cluster nodes and limited messages, not automatically extended to all community apps. Preserve original-node signatures, avoid loops/replay, report original and relayed reachability separately. Dedicated SignalR relay is suitable for Uplink commands; a secure network tunnel/overlay is needed where transparent TCP/consensus connections must pass through Id2.

See [Observer and Uplink](../Id-Observer-and-Uplink/en.md); no runtime changes yet.

## Autodiscovery: nearest eligible regional Id without new identity authority (2026-10-20)

Id should expose an **Autodiscovery** capability listing alive, verified, suitably capable regional Id service endpoints, e.g. a user connecting from the USA prefers a healthy USA VPS rather than a distant EU instance. This extends the Observer's health picture and Id's redundant deployment into latency-aware client routing.

**Distinguish** normal OpenID Connect discovery and *one stable logical issuer* from a new optional **Afrowave live endpoint/region catalog**. The preferred browser path is stable `id.afrowave.ltd` with geo-aware/global failover ingress so first-party Id cookies and token identity do not change by region. Id2 in USA may serve local work or forward mutations to current leader; no region gets autonomous system-grant/password/MFA write authority based on proximity.

Discovery must itself be highly available, validate the health/capability of each endpoint, expose only public/authorized metadata, cache for bounded periods, support trusted fallback and handle mid-login failover securely. Observer publishes candidate data but does not manufacture quorum. See [Id Autodiscovery and Regional Routing](../Id-Autodiscovery-and-Regional-Routing/en.md). **Design only.**

