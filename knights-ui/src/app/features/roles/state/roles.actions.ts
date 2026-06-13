import { CreateRoleRequest, UpdateRoleRequest } from '../../../core/models/role.model';

export class LoadRoles {
  static readonly type = '[Roles] Load Roles';
}

export class LoadRole {
  static readonly type = '[Roles] Load Role';
  constructor(readonly id: string) {}
}

export class CreateRole {
  static readonly type = '[Roles] Create Role';
  constructor(readonly request: CreateRoleRequest) {}
}

export class UpdateRole {
  static readonly type = '[Roles] Update Role';
  constructor(readonly id: string, readonly request: UpdateRoleRequest) {}
}

export class DeleteRole {
  static readonly type = '[Roles] Delete Role';
  constructor(readonly id: string) {}
}

export class AssignRolePermission {
  static readonly type = '[Roles] Assign Permission';
  constructor(readonly roleId: string, readonly permissionId: string) {}
}

export class RemoveRolePermission {
  static readonly type = '[Roles] Remove Permission';
  constructor(readonly roleId: string, readonly permissionId: string) {}
}
