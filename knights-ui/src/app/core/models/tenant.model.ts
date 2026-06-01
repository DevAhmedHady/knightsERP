export interface TenantResponse {
  id: string;
  codeName: string;
  name: string;
  description: string;
  isActive: boolean;
  expiryDate?: string;
  ownerId: string;
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
