# Plan: Tenant Data Isolation
**Date:** 2026-06-01  
**Planner:** claude-opus-4-8

## Requirement
Implement query-level tenant data isolation. JWT carries `tenant_id`/`tenant_code` claims but no EF global query filter exists. Tenant-scoped requests can read other tenants' data.

## Task Graph

| ID | Title | Owner | Complexity | Depends |
|----|-------|-------|-----------|---------|
| T1 | Define ITenantContext in Application layer | fullstack | low | — |
| T2 | Implement HttpTenantContext + wire DI | fullstack | medium | T1 |
| T3 | Apply User HasQueryFilter in KnightsDbContext | db | medium | T1 |
| T4 | Tests: isolation + system-admin bypass + HttpTenantContext | fullstack | medium | T2, T3 |

## Interface Contract
- `ITenantContext` (Application.Common.Interfaces): `Guid? TenantId` — null = system admin
- `HttpTenantContext` (Infrastructure.Security): reads `tenant_id` claim via `IHttpContextAccessor`
- `KnightsDbContext`: injects `ITenantContext`, applies `HasQueryFilter` on `User`
- `IUserRepository` signatures unchanged — filter transparent

## Coverage Threshold
0.85
