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
- `IIdentityManager`: central user accounts, credential/authentication lifecycle, optional external identity-provider connectors; avoid making a specific UI or storage provider compulsory.
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
