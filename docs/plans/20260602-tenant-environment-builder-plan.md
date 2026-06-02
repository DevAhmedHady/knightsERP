# Tenant Environment Builder Plan

## Scope

Planning-only artifact for a new multi-tenant feature on Knights where each tenant manager must complete an environment/world setup flow on first run, can begin using the tenant after 50% setup completion, and can select system-admin-published features.

## Planning Assumptions

- "World/environment" means tenant-branded operational workspace, not a 3D/game scene builder.
- First release is metadata-first with extension points for richer visual builder later.
- 50% unlock means tenant can access core workspace and only completed mandatory modules.
- System admin owns a feature catalog; tenant admins can pick from published features subject to rules and dependencies.
- Tenant isolation remains shared application/database context with tenant-aware data boundaries already present in the platform.
- Tenant managers are redirected into setup on first run; before 50% they may see only the setup shell and limited read-only context.

## Planner Output

```json
{
  "title": "Tenant environment builder and feature catalog onboarding",
  "coverage_threshold": 0.85,
  "interfaces": [
    "ITenantOnboardingService exposes status, progress, step completion, unlock eligibility, and first-run enforcement checks.",
    "ITenantFeatureCatalogService exposes admin-managed feature definitions, tenant selections, dependency validation, and activation preview.",
    "Frontend setup shell consumes a single tenant setup summary DTO: tenant metadata, completion percentage, unlocked flag, required steps, selected features, available features."
  ],
  "tasks": [
    {
      "id": "T1",
      "title": "Model tenant environment setup aggregate and progress rules",
      "owner": "db",
      "complexity": "complex",
      "depends_on": [],
      "acceptance_criteria": [
        "Data model stores tenant setup progress, required setup steps, step completion timestamps, and unlock threshold policy.",
        "Data model supports feature catalog definitions, feature dependencies, feature publication status, and tenant feature selections.",
        "Migration preserves existing tenants and assigns default first-run/setup state without breaking current tenant reads.",
        "Indexes and uniqueness constraints protect feature keys and tenant feature selection integrity."
      ],
      "files_touched": [
        "src/Knights.Domain/**",
        "src/Knights.Infrastructure/Persistence/**",
        "src/Knights.Application/Common/Interfaces/**"
      ]
    },
    {
      "id": "T2",
      "title": "Add backend services and endpoints for tenant onboarding workflow",
      "owner": "fullstack",
      "complexity": "complex",
      "depends_on": [
        "T1"
      ],
      "acceptance_criteria": [
        "Backend exposes endpoint(s) to get current tenant setup summary and required steps for current tenant manager.",
        "Backend exposes commands to complete/update setup steps and recompute progress percentage deterministically.",
        "Backend prevents tenant-manager access to protected tenant features before 50% completion while preserving system-admin access.",
        "Backend exposes tenant feature selection endpoints with dependency and publication validation."
      ],
      "files_touched": [
        "src/Knights.Application/**",
        "src/Knights.Api/**"
      ]
    },
    {
      "id": "T3",
      "title": "Add system-admin feature catalog management surface",
      "owner": "fullstack",
      "complexity": "complex",
      "depends_on": [
        "T1"
      ],
      "acceptance_criteria": [
        "System admin can create, edit, publish, retire, and order catalog features.",
        "Catalog feature definition includes stable key, display name, description, optional dependency list, and activation policy fields.",
        "Retired or unpublished features cannot be newly selected by tenant admins.",
        "Tests cover validation for duplicate keys, dependency loops rejection, and publish-state enforcement."
      ],
      "files_touched": [
        "src/Knights.Application/**",
        "src/Knights.Api/**",
        "knights-ui/src/app/features/**"
      ]
    },
    {
      "id": "T4",
      "title": "Add tenant first-run setup shell and progress-gated navigation",
      "owner": "fullstack",
      "complexity": "complex",
      "depends_on": [
        "T2",
        "T3"
      ],
      "acceptance_criteria": [
        "Authenticated tenant managers without sufficient setup progress are redirected into setup flow on first run.",
        "Setup shell displays progress, required steps, unlocked status, and available feature selections.",
        "Navigation clearly distinguishes locked vs unlocked areas and allows core usage after threshold is reached.",
        "Frontend tests cover redirect behavior, progress rendering, lock state handling, and feature selection interactions."
      ],
      "files_touched": [
        "knights-ui/src/app/app.routes.ts",
        "knights-ui/src/app/core/**",
        "knights-ui/src/app/features/**",
        "knights-ui/src/app/layout/**"
      ]
    },
    {
      "id": "T5",
      "title": "Add policy enforcement and cross-layer integration tests",
      "owner": "fullstack",
      "complexity": "low",
      "depends_on": [
        "T2",
        "T3",
        "T4"
      ],
      "acceptance_criteria": [
        "Automated tests verify tenant manager cannot bypass onboarding gate before threshold.",
        "Automated tests verify core usage unlocks at 50% and locked features remain blocked until their required steps are complete.",
        "Automated tests verify system admin can manage catalog independent of tenant setup state.",
        "Coverage remains at or above 0.85 for touched areas."
      ],
      "files_touched": [
        "tests/**",
        "knights-ui/src/**/*.spec.ts"
      ]
    }
  ]
}
```

## Suggested Enhancements

- Setup template presets by tenant type so progress starts from a guided blueprint instead of blank state.
- Mandatory vs optional setup steps so 50% is based on weighted required milestones, not raw item count.
- Feature pricing tier / license flags to support commercial packaging later.
- Audit trail for who changed setup state and who enabled each feature.
- Draft/publish mode for tenant environment changes to avoid breaking live usage.
- Health score panel that highlights missing setup pieces after unlock.
- In-app checklist reminders and admin notifications for stalled onboarding.

## Execution Split

- Codex 5.5 medium: T2, T3, T4, T5. Approx. 70% of execution effort.
- Sonnet low: T1 schema/data model shape and migration-first design review. Approx. 30% of execution effort.
- Opus: planning and review only, per orchestrator defaults.

## Open Decisions Before Build

- Confirm whether "world" remains metadata-first or requires a real visual/map builder in v1.
- Confirm exact step weights for progress calculation and which modules count toward the first 50%.
- Confirm whether tenant managers get read-only dashboard access before 50% or setup-only access.
- Confirm whether feature activation is immediate or requires approval/review for some feature classes.
