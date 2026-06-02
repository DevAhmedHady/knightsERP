using Mapster;
using Knights.Application.Common.Interfaces;
using Knights.Application.Common.Mapping;
using Knights.Application.Tenants.Requests;
using Knights.Application.Tenants.Responses;
using Knights.Domain.Tenants;

namespace Knights.Application.Tenants;

public sealed class TenantService(
    ITenantRepository tenantRepository,
    IUserRepository userRepository,
    ITenantContext tenantContext) : ITenantService
{
    private const int UnlockThresholdPercent = 50;

    static TenantService()
    {
        MapsterConfig.Register();
    }

    public async Task<TenantResponse> CreateAsync(CreateTenantRequest request, CancellationToken ct = default)
    {
        var tenant = Tenant.Create(request.CodeName, request.Name, request.Description, request.OwnerId, request.ExpiryDate);
        if (request.SessionTimeoutMinutes.HasValue)
            tenant.SetSessionTimeoutMinutes(request.SessionTimeoutMinutes.Value);

        await tenantRepository.AddAsync(tenant, ct);
        return MapTenantResponse(tenant);
    }

    public async Task<IReadOnlyCollection<TenantResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var tenants = await tenantRepository.GetAllAsync(ct);
        return tenants.Select(MapTenantResponse).ToList();
    }

    public async Task<TenantResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var tenant = await tenantRepository.GetByIdAsync(id, ct);
        return tenant is null ? null : MapTenantResponse(tenant);
    }

    public async Task<TenantResponse?> GetByCodeNameAsync(string codeName, CancellationToken ct = default)
    {
        var tenant = await tenantRepository.GetByCodeNameAsync(codeName, ct);
        return tenant is null ? null : MapTenantResponse(tenant);
    }

    public async Task UpdateAsync(Guid id, UpdateTenantRequest request, CancellationToken ct = default)
    {
        var tenant = await GetRequiredTenantAsync(id, ct);
        tenant.Update(request.Name, request.Description, request.ExpiryDate, request.SessionTimeoutMinutes);
        await tenantRepository.UpdateAsync(tenant, ct);
    }

    public async Task AssignRoleAsync(Guid tenantId, Guid roleId, CancellationToken ct = default)
    {
        var tenant = await GetRequiredTenantAsync(tenantId, ct);
        tenant.AssignRole(roleId);
        await tenantRepository.UpdateAsync(tenant, ct);
    }

    public async Task RemoveRoleAsync(Guid tenantId, Guid roleId, CancellationToken ct = default)
    {
        var tenant = await GetRequiredTenantAsync(tenantId, ct);
        tenant.RemoveRole(roleId);
        await tenantRepository.UpdateAsync(tenant, ct);
    }

    public async Task GrantPermissionAsync(Guid tenantId, Guid permissionId, CancellationToken ct = default)
    {
        var tenant = await GetRequiredTenantAsync(tenantId, ct);
        tenant.GrantPermission(permissionId);
        await tenantRepository.UpdateAsync(tenant, ct);
    }

    public async Task RevokePermissionAsync(Guid tenantId, Guid permissionId, CancellationToken ct = default)
    {
        var tenant = await GetRequiredTenantAsync(tenantId, ct);
        tenant.RevokePermission(permissionId);
        await tenantRepository.UpdateAsync(tenant, ct);
    }

    public async Task AddMemberAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var tenant = await GetRequiredTenantAsync(tenantId, ct);
        if (!tenant.IsActive || tenant.IsExpired)
            throw new InvalidOperationException($"Tenant '{tenantId}' is not active.");

        var user = await userRepository.GetByIdAsync(userId, ct)
            ?? throw new InvalidOperationException($"User '{userId}' was not found.");

        if (user.TenantId.HasValue && user.TenantId.Value != tenantId)
            throw new InvalidOperationException($"User '{userId}' already belongs to a different tenant.");

        user.JoinTenant(tenantId);
        await userRepository.UpdateAsync(user, ct);
    }

    public async Task RemoveMemberAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var user = await userRepository.GetByIdAsync(userId, ct)
            ?? throw new InvalidOperationException($"User '{userId}' was not found.");

        if (user.TenantId != tenantId)
            throw new InvalidOperationException($"User '{userId}' does not belong to tenant '{tenantId}'.");

        user.LeaveTenant();
        await userRepository.UpdateAsync(user, ct);
    }

    public async Task<TenantSetupSummaryResponse> GetCurrentSetupAsync(CancellationToken ct = default)
    {
        var tenant = await GetCurrentTenantAsync(ct);
        var availableFeatures = await tenantRepository.GetCatalogFeaturesAsync(includeUnpublished: false, ct);
        return BuildSetupSummary(tenant, availableFeatures);
    }

    public async Task<TenantSetupSummaryResponse> ConfigureCurrentEnvironmentAsync(ConfigureTenantEnvironmentRequest request, CancellationToken ct = default)
    {
        var tenant = await GetCurrentTenantAsync(ct);
        tenant.ConfigureEnvironment(request.EnvironmentDisplayName, request.ThemeKey, request.WorldDescription);

        var availableFeatures = await tenantRepository.GetCatalogFeaturesAsync(includeUnpublished: false, ct);
        tenant.SyncSetupCompletion(CalculateProgressPercent(tenant));
        await tenantRepository.UpdateAsync(tenant, ct);
        return BuildSetupSummary(tenant, availableFeatures);
    }

    public async Task<TenantSetupSummaryResponse> SelectCurrentFeatureAsync(Guid featureId, CancellationToken ct = default)
    {
        var tenant = await GetCurrentTenantAsync(ct);
        var feature = await tenantRepository.GetCatalogFeatureByIdAsync(featureId, ct)
            ?? throw new InvalidOperationException($"Feature '{featureId}' was not found.");

        if (!feature.IsPublished || feature.IsRetired)
            throw new InvalidOperationException("Feature is not currently available for tenant selection.");

        var catalogFeatures = await tenantRepository.GetCatalogFeaturesAsync(includeUnpublished: true, ct);
        var selectedKeys = tenant.TenantFeatureSelections
            .Select(selection => catalogFeatures.FirstOrDefault(item => item.Id == selection.FeatureCatalogItemId)?.Key)
            .Where(key => !string.IsNullOrWhiteSpace(key))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var missingDependencies = feature.DependencyKeys
            .Where(key => !selectedKeys.Contains(key))
            .ToArray();

        if (missingDependencies.Length > 0)
            throw new InvalidOperationException($"Feature '{feature.Key}' requires: {string.Join(", ", missingDependencies)}.");

        tenant.SelectFeature(feature.Id);
        tenant.SyncSetupCompletion(CalculateProgressPercent(tenant));
        await tenantRepository.UpdateAsync(tenant, ct);

        var availableFeatures = await tenantRepository.GetCatalogFeaturesAsync(includeUnpublished: false, ct);
        var refreshed = await GetRequiredTenantAsync(tenant.Id, ct);
        return BuildSetupSummary(refreshed, availableFeatures);
    }

    public async Task<TenantSetupSummaryResponse> RemoveCurrentFeatureAsync(Guid featureId, CancellationToken ct = default)
    {
        var tenant = await GetCurrentTenantAsync(ct);
        var feature = await tenantRepository.GetCatalogFeatureByIdAsync(featureId, ct)
            ?? throw new InvalidOperationException($"Feature '{featureId}' was not found.");

        var blockingDependents = tenant.TenantFeatureSelections
            .Select(selection => selection.FeatureCatalogItem)
            .Where(item => item is not null && item.DependencyKeys.Contains(feature.Key, StringComparer.OrdinalIgnoreCase))
            .Select(item => item!.Key)
            .ToArray();

        if (blockingDependents.Length > 0)
            throw new InvalidOperationException($"Feature '{feature.Key}' is required by: {string.Join(", ", blockingDependents)}.");

        tenant.RemoveFeature(feature.Id);
        tenant.SyncSetupCompletion(CalculateProgressPercent(tenant));
        await tenantRepository.UpdateAsync(tenant, ct);

        var availableFeatures = await tenantRepository.GetCatalogFeaturesAsync(includeUnpublished: false, ct);
        var refreshed = await GetRequiredTenantAsync(tenant.Id, ct);
        return BuildSetupSummary(refreshed, availableFeatures);
    }

    public async Task<TenantSetupSummaryResponse> UpdateCurrentFeatureSettingsAsync(Guid featureId, UpdateTenantFeatureSettingsRequest request, CancellationToken ct = default)
    {
        var tenant = await GetCurrentTenantAsync(ct);
        var selection = tenant.TenantFeatureSelections.FirstOrDefault(item => item.FeatureCatalogItemId == featureId)
            ?? throw new InvalidOperationException($"Feature '{featureId}' is not selected for this tenant.");

        selection.UpdateSettings(request.SettingsJson);
        await tenantRepository.UpdateAsync(tenant, ct);

        var availableFeatures = await tenantRepository.GetCatalogFeaturesAsync(includeUnpublished: false, ct);
        var refreshed = await GetRequiredTenantAsync(tenant.Id, ct);
        return BuildSetupSummary(refreshed, availableFeatures);
    }

    public async Task<IReadOnlyCollection<FeatureCatalogItemResponse>> GetCatalogAsync(CancellationToken ct = default)
    {
        var features = await tenantRepository.GetCatalogFeaturesAsync(includeUnpublished: IsSystemAdmin, ct);
        return features.Select(MapFeature).ToList();
    }

    public async Task<FeatureCatalogItemResponse> CreateCatalogFeatureAsync(CreateFeatureCatalogItemRequest request, CancellationToken ct = default)
    {
        EnsureSystemAdmin();

        var existing = await tenantRepository.GetCatalogFeatureByKeyAsync(request.Key, ct);
        if (existing is not null)
            throw new InvalidOperationException($"Feature key '{request.Key}' already exists.");

        ValidateDependencyLoop(request.Key, request.DependencyKeys);

        var feature = FeatureCatalogItem.Create(
            request.Key,
            request.Name,
            request.Description,
            request.Category,
            request.IconKey,
            request.Tags,
            request.DependencyKeys,
            request.SettingsSchemaJson,
            request.DefaultSettingsJson,
            request.SetupWeight,
            request.IsCore,
            request.DisplayOrder,
            request.IsPublished);

        await tenantRepository.AddCatalogFeatureAsync(feature, ct);
        return MapFeature(feature);
    }

    public async Task<FeatureCatalogItemResponse> UpdateCatalogFeatureAsync(Guid featureId, UpdateFeatureCatalogItemRequest request, CancellationToken ct = default)
    {
        EnsureSystemAdmin();

        var feature = await tenantRepository.GetCatalogFeatureByIdAsync(featureId, ct)
            ?? throw new InvalidOperationException($"Feature '{featureId}' was not found.");

        ValidateDependencyLoop(feature.Key, request.DependencyKeys);

        feature.Update(
            request.Name,
            request.Description,
            request.Category,
            request.IconKey,
            request.Tags,
            request.DependencyKeys,
            request.SettingsSchemaJson,
            request.DefaultSettingsJson,
            request.SetupWeight,
            request.IsCore,
            request.DisplayOrder,
            request.IsPublished,
            request.IsRetired);

        await tenantRepository.UpdateCatalogFeatureAsync(feature, ct);
        return MapFeature(feature);
    }

    private async Task<Tenant> GetRequiredTenantAsync(Guid id, CancellationToken ct)
    {
        var tenant = await tenantRepository.GetByIdAsync(id, ct);
        return tenant ?? throw new InvalidOperationException($"Tenant '{id}' was not found.");
    }

    private async Task<Tenant> GetCurrentTenantAsync(CancellationToken ct)
    {
        if (!tenantContext.TenantId.HasValue)
            throw new InvalidOperationException("Current request is not tenant-scoped.");

        return await GetRequiredTenantAsync(tenantContext.TenantId.Value, ct);
    }

    private bool IsSystemAdmin => !tenantContext.TenantId.HasValue;

    private void EnsureSystemAdmin()
    {
        if (!IsSystemAdmin)
            throw new InvalidOperationException("Only system admins can manage the feature catalog.");
    }

    private static void ValidateDependencyLoop(string key, IEnumerable<string> dependencyKeys)
    {
        var normalized = key.Trim().ToUpperInvariant();
        if (dependencyKeys.Any(value => string.Equals(value.Trim(), normalized, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Feature '{normalized}' cannot depend on itself.");
    }

    private static TenantResponse MapTenantResponse(Tenant tenant)
    {
        return new TenantResponse(
            tenant.Id,
            tenant.CodeName,
            tenant.Name,
            tenant.Description,
            tenant.EnvironmentDisplayName,
            tenant.ThemeKey,
            tenant.WorldDescription,
            tenant.IsActive,
            tenant.ExpiryDate,
            tenant.SessionTimeoutMinutes,
            tenant.OwnerId,
            tenant.SetupStartedAt,
            tenant.SetupCompletedAt,
            tenant.TenantRoles.Select(role => role.RoleId).ToList(),
            tenant.TenantPermissions.Select(permission => permission.PermissionId).ToList());
    }

    private static FeatureCatalogItemResponse MapFeature(FeatureCatalogItem feature)
    {
        return new FeatureCatalogItemResponse(
            feature.Id,
            feature.Key,
            feature.Name,
            feature.Description,
            feature.Category,
            feature.IconKey,
            feature.Tags.ToArray(),
            feature.DependencyKeys.ToArray(),
            feature.SettingsSchemaJson,
            feature.DefaultSettingsJson,
            feature.SetupWeight,
            feature.IsCore,
            feature.DisplayOrder,
            feature.IsPublished,
            feature.IsRetired);
    }

    private static int CalculateProgressPercent(Tenant tenant)
    {
        var completedSteps = GetSteps(tenant).Count(step => step.IsCompleted);
        return completedSteps * 25;
    }

    private static IReadOnlyCollection<TenantSetupStepResponse> GetSteps(Tenant tenant)
    {
        return
        [
            new TenantSetupStepResponse(
                "environment-profile",
                "Configure environment identity",
                !string.IsNullOrWhiteSpace(tenant.EnvironmentDisplayName) && !string.IsNullOrWhiteSpace(tenant.ThemeKey),
                true),
            new TenantSetupStepResponse(
                "world-description",
                "Describe world and workspace rules",
                !string.IsNullOrWhiteSpace(tenant.WorldDescription),
                true),
            new TenantSetupStepResponse(
                "access-model",
                "Assign at least one tenant role or permission",
                tenant.TenantRoles.Count > 0 || tenant.TenantPermissions.Count > 0,
                true),
            new TenantSetupStepResponse(
                "feature-selection",
                "Select at least one published feature",
                tenant.TenantFeatureSelections.Count > 0,
                true)
        ];
    }

    private static TenantSetupSummaryResponse BuildSetupSummary(Tenant tenant, IReadOnlyCollection<FeatureCatalogItem> availableFeatures)
    {
        var steps = GetSteps(tenant);
        var progressPercent = steps.Count(step => step.IsCompleted) * 25;
        tenant.SyncSetupCompletion(progressPercent);

        var featureLookup = availableFeatures.ToDictionary(feature => feature.Id, feature => feature);
        foreach (var selectionFeature in tenant.TenantFeatureSelections.Select(selection => selection.FeatureCatalogItem).Where(feature => feature is not null))
            featureLookup[selectionFeature!.Id] = selectionFeature;

        var selectedFeatures = tenant.TenantFeatureSelections
            .Select(selection =>
            {
                var feature = featureLookup.GetValueOrDefault(selection.FeatureCatalogItemId);
                return feature is null ? null : MapSelectedFeature(feature, selection.SettingsJson);
            })
            .Where(feature => feature is not null)
            .Select(feature => feature!)
            .ToList();

        return new TenantSetupSummaryResponse(
            tenant.Id,
            tenant.Name,
            tenant.EnvironmentDisplayName,
            tenant.ThemeKey,
            tenant.WorldDescription,
            progressPercent,
            progressPercent >= UnlockThresholdPercent,
            progressPercent >= 100,
            steps,
            availableFeatures.Select(MapFeature).ToList(),
            selectedFeatures);
    }

    private static TenantSelectedFeatureResponse MapSelectedFeature(FeatureCatalogItem feature, string settingsJson)
    {
        return new TenantSelectedFeatureResponse(
            feature.Id,
            feature.Key,
            feature.Name,
            feature.Description,
            feature.Category,
            feature.IconKey,
            feature.Tags.ToArray(),
            feature.DependencyKeys.ToArray(),
            feature.SettingsSchemaJson,
            feature.DefaultSettingsJson,
            settingsJson,
            feature.SetupWeight,
            feature.IsCore,
            feature.DisplayOrder,
            feature.IsPublished,
            feature.IsRetired);
    }
}
