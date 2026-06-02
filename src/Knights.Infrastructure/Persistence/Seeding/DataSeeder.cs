using Knights.Application.Common.Interfaces;
using Knights.Domain.Identity;
using Knights.Domain.Tenants;
using Microsoft.EntityFrameworkCore;

namespace Knights.Infrastructure.Persistence.Seeding;

public static class DataSeeder
{
    public static async Task SeedAsync(KnightsDbContext dbContext, IPasswordHasher passwordHasher)
    {
        await SeedPermissionsAsync(dbContext);
        await SeedRolesAsync(dbContext);
        await SeedRolePermissionsAsync(dbContext);
        await SeedFeatureCatalogAsync(dbContext);
        await SeedUsersAndTenantsAsync(dbContext, passwordHasher);
    }

    private sealed record PermissionSeed(string CodeName, string DisplayName, string Description);
    private sealed record RoleSeed(string Name, string Description, bool IsStatic, bool IsDefault, bool IsActive, params string[] PermissionCodes);
    private sealed record UserSeed(string FirstName, string MidName, string LastName, string UserName, string Email, string Password, bool IsEmailConfirmed, string[] RoleNames, string[] PermissionCodes);
    private sealed record TenantSeed(string CodeName, string Name, string Description, string EnvironmentDisplayName, string ThemeKey, string WorldDescription, string OwnerUserName, int ExpiryDays, int SetupProgress, string[] RoleNames, string[] PermissionCodes, string[] FeatureKeys, UserSeed[] Members);

    private static async Task SeedPermissionsAsync(KnightsDbContext dbContext)
    {
        var seeds = new[]
        {
            new PermissionSeed("SYSTEM_USERS_VIEW", "View System Users", "Read system-level user directory and user profiles."),
            new PermissionSeed("SYSTEM_USERS_MANAGE", "Manage System Users", "Create, update, activate, and deactivate system users."),
            new PermissionSeed("SYSTEM_ROLES_VIEW", "View System Roles", "Read system role catalog and role assignments."),
            new PermissionSeed("SYSTEM_ROLES_MANAGE", "Manage System Roles", "Create, update, and assign system roles."),
            new PermissionSeed("SYSTEM_PERMISSIONS_VIEW", "View System Permissions", "Read the system permission catalog."),
            new PermissionSeed("SYSTEM_PERMISSIONS_MANAGE", "Manage System Permissions", "Create, update, and retire system permissions."),
            new PermissionSeed("SYSTEM_TENANTS_VIEW", "View Tenants", "Read tenant records, health, and readiness."),
            new PermissionSeed("SYSTEM_TENANTS_MANAGE", "Manage Tenants", "Create, update, activate, and suspend tenants."),
            new PermissionSeed("SYSTEM_FEATURE_CATALOG_VIEW", "View Feature Catalog", "Read the global tenant feature catalog."),
            new PermissionSeed("SYSTEM_FEATURE_CATALOG_MANAGE", "Manage Feature Catalog", "Publish, retire, and configure tenant features."),
            new PermissionSeed("TENANT_ENVIRONMENT_VIEW", "View Tenant Environment", "Read tenant environment setup and workspace state."),
            new PermissionSeed("TENANT_ENVIRONMENT_CONFIGURE", "Configure Tenant Environment", "Edit tenant environment setup, branding, and world metadata."),
            new PermissionSeed("TENANT_FEATURES_SELECT", "Select Tenant Features", "Choose and configure enabled features for a tenant."),
            new PermissionSeed("TENANT_USERS_VIEW", "View Tenant Users", "Read tenant user directory and assignments."),
            new PermissionSeed("TENANT_USERS_MANAGE", "Manage Tenant Users", "Invite, update, and deactivate tenant users."),
            new PermissionSeed("TENANT_ROLES_VIEW", "View Tenant Roles", "Read tenant role templates and grants."),
            new PermissionSeed("TENANT_ROLES_MANAGE", "Manage Tenant Roles", "Assign tenant roles and access policies."),
            new PermissionSeed("TENANT_ANALYTICS_VIEW", "View Tenant Analytics", "Read tenant dashboards and readiness metrics."),
            new PermissionSeed("TENANT_OPERATIONS_VIEW", "View Tenant Operations", "Read operational queues, maps, and event streams."),
            new PermissionSeed("TENANT_OPERATIONS_MANAGE", "Manage Tenant Operations", "Manage operational workflows and dispatch actions.")
        };

        var existing = await dbContext.Permissions
            .ToDictionaryAsync(permission => permission.CodeName, StringComparer.OrdinalIgnoreCase);

        var missing = new List<Permission>();

        foreach (var seed in seeds)
        {
            if (existing.ContainsKey(seed.CodeName))
                continue;

            missing.Add(Permission.Create(seed.CodeName, seed.DisplayName, seed.Description));
        }

        if (missing.Count == 0)
            return;

        dbContext.Permissions.AddRange(missing);
        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedRolesAsync(KnightsDbContext dbContext)
    {
        var seeds = GetRoleSeeds();
        var existing = await dbContext.Roles
            .ToDictionaryAsync(role => role.CodeName, StringComparer.OrdinalIgnoreCase);

        var missing = new List<Role>();

        foreach (var seed in seeds)
        {
            var codeName = ToCodeName(seed.Name);
            if (existing.ContainsKey(codeName))
                continue;

            missing.Add(Role.Create(seed.Name, seed.Description, seed.IsStatic, seed.IsDefault, seed.IsActive));
        }

        if (missing.Count == 0)
            return;

        dbContext.Roles.AddRange(missing);
        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedRolePermissionsAsync(KnightsDbContext dbContext)
    {
        var roleMap = await dbContext.Roles
            .Include(role => role.Permissions)
            .ToDictionaryAsync(role => role.CodeName, StringComparer.OrdinalIgnoreCase);

        var permissionMap = await dbContext.Permissions
            .ToDictionaryAsync(permission => permission.CodeName, StringComparer.OrdinalIgnoreCase);

        foreach (var seed in GetRoleSeeds())
        {
            var codeName = ToCodeName(seed.Name);
            if (!roleMap.TryGetValue(codeName, out var role))
                continue;

            foreach (var permissionCode in seed.PermissionCodes)
            {
                if (!permissionMap.TryGetValue(permissionCode, out var permission))
                    continue;

                role.AssignPermission(permission.Id);
            }
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedUsersAndTenantsAsync(KnightsDbContext dbContext, IPasswordHasher passwordHasher)
    {
        var roleMap = await dbContext.Roles
            .ToDictionaryAsync(role => role.CodeName, StringComparer.OrdinalIgnoreCase);

        var permissionMap = await dbContext.Permissions
            .ToDictionaryAsync(permission => permission.CodeName, StringComparer.OrdinalIgnoreCase);

        var existingUsers = await dbContext.Users
            .Include(user => user.UserRoles)
            .Include(user => user.UserPermissions)
            .ToDictionaryAsync(user => user.UserName, StringComparer.OrdinalIgnoreCase);

        var systemUsers = GetSystemUsers();

        foreach (var seed in systemUsers)
        {
            await EnsureUserAsync(dbContext, existingUsers, roleMap, permissionMap, passwordHasher, seed, tenantId: null);
        }

        foreach (var tenantSeed in GetTenantSeeds())
        {
            foreach (var member in tenantSeed.Members)
            {
                await EnsureUserAsync(dbContext, existingUsers, roleMap, permissionMap, passwordHasher, member, tenantId: null);
            }
        }

        await dbContext.SaveChangesAsync();

        var refreshedUsers = await dbContext.Users
            .Include(user => user.UserRoles)
            .Include(user => user.UserPermissions)
            .ToDictionaryAsync(user => user.UserName, StringComparer.OrdinalIgnoreCase);

        var tenants = await dbContext.Tenants
            .Include(tenant => tenant.TenantRoles)
            .Include(tenant => tenant.TenantPermissions)
            .Include(tenant => tenant.TenantFeatureSelections)
            .ToDictionaryAsync(tenant => tenant.CodeName, StringComparer.OrdinalIgnoreCase);

        var featureMap = await dbContext.FeatureCatalogItems
            .ToDictionaryAsync(feature => feature.Key, StringComparer.OrdinalIgnoreCase);

        foreach (var seed in GetTenantSeeds())
        {
            if (!refreshedUsers.TryGetValue(seed.OwnerUserName, out var owner))
                continue;

            if (!tenants.TryGetValue(seed.CodeName, out var tenant))
            {
                tenant = Tenant.Create(
                    seed.CodeName,
                    seed.Name,
                    seed.Description,
                    owner.Id,
                    DateTime.UtcNow.AddDays(seed.ExpiryDays));

                tenant.ConfigureEnvironment(seed.EnvironmentDisplayName, seed.ThemeKey, seed.WorldDescription);
                dbContext.Tenants.Add(tenant);
                tenants.Add(tenant.CodeName, tenant);
            }
            else
            {
                tenant.Update(seed.Name, seed.Description, DateTime.UtcNow.AddDays(seed.ExpiryDays));
                tenant.ConfigureEnvironment(seed.EnvironmentDisplayName, seed.ThemeKey, seed.WorldDescription);
                tenant.SetActive(true);
            }

            foreach (var roleName in seed.RoleNames)
            {
                if (roleMap.TryGetValue(ToCodeName(roleName), out var role))
                    tenant.AssignRole(role.Id);
            }

            foreach (var permissionCode in seed.PermissionCodes)
            {
                if (permissionMap.TryGetValue(permissionCode, out var permission))
                    tenant.GrantPermission(permission.Id);
            }

            foreach (var featureKey in seed.FeatureKeys)
            {
                if (featureMap.TryGetValue(featureKey, out var feature))
                    tenant.SelectFeature(feature.Id);
            }

            tenant.SyncSetupCompletion(seed.SetupProgress);
        }

        await dbContext.SaveChangesAsync();

        var tenantMap = await dbContext.Tenants
            .ToDictionaryAsync(tenant => tenant.CodeName, StringComparer.OrdinalIgnoreCase);

        var scopedUsers = await dbContext.Users
            .Include(user => user.UserRoles)
            .Include(user => user.UserPermissions)
            .ToDictionaryAsync(user => user.UserName, StringComparer.OrdinalIgnoreCase);

        foreach (var tenantSeed in GetTenantSeeds())
        {
            if (!tenantMap.TryGetValue(tenantSeed.CodeName, out var tenant))
                continue;

            if (scopedUsers.TryGetValue(tenantSeed.OwnerUserName, out var owner))
                owner.JoinTenant(tenant.Id);

            foreach (var member in tenantSeed.Members)
            {
                await EnsureUserAsync(dbContext, scopedUsers, roleMap, permissionMap, passwordHasher, member, tenant.Id);
            }
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task<User> EnsureUserAsync(
        KnightsDbContext dbContext,
        IDictionary<string, User> existingUsers,
        IReadOnlyDictionary<string, Role> roleMap,
        IReadOnlyDictionary<string, Permission> permissionMap,
        IPasswordHasher passwordHasher,
        UserSeed seed,
        Guid? tenantId)
    {
        if (!existingUsers.TryGetValue(seed.UserName, out var user))
        {
            user = User.Create(
                seed.FirstName,
                seed.MidName,
                seed.LastName,
                seed.UserName,
                seed.Email,
                passwordHasher.Hash(seed.Password),
                seed.IsEmailConfirmed);

            if (tenantId.HasValue)
                user.JoinTenant(tenantId.Value);

            dbContext.Users.Add(user);
            existingUsers.Add(user.UserName, user);
        }
        else
        {
            user.UpdateProfile(seed.FirstName, seed.MidName, seed.LastName, seed.UserName, seed.Email, seed.IsEmailConfirmed);
            user.SetActive(true);
            user.SetPasswordHash(passwordHasher.Hash(seed.Password));

            if (tenantId.HasValue)
                user.JoinTenant(tenantId.Value);
        }

        foreach (var roleName in seed.RoleNames)
        {
            if (roleMap.TryGetValue(ToCodeName(roleName), out var role))
                user.AssignRole(role.Id);
        }

        foreach (var permissionCode in seed.PermissionCodes)
        {
            if (permissionMap.TryGetValue(permissionCode, out var permission))
                user.GrantPermission(permission.Id);
        }

        return await Task.FromResult(user);
    }

    private static async Task SeedFeatureCatalogAsync(KnightsDbContext dbContext)
    {
        var items = new[]
        {
            FeatureCatalogItem.Create(
                key: "BASE_WORLD",
                name: "Base World",
                description: "Core environment shell, workspace identity, and tenant bootstrap panels.",
                category: "Foundation",
                iconKey: "pi pi-globe",
                tags: ["core", "setup", "workspace"],
                dependencyKeys: [],
                settingsSchemaJson: """{"type":"object","properties":{"brandingMode":{"type":"string","enum":["basic","advanced"]}}}""",
                defaultSettingsJson: """{"brandingMode":"basic"}""",
                setupWeight: 30,
                isCore: true,
                displayOrder: 1,
                isPublished: true),
            FeatureCatalogItem.Create(
                key: "ACCESS_MATRIX",
                name: "Access Matrix",
                description: "Role, permission, and delegated access controls for tenant managers.",
                category: "Security",
                iconKey: "pi pi-shield",
                tags: ["security", "roles"],
                dependencyKeys: ["BASE_WORLD"],
                settingsSchemaJson: """{"type":"object","properties":{"approvalRequired":{"type":"boolean"}}}""",
                defaultSettingsJson: """{"approvalRequired":true}""",
                setupWeight: 20,
                isCore: true,
                displayOrder: 2,
                isPublished: true),
            FeatureCatalogItem.Create(
                key: "EVENT_STREAM",
                name: "Event Stream",
                description: "Operational events, notifications, and timeline feeds for the tenant world.",
                category: "Operations",
                iconKey: "pi pi-bolt",
                tags: ["events", "notifications"],
                dependencyKeys: ["BASE_WORLD"],
                settingsSchemaJson: """{"type":"object","properties":{"retentionDays":{"type":"integer","minimum":7}}}""",
                defaultSettingsJson: """{"retentionDays":30}""",
                setupWeight: 15,
                isCore: false,
                displayOrder: 3,
                isPublished: true),
            FeatureCatalogItem.Create(
                key: "FIELD_OPERATIONS",
                name: "Field Operations",
                description: "Location-driven work queues, team dispatch, and operational tracking.",
                category: "Operations",
                iconKey: "pi pi-map",
                tags: ["field", "map", "ops"],
                dependencyKeys: ["BASE_WORLD", "EVENT_STREAM"],
                settingsSchemaJson: """{"type":"object","properties":{"zones":{"type":"array","items":{"type":"string"}}}}""",
                defaultSettingsJson: """{"zones":[]}""",
                setupWeight: 20,
                isCore: false,
                displayOrder: 4,
                isPublished: true),
            FeatureCatalogItem.Create(
                key: "ANALYTICS_HUB",
                name: "Analytics Hub",
                description: "Dashboards, health scoring, and readiness metrics for tenant environments.",
                category: "Insights",
                iconKey: "pi pi-chart-bar",
                tags: ["analytics", "health"],
                dependencyKeys: ["BASE_WORLD"],
                settingsSchemaJson: """{"type":"object","properties":{"refreshMinutes":{"type":"integer","minimum":5}}}""",
                defaultSettingsJson: """{"refreshMinutes":15}""",
                setupWeight: 15,
                isCore: false,
                displayOrder: 5,
                isPublished: true)
        };

        var existingKeys = await dbContext.FeatureCatalogItems
            .Select(item => item.Key)
            .ToListAsync();

        var existingKeySet = existingKeys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missingItems = items
            .Where(item => !existingKeySet.Contains(item.Key))
            .ToArray();

        if (missingItems.Length == 0)
            return;

        dbContext.FeatureCatalogItems.AddRange(missingItems);
        await dbContext.SaveChangesAsync();
    }

    private static RoleSeed[] GetRoleSeeds()
    {
        return
        [
            new RoleSeed(
                "Admin",
                "System administrator with unrestricted platform access.",
                true,
                false,
                true,
                "SYSTEM_USERS_VIEW",
                "SYSTEM_USERS_MANAGE",
                "SYSTEM_ROLES_VIEW",
                "SYSTEM_ROLES_MANAGE",
                "SYSTEM_PERMISSIONS_VIEW",
                "SYSTEM_PERMISSIONS_MANAGE",
                "SYSTEM_TENANTS_VIEW",
                "SYSTEM_TENANTS_MANAGE",
                "SYSTEM_FEATURE_CATALOG_VIEW",
                "SYSTEM_FEATURE_CATALOG_MANAGE",
                "TENANT_ENVIRONMENT_VIEW",
                "TENANT_ENVIRONMENT_CONFIGURE",
                "TENANT_FEATURES_SELECT",
                "TENANT_USERS_VIEW",
                "TENANT_USERS_MANAGE",
                "TENANT_ROLES_VIEW",
                "TENANT_ROLES_MANAGE",
                "TENANT_ANALYTICS_VIEW",
                "TENANT_OPERATIONS_VIEW",
                "TENANT_OPERATIONS_MANAGE"),
            new RoleSeed(
                "User",
                "Baseline authenticated user with read-only tenant access.",
                true,
                true,
                true,
                "TENANT_ENVIRONMENT_VIEW",
                "TENANT_ANALYTICS_VIEW"),
            new RoleSeed(
                "System Operator",
                "System operations role for tenant lifecycle and catalog oversight.",
                true,
                false,
                true,
                "SYSTEM_TENANTS_VIEW",
                "SYSTEM_TENANTS_MANAGE",
                "SYSTEM_FEATURE_CATALOG_VIEW",
                "SYSTEM_FEATURE_CATALOG_MANAGE",
                "SYSTEM_USERS_VIEW"),
            new RoleSeed(
                "Tenant Admin",
                "Tenant administrator with setup, feature, user, and role control inside one tenant.",
                true,
                false,
                true,
                "TENANT_ENVIRONMENT_VIEW",
                "TENANT_ENVIRONMENT_CONFIGURE",
                "TENANT_FEATURES_SELECT",
                "TENANT_USERS_VIEW",
                "TENANT_USERS_MANAGE",
                "TENANT_ROLES_VIEW",
                "TENANT_ROLES_MANAGE",
                "TENANT_ANALYTICS_VIEW",
                "TENANT_OPERATIONS_VIEW",
                "TENANT_OPERATIONS_MANAGE"),
            new RoleSeed(
                "Tenant Manager",
                "Operational manager for tenant configuration and day-to-day execution.",
                true,
                false,
                true,
                "TENANT_ENVIRONMENT_VIEW",
                "TENANT_ENVIRONMENT_CONFIGURE",
                "TENANT_FEATURES_SELECT",
                "TENANT_USERS_VIEW",
                "TENANT_ANALYTICS_VIEW",
                "TENANT_OPERATIONS_VIEW",
                "TENANT_OPERATIONS_MANAGE"),
            new RoleSeed(
                "Tenant Viewer",
                "Read-only tenant member for dashboards and workspace visibility.",
                true,
                false,
                true,
                "TENANT_ENVIRONMENT_VIEW",
                "TENANT_ANALYTICS_VIEW",
                "TENANT_OPERATIONS_VIEW")
        ];
    }

    private static UserSeed[] GetSystemUsers()
    {
        return
        [
            new UserSeed("Admin", "System", "User", "admin", "admin@knights.local", "Admin@123456", true, ["Admin"], []),
            new UserSeed("Sara", "Platform", "Ops", "sysops", "sysops@knights.local", "Admin@123456", true, ["System Operator"], []),
            new UserSeed("Nora", "Portfolio", "Owner", "portfolio.owner", "portfolio.owner@knights.local", "Admin@123456", true, ["Admin"], ["SYSTEM_TENANTS_VIEW"])
        ];
    }

    private static TenantSeed[] GetTenantSeeds()
    {
        return
        [
            new TenantSeed(
                "RED_KEEP",
                "Red Keep Holdings",
                "Flagship tenant for environment-builder and operational readiness scenarios.",
                "Red Keep Command World",
                "crimson-command",
                "Operational workspace for command teams, tenant setup, and readiness analytics.",
                "red.admin",
                365,
                100,
                ["Tenant Admin", "Tenant Manager", "Tenant Viewer"],
                ["TENANT_ENVIRONMENT_VIEW", "TENANT_ANALYTICS_VIEW"],
                ["BASE_WORLD", "ACCESS_MATRIX", "EVENT_STREAM", "FIELD_OPERATIONS", "ANALYTICS_HUB"],
                [
                    new UserSeed("Rania", "Tenant", "Admin", "red.admin", "red.admin@knights.local", "Admin@123456", true, ["Tenant Admin"], []),
                    new UserSeed("Omar", "Field", "Lead", "red.manager", "red.manager@knights.local", "Admin@123456", true, ["Tenant Manager"], []),
                    new UserSeed("Lina", "Ops", "Viewer", "red.viewer", "red.viewer@knights.local", "Admin@123456", true, ["Tenant Viewer"], [])
                ]),
            new TenantSeed(
                "NORTHWATCH",
                "Northwatch Logistics",
                "Secondary tenant with partial setup completion for first-run enforcement testing.",
                "Northwatch Frontier",
                "midnight-ops",
                "Logistics and field operations environment with partial onboarding state.",
                "north.admin",
                365,
                50,
                ["Tenant Admin", "Tenant Manager", "Tenant Viewer"],
                ["TENANT_ENVIRONMENT_VIEW", "TENANT_OPERATIONS_VIEW"],
                ["BASE_WORLD", "ACCESS_MATRIX", "EVENT_STREAM"],
                [
                    new UserSeed("Youssef", "Tenant", "Admin", "north.admin", "north.admin@knights.local", "Admin@123456", true, ["Tenant Admin"], []),
                    new UserSeed("Maha", "Dispatch", "Lead", "north.manager", "north.manager@knights.local", "Admin@123456", true, ["Tenant Manager"], []),
                    new UserSeed("Salma", "Insights", "Viewer", "north.viewer", "north.viewer@knights.local", "Admin@123456", true, ["Tenant Viewer"], [])
                ])
        ];
    }

    private static string ToCodeName(string value)
    {
        return value.Trim().ToUpperInvariant().Replace(" ", "_");
    }
}
