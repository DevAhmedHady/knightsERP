# Code review log: Build new tenant environment/world setup feature for Knights platform.

Hard requirements:
1. Each tenant manager must be forced into first-run environment setup for the tenant.
2. Setup completeness must be tracked as progress.
3. Tenant can start using the build after completion reaches 50 percent.
4. System admin manages a feature picker catalog: system admin adds features, tenant admins pick from published features.
5. Suggest additional valuable capabilities in the resulting delivery docs if applicable.

Execution policy for this task only:
- Planner/Reviewer: gpt-5.5 medium.
- Main execution share: gpt-5.5 medium.
- Secondary execution share: gpt-5.4 medium.
- Use caveman ultra communication for all roles.
- Replan from scratch, then build directly.

Repo context and assumptions to use unless code strongly suggests better:
- Knights is a multi-tenant operational platform, not a 3D game engine.
- First release should be metadata-first, not a visual world editor.
- 50 percent unlock should allow core workspace usage while incomplete or locked setup areas remain gated.
- Feature catalog should support publication state and dependency validation.
- Existing tenant isolation and tenant context code already exists and must remain compatible.

Deliver production-quality code, tests, plan artifact, review log, and delivery doc.

_Reviewer: Opus 4.8 — one entry per review pass. Blocking items are the fixes required before acceptance._
