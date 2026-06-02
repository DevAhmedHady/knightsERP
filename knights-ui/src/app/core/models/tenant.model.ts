export interface TenantResponse {
  id: string;
  codeName: string;
  name: string;
  description: string;
  environmentDisplayName: string;
  themeKey: string;
  worldDescription: string;
  isActive: boolean;
  expiryDate?: string;
  sessionTimeoutMinutes: number;
  ownerId: string;
  setupStartedAt?: string;
  setupCompletedAt?: string;
  roleIds: string[];
  permissionIds: string[];
}

export interface CreateTenantRequest {
  codeName: string;
  name: string;
  description: string;
  ownerId: string;
  expiryDate?: string;
  sessionTimeoutMinutes?: number;
}

export interface UpdateTenantRequest {
  name: string;
  description: string;
  expiryDate?: string;
  sessionTimeoutMinutes?: number;
}

export interface FeatureCatalogItemResponse {
  id: string;
  key: string;
  name: string;
  description: string;
  category: string;
  iconKey: string;
  tags: string[];
  dependencyKeys: string[];
  settingsSchemaJson: string;
  defaultSettingsJson: string;
  setupWeight: number;
  isCore: boolean;
  displayOrder: number;
  isPublished: boolean;
  isRetired: boolean;
}

export interface TenantSetupStepResponse {
  code: string;
  title: string;
  isCompleted: boolean;
  isRequired: boolean;
}

export interface TenantSetupSummaryResponse {
  tenantId: string;
  tenantName: string;
  environmentDisplayName: string;
  themeKey: string;
  worldDescription: string;
  progressPercent: number;
  isUnlocked: boolean;
  isComplete: boolean;
  steps: TenantSetupStepResponse[];
  availableFeatures: FeatureCatalogItemResponse[];
  selectedFeatures: TenantSelectedFeatureResponse[];
}

export interface TenantSelectedFeatureResponse extends FeatureCatalogItemResponse {
  settingsJson: string;
}

export interface ConfigureTenantEnvironmentRequest {
  environmentDisplayName: string;
  themeKey: string;
  worldDescription: string;
}

export interface CreateFeatureCatalogItemRequest {
  key: string;
  name: string;
  description: string;
  category: string;
  iconKey: string;
  tags: string[];
  dependencyKeys: string[];
  settingsSchemaJson: string;
  defaultSettingsJson: string;
  setupWeight: number;
  isCore: boolean;
  displayOrder: number;
  isPublished: boolean;
}

export interface UpdateFeatureCatalogItemRequest {
  name: string;
  description: string;
  category: string;
  iconKey: string;
  tags: string[];
  dependencyKeys: string[];
  settingsSchemaJson: string;
  defaultSettingsJson: string;
  setupWeight: number;
  isCore: boolean;
  displayOrder: number;
  isPublished: boolean;
  isRetired: boolean;
}

export interface UpdateTenantFeatureSettingsRequest {
  settingsJson: string;
}
