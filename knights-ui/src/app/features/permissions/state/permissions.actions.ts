import { CreatePermissionRequest, UpdatePermissionRequest } from '../../../core/models/permission.model';

export class LoadPermissions {
  static readonly type = '[Permissions] Load Permissions';
}

export class LoadPermission {
  static readonly type = '[Permissions] Load Permission';
  constructor(readonly id: string) {}
}

export class CreatePermission {
  static readonly type = '[Permissions] Create Permission';
  constructor(readonly request: CreatePermissionRequest) {}
}

export class UpdatePermission {
  static readonly type = '[Permissions] Update Permission';
  constructor(readonly id: string, readonly request: UpdatePermissionRequest) {}
}

export class DeletePermission {
  static readonly type = '[Permissions] Delete Permission';
  constructor(readonly id: string) {}
}
