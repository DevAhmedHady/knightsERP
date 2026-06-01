export interface UserResponse {
  id: string;
  firstName: string;
  midName?: string;
  lastName: string;
  userName: string;
  email: string;
  isEmailConfirmed: boolean;
  isActive: boolean;
  lastLoginDate?: string;
  tenantId?: string;
  roleIds: string[];
  permissionIds: string[];
}

export interface CreateUserRequest {
  firstName: string;
  midName?: string;
  lastName: string;
  userName: string;
  email: string;
  password?: string;
  isEmailConfirmed?: boolean;
}

export interface UpdateUserRequest {
  firstName: string;
  midName?: string;
  lastName: string;
  userName: string;
  email: string;
  isEmailConfirmed: boolean;
}
