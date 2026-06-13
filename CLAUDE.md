# CLAUDE.md

This repository is an early-stage .NET 10 solution for Knights using Clean Architecture
with a rich Domain-Driven Design model.

## Commands

Run commands from the repository root.

```powershell
dotnet build Knights.slnx
dotnet test Knights.slnx
```

## Solution layout

```text
src/
  Knights.Domain/          # Entities, value objects, validation, domain exceptions
  Knights.Application/     # Use-case services, DTOs, repository contracts, Mapster mapping
  Knights.Infrastructure/  # Persistence/external integrations, currently scaffolded
  Knights.Api/             # API host, currently exposes /health

tests/
  Knights.Domain.Tests/
  Knights.Application.Tests/
```

The previous `src/Core` project is left on disk as legacy source but is no longer part of
`Knights.slnx`.

## Architecture rules

- `Knights.Domain` must not reference DTOs, Mapster, EF Core, API types, or infrastructure.
- Domain entities expose static factories and behavior methods instead of accepting DTOs.
- Entity state should be protected with private setters where practical.
- Business invariants belong in the domain model.
- `Knights.Application` owns request/response DTOs, service orchestration, repository
  interfaces, and Mapster mapping configuration.
- Mapster config currently lives in `Knights.Application/Common/Mapping/MapsterConfig.cs`.
- Infrastructure implementations should depend inward on application/domain abstractions.

## Current domain model

Identity:

- `User`
- `Role`
- `Permission`
- `UserRole`
- `UserPermission`

Tenancy:

- `Tenant`
- `TenantRole`

Common:

- `BaseEntity`
- `AuditedEntity`
- `ValueObject`
- `NameValueObject`
- `ValidationRules`
- `ValidationException`

## First application slice

The first application service is `IUserService` / `UserService`, backed by the
`IUserRepository` contract. It supports create, get, update, assign role, and grant
permission workflows. Tests use an in-memory repository fake.

<!-- hady:harness -->
## Hady harness

Task-specific context and selected skills are supplied by the harness at runtime.
Follow `AGENTS.md` as the canonical contract; do not invent a parallel workflow
or silently broaden the assigned task.
