import {
  ConfigureTenantEnvironmentRequest,
  CreateFeatureCatalogItemRequest,
  CreateTenantRequest,
  UpdateFeatureCatalogItemRequest,
  UpdateTenantRequest
} from '../../../core/models/tenant.model';

export class LoadTenants {
  static readonly type = '[Tenants] Load Tenants';
}

export class LoadTenant {
  static readonly type = '[Tenants] Load Tenant';
  constructor(readonly id: string) {}
}

export class LoadTenantByCodeName {
  static readonly type = '[Tenants] Load Tenant By Code Name';
  constructor(readonly codeName: string) {}
}

export class CreateTenant {
  static readonly type = '[Tenants] Create Tenant';
  constructor(readonly request: CreateTenantRequest) {}
}

export class UpdateTenant {
  static readonly type = '[Tenants] Update Tenant';
  constructor(readonly id: string, readonly request: UpdateTenantRequest) {}
}

export class DeactivateTenant {
  static readonly type = '[Tenants] Deactivate Tenant';
  constructor(readonly id: string) {}
}

export class AssignTenantRole {
  static readonly type = '[Tenants] Assign Role';
  constructor(readonly tenantId: string, readonly roleId: string) {}
}

export class RemoveTenantRole {
  static readonly type = '[Tenants] Remove Role';
  constructor(readonly tenantId: string, readonly roleId: string) {}
}

export class GrantTenantPermission {
  static readonly type = '[Tenants] Grant Permission';
  constructor(readonly tenantId: string, readonly permissionId: string) {}
}

export class RevokeTenantPermission {
  static readonly type = '[Tenants] Revoke Permission';
  constructor(readonly tenantId: string, readonly permissionId: string) {}
}

export class AddTenantMember {
  static readonly type = '[Tenants] Add Member';
  constructor(readonly tenantId: string, readonly userId: string) {}
}

export class RemoveTenantMember {
  static readonly type = '[Tenants] Remove Member';
  constructor(readonly tenantId: string, readonly userId: string) {}
}

export class LoadCurrentTenantSetup {
  static readonly type = '[Tenants] Load Current Setup';
}

export class ConfigureCurrentTenantEnvironment {
  static readonly type = '[Tenants] Configure Current Environment';
  constructor(readonly request: ConfigureTenantEnvironmentRequest) {}
}

export class SelectCurrentTenantFeature {
  static readonly type = '[Tenants] Select Current Feature';
  constructor(readonly featureId: string) {}
}

export class RemoveCurrentTenantFeature {
  static readonly type = '[Tenants] Remove Current Feature';
  constructor(readonly featureId: string) {}
}

export class UpdateCurrentTenantFeatureSettings {
  static readonly type = '[Tenants] Update Current Feature Settings';
  constructor(readonly featureId: string, readonly settingsJson: string) {}
}

export class LoadFeatureCatalog {
  static readonly type = '[Tenants] Load Feature Catalog';
}

export class CreateFeatureCatalogItem {
  static readonly type = '[Tenants] Create Feature Catalog Item';
  constructor(readonly request: CreateFeatureCatalogItemRequest) {}
}

export class UpdateFeatureCatalogItem {
  static readonly type = '[Tenants] Update Feature Catalog Item';
  constructor(readonly featureId: string, readonly request: UpdateFeatureCatalogItemRequest) {}
}
