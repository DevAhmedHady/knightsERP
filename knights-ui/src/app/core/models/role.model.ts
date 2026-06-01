export interface RoleResponse {
  id: string;
  name: string;
  description?: string;
  isStatic: boolean;
  isDefault: boolean;
  isActive: boolean;
}

export interface RoleWithPermissionsResponse extends RoleResponse {
  permissions: PermissionResponse[];
}

export interface CreateRoleRequest {
  name: string;
  description?: string;
  isStatic?: boolean;
  isDefault?: boolean;
  isActive?: boolean;
}

export interface UpdateRoleRequest {
  name: string;
  description?: string;
}

import { PermissionResponse } from './permission.model';
