import { CreateUserRequest, UpdateUserRequest } from '../../../core/models/user.model';

export class LoadUsers {
  static readonly type = '[Users] Load Users';
}

export class LoadUser {
  static readonly type = '[Users] Load User';
  constructor(readonly id: string) {}
}

export class CreateUser {
  static readonly type = '[Users] Create User';
  constructor(readonly request: CreateUserRequest) {}
}

export class UpdateUser {
  static readonly type = '[Users] Update User';
  constructor(readonly id: string, readonly request: UpdateUserRequest) {}
}

export class DeleteUser {
  static readonly type = '[Users] Delete User';
  constructor(readonly id: string) {}
}

export class AssignRole {
  static readonly type = '[Users] Assign Role';
  constructor(readonly userId: string, readonly roleId: string) {}
}

export class GrantPermission {
  static readonly type = '[Users] Grant Permission';
  constructor(readonly userId: string, readonly permissionId: string) {}
}
