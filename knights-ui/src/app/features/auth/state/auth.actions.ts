import { LoginRequest } from '../../../core/models/auth.model';

export class Login {
  static readonly type = '[Auth] Login';
  constructor(readonly request: LoginRequest) {}
}

export class Logout {
  static readonly type = '[Auth] Logout';
}

export class RestoreSession {
  static readonly type = '[Auth] Restore Session';
}
