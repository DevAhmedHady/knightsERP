# Plan — Add Login Page with JWT

- **Stamp:** 20260531
- **Slug:** add-login-jwt
- **Planner:** Opus 4.8 (Planner mode)
- **Requirement:** Add a login page with JWT authentication — backend auth endpoint that
  verifies credentials and issues a JWT, plus an Angular login page wired to it.

## Key design decisions (read before the task graph)

1. **Password hashing must be consistent.** Today `User.PasswordHash` is stored raw and
   `UserService.CreateAsync` passes the request value straight through. Login (verify
   password) is meaningless until create/set-password hashes with the *same* scheme.
   So we introduce `IPasswordHasher` and route the create path through it. This also makes
   the "login with valid credentials" criterion testable: a test creates a user (password
   hashed) then logs in.
2. **Owner-boundary call (explicit).** `IPasswordHasher` and `IJwtTokenGenerator`
   *implementations* land in `Knights.Infrastructure` but are **not** data-access. The db
   role owns data-access only. Therefore the **fullstack** role owns these app abstractions
   *and* their Infrastructure implementations. The **db** role owns only the `IUserRepository`
   lookup additions (EF impl + in-memory fake).
3. **Claims are minimal.** The JWT carries `sub` (user id), `email`, and `name` only — no
   embedded roles/permissions (no gold-plating). Can be extended later if authorization needs
   it.
4. **Coverage gate is backend-only.** Threshold `0.85` applies to the `.NET` tasks via
   `dotnet test`. Angular tasks are verified by `ng build` + their Jasmine specs, not by the
   .NET coverage number.

## Shared interfaces contract (the fullstack ↔ db boundary)

```text
# Owned by db (db-1)
IUserRepository.GetByUserNameAsync(string userName, CancellationToken) -> User?
IUserRepository.GetByEmailAsync(string email, CancellationToken)       -> User?

# Owned by fullstack (abstractions + Infra impls)
IPasswordHasher.Hash(string password)                      -> string
IPasswordHasher.Verify(string password, string hash)       -> bool
IJwtTokenGenerator.Generate(User user)                     -> (string token, DateTime expiresAtUtc)

# API contract
POST /api/auth/login
  body:     { "userNameOrEmail": string, "password": string }
  200:      { "accessToken": string, "expiresAtUtc": string(ISO-8601), "user": UserResponse }
  401:      invalid credentials OR inactive user (no detail leak)

# JWT claims: sub = user.Id, email = user.Email, name = user.UserName  (minimal)
# Config: appsettings "Jwt" section { Issuer, Audience, SecretKey(placeholder), ExpiryMinutes }
#         SecretKey is a placeholder in source — never a real key.
```

## Plan JSON (task graph)

```json
{
  "slug": "add-login-jwt",
  "coverage_threshold": 0.85,
  "coverage_scope": "dotnet-tasks-only",
  "interfaces": {
    "IUserRepository": [
      "GetByUserNameAsync(string userName, CancellationToken) -> User?",
      "GetByEmailAsync(string email, CancellationToken) -> User?"
    ],
    "IPasswordHasher": [
      "Hash(string password) -> string",
      "Verify(string password, string hash) -> bool"
    ],
    "IJwtTokenGenerator": [
      "Generate(User user) -> (string token, DateTime expiresAtUtc)"
    ],
    "api": "POST /api/auth/login { userNameOrEmail, password } -> 200 { accessToken, expiresAtUtc, user } | 401",
    "jwt_claims": "sub=user.Id, email=user.Email, name=user.UserName"
  },
  "tasks": [
    {
      "id": "db-1",
      "title": "Add username/email lookups to IUserRepository (EF + in-memory fake)",
      "owner": "db",
      "complexity": "low",
      "depends_on": [],
      "files_touched": [
        "src/Knights.Application/Common/Interfaces/IUserRepository.cs",
        "src/Knights.Infrastructure/Persistence/Repositories/UserRepository.cs",
        "tests/Knights.Application.Tests/ (in-memory user repo fake)"
      ],
      "acceptance_criteria": [
        "GetByUserNameAsync returns user when username matches (case-insensitive), null otherwise",
        "GetByEmailAsync returns user when email matches (case-insensitive), null otherwise",
        "EF impl loads UserRoles/UserPermissions consistent with GetByIdAsync",
        "In-memory fake used by Application tests implements both new methods"
      ]
    },
    {
      "id": "fs-1",
      "title": "IPasswordHasher abstraction + Infra impl; route create/set-password through it",
      "owner": "fullstack",
      "complexity": "complex",
      "depends_on": [],
      "files_touched": [
        "src/Knights.Application/Common/Interfaces/IPasswordHasher.cs (new)",
        "src/Knights.Infrastructure/Security/PasswordHasher.cs (new)",
        "src/Knights.Infrastructure/DependencyInjection.cs",
        "src/Knights.Application/Users/UserService.cs",
        "tests/Knights.Application.Tests/UserServiceTests.cs"
      ],
      "acceptance_criteria": [
        "Hash produces a verifiable hash; Verify returns true for correct password, false otherwise",
        "UserService.CreateAsync hashes the plaintext password before storing (no raw password persisted)",
        "Hash of same password twice is verifiable (salted scheme); not stored as plaintext",
        "DI registers IPasswordHasher"
      ]
    },
    {
      "id": "fs-2",
      "title": "IJwtTokenGenerator abstraction + Infra impl + Jwt appsettings section + auth middleware",
      "owner": "fullstack",
      "complexity": "complex",
      "depends_on": [],
      "files_touched": [
        "src/Knights.Application/Common/Interfaces/IJwtTokenGenerator.cs (new)",
        "src/Knights.Infrastructure/Security/JwtTokenGenerator.cs (new)",
        "src/Knights.Infrastructure/Security/JwtOptions.cs (new)",
        "src/Knights.Infrastructure/DependencyInjection.cs",
        "src/Knights.Api/Knights.Api.csproj (add Microsoft.AspNetCore.Authentication.JwtBearer)",
        "src/Knights.Api/Program.cs (AddAuthentication/AddAuthorization + UseAuthentication/UseAuthorization)",
        "src/Knights.Api/appsettings.json (Jwt section, placeholder secret)"
      ],
      "acceptance_criteria": [
        "Generate returns a signed JWT containing sub=user.Id, email, name claims and the expiry",
        "Token validates against the same Issuer/Audience/SecretKey via JwtBearer config",
        "appsettings has a Jwt section with placeholder SecretKey (not a real key)",
        "Program.cs registers authentication+authorization middleware in correct order"
      ]
    },
    {
      "id": "fs-3",
      "title": "Login use case: AuthService.LoginAsync (lookup, verify, RecordLogin, issue token)",
      "owner": "fullstack",
      "complexity": "complex",
      "depends_on": ["db-1", "fs-1", "fs-2"],
      "files_touched": [
        "src/Knights.Application/Auth/IAuthService.cs (new)",
        "src/Knights.Application/Auth/AuthService.cs (new)",
        "src/Knights.Application/Auth/Requests/LoginRequest.cs (new)",
        "src/Knights.Application/Auth/Responses/LoginResponse.cs (new)",
        "src/Knights.Application/DependencyInjection.cs",
        "tests/Knights.Application.Tests/AuthServiceTests.cs (new)"
      ],
      "acceptance_criteria": [
        "Valid username OR email + correct password returns token + expiry + UserResponse",
        "Unknown user returns auth failure without distinguishing user-not-found from bad-password",
        "Wrong password returns auth failure",
        "Inactive user (IsActive=false) is rejected",
        "Successful login calls User.RecordLogin and persists via repository.UpdateAsync"
      ]
    },
    {
      "id": "fs-4",
      "title": "POST /api/auth/login endpoint",
      "owner": "fullstack",
      "complexity": "low",
      "depends_on": ["fs-3"],
      "files_touched": [
        "src/Knights.Api/Endpoints/AuthEndpoints.cs (new)",
        "src/Knights.Api/Program.cs (MapAuthEndpoints)"
      ],
      "acceptance_criteria": [
        "POST /api/auth/login with valid credentials returns 200 + LoginResponse",
        "Invalid credentials return 401 (no leak of which field failed)",
        "Endpoint is anonymous (not behind auth)"
      ]
    },
    {
      "id": "fs-5",
      "title": "Angular auth plumbing: AuthService + token storage + JWT interceptor + auth guard + routing",
      "owner": "fullstack",
      "complexity": "complex",
      "depends_on": [],
      "files_touched": [
        "knights-ui/src/app/core/services/auth.service.ts (new)",
        "knights-ui/src/app/core/auth/jwt.interceptor.ts (new)",
        "knights-ui/src/app/core/auth/auth.guard.ts (new)",
        "knights-ui/src/app/core/models/auth.model.ts (new)",
        "knights-ui/src/app/app.config.ts (register interceptor)",
        "knights-ui/src/app/app.routes.ts (login route outside ShellComponent; guard on shell children)"
      ],
      "acceptance_criteria": [
        "AuthService.login posts to /api/auth/login, stores accessToken, exposes isAuthenticated/logout",
        "JWT interceptor attaches Authorization: Bearer <token> to API requests when present",
        "Auth guard redirects unauthenticated users to /login; allows authenticated through",
        "/login route is outside ShellComponent; shell children are guard-protected",
        "Specs cover guard redirect and interceptor header attachment"
      ]
    },
    {
      "id": "fs-6",
      "title": "Login page component (PrimeNG form, calls AuthService, error + loading states)",
      "owner": "fullstack",
      "complexity": "low",
      "depends_on": ["fs-5"],
      "files_touched": [
        "knights-ui/src/app/features/auth/login/login.component.ts (new)",
        "knights-ui/src/app/features/auth/login/login.component.html (new)",
        "knights-ui/src/app/app.routes.ts (wire /login -> LoginComponent)"
      ],
      "acceptance_criteria": [
        "Form with userNameOrEmail + password fields, submit disabled while pending",
        "On success navigates to /dashboard; on 401 shows an inline error message",
        "Matches existing PrimeNG/Aura styling used by other feature components",
        "Component spec covers success-navigation and error-display paths"
      ]
    }
  ]
}
```

## Execution rounds (dependency-derived)

- **Round 1 (parallel):** `db-1`, `fs-1`, `fs-2`, `fs-5` — no interdependencies.
- **Round 2:** `fs-3` (needs db-1, fs-1, fs-2), `fs-6` (needs fs-5).
- **Round 3:** `fs-4` (needs fs-3).

## Out of scope (explicit)

- Refresh tokens / token revocation / logout-server-side.
- Registration UI, password reset, email confirmation flows.
- Role/permission-based authorization on endpoints (claims are minimal; `[Authorize]`
  policies are a follow-up).
- A seeded login user: tests obtain a known credential via the hashed create path. For
  manual end-to-end use you'll need to create a user through `POST /api/users` first.
