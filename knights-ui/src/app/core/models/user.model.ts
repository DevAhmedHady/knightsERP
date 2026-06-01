export interface UserResponse {
  id: string;
  firstName: string;
  midName?: string;
  lastName: string;
  userName: string;
  email: string;
  isEmailConfirmed: boolean;
  roles?: RoleResponse[];
  permissions?: PermissionResponse[];
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

import { RoleResponse } from './role.model';
import { PermissionResponse } from './permission.model';
