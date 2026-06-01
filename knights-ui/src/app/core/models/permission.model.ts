export interface PermissionResponse {
  id: string;
  codeName: string;
  displayName: string;
  description?: string;
}

export interface CreatePermissionRequest {
  codeName: string;
  displayName: string;
  description?: string;
}

export interface UpdatePermissionRequest {
  displayName: string;
  description?: string;
}
