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
}

export interface UpdateTenantRequest {
  name: string;
  description: string;
  expiryDate?: string;
}

export interface FeatureCatalogItemResponse {
  id: string;
  key: string;
  name: string;
  description: string;
  category: string;
  dependencyKeys: string[];
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
  selectedFeatures: FeatureCatalogItemResponse[];
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
  dependencyKeys: string[];
  displayOrder: number;
  isPublished: boolean;
}

export interface UpdateFeatureCatalogItemRequest {
  name: string;
  description: string;
  category: string;
  dependencyKeys: string[];
  displayOrder: number;
  isPublished: boolean;
  isRetired: boolean;
}
