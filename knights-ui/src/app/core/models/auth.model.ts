import { UserResponse } from './user.model';

export interface LoginRequest {
  userNameOrEmail: string;
  password: string;
  tenantCodeName?: string;
}

export interface LoginResponse {
  accessToken: string;
  expiresAtUtc: string;
  user: UserResponse;
  tenantId?: string;
  tenantCodeName?: string;
}
