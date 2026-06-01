# Review Log — Add Login Page with JWT

Stamp: 20260531 · Reviewer: Opus 4.8 · Gate: `dotnet test` + `ng build` + `ng test` (Vitest)

## Gate results
- **Backend build:** `dotnet build Knights.slnx` succeeded — **0 warnings, 0 errors**, all 6
  projects incl. Api + Infrastructure compiled (verified explicitly; the test projects alone do
  not reference Api/Infrastructure, so a passing `dotnet test` does not prove they compile).
- **Backend tests:** `dotnet test Knights.slnx` = **38 passed** (9 Domain + 20 Application +
  9 Infrastructure), 0 failed. Stable across 3 consecutive runs. (A transient first-run race
  while the newly-added `Knights.Infrastructure.Tests` project was compiling produced spurious
  failures once; clean re-runs are green.)
- **Coverage (measured, `--collect:"XPlat Code Coverage"`) — feature code:**
  - `AuthService` **100%**, `UserService.CreateAsync` (hash path) **100%**.
  - `PasswordHasher` (PBKDF2) **100%**, `JwtTokenGenerator` **100%** — covered by the new
    `Knights.Infrastructure.Tests` project (9 tests). The earlier 0%-coverage gap is **closed**.
  - Repo-wide aggregate stays below 0.85, dominated by pre-existing untested code
    (RoleService/PermissionService, mapping) — not introduced here and out of this slice's scope.
- **Frontend:** `ng build` clean (login-component chunk emitted); `ng test` (Vitest) = 6 new
  tests passed across 3 spec files. 1 unrelated failure (`app.spec.ts` default-scaffold
  "should render title") is **pre-existing** and out of scope.

## Verdicts per task

| Task | Owner (model) | Verdict | Rounds |
|------|---------------|---------|--------|
| db-1 | db (Opus*) | accept | 1 |
| fs-1 | fullstack (Opus*) | accept | 1 |
| fs-2 | fullstack (Opus*) | accept | 1 |
| fs-3 | fullstack (Opus*) | accept | 1 |
| fs-4 | fullstack (Opus*) | accept | 1 |
| fs-5 | fullstack (Codex gpt-5.5) | accept | 1 |
| fs-6 | fullstack (Codex gpt-5.5) | accept | 2 (1 revise) |
| infra-tests | fullstack (Codex gpt-5.5) | accept | 1 | added `Knights.Infrastructure.Tests` (PasswordHasher + JwtTokenGenerator), closing the coverage gap |
| ui-fix (debt #2) | fullstack (Codex gpt-5.5) | accept | 1 | create-user form `passwordHash → password` |

\* **Process note / debt:** db-1 and fs-1..fs-4 were authored by Opus before the role-separation
rule was enforced (user-flagged). They were reviewed and the tests pass, but they did not go
through an independent developer model. The Angular tasks (fs-5/fs-6) were correctly dispatched
to the Codex full-stack dev. Future tasks must follow role separation (see memory:
`orchestrator-role-separation`).

## Revise round (fs-6)
- **blocking (resolved):** specs used Jasmine API (`jasmine.createSpyObj`, `toBeTrue`) but the
  repo runner is Vitest → `TS2503 Cannot find namespace 'jasmine'`, `TS2339 toBeTrue`.
  Codex re-emitted Vitest specs (`vi.fn()`, `toBe(true)`). Re-gate green.

## Outstanding technical debt (nonblocking)

1. **JWT secret is a placeholder in `appsettings.json`.** Must be sourced from env var / user-
   secrets / vault before any non-dev deploy. Never commit a real key.
2. **~~Create-user UI/API field drift.~~ RESOLVED.** The `PasswordHash → Password` rename had
   left the Angular create form posting `passwordHash` (silently dropped → unusable login).
   Fixed: `user.model.ts` `CreateUserRequest.password`, and the `users-list` form control +
   `create()` call + template now use `password`. Dispatched to Codex; frontend rebuild clean.
3. **Token in `localStorage`** → readable by XSS. Acceptable for this slice; revisit
   (httpOnly cookie / in-memory + refresh) when hardening.
4. **No `[Authorize]` enforced** on existing API endpoints yet. Claims are minimal (sub/email/
   name); authorization policies are a deliberate follow-up.
5. **Case-insensitive lookups use `ToLower()`** in EF queries — not index-sargable on Postgres.
   Consider `citext` or normalized columns if the users table grows.
6. **Pre-existing `app.spec.ts` failure** (default scaffold test) should be fixed or removed by
   the UI owner; unrelated to this feature.
